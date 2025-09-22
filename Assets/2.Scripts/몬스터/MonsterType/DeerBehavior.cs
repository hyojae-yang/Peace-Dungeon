using UnityEngine;
using System.Collections;

/// <summary>
/// 사슴 몬스터의 고유한 행동 로직(도망치기, 공격)을 담당하는 클래스입니다.
/// MonsterBase의 상태를 관찰하며 특별한 행동을 실행합니다.
/// 데미지 이벤트에 반응하여 행동을 바꿉니다.
/// </summary>
[RequireComponent(typeof(Monster))]
public class DeerBehavior : MonoBehaviour // <-- IDamageable 인터페이스 제거
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private Transform playerTransform;

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

    [Header("순찰 행동 설정")]
    [Tooltip("순찰의 중심이 되는 지점입니다. 몬스터의 초기 위치를 사용하거나 별도 오브젝트를 지정할 수 있습니다.")]
    public Transform homePoint;
    [Tooltip("중심 지점을 기준으로 순찰할 반경입니다.")]
    public float patrolRadius = 10f;
    [Tooltip("순찰 시 이동 속도입니다.")]
    public float patrolSpeed = 3f;

    private bool hasTakenDamage = false;
    private float lastAttackTime;
    private Vector3 currentPatrolPoint;

    void Awake()
    {
        monster = GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError("DeerBehavior: Monster 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        monsterCombat = GetComponent<MonsterCombat>();
        if (monsterCombat == null)
        {
            Debug.LogError("DeerBehavior: MonsterCombat 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        if (homePoint == null)
        {
            homePoint = this.transform;
        }

        SetNewPatrolPoint();
    }

    /// <summary>
    /// 스크립트가 활성화될 때 이벤트를 구독합니다.
    /// </summary>
    private void OnEnable()
    {
        // monsterCombat이 null이 아닐 때만 이벤트를 구독합니다.
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    /// <summary>
    /// 스크립트가 비활성화될 때 이벤트를 구독 해제합니다.
    /// 메모리 누수를 방지하기 위해 반드시 필요합니다.
    /// </summary>
    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    /// <summary>
    /// MonsterCombat에서 데미지 이벤트가 발생했을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="damage">입은 데미지 양 (현재는 사용되지 않음)</param>
    private void OnMonsterDamaged(float damage)
    {
        // 데미지를 입었음을 알리고 공격 상태로 전환합니다.
        hasTakenDamage = true;
        monster.ChangeState(MonsterBase.MonsterState.Attack);
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                if (distanceToPlayer < fleeDistance)
                {
                    monster.ChangeState(MonsterBase.MonsterState.Flee);
                }
                else
                {
                    Patrol();
                }
                break;

            case MonsterBase.MonsterState.Flee:
                // 데미지를 받지 않았다면 계속 도망칩니다.
                if (!hasTakenDamage)
                {
                    FleeFromPlayer();
                }
                // 데미지를 받았다면 MonsterCombat의 이벤트 핸들러에 의해 이미 상태가 전환되었을 것입니다.
                break;

            case MonsterBase.MonsterState.Attack:
                AttackPlayer(distanceToPlayer);
                break;
        }
    }

    /// <summary>
    /// 플레이어에게서 도망치는 로직을 실행합니다.
    /// </summary>
    private void FleeFromPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < stopFleeDistance)
        {
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            transform.Translate(fleeDirection * monster.monsterData.moveSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);
            Quaternion lookRotation = Quaternion.LookRotation(fleeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            monster.ChangeState(MonsterBase.MonsterState.Idle);
        }
    }

    /// <summary>
    /// 플레이어를 공격하는 로직을 실행합니다.
    /// </summary>
    /// <param name="distanceToPlayer">플레이어와의 현재 거리</param>
    private void AttackPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > attackDistance)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attackSpeed * Time.deltaTime;

            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            PerformAttack();
        }
    }

    /// <summary>
    /// 순찰 로직을 실행합니다.
    /// </summary>
    private void Patrol()
    {
        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolPoint);

        if (distanceToTarget < 1.0f)
        {
            SetNewPatrolPoint();
        }

        transform.position = Vector3.MoveTowards(transform.position, currentPatrolPoint, patrolSpeed * Time.deltaTime);

        Vector3 direction = (currentPatrolPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 순찰 범위 내에 새로운 랜덤 지점을 설정합니다.
    /// </summary>
    private void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint.position;
        randomDirection.y = transform.position.y;
        currentPatrolPoint = randomDirection;
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
        if (homePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(homePoint.position, patrolRadius);
        }
    }
}