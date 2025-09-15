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
        // ����� ���� ����: � �޼��忡��, �� AddItem�� ȣ��Ǿ����� ��Ȯ�� �ľ��ϱ� �����Դϴ�.
        Debug.Log($"<color=lime>AddItem ȣ���:</color> {item.itemName} ({amount}��)\n" +
                  $"ȣ�� ����:\n{new StackTrace(true)}");

        if (item == null || amount <= 0) return false;

        // 1. ��ĥ �� �ִ� �������� ã���ϴ�. (��� �������� maxStack�� 1�̹Ƿ� ���⿡ ���Ե��� ����)
        for (int i = 0; i < inventoryData.inventoryItems.Count; i++)
        {
            if (inventoryData.inventoryItems[i].itemSO == item && inventoryData.inventoryItems[i].stackCount < item.maxStack)
            {
                int spaceLeft = item.maxStack - inventoryData.inventoryItems[i].stackCount;
                int addAmount = Mathf.Min(amount, spaceLeft);
                inventoryData.inventoryItems[i].stackCount += addAmount;
                Debug.Log($"<color=green>������ �߰�:</color> {item.itemName} (+{addAmount})");

                // ���� ������ �ִٸ�, �ٽ� �� �޼��带 ȣ���Ͽ� �� ���Կ� �߰��մϴ�.
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
        // ����� ���� ����: � �޼��忡��, �� RemoveItem�� ȣ��Ǿ����� �ľ��ϱ� �����Դϴ�.
        Debug.Log($"<color=red>RemoveItem ȣ���:</color> {item.itemName} ({amount}��)\n" +
                  $"ȣ�� ����:\n{new StackTrace(true)}");

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
                        Debug.Log($"<color=red>������ ����:</color> {item.itemName} (���� ���)");
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
    /// <param name="itemToUse">����� �Ҹ� ������ ������</param>
    public void UseItem(ConsumableItemSO itemToUse)
    {
        if (itemToUse == null) return;

        // ������ ��� ���� (��: ü�� ȸ��, ���� ���� ��)
        // TODO: ���⿡ ���� ������ ȿ���� �����ϴ� �ڵ带 �߰��ϼ���.
        // ��: PlayerStatSystem.Instance.Heal(itemToUse.healAmount);
        Debug.Log($"<color=cyan>�Ҹ�ǰ ���:</color> {itemToUse.itemName}");

        // ����� �������� �κ��丮���� �����մϴ�.
        RemoveItem(itemToUse, 1);
        onInventoryChanged?.Invoke();
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