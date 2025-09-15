using UnityEngine;

// �÷��̾��� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
public class PlayerAttack : MonoBehaviour
{
    // ���� ������ �ð������� �����ֱ� ���� ���� (����Ƽ �����Ϳ��� ���Դϴ�)
    [Header("���� ����")]
    [Tooltip("������ ��� �ִ� �Ÿ� (������)�Դϴ�.")]
    public float attackRange = 2f; // ���� �Ÿ� (��ä���� ������)

    [Tooltip("������ ��� ��ä�� �����Դϴ�. (�� ����, ���� ��� 90�� �¿� 45����)")]
    [Range(0, 360)]
    public float attackAngle = 90f; // ���� ���� (��ä���� �� ����)

    [Tooltip("���Ϳ��� �������� ������ �� ����� ���̾� ����ũ�Դϴ�.")]
    public LayerMask monsterLayer;

    // PlayerStats ��ũ��Ʈ�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // private PlayerStats playerStats;

    void Start()
    {
        // ���� ���� ��, PlayerStats �̱��� �ν��Ͻ��� �����ϴ��� Ȯ���մϴ�.
        // GetComponent�� ���� ������ �ʿ䰡 �����ϴ�.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� �������� �ʽ��ϴ�. ���� ���� �� PlayerStats�� ���� ���� ������Ʈ�� ���� �ִ��� Ȯ���� �ּ���.");
        }
    }

    void Update()
    {
        // ���콺 ��Ŭ��(0)�� ���ȴ��� Ȯ���մϴ�.
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    /// <summary>
    /// �÷��̾��� �⺻ ���� ������ �����մϴ�.
    /// </summary>
    void Attack()
    {
        // �̱��� �ν��Ͻ��� ��ȿ���� �ٽ� �ѹ� Ȯ���մϴ�.
        if (PlayerStats.Instance == null) return;

        // Physics.OverlapSphere�� ����Ͽ� attackRange ���� ��� �ݶ��̴��� �켱 �����մϴ�.
        // �� ���� ��ä�� ������ �ִ� �ݰ��� ��Ÿ���ϴ�.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, monsterLayer);

        // ������ �ݶ��̴����� ��ȸ�ϸ鼭 ��ä�� ���� ���� �ִ��� Ȯ���մϴ�.
        foreach (Collider monsterCollider in hitColliders)
        {
            // ���� ������Ʈ�� �÷��̾��� �þ�(�� ����) ���� ���� �ִ��� Ȯ���մϴ�.
            Vector3 directionToMonster = (monsterCollider.transform.position - transform.position).normalized;

            // �÷��̾��� �� ����� ���� ���� ���� ���� ������ ����մϴ�.
            // Vector3.Angle �Լ��� �� ���� ������ ������ 0������ 180�� ������ ������ ��ȯ�մϴ�.
            float angle = Vector3.Angle(transform.forward, directionToMonster);

            // ���� ������ ������ ���� ������ ���ݺ��� ������ (��, ��ä�� ���� ���� ������) ����
            if (angle < attackAngle * 0.5f)
            {
                // ���� �±� Ȯ��
                if (monsterCollider.CompareTag("Monster"))
                {
                    // ���� �θ� Ŭ������ Monster ������Ʈ�� �����ɴϴ�.
                    Monster monster = monsterCollider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        // PlayerStats.Instance�� ���� ���ݷ��� �����ɴϴ�.
                        float damage = PlayerStats.Instance.attackPower;
                        monster.TakeDamage(damage); // Monster Ŭ������ TakeDamage �޼��� ȣ��
                        Debug.Log(monsterCollider.name + "���� " + damage + "�� �������� �������ϴ�! ���� ü��: " + monster.health);
                    }
                    else
                    {
                        Debug.LogWarning(monsterCollider.name + "�� Monster ��ũ��Ʈ�� �����ϴ�.");
                    }
                }
            }
        }
    }

    // ���� ������ ����Ƽ �����Ϳ��� �ð������� Ȯ���ϱ� ���� �ڵ�
    private void OnDrawGizmosSelected()
    {
        // Gizmo ���� ����
        Gizmos.color = Color.red;

        // ���� ����(������)�� ���� ������ ����Ͽ� ��ä�� ����� �׸��ϴ�.
        // ��Ȯ�� ��ä���� �׸��� ���� ���� ����������, ���⼭�� �ð������� �뷫���� ������ �����ݴϴ�.
        // �߽ɿ��� attackRange���� ���� �߰�, ������ ���� ������ ������ ���� �׸��ϴ�.
        Vector3 forwardLimit = transform.position + transform.forward * attackRange;
        Gizmos.DrawLine(transform.position, forwardLimit);

        // ��ä���� ���� ��輱
        Vector3 leftLimit = Quaternion.Euler(0, -attackAngle * 0.5f, 0) * transform.forward * attackRange;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);

        // ��ä���� ������ ��輱
        Vector3 rightLimit = Quaternion.Euler(0, attackAngle * 0.5f, 0) * transform.forward * attackRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);

        // ��ä���� �ܰ��� (ȣ)�� �׸��� ���� ���� ���е�� �����մϴ�.
        // ��Ȯ�� ȣ�� �׸����� �� ���� ����� �ʿ��ϹǷ�, ���⼭�� �ܼ�ȭ�Ͽ� �ð������� �����ϴ�.
        // Unity Editor�� Scene �信�� �÷��̾��� ���� ����� ������ ���������� Ȯ���� �� �ְ� �˴ϴ�.
    }
}