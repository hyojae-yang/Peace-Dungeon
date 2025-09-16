using UnityEngine;

/// <summary>
/// ���Ÿ� ���ݿ� ���Ǵ� �߻�ü(����ü)�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class Projectile : MonoBehaviour
{
    private WeaponItemSO weaponData;
    private LayerMask monsterLayer;
    private float maxDistance;
    private Vector3 startPosition;

    [Header("�߻�ü �Ӽ�")]
    [Tooltip("�߻�ü�� �̵� �ӵ��Դϴ�. �ν����Ϳ��� ���� �����ϼ���.")]
    public float projectileSpeed = 10f; // �⺻���� 10���� �����߽��ϴ�.

    private Transform target; // �߻�ü�� ������ ��� (���� ����� ����)

    void Awake()
    {
        startPosition = transform.position;
    }

    /// <summary>
    /// PlayerAttack ��ũ��Ʈ���� ȣ���Ͽ� �߻�ü�� �����͸� �����մϴ�.
    /// </summary>
    /// <param name="weapon">�߻�ü�� ����� ���� ������</param>
    /// <param name="layer">���� ���̾� ����ũ</param>
    public void SetProjectileData(WeaponItemSO weapon, LayerMask layer)
    {
        weaponData = weapon;
        monsterLayer = layer;
        maxDistance = weapon.attackRange;

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
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                float finalDamage = PlayerStats.Instance.attackPower;
                monster.TakeDamage(finalDamage, weaponData.damageType);
                Debug.Log($"<color=red>���Ÿ� ���� ��Ʈ:</color> {other.name}���� {finalDamage}�� �������� �������ϴ�.");

                // �˹� ȿ�� ����
                if (weaponData.knockbackForce > 0)
                {
                    Rigidbody monsterRb = monster.GetComponent<Rigidbody>();
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