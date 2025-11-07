// FireballProjectile.cs
using UnityEngine;

// 파이어볼 투사체의 움직임, 충돌 처리, 그리고 수명을 관리하는 스크립트입니다.
public class FireballProjectile : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("파이어볼이 날아가는 속도입니다.")]
    public float moveSpeed = 10f;

    [Tooltip("파이어볼이 자동으로 사라지는 최대 거리(유닛)입니다.")]
    public float maxDistance = 50f;
    private Vector3 startPosition;

    [Tooltip("파이어볼이 생성된 후 충돌을 무시할 시간(초)입니다. 플레이어와의 즉시 충돌을 방지합니다.")]
    public float ignoreCollisionDuration = 0.2f;

    private float damage;
    private float startTime;
    private DamageType damageType;

    private Vector3 moveDirection = Vector3.forward;

    void Start()
    {
        startTime = Time.time;
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. 이동 처리
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 2. 최대 거리 도달 시 파괴 (관통 여부와 무관하게 최대 비행 거리 보장)
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// PlayerSkillController로부터 최종 데미지 값과 타입을 전달받는 메서드입니다.
    /// </summary>
    /// <param name="finalDamage">계산된 최종 데미지</param>
    /// <param name="type">데미지 타입 (물리, 마법 등)</param>
    public void SetDamage(float finalDamage, DamageType type)
    {
        damage = finalDamage;
        damageType = type;
    }

    /// <summary>
    /// 외부(FireballSkillData)로부터 투사체의 발사 방향을 주입받는 메서드입니다.
    /// </summary>
    /// <param name="direction">계산된 발사 방향 벡터</param>
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    // 콜라이더에 부딪혔을 때 호출되는 메서드입니다.
    void OnTriggerEnter(Collider other)
    {
        // 1. 일정 시간 동안은 충돌을 무시합니다.
        if (Time.time < startTime + ignoreCollisionDuration)
        {
            return;
        }

        // 2. [핵심 수정] IDamageable 인터페이스를 가진 객체인지 확인합니다.
        IDamageable damageableObject = other.GetComponent<IDamageable>();

        if (damageableObject != null)
        {
            // 3. 데미지 처리
            damageableObject.TakeDamage(damage, damageType);

            // 4. 충돌 SFX 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Skill_Fireball_Impact, 0.8f);
            }

            Debug.Log("IDamageable 타겟(" + other.gameObject.name + ")과 충돌하여 파괴됨.");

            // 5. [핵심 목표 달성] 데미지 대상과 충돌했으므로 투사체를 파괴합니다.
            Destroy(gameObject);
        }

        // IDamageable이 아닌 모든 객체 (지형지물, 플레이어, 기타 이펙트)와 충돌한 경우,
        // 위 if 문을 통과하지 못하고 메서드가 종료되면서 투사체는 파괴되지 않고 계속 날아갑니다 (관통).
    }
}