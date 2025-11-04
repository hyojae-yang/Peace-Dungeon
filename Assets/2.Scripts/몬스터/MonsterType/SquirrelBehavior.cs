using UnityEngine;
using System.Collections;
using System; // 이벤트 사용을 위해 System 네임스페이스 추가

/// <summary>
/// 다람쥐 몬스터의 고유한 행동 로직(도망치기)을 담당하는 클래스입니다.
/// MonsterPatrol 컴포넌트를 제어합니다.
/// SOLID 원칙: SRP(단일 책임 원칙)에 따라 다람쥐의 고유 행동 로직(Flee 결정 및 실행)만 담당합니다.
/// </summary>
public class SquirrelBehavior : MonoBehaviour
{
    // === 도망 행동 설정 ===
    [Header("도망 행동 설정")]
    [Tooltip("플레이어가 시야에서 벗어났을 때 몬스터가 멈출 거리입니다. (도주 목표 거리)")]
    public float stopFleeDistance = 20f;
    [Tooltip("도망치는 방향으로의 이동 속도 배율입니다.")]
    public float fleeSpeedMultiplier = 1.5f;
    [Tooltip("도주 상태에서 플레이어 반대 방향으로 회전하는 속도입니다.")]
    [SerializeField] private float rotationSpeed = 10.0f;

    [Header("사운드 설정")]
    [Tooltip("플레이어 감지 시 한 번 재생되는 놀라는 소리(예: '찍')")]
    public AudioClip startledClip;

    [Header("경직 설정")]
    [Tooltip("경직 효과가 지속될 최소 시간입니다. (랜덤 범위)")]
    [SerializeField] private float minStunDuration = 1.0f;
    [Tooltip("경직 효과가 지속될 최대 시간입니다. (랜덤 범위)")]
    [SerializeField] private float maxStunDuration = 2.0f;

    // === 종속성 ===
    private Monster monster;
    private MonsterPatrol monsterPatrol;
    private MonsterCombat monsterCombat; //경직 이벤트 구독을 위한 Combat 참조
    private Transform playerTransform;
    private Animator animator;
    private AudioSource audioSource;
    private Coroutine stunCoroutine; // 경직 코루틴 참조

    // 몬스터의 기본 순찰 속도 참조 (도망 속도 계산에 사용)
    private float basePatrolSpeed = 3.0f;

    void Awake()
    {
        // === 필수 컴포넌트 참조 ===
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        monsterCombat = GetComponent<MonsterCombat>(); //MonsterCombat 참조 추가

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (monster == null || monsterPatrol == null || monsterCombat == null || animator == null)
        {
            Debug.LogError("SquirrelBehavior: 필수 컴포넌트(Monster, MonsterPatrol, MonsterCombat, Animator)를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        if (monster.monsterData != null)
        {
            basePatrolSpeed = monster.monsterData.moveSpeed;
        }
    }

    private void Start()
    {
        // 초기 상태를 Patrol로 설정합니다.
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
        animator.SetFloat("Vert", 0.5f);

        // **[핵심 추가]** MonsterCombat의 경직 이벤트 구독
        monsterCombat.OnStunApplied += ApplyHitStun;
    }

    // **[추가]** 컴포넌트가 파괴될 때 이벤트 구독 해제
    private void OnDestroy()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnStunApplied -= ApplyHitStun;
        }
    }

    /// <summary>
    /// MonsterCombat.OnStunApplied 이벤트 발생 시 호출됩니다.
    /// 몬스터의 상태를 Stun으로 변경하고 경직 타이머를 시작합니다.
    /// </summary>
    private void ApplyHitStun()
    {
        // 몬스터가 이미 사망했거나 현재 경직 중이라면 로직을 무시합니다.
        if (monster.currentState == MonsterBase.MonsterState.Dead) return;

        // 이전에 진행 중이던 경직 코루틴이 있다면 중지하고 새 경직을 적용합니다. (경직 갱신)
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        // 경직 코루틴을 시작합니다.
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    /// <summary>
    /// 경직 상태를 관리하고 타이머가 끝나면 이전 상태로 복귀시키는 코루틴입니다.
    /// </summary>
    private IEnumerator StunRoutine()
    {
        // 1. 경직 직전 상태를 저장합니다.
        MonsterBase.MonsterState previousState = monster.currentState;

        // 2. 상태를 Stun으로 전환
        monster.ChangeState(MonsterBase.MonsterState.Stun);

        // 3. **[핵심 추가]** 순찰 및 모든 이동을 즉시 중지!
        monsterPatrol.StopPatrol();

        // 4. 애니메이션 정지 (멈춘 것처럼 보이도록)
        animator.SetFloat("Vert", 0f);

        // 5. 경직 시간 대기
        float duration = UnityEngine.Random.Range(minStunDuration, maxStunDuration);
        yield return new WaitForSeconds(duration);

        // 6. 경직 해제 및 상태 복귀
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            // 이전 상태로 복귀
            monster.ChangeState(previousState);

            if (previousState == MonsterBase.MonsterState.Patrol)
            {
                // 순찰 상태로 복귀 시 순찰을 다시 시작해야 합니다.
                monsterPatrol.StartPatrol();
                animator.SetFloat("Vert", 0.5f);
            }
            else if (previousState == MonsterBase.MonsterState.Flee)
            {
                // Flee 상태로 복귀 시, Update 루프의 HandleFleeState가 이동을 처리합니다.
                animator.SetFloat("Vert", 1f);
            }
            else // Idle 상태로 복귀
            {
                animator.SetFloat("Vert", 0f);
            }
        }
        // 코루틴 종료
    }

