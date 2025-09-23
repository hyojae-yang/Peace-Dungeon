using UnityEngine;
using System.Collections;

/// <summary>
/// �罿 ������ ������ �ൿ ����(����ġ��, ����)�� ����ϴ� Ŭ�����Դϴ�.
/// MonsterPatrol ������Ʈ�� �����ϰ�, ������ �̺�Ʈ�� �����Ͽ� �ൿ�� �ٲߴϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))] // MonsterPatrol ������Ʈ�� �ʿ����� ���
public class DeerBehavior : MonoBehaviour
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol; // MonsterPatrol ������Ʈ ���� �߰�
    private Transform playerTransform;

    // === �罿 �ൿ ���� ===
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

    // === ���� ���� ===
    private bool hasTakenDamage = false;
    private float lastAttackTime;

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>(); // MonsterPatrol ����
        if (monster == null || monsterCombat == null || monsterPatrol == null)
        {
            Debug.LogError("DeerBehavior: �ʼ� ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // �罿 ���ʹ� �⺻������ ���� ���·� �����մϴ�.
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
    }

    /// <summary>
    /// ��ũ��Ʈ�� Ȱ��ȭ�� �� ������ �̺�Ʈ�� �����մϴ�.
    /// </summary>
    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    /// <summary>
    /// ��ũ��Ʈ�� ��Ȱ��ȭ�� �� ������ �̺�Ʈ�� ���� �����մϴ�.
    /// </summary>
    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    /// <summary>
    /// MonsterCombat���� ������ �̺�Ʈ�� �߻����� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnMonsterDamaged(float damage)
    {
        hasTakenDamage = true;
        // �������� ������ ��� ���� ���·� ��ȯ�մϴ�.
        monster.ChangeState(MonsterBase.MonsterState.Attack);
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            // ���Ͱ� ������ ������ ����ϴ�.
            monsterPatrol.StopPatrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // Patrol ������ �� ���� ����
                monsterPatrol.StartPatrol();

                // �÷��̾ �����ϸ� ���� ���·� ��ȯ
                if (distanceToPlayer < fleeDistance)
                {
                    monster.ChangeState(MonsterBase.MonsterState.Flee);
                    monsterPatrol.StopPatrol();
                }
                break;

            case MonsterBase.MonsterState.Flee:
                // ������ ���߰� ���� ���� ����
                monsterPatrol.StopPatrol();
                if (!hasTakenDamage)
                {
                    FleeFromPlayer(distanceToPlayer);
                }
                break;

            case MonsterBase.MonsterState.Attack:
                // ������ ���߰� ���� ���� ����
                monsterPatrol.StopPatrol();
                AttackPlayer(distanceToPlayer);
                break;
        }
    }

    /// <summary>
    /// �÷��̾�Լ� ����ġ�� ������ �����մϴ�.
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
            // ����� �ָ� ���������� �ٽ� Patrol ���·� ���ư��ϴ�.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// �÷��̾ �����ϴ� ������ �����մϴ�.
    /// </summary>
    private void AttackPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > attackDistance)
        {
            // ���� ���� ���̸� �߰�
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attackSpeed * Time.deltaTime;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            // ���� ���� ���̸� ���� ����
            PerformAttack();
        }
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
        // ������ ���� ���� ������ Monster ��ũ��Ʈ���� �׸��ϴ�.
        // Flee �� Attack �Ÿ��� ������ �ð�ȭ���� �ʽ��ϴ�.
    }
}