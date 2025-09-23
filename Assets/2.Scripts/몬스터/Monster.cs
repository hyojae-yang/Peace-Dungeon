using UnityEngine;

/// <summary>
/// 몬스터의 AI 행동(감지, 추적, 상태 관리)을 담당하는 클래스입니다.
/// MonsterBase를 상속받아 공통 기능을 구현합니다.
/// </summary>
public class Monster : MonsterBase, IDetectable
{
    // === 플레이어 감지 관련 변수 ===
    [Header("플레이어 감지 설정")]
    [Tooltip("플레이어를 감지하는 범위(반경)입니다.")]
    public float detectionRange = 10f;
    [Tooltip("플레이어를 감지하는 부채꼴 각도입니다. (총 각도)")]
    [Range(0, 360)]
    public float detectionAngle = 120f;
    [Tooltip("플레이어 레이어 마스크입니다.")]
    public LayerMask playerLayer;


    // === 종속성 ===
    private MonsterCombat combat;
    private MonsterLoot loot;
    [HideInInspector]
    public IDetectable detectableTarget;

    private void Awake()
    {
        combat = GetComponent<MonsterCombat>();
        if (combat == null) Debug.LogError("MonsterCombat 컴포넌트를 찾을 수 없습니다!");

        loot = GetComponent<MonsterLoot>();
        if (loot == null) Debug.LogError("MonsterLoot 컴포넌트를 찾을 수 없습니다!");
    }

    private void Update()
    {
        DetectPlayer();
        switch (currentState)
        {
            case MonsterState.Patrol:
                break;
            case MonsterState.Chase:
                if (detectableTarget != null)
                {
                    MoveTowardsTarget(detectableTarget.GetTransform());
                }
                else
                {
                    ChangeState(MonsterState.Idle);
                }
                break;
            case MonsterState.Attack:
                break;
            case MonsterState.Flee:
                // Flee 상태는 SquirrelBehavior와 같은 전용 스크립트가 처리합니다.
                break;
            case MonsterState.Dead:
                break;
        }
    }

    /// <summary>
    /// 플레이어를 감지하는 메서드.
    /// 오버랩 스피어와 시야각 체크를 통해 타겟을 탐지합니다.
    /// </summary>
    private void DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        detectableTarget = null;

        foreach (Collider hit in hitColliders)
        {
            IDetectable target = hit.GetComponent<IDetectable>();
            if (target != null && target.IsDetectable())
            {
                Vector3 directionToTarget = (target.GetTransform().position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);
                if (angle < detectionAngle * 0.5f)
                {
                    detectableTarget = target;
                    // 플레이어 감지 시 Chase 상태로 즉시 변경
                    ChangeState(MonsterState.Chase);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 감지된 대상을 향해 이동하는 메서드입니다.
    /// </summary>
    /// <param name="targetTransform">추적할 대상의 Transform</param>
    private void MoveTowardsTarget(Transform targetTransform)
    {
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        transform.position += direction * monsterData.moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 외부에서 몬스터의 상태를 안전하게 변경하기 위한 메서드입니다.
    /// </summary>
    /// <param name="newState">변경할 몬스터의 새로운 상태</param>
    public void ChangeState(MonsterState newState)
    {
        SetState(newState);
    }

    // --- MonsterBase 가상 메서드 오버라이드 ---
    public override void Die()
    {
        ChangeState(MonsterState.Dead);
        loot.GiveReward();
        Destroy(gameObject);
    }

    // --- IDetectable 인터페이스 구현 ---
    public bool IsDetectable()
    {
        return currentState != MonsterState.Dead;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 forwardLimit = transform.position + transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, forwardLimit);
        Vector3 leftLimit = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
        Vector3 rightLimit = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
    }
}