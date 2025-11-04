using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// 늑대 몬스터의 고유한 행동 로직을 담당하는 클래스입니다.
/// 플레이어를 공격하다 체력이 절반 이하로 떨어지면 주변 동료를 소집해 무리를 형성하고 함께 공격합니다.
/// 합류한 늑대는 리더와 독립적으로 플레이어를 추적합니다.
/// SOLID 원칙: SRP, OCP 준수.
/// </summary>
public class WolfBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;           // 몬스터의 기본 데이터 및 상태 관리를 위한 컴포넌트
    private MonsterPatrol monsterPatrol; // 몬스터의 순찰 로직을 처리하는 컴포넌트 (SRP 준수)
    private MonsterCombat monsterCombat;  // 몬스터의 전투, 체력 관리 로직을 처리하는 컴포넌트 (SRP 준수)
    private Transform playerTransform; // 플레이어의 위치를 참조하기 위한 Transform
    private Animator animator;         // 애니메이션 제어를 위한 컴포넌트
    private AudioSource audioSource;  // AudioSource 종속성 추가
    private Coroutine stunCoroutine; // **[추가]** 경직 코루틴 참조

    // === 사운드 설정 추가 ===
    [Header("사운드 설정")]
    [Tooltip("체력이 일정 비율 이하로 떨어져 무리를 소집할 때 재생되는 울음소리입니다.")]
    public AudioClip callForHelpClip;
    [Tooltip("플레이어에게 일반 공격을 시도할 때 재생되는 효과음입니다.")]
    public AudioClip normalAttackClip;

    // === 행동 설정 ===
    [Header("늑대 행동 설정")]
    [Tooltip("체력이 이 비율 이하로 떨어지면 무리를 소집합니다.")]
    [Range(0.1f, 0.9f)]
    public float callForHelpHealthRatio = 0.5f;
    [Tooltip("동료를 찾기 위해 주변을 탐색할 반경입니다.")]
    public float flockDetectionRadius = 15f;
    [Tooltip("플레이어에게 공격을 시작하는 최소 거리입니다.")]
    public float attackRange = 2f;
    [Tooltip("무리 공격 시 몬스터의 이동 속도입니다.")]
    public float packAttackSpeed = 5f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;
    [Tooltip("몬스터의 회전 속도 (클수록 더 빠르게 회전합니다.)")]
    public float rotationSpeed = 8f;

    [Tooltip("추종자가 리더 주변에 자리잡았다고 판단하고 플레이어를 목표로 전환할 거리입니다.")]
    public float followerJoinRange = 3.5f;
    [Tooltip("추종자가 리더 주변의 목표 위치에 도달했다고 판단하는 거리입니다.")]
    public float followerStoppingDistance = 0.2f;

    // === 경직 설정 추가 ===
    [Header("경직 설정")]
    [Tooltip("경직 효과가 지속될 최소 시간입니다. (랜덤 범위)")]
    [SerializeField] private float minStunDuration = 0.7f;
    [Tooltip("경직 효과가 지속될 최대 시간입니다. (랜덤 범위)")]
    [SerializeField] private float maxStunDuration = 1.5f;


    // === 내부 상태 변수 ===
    private bool hasCalledForHelp = false; // 무리 소집을 한 번만 하도록 플래그 설정
    private bool isLeader = false;           // 현재 늑대가 무리의 리더인지 여부
    private WolfBehavior leader;            // 추종자인 경우, 리더 늑대의 참조
    private List<WolfBehavior> followers = new List<WolfBehavior>(); // 리더인 경우, 추종자 늑대 목록
    private float lastAttackTime;            // 마지막 공격 시간 기록
    private bool isJoinedPack = false;       // 추종자가 리더 주변 합류 위치에 도달했는지 여부
    private Vector3 initialFlockTarget = Vector3.zero; // 추종자가 합류해야 할, 리더 주변의 고정된 목표 위치

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        monsterCombat = GetComponent<MonsterCombat>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 참조

        if (monster == null || monsterPatrol == null || monsterCombat == null || animator == null || audioSource == null)
        {
            Debug.LogError("WolfBehavior: 필수 컴포넌트(Monster, MonsterPatrol, MonsterCombat, Animator, AudioSource) 중 일부를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    /// <summary>
    /// 몬스터 시작 시 초기 상태를 Patrol로 설정하여 순찰이 시작되도록 합니다.
    /// </summary>
    void Start()
    {
        if (monster != null)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    void OnEnable()
    {
        // 몬스터가 데미지를 입을 때 OnMonsterDamaged 메서드를 호출하도록 구독
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
            // **[핵심 추가]** 경직 이벤트 구독
            monsterCombat.OnStunApplied += ApplyHitStun;
        }
    }

    void OnDisable()
    {
        // 구독 해제
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
            // **[핵심 추가]** 경직 이벤트 구독 해제
            monsterCombat.OnStunApplied -= ApplyHitStun;
        }
    }

    /// <summary>
    /// MonsterCombat.OnStunApplied 이벤트 발생 시 호출되며 경직 코루틴을 시작합니다. **[추가]**
    /// </summary>
    private void ApplyHitStun()
    {
        if (monster.currentState == MonsterBase.MonsterState.Dead) return;

        // 진행 중이던 경직 코루틴이 있다면 중지
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        // 경직 코루틴 시작
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    /// <summary>
    /// 경직 상태를 관리하고 타이머가 끝나면 이전 상태로 복귀시키는 코루틴입니다. **[추가]**
    /// </summary>
    private IEnumerator StunRoutine()
    {
        // 1. 경직 직전 상태를 저장합니다.
        MonsterBase.MonsterState previousState = monster.currentState;

        // 2. 상태를 Stun으로 전환
        monster.ChangeState(MonsterBase.MonsterState.Stun);

        // 3. 순찰 이동 중지 및 애니메이션 정지 (경직 모션 또는 Idle로 전환)
        monsterPatrol.StopPatrol();
        animator.SetFloat("Run", 0f);

        // 4. 경직 시간 대기
        float duration = UnityEngine.Random.Range(minStunDuration, maxStunDuration);
        yield return new WaitForSeconds(duration);

        // 5. 경직 해제 및 상태 복귀
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            // 이전 상태로 복귀 (늑대는 보통 Patrol, Chase, Flocking 중 하나로 복귀)
            monster.ChangeState(previousState);

            // 복귀 상태에 따라 순찰 재시작을 명시적으로 호출합니다.
            if (previousState == MonsterBase.MonsterState.Patrol)
            {
                monsterPatrol.StartPatrol();
                animator.SetFloat("Run", 0f); // Patrol은 걷는 모션
            }
            else if (previousState == MonsterBase.MonsterState.Chase || previousState == MonsterBase.MonsterState.Flocking)
            {
                animator.SetFloat("Run", 1f); // 추격/무리 상태는 뛰는 모션
            }
            // Attack 상태였다면 다음 Update()에서 Chase로 전환되거나 다시 Attack을 시도합니다.
        }
        // 코루틴 종료
    }


    /// <summary>
    /// 몬스터가 데미지를 입었을 때 호출되는 메서드입니다.
    /// 체력 비율을 확인하여 무리 소집을 결정합니다.
    /// </summary>
    /// <param name="damage">입은 피해량</param>
    private void OnMonsterDamaged(float damage)
    {
        // 경직 상태와 무관하게 무리 소집 로직은 실행됩니다.

        // 체력이 절반 이하로 떨어지면 동료를 소집
        if (!hasCalledForHelp && monsterCombat.GetCurrentHealth() <= monster.monsterData.maxHealth * callForHelpHealthRatio)
        {
            // 울부짖기 애니메이션 재생
            if (animator != null)
            {
                // 경직 중에도 애니메이션 트리거는 실행하여 울부짖기 시퀀스를 만듭니다.
                // (애니메이션이 Stun 애니메이션을 덮어쓰도록 가정)
                animator.SetTrigger("Howl");
            }

            // 동료 소집(울부짖음) 사운드 재생
            if (audioSource != null && callForHelpClip != null)
            {
                audioSource.PlayOneShot(callForHelpClip);
            }

            CallForHelp();
        }
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        // **[핵심 추가]** Stun 상태일 때는 모든 행동 로직을 건너뛰고 정지합니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun)
        {
            // StunRoutine에서 애니메이션과 이동을 정지시켰으므로 여기서는 대기
            return;
        }

        // 게임 오버 상태 체크
        if (MainSceneManager.Instance != null && MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            if (animator != null) animator.SetFloat("Run", 0f);
            return;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 현재 상태에 따른 행동 로직 호출 (상태 패턴)
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Chase:
                HandleChase(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Flocking:
                HandleFlocking(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Attack:
                HandleAttack(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StopPatrol();
                if (animator != null) animator.SetFloat("Run", 0f);
                break;
        }
    }

    /// <summary>
    /// 순찰 상태 로직을 처리합니다.
    /// </summary>
    private void HandlePatrol(float distanceToPlayer)
    {
        monsterPatrol.StartPatrol();

        if (animator != null)
        {
            animator.SetFloat("Run", 0f); // 걷는 모션 (Patrol)
        }

        if (distanceToPlayer < monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Chase);
            monsterPatrol.StopPatrol();
        }
    }

    /// <summary>
    /// 추적 상태 로직을 처리합니다.
    /// </summary>
    private void HandleChase(float distanceToPlayer)
    {
        if (animator != null)
        {
            animator.SetFloat("Run", 1f); // 뛰는 모션 (Chase)
        }

        if (playerTransform != null)
        {
            // MoveTowardsTarget으로 플레이어 추격
            MoveTowardsTarget(playerTransform, monster.monsterData.moveSpeed * 1.5f, attackRange - 0.1f);
        }

        // 1. 플레이어가 공격 범위에 들어오면 공격 상태로 전환
        if (distanceToPlayer <= attackRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            return;
        }

        // 2. 플레이어가 감지 범위를 벗어나면 순찰 상태로 돌아감 (무리 해제 로직 포함)
        if (distanceToPlayer > monster.detectionRange + 2f)
        {
            // 감지 범위를 벗어나면 Patrol로 전환합니다.
            ExitPackInternal();
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 일반 공격 상태 로직을 처리합니다.
    /// </summary>
    private void HandleAttack(float distanceToPlayer)
    {
        if (animator != null) animator.SetFloat("Run", 0f); // 정지

        if (distanceToPlayer > attackRange)
        {
            // 공격 범위를 벗어나면 Chase 상태로 전환
            monster.ChangeState(MonsterBase.MonsterState.Chase);
        }
        else
        {
            PerformAttack();
        }
    }

    /// <summary>
    /// 무리 행동 상태 로직을 처리합니다. (Flocking 상태)
    /// 추종자는 리더에게 합류 후 목표를 플레이어를 추적하는 Chase 상태로 전환합니다.
    /// </summary>
    private void HandleFlocking(float distanceToPlayer)
    {
        if (animator != null)
        {
            animator.SetFloat("Run", 1f); // 뛰는 모션 (Flocking)
        }

        // 1. 공격 범위 체크 (리더/추종자 공통)
        if (distanceToPlayer <= attackRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            return;
        }

        // 2. 이동 처리 (역할에 따라 분리)
        if (isLeader)
        {
            // 리더: 플레이어를 직접 추격
            MoveTowardsTarget(playerTransform, packAttackSpeed, attackRange - 0.1f);
        }
        else if (leader != null) // 추종자
        {
            // 추종자가 합류를 완료하면 Chase 상태로 즉시 전환합니다.**
            if (isJoinedPack)
            {
                monster.ChangeState(MonsterBase.MonsterState.Chase);
                return;
            }

            // 2-1. [합류 단계] 고정된 목표 위치로 이동
            MoveTowardsPosition(initialFlockTarget, packAttackSpeed, followerStoppingDistance);

            // 합류 중에는 목표 지점이 아닌, 플레이어 방향을 바라보도록 강제 회전 로직을 덮어씁니다.
            if (playerTransform != null)
            {
                Vector3 lookDirection = playerTransform.position - transform.position;
                lookDirection.y = 0;

                if (lookDirection != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(lookDirection.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                }
            }

            // **합류 여부 판단 및 목표 전환**
            float distanceToTargetPos = Vector3.Distance(transform.position, initialFlockTarget);
            if (distanceToTargetPos <= followerStoppingDistance)
            {
                // 합류 완료. Chase 상태로 전환을 예약 (다음 Update에서 바로 전환)
                isJoinedPack = true;
            }
        }

        // 3. 플레이어가 일정 범위를 벗어나면 무리 해제 및 추적 중단 (리더만 결정)
        if (isLeader && distanceToPlayer > monster.detectionRange + 5f)
        {
            ExitPack();
        }
    }

    /// <summary>
    /// 동료 늑대들을 탐색하여 무리를 소집합니다.
    /// </summary>
    private void CallForHelp()
    {
        if (isLeader) return;

        hasCalledForHelp = true;
        isLeader = true;

        // 경직 상태가 아니라면 바로 Flocking으로 전환합니다.
        if (monster.currentState != MonsterBase.MonsterState.Stun)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flocking); // 리더는 Flocking 상태로 전환
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, flockDetectionRadius);

        foreach (var hitCollider in hitColliders)
        {
            WolfBehavior otherWolf = hitCollider.GetComponent<WolfBehavior>();

            if (otherWolf != null && otherWolf != this && otherWolf.monster.currentState != MonsterBase.MonsterState.Dead)
            {
                if (!otherWolf.IsPartOfPack())
                {
                    otherWolf.JoinPack(this);
                    AddFollower(otherWolf);
                }
            }
        }
    }

    /// <summary>
    /// 다른 늑대가 이 늑대를 무리에 합류시키는 데 사용합니다.
    /// </summary>
    /// <param name="newLeader">무리의 리더 늑대</param>
    public void JoinPack(WolfBehavior newLeader)
    {
        if (isLeader) return;

        leader = newLeader;
        isLeader = false;
        isJoinedPack = false;

        Vector3 leaderToPlayerDir = playerTransform.position - leader.transform.position;
        leaderToPlayerDir.y = 0;
        initialFlockTarget = leader.transform.position - leaderToPlayerDir.normalized * followerJoinRange;

        // 추종자도 경직 중이 아니라면 Flocking 상태로 전환합니다.
        if (monster.currentState != MonsterBase.MonsterState.Stun)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flocking); // Flocking 상태로 전환
        }
    }

    /// <summary>
    /// 리더 늑대가 플레이어를 놓쳐 무리를 해산할 때 호출됩니다.
    /// </summary>
    private void ExitPack()
    {
        // 리더만 무리 플래그를 해제하고 순찰 상태로 돌아갑니다.
        followers.Clear();

        // 리더 자신만 상태를 Patrol로 전환
        ExitPackInternal();
    }

    /// <summary>
    /// 무리 이탈의 실제 로직을 수행합니다.
    /// </summary>
    private void ExitPackInternal()
    {
        // 리더-추종자 관계 해제 및 플래그 초기화
        hasCalledForHelp = false;
        isLeader = false;
        leader = null;
        isJoinedPack = false;
        initialFlockTarget = Vector3.zero; // 목표 위치 초기화

        // 상태 전환
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 무리에 속해 있는지 여부를 반환합니다.
    /// </summary>
    public bool IsPartOfPack()
    {
        return leader != null || isLeader;
    }

    /// <summary>
    /// 추종자를 무리 목록에 추가합니다. (리더만 사용)
    /// </summary>
    /// <param name="wolf">추가할 추종자 늑대</param>
    private void AddFollower(WolfBehavior wolf)
    {
        if (!followers.Contains(wolf))
        {
            followers.Add(wolf);
        }
    }

    /// <summary>
    /// 목표 Transform을 향해 이동하는 공통 로직.
    /// </summary>
    /// <param name="targetTransform">목표 Transform</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="stoppingDistance">목표에 도달했다고 판단할 최소 거리</param>
    public void MoveTowardsTarget(Transform targetTransform, float speed, float stoppingDistance = 0.1f)
    {
        if (targetTransform == null) return;
        MoveTowardsPosition(targetTransform.position, speed, stoppingDistance);
    }

    /// <summary>
    /// 특정 위치(Vector3)를 향해 이동하는 공통 로직. (OCP 준수)
    /// </summary>
    /// <param name="targetPosition">목표 Vector3 위치</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="stoppingDistance">목표에 도달했다고 판단할 최소 거리</param>
    public void MoveTowardsPosition(Vector3 targetPosition, float speed, float stoppingDistance = 0.1f)
    {
        Vector3 direction = (targetPosition - transform.position);
        float distance = direction.magnitude;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        // 1. 목표 회전 
        if (flatDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(flatDirection.normalized);
            float slerpSpeed = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, slerpSpeed);
        }

        // 2. 이동 (정지 거리 체크)
        if (distance <= stoppingDistance)
        {
            return;
        }

        // 3. 실제 이동
        transform.position += flatDirection.normalized * speed * Time.deltaTime;
    }

    /// <summary>
    /// 플레이어에게 데미지를 입히는 일반 공격 로직을 실행합니다.
    /// </summary>
    private void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                if (animator != null)
                {
                    animator.SetTrigger("Attack"); // 공격 애니메이션 재생
                }

                //일반 공격 사운드 재생
                if (audioSource != null && normalAttackClip != null)
                {
                    audioSource.PlayOneShot(normalAttackClip);
                }

                playerDamageable.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
                lastAttackTime = Time.time;
            }
        }
    }
}