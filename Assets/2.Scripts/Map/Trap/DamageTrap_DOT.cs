using UnityEngine;

public class DamageTrap_DOT : MonoBehaviour
{
    // === 설정 가능한 변수 ===

    [Header("데미지 설정")]
    [Tooltip("대상에게 가할 기본 데미지 양입니다. (True Damage로 방어력 무시)")]
    public float baseDamageAmount = 30f;

    [Tooltip("데미지를 가하는 주기(초)입니다. (예: 1.0f는 1초마다)")]
    public float damageInterval = 1.0f;

    [Tooltip("이 함정이 가하는 데미지 타입입니다. (True Damage로 고정)")]
    private const DamageType TRAP_DAMAGE_TYPE = DamageType.True;

    // === 플레이어 타겟팅을 위한 상수 ===
    private const string PLAYER_TAG = "Player";

    // === 내부 상태 관리 변수 ===

    /// <summary>
    /// 다음 데미지를 적용할 수 있는 Unity의 Time.time 값입니다. 쿨타임 역할을 합니다.
    /// </summary>
    private float nextDamageTime;

    /// <summary>
    /// 현재 함정과 충돌 중인 플레이어 대상(IDamageable)에 대한 참조입니다.
    /// </summary>
    private IDamageable currentTarget;

    // === 초기화 및 설정 ===

    private void Start()
    {
        // 시작 후 바로 데미지를 줄 수 있도록 설정합니다.
        nextDamageTime = Time.time;
    }

    // === 충돌 감지 및 대상 설정 ===

    /// <summary>
    /// 다른 콜라이더와 물리적으로 충돌이 시작되었을 때 호출됩니다.
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    private void OnCollisionEnter(Collision collision)
    {
        // 1. 충돌한 오브젝트가 "Player" 태그를 가졌는지 확인합니다.
        if (collision.gameObject.CompareTag(PLAYER_TAG))
        {
            // 2. IDamageable 인터페이스를 구현했는지 확인합니다.
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            // 3. IDamageable이며 현재 타겟이 설정되어 있지 않을 때만 타겟으로 지정합니다.
            if (damageable != null && currentTarget == null)
            {
                currentTarget = damageable;
                // 충돌이 시작될 때 다음 데미지 시간을 현재 시간으로 리셋하여 즉시 데미지를 줄 수 있게 합니다.
                nextDamageTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 다른 콜라이더와의 물리적 충돌이 끝났을 때 호출됩니다.
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    private void OnCollisionExit(Collision collision)
    {
        // 1. 충돌을 벗어난 오브젝트가 "Player" 태그를 가졌는지 확인합니다.
        if (collision.gameObject.CompareTag(PLAYER_TAG))
        {
            // 2. 벗어난 대상이 현재 타겟과 동일한지 확인합니다.
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            if (damageable != null && currentTarget == damageable)
            {
                // 대상이 영역을 벗어났으므로 타겟을 해제합니다.
                currentTarget = null;
            }
        }
    }

    // === 핵심 로직: 주기적인 데미지 처리 (Update는 이전과 동일) ===

    /// <summary>
    /// 매 프레임 호출되어 데미지 주기를 확인하고 데미지를 적용합니다.
    /// </summary>
    private void Update()
    {
        // 1. 현재 타겟(충돌 중인 대상)이 유효한지 확인합니다.
        if (currentTarget != null)
        {
            // 2. 쿨타임 체크: 현재 시간(Time.time)이 다음 데미지 가능 시간(nextDamageTime)을 지났는지 확인합니다.
            if (Time.time >= nextDamageTime)
            {
                // 3. 타겟의 TakeDamage(float amount, DamageType type) 메서드를 호출합니다.
                currentTarget.TakeDamage(baseDamageAmount, TRAP_DAMAGE_TYPE);

                // 4. 다음 데미지 시간을 갱신합니다.
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }
}
