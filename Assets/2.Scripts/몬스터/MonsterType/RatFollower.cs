using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 쥐 추종자 몬스터의 행동 로직을 담당하는 클래스입니다.
/// 리더를 단순 추종하며, 이동 및 공격 행동을 관리합니다. (플로킹 로직 제거)
/// </summary>
public class RatFollower : RatBehavior
{
    private RatLeader leader;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조

    // ⭐️ 플로킹 관련 필드 모두 제거 (스크립트 간소화)

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
            return;
        }

        // 리더가 살아있으면 상태에 따른 행동 수행
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StartPatrol();
                break;
            case MonsterBase.MonsterState.Flocking:
                monsterPatrol.StopPatrol(); // 추종 상태 중 순찰 중지
                HandleFlocking(); // 리더를 향해 단순 이동
                break;
            case MonsterBase.MonsterState.Attack:
                monsterPatrol.StopPatrol(); // 공격 중 순찰 중지
                HandleAttack();
                break;
        }
    }

    /// <summary>
    /// [최종 간소화 로직] 리더를 향해 단순 이동합니다.
    /// </summary>
    private void HandleFlocking()
    {
        // 리더의 위치로 이동할 방향을 계산합니다.
        Vector3 directionToLeader = leader.transform.position - transform.position;

        // 거리가 너무 가까우면 움직임을 멈춰 덜덜거림을 방지합니다.
        // ⭐️ [덜덜거림 방지]: 리더와의 거리가 1.0f 이하이면 이동하지 않습니다.
        if (directionToLeader.sqrMagnitude < 1.0f)
        {
            // 가까이 붙었으면 덜덜거리지 않도록 이동을 멈춥니다.
            return;
        }

        // Move() 메서드를 사용하여 이동합니다.
        Move(directionToLeader, monster.monsterData.moveSpeed);
    }

    // ⭐️ CalculateFlockingForce() 메서드는 완전히 제거되었습니다.

    private void HandleAttack()
    {
        // ... (기존 로직 유지)
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
        monsterPatrol.SetNewPatrolPoint();
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