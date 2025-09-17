using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

// Monster Ŭ������ ��ӹ޴� Slime Ŭ�����Դϴ�.
public class Slime : Monster
{
    // �������� ���� ��Ÿ���� ���� ����
    [Header("������ ���� ����")]
    public float attackCooldown = 2f; // ���� ��Ÿ�� (2��)
    public float attackRange = 1.5f; // ���� ����

    private float lastAttackTime;

    // 'protected override' Ű���带 �߰��Ͽ� �θ� Ŭ������ Awake �޼��带 �������մϴ�.
    protected override void Awake()
    {
        // �θ� Ŭ������ Awake�� ȣ���մϴ�.
        base.Awake();
        lastAttackTime = -attackCooldown; // ���� ���� �� �ٷ� ������ �� �ֵ��� �ʱ�ȭ
    }

    // �θ� Ŭ������ Update()�� �������̵��Ͽ� ���ο� ������ �߰��մϴ�.
    protected override void Update()
    {
        // �θ� Ŭ������ Monster�� Update()�� ���� ȣ���Ͽ� �÷��̾� ���� �� �̵� ������ �����մϴ�.
        base.Update();

        // �÷��̾ �����Ǿ����� Ȯ���մϴ�.
        if (isPlayerDetected && detectableTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, detectableTarget.GetTransform().position);

            // �÷��̾ ���� ���� �ȿ� �ְ� ��Ÿ���� �����ٸ� ������ �õ��մϴ�.
            if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(); // ���� �޼��� ȣ��
            }
        }
    }

    // Attack() �޼��带 ������(Override)�Ͽ� �����Ӹ��� ���� ����� �����մϴ�.
    public override void Attack()
    {
        // �θ� Ŭ���� Monster�� Attack() �޼��带 ���� ȣ���մϴ�.
        base.Attack();

        // ���� ��Ÿ�� ������Ʈ
        lastAttackTime = Time.time;

        Debug.Log("�������� ������ ������ �õ��մϴ�!");
    }
}