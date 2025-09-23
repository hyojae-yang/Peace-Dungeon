using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �� ������ Ư�� �ൿ ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �Ϲ� ���ݰ� Ư�� ������ ����ϸ�, ������ �⺻ ���� ����(����, ����)���� �и��Ǿ� �ֽ��ϴ�.
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
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        rb = GetComponent<Rigidbody>();

        // Ư�� ���� ��Ÿ���� �ʱ�ȭ�Ͽ� ���� ���� ��� Ư�� ������ �����ϰ� �մϴ�.
        lastAoeAttackTime = -aoeAttackCooldown;
    }

    private void Update()
    {
        // ���Ͱ� �׾��ٸ� ������ �������� �ʽ��ϴ�.
        if (monster.currentState == Monster.MonsterState.Dead)
        {
            return;
        }

        // ������ ���� ���¿� ���� ������ �ൿ�� �����մϴ�.
        switch (monster.currentState)
        {
            case Monster.MonsterState.Attack:
                HandleAttackState();
                break;
            case Monster.MonsterState.Charge:
                HandleChargeState();
                break;
        }
    }

    /// <summary>
    /// ���� ���¿��� �Ϲ� ���ݰ� Ư�� ���� ������ �����մϴ�.
    /// Monster ��ũ��Ʈ�� �̹� �÷��̾� ���� �� ���� ��ȯ�� ó���ϹǷ�,
    /// �� �޼���� ���� ���� ������ ����մϴ�.
    /// </summary>
    private void HandleAttackState()
    {
        // �÷��̾� Ʈ�������� ���� ���, ���� ���·� �����մϴ�.
        // �̴� Monster ��ũ��Ʈ���� Ÿ���� �Ҿ��� �� �Ͼ�ϴ�.
        if (monster.detectableTarget == null)
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
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // �Ϲ� ���� ��Ÿ�� üũ.
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
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
    /// �÷��̾�� ���� ������ �����ϰ� �������� ������ �޼����Դϴ�.
    /// �� �޼���� Attack ���¿����� ȣ��˴ϴ�.
    /// </summary>
    private void PerformMeleeAttack()
    {
        // Monster ��ũ��Ʈ���� Ÿ���� �̹� �����ߴ��� Ȯ���մϴ�.
        if (monster.detectableTarget == null) return;

        // �÷��̾�Ը� �������� �������� ��Ȯ�� �����մϴ�.
        if (monster.detectableTarget.GetTransform().TryGetComponent(out IDamageable damageable))
        {
            monsterCombat.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
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