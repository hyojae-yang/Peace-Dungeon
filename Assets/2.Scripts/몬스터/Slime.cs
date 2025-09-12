using UnityEngine;

// Monster Ŭ������ ��ӹ޴� Slime Ŭ�����Դϴ�.
public class Slime : Monster
{
    // �������� ���� ��Ÿ���� ���� ����
    [Header("������ ���� ����")]
    public float attackCooldown = 2f; // ���� ��Ÿ�� (2��)
    public float attackRange = 1.5f; // ���� ����

    private float lastAttackTime;

    void Awake()
    {
        lastAttackTime = -attackCooldown; // ���� ���� �� �ٷ� ������ �� �ֵ��� �ʱ�ȭ
    }

    // �θ� Ŭ������ Update()�� �������̵��Ͽ� ���ο� ������ �߰��մϴ�.
    protected override void Update()
    {
        // �θ� Ŭ������ Monster�� Update()�� ���� ȣ���Ͽ� �÷��̾� ���� ������ �����մϴ�.
        base.Update();

        // �÷��̾ �����Ǿ����� Ȯ���մϴ�.
        // isPlayerDetected�� detectableTarget ������ Monster ��ũ��Ʈ���� �����˴ϴ�.
        if (isPlayerDetected && detectableTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, detectableTarget.GetTransform().position);

            // �÷��̾���� �Ÿ��� ���� �������� �ָ� �÷��̾ ����
            if (distanceToTarget > attackRange)
            {
                // �÷��̾ ���� �̵�
                Vector3 direction = (detectableTarget.GetTransform().position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            // �÷��̾ ���� ���� �ȿ� �ְ� ��Ÿ���� �����ٸ� ����
            else if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(); // ���� �޼��� ȣ��
            }
        }
    }

    // Attack() �޼��带 ������(Override)�Ͽ� �����Ӹ��� ���� ����� �����մϴ�.
    public override void Attack()
    {
        // �θ� Ŭ���� Monster�� Attack() �޼��带 ���� ȣ���մϴ�.
        // �� �޼��� ������ IDamageable �������̽��� ���� ������ ������ ó���մϴ�.
        base.Attack();

        // ���� ��Ÿ�� ������Ʈ
        lastAttackTime = Time.time;

        Debug.Log("�������� ������ ������ �õ��մϴ�!");
    }
}