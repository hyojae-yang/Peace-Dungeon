// InventoryManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// �κ��丮 �ý����� �ٽ� ������ ����ϴ� �Ŵ��� Ŭ�����Դϴ�.
/// ������ ������ ����, �߰�, ����, ��� ����/���� �� ���� �̺�Ʈ�� �����մϴ�.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static InventoryManager Instance { get; private set; }

    // === �̺�Ʈ (��������Ʈ) ���� ===
    public event Action<BaseItemSO, int> onInventoryItemAdded;
    public event Action<BaseItemSO, int> onInventoryItemRemoved;
    public event Action onInventoryChanged; // �κ��丮 ��ü ���� �� ȣ�� (��� ����/���� ��)
    public event Action onEquipmentChanged; // ��� ���� ���� ���� �� ȣ��

    // === ������ ����� ���� ===
    [Header("�κ��丮 ������")]
    [Tooltip("���� ���Ϸ� ����� �κ��丮 �����͸� �Ҵ��մϴ�.")]
    [SerializeField] private InventoryData inventoryData;

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (inventoryData != null)
            {
                // �κ��丮 �����͸� �ε��մϴ�.
                inventoryData.Initialize();
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
    /// ������ 0�� ���ϸ� �߰����� �ʽ��ϴ�.
    /// </summary>
    /// <param name="item">�߰��� ������ SO</param>
    /// <param name="amount">�߰��� ����</param>
    public void AddItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning("��ȿ���� ���� ������ �Ǵ� �����Դϴ�. �������� �߰��� �� �����ϴ�.");
            return;
        }

        // === �ٽ� ���� �κ�: ������ ������Ʈ�� ���� �����մϴ�. ===
        // �������� �̹� �κ��丮�� �ִ��� Ȯ���մϴ�.
        if (inventoryData.inventoryItems.ContainsKey(item))
        {
            inventoryData.inventoryItems[item] += amount;
        }
        else
        {
            inventoryData.inventoryItems.Add(item, amount);
        }

        // ������ ������Ʈ�� �Ϸ�� ��, �̺�Ʈ�� �ߵ��˴ϴ�.
        onInventoryItemAdded?.Invoke(item, amount);
    }

    /// <summary>
    /// �������� �κ��丮���� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="item">������ ������ SO</param>
    /// <param name="amount">������ ����</param>
    public void RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning("��ȿ���� ���� ������ �Ǵ� �����Դϴ�. �������� ������ �� �����ϴ�.");
            return;
        }

        if (!inventoryData.inventoryItems.ContainsKey(item))
        {
            Debug.LogWarning("�κ��丮�� �ش� �������� �����ϴ�.");
            return;
        }

        inventoryData.inventoryItems[item] -= amount;

        if (inventoryData.inventoryItems[item] <= 0)
        {
            inventoryData.inventoryItems.Remove(item);
        }

        // ������ ���Ű� �Ϸ�� ��, �̺�Ʈ�� �ߵ��˴ϴ�.
        onInventoryItemRemoved?.Invoke(item, amount);
    }

    /// <summary>
    /// �κ��丮 �������� ��� ���Կ� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    public void EquipItem(EquipmentItemSO item)
    {
        if (item == null) return;

        // �̹� �ش� ���Կ� ������ �������� �ִ��� Ȯ���մϴ�.
        if (inventoryData.equippedItems.ContainsKey(item.equipSlot))
        {
            // ���� ��� �����մϴ�.
            UnEquipItem(item.equipSlot);
        }

        // �κ��丮���� �������� �����ϰ� ��� ���Կ� �߰��մϴ�.
        inventoryData.equippedItems.Add(item.equipSlot, item);
        RemoveItem(item, 1);

        // ��� ���� �̺�Ʈ�� �ߵ��մϴ�.
        onEquipmentChanged?.Invoke();
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// ��� ������ �������� �����ϰ� �κ��丮�� �ǵ����� �޼����Դϴ�.
    /// </summary>
    /// <param name="slotType">������ ��� ������ Ÿ��</param>
    public void UnEquipItem(EquipSlot slotType)
    {
        if (inventoryData.equippedItems.ContainsKey(slotType))
        {
            // ��� ���Կ��� �������� �����մϴ�.
            EquipmentItemSO unequippedItem = inventoryData.equippedItems[slotType];
            inventoryData.equippedItems.Remove(slotType);

            // �������� �κ��丮�� �ٽ� �߰��մϴ�.
            AddItem(unequippedItem, 1);

            // ��� ���� �̺�Ʈ�� �ߵ��մϴ�.
            onEquipmentChanged?.Invoke();
            onInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// ���� �κ��丮�� ��� ������ �����͸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�κ��丮 ������ ��ųʸ�</returns>
    public Dictionary<BaseItemSO, int> GetInventoryData()
    {
        return inventoryData.inventoryItems;
    }

    /// <summary>
    /// ���� ������ ������ �����͸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ��ųʸ�</returns>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedData()
    {
        return inventoryData.equippedItems;
    }
}