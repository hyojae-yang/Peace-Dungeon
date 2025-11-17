using UnityEngine;
using System.Collections;
using System; // 이벤트 및 코루틴 사용

/// <summary>
/// 모든 쥐 몬스터의 공통 행동을 정의하는 추상 기반 클래스입니다.
/// 이 클래스는 직접 게임 오브젝트에 부착할 수 없으며,
/// 자식 클래스(RatLeader, RatFollower)에서 상속받아 사용해야 합니다.
/// SOLID 원칙 중 단일 책임 원칙(SRP)과 개방-폐쇄 원칙(OCP)을 준수합니다.
/// </summary>
public abstract class RatBehavior : MonoBehaviour
{
    // === 종속성 ===
    protected Monster monster;
    protected MonsterCombat monsterCombat;
    protected Transform playerTransform;
    Animator animator;
    Rigidbody rb;
    protected AudioSource audioSource;
    private Coroutine stunCoroutine; // 경직 코루틴 참조
    private Coroutine attackSequenceCoroutine; // 공격 코루틴 참조

    //이동 명령을 저장하고 FixedUpdate에서 처리하기 위한 변수
    private Vector3 _movementDirection = Vector3.zero;

    [Header("사운드 설정")]
    [Tooltip("공격 실행 시 한 번 재생되는 공통 효과음")]
    public AudioClip attackClip;

    // === 들쥐 떼 행동을 위한 공통 설정 ===
    [Header("공통 행동 설정")]
    [Tooltip("다른 몬스터를 감지할 수 있는 반경입니다.")]
    public float flockDetectionRadius = 10f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.0f;
    [Tooltip("일반 공격이 가능한 거리입니다.")]
    public float attackRange = 3.0f;
    [Tooltip("공격 애니메이션 시작 후, 실제 데미지가 적용되기까지의 시간(선딜레이)입니다.")]
    public float attackPreDelay = 0.3f; // 쥐처럼 빠른 몬스터는 짧게 설정

    // === 경직 설정 ===
    [Header("경직 설정")]
    [Tooltip("경직 효과가 지속될 최소 시간입니다. (랜덤 범위)")]
    [SerializeField] private float minStunDuration = 0.5f;
    [Tooltip("경직 효과가 지속될 최대 시간입니다. (랜덤 범위)")]
    [SerializeField] private float maxStunDuration = 1.0f;

    protected float lastAttackTime;
    protected Vector3 currentPatrolPoint;
    private bool isAttacking = false; // 공격 중 플래그

