using UnityEngine;
using System.Collections;

/// <summary>
/// �ٶ��� ������ ������ �ൿ ����(����ġ��)�� ����ϴ� Ŭ�����Դϴ�.
/// MonsterPatrol ������Ʈ�� �����մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))]
public class SquirrelBehavior : MonoBehaviour
{
    // === ���� �ൿ ���� ===
    [Header("���� �ൿ ����")]
    [Tooltip("�÷��̾ �������� �� ���Ͱ� ����ĥ �Ÿ��Դϴ�.")]
    public float fleeDistance = 15f;
    [Tooltip("�÷��̾ �þ߿��� ����� �� ���Ͱ� ���� �Ÿ��Դϴ�.")]
    public float stopFleeDistance = 20f;
    [Tooltip("����ġ�� ���������� �̵� �ӵ� �����Դϴ�.")]
    public float fleeSpeedMultiplier = 1.5f;

    // === ���Ӽ� ===
    private Monster monster;
    private MonsterPatrol monsterPatrol;
    private Transform playerTransform;

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        if (monster == null || monsterPatrol == null)
        {
            Debug.LogError("SquirrelBehavior: �ʼ� ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void Start()
    {
        // �ʱ� ���¸� Patrol�� �����մϴ�.
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
    }
    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // Patrol ������ ���� ������ �����մϴ�.
                monsterPatrol.StartPatrol();

                // �÷��̾ �����ϸ� Flee ���·� ��ȯ�ϰ� ������ ����ϴ�.
                if (distanceToPlayer < fleeDistance)
                {
                    monster.ChangeState(MonsterBase.MonsterState.Flee);
                    monsterPatrol.StopPatrol();
                }
                break;

            case MonsterBase.MonsterState.Flee:
                FleeFromPlayer(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                // Idle ���¿����� ������, ������ ���� �ʰ� �����մϴ�.
                monsterPatrol.StopPatrol();
                break;
        }
    }

    /// <summary>
    /// �÷��̾�κ��� ����ġ�� ������ �����մϴ�.
    /// </summary>
    private void FleeFromPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer < stopFleeDistance)
        {
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            // ������ �⺻ �̵� �ӵ��� ���� �ӵ� ������ ���Ͽ� �̵���ŵ�ϴ�.
            transform.Translate(fleeDirection * monster.monsterData.moveSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);

            Quaternion lookRotation = Quaternion.LookRotation(fleeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            // ����� �ָ� ���������� Patrol ���·� ���ư� ������ �����ϰ� �մϴ�.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }
}