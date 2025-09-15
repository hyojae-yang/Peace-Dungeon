using UnityEngine;
using System;

/// <summary>
/// �κ��丮 ���Կ� ����Ǵ� �������� ������ Ŭ�����Դϴ�.
/// BaseItemSO�� ���� ������ �Բ� �����Ͽ�, �������� �������� �����մϴ�.
/// </summary>
[Serializable]
public class ItemData
{
    // === �������� �⺻ ���� ===
    [Tooltip("�� ������ ������ ��� �ִ� �������� ScriptableObject�Դϴ�.")]
    public BaseItemSO itemSO;

    // === �������� ���� ���� ===
    [Tooltip("�� ������ ���Կ� ���� �׿� �ִ� �������� �����Դϴ�.")]
    public int stackCount;

    /// <summary>
    /// ���ο� ItemData �ν��Ͻ��� �����մϴ�.
    /// </summary>
    public ItemData(BaseItemSO item, int count)
    {
        this.itemSO = item;
        this.stackCount = count;
    }
}