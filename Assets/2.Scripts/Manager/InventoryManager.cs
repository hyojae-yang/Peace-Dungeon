using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug; // StackTrace�� ����ϱ� ���� �߰�

/// <summary>
/// �÷��̾��� �κ��丮 ������ ������ ����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ �߰�, ����, �Ҹ�ǰ ���, ������ ���� ������ ó���ϸ�,
/// PlayerEquipmentManager�� �����Ͽ� ��� ���� �� �������� �κ��丮�� �ǵ����ϴ�.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static InventoryManager Instance { get; private set; }

    // === �̺�Ʈ ===
    /// <summary>
    /// �κ��丮 ������ ����� ������ ȣ��Ǵ� �̺�Ʈ�Դϴ�.
    /// </summary>
    public event Action onInventoryChanged;

    // === ������ ����� ���� ===
    [Header("�κ��丮 ������")]
    [Tooltip("���� ���Ϸ� ����� �κ��丮 �����͸� �Ҵ��մϴ�.")]
    [SerializeField] private InventoryData inventoryData;

    [Tooltip("�κ��丮�� �ִ� ���� �����Դϴ�.")]
    [SerializeField] private int inventorySize = 20;

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            if (inventoryData != null)
            {
                inventoryData.Initialize(); // �����͸� �ʱ�ȭ�մϴ�.
            }
            else
            {
                Debug.LogError("InventoryData SO�� InventoryManager�� �Ҵ���� �ʾҽ��ϴ�!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �������� �κ��丮�� �߰��ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="item">�߰��� ������ ������</param>
    /// <param name="amount">�߰��� �������� ����</param>
    /// <returns>�������� ��� �߰��Ǿ����� ����</returns>
    public bool AddItem(BaseItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // 1. ��ĥ �� �ִ� �������� ã���ϴ�.
        // ���� �ذ�: itemID�� ������ ���������� Ȯ���մϴ�.
        for (int i = 0; i < inventoryData.inventoryItems.Count; i++)
        {
            if (inventoryData.inventoryItems[i].itemSO.itemID == item.itemID && inventoryData.inventoryItems[i].stackCount < item.maxStack)
            {
                int spaceLeft = item.maxStack - inventoryData.inventoryItems[i].stackCount;
                int addAmount = Mathf.Min(amount, spaceLeft);
                inventoryData.inventoryItems[i].stackCount += addAmount;

                // ���� ������ �ִٸ�, ��� ȣ���Ͽ� �� ���Կ� �߰��մϴ�.
                if (addAmount < amount)
                {
                    return AddItem(item, amount - addAmount);
                }

                onInventoryChanged?.Invoke(); // �κ��丮 ���� �̺�Ʈ ȣ��
                return true;
            }
        }

        // 2. ���� ������ �ִٸ�, �� ������ ã�� ���� �߰��մϴ�.
        if (amount > 0)
        {
            if (inventoryData.inventoryItems.Count >= inventorySize)
            {
                Debug.LogWarning($"�κ��丮�� ���� ���� ������ '{item.itemName}'({amount}��)��(��) �߰��� �� �����ϴ�.");
                return false;
            }
            // �� ���������� item ��ü�� �ƴ϶� ���ο� ItemData�� �����ؾ� �մϴ�.
            // item�� ������ �ν��Ͻ��̹Ƿ�, ���� SO�� itemID�� ����Ͽ� ItemData�� �����ϴ� ���� �� �����մϴ�.
            inventoryData.inventoryItems.Add(new ItemData(item, amount));
            Debug.Log($"<color=green>������ �߰�:</color> {item.itemName} ({amount}��) �� ���Կ� �߰�.");
        }

        onInventoryChanged?.Invoke(); // �κ��丮 ���� �̺�Ʈ ȣ��
        return true;
    }

    /// <summary>
    /// �κ��丮���� �������� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="item">������ ������ SO</param>
    /// <param name="amount">������ ����</param>
    /// <returns>������ ���� ���� ����</returns>
    public bool RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // ������ �������� �κ��丮���� ã���ϴ�. (���� �������� �߰��� ���� �����ۺ��� ó��)
        for (int i = inventoryData.inventoryItems.Count - 1; i >= 0; i--)
        {
            if (inventoryData.inventoryItems[i].itemSO == item)
            {
                if (inventoryData.inventoryItems[i].stackCount >= amount)
                {
                    inventoryData.inventoryItems[i].stackCount -= amount;
                    if (inventoryData.inventoryItems[i].stackCount <= 0)
                    {
                        inventoryData.inventoryItems.RemoveAt(i);
                    }
                    else
                    {
                        Debug.Log($"<color=red>������ ����:</color> {item.itemName} (-{amount}, ���� ����: {inventoryData.inventoryItems[i].stackCount})");
                    }
                    onInventoryChanged?.Invoke();
                    return true; // ���������� ����
                }
                else
                {
                    // �� ���, ��û�� ������ŭ ������ �� �����Ƿ� ��� �ϰ� �����մϴ�.
                    Debug.LogWarning($"�κ��丮���� '{item.itemName}' �������� {amount}�� �����Ϸ� ������, ���� ������ �����մϴ�. (����: {inventoryData.inventoryItems[i].stackCount}��)");
                    return false; // ���� ����
                }
            }
        }
        Debug.LogWarning($"�κ��丮�� '{item.itemName}' �������� �����ϴ�. ������ �� �����ϴ�.");
        return false; // ���� ����
    }

    /// <summary>
    /// �Ҹ� �������� ����ϰ� �κ��丮���� �����մϴ�.
    /// </summary>
    public void UseItem(ConsumableItemSO itemToUse, PlayerCharacter playerCharacter)
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
    /// <param name="itemToRemove">���� ������ ������</param>
    /// <param name="amount">���� ������ ����</param>
    public void DiscardItem(BaseItemSO itemToRemove, int amount)
    {
        RemoveItem(itemToRemove, amount);
        onInventoryChanged?.Invoke();
        Debug.Log($"<color=red>������ ������:</color> {itemToRemove.itemName} (����: {amount})");
    }

    /// <summary>
    /// ���� �κ��丮�� ������ ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�κ��丮 ������ ������ ����Ʈ</returns>
    public List<ItemData> GetInventoryItems()
    {
        return inventoryData.inventoryItems;
    }

    /// <summary>
    /// ���� ������ ������ �����͸� �����ɴϴ�. (PlayerEquipmentManager���� ������ �� ���)
    /// </summary>
    /// <returns>���� ������ ��ųʸ�</returns>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        // InventoryManager�� ���� ���� ������ ������ �������� ������,
        // PlayerEquipmentManager�� �ʿ��� �� ȣ���� �� �ֵ��� ������ �����մϴ�.
        // ���� ��� ������ PlayerEquipmentManager�� �����մϴ�.
        return PlayerEquipmentManager.Instance.GetEquippedItems();
    }
}