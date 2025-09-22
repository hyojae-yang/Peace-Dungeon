using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 몬스터의 고유한 행동 로직(순찰, 돌진, 공격)을 담당하는 클래스입니다.
/// MonsterBase의 상태를 관찰하며 특별한 행동을 실행합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
public class BoarBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private Transform playerTransform;

    [Header("돌진 및 공격 설정")]
    [Tooltip("플레이어와 이 거리보다 멀리 떨어져 있을 때 돌진을 시작합니다.")]
    public float chargeDistance = 10f;
    [Tooltip("돌진 시 이동 속도입니다.")]
    public float chargeSpeed = 15f;
    [Tooltip("일반 공격을 시작할 거리입니다. 돌진 후 이 거리에 들어오면 일반 공격을 시작합니다.")]
    public float attackRange = 2f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;
    [Tooltip("돌진 한 번에 소모되는 마나 양입니다.")]
    public float manaCostPerCharge = 20f;
    [Tooltip("초당 회복되는 마나 양입니다.")]
    public float manaRegenRate = 5f;

    [Header("순찰 행동 설정")]
    [Tooltip("순찰의 중심이 되는 지점입니다. 몬스터의 초기 위치를 사용하거나 별도 오브젝트를 지정할 수 있습니다.")]
    public Transform homePoint;
    [Tooltip("중심 지점을 기준으로 순찰할 반경입니다.")]
    public float patrolRadius = 10f;
    [Tooltip("순찰 시 이동 속도입니다.")]
    public float patrolSpeed = 3f;

    private float currentMana;
    private float lastAttackTime;
    private Vector3 currentPatrolPoint;
    private Vector3 chargeDestination;
    private bool hasInitiatedCharge = false;
    private bool hasDealtChargeDamage = false; // 추가: 돌진 중 데미지를 입혔는지 여부

    void Awake()
    {
        monster = GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError("BoarBehavior: Monster 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        monsterCombat = GetComponent<MonsterCombat>();
        if (monsterCombat == null)
        {
            Debug.LogError("BoarBehavior: MonsterCombat 컴포넌트를 찾을 수 없습니다.");
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
    }

    void Start()
    {
        currentMana = monster.monsterData.maxMana;
        SetNewPatrolPoint();
    }

    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    private void OnMonsterDamaged(float damage)
    {
        monster.ChangeState(MonsterBase.MonsterState.Attack);
    }

    // 돌진 중 플레이어와 충돌하면 데미지를 입히는 콜백 함수
    private void OnCollisionEnter(Collision other)
    {
        // 돌진 상태이고, 아직 데미지를 주지 않았으며, 충돌 대상이 플레이어 태그를 가지고 있을 때만 발동
        if (monster.currentState == MonsterBase.MonsterState.Charge && !hasDealtChargeDamage && other.gameObject.CompareTag("Player"))
        {
            IDamageable playerDamageable = other.gameObject.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                float chargeDamage = monster.monsterData.attackPower * 1.5f;
                playerDamageable.TakeDamage(chargeDamage);
                hasDealtChargeDamage = true; // 데미지를 입혔으므로 true로 설정
                Debug.Log($"멧돼지 돌진 공격 성공! 플레이어에게 {chargeDamage} 데미지!");
            }
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (currentMana < monster.monsterData.maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                if (distanceToPlayer < monster.detectionRange)
                {
                    if (currentMana >= manaCostPerCharge)
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Charge);
                    }
                    else
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                }
                else
                {
                    Patrol();
                }
                break;

            case MonsterBase.MonsterState.Charge:
                HandleCharge(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                HandleAttack(distanceToPlayer);
                break;
        }
    }

    private void HandleCharge(float distanceToPlayer)
    {
        if (!hasInitiatedCharge)
        {
            chargeDestination = transform.position + (playerTransform.position - transform.position).normalized * chargeDistance;
            currentMana -= manaCostPerCharge;
            hasInitiatedCharge = true;
            hasDealtChargeDamage = false; // 새로운 돌진 시작 시 초기화
        }

        transform.position = Vector3.MoveTowards(transform.position, chargeDestination, chargeSpeed * Time.deltaTime);

        Vector3 direction = (chargeDestination - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (Vector3.Distance(transform.position, chargeDestination) < 0.5f || distanceToPlayer <= attackRange)
        {
            hasInitiatedCharge = false;
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    private void HandleAttack(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            if (currentMana >= manaCostPerCharge)
            {
                monster.ChangeState(MonsterBase.MonsterState.Charge);
            }
            else
            {
                MoveTowardsTarget(playerTransform, monster.monsterData.moveSpeed);
            }
        }
        else
        {
            PerformAttack();
        }

        if (distanceToPlayer > monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Idle);
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

        MoveTowardsTarget(currentPatrolPoint, patrolSpeed);
    }

    /// <summary>
    /// 목표 지점을 향해 이동하고 회전하는 공통 로직
    /// </summary>
    private void MoveTowardsTarget(Vector3 targetPoint, float speed)
    {
        Vector3 direction = (targetPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 목표 Transform을 향해 이동하고 회전하는 공통 로직
    /// </summary>
    private void MoveTowardsTarget(Transform targetTransform, float speed)
    {
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            transform.position += direction * speed * Time.deltaTime;
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
    /// 플레이어에게 데미지를 입히는 일반 공격 로직을 실행합니다.
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
                Debug.Log($"멧돼지가 플레이어에게 {monster.monsterData.attackPower}의 데미지를 입혔습니다!");
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