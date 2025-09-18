using UnityEngine;
using System.Collections;

/// <summary>
/// 다람쥐 몬스터의 고유한 행동 로직(도망치기 및 순찰)을 담당하는 클래스입니다.
/// MonsterBase의 상태를 관찰하며 특별한 행동을 실행합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
public class SquirrelBehavior : MonoBehaviour
{
    [Header("도망 행동 설정")]
    [Tooltip("플레이어를 감지했을 때 몬스터가 도망칠 거리입니다.")]
    public float fleeDistance = 15f;
    [Tooltip("플레이어가 시야에서 벗어났을 때 몬스터가 멈출 거리입니다.")]
    public float stopFleeDistance = 20f;
    [Tooltip("도망치는 방향으로의 이동 속도 배율입니다.")]
    public float fleeSpeedMultiplier = 1.5f;

    [Header("순찰 행동 설정")]
    [Tooltip("순찰의 중심이 되는 지점입니다. 몬스터의 초기 위치를 사용하거나 별도 오브젝트를 지정할 수 있습니다.")]
    public Transform homePoint;
    [Tooltip("중심 지점을 기준으로 순찰할 반경입니다.")]
    public float patrolRadius = 10f;
    [Tooltip("순찰 시 이동 속도입니다.")]
    public float patrolSpeed = 3f;

    // === 순찰 관련 변수 ===
    private Vector3 currentPatrolPoint;

    // === 종속성 ===
    private Monster monster;
    private Transform playerTransform;

    void Awake()
    {
        // 동일 게임 오브젝트에 있는 Monster 컴포넌트를 가져옵니다.
        monster = GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError("SquirrelBehavior: Monster 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        // 플레이어 트랜스폼을 한 번만 찾아서 캐싱합니다.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // 홈 포인트가 지정되지 않았으면 몬스터의 시작 위치를 사용합니다.
        if (homePoint == null)
        {
            homePoint = this.transform;
        }

        // 초기 순찰 지점을 설정합니다.
        SetNewPatrolPoint();
    }

    void Update()
    {
        // 플레이어가 없거나 몬스터가 죽은 상태라면 로직을 실행하지 않습니다.
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        // Monster 스크립트의 현재 상태를 확인합니다.
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                // Idle 상태일 때만 플레이어를 감지하고 순찰합니다.
                DetectAndPatrol();
                break;

            case MonsterBase.MonsterState.Flee:
                FleeFromPlayer();
                break;
        }
    }

    /// <summary>
    /// Idle 상태에서 플레이어를 감지하거나 순찰하는 메서드입니다.
    /// </summary>
    private void DetectAndPatrol()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 1. 플레이어를 감지하면 Flee 상태로 전환합니다.
        if (distanceToPlayer < fleeDistance)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flee);
        }
        // 2. 플레이어가 감지되지 않으면 순찰합니다.
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// 순찰 로직을 실행합니다.
    /// </summary>
    private void Patrol()
    {
        // 현재 위치에서 목표 지점까지의 거리를 계산합니다.
        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolPoint);

        // 목표 지점에 거의 도착했다면 새로운 순찰 지점을 설정합니다.
        if (distanceToTarget < 1.0f) // 1.0f는 도착했다고 판단하는 임계값
        {
            SetNewPatrolPoint();
        }

        // 목표 지점을 향해 이동합니다.
        transform.position = Vector3.MoveTowards(transform.position, currentPatrolPoint, patrolSpeed * Time.deltaTime);

        // 이동 방향으로 몸을 회전시킵니다.
        Vector3 direction = (currentPatrolPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 순찰 범위 내에 새로운 랜덤 지점을 설정합니다.
    /// </summary>
    private void SetNewPatrolPoint()
    {
        // 중심점에서 랜덤한 방향으로 반경 내의 새로운 지점을 찾습니다.
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint.position;

        // y축은 그대로 유지하여 지상에서만 순찰하도록 합니다.
        randomDirection.y = transform.position.y;

        currentPatrolPoint = randomDirection;
    }

    /// <summary>
    /// 플레이어로부터 도망치는 로직을 실행합니다.
    /// </summary>
    private void FleeFromPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 플레이어가 도망쳐야 할 범위 안에 있다면
        if (distanceToPlayer < stopFleeDistance)
        {
            // 플레이어로부터 멀어지는 방향을 계산합니다.
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;

            // 몬스터의 이동 속도에 도망 속도 배율을 곱하여 이동시킵니다.
            transform.Translate(fleeDirection * monster.monsterData.moveSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);

            // 도망치는 방향으로 몸을 돌립니다.
            Quaternion lookRotation = Quaternion.LookRotation(fleeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        // 플레이어에게서 충분히 멀리 도망쳤다면
        else
        {
            // Idle 상태로 돌아가 순찰을 시작하게 합니다.
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