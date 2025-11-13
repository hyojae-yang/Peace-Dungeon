using UnityEngine;
using System.Collections;
using System; // 이벤트 및 코루틴 사용

/// <summary>
/// 사슴 몬스터의 고유한 행동 로직(도망치기, 공격)을 담당하는 클래스입니다.
/// MonsterPatrol 컴포넌트를 제어하고, 데미지 이벤트에 반응하여 행동을 바꿉니다.
/// SOLID 원칙: SRP(단일 책임 원칙)에 따라 사슴의 고유 행동 로직(Flee, Attack 결정)만 담당합니다.
/// </summary>
public class DeerBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조
    private Transform playerTransform;
    private Animator animator;
    private AudioSource audioSource;
    private Coroutine stunCoroutine; // 경직 코루틴 참조

    [Header("사운드 설정")]
    [Tooltip("도망 상태로 전환될 때 한 번 재생되는 놀람 소리.")]
    public AudioClip fleeStartClip;
    [Tooltip("공격 추격 상태에서 반복 재생되는 괴성/포효 소리.")]
    public AudioClip attackRoarLoop;
    [Tooltip("실제 공격 시 한 번 재생되는 타격 효과음.")]
    public AudioClip attackHitClip;

    // === 사슴 행동 설정 ===
    [Header("사슴 행동 설정")]
    [Tooltip("플레이어에게서 충분히 멀어져서 멈출 거리입니다. (도주 목표 거리)")]
    public float stopFleeDistance = 20f;
    [Tooltip("플레이어에게 데미지를 입었을 때 공격을 시작할 거리입니다.")]
    public float attackDistance = 3f;
    [Tooltip("공격 시 추격 이동 속도입니다. (Patrol 속도보다 빨라야 함)")]
    public float attackSpeed = 6f;
    [Tooltip("공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;
    [Tooltip("도망치는 방향으로의 이동 속도 배율입니다.")]
    public float fleeSpeedMultiplier = 1.5f;

    // **[핵심 추가]** 공격 딜레이 변수
    [Tooltip("공격 애니메이션 시작 후, 실제 데미지가 적용되기까지의 시간(선딜레이)입니다.")]
    public float attackPreDelay = 0.5f;
    [Tooltip("데미지 적용 후, 다음 공격 쿨타임까지 남은 시간(후딜레이)입니다.")]
    public float attackPostDelay = 0.5f;

    [Tooltip("도주/추격 상태에서 플레이어를 향해 회전하는 속도입니다.")]
    [SerializeField] private float rotationSpeed = 10.0f;

    // 기존 attackAnimationDuration은 사용하지 않거나, attackPreDelay + attackPostDelay와 연동되게 조정할 수 있습니다.
    // 여기서는 로직을 명확히 하기 위해 제거하고 위 두 딜레이를 사용합니다.

    // === 경직 설정 ===
    [Header("경직 설정")]
    [Tooltip("경직 효과가 지속될 최소 시간입니다. (랜덤 범위)")]
    [SerializeField] private float minStunDuration = 0.5f;
    [Tooltip("경직 효과가 지속될 최대 시간입니다. (랜덤 범위)")]
    [SerializeField] private float maxStunDuration = 1.0f;

    // === 내부 변수 ===
    private bool hasTakenDamage = false; // 데미지를 입었는지 여부 (복수심 트리거)
    private float lastAttackTime;        // 마지막 공격 시간
    private bool isAttacking = false;    // 현재 공격 애니메이션 재생 중 여부 (이동/회전 제어용 플래그)
    private float basePatrolSpeed = 3.0f;
    private Coroutine attackSequenceCoroutine; // **[추가]** 공격 코루틴 참조

    void Awake()
    {
        // === 필수 컴포넌트 참조 ===
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (monster == null || monsterCombat == null || monsterPatrol == null || animator == null)
        {
            UnityEngine.Debug.LogError("DeerBehavior: 필수 컴포넌트(Monster, MonsterCombat, MonsterPatrol, Animator)를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 플레이어 트랜스폼 찾기 (Tag 기반)
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // 사슴 몬스터는 기본적으로 순찰 상태로 시작합니다.
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
        animator.SetFloat("Vert", 1f);
        animator.SetFloat("State", 0f);

        if (monster.monsterData != null)
        {
            basePatrolSpeed = monster.monsterData.moveSpeed;
        }
    }

    /// <summary>
    /// 스크립트가 활성화될 때 데미지 및 경직 이벤트를 구독합니다.
    /// </summary>
    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
            // 경직 이벤트 구독
            monsterCombat.OnStunApplied += ApplyHitStun;
        }
    }

    /// <summary>
    /// 스크립트가 비활성화될 때 데미지 및 경직 이벤트를 구독 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
            // 경직 이벤트 구독 해제
            monsterCombat.OnStunApplied -= ApplyHitStun;
        }
        StopAllCoroutines();
        if (audioSource != null && audioSource.isPlaying && audioSource.loop)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// MonsterCombat에서 데미지 이벤트가 발생했을 때 호출됩니다.
    /// </summary>
    private void OnMonsterDamaged(float damage)
    {
        // 경직 중에는 상태 전환 로직을 실행하지 않습니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun) return;

        hasTakenDamage = true; // 복수심 발동!

        // 공격 중 피격 시 코루틴을 중지하여 바로 추격으로 전환합니다.
        if (isAttacking && attackSequenceCoroutine != null)
        {
            StopCoroutine(attackSequenceCoroutine);
            isAttacking = false;
        }

        // 데미지를 입으면 즉시 공격(추격) 상태로 전환합니다.
        if (monster.currentState != MonsterBase.MonsterState.Attack)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            StartAttackRoarLoop();
        }

        // StunRoutine이 아닌 OnMonsterDamaged에서 StopAllCoroutines()을 사용하면
        // AttackSequenceCoroutine 외의 다른 코루틴(예: Patrol)도 멈출 수 있으므로,
        // 명시적으로 AttackSequenceCoroutine만 중지하도록 로직을 변경했습니다.
    }

    /// <summary>
    /// MonsterCombat.OnStunApplied 이벤트 발생 시 호출되며 경직 코루틴을 시작합니다.
    /// </summary>
    private void ApplyHitStun()
    {
        if (monster.currentState == MonsterBase.MonsterState.Dead) return;

        // 공격 애니메이션 중이라면 코루틴을 종료하고 isAttacking 플래그도 초기화합니다.
        if (isAttacking && attackSequenceCoroutine != null)
        {
            StopCoroutine(attackSequenceCoroutine);
            isAttacking = false;
        }

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

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

        // 3. 순찰 이동 중지 및 사운드/애니메이션 정지
        monsterPatrol.StopPatrol();
        StopRoarLoop(); // 공격 포효 루프 정지
        animator.SetFloat("Vert", 0f);
        animator.SetFloat("State", 0f);

        // 4. 경직 시간 대기
        float duration = UnityEngine.Random.Range(minStunDuration, maxStunDuration);
        yield return new WaitForSeconds(duration);

        // 5. 경직 해제 및 상태 복귀
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            // 이전 상태로 복귀
            monster.ChangeState(previousState);

            // 6. 복귀 상태에 따라 행동 재개
            if (previousState == MonsterBase.MonsterState.Attack)
            {
                // Attack 상태로 복귀 시 포효 사운드 재개
                StartAttackRoarLoop();
            }
            else if (previousState == MonsterBase.MonsterState.Patrol)
            {
                // Patrol 상태로 복귀 시 순찰 재개
                monsterPatrol.StartPatrol();
            }
        }
        // 코루틴 종료
        stunCoroutine = null;
    }


    void Update()
    {
        // === 몬스터 상태 확인 및 예외 처리 (Dead, Game Over) ===
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
            StopRoarLoop();
            return;
        }
        if (MainSceneManager.Instance != null && MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
            StopRoarLoop();
            return;
        }

        // Stun 상태일 때는 모든 행동 로직을 건너뜁니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun)
        {
            return;
        }

        // 플레이어와의 거리는 이동/정지 조건에만 사용
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // === 상태 머신 실행 ===
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                HandlePatrolState();
                break;

            case MonsterBase.MonsterState.Flee:
                HandleFleeState(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                HandleAttackState(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                // Idle 상태에 대한 처리가 필요하다면 여기에 추가
                monsterPatrol.StopPatrol();
                animator.SetFloat("Vert", 0f);
                animator.SetFloat("State", 0f);
                break;
        }
    }
    // ---------------------------------------------------------------------
    // === 사운드 제어 헬퍼 메서드 (변경 없음) ===
    // ---------------------------------------------------------------------

    /// <summary>
    /// 공격 상태에서 재생되는 괴성 루프 사운드를 시작합니다.
    /// </summary>
    private void StartAttackRoarLoop()
    {
        if (audioSource != null && attackRoarLoop != null)
        {
            audioSource.clip = attackRoarLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// 현재 재생 중인 루프 사운드를 정지하고 설정을 초기화합니다.
    /// </summary>
    private void StopRoarLoop()
    {
        if (audioSource != null && audioSource.loop)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
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
        animator.SetFloat("Vert", 1f);
        animator.SetFloat("State", 0f);

        if (!hasTakenDamage && monster.detectableTarget != null)
        {
            if (audioSource != null && fleeStartClip != null)
            {
                audioSource.PlayOneShot(fleeStartClip);
            }
            monster.ChangeState(MonsterBase.MonsterState.Flee);
            monsterPatrol.StopPatrol(); // 순찰 중지
        }
        else
        {
            monsterPatrol.StartPatrol(); // 순찰 계속
        }
    }

    /// <summary>
    /// 도망 상태 로직을 처리합니다.
    /// </summary>
    private void HandleFleeState(float distanceToPlayer)
    {
        // 1. 복수심이 발동되면 Flee 상태를 벗어납니다.
        if (hasTakenDamage)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            monsterPatrol.StopPatrol();
            StartAttackRoarLoop();
            return;
        }

        // 2. 플레이어 감지 해제 또는 충분한 거리를 확보했는지 확인
        if (monster.detectableTarget == null && distanceToPlayer > stopFleeDistance)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
            return;
        }

        // 3. 도주 실행: 도주 목표 거리에 도달하지 않았거나
        if (distanceToPlayer < stopFleeDistance || monster.detectableTarget != null)
        {
            monsterPatrol.StopPatrol();

            MoveAwayFromTarget(playerTransform, basePatrolSpeed * fleeSpeedMultiplier, stopFleeDistance);
            RotateAwayFromTarget(playerTransform);

            animator.SetFloat("Vert", 1f);
            animator.SetFloat("State", 1f);
        }
        else
        {
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
        }
    }

    /// <summary>
    /// 플레이어를 추격 및 공격하는 로직을 실행합니다.
    /// </summary>
    private void HandleAttackState(float distanceToPlayer)
    {
        if (audioSource != null && (audioSource.clip != attackRoarLoop || !audioSource.loop || !audioSource.isPlaying))
        {
            StartAttackRoarLoop();
        }
        monsterPatrol.StopPatrol();
        AttackPlayer(distanceToPlayer);
    }

    /// <summary>
    /// 플레이어를 추격 및 공격하는 로직을 실행합니다.
    /// </summary>
    private void AttackPlayer(float distanceToPlayer)
    {
        // isAttacking 플래그 확인: 공격 애니메이션 재생 중이면 이동/회전 로직 건너뛰고 멈춰있습니다.
        if (isAttacking)
        {
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
            return;
        }

        if (distanceToPlayer > attackDistance)
        {
            // === 추격 로직 (뛰는 모션 적용) ===
            MoveTowardsTarget(playerTransform, attackSpeed, attackDistance - 0.1f);
            RotateTowardsTarget(playerTransform);

            animator.SetFloat("Vert", 1f);
            animator.SetFloat("State", 1f);
        }
        else
        {
            // === 공격 준비 및 실행 ===
            RotateTowardsTarget(playerTransform, 10f); // 빠르게 바라보기

            PerformAttack();
        }
    }

    /// <summary>
    /// 공격 쿨타임을 확인하고 공격 시퀀스 코루틴을 시작합니다.
    /// </summary>
    private void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            // 중복 실행 방지
            if (attackSequenceCoroutine == null)
            {
                attackSequenceCoroutine = StartCoroutine(AttackSequenceCoroutine());
            }
        }
    }

    /// <summary>
    /// 공격 애니메이션 재생, 선딜레이, 데미지 적용, 후딜레이를 담당하는 코루틴입니다. **[핵심 수정]**
    /// </summary>
    private IEnumerator AttackSequenceCoroutine()
    {
        // 1. 공격 시작 - 이동 중지
        isAttacking = true;
        animator.SetTrigger("Attack"); // 공격 애니메이션 트리거

        // 2. 선딜레이 대기 (데미지 적용 전 대기 시간)
        yield return new WaitForSeconds(attackPreDelay);

        // **[핵심 체크 1]** 선딜레이 후, Stun 상태가 되었다면 데미지 로직을 건너뜁니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun)
        {
            isAttacking = false;
            attackSequenceCoroutine = null;
            yield break;
        }

        // 3. 실제 데미지 적용 시점: 플레이어가 여전히 공격 범위 내에 있는지 확인
        float currentDistance = Vector3.Distance(transform.position, playerTransform.position);

        if (currentDistance <= attackDistance)
        {
            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                if (audioSource != null && attackHitClip != null)
                {
                    audioSource.PlayOneShot(attackHitClip);
                }
                // 데미지 입히기
                playerDamageable.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
            }
        }

        // 4. 후딜레이 대기 (쿨타임의 남은 시간)
        // 전체 쿨타임에서 선딜레이를 뺀 시간을 대기합니다.
        float remainingCooldown = attackCooldown - attackPreDelay;
        if (remainingCooldown > 0)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        // 5. 공격 종료 및 쿨타임 설정
        lastAttackTime = Time.time;
        isAttacking = false; // 이동 로직 재개 허용
        attackSequenceCoroutine = null; // 코루틴 참조 해제
    }

    // ---------------------------------------------------------------------
    // === 이동/회전 헬퍼 메서드 (변경 없음) ===
    // ---------------------------------------------------------------------

    /// <summary>
    /// 몬스터를 지정된 속도로 목표 지점(target)을 향해 이동시킵니다. (추격)
    /// </summary>
    private void MoveTowardsTarget(Transform target, float speed, float stoppingDistance)
    {
        // Update()에서 Stun 상태가 이미 필터링되므로, 여기에 Stun 체크는 불필요합니다.
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > stoppingDistance)
        {
            Vector3 direction = target.position - transform.position;
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);
            transform.position += flatDirection.normalized * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 몬스터를 지정된 속도로 목표 지점(target)에게서 멀어지도록 이동시킵니다. (도주)
    /// </summary>
    private void MoveAwayFromTarget(Transform target, float speed, float stoppingDistance)
    {
        // Update()에서 Stun 상태가 이미 필터링되므로, 여기에 Stun 체크는 불필요합니다.
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < stoppingDistance || monster.detectableTarget != null)
        {
            Vector3 direction = transform.position - target.position;
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);
            transform.position += flatDirection.normalized * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 몬스터를 목표를 향해 부드럽게 회전시킵니다. (추격)
    /// </summary>
    private void RotateTowardsTarget(Transform target, float slerpSpeed = -1f)
    {
        // Update()에서 Stun 상태가 이미 필터링되므로, 여기에 Stun 체크는 불필요합니다.
        float finalSpeed = (slerpSpeed > 0) ? slerpSpeed : rotationSpeed;

        Vector3 direction = target.position - transform.position;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, finalSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 몬스터를 목표의 반대 방향으로 부드럽게 회전시킵니다. (도주)
    /// </summary>
    private void RotateAwayFromTarget(Transform target)
    {
        // Update()에서 Stun 상태가 이미 필터링되므로, 여기에 Stun 체크는 불필요합니다.
        Vector3 direction = transform.position - target.position;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}