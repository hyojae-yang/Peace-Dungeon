// InventoryUIController.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 인벤토리와 장비 패널의 UI를 총괄적으로 관리하는 컨트롤러 클래스입니다.
/// InventoryManager의 이벤트에 반응하여 UI를 업데이트합니다.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    public static InventoryUIController Instance { get; private set; }

    // === 참조 변수 (유니티 인스펙터에서 할당) ===
    [Header("UI 패널 참조")]
    [Tooltip("메인 인벤토리 패널 오브젝트를 할당합니다.")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("장비 착용 슬롯")]
    [Tooltip("장비 착용 UI에 있는 슬롯들입니다. (EquipmentSlotUI 스크립트 사용)")]
    [SerializeField] private List<EquipmentSlotUI> equippedSlots = new List<EquipmentSlotUI>();

    [Header("인벤토리 슬롯 리스트")]
    [Tooltip("무기 패널에 있는 아이템 슬롯들을 순서대로 할당합니다. (ItemSlotUI 스크립트 사용)")]
    [SerializeField] private List<ItemSlotUI> weaponInventorySlots = new List<ItemSlotUI>();
    [Tooltip("방어구 패널에 있는 아이템 슬롯들을 순서대로 할당합니다.")]
    [SerializeField] private List<ItemSlotUI> armorInventorySlots = new List<ItemSlotUI>();
    [Tooltip("장신구 패널에 있는 아이템 슬롯들을 순서대로 할당합니다.")]
    [SerializeField] private List<ItemSlotUI> accessoryInventorySlots = new List<ItemSlotUI>();
    [Tooltip("소모품 패널에 있는 아이템 슬롯들을 순서대로 할당합니다.")]
    [SerializeField] private List<ItemSlotUI> consumableSlots = new List<ItemSlotUI>();
    [Tooltip("재료 패널에 있는 아이템 슬롯들을 순서대로 할당합니다.")]
    [SerializeField] private List<ItemSlotUI> materialSlots = new List<ItemSlotUI>();
    [Tooltip("퀘스트 아이템 패널에 있는 슬롯들을 순서대로 할당합니다.")]
    [SerializeField] private List<ItemSlotUI> questSlots = new List<ItemSlotUI>();
    [Tooltip("특수 아이템 패널에 있는 슬롯들을 순서대로 할당합니다.")]
    [SerializeField] private List<ItemSlotUI> specialSlots = new List<ItemSlotUI>();

    // === 슬롯 그룹화를 위한 딕셔너리 ===
    private Dictionary<ItemType, List<ItemSlotUI>> itemSlotGroups = new Dictionary<ItemType, List<ItemSlotUI>>();

    // === MonoBehaviour 메서드 ===
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

        // InventoryManager의 이벤트에 이 스크립트의 메서드를 구독합니다.
        // 이제는 아이템 추가/제거 이벤트를 각각 구독합니다.
        InventoryManager.Instance.onInventoryItemAdded += HandleItemAdded;
        InventoryManager.Instance.onInventoryItemRemoved += HandleItemRemoved;
        InventoryManager.Instance.onInventoryChanged += RefreshInventoryUI;
        InventoryManager.Instance.onEquipmentChanged += RefreshEquipmentUI;
    }

    private void OnDestroy()
    {
        // 메모리 누수를 방지하기 위해 씬이 파괴될 때 이벤트를 구독 해제합니다.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryItemAdded -= HandleItemAdded;
            InventoryManager.Instance.onInventoryItemRemoved -= HandleItemRemoved;
            InventoryManager.Instance.onInventoryChanged -= RefreshInventoryUI;
            InventoryManager.Instance.onEquipmentChanged -= RefreshEquipmentUI;
        }
    }

    /// <summary>
    /// 아이템이 인벤토리에 추가될 때 호출됩니다.
    /// 추가된 아이템 정보만을 기반으로 특정 슬롯을 업데이트합니다.
    /// </summary>
    /// <param name="item">추가된 아이템 정보</param>
    /// <param name="amount">추가된 아이템의 개수</param>
    private void HandleItemAdded(BaseItemSO item, int amount)
    {
        Debug.Log($"<color=cyan>[UI Controller]</color> 이벤트 수신! 아이템: {item.itemName}, 개수: {amount}");
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
                // ===== 핵심 추가 로그 =====
                Debug.Log($"<color=yellow>현재 검사 중인 슬롯: {slot.name}</color>");
                // =========================

                if (slot.GetItem() == null)
                {
                    Debug.Log($"<color=green>비어있는 슬롯 발견! 아이템을 추가합니다.</color>");
                    slot.UpdateSlot(item, slot.GetItemCount() + amount);
                    return;
                }
                else if (slot.GetItem() == item)
                {
                    Debug.Log($"<color=yellow>이미 있는 아이템 발견! 개수를 추가합니다.</color>");
                    slot.UpdateSlot(item, slot.GetItemCount() + amount);
                    return;
                }
                else
                {
                    Debug.LogWarning($"<color=red>슬롯에 아이템이 이미 있음: {slot.GetItem().itemName} - (추가하려는 아이템과 다름)</color>");
                }
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>아이템 타입({item.itemType})에 맞는 슬롯 그룹을 찾을 수 없습니다! 에디터에서 슬롯을 할당했는지 확인하세요.</color>");
        }
    }

    /// <summary>
    /// 아이템이 인벤토리에서 제거될 때 호출됩니다.
    /// 제거된 아이템 정보만을 기반으로 특정 슬롯을 업데이트합니다.
    /// </summary>
    /// <param name="item">제거된 아이템 정보</param>
    /// <param name="amount">제거된 아이템의 개수</param>
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
                    existingSlot.UpdateSlot(null, 0); // 개수가 0이면 슬롯을 비웁니다.
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
                        existingSlot.UpdateSlot(null, 0); // 개수가 0이면 슬롯을 비웁니다.
                    }
                }
            }
        }
    }

    /// <summary>
    /// InventoryManager의 onInventoryChanged 이벤트가 발생하면 호출됩니다.
    /// 장비 장착/해제로 인해 인벤토리 데이터가 변경될 때 UI를 갱신합니다.
    /// </summary>
    public void RefreshInventoryUI()
    {
        ClearAllInventorySlots();
        UpdateInventoryUI(InventoryManager.Instance.GetInventoryData());
    }

    /// <summary>
    /// InventoryManager의 onEquipmentChanged 이벤트가 발생하면 호출됩니다.
    /// 장비 패널의 UI만 갱신합니다.
    /// </summary>
    public void RefreshEquipmentUI()
    {
        UpdateEquipmentUI(InventoryManager.Instance.GetEquippedData());
    }

    /// <summary>
    /// 에디터에서 할당한 슬롯들을 기반으로 딕셔너리를 초기화합니다.
    /// </summary>
    private void InitializeSlotGroups()
    {
        itemSlotGroups.Add(ItemType.Consumable, consumableSlots);
        itemSlotGroups.Add(ItemType.Material, materialSlots);
        itemSlotGroups.Add(ItemType.Quest, questSlots);
        itemSlotGroups.Add(ItemType.Special, specialSlots);
    }

    /// <summary>
    /// 실제 인벤토리 데이터에 기반하여 아이템 슬롯 UI를 업데이트합니다.
    /// 아이템의 ItemType을 기준으로 올바른 패널에 정렬하여 배치합니다.
    /// (주로 장비 장착/해제 시에 사용됩니다)
    /// </summary>
    /// <param name="items">업데이트할 아이템 목록과 개수입니다.</param>
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
    /// 실제 장비 착용 데이터에 기반하여 장비 착용 슬롯 UI를 업데이트합니다.
    /// </summary>
    /// <param name="equippedItems">업데이트할 장비 목록입니다.</param>
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
    /// 모든 인벤토리 슬롯을 초기화(비움)합니다.
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
    /// 아이템 타입에 따라 올바른 장비 슬롯 리스트를 반환합니다.
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
    /// 주어진 슬롯 리스트에 아이템 목록을 순서대로 채워 넣습니다.
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