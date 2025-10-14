using UnityEngine;
using System.Collections;
using System.Diagnostics; // Debug.Log 사용을 위해 필요할 수 있습니다.

/// <summary>
/// 사슴 몬스터의 고유한 행동 로직(도망치기, 공격)을 담당하는 클래스입니다.
/// MonsterPatrol 컴포넌트를 제어하고, 데미지 이벤트에 반응하여 행동을 바꿉니다.
/// SOLID 원칙: SRP(단일 책임 원칙)에 따라 사슴의 고유 행동 로직(Flee, Attack 결정)만 담당합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
[RequireComponent(typeof(MonsterPatrol))]
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
    [Tooltip("플레이어 감지 시 도망치기 시작할 거리입니다.")]
    public float fleeDistance = 15f;
    [Tooltip("플레이어에게서 충분히 멀어져서 멈출 거리입니다.")]
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

    // === 내부 변수 ===
    private bool hasTakenDamage = false; // 데미지를 입었는지 여부 (복수심 트리거)
    private float lastAttackTime;         // 마지막 공격 시간
    private bool isAttacking = false;    // 현재 공격 애니메이션 재생 중 여부 (이동/회전 제어용 플래그)

    // 기본 순찰 속도 (애니메이션 동기화를 위해 Monster Data에서 가져오는 것이 이상적)
    // 현재 스크립트 구조상, 걷기 속도에 해당하는 몬스터의 기본 이동 속도를 가정합니다.
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
        // 기본 걷기 모션 설정: Vert=1 (움직임), State=0 (걷기)
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
        StopAllCoroutines(); // 혹시 모를 코루틴 실행 중단
    }

    /// <summary>
    /// MonsterCombat에서 데미지 이벤트가 발생했을 때 호출됩니다.
    /// 사슴의 행동 컨셉: 데미지를 입으면 도망치지 않고 끝까지 추격/공격합니다.
    /// </summary>
    /// <param name="damage">입은 데미지 양</param>
    private void OnMonsterDamaged(float damage)
    {
        hasTakenDamage = true; // 복수심 발동!
        // 데미지를 입으면 즉시 공격(추격) 상태로 전환합니다.
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        StopAllCoroutines(); // 이전 공격 시퀀스가 남아있다면 중단
        isAttacking = false; // 공격 플래그 초기화 (바로 추격 시작)
    }

    void Update()
    {
        // === 몬스터 상태 확인 및 예외 처리 (Dead, Game Over) ===
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            monsterPatrol.StopPatrol();
            // 죽었을 때는 Idle 모션 (Vert=0, State=0) 또는 Death 애니메이션을 설정해야 합니다.
            // 여기서는 일단 이동 애니메이션만 멈춥니다.
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

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // === 상태 머신 실행 ===
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                HandlePatrolState(distanceToPlayer);
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
    /// 순찰 상태 로직을 처리합니다.
    /// </summary>
    private void HandlePatrolState(float distanceToPlayer)
    {
        // 걷기 모션 설정 (Vert=1, State=0)
        animator.SetFloat("Vert", 1f);
        animator.SetFloat("State", 0f);

        // 데미지를 입지 않았고, 플레이어를 감지하면 도망 상태로 전환
        if (!hasTakenDamage && distanceToPlayer < fleeDistance)
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
        // 데미지를 입은 후에는 Flee 상태에 머물지 않습니다.
        if (hasTakenDamage)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            monsterPatrol.StopPatrol();
            return;
        }

        monsterPatrol.StopPatrol();
        FleeFromPlayer(distanceToPlayer);

        // 도망치는 상태는 뛰는 모션 (Vert=1, State=1)을 사용합니다.
        animator.SetFloat("Vert", 1f);
        animator.SetFloat("State", 1f);
    }

    /// <summary>
    /// 추격 및 공격 상태 로직을 처리합니다.
    /// </summary>
    private void HandleAttackState(float distanceToPlayer)
    {
        monsterPatrol.StopPatrol();
        AttackPlayer(distanceToPlayer);
    }

    /// <summary>
    /// 플레이어에게서 도망치는 로직을 실행합니다. (데미지를 입기 전의 기본 행동)
    /// </summary>
    private void FleeFromPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer < stopFleeDistance)
        {
            // 도망 로직: 플레이어 반대 방향으로 이동
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            transform.Translate(fleeDirection * basePatrolSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);

            // 이동 방향으로 부드럽게 회전
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(fleeDirection.x, 0, fleeDirection.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            // 충분히 멀리 도망쳤으면 다시 Patrol 상태로 돌아갑니다.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 플레이어를 추격 및 공격하는 로직을 실행합니다. (데미지를 입은 후의 복수 행동)
    /// </summary>
    private void AttackPlayer(float distanceToPlayer)
    {
        // isAttacking 플래그 확인: 공격 애니메이션 재생 중이면 이동/회전 로직 건너뛰고 멈춰있습니다. (우왕좌왕 문제 해결)
        if (isAttacking)
        {
            // 공격 애니메이션 재생 중에는 멈춰서 Idle 모션 (Vert=0, State=0)을 강제하여 충돌 방지
            animator.SetFloat("Vert", 0f);
            animator.SetFloat("State", 0f);
            return;
        }

        // isAttacking이 false일 때 (추격 또는 공격 준비)
        if (distanceToPlayer > attackDistance)
        {
            // === 추격 로직 (뛰는 모션 적용) ===
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attackSpeed * Time.deltaTime;

            // 추격 방향으로 회전
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // '뛰는' 애니메이션 설정 (Vert=1, State=1)
            animator.SetFloat("Vert", 1f);
            animator.SetFloat("State", 1f);
        }
        else
        {
            // === 공격 준비 및 실행 ===
            // 공격 범위 안이면 플레이어를 바라보고 공격 실행
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f); // 빠르게 바라보기

            PerformAttack();
            // isAttacking 플래그가 true가 되면서 다음 Update부터는 이동/회전이 중단됩니다.
        }
    }

    /// <summary>
    /// 공격 쿨타임을 확인하고 공격 시퀀스 코루틴을 시작합니다.
    /// </summary>
    private void PerformAttack()
    {
        // 쿨타임이 지났는지 확인
        if (Time.time > lastAttackTime + attackCooldown)
        {
            // 공격 시퀀스 코루틴 시작
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

    private void OnDrawGizmosSelected()
    {
        // 기즈모 시각화 로직
    }
}