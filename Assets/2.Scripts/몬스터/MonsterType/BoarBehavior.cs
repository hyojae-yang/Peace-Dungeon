using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ����� ������ ������ �ൿ ����(����, ����, ����)�� ����ϴ� Ŭ�����Դϴ�.
/// MonsterBase�� ���¸� �����ϸ� Ư���� �ൿ�� �����մϴ�.
/// MonsterPatrol ������Ʈ�� �����Ͽ� ���� ����� �����մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))] // MonsterPatrol ������Ʈ�� �ʿ����� ���
public class BoarBehavior : MonoBehaviour
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol; // MonsterPatrol ������Ʈ ����
    private Transform playerTransform;

    // === ���� �� ���� ���� ===
    [Header("���� �� ���� ����")]
    [Tooltip("�÷��̾�� �� �Ÿ����� �ָ� ������ ���� �� ������ �����մϴ�.")]
    public float chargeDistance = 8f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�.")]
    public float chargeSpeed = 15f;
    [Tooltip("�Ϲ� ������ ������ �Ÿ��Դϴ�. ���� �� �� �Ÿ��� ������ �Ϲ� ������ �����մϴ�.")]
    public float attackRange = 3f;
    [Tooltip("�Ϲ� ���� ��Ÿ���Դϴ�.")]
    public float attackCooldown = 1.5f;
    [Tooltip("���� �� ���� �Ҹ�Ǵ� ���� ���Դϴ�.")]
    public float manaCostPerCharge = 5f;
    [Tooltip("�ʴ� ȸ���Ǵ� ���� ���Դϴ�.")]
    public float manaRegenRate = 1f;

    // === ���� ���� ===
    private float currentMana;
    private float lastAttackTime;
    private Vector3 chargeDestination;
    private bool hasInitiatedCharge = false;
    private bool hasDealtChargeDamage = false; // �߰�: ���� �� �������� �������� ����

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>(); // MonsterPatrol ������Ʈ ����
        if (monster == null || monsterCombat == null || monsterPatrol == null)
        {
            Debug.LogError("BoarBehavior: �ʼ� ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void Start()
    {
        currentMana = monster.monsterData.maxMana;
        // ����� ���ʹ� ���ۺ��� ���� ���·� ����
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
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
        // �������� ������ ��� ���� ���·� ��ȯ
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        // ���� ���̾��ٸ� ���� ����
        monsterPatrol.StopPatrol();
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
            // ���Ͱ� ������ ��� �ൿ ����
            monsterPatrol.StopPatrol();
            return;
        }

        // ���� ȸ�� ����
        if (currentMana < monster.monsterData.maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // Patrol ������ �� ���� ����
                monsterPatrol.StartPatrol();

                // �÷��̾ �����ϸ� ������ ���� ���� �Ǵ� ���� ���·� ��ȯ
                if (distanceToPlayer < monster.detectionRange)
                {
                    monsterPatrol.StopPatrol(); // ���� ����
                    if (currentMana >= manaCostPerCharge)
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Charge);
                    }
                    else
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                }
                break;

            case MonsterBase.MonsterState.Charge:
                // ���� ���� �� ���� ���� ����
                monsterPatrol.StopPatrol();
                HandleCharge(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                // ���� ���� �� ���� ���� ����
                monsterPatrol.StopPatrol();
                HandleAttack(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                // Idle ���¿����� ���� ����
                monsterPatrol.StopPatrol();
                break;
        }
    }

    private void HandleCharge(float distanceToPlayer)
    {
        if (!hasInitiatedCharge)
        {
            // ���� ��ǥ ���� ����
            chargeDestination = transform.position + (playerTransform.position - transform.position).normalized * chargeDistance;
            currentMana -= manaCostPerCharge;
            hasInitiatedCharge = true;
            hasDealtChargeDamage = false; // ���ο� ���� ���� �� �ʱ�ȭ
        }

        // ���� �������� �̵�
        transform.position = Vector3.MoveTowards(transform.position, chargeDestination, chargeSpeed * Time.deltaTime);

        // �÷��̾� �������� �ü� ����
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // ��ǥ ������ �����߰ų� �÷��̾�� �����ϸ� ���� ���·� ��ȯ
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
            // ���� ���� ���̸� ������ ���� ���� �Ǵ� �߰�
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
            // ���� ���� ���̸� ���� ����
            PerformAttack();
        }

        // �÷��̾ ���� ������ ����� Idle ���·� ��ȯ
        if (distanceToPlayer > monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol); // Idle ��� Patrol ���·� ����
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
}