    void Update()
    {
        // === 몬스터 상태 확인 및 예외 처리 (Dead, Game Over) ===
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            return;
        }

        if (MainSceneManager.Instance != null && MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                HandlePatrolState();
                break;

            case MonsterBase.MonsterState.Flee:
                HandleFleeState(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Stun:
                // Stun 상태일 때는 Patrol/Flee 로직을 실행하지 않고 대기합니다.
                // 이동 정지는 MoveAwayFromTarget에서 처리되며, 애니메이션은 StunRoutine에서 0f로 설정됩니다.
                break;

            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StopPatrol();
                animator.SetFloat("Vert", 0f);
                break;
        }
    }

    // ---------------------------------------------------------------------
    // === 상태별 전용 핸들러 메서드 ===
    // ---------------------------------------------------------------------

    /// <summary>
    /// 순찰 상태 로직을 처리합니다. 플레이어 감지 시 Flee 상태로 전환합니다.
    /// </summary>
    private void HandlePatrolState()
    {
        // 순찰 상태일 때만 순찰을 시작합니다.
        monsterPatrol.StartPatrol();
        animator.SetFloat("Vert", 0.5f); // 걷는 모션

        // Monster가 플레이어(detectableTarget)를 감지했다면 도망 상태로 전환
        if (monster.detectableTarget != null)
        {
            // [효과음 추가 로직] 감지 순간 딱 한번만 재생됩니다.
            if (audioSource != null && startledClip != null)
            {
                // PlayOneShot을 사용하여 현재 재생 중인 클립에 영향을 주지 않고 한 번만 재생합니다.
                audioSource.PlayOneShot(startledClip);
            }
            monster.ChangeState(MonsterBase.MonsterState.Flee);
            monsterPatrol.StopPatrol(); // 순찰 중지
        }
    }

    /// <summary>
    /// 도망 상태 로직을 처리합니다.
    /// </summary>
    private void HandleFleeState(float distanceToPlayer)
    {
        // 1. 도주 해제 조건 확인
        // 플레이어를 놓치고(detectableTarget == null) 충분한 거리를 확보했으면 Patrol로 복귀
        if (monster.detectableTarget == null && distanceToPlayer > stopFleeDistance)
        {
            // 충분히 멀리 도망쳤으면 다시 Patrol 상태로 돌아갑니다.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
            animator.SetFloat("Vert", 0.5f); // 걷는 모션으로 복귀
            return;
        }

        // 2. 도주 실행 (detectableTarget이 있거나, stopFleeDistance 내에 있으면 계속 도망)
        if (distanceToPlayer < stopFleeDistance || monster.detectableTarget != null)
        {
            monsterPatrol.StopPatrol();

            // MoveAwayFromTarget 헬퍼 메서드 내부에서 Stun 상태를 확인하여 이동을 차단합니다.
            MoveAwayFromTarget(playerTransform, basePatrolSpeed * fleeSpeedMultiplier, stopFleeDistance);

            // 경직 중에도 회전은 계속 실행됩니다.
            RotateAwayFromTarget(playerTransform);

            animator.SetFloat("Vert", 1f); // 뛰는 모션
        }
        else
        {
            // 도주 목표 거리에 도달했고 플레이어를 놓쳤다면 정지
            monster.ChangeState(MonsterBase.MonsterState.Idle);
            animator.SetFloat("Vert", 0f);
        }
    }

    /// <summary>
    /// 몬스터를 지정된 속도로 목표 지점(target)에게서 멀어지도록 이동시킵니다. (도주)
    /// </summary>
    /// <param name="target">멀어질 목표 지점의 Transform.</param>
    /// <param name="speed">적용할 이동 속도.</param>
    /// <param name="stoppingDistance">도주를 멈출 거리.</param>
    private void MoveAwayFromTarget(Transform target, float speed, float stoppingDistance)
    {
        //몬스터가 Stun 상태라면 이동 명령을 실행하지 않고 즉시 종료합니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun)
        {
            // 이동만 멈추고, 회전은 RotateAwayFromTarget에서 계속 실행됩니다.
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 도주 조건이 충족되면 이동
        if (distanceToTarget < stoppingDistance || monster.detectableTarget != null)
        {
            // 도주 방향 벡터 = (몬스터 위치 - 타겟 위치) (XZ 평면만 고려)
            Vector3 direction = transform.position - target.position;
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

            // 이동 (기존 FleeFromPlayer 로직과 동일)
            transform.Translate(flatDirection.normalized * speed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// 몬스터를 목표의 반대 방향으로 부드럽게 회전시킵니다. (도주)
    /// 이 메서드는 Stun 상태와 관계없이 실행되어 경직 중에도 시선 처리가 가능합니다.
    /// </summary>
    /// <param name="target">반대 방향을 계산할 목표 지점의 Transform.</param>
    private void RotateAwayFromTarget(Transform target)
    {
        // Stun 상태와 관계없이 실행
        Vector3 direction = transform.position - target.position;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            // 기존 FleeFromPlayer의 Time.deltaTime * 5f를 rotationSpeed로 대체하여 통일
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}