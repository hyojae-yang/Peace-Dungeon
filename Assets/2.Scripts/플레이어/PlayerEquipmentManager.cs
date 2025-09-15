using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// �÷��̾��� ��� ���� ���¸� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �������� �����ϰ�, �����ϸ�, ��� �ɷ�ġ�� ������Ʈ�մϴ�.
/// </summary>
public class PlayerEquipmentManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static PlayerEquipmentManager Instance { get; private set; }

    // === �̺�Ʈ ===
    /// <summary>
    /// ��� ���°� ����� �� ȣ��Ǵ� �̺�Ʈ�Դϴ�.
    /// </summary>
    public event Action onEquipmentChanged;

    // === ������ ����� ���� ===
    // ���� ������ �����۵��� �����ϴ� ��ųʸ�
    private Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� �����ǵ��� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��� �������� �����ϰų� ��ü�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="itemToEquip">������ ��� ������ ������</param>
    public void EquipItem(EquipmentItemSO itemToEquip)
    {
        if (itemToEquip == null) return;

        EquipSlot slot = itemToEquip.equipSlot;
        EquipmentItemSO oldItem = null;

        // 1. ���� ���Կ� �ٸ� ��� �ִٸ�, ���� ��� �κ��丮�� �ǵ����ϴ�.
        if (equippedItems.TryGetValue(slot, out oldItem))
        {
            // ���� ��� �κ��丮�� �߰��մϴ�.
            InventoryManager.Instance.AddItem(oldItem);
            Debug.Log($"<color=yellow>��� ���� (��ü):</color> {oldItem.itemName}��(��) �κ��丮�� ��ȯ.");
        }

        // 2. �κ��丮���� ���� �����Ϸ��� �������� �����մϴ�.
        // �� ������ ���� ���� �ÿ��� ����Ǿ�� �մϴ�.
        if (InventoryManager.Instance.RemoveItem(itemToEquip, 1))
        {
            // 3. �κ��丮 ���ſ� �����ϸ�, ���ο� ��� ���Կ� �Ҵ��մϴ�.
            equippedItems[slot] = itemToEquip;
            Debug.Log($"<color=lime>��� ����:</color> {itemToEquip.itemName}��(��) {slot} ���Կ� ����.");
        }
        else
        {
            // �κ��丮���� �������� ã�� ���ϸ� ���� ����
            Debug.LogWarning($"<color=red>��� ���� ����:</color> �κ��丮�� {itemToEquip.itemName} �������� �����ϴ�.");
            return; // �޼��� ����
        }

        // 4. ��� ���°� ����Ǿ����� �˸��ϴ�. (UI ���� ��)
        onEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// Ư�� ���Կ� ���� �������� �����մϴ�. (Void ��ȯ Ÿ��)
    /// </summary>
    /// <param name="slot">������ ��� ����</param>
    public void UnEquipItem(EquipSlot slot)
    {
        // �ش� ���Կ� ������ �������� �ִ��� Ȯ���մϴ�.
        if (equippedItems.TryGetValue(slot, out EquipmentItemSO itemToUnequip))
        {
            // �������� �κ��丮�� �ǵ����ϴ�.
            InventoryManager.Instance.AddItem(itemToUnequip);
            Debug.Log($"<color=yellow>��� ����:</color> {itemToUnequip.itemName}��(��) {slot} ���Կ��� �����ϰ� �κ��丮�� ��ȯ�߽��ϴ�.");

            // ��ųʸ����� �ش� ��� ������ �����մϴ�.
            equippedItems.Remove(slot);

            // ��� ���� ���� �̺�Ʈ ȣ��
            onEquipmentChanged?.Invoke();
        }
        else
        {
            // �ش� ���Կ� ��� ������ ��� �α׸� ����մϴ�.
            Debug.LogWarning($"<color=red>��� ���� ����:</color> {slot} ���Կ� ������ �������� �����ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ ������ ����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>��� ������ ��ųʸ�</returns>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        return equippedItems;
    }
}