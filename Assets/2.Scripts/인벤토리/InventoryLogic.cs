// InventoryLogic.cs
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
    /// </summary>
    /// <param name="data">������ �����͸� ��� �ִ� InventoryData ScriptableObject�Դϴ�.</param>
    /// <param name="itemToAdd">�߰��� ������ �����Դϴ�.</param>
    /// <param name="amount">�߰��� �������� �����Դϴ�.</param>
    public void AddItem(InventoryData data, BaseItemSO itemToAdd, int amount)
    {
        // �̹� �κ��丮�� ���� �������� �ִ��� Ȯ���մϴ�.
        if (data.inventoryItems.ContainsKey(itemToAdd))
        {
            // �������� �̹� �ִٸ� ������ �÷��ݴϴ�.
            data.inventoryItems[itemToAdd] += amount;
            // TODO: UI�� ������Ʈ�� �˸��� �̺�Ʈ ȣ�� ���� �߰� ����
        }
        else
        {
            // �������� ���ٸ� ���� �߰��մϴ�.
            data.inventoryItems.Add(itemToAdd, amount);
            // TODO: UI�� ������Ʈ�� �˸��� �̺�Ʈ ȣ�� ���� �߰� ����
        }
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
        // �κ��丮�� �ش� �������� �ִ��� Ȯ���մϴ�.
        if (data.inventoryItems.ContainsKey(itemToRemove))
        {
            // ������ ������� Ȯ���մϴ�.
            if (data.inventoryItems[itemToRemove] >= amount)
            {
                data.inventoryItems[itemToRemove] -= amount;

                // ������ ������ 0�� �Ǹ� ��ųʸ����� ������ �����մϴ�.
                if (data.inventoryItems[itemToRemove] <= 0)
                {
                    data.inventoryItems.Remove(itemToRemove);
                }

                // TODO: UI�� ������Ʈ�� �˸��� �̺�Ʈ ȣ�� ���� �߰� ����
                return true;
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
    public void EquipItem(InventoryData data, EquipmentItemSO itemToEquip)
    {
        // ���� ������ ���Կ� �̹� �������� �ִ��� Ȯ���մϴ�.
        if (data.equippedItems.ContainsKey(itemToEquip.equipSlot))
        {
            // �̹� ��� �ִٸ�, ���� ��� �����ϰ� �κ��丮�� �ǵ����ϴ�.
            UnEquipItem(data, itemToEquip.equipSlot);
        }

        // �κ��丮���� �������� �����մϴ�. (���� �� �κ��丮���� ������� �ϹǷ�)
        RemoveItem(data, itemToEquip, 1);

        // ��� ���Կ� ���ο� �������� �߰��մϴ�.
        data.equippedItems.Add(itemToEquip.equipSlot, itemToEquip);

        // TODO: UI�� ������Ʈ�� �˸��� �̺�Ʈ ȣ�� ���� �߰� ����
    }

    /// <summary>
    /// ������ �������� �����ϰ� �κ��丮�� �ǵ����ϴ�.
    /// </summary>
    /// <param name="data">������ �����͸� ��� �ִ� InventoryData ScriptableObject�Դϴ�.</param>
    /// <param name="slotToUnEquip">������ ��� ������ Ÿ���Դϴ�.</param>
    public void UnEquipItem(InventoryData data, EquipSlot slotToUnEquip)
    {
        if (data.equippedItems.ContainsKey(slotToUnEquip))
        {
            EquipmentItemSO itemToUnEquip = data.equippedItems[slotToUnEquip];

            // ������ �������� �κ��丮�� �ǵ����ϴ�.
            AddItem(data, itemToUnEquip, 1);

            // ��� ���Կ��� �������� �����մϴ�.
            data.equippedItems.Remove(slotToUnEquip);

            // TODO: UI�� ������Ʈ�� �˸��� �̺�Ʈ ȣ�� ���� �߰� ����
        }
    }
}