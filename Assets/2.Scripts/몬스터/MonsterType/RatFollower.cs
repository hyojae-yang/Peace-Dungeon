using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 쥐 추종자 몬스터의 행동 로직을 담당하는 클래스입니다.
/// 리더를 추종하며, 무리(Flock) 행동과 공격 행동을 관리합니다.
/// </summary>
[RequireComponent(typeof(MonsterPatrol))] // MonsterPatrol 컴포넌트 의존성 명시
public class RatFollower : RatBehavior
{
    private RatLeader leader;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조

    [Header("플로킹 설정")]
    [Tooltip("추종자들이 서로 유지하려는 최소 거리입니다.")]
    public float separationDistance = 1.5f;
    [Tooltip("분리, 정렬, 응집 힘의 가중치입니다.")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        monsterPatrol = GetComponent<MonsterPatrol>();
        if (monsterPatrol == null)
        {
            Debug.LogError("RatFollower: MonsterPatrol 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }
    }

    public override void UpdateBehavior()
    {
        // 리더가 존재하지 않거나 죽었으면 무리에서 이탈
        if (leader == null || leader.GetMonster().currentState == MonsterBase.MonsterState.Dead)
        {
            ExitFlock();
        }
        else
        {
            // 리더가 살아있으면 상태에 따른 행동 수행
            switch (monster.currentState)
            {
                case MonsterBase.MonsterState.Idle:
                    // 리더가 지정되지 않은 상태에서 순찰
                    monsterPatrol.StartPatrol();
                    break;
                case MonsterBase.MonsterState.Flocking:
                    monsterPatrol.StopPatrol(); // 무리 행동 중 순찰 중지
                    HandleFlocking();
                    break;
                case MonsterBase.MonsterState.Attack:
                    monsterPatrol.StopPatrol(); // 공격 중 순찰 중지
                    HandleAttack();
                    break;
            }
        }
    }

    private void HandleFlocking()
    {
        // 리더가 무리 상태일 때만 무리 행동을 수행합니다.
        if (leader.GetMonster().currentState == MonsterBase.MonsterState.Flocking)
        {
            Vector3 finalDirection = CalculateFlockingForce();
            Move(finalDirection, monster.monsterData.moveSpeed);
        }
        else
        {
            // 리더가 Idle 또는 Attack 상태일 때는 리더의 위치로 이동
            Move(leader.transform.position - transform.position, monster.monsterData.moveSpeed);
        }
    }

    private Vector3 CalculateFlockingForce()
    {
        Vector3 separationVector = Vector3.zero;
        Vector3 alignmentVector = Vector3.zero;
        Vector3 cohesionVector = Vector3.zero;
        int neighborCount = 0;

        Collider[] colliders = Physics.OverlapSphere(transform.position, flockDetectionRadius, LayerMask.GetMask("Monster"));

        foreach (var collider in colliders)
        {
            if (collider.gameObject == this.gameObject) continue;

            RatFollower otherFollower = collider.GetComponent<RatFollower>();
            if (otherFollower != null)
            {
                neighborCount++;
                Vector3 directionToOther = transform.position - otherFollower.transform.position;
                float distance = directionToOther.magnitude;

                if (distance < separationDistance)
                {
                    separationVector += directionToOther.normalized * (separationDistance / distance);
                }

                alignmentVector += otherFollower.transform.forward;
            }
        }

        cohesionVector = (leader.transform.position - transform.position).normalized;

        if (neighborCount > 0)
        {
            alignmentVector /= neighborCount;
            separationVector /= neighborCount;
        }

        Vector3 finalDirection = separationVector * separationWeight +
                                 alignmentVector * alignmentWeight +
                                 cohesionVector * cohesionWeight;

        if (finalDirection.magnitude < 0.1f)
        {
            finalDirection = cohesionVector;
        }

        return finalDirection.normalized;
    }

    private void HandleAttack()
    {
        if (playerTransform == null)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flocking);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            PerformAttack();
        }
        else
        {
            Move(playerTransform.position - transform.position, monster.monsterData.moveSpeed);
        }

        if (distanceToPlayer > monster.detectionRange + 5f)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flocking);
        }
    }

    private void ExitFlock()
    {
        leader = null;
        monster.ChangeState(MonsterBase.MonsterState.Idle);
        monsterPatrol.SetNewPatrolPoint(); // 무리 이탈 후 순찰 지점을 재설정합니다.
    }

    public void SetLeader(RatLeader newLeader)
    {
        leader = newLeader;
        monster.ChangeState(MonsterBase.MonsterState.Flocking);
    }

    public void ChangeStateToAttack()
    {
        monster.ChangeState(MonsterBase.MonsterState.Attack);
    }
}