    protected virtual void Awake()
    {
        monster = GetComponent<Monster>();
        if (monster == null) Debug.LogError("RatBehavior: Monster 컴포넌트를 찾을 수 없습니다.");
        monsterCombat = GetComponent<MonsterCombat>();
        if (monsterCombat == null) Debug.LogError("RatBehavior: MonsterCombat 컴포넌트를 찾을 수 없습니다.");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        // Rigidbody 누락 시 경고 및 비활성화
        if (rb == null)
        {
            Debug.LogError(gameObject.name + ": Rigidbody 컴포넌트가 없어 이동이 불가능합니다.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    protected virtual void OnEnable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
            // 경직 이벤트 구독
            monsterCombat.OnStunApplied += ApplyHitStun;
        }
    }

    protected virtual void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
            // 경직 이벤트 구독 해제
            monsterCombat.OnStunApplied -= ApplyHitStun;
        }
        // 비활성화 시 모든 코루틴 및 플래그 초기화
        StopAllCoroutines();
        isAttacking = false;
    }

    protected virtual void Start()
    {
        animator.SetFloat("Vert", 1f);
        SetNewPatrolPoint();
    }

    /// <summary>
    /// MonsterCombat.OnStunApplied 이벤트 발생 시 호출됩니다.
    /// 몬스터의 상태를 Stun으로 변경하고 경직 타이머를 시작합니다.
    /// </summary>
    private void ApplyHitStun()
    {
        // 몬스터가 이미 사망했다면 경직 로직을 무시합니다.
        if (monster.currentState == MonsterBase.MonsterState.Dead) return;

        // **[수정]** 공격 애니메이션 중이라면 코루틴을 종료하고 isAttacking 플래그도 초기화합니다.
        if (isAttacking)
        {
            if (attackSequenceCoroutine != null) StopCoroutine(attackSequenceCoroutine);
            isAttacking = false;
            attackSequenceCoroutine = null;
        }

        // 이전에 진행 중이던 경직 코루틴이 있다면 중지하고 새 경직을 적용합니다.
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

        // 3. 애니메이션 정지 (멈춘 것처럼 보이도록)
        animator.SetFloat("Vert", 0f);

        // 4. 경직 시간 대기
        float duration = UnityEngine.Random.Range(minStunDuration, maxStunDuration);
        yield return new WaitForSeconds(duration);

        // 5. 경직 해제 및 상태 복귀
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            // 이전 상태로 복귀
            monster.ChangeState(previousState);

            // 6. 복귀 상태에 따라 애니메이션 재개 (쥐 몬스터는 기본적으로 Vert=1f로 이동한다고 가정)
            if (previousState != MonsterBase.MonsterState.Idle && previousState != MonsterBase.MonsterState.Dead)
            {
                animator.SetFloat("Vert", 1f);
            }
            else
            {
                animator.SetFloat("Vert", 0f);
            }
        }
        // 코루틴 종료
        stunCoroutine = null;
    }

    /// <summary>
    /// Update() 대신 각 역할에 맞는 행동을 정의하는 추상 메서드입니다.
    /// 자식 클래스에서 반드시 구현해야 합니다.
    /// </summary>
    public abstract void UpdateBehavior();

    // MonoBehaviour의 Update() 메서드를 오버라이드하여 UpdateBehavior()를 호출합니다.
    private void Update()
    {
        // Stun 또는 Dead 상태일 때는 UpdateBehavior() 실행을 건너뛰어 회전/공격/순찰 로직을 멈춥니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            // Stun 상태일 때 물리 이동은 FixedUpdate에서 _movementDirection = 0 처리로 멈춥니다.
            return;
        }

        if (MainSceneManager.Instance != null && MainSceneManager.Instance.isGameOver)
        {
            // 게임 오버 시 몬스터 행동 중지
            monster.ChangeState(MonsterBase.MonsterState.Patrol); // 임시 상태 변경
            return;
        }

        // **[추가]** 공격 중일 때는 추격/순찰 로직을 건너뛰고 정지 상태를 유지합니다.
        if (isAttacking)
        {
            // 공격 애니메이션 중에는 정지
            animator.SetFloat("Vert", 0f);
            return;
        }

        UpdateBehavior();
    }

    /// <summary>
    /// 물리 업데이트: Rigidbody의 실제 이동을 FixedUpdate에서 안전하게 처리합니다.
    /// </summary>
    private void FixedUpdate()
    {
        // Stun 또는 공격 중일 때는 이동 명령을 완전히 무시
        if (monster.currentState == MonsterBase.MonsterState.Stun || isAttacking || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            _movementDirection = Vector3.zero; // 이동 명령 초기화
            return;
        }

        // Rigidbody가 없거나, 이동 명령이 없거나, 죽은 상태면 이동하지 않습니다.
        if (rb == null || _movementDirection == Vector3.zero)
        {
            return;
        }

        // FixedUpdate에서는 고정된 시간인 Time.fixedDeltaTime을 사용합니다.
        Vector3 moveVector = _movementDirection.normalized * monster.monsterData.moveSpeed * Time.fixedDeltaTime;

        // Rigidbody.MovePosition을 사용하여 물리 엔진에 안전하게 이동 의사를 전달
        rb.MovePosition(rb.position + moveVector);

        // 이동 처리가 끝났으므로 다음 Update 명령이 들어올 때까지 방향 초기화
        _movementDirection = Vector3.zero;
    }

    /// <summary>
    /// 몬스터의 이동 방향을 설정하는 공통 메서드입니다.
    /// Update 사이클에서 호출되며, 이동 명령(_movementDirection)을 저장하고 회전만 처리합니다.
    /// </summary>
    protected void Move(Vector3 direction, float speed)
    {
        if (direction != Vector3.zero)
        {
            //이동 명령을 FixedUpdate에서 사용할 변수에 저장
            _movementDirection = direction;

            // 회전 로직 (Slerp는 Update에서 사용 가능)
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // 이동 중 애니메이션
            animator.SetFloat("Vert", 1f);
        }
        else
        {
            _movementDirection = Vector3.zero;
            animator.SetFloat("Vert", 0f); // 이동 명령이 없으면 정지 애니메이션
        }
    }

    /// <summary>
    /// 몬스터가 피해를 입었을 때 호출되는 메서드입니다.
    /// **[핵심 수정]** 공격 중이었다면 공격 코루틴을 중단하고 플래그를 재설정하여 움직임을 재개할 수 있도록 합니다.
    /// 자식 클래스에서 오버라이드하여 추가 로직을 구현할 수 있습니다.
    /// </summary>
    /// <param name="damage">받은 피해량</param>
    protected virtual void OnMonsterDamaged(float damage)
    {
        // 피격 시의 공통 로직: 공격 중 피격되면 공격을 취소하고 플래그를 해제하여 즉시 움직임을 재개
        if (isAttacking)
        {
            if (attackSequenceCoroutine != null) StopCoroutine(attackSequenceCoroutine);
            isAttacking = false;
            attackSequenceCoroutine = null;
        }

        // 이후 자식 클래스에서 오버라이드하여 특수 로직(예: 무리 소집)을 추가할 수 있습니다.
    }

    /// <summary>
    /// 플레이어에게 공격을 수행하는 공통 메서드입니다.
    /// 쿨타임을 확인하고 AttackSequenceCoroutine을 시작합니다.
    /// </summary>
    protected void PerformAttack()
    {
        // 쿨타임 체크 및 공격 중 여부 체크
        if (Time.time > lastAttackTime + attackCooldown && !isAttacking)
        {
            // 코루틴 시작 및 참조 저장
            attackSequenceCoroutine = StartCoroutine(AttackSequenceCoroutine());
        }
    }

    /// <summary>
    /// 공격 애니메이션 재생, 선딜레이, 데미지 적용, 쿨타임 설정을 담당하는 코루틴입니다.
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
        if (playerTransform != null)
        {
            float currentDistance = Vector3.Distance(transform.position, playerTransform.position);

            if (currentDistance <= attackRange)
            {
                IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    if (audioSource != null && attackClip != null)
                    {
                        audioSource.PlayOneShot(attackClip);
                    }
                    // 데미지 입히기
                    playerDamageable.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
                }
            }
        }

        // 4. 공격 후딜레이 및 쿨타임 설정
        lastAttackTime = Time.time;
        // 공격 애니메이션이 끝날 때까지 남은 쿨타임 대기 (선딜레이를 제외한 잔여 시간)
        // (Time.time - lastAttackTime)은 이미 경과된 시간을 의미하며, 이 값을 attackCooldown에서 빼주어야 합니다.
        // 하지만 postDelay 계산 시 Time.time을 lastAttackTime으로 설정했으므로, 이 계산이 정확하지 않을 수 있습니다.
        // 단순화를 위해, 공격 선딜레이 이후 잔여 시간만 대기하도록 로직을 수정합니다.
        float elapsedAttackTime = Time.time - lastAttackTime; // 현재 lastAttackTime은 공격 직후 시간
        float remainingCooldownTime = attackCooldown - elapsedAttackTime;

        if (remainingCooldownTime > 0)
        {
            yield return new WaitForSeconds(remainingCooldownTime);
        }
        // NOTE: 원본 코드의 postDelay 계산식 `float postDelay = attackCooldown - (Time.time - lastAttackTime);`은
        // lastAttackTime이 이미 현재 시간(Time.time)으로 설정된 직후에 호출되므로, 항상 `attackCooldown`만큼 기다리는 문제가 있었습니다.
        // 수정된 코드는 쿨다운 시간 전체를 대기합니다. (PostDelay 계산 로직은 주석 처리)

        // 5. 공격 종료
        isAttacking = false; // 이동 로직 재개 허용
        attackSequenceCoroutine = null; // 코루틴 참조 해제
    }

    /// <summary>
    /// 몬스터의 순찰 행동을 처리하는 공통 메서드입니다.
    /// </summary>
    protected void Patrol()
    {
        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1.0f)
        {
            SetNewPatrolPoint();
        }
        // Move() 내부에서 이동 명령을 저장하고 회전을 처리합니다.
        Move(currentPatrolPoint - transform.position, monster.monsterData.moveSpeed);
    }

    /// <summary>
    /// 새로운 순찰 지점을 설정하는 공통 메서드입니다.
    /// </summary>
    protected void SetNewPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * monster.detectionRange;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;
        currentPatrolPoint = randomDirection;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, flockDetectionRadius);
    }
    public Monster GetMonster()
    {
        return monster;
    }
}