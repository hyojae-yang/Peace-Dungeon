using UnityEngine;
using System.Collections;

/// <summary>
/// �ٶ��� ������ ������ �ൿ ����(����ġ�� �� ����)�� ����ϴ� Ŭ�����Դϴ�.
/// MonsterBase�� ���¸� �����ϸ� Ư���� �ൿ�� �����մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
public class SquirrelBehavior : MonoBehaviour
{
    [Header("���� �ൿ ����")]
    [Tooltip("�÷��̾ �������� �� ���Ͱ� ����ĥ �Ÿ��Դϴ�.")]
    public float fleeDistance = 15f;
    [Tooltip("�÷��̾ �þ߿��� ����� �� ���Ͱ� ���� �Ÿ��Դϴ�.")]
    public float stopFleeDistance = 20f;
    [Tooltip("����ġ�� ���������� �̵� �ӵ� �����Դϴ�.")]
    public float fleeSpeedMultiplier = 1.5f;

    [Header("���� �ൿ ����")]
    [Tooltip("������ �߽��� �Ǵ� �����Դϴ�. ������ �ʱ� ��ġ�� ����ϰų� ���� ������Ʈ�� ������ �� �ֽ��ϴ�.")]
    public Transform homePoint;
    [Tooltip("�߽� ������ �������� ������ �ݰ��Դϴ�.")]
    public float patrolRadius = 10f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�.")]
    public float patrolSpeed = 3f;

    // === ���� ���� ���� ===
    private Vector3 currentPatrolPoint;

    // === ���Ӽ� ===
    private Monster monster;
    private Transform playerTransform;

    void Awake()
    {
        // ���� ���� ������Ʈ�� �ִ� Monster ������Ʈ�� �����ɴϴ�.
        monster = GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError("SquirrelBehavior: Monster ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        // �÷��̾� Ʈ�������� �� ���� ã�Ƽ� ĳ���մϴ�.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // Ȩ ����Ʈ�� �������� �ʾ����� ������ ���� ��ġ�� ����մϴ�.
        if (homePoint == null)
        {
            homePoint = this.transform;
        }

        // �ʱ� ���� ������ �����մϴ�.
        SetNewPatrolPoint();
    }

    void Update()
    {
        // �÷��̾ ���ų� ���Ͱ� ���� ���¶�� ������ �������� �ʽ��ϴ�.
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        // Monster ��ũ��Ʈ�� ���� ���¸� Ȯ���մϴ�.
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                // Idle ������ ���� �÷��̾ �����ϰ� �����մϴ�.
                DetectAndPatrol();
                break;

            case MonsterBase.MonsterState.Flee:
                FleeFromPlayer();
                break;
        }
    }

    /// <summary>
    /// Idle ���¿��� �÷��̾ �����ϰų� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void DetectAndPatrol()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 1. �÷��̾ �����ϸ� Flee ���·� ��ȯ�մϴ�.
        if (distanceToPlayer < fleeDistance)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flee);
        }
        // 2. �÷��̾ �������� ������ �����մϴ�.
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void Patrol()
    {
        // ���� ��ġ���� ��ǥ ���������� �Ÿ��� ����մϴ�.
        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolPoint);

        // ��ǥ ������ ���� �����ߴٸ� ���ο� ���� ������ �����մϴ�.
        if (distanceToTarget < 1.0f) // 1.0f�� �����ߴٰ� �Ǵ��ϴ� �Ӱ谪
        {
            SetNewPatrolPoint();
        }

        // ��ǥ ������ ���� �̵��մϴ�.
        transform.position = Vector3.MoveTowards(transform.position, currentPatrolPoint, patrolSpeed * Time.deltaTime);

        // �̵� �������� ���� ȸ����ŵ�ϴ�.
        Vector3 direction = (currentPatrolPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// ���� ���� ���� ���ο� ���� ������ �����մϴ�.
    /// </summary>
    private void SetNewPatrolPoint()
    {
        // �߽������� ������ �������� �ݰ� ���� ���ο� ������ ã���ϴ�.
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint.position;

        // y���� �״�� �����Ͽ� ���󿡼��� �����ϵ��� �մϴ�.
        randomDirection.y = transform.position.y;

        currentPatrolPoint = randomDirection;
    }

    /// <summary>
    /// �÷��̾�κ��� ����ġ�� ������ �����մϴ�.
    /// </summary>
    private void FleeFromPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // �÷��̾ �����ľ� �� ���� �ȿ� �ִٸ�
        if (distanceToPlayer < stopFleeDistance)
        {
            // �÷��̾�κ��� �־����� ������ ����մϴ�.
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;

            // ������ �̵� �ӵ��� ���� �ӵ� ������ ���Ͽ� �̵���ŵ�ϴ�.
            transform.Translate(fleeDirection * monster.monsterData.moveSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);

            // ����ġ�� �������� ���� �����ϴ�.
            Quaternion lookRotation = Quaternion.LookRotation(fleeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        // �÷��̾�Լ� ����� �ָ� �����ƴٸ�
        else
        {
            // Idle ���·� ���ư� ������ �����ϰ� �մϴ�.
            monster.ChangeState(MonsterBase.MonsterState.Idle);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (homePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(homePoint.position, patrolRadius);
        }
    }
}