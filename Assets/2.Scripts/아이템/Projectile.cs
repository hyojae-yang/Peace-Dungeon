using UnityEngine;
using System.Collections;

/// <summary>
/// ���Ÿ� ���ݿ� ���Ǵ� �߻�ü(����ü)�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���� å�� ��Ģ(SRP)�� �ؼ��ϱ� ���� ������ ��� ������ PlayerAttack ��ũ��Ʈ���� �޾ƿ����� �����߽��ϴ�.
/// </summary>
public class Projectile : MonoBehaviour
{
    // === �߻�ü ������ ===
    /// <summary>
    /// �߻�ü�� ����� ���� �������Դϴ�.
    /// </summary>
    private WeaponItemSO weaponData;

    /// <summary>
    /// ���� ���̾ �ĺ��ϴ� �� ���Ǵ� ���̾� ����ũ�Դϴ�.
    /// </summary>
    private LayerMask monsterLayer;

    /// <summary>
    /// �߻�ü�� ���ư� �� �ִ� �ִ� �Ÿ��Դϴ�.
    /// </summary>
    private float maxDistance;

    /// <summary>
    /// �߻�ü�� ������ ���� ��ġ�Դϴ�. �ִ� ��Ÿ��� ����ϴ� �� ���˴ϴ�.
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// PlayerAttack ��ũ��Ʈ���� ���Ǿ� ���޹��� ���� ������ ���Դϴ�.
    /// </summary>
    private float finalDamage;

    // === �߻�ü �Ӽ� ===
    [Header("�߻�ü �Ӽ�")]
    [Tooltip("�߻�ü�� �̵� �ӵ��Դϴ�. �ν����Ϳ��� ���� �����ϼ���.")]
    public float projectileSpeed = 10f;

    /// <summary>
    /// �߻�ü�� ������ ��� (���� ����� ����)�� Transform�Դϴ�.
    /// </summary>
    private Transform target;

    void Awake()
    {
        // �߻�ü ���� ������ ��ġ�� ����մϴ�.
        startPosition = transform.position;
    }

    /// <summary>
    /// PlayerAttack ��ũ��Ʈ���� ȣ���Ͽ� �߻�ü�� �����͸� �����մϴ�.
    /// </summary>
    /// <param name="weapon">�߻�ü�� ����� ���� ������</param>
    /// <param name="layer">���� ���̾� ����ũ</param>
    /// <param name="damage">���� ���� ������ ��</param>
    public void SetProjectileData(WeaponItemSO weapon, LayerMask layer, float damage)
    {
        weaponData = weapon;
        monsterLayer = layer;
        maxDistance = weapon.attackRange;
        finalDamage = damage; // ���޹��� ���� ������ ���� �����մϴ�.

        // �߻�ü ���� ������ ���� ����� ���͸� ã�� Ÿ������ �����մϴ�.
        FindClosestMonster();
    }

    /// <summary>
    /// ���� ���� ������ ���� ����� ���͸� ã�� Ÿ������ �����մϴ�.
    /// </summary>
    private void FindClosestMonster()
    {
        // OverlapSphere�� ����� �ֺ� ���͸� ��� ã���ϴ�.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDistance, monsterLayer);

        if (hitColliders.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestMonster = null;

            foreach (Collider col in hitColliders)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMonster = col.transform;
                }
            }
            target = closestMonster;
        }
    }

    void Update()
    {
        // 1. �߻�ü �̵�
        // projectileSpeed�� �ν����Ϳ��� ������ ���� ����մϴ�.
        if (target != null)
        {
            // Ÿ���� ������ Ÿ���� ���� ȸ���ϰ� �̵��մϴ�.
            Vector3 direction = (target.position - transform.position).normalized;
            transform.forward = direction;
            transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            // Ÿ���� ������ �����մϴ�.
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
        }

        // 2. �ִ� ��Ÿ� Ȯ�� �� �ı�
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �ٸ� �ݶ��̴��� �浹���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // �浹�� ������Ʈ�� ���� ���̾ ���ϴ��� Ȯ��
        if (((1 << other.gameObject.layer) & monsterLayer) != 0)
        {
            // ������ �κ�: Monster ������Ʈ ��� IDamageable �������̽��� �����ɴϴ�.
            IDamageable damageableTarget = other.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                // PlayerAttack ��ũ��Ʈ���� ���޹��� ���� ������ ���� ����մϴ�.
                damageableTarget.TakeDamage(finalDamage, weaponData.damageType);

                // �˹� ȿ�� ���� (���� ���� ����)
                if (weaponData.knockbackForce > 0)
                {
                    Rigidbody monsterRb = other.GetComponent<Rigidbody>();
                    if (monsterRb != null)
                    {
                        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                        monsterRb.AddForce(knockbackDirection * weaponData.knockbackForce, ForceMode.Impulse);
                    }
                }
            }
            // ���Ϳ��� �������� �������Ƿ� �߻�ü�� �ı��մϴ�.
            Destroy(gameObject);
        }
    }
}