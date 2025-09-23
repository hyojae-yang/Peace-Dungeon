using UnityEngine;
using System.Collections;

/// <summary>
/// 사슴 몬스터의 고유한 행동 로직(도망치기, 공격)을 담당하는 클래스입니다.
/// MonsterPatrol 컴포넌트를 제어하고, 데미지 이벤트에 반응하여 행동을 바꿉니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))] // MonsterPatrol 컴포넌트가 필요함을 명시
public class DeerBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조 추가
    private Transform playerTransform;

    // === 사슴 행동 설정 ===
    [Header("사슴 행동 설정")]
    [Tooltip("플레이어 감지 시 도망치기 시작할 거리입니다.")]
    public float fleeDistance = 15f;
    [Tooltip("플레이어에게서 충분히 멀어져서 멈출 거리입니다.")]
    public float stopFleeDistance = 20f;
    [Tooltip("플레이어에게 데미지를 입었을 때 공격을 시작할 거리입니다.")]
    public float attackDistance = 3f;
    [Tooltip("공격 시 이동 속도입니다.")]
    public float attackSpeed = 6f;
    [Tooltip("공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;
    [Tooltip("도망치는 방향으로의 이동 속도 배율입니다.")]
    public float fleeSpeedMultiplier = 1.5f;

    // === 내부 변수 ===
    private bool hasTakenDamage = false;
    private float lastAttackTime;

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>(); // MonsterPatrol 참조
        if (monster == null || monsterCombat == null || monsterPatrol == null)
        {
            Debug.LogError("DeerBehavior: 필수 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // 사슴 몬스터는 기본적으로 순찰 상태로 시작합니다.
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
    }

    /// <summary>
    /// 스크립트가 활성화될 때 데미지 이벤트를 구독합니다.
    /// </summary>
    private void OnEnable()
    {
        if (monsterCombat != null)
        {
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
    }

    /// <summary>
    /// MonsterCombat에서 데미지 이벤트가 발생했을 때 호출됩니다.
    /// </summary>
    private void OnMonsterDamaged(float damage)
    {
        hasTakenDamage = true;
        // 데미지를 입으면 즉시 공격 상태로 전환합니다.
        monster.ChangeState(MonsterBase.MonsterState.Attack);
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            // 몬스터가 죽으면 순찰도 멈춥니다.
            monsterPatrol.StopPatrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // Patrol 상태일 때 순찰 시작
                monsterPatrol.StartPatrol();

                // 플레이어를 감지하면 도망 상태로 전환
                if (distanceToPlayer < fleeDistance)
                {
                    monster.ChangeState(MonsterBase.MonsterState.Flee);
                    monsterPatrol.StopPatrol();
                }
                break;

            case MonsterBase.MonsterState.Flee:
                // 순찰을 멈추고 도망 로직 실행
                monsterPatrol.StopPatrol();
                if (!hasTakenDamage)
                {
                    FleeFromPlayer(distanceToPlayer);
                }
                break;

            case MonsterBase.MonsterState.Attack:
                // 순찰을 멈추고 공격 로직 실행
                monsterPatrol.StopPatrol();
                AttackPlayer(distanceToPlayer);
                break;
        }
    }

    /// <summary>
    /// 플레이어에게서 도망치는 로직을 실행합니다.
    /// </summary>
    private void FleeFromPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer < stopFleeDistance)
        {
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            transform.Translate(fleeDirection * monster.monsterData.moveSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);
            Quaternion lookRotation = Quaternion.LookRotation(fleeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            // 충분히 멀리 도망쳤으면 다시 Patrol 상태로 돌아갑니다.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 플레이어를 공격하는 로직을 실행합니다.
    /// </summary>
    private void AttackPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > attackDistance)
        {
            // 공격 범위 밖이면 추격
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attackSpeed * Time.deltaTime;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            // 공격 범위 안이면 공격 실행
            PerformAttack();
        }
    }

    /// <summary>
    /// 플레이어에게 데미지를 입히는 로직을 실행합니다.
    /// </summary>
    private void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(monster.monsterData.attackPower);
                lastAttackTime = Time.time;
                Debug.Log($"사슴이 플레이어에게 {monster.monsterData.attackPower}의 데미지를 입혔습니다!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 몬스터의 감지 범위 기즈모는 Monster 스크립트에서 그립니다.
        // Flee 및 Attack 거리는 기즈모로 시각화하지 않습니다.
    }
}