// InventoryUIController.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �κ��丮�� ��� �г��� UI�� �Ѱ������� �����ϴ� ��Ʈ�ѷ� Ŭ�����Դϴ�.
/// InventoryManager�� �̺�Ʈ�� �����Ͽ� UI�� ������Ʈ�մϴ�.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static InventoryUIController Instance { get; private set; }

    // === ���� ���� (����Ƽ �ν����Ϳ��� �Ҵ�) ===
    [Header("UI �г� ����")]
    [Tooltip("���� �κ��丮 �г� ������Ʈ�� �Ҵ��մϴ�.")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("��� ���� ����")]
    [Tooltip("��� ���� UI�� �ִ� ���Ե��Դϴ�. (EquipmentSlotUI ��ũ��Ʈ ���)")]
    [SerializeField] private List<EquipmentSlotUI> equippedSlots = new List<EquipmentSlotUI>();

    [Header("�κ��丮 ���� ����Ʈ")]
    [Tooltip("���� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�. (ItemSlotUI ��ũ��Ʈ ���)")]
    [SerializeField] private List<ItemSlotUI> weaponInventorySlots = new List<ItemSlotUI>();
    [Tooltip("�� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> armorInventorySlots = new List<ItemSlotUI>();
    [Tooltip("��ű� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> accessoryInventorySlots = new List<ItemSlotUI>();
    [Tooltip("�Ҹ�ǰ �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> consumableSlots = new List<ItemSlotUI>();
    [Tooltip("��� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> materialSlots = new List<ItemSlotUI>();
    [Tooltip("����Ʈ ������ �гο� �ִ� ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> questSlots = new List<ItemSlotUI>();
    [Tooltip("Ư�� ������ �гο� �ִ� ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> specialSlots = new List<ItemSlotUI>();

    // === ���� �׷�ȭ�� ���� ��ųʸ� ===
    private Dictionary<ItemType, List<ItemSlotUI>> itemSlotGroups = new Dictionary<ItemType, List<ItemSlotUI>>();

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeSlotGroups();

        // InventoryManager�� �̺�Ʈ�� �� ��ũ��Ʈ�� �޼��带 �����մϴ�.
        // ������ ������ �߰�/���� �̺�Ʈ�� ���� �����մϴ�.
        InventoryManager.Instance.onInventoryItemAdded += HandleItemAdded;
        InventoryManager.Instance.onInventoryItemRemoved += HandleItemRemoved;
        InventoryManager.Instance.onInventoryChanged += RefreshInventoryUI;
        InventoryManager.Instance.onEquipmentChanged += RefreshEquipmentUI;
    }

    private void OnDestroy()
    {
        // �޸� ������ �����ϱ� ���� ���� �ı��� �� �̺�Ʈ�� ���� �����մϴ�.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryItemAdded -= HandleItemAdded;
            InventoryManager.Instance.onInventoryItemRemoved -= HandleItemRemoved;
            InventoryManager.Instance.onInventoryChanged -= RefreshInventoryUI;
            InventoryManager.Instance.onEquipmentChanged -= RefreshEquipmentUI;
        }
    }

    /// <summary>
    /// �������� �κ��丮�� �߰��� �� ȣ��˴ϴ�.
    /// �߰��� ������ �������� ������� Ư�� ������ ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="item">�߰��� ������ ����</param>
    /// <param name="amount">�߰��� �������� ����</param>
    private void HandleItemAdded(BaseItemSO item, int amount)
    {
        Debug.Log($"<color=cyan>[UI Controller]</color> �̺�Ʈ ����! ������: {item.itemName}, ����: {amount}");
        List<ItemSlotUI> slotsToSearch = null;

        if (item is EquipmentItemSO equipmentItem)
        {
            slotsToSearch = GetEquipmentSlotsByType(equipmentItem.equipType);
        }
        else
        {
            itemSlotGroups.TryGetValue(item.itemType, out slotsToSearch);
        }

        if (slotsToSearch != null)
        {
            foreach (var slot in slotsToSearch)
            {
                // ===== �ٽ� �߰� �α� =====
                Debug.Log($"<color=yellow>���� �˻� ���� ����: {slot.name}</color>");
                // =========================

                if (slot.GetItem() == null)
                {
                    Debug.Log($"<color=green>����ִ� ���� �߰�! �������� �߰��մϴ�.</color>");
                    slot.UpdateSlot(item, slot.GetItemCount() + amount);
                    return;
                }
                else if (slot.GetItem() == item)
                {
                    Debug.Log($"<color=yellow>�̹� �ִ� ������ �߰�! ������ �߰��մϴ�.</color>");
                    slot.UpdateSlot(item, slot.GetItemCount() + amount);
                    return;
                }
                else
                {
                    Debug.LogWarning($"<color=red>���Կ� �������� �̹� ����: {slot.GetItem().itemName} - (�߰��Ϸ��� �����۰� �ٸ�)</color>");
                }
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>������ Ÿ��({item.itemType})�� �´� ���� �׷��� ã�� �� �����ϴ�! �����Ϳ��� ������ �Ҵ��ߴ��� Ȯ���ϼ���.</color>");
        }
    }

    /// <summary>
    /// �������� �κ��丮���� ���ŵ� �� ȣ��˴ϴ�.
    /// ���ŵ� ������ �������� ������� Ư�� ������ ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="item">���ŵ� ������ ����</param>
    /// <param name="amount">���ŵ� �������� ����</param>
    private void HandleItemRemoved(BaseItemSO item, int amount)
    {
        if (item.itemType == ItemType.Equipment)
        {
            EquipmentItemSO equipmentItem = (EquipmentItemSO)item;
            List<ItemSlotUI> slots = GetEquipmentSlotsByType(equipmentItem.equipType);

            ItemSlotUI existingSlot = slots.FirstOrDefault(s => s.GetItem() == item);
            if (existingSlot != null)
            {
                int newCount = existingSlot.GetItemCount() - amount;
                if (newCount > 0)
                {
                    existingSlot.UpdateSlot(item, newCount);
                }
                else
                {
                    existingSlot.UpdateSlot(null, 0); // ������ 0�̸� ������ ���ϴ�.
                }
            }
        }
        else
        {
            if (itemSlotGroups.TryGetValue(item.itemType, out List<ItemSlotUI> slots))
            {
                ItemSlotUI existingSlot = slots.FirstOrDefault(s => s.GetItem() == item);
                if (existingSlot != null)
                {
                    int newCount = existingSlot.GetItemCount() - amount;
                    if (newCount > 0)
                    {
                        existingSlot.UpdateSlot(item, newCount);
                    }
                    else
                    {
                        existingSlot.UpdateSlot(null, 0); // ������ 0�̸� ������ ���ϴ�.
                    }
                }
            }
        }
    }

    /// <summary>
    /// InventoryManager�� onInventoryChanged �̺�Ʈ�� �߻��ϸ� ȣ��˴ϴ�.
    /// ��� ����/������ ���� �κ��丮 �����Ͱ� ����� �� UI�� �����մϴ�.
    /// </summary>
    public void RefreshInventoryUI()
    {
        ClearAllInventorySlots();
        UpdateInventoryUI(InventoryManager.Instance.GetInventoryData());
    }

    /// <summary>
    /// InventoryManager�� onEquipmentChanged �̺�Ʈ�� �߻��ϸ� ȣ��˴ϴ�.
    /// ��� �г��� UI�� �����մϴ�.
    /// </summary>
    public void RefreshEquipmentUI()
    {
        UpdateEquipmentUI(InventoryManager.Instance.GetEquippedData());
    }

    /// <summary>
    /// �����Ϳ��� �Ҵ��� ���Ե��� ������� ��ųʸ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeSlotGroups()
    {
        itemSlotGroups.Add(ItemType.Consumable, consumableSlots);
        itemSlotGroups.Add(ItemType.Material, materialSlots);
        itemSlotGroups.Add(ItemType.Quest, questSlots);
        itemSlotGroups.Add(ItemType.Special, specialSlots);
    }

    /// <summary>
    /// ���� �κ��丮 �����Ϳ� ����Ͽ� ������ ���� UI�� ������Ʈ�մϴ�.
    /// �������� ItemType�� �������� �ùٸ� �гο� �����Ͽ� ��ġ�մϴ�.
    /// (�ַ� ��� ����/���� �ÿ� ���˴ϴ�)
    /// </summary>
    /// <param name="items">������Ʈ�� ������ ��ϰ� �����Դϴ�.</param>
    private void UpdateInventoryUI(Dictionary<BaseItemSO, int> items)
    {
        ClearAllInventorySlots();

        var groupedAndSortedItems = items
            .OrderBy(x => x.Key.itemID)
            .GroupBy(x => x.Key.itemType);

        foreach (var group in groupedAndSortedItems)
        {
            if (group.Key == ItemType.Equipment)
            {
                var equippedItemsGrouped = group
                    .GroupBy(x => ((EquipmentItemSO)x.Key).equipType)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var equipGroup in equippedItemsGrouped)
                {
                    List<ItemSlotUI> slots = GetEquipmentSlotsByType(equipGroup.Key);
                    if (slots != null)
                    {
                        FillSlots(slots, equipGroup.Value);
                    }
                }
            }
            else
            {
                if (itemSlotGroups.TryGetValue(group.Key, out List<ItemSlotUI> slots))
                {
                    FillSlots(slots, group.ToList());
                }
            }
        }
    }

    /// <summary>
    /// ���� ��� ���� �����Ϳ� ����Ͽ� ��� ���� ���� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="equippedItems">������Ʈ�� ��� ����Դϴ�.</param>
    private void UpdateEquipmentUI(Dictionary<EquipSlot, EquipmentItemSO> equippedItems)
    {
        foreach (var slot in equippedSlots)
        {
            slot.UpdateSlot(null);
        }

        foreach (var equippedItemPair in equippedItems)
        {
            foreach (var slot in equippedSlots)
            {
                if (slot.equipSlotType == equippedItemPair.Key)
                {
                    slot.UpdateSlot(equippedItemPair.Value);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ��� �κ��丮 ������ �ʱ�ȭ(���)�մϴ�.
    /// </summary>
    private void ClearAllInventorySlots()
    {
        foreach (var slot in weaponInventorySlots) slot.UpdateSlot(null, 0);
        foreach (var slot in armorInventorySlots) slot.UpdateSlot(null, 0);
        foreach (var slot in accessoryInventorySlots) slot.UpdateSlot(null, 0);
        foreach (var slotList in itemSlotGroups.Values)
        {
            foreach (var slot in slotList)
            {
                slot.UpdateSlot(null, 0);
            }
        }
    }

    /// <summary>
    /// ������ Ÿ�Կ� ���� �ùٸ� ��� ���� ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    private List<ItemSlotUI> GetEquipmentSlotsByType(EquipType equipType)
    {
        switch (equipType)
        {
            case EquipType.Weapon:
                return weaponInventorySlots;
            case EquipType.Armor:
                return armorInventorySlots;
            case EquipType.Accessory:
                return accessoryInventorySlots;
            default:
                return null;
        }
    }

    /// <summary>
    /// �־��� ���� ����Ʈ�� ������ ����� ������� ä�� �ֽ��ϴ�.
    /// </summary>
    private void FillSlots(List<ItemSlotUI> slots, List<KeyValuePair<BaseItemSO, int>> items)
    {
        int i = 0;
        foreach (var itemPair in items)
        {
            if (i < slots.Count)
            {
                slots[i].UpdateSlot(itemPair.Key, itemPair.Value);
                i++;
            }
        }
    }
}