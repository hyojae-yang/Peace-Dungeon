using UnityEngine;

/// <summary>
/// 매직 미사일 투사체의 움직임, 타겟팅 및 충돌 처리를 담당하는 클래스입니다.
/// SRP (단일 책임 원칙): 오직 투사체의 생명 주기와 추적, 데미지 적용 책임만을 가집니다.
/// </summary>
public class MagicMissileProjectile : MonoBehaviour
{
    // === 내부 상태 및 설정 필드 ===

    [Header("투사체 이동 설정")]
    [Tooltip("투사체가 타겟을 향해 이동하는 속도입니다.")]
    [SerializeField]
    private float moveSpeed = 10f;

    [Tooltip("투사체가 타겟으로 방향을 틀 때의 회전 속도입니다. (0이면 즉시 회전)")]
    [SerializeField]
    private float rotationSpeed = 5f;

    // === 스킬 데이터로부터 주입받는 필드 (Private) ===
    private float damage;             // 이 투사체가 가할 데미지
    private float maxTargetingRange;  // 몬스터를 찾을 최대 반경
    private LayerMask monsterLayer;   // 몬스터 레이어 마스크
    private DamageType damageType; // <--- 데미지 타입 필드 추가

    // === 타겟 상태 필드 ===
    private Transform targetTransform; // 현재 추적 중인 몬스터의 Transform
    private Vector3 initialTargetingOrigin; // 타겟을 찾을 때의 중심 위치 (재탐색에 사용)

    /// <summary>
    /// 매 프레임 타겟을 추적하고, 타겟의 유효성을 검사합니다.
    /// </summary>
    private void Update()
    {
        // 1. 타겟 유효성 검사
        // 타겟 Transform이 사라졌거나(null), 비활성화(죽었거나)된 경우를 체크합니다.
        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
        {
            // 타겟이 죽었거나 사라졌으므로, 재탐색 로직을 실행합니다. (예외 처리 3번)
            FindTargetAndStartTracking();

            // 재탐색 후에도 타겟이 없으면 FindTargetAndStartTracking() 내에서 Destroy됩니다.
            // 따라서 여기서는 추가 로직 없이 리턴합니다.
            return;
        }

        // 2. 타겟을 향해 회전 (방향 정렬)
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

        // 미사일이 항상 타겟을 향하도록 부드럽게 회전합니다.
        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // 3. 타겟을 향해 이동
        // 부드러운 추적을 위해 Vector3.MoveTowards를 사용하거나, 단순히 Transform.Translate를 사용할 수 있습니다.
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * Time.deltaTime);

        // *보조 로직: 타겟에 너무 가까워지면 충돌 처리 없이 Destroy될 수 있으므로, 
        // 일정 거리 이내로 접근하면 OnDestroy나 OnTriggerEnter를 유도하도록 할 수도 있습니다.
    }
    /// <summary>
    /// 콜라이더 충돌 시 데미지를 적용하고 투사체를 파괴합니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != targetTransform)
        {
            // 타겟이 아닌 것에 부딪혔지만, 충돌 즉시 파괴를 원한다면 이 부분을 수정
            // 만약 타겟 외의 모든 것에 충돌하면 파괴되길 원한다면, 아래 return을 제거하고
            // 일반적인 충돌 처리 로직으로 넘겨야 합니다.
            // 현재는 '타겟에만 데미지를 주겠다'는 로직이므로 return을 유지합니다.
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage, damageType);
            Destroy(gameObject); // 성공적으로 데미지 적용 후 파괴
        }
        else
        {
            // 타겟은 맞췄지만 IDamageable이 없는 경우 (예: 데미지를 받지 않는 몬스터의 자식 오브젝트)
            Destroy(gameObject); // 데미지는 못 줘도 파괴는 해야 함!
        }
    }

    /// <summary>
    /// 투사체를 초기화하고 타겟 탐색을 시작합니다.
    /// [수정] DamageType 인자를 추가하여 공격 타입을 주입받습니다.
    /// </summary>
    /// <param name="initialDamage">이 투사체가 가할 데미지</param>
    /// <param name="range">타겟을 찾을 최대 반경</param>
    /// <param name="layer">타겟 레이어 마스크</param>
    /// <param name="type">공격 타입 (물리, 마법 등)</param>
    public void Initialize(float initialDamage, float range, LayerMask layer, DamageType type)
    {
        // 1. 데이터 저장
        this.damage = initialDamage;
        this.maxTargetingRange = range;
        this.monsterLayer = layer;
        this.damageType = type; // <--- 타입 저장 로직 추가

        // 2. 타겟을 찾고 추적을 시작하는 로직 호출 (핵심 로직 위임)
        FindTargetAndStartTracking();
    }
    /// <summary>
    /// 투사체의 탐색 반경 내에서 가장 가까운 몬스터를 찾아 targetTransform에 저장합니다.
    /// 몬스터를 찾지 못하면 스스로를 파괴합니다.
    /// </summary>
    private void FindTargetAndStartTracking()
    {
        // [수정] 탐색 중심을 'initialTargetingOrigin' 대신 'transform.position' (현재 위치)으로 변경
        Vector3 searchOrigin = transform.position;

        // 1. OverlapSphere를 사용해 주변 몬스터를 모두 찾습니다.
        Collider[] hitColliders = Physics.OverlapSphere(searchOrigin, maxTargetingRange, monsterLayer);

        targetTransform = null; // 타겟을 찾기 전에 초기화

        if (hitColliders.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestMonster = null;

            // 2. 가장 가까운 몬스터를 찾습니다. (거리 계산 기준도 searchOrigin으로 통일)
            foreach (Collider col in hitColliders)
            {
                float distance = Vector3.Distance(searchOrigin, col.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMonster = col.transform;
                }
            }

            // 3. 타겟 설정
            targetTransform = closestMonster;
        }

        // 4. 예외 처리: 타겟을 찾지 못하면 즉시 파괴
        if (targetTransform == null)
        {
            // Debug.Log("매직 미사일: 주변에 유효 타겟이 없어 스스로 소멸합니다.");
            Destroy(gameObject);
        }
    }
}