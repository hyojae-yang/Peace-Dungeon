using UnityEngine;

/// <summary>
/// �� ������ Ư�� �ൿ ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �Ϲ� ���� �� ���� �ð����� ���� ���� ������ �����մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
[RequireComponent(typeof(Rigidbody))]
public class BearBehavior : MonoBehaviour
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private Rigidbody rb;
    private Transform playerTransform; // �÷��̾��� ��ġ�� �����ϱ� ���� ����

    // === �Ϲ� ���� ���� ���� ===
    [Header("�Ϲ� ���� ����")]
    [Tooltip("�Ϲ� ������ ������ �ּ� �Ÿ��Դϴ�.")]
    [SerializeField] private float attackRange = 2f;
    [Tooltip("�Ϲ� ������ ��Ÿ���Դϴ�.")]
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime; // ������ �Ϲ� ������ ����� �ð�

    // === Ư�� ���� ���� ���� ===
    [Header("Ư�� ���� ����")]
    [Tooltip("Ư�� ������ ����(������)�Դϴ�.")]
    [SerializeField] private float aoeAttackRadius = 5f;
    [Tooltip("Ư�� ������ ��Ÿ���Դϴ�.")]
    [SerializeField] private float aoeAttackCooldown = 10f;
    [Tooltip("Ư�� ���� �غ� �ð��Դϴ�. (��¡ �ִϸ��̼� ���̿� ���߾� ����)")]
    [SerializeField] private float aoeChargeTime = 1.5f;

    // === ���� ���� ���� ===
    private float lastAoeAttackTime; // ������ Ư�� ������ ����� �ð�
    private float currentChargeTime; // ���� ��¡�� ����� �ð�

    private void Awake()
    {
        // �ʿ��� ������Ʈ���� �����ɴϴ�.
        // GetComponent<T>�� ����Ͽ� ���Ӽ��� ��Ȯ�� �����մϴ�.
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        rb = GetComponent<Rigidbody>();

        // �÷��̾� ������Ʈ�� ã�� Transform�� �����մϴ�.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // Ư�� ���� ��Ÿ���� �ʱ�ȭ�Ͽ� ���� ���� ��� Ư�� ������ �����ϰ� �մϴ�.
        lastAoeAttackTime = -aoeAttackCooldown;
    }

    private void Update()
    {
        // �÷��̾� Ʈ�������� ���ų� ���Ͱ� �׾��ٸ� ������ �������� �ʽ��ϴ�.
        if (playerTransform == null || monster.currentState == Monster.MonsterState.Dead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // ������ ���� ���¿� ���� ������ �ൿ�� �����մϴ�.
        // ���� ������ Ȱ���Ͽ� �� ������ ������ �и��մϴ�.
        switch (monster.currentState)
        {
            case Monster.MonsterState.Patrol:
                // Monster ��ũ��Ʈ�� ���� ������ �������� �ʰ�, ��ü������ �÷��̾ �����մϴ�.
                if (distanceToPlayer <= monster.detectionRange)
                {
                    monster.ChangeState(Monster.MonsterState.Chase);
                }
                break;
            case Monster.MonsterState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case Monster.MonsterState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
            case Monster.MonsterState.Charge:
                HandleChargeState();
                break;
        }
    }

    /// <summary>
    /// ���� ���� ������ ó���մϴ�.
    /// �÷��̾ ���� ������ ������ ���� ���·� ��ȯ�մϴ�.
    /// </summary>
    private void HandleChaseState(float distanceToPlayer)
    {
        // �÷��̾ ���� ������ ������ ���� ���·� ��ȯ�մϴ�.
        if (distanceToPlayer <= attackRange)
        {
            monster.ChangeState(Monster.MonsterState.Attack);
        }
        // �÷��̾ ���� ������ ����� �ٽ� ���� ���·� ���ư��ϴ�.
        else if (distanceToPlayer > monster.detectionRange)
        {
            monster.ChangeState(Monster.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// ���� ���¿��� �Ϲ� ���ݰ� Ư�� ���� ������ �����մϴ�.
    /// Ư�� ���� ��Ÿ���� ������ Charge ���·� ��ȯ�մϴ�.
    /// </summary>
    private void HandleAttackState(float distanceToPlayer)
    {
        // �÷��̾���� �Ÿ��� üũ�Ͽ� ���� ���� ���θ� �Ǵ��մϴ�.
        // �÷��̾ ������ ����� Chase ���·� �����մϴ�.
        if (distanceToPlayer > attackRange)
        {
            monster.ChangeState(Monster.MonsterState.Chase);
            return;
        }

        // Ư�� ���� ��Ÿ�� üũ.
        if (Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            // ��Ÿ���� ������ Ư�� ������ ���� Charge ���·� ��ȯ�մϴ�.
            monster.ChangeState(Monster.MonsterState.Charge);
            currentChargeTime = 0;
            // Charge ���� ���� ��, Rigidbody�� �ӵ��� 0���� ����� ������ �������� ����ϴ�.
            // ScriptableObject�� ���� ������(moveSpeed)�� �������� �ʾ� SOLID ��Ģ�� �����մϴ�.
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // �Ϲ� ���� ��Ÿ�� üũ.
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // �Ϲ� ���� ���� ����.
            // ������ �Ϲ� ���ݷ��� ������ ���� �������� �����ϴ�.
            monsterCombat.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
            lastAttackTime = Time.time;
        }
    }

    /// <summary>
    /// Ư�� ������ �غ��ϴ� ��¡ ���¸� ó���մϴ�.
    /// ���� �ð��� ������ Ư�� ������ �����ϰ� �ٽ� Attack ���·� ���ư��ϴ�.
    /// </summary>
    private void HandleChargeState()
    {
        // ��¡ �ð��� ������Ʈ�մϴ�.
        currentChargeTime += Time.deltaTime;

        // ��¡ �ð��� �������� �ʰ��ϸ� Ư�� ������ �����մϴ�.
        if (currentChargeTime >= aoeChargeTime)
        {
            PerformAOEAttack(); // Ư�� ���� ����
            monster.ChangeState(Monster.MonsterState.Attack); // Attack ���·� ����
            lastAoeAttackTime = Time.time; // ������ Ư�� ���� �ð� ����
        }
    }

    /// <summary>
    /// �ֺ��� ��� ����ü���� ���� ���ظ� ������ Ư�� ���� �޼����Դϴ�.
    /// </summary>
    private void PerformAOEAttack()
    {
        // �� �ֺ��� �ִ� ��� �ݶ��̴��� �����մϴ�.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeAttackRadius);

        // ������ �ݶ��̴����� ��ȸ�ϸ� �������� �����ϴ�.
        foreach (var hitCollider in hitColliders)
        {
            // IDamageable �������̽��� ���� ������Ʈ�� ã��, �ڱ� �ڽ��� ���� ��󿡼� �����մϴ�.
            if (hitCollider.TryGetComponent(out IDamageable damageable) && hitCollider.gameObject != this.gameObject)
            {
                // MonsterData���� ������ ���� ���ݷ��� �����ɴϴ�.
                float magicDamage = monster.monsterData.magicAttackPower;

                // TakeDamage �޼��带 ȣ���Ͽ� ���� �������� �����ϴ�.
                // MonsterCombat ��ũ��Ʈ�� �˾Ƽ� ���� ������ �����մϴ�.
                damageable.TakeDamage(magicDamage, DamageType.Magic);
            }
        }
    }

    /// <summary>
    /// ����� �� �ð�ȭ�� ���� ����� �׸��ϴ�.
    /// �����Ϳ��� Ư�� ���� ������ ������ ��ü�� ǥ���մϴ�.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeAttackRadius);
    }
}