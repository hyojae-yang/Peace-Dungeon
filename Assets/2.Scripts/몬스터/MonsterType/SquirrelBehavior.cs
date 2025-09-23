using UnityEngine;
using System.Collections;

/// <summary>
/// 다람쥐 몬스터의 고유한 행동 로직(도망치기)을 담당하는 클래스입니다.
/// MonsterPatrol 컴포넌트를 제어합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))]
public class SquirrelBehavior : MonoBehaviour
{
    // === 도망 행동 설정 ===
    [Header("도망 행동 설정")]
    [Tooltip("플레이어를 감지했을 때 몬스터가 도망칠 거리입니다.")]
    public float fleeDistance = 15f;
    [Tooltip("플레이어가 시야에서 벗어났을 때 몬스터가 멈출 거리입니다.")]
    public float stopFleeDistance = 20f;
    [Tooltip("도망치는 방향으로의 이동 속도 배율입니다.")]
    public float fleeSpeedMultiplier = 1.5f;

    // === 종속성 ===
    private Monster monster;
    private MonsterPatrol monsterPatrol;
    private Transform playerTransform;

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        if (monster == null || monsterPatrol == null)
        {
            Debug.LogError("SquirrelBehavior: 필수 컴포넌트를 찾을 수 없습니다.");
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
        // 초기 상태를 Patrol로 설정합니다.
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
                // Patrol 상태일 때만 순찰을 시작합니다.
                monsterPatrol.StartPatrol();

                // 플레이어를 감지하면 Flee 상태로 전환하고 순찰을 멈춥니다.
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
                // Idle 상태에서는 순찰도, 도망도 하지 않고 정지합니다.
                monsterPatrol.StopPatrol();
                break;
        }
    }

    /// <summary>
    /// 플레이어로부터 도망치는 로직을 실행합니다.
    /// </summary>
    private void FleeFromPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer < stopFleeDistance)
        {
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            // 몬스터의 기본 이동 속도에 도망 속도 배율을 곱하여 이동시킵니다.
            transform.Translate(fleeDirection * monster.monsterData.moveSpeed * fleeSpeedMultiplier * Time.deltaTime, Space.World);

            Quaternion lookRotation = Quaternion.LookRotation(fleeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            // 충분히 멀리 도망쳤으면 Patrol 상태로 돌아가 순찰을 시작하게 합니다.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }
}