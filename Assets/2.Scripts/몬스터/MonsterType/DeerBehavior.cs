using UnityEngine;
using System.Collections;
using System.Diagnostics;

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

    // === 사슴 행동 설정 ===
    [Header("사슴 행동 설정")]
    // [제거] 기존의 fleeDistance는 Monster.detectionRange가 대신합니다.
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
    [Tooltip("공격 애니메이션이 실제로 재생되는 시간입니다. (애니메이터 클립 길이에 맞춰야 함)")]
    public float attackAnimationDuration = 0.8f; // 공격 중 이동 중지를 위한 시간
    // [추가] 회전 속도 (도주 시 타겟 반대 방향으로 회전)
    [Tooltip("도주/추격 상태에서 플레이어를 향해 회전하는 속도입니다.")]
    [SerializeField] private float rotationSpeed = 10.0f;

    // === 내부 변수 ===
    private bool hasTakenDamage = false; // 데미지를 입었는지 여부 (복수심 트리거)
    private float lastAttackTime;        // 마지막 공격 시간
    private bool isAttacking = false;    // 현재 공격 애니메이션 재생 중 여부 (이동/회전 제어용 플래그)

    // 몬스터의 기본 걷기 속도 참조
    private float basePatrolSpeed = 3.0f;

    void Awake()
    {
        // === 필수 컴포넌트 참조 ===
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        animator = GetComponent<Animator>();

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

        // MonsterData에 moveSpeed가 있다면 기본 순찰 속도로 사용
        if (monster.monsterData != null)
        {
            basePatrolSpeed = monster.monsterData.moveSpeed;
        }
    }

    /// <summary>
    /// 스크립트가 활성화될 때 데미지 이벤트를 구독합니다.
    /// </summary>
    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            // OCP: MonsterCombat의 이벤트에 의존하여 유연하게 반응합니다.
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    /// <summary>
    /// 스크립트가 비활성화될 때 데미지 이벤트를 구독 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
        StopAllCoroutines();
    }

    /// <summary>
    /// MonsterCombat에서 데미지 이벤트가 발생했을 때 호출됩니다.
    /// 사슴의 행동 컨셉: 데미지를 입으면 도망치지 않고 끝까지 추격/공격합니다. (복수심 발동)
    /// </summary>
    private void OnMonsterDamaged(float damage)
    {
        hasTakenDamage = true; // 복수심 발동!
        // 데미지를 입으면 즉시 공격(추격) 상태로 전환합니다.
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        StopAllCoroutines();
        isAttacking = false; // 공격 플래그 초기화 (바로 추격 시작)
    }

    void Update()
    {
        // === 몬스터 상태 확인 및 예외 처리 (Dead, Game Over) ===
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
            return;
        }
        if (MainSceneManager.Instance != null && MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
            return;
        }

        // 플레이어와의 거리는 이동/정지 조건에만 사용
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // === 상태 머신 실행 ===
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // ⭐ [수정] distanceToPlayer 인자 제거 및 로직 변경
                HandlePatrolState();
                break;

            case MonsterBase.MonsterState.Flee:
                HandleFleeState(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }
    }

    // === 상태별 전용 핸들러 메서드 (SRP 적용 및 코드 정리) ===

    /// <summary>
    /// 순찰 상태 로직을 처리합니다. 플레이어 감지 시 Flee 상태로 전환합니다.
    /// </summary>
    private void HandlePatrolState()
    {
        // 걷기 모션 설정 (Vert=1, State=0)
        animator.SetFloat("Vert", 1f);
        animator.SetFloat("State", 0f);

        // ⭐ [핵심 수정] Monster.cs에서 시야각 감지를 했는지 확인합니다.
        // 데미지를 입지 않았고, Monster가 플레이어를 감지했다면 도망 상태로 전환
        if (!hasTakenDamage && monster.detectableTarget != null)
        {
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
            return;
        }

        // 2. 플레이어 감지 해제 또는 충분한 거리를 확보했는지 확인
        // 플레이어를 놓치고(detectableTarget == null) 충분한 거리를 확보했으면 Patrol로 복귀
        if (monster.detectableTarget == null && distanceToPlayer > stopFleeDistance)
        {
            // 충분히 멀리 도망쳤으면 다시 Patrol 상태로 돌아갑니다.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
            return;
        }

        // 3. 도주 실행: 도주 목표 거리에 도달하지 않았거나(distanceToPlayer < stopFleeDistance) 
        // 아직 플레이어가 시야 내에 있으면 계속 도망칩니다.
        if (distanceToPlayer < stopFleeDistance || monster.detectableTarget != null)
        {
            monsterPatrol.StopPatrol();

            // ⭐ [변경] MoveAwayFromTarget 및 RotateTowardsTarget 헬퍼 메서드 사용
            MoveAwayFromTarget(playerTransform, basePatrolSpeed * fleeSpeedMultiplier, stopFleeDistance);
            RotateAwayFromTarget(playerTransform);

            // 도망치는 상태는 뛰는 모션 (Vert=1, State=1)을 사용합니다.
            animator.SetFloat("Vert", 1f);
            animator.SetFloat("State", 1f);
        }
        else
        {
            // 목표 거리에 도달했고 플레이어를 놓쳤다면 정지 후 Patrol로 복귀 (위의 조건에서 이미 처리됨)
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
        }
    }

    /// <summary>
    /// 플레이어를 추격 및 공격하는 로직을 실행합니다. (데미지를 입은 후의 복수 행동)
    /// </summary>
    private void HandleAttackState(float distanceToPlayer)
    {
        monsterPatrol.StopPatrol();
        AttackPlayer(distanceToPlayer);
    }

    /// <summary>
    /// 플레이어를 추격 및 공격하는 로직을 실행합니다. (데미지를 입은 후의 복수 행동)
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

        // isAttacking이 false일 때 (추격 또는 공격 준비)
        if (distanceToPlayer > attackDistance)
        {
            // === 추격 로직 (뛰는 모션 적용) ===
            // [변경] 헬퍼 메서드 사용
            MoveTowardsTarget(playerTransform, attackSpeed, attackDistance - 0.1f);
            RotateTowardsTarget(playerTransform);

            // '뛰는' 애니메이션 설정 (Vert=1, State=1)
            animator.SetFloat("Vert", 1f);
            animator.SetFloat("State", 1f);
        }
        else
        {
            // === 공격 준비 및 실행 ===
            // 공격 범위 안이면 플레이어를 바라보고 공격 실행
            RotateTowardsTarget(playerTransform, 10f); // 빠르게 바라보기

            PerformAttack();
            // isAttacking 플래그가 true가 되면서 다음 Update()부터는 이동/회전이 중단됩니다.
        }
    }

    /// <summary>
    /// 공격 쿨타임을 확인하고 공격 시퀀스 코루틴을 시작합니다.
    /// </summary>
    private void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackSequenceCoroutine());
        }
    }

    /// <summary>
    /// 공격 애니메이션 재생, 데미지 적용, 쿨타임 설정을 담당하는 코루틴입니다.
    /// 이 코루틴이 실행되는 동안 isAttacking 플래그가 true로 유지되어 몬스터의 이동을 막습니다.
    /// </summary>
    private IEnumerator AttackSequenceCoroutine()
    {
        // 1. 공격 시작 - 이동 중지
        isAttacking = true;
        animator.SetTrigger("Attack"); // 공격 애니메이션 트리거

        // 공격 애니메이션이 끝날 때까지 멈춰있도록 대기
        yield return new WaitForSeconds(attackAnimationDuration);

        // 2. 실제 데미지 적용
        IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
        if (playerDamageable != null)
        {
            playerDamageable.TakeDamage(monster.monsterData.attackPower);
        }

        // 3. 공격 종료 및 쿨타임 설정
        lastAttackTime = Time.time;
        isAttacking = false; // 이동 로직 재개 허용 (다음 Update()부터 추격 시작)
    }
    /// <summary>
    /// 몬스터를 지정된 속도로 목표 지점(target)을 향해 이동시킵니다. (추격)
    /// </summary>
    private void MoveTowardsTarget(Transform target, float speed, float stoppingDistance)
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > stoppingDistance)
        {
            // 목표 방향 벡터 (XZ 평면만 고려)
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
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // stopFleeDistance보다 가까울 때만 도망칩니다.
        // 또는 아직 플레이어가 시야 내에 있는 경우 (HandleFleeState에서 제어)
        if (distanceToTarget < stoppingDistance || monster.detectableTarget != null)
        {
            // 도주 방향 벡터 = (몬스터 위치 - 타겟 위치) (XZ 평면만 고려)
            Vector3 direction = transform.position - target.position;
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

            // 이동
            transform.position += flatDirection.normalized * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 몬스터를 목표를 향해 부드럽게 회전시킵니다. (추격)
    /// </summary>
    private void RotateTowardsTarget(Transform target, float slerpSpeed = -1f)
    {
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
        // 도망 방향 = 타겟 반대 방향
        Vector3 direction = transform.position - target.position;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
