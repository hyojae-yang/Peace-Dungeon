using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ������ ���� �ൿ�� �����ϴ� Ŭ�����Դϴ�.
/// �ڷ�ƾ�� �̿��� ���� ������ �浹 ���� �� ��� �缳�� ����� �����մϴ�.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MonsterPatrol : MonoBehaviour
{
    // === ���� �ൿ ���� ===
    [Header("���� �ൿ ����")]
    [Tooltip("������ �߽��� �Ǵ� �����Դϴ�. (���� ��ǥ)")]
    public Vector3 homePoint;
    [Tooltip("�߽� ������ �������� ������ �ݰ��Դϴ�.")]
    public float patrolRadius = 10f;
    [Tooltip("���� �� �̵� �ӵ��Դϴ�. ������ �⺻ �̵� �ӵ��� ���Ͽ� ���˴ϴ�.")]
    public float patrolSpeedMultiplier = 1f;
    [Tooltip("���ο� ���� ������ �����ϱ� �� ��� �ð��Դϴ�.")]
    public float waitTimeBetweenPatrols = 1f;
    [Tooltip("���� �������� �̵��ϱ� ������ ��ٸ��� �ִ� �ð��Դϴ�. �� �ð��� ������ ��ǥ �������� �������� �ʾҾ �� ������ �����մϴ�.")]
    public float patrolPointChangeInterval = 5f;
    [Tooltip("���� �߽�(HomePoint)�� �����ϴ� �ֱ��Դϴ�. �� �ð��� ������ ���ο� �������� ���� ������ �ű�ϴ�.")]
    public float homePointChangeInterval = 50f;

    // === ���Ӽ� ===
    private Transform monsterTransform;
    private Coroutine patrolCoroutine;
    private MonsterBase monsterBase;

    // === ���� ���� ===
    private Vector3 currentPatrolPoint;
    private float homePointTimer;

    private void Awake()
    {
        monsterTransform = this.transform;
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterPatrol: MonsterBase ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
            return;
        }

        // Ȩ ����Ʈ�� �������� �ʾ����� ������ ���� ��ġ�� ����մϴ�.
        if (homePoint == Vector3.zero)
        {
            homePoint = monsterTransform.position;
        }

        // Ÿ�̸Ӹ� �������� �ʱ�ȭ�Ͽ� ��� ���Ͱ� ���ÿ� ���� ������ �ٲٴ� ���� ����
        homePointTimer = UnityEngine.Random.Range(0, homePointChangeInterval);
    }

    /// <summary>
    /// �ܺο��� ���� �ൿ�� �����ϴ� �޼����Դϴ�.
    /// �̹� ���� ���̸� �ߺ� ������ �����մϴ�.
    /// </summary>
    public void StartPatrol()
    {
        if (patrolCoroutine == null)
        {
            patrolCoroutine = StartCoroutine(PatrolCoroutine());
        }
    }

    /// <summary>
    /// �ܺο��� ���� �ൿ�� ���ߴ� �޼����Դϴ�.
    /// </summary>
    public void StopPatrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
    }

    /// <summary>
    /// ���� ������ �����ϴ� �ڷ�ƾ�Դϴ�.
    /// ��ǥ �������� �̵��ϰ�, ���� �Ǵ� ���� �ð� ��� �� �� �������� �̵��մϴ�.
    /// </summary>
    private IEnumerator PatrolCoroutine()
    {
        SetNewPatrolPoint(); // �ʱ� ���� ���� ����

        float patrolTimer = 0f;
        while (true)
        {
            // ��ǥ ������ ���� �����߰ų�, ���� �ð��� �������� ���ο� ���� ������ �����մϴ�.
            if (Vector3.Distance(monsterTransform.position, currentPatrolPoint) < 1.0f || patrolTimer >= patrolPointChangeInterval)
            {
                // ���� �߽� ���� Ÿ�̸Ӱ� ����Ǹ� ���� �߽ɵ� �Բ� ����
                if (homePointTimer >= homePointChangeInterval)
                {
                    UpdateHomePointAndPatrolPoint();
                }
                else
                {
                    SetNewPatrolPoint();
                }

                patrolTimer = 0f; // ���� ���� Ÿ�̸� ����
                yield return new WaitForSeconds(waitTimeBetweenPatrols);
            }

            // ��ǥ ������ ���� �̵�
            Vector3 direction = (currentPatrolPoint - monsterTransform.position).normalized;
            if (direction != Vector3.zero)
            {
                monsterTransform.position += direction * monsterBase.monsterData.moveSpeed * patrolSpeedMultiplier * Time.deltaTime;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            patrolTimer += Time.deltaTime; // ���� ���� Ÿ�̸� ������Ʈ
            homePointTimer += Time.deltaTime; // Ȩ ����Ʈ Ÿ�̸� ������Ʈ
            yield return null; // ���� �����ӱ��� ���
        }
    }

    /// <summary>
    /// ���� ���� ���� ���ο� ���� ������ �����մϴ�. (���� �߽��� ����)
    /// </summary>
    public void SetNewPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint;

        randomDirection.y = monsterTransform.position.y;
        currentPatrolPoint = randomDirection;
    }

    /// <summary>
    /// ���� �߽��� ���� ��ġ�� �����ϰ� ���ο� ���� ������ �����մϴ�.
    /// </summary>
    private void UpdateHomePointAndPatrolPoint()
    {
        homePoint = monsterTransform.position;
        SetNewPatrolPoint();
        homePointTimer = 0f;
    }

    /// <summary>
    /// �浹�� �߻����� �� ȣ��Ǿ� ���� ������ �߽��� �缳���մϴ�.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // �浹�� ������Ʈ�� �±װ� 'Player' �Ǵ� 'Monster'�� �ƴ� ���� �����մϴ�.
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Monster"))
        {
            StopPatrol();
            UpdateHomePointAndPatrolPoint(); // �浹 �� ���� �߽ɰ� ��ǥ ������ ��� �����մϴ�.
            StartPatrol();
        }
    }

    /// <summary>
    /// ���� ���� ��ǥ ������ ���� ��ȯ�մϴ�.
    /// </summary>
    public Vector3 GetPatrolPoint()
    {
        return currentPatrolPoint;
    }

    private void OnDrawGizmosSelected()
    {
        if (homePoint != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(homePoint, patrolRadius);
        }
    }
}