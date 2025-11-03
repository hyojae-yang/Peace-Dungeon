using UnityEngine;

// 참고: DamageType 열거형 및 IDamageable 인터페이스는 프로젝트의 공용 스크립트에 정의되어 있다고 가정합니다.

/// <summary>
/// 다른 오브젝트의 트리거 영역에 '진입한 순간'에 오직 한 번 데미지를 가하는 함정 스크립트입니다.
/// 데미지는 "Player" 태그를 가진 IDamageable 대상에게만 적용됩니다.
/// </summary>
public class DamageTrap_OneShot_Trigger : MonoBehaviour
{
    // === 설정 가능한 변수 ===

    [Header("데미지 설정")]
    [Tooltip("대상에게 한 번 가할 데미지 양입니다. (True Damage로 방어력 무시)")]
    public float baseDamageAmount = 50f; // 한 번에 강한 데미지를 가정하여 기본값 유지

    [Tooltip("이 함정이 가하는 데미지 타입입니다. (True Damage로 고정)")]
    private const DamageType TRAP_DAMAGE_TYPE = DamageType.True;

    // === 플레이어 타겟팅을 위한 상수 ===
    private const string PLAYER_TAG = "Player";

    // === 핵심 로직: 트리거 진입 시 데미지 적용 ===

    /// <summary>
    /// 다른 콜라이더가 이 오브젝트의 트리거 콜라이더 영역 안에 들어왔을 때 호출됩니다.
    /// 이 함수는 영역에 진입한 순간에 단 한 번만 실행됩니다.
    /// </summary>
    /// <param name="other">영역에 진입한 다른 콜라이더</param>
    private void OnTriggerEnter(Collider other) // **OnCollisionEnter에서 OnTriggerEnter로 변경**
    {
        // 1. 진입한 오브젝트가 "Player" 태그를 가졌는지 확인합니다.
        if (other.CompareTag(PLAYER_TAG)) // **collision.gameObject 대신 other 사용**
        {
            // 2. IDamageable 인터페이스를 구현했는지 확인하고 컴포넌트를 가져옵니다.
            IDamageable damageable = other.GetComponent<IDamageable>();

            // 3. 데미지 적용이 가능한 대상인지 최종 확인합니다.
            if (damageable != null)
            {
                // 4. 데미지를 단 한 번 적용합니다.
                // TakeDamage(float amount, DamageType type) 메서드를 호출하여 고정 피해를 입힙니다.
                damageable.TakeDamage(baseDamageAmount, TRAP_DAMAGE_TYPE);

                // (선택 사항) 만약 함정이 한 번 작동 후 파괴되어야 한다면 여기에 Destroy(gameObject); 를 추가할 수 있습니다.
            }
        }
        // OnTriggerExit이나 Update()는 필요 없습니다.
    }
}