using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 모든 몬스터의 공통 기반 클래스입니다.
/// 모든 몬스터는 이 클래스를 상속받아 공통 속성과 기능을 가집니다.
/// </summary>
public abstract class MonsterBase : MonoBehaviour
{
    /// <summary>
    /// **정적 이벤트:** 모든 몬스터가 사망했을 때 호출됩니다.
    /// 인수는 사망한 몬스터의 고유 ID입니다.
    /// QuestManager와 같은 외부 시스템이 몬스터 처치 이벤트를 안전하게 감지하는 데 사용됩니다.
    /// </summary>
    public static event Action<int> OnAnyMonsterKilled;
    // === 몬스터 상태 열거형 ===
    public enum MonsterState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Charge,// <--- 새로운 기모으기 상태 추가
        Flocking, // <--- 추가: 무리 짓기 상태
        Dead
    }

    // === 종속성 ===
    [Tooltip("몬스터의 기본 스탯과 드롭 정보를 담는 스크립터블 오브젝트입니다.")]
    public MonsterData monsterData;

    // === 상태 변수 ===
    [HideInInspector]
    public MonsterState currentState = MonsterState.Idle;


    /// <summary>
    /// 몬스터의 상태를 변경하는 메서드입니다.
    /// 이 메서드는 자식 클래스에서만 접근 가능합니다.
    /// </summary>
    /// <param name="newState">변경할 몬스터의 새로운 상태</param>
    protected void SetState(MonsterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }
    /// <summary>
    /// 자식 클래스(예: Monster.cs)가 몬스터 사망 이벤트를 안전하게 호출할 수 있도록 제공하는 메서드입니다.
    /// 이벤트를 캡슐화하고 CS0070 오류를 방지합니다.
    /// </summary>
    /// <param name="monsterID">사망한 몬스터의 고유 ID입니다.</param>
    protected void RaiseMonsterKilledEvent(int monsterID)
    {
        // 구독자가 있는지 확인하고 안전하게 이벤트를 호출합니다.
        OnAnyMonsterKilled?.Invoke(monsterID);
    }
    /// <summary>
    /// 몬스터가 사망했을 때 호출되는 추상 메서드입니다.
    /// 각 몬스터의 타입에 맞게 오버라이드해야 합니다.
    /// </summary>
    public abstract void Die();
}