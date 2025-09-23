using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// �� ������ ���� ������ �ൿ ������ ����ϴ� Ŭ�����Դϴ�.
/// �����ڵ��� �̲���, ����(Flock) �ൿ�� ���� �ൿ�� �����ϸ�, �ٸ� ������ �պ��մϴ�.
/// </summary>
[RequireComponent(typeof(MonsterPatrol))]
public class RatLeader : RatBehavior
{
    private List<RatFollower> followers = new List<RatFollower>();
    private MonsterPatrol monsterPatrol; // MonsterPatrol ������Ʈ ����

    [Header("���� ����")]
    public float flockingDistance = 3f;
    public float flockingSpeed = 4f;

    [Header("�÷�ŷ �˰��� ����ġ")]
    public float separationWeight = 1.0f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        monsterPatrol = GetComponent<MonsterPatrol>();
        if (monsterPatrol == null)
        {
            Debug.LogError("RatLeader: MonsterPatrol ������Ʈ�� ã�� �� �����ϴ�.");
            enabled = false;
        }
    }

    public override void UpdateBehavior()
    {
        // �� �����Ӹ��� �ֺ� ������ ���������� Ž���ϰ� �պ��մϴ�.
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

        // ������ ���� �÷�ŷ ��꿡 ����
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
            // ������ ������ ���� �������� �̵�
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
        monsterPatrol.SetNewPatrolPoint(); // ���� ��Ż �� ���� ���� �缳��

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