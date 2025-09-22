using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ����� ������ ������ �ൿ ����(����, ����, ����)�� ����ϴ� Ŭ�����Դϴ�.
/// MonsterBase�� ���¸� �����ϸ� Ư���� �ൿ�� �����մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
public class BoarBehavior : MonoBehaviour
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private Transform playerTransform;

    [Header("���� �� ���� ����")]
    [Tooltip("�÷��̾�� �� �Ÿ����� �ָ� ������ ���� �� ������ �����մϴ�.")]
    public float chargeDistance = 10f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�.")]
    public float chargeSpeed = 15f;
    [Tooltip("�Ϲ� ������ ������ �Ÿ��Դϴ�. ���� �� �� �Ÿ��� ������ �Ϲ� ������ �����մϴ�.")]
    public float attackRange = 2f;
    [Tooltip("�Ϲ� ���� ��Ÿ���Դϴ�.")]
    public float attackCooldown = 1.5f;
    [Tooltip("���� �� ���� �Ҹ�Ǵ� ���� ���Դϴ�.")]
    public float manaCostPerCharge = 20f;
    [Tooltip("�ʴ� ȸ���Ǵ� ���� ���Դϴ�.")]
    public float manaRegenRate = 5f;

    [Header("���� �ൿ ����")]
    [Tooltip("������ �߽��� �Ǵ� �����Դϴ�. ������ �ʱ� ��ġ�� ����ϰų� ���� ������Ʈ�� ������ �� �ֽ��ϴ�.")]
    public Transform homePoint;
    [Tooltip("�߽� ������ �������� ������ �ݰ��Դϴ�.")]
    public float patrolRadius = 10f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�.")]
    public float patrolSpeed = 3f;

    private float currentMana;
    private float lastAttackTime;
    private Vector3 currentPatrolPoint;
    private Vector3 chargeDestination;
    private bool hasInitiatedCharge = false;
    private bool hasDealtChargeDamage = false; // �߰�: ���� �� �������� �������� ����

    void Awake()
    {
        monster = GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError("BoarBehavior: Monster ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        monsterCombat = GetComponent<MonsterCombat>();
        if (monsterCombat == null)
        {
            Debug.LogError("BoarBehavior: MonsterCombat ������Ʈ�� ã�� �� �����ϴ�.");
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

    // ���� �� �÷��̾�� �浹�ϸ� �������� ������ �ݹ� �Լ�
    private void OnCollisionEnter(Collision other)
    {
        // ���� �����̰�, ���� �������� ���� �ʾ�����, �浹 ����� �÷��̾� �±׸� ������ ���� ���� �ߵ�
        if (monster.currentState == MonsterBase.MonsterState.Charge && !hasDealtChargeDamage && other.gameObject.CompareTag("Player"))
        {
            IDamageable playerDamageable = other.gameObject.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                float chargeDamage = monster.monsterData.attackPower * 1.5f;
                playerDamageable.TakeDamage(chargeDamage);
                hasDealtChargeDamage = true; // �������� �������Ƿ� true�� ����
                Debug.Log($"����� ���� ���� ����! �÷��̾�� {chargeDamage} ������!");
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
            hasDealtChargeDamage = false; // ���ο� ���� ���� �� �ʱ�ȭ
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
    /// ���� ������ �����մϴ�.
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
    /// ��ǥ ������ ���� �̵��ϰ� ȸ���ϴ� ���� ����
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
    /// ��ǥ Transform�� ���� �̵��ϰ� ȸ���ϴ� ���� ����
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
    /// ���� ���� ���� ���ο� ���� ������ �����մϴ�.
    /// </summary>
    private void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint.position;
        randomDirection.y = transform.position.y;
        currentPatrolPoint = randomDirection;
    }

    /// <summary>
    /// �÷��̾�� �������� ������ �Ϲ� ���� ������ �����մϴ�.
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
                Debug.Log($"������� �÷��̾�� {monster.monsterData.attackPower}�� �������� �������ϴ�!");
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