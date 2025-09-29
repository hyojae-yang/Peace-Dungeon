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
    /// <param name="other">충돌한 Collider</param>
    private void OnTriggerEnter(Collider other)
    {
        // 1. 충돌한 대상이 현재 추적 중인 타겟이 맞는지 확인 (선택적)
        // 매직 미사일은 타겟에게만 데미지를 주도록 구현하는 것이 일반적입니다.
        if (other.transform != targetTransform)
        {
            // 현재 타겟이 아닌 다른 오브젝트와 충돌했다면, 무시하고 계속 날아갑니다.
            // 또는, 환경 오브젝트(벽 등)와 충돌 시 파괴될 수도 있습니다.
            // 여기서는 타겟이 아니면 계속 날아가도록 가정합니다.
            return;
        }

        // 2. 데미지 적용 대상 확인
        // 몬스터가 IDamageable 인터페이스를 구현했다고 가정합니다.
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            // 3. 데미지 적용
            damageable.TakeDamage(damage);

            // 4. 투사체 소멸
            Destroy(gameObject);
        }
        // 타겟은 맞지만 데미지 적용 컴포넌트가 없는 경우(예: 데미지를 받지 않는 UI), 파괴하지 않고 지나칠 수 있습니다.
        // 하지만 대부분의 경우, 타겟을 맞췄다면 파괴하는 것이 일반적입니다.
        else if (other.transform == targetTransform)
        {
            // 타겟은 맞췄으나 IDamageable이 없다면, 그래도 파괴 (오류 방지)
            Destroy(gameObject);
        }
    }

    // *참고: IDamageable 인터페이스가 없다면, MonsterBase와 같은 구체적인 클래스로 대체해야 합니다.
    // (예: MonsterBase monster = other.GetComponent<MonsterBase>();)
    // === Initialize 메서드 정의 (다음 단계) ===
    /// <summary>
    /// 투사체를 초기화하고 타겟 탐색을 시작합니다.
    /// DIP (의존성 역전): 외부(SkillData)에서 필요한 데이터를 주입받습니다.
    /// </summary>
    /// <param name="initialDamage">이 투사체가 가할 데미지</param>
    /// <param name="range">타겟을 찾을 최대 반경</param>
    /// <param name="layer">타겟 레이어 마스크</param>
    public void Initialize(float initialDamage, float range, LayerMask layer)
    {
        // 1. 데이터 저장
        this.damage = initialDamage;
        this.maxTargetingRange = range;
        this.monsterLayer = layer;

        // 2. 타겟팅의 기준 위치를 현재 위치로 설정 (재탐색 시 필요)
        this.initialTargetingOrigin = transform.position;

        // 3. 타겟을 찾고 추적을 시작하는 로직 호출 (핵심 로직 위임)
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