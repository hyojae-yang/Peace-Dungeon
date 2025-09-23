using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 쥐 무리의 리더 몬스터의 행동 로직을 담당하는 클래스입니다.
/// 추종자들을 이끌고, 무리(Flock) 행동과 공격 행동을 관리하며, 다른 무리를 합병합니다.
/// </summary>
[RequireComponent(typeof(MonsterPatrol))]
public class RatLeader : RatBehavior
{
    private List<RatFollower> followers = new List<RatFollower>();
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조

    [Header("무리 관리")]
    public float flockingDistance = 3f;
    public float flockingSpeed = 4f;

    [Header("플로킹 알고리즘 가중치")]
    public float separationWeight = 1.0f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        monsterPatrol = GetComponent<MonsterPatrol>();
        if (monsterPatrol == null)
        {
            Debug.LogError("RatLeader: MonsterPatrol 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }
    }

    public override void UpdateBehavior()
    {
        // 매 프레임마다 주변 무리를 지속적으로 탐색하고 합병합니다.
        CheckAndFormFlock();

        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= monster.detectionRange)
        {
            if (monster.currentState == MonsterBase.MonsterState.Idle || monster.currentState == MonsterBase.MonsterState.Flocking)
            {
                AlertFollowersToAttack();
            }
        }

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StartPatrol();
                break;
            case MonsterBase.MonsterState.Flocking:
                monsterPatrol.StopPatrol();
                HandleFlocking();
                break;
            case MonsterBase.MonsterState.Attack:
                monsterPatrol.StopPatrol();
                HandleAttack();
                break;
        }
    }

    private void CheckAndFormFlock()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, flockDetectionRadius, LayerMask.GetMask("Monster"));

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == this.gameObject) continue;

            RatFollower follower = collider.GetComponent<RatFollower>();
            RatLeader otherLeader = collider.GetComponent<RatLeader>();

            if (otherLeader != null && otherLeader.GetMonster().currentState != MonsterBase.MonsterState.Dead)
            {
                MergeFlock(otherLeader);
                return;
            }
            else if (follower != null && follower.GetMonster().currentState != MonsterBase.MonsterState.Dead)
            {
                AddFollower(follower);
            }
        }
    }

    private void MergeFlock(RatLeader otherLeader)
    {
        if (this.followers.Count >= otherLeader.followers.Count)
        {
            foreach (var follower in otherLeader.followers)
            {
                if (follower != null && follower.GetMonster().currentState != MonsterBase.MonsterState.Dead)
                {
                    AddFollower(follower);
                }
            }
            RatFollower downgradedLeader = otherLeader.gameObject.AddComponent<RatFollower>();
            downgradedLeader.SetLeader(this);
            otherLeader.followers.Clear();
            Destroy(otherLeader);
        }
        else
        {
            otherLeader.MergeFlock(this);
        }
    }

    private void AddFollower(RatFollower rat)
    {
        if (!followers.Contains(rat))
        {
            followers.Add(rat);
            rat.SetLeader(this);
        }
    }

    public void RemoveFollower(RatFollower rat)
    {
        if (followers.Contains(rat))
        {
            followers.Remove(rat);
        }
    }

    private void AlertFollowersToAttack()
    {
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        foreach (var follower in followers)
        {
            if (follower != null && follower.GetMonster().currentState != MonsterBase.MonsterState.Dead)
            {
                follower.ChangeStateToAttack();
            }
        }
    }

    private void HandleFlocking()
    {
        Vector3 finalDirection = CalculateFlockingForce();
        Move(finalDirection, flockingSpeed);

        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > monster.detectionRange + 5f)
        {
            ExitFlock();
        }
    }

    private Vector3 CalculateFlockingForce()
    {
        Vector3 separationVector = Vector3.zero;
        Vector3 alignmentVector = Vector3.zero;
        Vector3 cohesionVector = Vector3.zero;
        int activeRats = 0;

        // 리더의 힘도 플로킹 계산에 포함
        cohesionVector += transform.position;
        alignmentVector += transform.forward;
        activeRats++;

        foreach (var follower in followers)
        {
            if (follower == null || follower.GetMonster().currentState == MonsterBase.MonsterState.Dead) continue;

            activeRats++;
            Vector3 otherRatPos = follower.transform.position;
            Vector3 directionToOther = transform.position - otherRatPos;
            float distance = directionToOther.magnitude;

            if (distance < flockingDistance)
            {
                separationVector += directionToOther.normalized / distance;
            }

            alignmentVector += follower.transform.forward;
            cohesionVector += otherRatPos;
        }

        if (activeRats > 0)
        {
            alignmentVector /= activeRats;
            cohesionVector /= activeRats;
            cohesionVector = (cohesionVector - transform.position).normalized;
        }
        else
        {
            // 무리가 없으면 순찰 방향으로 이동
            if (monsterPatrol != null)
            {
                return (monsterPatrol.GetPatrolPoint() - transform.position).normalized;
            }
            return Vector3.zero;
        }

        return (separationVector * separationWeight + alignmentVector * alignmentWeight + cohesionVector * cohesionWeight).normalized;
    }

    private void HandleAttack()
    {
        if (playerTransform == null)
        {
            ExitFlock();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= monster.detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                PerformAttack();
            }
            else
            {
                Move(playerTransform.position - transform.position, monster.monsterData.moveSpeed);
            }
        }
        else
        {
            monster.ChangeState(MonsterBase.MonsterState.Flocking);
        }
    }

    private void ExitFlock()
    {
        monster.ChangeState(MonsterBase.MonsterState.Idle);
        monsterPatrol.SetNewPatrolPoint(); // 무리 이탈 후 순찰 지점 재설정

        foreach (var follower in followers)
        {
            if (follower != null)
            {
                follower.SetLeader(null);
            }
        }
        followers.Clear();
    }
}