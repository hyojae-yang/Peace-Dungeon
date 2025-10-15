using System.Collections;
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

    public float attackRange;
    // === 종속성 ===
    private MonsterCombat combat;
    private MonsterLoot loot;
    AudioSource audioSource;
    [HideInInspector]
    public IDetectable detectableTarget;
    Animator animator;
    [Header("사망 설정")]
    [Tooltip("사망 애니메이션이 재생되는 시간입니다. 이 시간 후 오브젝트가 파괴됩니다.")]
    public float deathAnimationDuration = 5.0f;
    public AudioClip deathSound;
    private void Awake()
    {
        combat = GetComponent<MonsterCombat>();
        if (combat == null) Debug.LogError("MonsterCombat 컴포넌트를 찾을 수 없습니다!");

        loot = GetComponent<MonsterLoot>();
        if (loot == null) Debug.LogError("MonsterLoot 컴포넌트를 찾을 수 없습니다!");
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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
                    // [수정] 거리 체크 및 상태 전환 코드를 제거하고 순수 이동만 남깁니다.
                    MoveTowardsTarget(detectableTarget.GetTransform());
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
        // [수정 1] 이미 타겟을 발견한 경우의 재확인 로직을 추가
        if (detectableTarget != null && detectableTarget.IsDetectable())
        {
            Vector3 currentDirectionToTarget = (detectableTarget.GetTransform().position - transform.position);
            float distance = currentDirectionToTarget.magnitude;

            // 1. 거리가 감지 범위 내에 있는지 확인합니다.
            if (distance <= detectionRange)
            {
                // 2. 레이캐스트를 이용해 시야가 가려지지 않았는지 확인합니다. (옵션)
                // RaycastHit hit;
                // if (!Physics.Raycast(transform.position, currentDirectionToTarget.normalized, out hit, distance, playerLayer))
                // {
                // 시야가 가려지지 않았고, 범위 내에 있으므로 계속 추적합니다.
                return;
                // }
            }

            // 거리를 벗어났거나 시야가 가려지면 타겟을 놓칩니다.
            detectableTarget = null;
            return;
        }

        // [수정 2] 타겟이 없을 때만 초기 시야각 감지를 수행합니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        foreach (Collider hit in hitColliders)
        {
            IDetectable target = hit.GetComponent<IDetectable>();
            if (target != null && target.IsDetectable())
            {
                Vector3 directionToTarget = (target.GetTransform().position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                // 몬스터가 타겟을 **처음** 감지할 때만 시야각 체크를 엄격하게 적용합니다.
                if (angle < detectionAngle * 0.5f)
                {
                    detectableTarget = target;
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

        // [수정 1] Y축만 고려한 수평 회전 목표 계산
        // 곰이 공중을 보지 않고 항상 수평하게 플레이어를 바라보게 합니다.
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // [수정 2] 회전 속도 Slerp 인수를 고정값으로 변경 (흔들림 완화)
        // 기존: Time.deltaTime * 5f
        // 변경: 몬스터 이동 속도(moveSpeed)의 일부를 활용하여 일관된 회전 속도를 적용.
        // 5.0f * Time.deltaTime 대신, moveSpeed를 이용해 부드러운 회전 속도를 계산합니다.
        // 여기서는 기존에 사용하시던 5f를 상수 변수로 대체하여 활용합니다.
        const float rotationFactor = 5.0f;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * rotationFactor // 상수로 대체된 회전 속도 적용
        );

        // [수정 3] 이동 방향 계산 시에도 Y축을 제거한 방향 벡터 사용
        // 몬스터가 경사를 오르내릴 때 transform.position에 직접 적용하면 부자연스러울 수 있으므로,
        // Y축을 0으로 고정한 평면 방향으로 이동하도록 명시적으로 처리합니다.
        transform.position += new Vector3(direction.x, 0, direction.z).normalized * monsterData.moveSpeed * Time.deltaTime;

        // 참고: 만약 몬스터가 NavMeshAgent를 사용한다면 transform.position 제어는 제거해야 합니다.
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
        // [수정] 몬스터 사망 시 이벤트를 발생시켜 외부에 알립니다.
        // MonsterBase에서 정의한 보호된 메서드를 호출하여 안전하게 이벤트를 전파합니다.
        if (monsterData != null)
        {
            // [추가된 로직] 이벤트 발생: 몬스터의 고유 ID(Target ID)를 QuestManager로 전달합니다.
            RaiseMonsterKilledEvent(monsterData.monsterID);
        }
        else
        {
            // monsterData가 없을 경우의 안전 장치
            Debug.LogError("MonsterData가 할당되지 않아 몬스터 처치 이벤트를 발생시킬 수 없습니다.");
        }
        // 사망 사운드 재생
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        // 사망 애니메이션 길이만큼 대기 후 오브젝트 제거
        // [수정된 로직] deathAnimationDuration이 0보다 큰지 확인하여 지연 파괴 또는 즉시 파괴를 결정합니다.
        if (deathAnimationDuration > 0f)
        {
            // 애니메이션 대기 후 오브젝트 파괴를 코루틴으로 처리
            StartCoroutine(HandleDeathSequence(deathAnimationDuration));
        }
        else
        {
            // 애니메이션 길이가 0 이하이므로 즉시 파괴합니다.
            Destroy(gameObject);
        }
    }
    // [추가] 몬스터 사망 후 지연 파괴를 처리하는 코루틴입니다.
    /// <summary>
    /// 몬스터 사망 애니메이션 재생 시간만큼 대기한 후 오브젝트를 파괴합니다.
    /// </summary>
    /// <param name="delayTime">사망 애니메이션 길이 (대기 시간)</param>
    private IEnumerator HandleDeathSequence(float delayTime)
    {
        // 지정된 시간(사망 애니메이션 길이)만큼 대기합니다.
        yield return new WaitForSeconds(delayTime);

        // 대기 시간이 끝나면 몬스터 오브젝트를 제거합니다.
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