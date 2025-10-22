using UnityEngine;

/// <summary>
/// 쥐 추종자 몬스터의 행동 로직을 담당하는 클래스입니다.
/// 리더를 단순 추종하며, 이동 및 공격 행동을 관리합니다. (플레이어 감지 시 독립적인 공격 전환 추가)
/// </summary>
public class RatFollower : RatBehavior
{
    private RatLeader leader;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조

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

    /// <summary>
    /// 매 프레임 실행되는 행동 업데이트 로직입니다.
    /// 리더의 상태를 확인하고, **가장 먼저 플레이어 감지 여부를 판단**하여 독립적인 공격 모드 전환을 시도합니다.
    /// </summary>
    public override void UpdateBehavior()
    {
        // 1. 리더 상태 확인 (사망 또는 부재 시 이탈)
        if (leader == null || leader.GetMonster().currentState == MonsterBase.MonsterState.Dead)
        {
            ExitFlock();
            return;
        }

        // 현재 Idle 또는 Flocking 상태일 때만 공격 감지 및 전환을 시도합니다.
        if (playerTransform != null && monster.currentState != MonsterBase.MonsterState.Attack)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) <= monster.detectionRange)
            {
                // 플레이어가 감지 범위 내에 들어오면 즉시 공격 상태로 전환
                ChangeStateToAttack();
            }
        }

        // 3. 현재 상태에 따른 행동 수행
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
                HandleAttack(); // 플레이어 추적 및 공격
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

        // 거리가 너무 가까우면 움직임을 멈춰 덜덜거림을 방지합니다. (이전 논의 참조: 1.0f 이하)
        if (directionToLeader.sqrMagnitude < 1.0f)
        {
            // 가까이 붙었으면 덜덜거리지 않도록 이동을 멈춥니다.
            return;
        }

        // Move() 메서드를 사용하여 이동합니다.
        Move(directionToLeader, monster.monsterData.moveSpeed);
    }

    /// <summary>
    /// 플레이어를 추적하고 공격하는 로직입니다.
    /// </summary>
    private void HandleAttack()
    {
        if (playerTransform == null)
        {
            // 플레이어가 사라지면 즉시 Flocking 상태로 복귀
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
            // 플레이어에게 이동 (추격)
            Move(playerTransform.position - transform.position, monster.monsterData.moveSpeed);
        }

        // 플레이어 감지 범위를 완전히 벗어나면 다시 Flocking 상태로 복귀
        // 리더처럼 너무 멀리 벗어난 경우 (detectionRange + 5f) 복귀
        if (distanceToPlayer > monster.detectionRange + 5f)
        {
            monster.ChangeState(MonsterBase.MonsterState.Flocking);
        }
    }

    /// <summary>
    /// 리더가 사라지거나 죽었을 때 무리에서 이탈하고 순찰 상태로 돌아갑니다.
    /// </summary>
    private void ExitFlock()
    {
        // 리더에게서 자신을 제거하는 로직은 이미 리더 스크립트에 있으므로, 여기서는 자신의 상태만 정리합니다.
        leader = null;
        monster.ChangeState(MonsterBase.MonsterState.Idle);
        monsterPatrol.SetNewPatrolPoint(); // 순찰 지점 재설정
    }

    /// <summary>
    /// 리더로부터 새로운 리더를 설정받고 Flocking 상태로 전환합니다. (LSP 준수)
    /// </summary>
    /// <param name="newLeader">새로운 리더 객체</param>
    public void SetLeader(RatLeader newLeader)
    {
        // Null이 아니거나, 이미 리더를 따르지 않는 경우에만 상태를 전환하여 불필요한 호출 방지
        if (newLeader != null && leader != newLeader)
        {
            leader = newLeader;
            // 리더를 설정받으면 기본적으로 추종 상태로 전환
            monster.ChangeState(MonsterBase.MonsterState.Flocking);
        }
    }

    /// <summary>
    /// 리더가 플레이어 감지 시 호출하는 공격 상태 전환 명령입니다.
    /// (추종자가 스스로 감지하는 로직이 추가되었으나, 리더의 명령도 유지)
    /// </summary>
    public void ChangeStateToAttack()
    {
        if (monster.currentState != MonsterBase.MonsterState.Attack)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }
}