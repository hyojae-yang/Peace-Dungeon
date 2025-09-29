using UnityEngine;

// 흡혈 화살 투사체의 움직임, 충돌 처리, 데미지 적용 및 체력 흡수(힐)를 관리하는 스크립트입니다.
public class LifestealBoltProjectile : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("투사체가 날아가는 속도입니다.")]
    public float moveSpeed = 7f;

    [Tooltip("투사체가 자동으로 사라지는 시간(초)입니다.")]
    public float lifetime = 8f;

    [Tooltip("투사체가 생성된 후 충돌을 무시할 시간(초)입니다. 플레이어와의 즉시 충돌을 방지합니다.")]
    public float ignoreCollisionDuration = 0.2f;

    // === 스킬 데이터 저장 변수 ===
    private float damage;             // 최종 계산된 데미지
    private DamageType damageType;     // 데미지 타입 (마법, 물리 등)
    private float lifestealRate;      // 흡혈률 (예: 0.1f = 10%)
    private PlayerStats casterStats;  // 스킬 시전자인 플레이어의 스탯 참조 (힐을 적용할 대상)

    private float startTime; // 투사체가 생성된 시간

    void Start()
    {
        startTime = Time.time; // 현재 시간 기록

        // 관통 스킬이므로 충돌 시 파괴되지 않습니다.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 매 프레임마다 투사체를 앞으로 이동시킵니다.
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
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

    // 콜라이더에 부딪혔을 때 호출되는 메서드입니다. (Collider의 Is Trigger가 체크되어 있어야 작동)
    void OnTriggerEnter(Collider other)
    {
        // === 0. NEW: 플레이어 충돌 검사 및 무시 ===
        // 충돌 대상이 플레이어 태그를 가지고 있다면, 데미지나 효과를 적용하지 않고 즉시 종료합니다.
        if (other.CompareTag("Player"))
        {
            return;
        }

        // === 1. 충돌 무시 시간 확인 ===
        if (Time.time < startTime + ignoreCollisionDuration)
        {
            return;
        }

        // === 2. 데미지 대상 확인 및 적용 ===
        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            // IDamageable 인터페이스를 가진 객체에게 데미지를 입힙니다.
            damageableObject.TakeDamage(damage, damageType);

            // === 3. 흡혈 로직 실행 ===
            float healAmount = damage * lifestealRate;

            if (casterStats != null && healAmount > 0f)
            {
                float currentHealth = casterStats.health;
                float maxHealth = casterStats.MaxHealth;

                // PlayerStats.health에 직접 접근하여 체력을 회복시키고 최대치를 넘지 않도록 보정합니다.
                casterStats.health = Mathf.Min(currentHealth + healAmount, maxHealth);

                Debug.Log($"[흡혈 성공] {other.name}에게 {damage} 피해를 입히고, {healAmount}만큼 체력을 회복했습니다. (흡혈률: {lifestealRate * 100}%)");
            }
        }

        // 몬스터나 지형지물에 닿더라도 파괴되지 않고,
        // 오직 Start()에서 설정한 lifetime이 지나야 파괴됩니다.
    }
}