using UnityEngine;
using System.Collections;

/// <summary>
/// �罿 ������ ������ �ൿ ����(����ġ��, ����)�� ����ϴ� Ŭ�����Դϴ�.
/// MonsterBase�� ���¸� �����ϸ� Ư���� �ൿ�� �����մϴ�.
/// ������ �̺�Ʈ�� �����Ͽ� �ൿ�� �ٲߴϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
public class DeerBehavior : MonoBehaviour // <-- IDamageable �������̽� ����
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private Transform playerTransform;

    [Header("�罿 �ൿ ����")]
    [Tooltip("�÷��̾� ���� �� ����ġ�� ������ �Ÿ��Դϴ�.")]
    public float fleeDistance = 15f;
    [Tooltip("�÷��̾�Լ� ����� �־����� ���� �Ÿ��Դϴ�.")]
    public float stopFleeDistance = 20f;
    [Tooltip("�÷��̾�� �������� �Ծ��� �� ������ ������ �Ÿ��Դϴ�.")]
    public float attackDistance = 3f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�.")]
    public float attackSpeed = 6f;
    [Tooltip("���� ��Ÿ���Դϴ�.")]
    public float attackCooldown = 1.5f;
    [Tooltip("����ġ�� ���������� �̵� �ӵ� �����Դϴ�.")]
    public float fleeSpeedMultiplier = 1.5f;

    [Header("���� �ൿ ����")]
    [Tooltip("������ �߽��� �Ǵ� �����Դϴ�. ������ �ʱ� ��ġ�� ����ϰų� ���� ������Ʈ�� ������ �� �ֽ��ϴ�.")]
    public Transform homePoint;
    [Tooltip("�߽� ������ �������� ������ �ݰ��Դϴ�.")]
    public float patrolRadius = 10f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�.")]
    public float patrolSpeed = 3f;

    private bool hasTakenDamage = false;
    private float lastAttackTime;
    private Vector3 currentPatrolPoint;

    void Awake()
    {
        monster = GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError("DeerBehavior: Monster ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        monsterCombat = GetComponent<MonsterCombat>();
        if (monsterCombat == null)
        {
            Debug.LogError("DeerBehavior: MonsterCombat ������Ʈ�� ã�� �� �����ϴ�.");
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
    /// ��ũ��Ʈ�� Ȱ��ȭ�� �� �̺�Ʈ�� �����մϴ�.
    /// </summary>
    private void OnEnable()
    {
        // monsterCombat�� null�� �ƴ� ���� �̺�Ʈ�� �����մϴ�.
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    /// <summary>
    /// ��ũ��Ʈ�� ��Ȱ��ȭ�� �� �̺�Ʈ�� ���� �����մϴ�.
    /// �޸� ������ �����ϱ� ���� �ݵ�� �ʿ��մϴ�.
    /// </summary>
    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    /// <summary>
    /// MonsterCombat���� ������ �̺�Ʈ�� �߻����� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="damage">���� ������ �� (����� ������ ����)</param>
    private void OnMonsterDamaged(float damage)
    {
        // �������� �Ծ����� �˸��� ���� ���·� ��ȯ�մϴ�.
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
                // �������� ���� �ʾҴٸ� ��� ����Ĩ�ϴ�.
                if (!hasTakenDamage)
                {
                    FleeFromPlayer();
                }
                // �������� �޾Ҵٸ� MonsterCombat�� �̺�Ʈ �ڵ鷯�� ���� �̹� ���°� ��ȯ�Ǿ��� ���Դϴ�.
                break;

            case MonsterBase.MonsterState.Attack:
                AttackPlayer(distanceToPlayer);
                break;
        }
    }

    /// <summary>
    /// �÷��̾�Լ� ����ġ�� ������ �����մϴ�.
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
    /// �÷��̾ �����ϴ� ������ �����մϴ�.
    /// </summary>
    /// <param name="distanceToPlayer">�÷��̾���� ���� �Ÿ�</param>
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
    /// ���� ������ �����մϴ�.
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
    /// �÷��̾�� �������� ������ ������ �����մϴ�.
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
                Debug.Log($"�罿�� �÷��̾�� {monster.monsterData.attackPower}�� �������� �������ϴ�!");
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