using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 몬스터의 공통 기반 클래스입니다.
/// 모든 몬스터는 이 클래스를 상속받아 공통 속성과 기능을 가집니다.
/// </summary>
public abstract class MonsterBase : MonoBehaviour
{
    // === 몬스터 상태 열거형 ===
    public enum MonsterState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee, // <--- 새로운 도망 상태 추가
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
    /// 몬스터가 사망했을 때 호출되는 추상 메서드입니다.
    /// 각 몬스터의 타입에 맞게 오버라이드해야 합니다.
    /// </summary>
    public abstract void Die();
}