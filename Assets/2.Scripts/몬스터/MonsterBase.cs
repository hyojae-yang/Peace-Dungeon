using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��� ������ ���� ��� Ŭ�����Դϴ�.
/// ��� ���ʹ� �� Ŭ������ ��ӹ޾� ���� �Ӽ��� ����� �����ϴ�.
/// </summary>
public abstract class MonsterBase : MonoBehaviour
{
    // === ���� ���� ������ ===
    public enum MonsterState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee, // <--- ���ο� ���� ���� �߰�
        Dead
    }

    // === ���Ӽ� ===
    [Tooltip("������ �⺻ ���Ȱ� ��� ������ ��� ��ũ���ͺ� ������Ʈ�Դϴ�.")]
    public MonsterData monsterData;

    // === ���� ���� ===
    [HideInInspector]
    public MonsterState currentState = MonsterState.Idle;

    /// <summary>
    /// ������ ���¸� �����ϴ� �޼����Դϴ�.
    /// �� �޼���� �ڽ� Ŭ���������� ���� �����մϴ�.
    /// </summary>
    /// <param name="newState">������ ������ ���ο� ����</param>
    protected void SetState(MonsterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    /// <summary>
    /// ���Ͱ� ������� �� ȣ��Ǵ� �߻� �޼����Դϴ�.
    /// �� ������ Ÿ�Կ� �°� �������̵��ؾ� �մϴ�.
    /// </summary>
    public abstract void Die();
}