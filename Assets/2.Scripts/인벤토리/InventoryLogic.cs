using System.Collections.Generic;

/// <summary>
/// �κ��丮�� ��� �ٽ� ������ ó���ϴ� Ŭ�����Դϴ�.
/// InventoryData�� ���� �����ϸ�, MonoBehaviour�� �ƴմϴ�.
/// </summary>
public class InventoryLogic
{
    // === �κ��丮�� �������� �߰��ϴ� ���� ===

    /// <summary>
    /// �κ��丮�� �������� �߰��մϴ�.
    /// �������� maxStack�� ����Ͽ� ��ġ�� �� �� ���� �߰��� ó���մϴ�.
    /// </summary>
    /// <param name="data">������ �����͸� ��� �ִ� InventoryData ScriptableObject�Դϴ�.</param>
    /// <param name="itemToAdd">�߰��� ������ �����Դϴ�.</param>
    /// <param name="amount">�߰��� �������� �����Դϴ�.</param>
    /// <param name="inventorySize">�κ��丮�� �ִ� ���� �����Դϴ�.</param>
    /// <returns>������ �߰��� �����ߴ��� ���θ� ��ȯ�մϴ�.</returns>
    public bool AddItem(InventoryData data, BaseItemSO itemToAdd, int amount, int inventorySize)
    {
        // 1. ������ �� �ִ� ������ ������ ã���ϴ�. (maxStack�� 1���� ū ���)
        // ��� �������� �������� �����Ƿ� �� ������ �ǳʶݴϴ�.
        for (int i = 0; i < data.inventoryItems.Count; i++)
        {
            if (data.inventoryItems[i].itemSO == itemToAdd && data.inventoryItems[i].stackCount < itemToAdd.maxStack)
            {
                // 2. ���� �������� �ְ�, ���� ���� ���� �ʾҴٸ� ������ ������Ʈ�մϴ�.
                data.inventoryItems[i].stackCount += amount;
                return true;
            }
        }

        // 3. �κ��丮�� �� á���� Ȯ���մϴ�.
        if (data.inventoryItems.Count >= inventorySize)
        {
            return false;
        }

        // 4. ������ ���� �������̰ų�, ��ĥ �� ���� ������(��� ��)�̶�� ���� �߰��մϴ�.
        data.inventoryItems.Add(new ItemData(itemToAdd, amount));
        return true;
    }

    // === �κ��丮���� �������� �����ϴ� ���� ===

    /// <summary>
    /// �κ��丮���� �������� �����մϴ�.
    /// </summary>
    /// <param name="data">������ �����͸� ��� �ִ� InventoryData ScriptableObject�Դϴ�.</param>
    /// <param name="itemToRemove">������ ������ �����Դϴ�.</param>
    /// <param name="amount">������ �������� �����Դϴ�.</param>
    /// <returns>������ ���ſ� �����ߴ��� ���θ� ��ȯ�մϴ�.</returns>
    public bool RemoveItem(InventoryData data, BaseItemSO itemToRemove, int amount)
    {
        // ������ �������� �κ��丮���� ã���ϴ�.
        // ���� ���Կ� ������ ���� �� �����Ƿ�, ��� ����� ������ ã���ϴ�.
        for (int i = 0; i < data.inventoryItems.Count; i++)
        {
            if (data.inventoryItems[i].itemSO == itemToRemove)
            {
                // 2. ������ ������� Ȯ���մϴ�.
                if (data.inventoryItems[i].stackCount >= amount)
                {
                    // 3. ������ �����ϰ�, 0�� �Ǹ� ������ �����մϴ�.
                    data.inventoryItems[i].stackCount -= amount;
                    if (data.inventoryItems[i].stackCount <= 0)
                    {
                        data.inventoryItems.RemoveAt(i);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    // === ��� ������ ���� �� ���� ���� ===

    /// <summary>
    /// �κ��丮�� ��� �������� �����մϴ�.
    /// </summary>
    /// <param name="data">������ �����͸� ��� �ִ� InventoryData ScriptableObject�Դϴ�.</param>
    /// <param name="itemToEquip">������ ��� ������ �����Դϴ�.</param>
    /// <param name="inventorySize">�κ��丮�� �ִ� ���� �����Դϴ�.</param>
    public void EquipItem(InventoryData data, EquipmentItemSO itemToEquip, int inventorySize)
    {
        // ���� ������ ���Կ� �̹� �������� �ִ��� Ȯ���մϴ�.
        if (data.equippedItems.ContainsKey(itemToEquip.equipSlot))
        {
            // �̹� ��� �ִٸ�, ���� ��� �����ϰ� �κ��丮�� �ǵ����ϴ�.
            UnEquipItem(data, itemToEquip.equipSlot, inventorySize);
        }

        // �κ��丮���� �������� �����մϴ�. (���� �� �κ��丮���� ������� �ϹǷ�)
        RemoveItem(data, itemToEquip, 1);

        // ��� ���Կ� ���ο� �������� �߰��մϴ�.
        data.equippedItems.Add(itemToEquip.equipSlot, itemToEquip);
    }

    /// <summary>
    /// ������ �������� �����ϰ� �κ��丮�� �ǵ����ϴ�.
    /// </summary>
    /// <param name="data">������ �����͸� ��� �ִ� InventoryData ScriptableObject�Դϴ�.</param>
    /// <param name="slotToUnEquip">������ ��� ������ Ÿ���Դϴ�.</param>
    /// <param name="inventorySize">�κ��丮�� �ִ� ���� �����Դϴ�.</param>
    public void UnEquipItem(InventoryData data, EquipSlot slotToUnEquip, int inventorySize)
    {
        if (data.equippedItems.ContainsKey(slotToUnEquip))
        {
            EquipmentItemSO itemToUnEquip = data.equippedItems[slotToUnEquip];

            // ������ �������� �κ��丮�� �ǵ����ϴ�.
            AddItem(data, itemToUnEquip, 1, inventorySize);

            // ��� ���Կ��� �������� �����մϴ�.
            data.equippedItems.Remove(slotToUnEquip);
        }
    }
}