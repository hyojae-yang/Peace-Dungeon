using UnityEngine;
using System.Collections;

/// <summary>
/// �÷��̾��� ���� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ ���� �����Ϳ� ���� ���� ����� �������� ����˴ϴ�.
/// SOLID ��Ģ �� ���� å�� ��Ģ(SRP)�� �ؼ��մϴ�.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    // === ���� ������ ===
    /// <summary>
    /// PlayerEquipment�κ��� ���޹��� ���� �������Դϴ�.
    /// </summary>
    private WeaponItemSO equippedWeapon;

    /// <summary>
    /// ������ ���� �ð��� ����ϴ� �����Դϴ�.
    /// ���� ��Ÿ���� üũ�ϴ� �� ���˴ϴ�.
    /// </summary>
    private float lastAttackTime;

    // === �÷��̾� ���� �� ���̾� ����ũ ===
    [Header("����")]
    [Tooltip("���Ϳ��� �������� ������ �� ����� ���̾� ����ũ�Դϴ�.")]
    public LayerMask monsterLayer;

    private void Start()
    {
        // PlayerCharacter�� �ν��Ͻ��� �����ͼ� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� ã�� �� �����ϴ�. ��ũ��Ʈ�� ����� �������� ���� �� �ֽ��ϴ�.");
            return;
        }

        // �ʱ�ȭ ��, ��� ���� ���� ���·� ����ϴ�.
        //equippedWeapon�� �ʱ�ȭ���� �ʾ����Ƿ� -100f�� ���� ����� ���� ������ ����
        lastAttackTime = -100f;
    }

    /// <summary>
    /// PlayerEquipment ��ũ��Ʈ�κ��� ���� ������ ���� �����͸� ���޹޴� �޼����Դϴ�.
    /// </summary>
    /// <param name="weapon">���� ������ ���� ������</param>
    public void UpdateEquippedWeapon(WeaponItemSO weapon)
    {
        equippedWeapon = weapon;
        if (equippedWeapon != null)
        {
            // ���� ���� ��, ������ ���� �ð��� �ʱ�ȭ�Ͽ� ��� ���� ���� ���·� ����ϴ�.
            lastAttackTime = Time.time - equippedWeapon.attackSpeed;
        }
    }

    void Update()
    {
        // 1. ���� ���� Ȯ��
        // ���Ⱑ �����Ǿ�����, ���콺 ���� ��ư�� ���ȴ���, ���� ��Ÿ���� �������� Ȯ���մϴ�.
        if (equippedWeapon != null && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + equippedWeapon.attackSpeed)
        {
            Attack();
            lastAttackTime = Time.time; // ���� �ð� ������Ʈ
        }
    }

    /// <summary>
    /// �÷��̾��� �⺻ ���� ������ �����մϴ�.
    /// �� �޼���� ������ ���� ���� ���� ������ ����մϴ�.
    /// </summary>
    void Attack()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerStats�� �ʱ�ȭ���� �ʾҽ��ϴ�. ������ ������ �� �����ϴ�.");
            return;
        }

        // 2. ���� Ÿ�Կ� ���� �⺻ ������ ����
        float baseDamage;
        bool isMagicAttack = (equippedWeapon.weaponType == WeaponType.Staff);

        if (isMagicAttack)
        {
            // �������� ��� ���� ���ݷ� ���
            baseDamage = playerCharacter.playerStats.magicAttackPower;
        }
        else
        {
            // �� �� ������ ��� �Ϲ� ���ݷ� ���
            baseDamage = playerCharacter.playerStats.attackPower;
        }

        // 3. ġ��Ÿ ���� ����
        // ġ��Ÿ Ȯ��(criticalChance)�� ������� ġ��Ÿ �߻� ���θ� �����մϴ�.
        bool isCritical = Random.Range(0f, 1f) <= playerCharacter.playerStats.criticalChance;

        // 4. ���� ������ ���
        float finalDamage = baseDamage;
        if (isCritical)
        {
            // ġ��Ÿ �߻� ��, ġ��Ÿ ������ ����(criticalDamageMultiplier)�� ���մϴ�.
            finalDamage *= playerCharacter.playerStats.criticalDamageMultiplier;
        }

        // 5. ���� Ÿ�Կ� ���� �ٸ� ���� ���� ���� (���� ������ ����)
        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Spear:
                PerformMeleeAttack(finalDamage);
                break;
            case WeaponType.Staff:
            case WeaponType.Bow:
                PerformRangedAttack(finalDamage);
                break;
            default:
                Debug.LogWarning("�� �� ���� ���� Ÿ���Դϴ�.");
                break;
        }
    }

    /// <summary>
    /// ���� ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">���� ���� ������</param>
    private void PerformMeleeAttack(float damage)
    {
        float currentAttackRange = equippedWeapon.attackRange;
        float currentAttackAngle = equippedWeapon.attackAngle;

        // ���� ���� ���� ���͸� ã���ϴ�.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentAttackRange, monsterLayer);

        foreach (Collider monsterCollider in hitColliders)
        {
            Vector3 directionToMonster = (monsterCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToMonster);

            // ���Ͱ� ���� ���� ���� �ȿ� �ִ��� Ȯ���մϴ�.
            if (angle < currentAttackAngle * 0.5f)
            {
                // ������ �κ�: Monster ������Ʈ ��� IDamageable �������̽��� �����ɴϴ�.
                IDamageable damageableTarget = monsterCollider.GetComponent<IDamageable>();

                if (damageableTarget != null)
                {
                    // ���� ���� �������� �����Ͽ� ���Ϳ��� ���ظ� �����ϴ�.
                    damageableTarget.TakeDamage(damage, equippedWeapon.damageType);

                    // �˹� ���� (���� ���� ����)
                    if (equippedWeapon.knockbackForce > 0)
                    {
                        Rigidbody monsterRb = monsterCollider.GetComponent<Rigidbody>();
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
    /// ���Ÿ� ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">���� ���� ������</param>
    private void PerformRangedAttack(float damage)
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
                // ���� ���� �������� �߻�ü�� �����մϴ�.
                projectileComponent.SetProjectileData(equippedWeapon, monsterLayer, damage);
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