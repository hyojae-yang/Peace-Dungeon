using UnityEngine;

// ���̾ ����ü�� ������, �浹 ó��, �׸��� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
public class FireballProjectile : MonoBehaviour
{
    [Header("����")]
    [Tooltip("���̾�� ���ư��� �ӵ��Դϴ�.")]
    public float moveSpeed = 10f;

    [Tooltip("���̾�� �ڵ����� ������� �ð�(��)�Դϴ�. �ʹ� ���� ���ư��� ���� �����մϴ�.")]
    public float lifetime = 5f;

    [Tooltip("���̾�� ������ �� �浹�� ������ �ð�(��)�Դϴ�. �÷��̾���� ��� �浹�� �����մϴ�.")]
    public float ignoreCollisionDuration = 0.2f; // ����: 0.2�� ���� �浹 ����

    private float damage; // PlayerSkillController�κ��� ���޹��� ������ ����
    private float startTime; // ���̾�� ������ �ð�
    private DamageType damageType; // �߰��� ������ Ÿ�� ����

    void Start()
    {
        startTime = Time.time; // ���� �ð� ���

        // ���� �ð� �� ������ �ı��ǵ��� Ÿ�̸Ӹ� �����մϴ�.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // �� �����Ӹ��� ���̾�� ������ �̵���ŵ�ϴ�.
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// PlayerSkillController�κ��� ���� ������ ���� Ÿ���� ���޹޴� �޼����Դϴ�.
    /// </summary>
    /// <param name="finalDamage">���� ���� ������</param>
    /// <param name="type">������ Ÿ�� (����, ���� ��)</param>
    public void SetDamage(float finalDamage, DamageType type)
    {
        damage = finalDamage;
        damageType = type;
    }

    // �ݶ��̴��� �ε����� �� ȣ��Ǵ� �޼����Դϴ�.
    // Is Trigger�� üũ�� �ݶ��̴��� �浹���� �� �۵��մϴ�.
    void OnTriggerEnter(Collider other)
    {
        // ���� �ð� ������ �浹�� �����մϴ�.
        if (Time.time < startTime + ignoreCollisionDuration)
        {
            return;
        }

        // �浹�� ����� IDamageable �������̽��� ������ �ִ��� Ȯ���մϴ�.
        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            // IDamageable �������̽��� ���� ��ü���� �������� �����ϴ�.
            damageableObject.TakeDamage(damage, damageType);
        }

        // ���ͳ� ���������� ������ ��� �ı��˴ϴ�.
        Destroy(gameObject);
    }
}