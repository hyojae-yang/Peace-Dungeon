// Inventory.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �κ��丮�� �� �����۰� ������ �� ������ ���� Ŭ�����Դϴ�.
/// Ŭ������ �����Ͽ� ����Ʈ ���� ��ü�� ���� ������ �� �ְ� �մϴ�.
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public BaseItemSO itemData;
    public int stackSize;

    /// <summary>
    /// �����۰� ������ �����Ͽ� �ʱ�ȭ�ϴ� �������Դϴ�.
    /// </summary>
    public InventorySlot(BaseItemSO item, int quantity)
    {
        this.itemData = item;
        this.stackSize = quantity;
    }
}

/// <summary>
/// �÷��̾��� ������ ����ǰ�� �����ϴ� �κ��丮 �ý��� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Tooltip("�κ��丮�� �ִ� ���� �����Դϴ�.")]
    public int inventorySize = 20;

    [Tooltip("�÷��̾ ���� �����ϰ� �ִ� ������ ����Դϴ�.")]
    public List<InventorySlot> inventorySlots;

    void Awake()
    {
        // ���� ���� �� �κ��丮 ���� ����Ʈ�� null�� ä�� �ʱ�ȭ�մϴ�.
        inventorySlots = new List<InventorySlot>(new InventorySlot[inventorySize]);
    }

    /// <summary>
    /// �������� �κ��丮�� �߰��ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="itemToAdd">�߰��� ������ ������</param>
    /// <param name="quantity">�߰��� ����</param>
    /// <returns>�������� ��� �߰��Ǿ����� ����</returns>
    public bool AddItem(BaseItemSO itemToAdd, int quantity)
    {
        // �Ҹ�ǰ �� ��� �������� ���� ���Կ� ��ĥ �� �ִ��� Ȯ���մϴ�.
        // ����� ConsumableItemSO���� maxStackCount�� ���������Ƿ�,
        // �ش� ������ Ÿ�Կ��� ���� ������ �����մϴ�.
        if (itemToAdd.itemType == ItemType.Consumable)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                if (inventorySlots[i] != null && inventorySlots[i].itemData != null && inventorySlots[i].itemData.itemID == itemToAdd.itemID)
                {
                    int maxStack = ((ConsumableItemSO)itemToAdd).maxStackCount;
                    if (inventorySlots[i].stackSize < maxStack)
                    {
                        int spaceLeft = maxStack - inventorySlots[i].stackSize;
                        int addAmount = Mathf.Min(quantity, spaceLeft);

                        inventorySlots[i].stackSize += addAmount; // Ŭ�����̹Ƿ� ���� ���� ����

                        quantity -= addAmount;
                        if (quantity <= 0)
                        {
                            Debug.Log($"{itemToAdd.itemName} {addAmount}���� �κ��丮�� �߰��߽��ϴ�.");
                            return true;
                        }
                    }
                }
            }
        }

        // ���� ���Կ� ��ġ�� ���߰ų� ��� �������� ���, �� ������ ã�� �߰��մϴ�.
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] == null || inventorySlots[i].itemData == null)
            {
                inventorySlots[i] = new InventorySlot(itemToAdd, quantity); // ���ο� InventorySlot �ν��Ͻ� ����
                Debug.Log($"{itemToAdd.itemName} {quantity}���� ���ο� ���Կ� �߰��߽��ϴ�.");
                return true;
            }
        }

        Debug.LogWarning("�κ��丮�� ���� ���� �������� �߰��� �� �����ϴ�!");
        return false;
    }

    /// <summary>
    /// �κ��丮���� Ư�� ������ �������� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="slotIndex">������ ������ �ε���</param>
    /// <param name="quantity">������ ����</param>
    public void RemoveItem(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.LogError("�߸��� ���� �ε����Դϴ�.");
            return;
        }

        if (inventorySlots[slotIndex] == null || inventorySlots[slotIndex].itemData == null)
        {
            Debug.LogWarning("�ش� ���Կ� �������� �����ϴ�.");
            return;
        }

        // ������ ������ �� ��, 0 ���ϰ� �Ǹ� ������ ���ϴ�.
        inventorySlots[slotIndex].stackSize -= quantity;
        if (inventorySlots[slotIndex].stackSize <= 0)
        {
            Debug.Log($"{inventorySlots[slotIndex].itemData.itemName}��(��) �κ��丮���� ���ŵǾ����ϴ�.");
            inventorySlots[slotIndex] = null; // null�� �Ҵ��Ͽ� ������ ���ϴ�.
        }
        else
        {
            Debug.Log($"{inventorySlots[slotIndex].itemData.itemName} {quantity}���� ���ŵǾ����ϴ�. ���� ����: {inventorySlots[slotIndex].stackSize}");
        }
    }
}