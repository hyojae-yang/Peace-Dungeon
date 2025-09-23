using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System; // �̺�Ʈ ����� ���� System ���ӽ����̽� �߰�

/// <summary>
/// ���� ������ ������ �ൿ ������ ����ϴ� Ŭ�����Դϴ�.
/// �÷��̾ �����ϴ� ü���� ���� ���Ϸ� �������� �ֺ� ���Ḧ ������ ������ �����ϰ� �Բ� �����մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))]
[RequireComponent(typeof(MonsterCombat))] // MonsterCombat ������Ʈ�� �ʿ����� ���
public class WolfBehavior : MonoBehaviour
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterPatrol monsterPatrol;
    private MonsterCombat monsterCombat;
    private Transform playerTransform;

    // === �ൿ ���� ===
    [Header("���� �ൿ ����")]
    [Tooltip("ü���� �� ���� ���Ϸ� �������� ������ �����մϴ�.")]
    [Range(0.1f, 0.9f)]
    public float callForHelpHealthRatio = 0.5f;
    [Tooltip("���Ḧ ã�� ���� �ֺ��� Ž���� �ݰ��Դϴ�.")]
    public float flockDetectionRadius = 15f;
    [Tooltip("�÷��̾�� ������ �����ϴ� �ּ� �Ÿ��Դϴ�.")]
    public float attackRange = 2f;
    [Tooltip("���� ���� �� ������ �̵� �ӵ��Դϴ�.")]
    public float packAttackSpeed = 5f;
    [Tooltip("�Ϲ� ���� ��Ÿ���Դϴ�.")]
    public float attackCooldown = 1.5f;

    // === ���� ���� ���� ===
    private bool hasCalledForHelp = false;
    private bool isLeader = false;
    private WolfBehavior leader;
    private List<WolfBehavior> followers = new List<WolfBehavior>();
    private float lastAttackTime;

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        monsterCombat = GetComponent<MonsterCombat>();
        if (monster == null || monsterPatrol == null || monsterCombat == null)
        {
            Debug.LogError("WolfBehavior: �ʼ� ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void OnEnable()
    {
        // ���Ͱ� �������� ���� �� OnMonsterDamaged �޼��带 ȣ���ϵ��� ����
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    void OnDisable()
    {
        // ���� ����
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    /// <summary>
    /// ���Ͱ� �������� �Ծ��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="damage">���� ���ط�</param>
    private void OnMonsterDamaged(float damage)
    {
        // ü���� ���� ���Ϸ� �������� ���Ḧ ����
        if (!hasCalledForHelp && monsterCombat.GetCurrentHealth() <= monster.monsterData.maxHealth * callForHelpHealthRatio)
        {
            CallForHelp();
        }
    }

    void Start()
    {
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
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
            case MonsterBase.MonsterState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Chase:
                HandleChase(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Flocking:
                HandleFlocking(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Attack:
                HandleAttack(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StopPatrol();
                break;
        }
    }

    /// <summary>
    /// ���� ���� ������ ó���մϴ�.
    /// �÷��̾ �����ϸ� ���� ���·� ��ȯ�մϴ�.
    /// </summary>
    private void HandlePatrol(float distanceToPlayer)
    {
        monsterPatrol.StartPatrol();
        if (distanceToPlayer < monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Chase);
            monsterPatrol.StopPatrol();
        }
    }

    /// <summary>
    /// ���� ���� ������ ó���մϴ�.
    /// ü�� ���ǿ� ���� ���� ������ �õ��ϰų�, ���� ������ ������ ���� ���·� ��ȯ�մϴ�.
    /// </summary>
    private void HandleChase(float distanceToPlayer)
    {
        // 1. �÷��̾ ���� ������ ������ ���� ���·� ��ȯ
        if (distanceToPlayer <= attackRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            return;
        }

        // 2. �÷��̾ ���� ������ ����� ���� ���·� ���ư�
        if (distanceToPlayer > monster.detectionRange + 2f)
        {
            ExitPack();
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// �Ϲ� ���� ���� ������ ó���մϴ�.
    /// </summary>
    private void HandleAttack(float distanceToPlayer)
    {
        // �÷��̾ ���� ������ ����� �ٽ� ���� ���·� ��ȯ
        if (distanceToPlayer > attackRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Chase);
        }
        else
        {
            // ���� ���� ������ ����
            PerformAttack();
        }
    }

    /// <summary>
    /// ���� �ൿ ���� ������ ó���մϴ�. (Flocking ����)
    /// ������ ������ ���� �������� �и��Ͽ� �����ϸ�, ���� ������ ������ ���� ���·� ��ȯ�մϴ�.
    /// </summary>
    private void HandleFlocking(float distanceToPlayer)
    {
        if (isLeader)
        {
            // ������ �÷��̾ ���� �߰�
            MoveTowardsTarget(playerTransform, packAttackSpeed);

            // ��� �����ڵ��� �÷��̾ �����ϵ��� ���
            foreach (var follower in followers)
            {
                if (follower != null)
                {
                    // �����ڵ� ���� ������ ������ ���� ���·� ��ȯ
                    if (Vector3.Distance(follower.transform.position, playerTransform.position) <= follower.attackRange)
                    {
                        follower.monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                    else
                    {
                        follower.MoveTowardsTarget(playerTransform, packAttackSpeed);
                    }
                }
            }
        }
        else if (leader != null)
        {
            // �����ڴ� ������ ���� �̵�
            MoveTowardsTarget(leader.transform, packAttackSpeed);

            // �����ڵ� ���� ������ ������ ���� ���·� ��ȯ
            if (distanceToPlayer <= attackRange)
            {
                monster.ChangeState(MonsterBase.MonsterState.Attack);
            }
        }

        // �÷��̾ ���� ������ ����� ���� ���� �� ���� �ߴ�
        if (distanceToPlayer > monster.detectionRange + 5f)
        {
            ExitPack();
        }
    }

    /// <summary>
    /// ���� ������� Ž���Ͽ� ������ �����մϴ�.
    /// �� �޼��尡 ȣ��� ���밡 ������ ������ �˴ϴ�.
    /// </summary>
    private void CallForHelp()
    {

        hasCalledForHelp = true;
        isLeader = true;
        monster.ChangeState(MonsterBase.MonsterState.Flocking);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, flockDetectionRadius);


        foreach (var hitCollider in hitColliders)
        {
            WolfBehavior otherWolf = hitCollider.GetComponent<WolfBehavior>();

            // ����׿� �α�: WolfBehavior ������Ʈ�� ã�Ҵ��� Ȯ��
            if (otherWolf != null && otherWolf != this)
            {
                if (!otherWolf.IsPartOfPack())
                {
                    otherWolf.JoinPack(this);
                    AddFollower(otherWolf);
                }
            }
        }
    }

    /// <summary>
    /// �ٸ� ���밡 �� ���븦 ������ �շ���Ű�� �� ����մϴ�.
    /// </summary>
    /// <param name="newLeader">������ ���� ����</param>
    public void JoinPack(WolfBehavior newLeader)
    {
        if (isLeader) return; // �̹� ������� ����

        leader = newLeader;
        isLeader = false;
        monster.ChangeState(MonsterBase.MonsterState.Flocking); // Flocking ���·� ��ȯ
    }

    /// <summary>
    /// �������� ��Ż�Ͽ� �ٽ� ���� ���·� ���ư��ϴ�.
    /// ������ ��� �����ڵ��� �ػ��ŵ�ϴ�.
    /// </summary>
    private void ExitPack()
    {
        hasCalledForHelp = false;
        isLeader = false;
        leader = null;
        followers.Clear();
        monster.ChangeState(MonsterBase.MonsterState.Patrol);

        // �����ڵ鿡�� ���� �ػ��� �˸�
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                follower.ExitPack();
            }
        }
    }

    /// <summary>
    /// ������ ���� �ִ��� ���θ� ��ȯ�մϴ�.
    /// </summary>
    public bool IsPartOfPack()
    {
        return leader != null || isLeader;
    }

    /// <summary>
    /// �����ڸ� ���� ��Ͽ� �߰��մϴ�.
    /// </summary>
    /// <param name="wolf">�߰��� ������ ����</param>
    private void AddFollower(WolfBehavior wolf)
    {
        if (!followers.Contains(wolf))
        {
            followers.Add(wolf);
        }
    }

    /// <summary>
    /// ��ǥ ������ ���� �̵��ϴ� ���� ����.
    /// </summary>
    public void MoveTowardsTarget(Transform targetTransform, float speed)
    {
        if (targetTransform == null) return;

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
                Debug.Log($"���밡 �÷��̾�� {monster.monsterData.attackPower}�� �������� �������ϴ�!");
            }
        }
    }
}