using UnityEngine;
using System.Collections;

/// <summary>
/// �÷��̾��� ���� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ ���� �����Ϳ� ���� ���� ����� �������� ����˴ϴ�.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    // === ���� ������ ===
    private WeaponItemSO equippedWeapon; // PlayerEquipment�κ��� ���޹��� ���� ������
    private float lastAttackTime;        // ������ ���� �ð��� ����ϴ� ����

    // === �÷��̾� ���� �� ���̾� ����ũ ===
    [Header("����")]
    [Tooltip("���Ϳ��� �������� ������ �� ����� ���̾� ����ũ�Դϴ�.")]
    public LayerMask monsterLayer;

    // TODO: PlayerStats�� �̱������� �����ϰ� �����Ƿ�, Start()���� ������ ������ �ʿ� ����.

    /// <summary>
    /// PlayerEquipment ��ũ��Ʈ�κ��� ���� ������ ���� �����͸� ���޹޴� �޼����Դϴ�.
    /// </summary>
    public void UpdateEquippedWeapon(WeaponItemSO weapon)
    {
        equippedWeapon = weapon;
        if (equippedWeapon != null)
        {
            lastAttackTime = -equippedWeapon.attackSpeed;
        }
        else
        {
            // ���Ⱑ ������ ���, ������ ���� �ð��� �ʱ�ȭ�Ͽ� ��� ���� �Ұ� ���·� ����ϴ�.
            lastAttackTime = 0;
        }
    }

    void Update()
    {
        // 1. ���� ���� Ȯ��
        if (equippedWeapon != null && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + equippedWeapon.attackSpeed)
        {
            Attack();
            lastAttackTime = Time.time; // ���� �ð� ������Ʈ
        }
    }

    /// <summary>
    /// �÷��̾��� �⺻ ���� ������ �����մϴ�.
    /// ������ ���� �����Ϳ� ���� ���� ����� �������� ����˴ϴ�.
    /// </summary>
    void Attack()
    {
        // 2. ���� Ÿ�Կ� ���� �ٸ� ���� ���� ����
        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Spear:
                PerformMeleeAttack();
                break;
            case WeaponType.Staff:
            case WeaponType.Bow:
                PerformRangedAttack();
                break;
            default:
                Debug.LogWarning("�� �� ���� ���� Ÿ���Դϴ�.");
                break;
        }
    }

    /// <summary>
    /// ���� ���� ������ �����մϴ�. (���� ���� ����)
    /// </summary>
    private void PerformMeleeAttack()
    {
        float currentAttackRange = equippedWeapon.attackRange;
        float currentAttackAngle = equippedWeapon.attackAngle;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentAttackRange, monsterLayer);

        foreach (Collider monsterCollider in hitColliders)
        {
            Vector3 directionToMonster = (monsterCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToMonster);

            if (angle < currentAttackAngle * 0.5f)
            {
                Monster monster = monsterCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    float finalDamage = PlayerStats.Instance.attackPower;
                    monster.TakeDamage(finalDamage, equippedWeapon.damageType);

                    if (equippedWeapon.knockbackForce > 0)
                    {
                        Rigidbody monsterRb = monster.GetComponent<Rigidbody>();
                        if (monsterRb != null)
                        {
                            Vector3 knockbackDirection = (monsterCollider.transform.position - transform.position).normalized;
                            monsterRb.AddForce(knockbackDirection * equippedWeapon.knockbackForce, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// ���Ÿ� ���� ������ �����մϴ�. (�ű� �߰�)
    /// ���� �������� �����Ͽ� �߻�ü�� ����ϴ�.
    /// </summary>
    private void PerformRangedAttack()
    {
        // ���Ÿ� ���� ������(�߻�ü)�� �����Ǿ� ���� ��쿡�� ����
        if (equippedWeapon.projectilePrefab != null)
        {
            // �߻�ü(����ü)�� �÷��̾��� ��ġ���� �����մϴ�.
            GameObject projectile = Instantiate(equippedWeapon.projectilePrefab, transform.position, transform.rotation);

            // �߻�ü�� �����͸� �����մϴ�.
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                // ������ �κ�: ���� �ӵ� �Ű������� ������ �ʿ䰡 �����ϴ�.
                projectileComponent.SetProjectileData(equippedWeapon, monsterLayer);
            }
            else
            {
                Debug.LogError("�߻�ü �����տ� Projectile ��ũ��Ʈ�� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogWarning("���Ÿ� ������ ���� �߻�ü �������� �������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ ����Ƽ �����Ϳ��� �ð������� Ȯ���ϱ� ���� �Լ��Դϴ�.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (equippedWeapon != null)
        {
            Gizmos.color = Color.red;

            switch (equippedWeapon.weaponType)
            {
                case WeaponType.Sword:
                case WeaponType.Axe:
                case WeaponType.Spear:
                    // ���� ������ ��� ��ä�� ������ �׸��ϴ�.
                    Vector3 forwardLimit = transform.position + transform.forward * equippedWeapon.attackRange;
                    Gizmos.DrawLine(transform.position, forwardLimit);

                    Vector3 leftLimit = Quaternion.Euler(0, -equippedWeapon.attackAngle * 0.5f, 0) * transform.forward * equippedWeapon.attackRange;
                    Gizmos.DrawLine(transform.position, transform.position + leftLimit);

                    Vector3 rightLimit = Quaternion.Euler(0, equippedWeapon.attackAngle * 0.5f, 0) * transform.forward * equippedWeapon.attackRange;
                    Gizmos.DrawLine(transform.position, transform.position + rightLimit);
                    break;
                case WeaponType.Staff:
                case WeaponType.Bow:
                    // ���Ÿ� ������ ��� ���� ������ ������ �׸��ϴ�.
                    Gizmos.DrawWireSphere(transform.position, equippedWeapon.attackRange);
                    break;
            }
        }
    }
}