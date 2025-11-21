using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 추가

// 흡혈 화살 투사체의 움직임, 충돌 처리, 데미지 적용 및 체력 흡수(힐)를 관리하는 스크립트입니다.
// 이 버전은 충돌 대상에게 'damageInterval' 주기로 지속 피해(DOT)를 입힙니다.
public class LifestealBoltProjectile : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("투사체가 날아가는 속도입니다.")]
    public float moveSpeed = 7f;

    [Tooltip("투사체가 스스로 회전하는 속도(도/초)입니다.")]
    public float rotationSpeed = 180f;

    [Tooltip("투사체가 자동으로 사라지는 시간(초)입니다.")]
    public float lifetime = 8f;

    [Tooltip("투사체가 생성된 후 충돌을 무시할 시간(초)입니다. 플레이어와의 즉시 충돌을 방지합니다.")]
    public float ignoreCollisionDuration = 0.2f;

    [Header("DOT 설정")]
    [Tooltip("동일 대상에게 데미지를 반복해서 주는 간격(초)입니다.")]
    public float damageInterval = 1.0f; // 1초에 한 번 데미지

    // === 스킬 데이터 저장 변수 ===
    private float damage;             // 최종 계산된 데미지
    private DamageType damageType;     // 데미지 타입 (마법, 물리 등)
    private float lifestealRate;      // 흡혈률 (예: 0.1f = 10%)
    private PlayerStats casterStats;  // 스킬 시전자인 플레이어의 스탯 참조 (힐을 적용할 대상)

    private float startTime; // 투사체가 생성된 시간

    // NEW: 투사체와 충돌하고 있는 IDamageable 대상들을 저장하는 리스트입니다.
    private List<IDamageable> damagedTargets = new List<IDamageable>();

    private Coroutine damageCoroutine; // 지속 피해를 주기 위한 코루틴 참조 변수입니다.

    void Start()
    {
        startTime = Time.time; // 현재 시간 기록

        // 관통 스킬이므로 충돌 시 파괴되지 않습니다.
        Destroy(gameObject, lifetime);

        // 지속 피해 코루틴을 시작합니다.
        damageCoroutine = StartCoroutine(DamageOverTimeCoroutine());
    }

    void Update()
    {
        // 매 프레임마다 투사체를 앞으로 이동시킵니다.
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        // 매 프레임마다 투사체를 스스로 회전시킵니다.
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// LifestealBoltSkillData로부터 최종 데이터를 전달받아 초기화하는 메서드입니다.
    /// </summary>
    /// <param name="finalDamage">계산된 최종 데미지</param>
    /// <param name="type">데미지 타입</param>
    /// <param name="rate">흡혈률 (0.0 ~ 1.0)</param>
    /// <param name="stats">시전자(플레이어)의 스탯 컴포넌트</param>
    public void Initialize(float finalDamage, DamageType type, float rate, PlayerStats stats)
    {
        damage = finalDamage;
        damageType = type;
        lifestealRate = rate;
        casterStats = stats;
    }

    /// <summary>
    /// 충돌 범위 내에 들어온 대상을 리스트에 추가합니다. (데미지를 즉시 주지 않습니다.)
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // 1. 초기 충돌 무시 시간 확인
        if (Time.time < startTime + ignoreCollisionDuration)
        {
            return;
        }

        // 2. 플레이어 태그 무시
        if (other.CompareTag("Player"))
        {
            return;
        }

        // 3. IDamageable 대상 확인 및 리스트에 추가
        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            // 대상이 리스트에 이미 없다면 추가합니다.
            if (!damagedTargets.Contains(damageableObject))
            {
                damagedTargets.Add(damageableObject);
            }
        }
    }

    /// <summary>
    /// 충돌 범위에서 벗어난 대상을 리스트에서 제거합니다.
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            // 대상이 리스트에 있다면 제거합니다.
            damagedTargets.Remove(damageableObject);
        }
    }

    /// <summary>
    /// damageInterval마다 충돌 리스트에 있는 모든 대상에게 데미지를 입히고 흡혈하는 코루틴입니다.
    /// </summary>
    private IEnumerator DamageOverTimeCoroutine()
    {
        // 이 코루틴은 투사체의 수명 동안 계속 실행됩니다.
        while (true)
        {
            // 지정된 간격만큼 대기합니다.
            yield return new WaitForSeconds(damageInterval);

            // 데미지 대상이 없다면 다음 간격을 기다립니다.
            if (damagedTargets.Count == 0) continue;

            // 리스트를 역순으로 순회하며 데미지를 적용합니다. (리스트에서 제거하는 경우 안전합니다.)
            for (int i = damagedTargets.Count - 1; i >= 0; i--)
            {
                IDamageable target = damagedTargets[i];

                // === [수정된 핵심 로직] IDamageable 인터페이스 변경 없이 파괴된 대상 체크 ===
                // IDamageable은 반드시 Component를 구현하므로, Component 타입으로 변환하여
                // 유니티의 null 체크 및 파괴된 GameObject 체크를 수행합니다.
                Component targetComponent = target as Component;

                if (targetComponent == null || targetComponent.gameObject == null)
                {
                    // 오브젝트가 파괴되었거나 유효하지 않으면 리스트에서 제거합니다.
                    damagedTargets.RemoveAt(i);
                    continue;
                }
                // ======================================================================

                // 1. 데미지 적용
                target.TakeDamage(damage, damageType);

                // 2. 흡혈 로직 실행
                ApplyLifesteal(damage);
            }
        }
    }

    /// <summary>
    /// 입힌 데미지를 기반으로 시전자에게 체력을 회복시키는 메서드입니다.
    /// 흡혈 로직의 책임을 명확히 분리합니다. (단일 책임 원칙)
    /// </summary>
    /// <param name="appliedDamage">입힌 피해량</param>
    private void ApplyLifesteal(float appliedDamage)
    {
        float healAmount = appliedDamage * lifestealRate;

        if (casterStats != null && healAmount > 0f)
        {
            // 현재 체력과 최대 체력 정보를 얻습니다.
            float currentHealth = casterStats.health;
            // NOTE: PlayerStats 클래스에 MaxHealth 속성이 있다고 가정합니다.
            float maxHealth = casterStats.MaxHealth;

            // PlayerStats.health에 직접 접근하여 체력을 회복시키고 최대치를 넘지 않도록 보정합니다.
            casterStats.health = Mathf.Min(currentHealth + healAmount, maxHealth);

            // Debug.Log($"[흡혈 성공] 피해를 입히고, {healAmount}만큼 체력을 회복했습니다. (흡혈률: {lifestealRate * 100}%)");
        }
    }
}