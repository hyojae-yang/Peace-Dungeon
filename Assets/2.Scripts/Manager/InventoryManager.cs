using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

/// <summary>
/// �÷��̾��� �κ��丮 ������ ������ ����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ �߰�, ����, �Ҹ�ǰ ���, ������ ���� ������ ó���ϸ�,
/// PlayerEquipmentManager�� �����Ͽ� ��� ���� �� �������� �κ��丮�� �ǵ����ϴ�.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    // === �̺�Ʈ ===
    /// <summary>
    /// �κ��丮 ������ ����� ������ ȣ��Ǵ� �̺�Ʈ�Դϴ�.
    /// UI ���ſ� ���˴ϴ�.
    /// </summary>
    public event Action onInventoryChanged;

    /// <summary>
    /// �������� �κ��丮�� �߰��� �� ȣ��Ǵ� �̺�Ʈ�Դϴ�.
    /// QuestManager�� ����Ʈ ���� ��Ȳ�� �˸��ϴ�.
    /// </summary>
    public event Action<int, int> OnItemAdded; // string -> int ����

    /// <summary>
    /// �������� �κ��丮���� ���ŵ� �� ȣ��Ǵ� �̺�Ʈ�Դϴ�.
    /// QuestManager�� ����Ʈ ���� ��Ȳ�� �˸��ϴ�.
    /// </summary>
    public event Action<int, int> OnItemRemoved; // string -> int ����

    // === ������ ����� ���� ===
    [Header("�κ��丮 ������")]
    [Tooltip("���� ���Ϸ� ����� �κ��丮 �����͸� �Ҵ��մϴ�.")]
    [SerializeField] private InventoryData inventoryData;

    [Tooltip("�κ��丮�� �ִ� ���� �����Դϴ�.")]
    [SerializeField] private int inventorySize = 80;

    // === MonoBehaviour �޼��� ===
    private void Start()
    {
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� ã�� �� �����ϴ�. ��ũ��Ʈ�� ����� �������� ���� �� �ֽ��ϴ�.");
            return;
        }

        if (inventoryData != null)
        {
            inventoryData.Initialize();
        }
        else
        {
            Debug.LogError("InventoryData SO�� InventoryManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }

    /// <summary>
    /// �������� �κ��丮�� �߰��ϴ� �޼����Դϴ�.
    /// </summary>
    public bool AddItem(BaseItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int remainingAmount = amount;

        var existingItems = inventoryData.inventoryItems.Where(i => i.itemSO.itemID == item.itemID && i.stackCount < item.maxStack);
        foreach (var existingItem in existingItems)
        {
            int spaceLeft = item.maxStack - existingItem.stackCount;
            int addAmount = Mathf.Min(remainingAmount, spaceLeft);
            existingItem.stackCount += addAmount;
            remainingAmount -= addAmount;

            if (remainingAmount <= 0) break;
        }

        while (remainingAmount > 0)
        {
            if (inventoryData.inventoryItems.Count >= inventorySize)
            {
                Debug.LogWarning("�κ��丮�� ���� á���ϴ�. ��� �������� �߰��� �� �����ϴ�.");
                onInventoryChanged?.Invoke();
                return false;
            }
            int newStackAmount = Mathf.Min(remainingAmount, item.maxStack);
            inventoryData.inventoryItems.Add(new ItemData(item, newStackAmount));
            remainingAmount -= newStackAmount;
        }

        OnItemAdded?.Invoke(item.itemID, amount); // int ID ���

        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// �κ��丮���� �������� �����ϴ� �޼����Դϴ�.
    /// </summary>
    public bool RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        int totalCount = GetItemCount(item.itemID); // int ID ���
        if (totalCount < amount)
        {
            Debug.LogWarning($"�κ��丮�� '{item.itemName}' �������� �����մϴ�. (�ʿ�: {amount}, ����: {totalCount})");
            return false;
        }

        int remainingAmount = amount;

        for (int i = inventoryData.inventoryItems.Count - 1; i >= 0 && remainingAmount > 0; i--)
        {
            if (inventoryData.inventoryItems[i].itemSO.itemID == item.itemID) // int ID ���
            {
                int removeAmount = Mathf.Min(remainingAmount, inventoryData.inventoryItems[i].stackCount);
                inventoryData.inventoryItems[i].stackCount -= removeAmount;
                remainingAmount -= removeAmount;

                if (inventoryData.inventoryItems[i].stackCount <= 0)
                {
                    inventoryData.inventoryItems.RemoveAt(i);
                }
            }
        }

        OnItemRemoved?.Invoke(item.itemID, amount); // int ID ���

        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// �κ��丮�� Ư�� �������� �ʿ��� ������ŭ �ִ��� Ȯ���մϴ�.
    /// QuestManager���� ����Ʈ �Ϸ� ���� Ȯ�� �� ���˴ϴ�.
    /// </summary>
    public bool HasItem(int itemID, int requiredAmount) // string -> int ����
    {
        return GetItemCount(itemID) >= requiredAmount;
    }

    /// <summary>
    /// �κ��丮���� Ư�� �������� �� ������ ����մϴ�.
    /// </summary>
    public int GetItemCount(int itemID) // string -> int ����
    {
        int totalCount = 0;
        foreach (var itemData in inventoryData.inventoryItems)
        {
            if (itemData.itemSO.itemID == itemID) // int ID ���
            {
                totalCount += itemData.stackCount;
            }
        }
        return totalCount;
    }

    /// <summary>
    /// �Ҹ� �������� ����ϰ� �κ��丮���� �����մϴ�.
    /// </summary>
    public void UseItem(ConsumableItemSO itemToUse)
    {
        if (itemToUse == null || playerCharacter == null)
        {
            Debug.LogError("������ �Ǵ� �÷��̾� ĳ���Ͱ� ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        itemToUse.Use(playerCharacter);
        RemoveItem(itemToUse, 1);
    }

    /// <summary>
    /// �������� �κ��丮���� �����ϴ�.
    /// </summary>
    public void DiscardItem(BaseItemSO itemToRemove, int amount)
    {
        RemoveItem(itemToRemove, amount);
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// ���� �κ��丮�� ������ ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public List<ItemData> GetInventoryItems()
    {
        return inventoryData.inventoryItems;
    }

    /// <summary>
    /// ���� ������ ������ �����͸� �����ɴϴ�. (PlayerEquipmentManager���� ������ �� ���)
    /// </summary>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        if (playerCharacter == null || playerCharacter.playerEquipmentManager == null)
        {
            Debug.LogError("PlayerEquipmentManager�� ������ �� �����ϴ�.");
            return null;
        }
        return playerCharacter.playerEquipmentManager.GetEquippedItems();
    }
}