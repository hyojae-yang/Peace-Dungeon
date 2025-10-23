using UnityEngine;

// 파이어볼 투사체의 움직임, 충돌 처리, 그리고 수명을 관리하는 스크립트입니다.
public class FireballProjectile : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("파이어볼이 날아가는 속도입니다.")]
    public float moveSpeed = 10f;

    [Tooltip("파이어볼이 자동으로 사라지는 시간(초)입니다. 너무 오래 날아가는 것을 방지합니다.")]
    public float lifetime = 5f;

    [Tooltip("파이어볼이 생성된 후 충돌을 무시할 시간(초)입니다. 플레이어와의 즉시 충돌을 방지합니다.")]
    public float ignoreCollisionDuration = 0.2f; // 예시: 0.2초 동안 충돌 무시

    private float damage; // PlayerSkillController로부터 전달받을 데미지 변수
    private float startTime; // 파이어볼이 생성된 시간
    private DamageType damageType; // 추가된 데미지 타입 변수

    void Start()
    {
        startTime = Time.time; // 현재 시간 기록

        // 일정 시간 후 스스로 파괴되도록 타이머를 설정합니다.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 매 프레임마다 파이어볼을 앞으로 이동시킵니다.
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
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

    // 콜라이더에 부딪혔을 때 호출되는 메서드입니다.
    // Is Trigger가 체크된 콜라이더와 충돌했을 때 작동합니다.
    void OnTriggerEnter(Collider other)
    {
        // 일정 시간 동안은 충돌을 무시합니다.
        if (Time.time < startTime + ignoreCollisionDuration)
        {
            return;
        }

        // 충돌한 대상이 IDamageable 인터페이스를 가지고 있는지 확인합니다.
        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            // IDamageable 인터페이스를 가진 객체에게 데미지를 입힙니다.
            damageableObject.TakeDamage(damage, damageType);
        }

        // 몬스터나 지형지물에 닿으면 즉시 파괴됩니다.
        Destroy(gameObject);
    }
}