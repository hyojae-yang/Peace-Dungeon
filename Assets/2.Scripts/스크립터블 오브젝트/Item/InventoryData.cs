// InventoryData.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �κ��丮 �� ��� �����͸� ��� ScriptableObject Ŭ�����Դϴ�.
/// �� Ŭ������ ���� ������ ���� ���Ҹ� �ϸ�, ������ �������� �ʽ��ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "InventoryData", menuName = "Data/Inventory Data", order = 1)]
public class InventoryData : ScriptableObject
{
    // === �κ��丮 ������ ===
    // Key: ������ ���� (BaseItemSO), Value: ������ ����
    [Tooltip("�÷��̾��� �κ��丮�� �ִ� ��� �����۰� �� ������ �����մϴ�.")]
    public Dictionary<BaseItemSO, int> inventoryItems = new Dictionary<BaseItemSO, int>();

    // === ���� ��� ������ ===
    // Key: ���� ���� (EquipSlot), Value: ��� ������ ���� (EquipmentItemSO)
    [Tooltip("�÷��̾ ���� �����ϰ� �ִ� ��� �������� �����մϴ�.")]
    public Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();

    /// <summary>
    /// �κ��丮 �� ��� ��ųʸ��� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// �����Ϳ��� �÷��� ��忡 �����ϰų� ������ ������� �� �����͸� �����ϴ� �� ���˴ϴ�.
    /// </summary>
    public void Initialize()
    {
        inventoryItems.Clear();
        equippedItems.Clear();
    }
}