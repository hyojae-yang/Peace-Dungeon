using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 인벤토리와 장비 패널의 UI를 총괄적으로 관리하는 컨트롤러 클래스입니다.
/// InventoryManager와 PlayerEquipmentManager의 이벤트에 반응하여 UI를 업데이트합니다.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static InventoryUIController Instance { get; private set; }

    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

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

    [Header("확인창")]
    [Tooltip("인벤토리 아이템 버리기 전용 확인창 UI 오브젝트를 할당합니다.")]
    [SerializeField] private GameObject itemDiscardConfirmPanel;
    [Tooltip("장비 아이템 버리기 전용 확인창 UI 오브젝트를 할당합니다.")]
    [SerializeField] private GameObject equipDiscardConfirmPanel;

    // === 슬롯 그룹화를 위한 딕셔너리 ===
    private Dictionary<ItemType, List<ItemSlotUI>> itemSlotGroups = new Dictionary<ItemType, List<ItemSlotUI>>();
    private Dictionary<EquipType, List<ItemSlotUI>> equipSlotGroups = new Dictionary<EquipType, List<ItemSlotUI>>();

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

        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;

        // PlayerCharacter의 매니저들을 통해 이벤트에 구독합니다.
        if (playerCharacter != null && playerCharacter.inventoryManager != null)
        {
            playerCharacter.inventoryManager.onInventoryChanged += RefreshInventoryUI;
        }
        if (playerCharacter != null && playerCharacter.playerEquipmentManager != null)
        {
            playerCharacter.playerEquipmentManager.onEquipmentChanged += RefreshEquipmentUI;
        }
    }

    private void Start()
    {
        // 게임 시작 시 초기 UI를 한번 갱신합니다.
        RefreshInventoryUI();
        RefreshEquipmentUI();
    }

    private void OnDestroy()
    {
        // 메모리 누수를 방지하기 위해 씬이 파괴될 때 이벤트를 구독 해제합니다.
        if (playerCharacter != null && playerCharacter.inventoryManager != null)
        {
            playerCharacter.inventoryManager.onInventoryChanged -= RefreshInventoryUI;
        }
        if (playerCharacter != null && playerCharacter.playerEquipmentManager != null)
        {
            playerCharacter.playerEquipmentManager.onEquipmentChanged -= RefreshEquipmentUI;
        }
    }

    /// <summary>
    /// InventoryManager의 onInventoryChanged 이벤트가 발생하면 호출됩니다.
    /// 인벤토리 데이터를 새로 불러와 모든 아이템 슬롯 UI를 갱신합니다.
    /// </summary>
    public void RefreshInventoryUI()
    {
        if (playerCharacter == null || playerCharacter.inventoryManager == null)
        {
            Debug.LogError("PlayerCharacter 또는 InventoryManager가 누락되었습니다.");
            return;
        }

        // 1. 모든 인벤토리 슬롯을 비워줍니다.
        ClearAllInventorySlots();

        // 2. InventoryManager에서 최신 인벤토리 데이터를 가져옵니다.
        List<ItemData> inventoryData = playerCharacter.inventoryManager.GetInventoryItems();

        // 3. 각 아이템 타입에 맞는 패널에 아이템을 배치합니다.
        foreach (var itemData in inventoryData)
        {
            List<ItemSlotUI> slotsToFill = null;
            // 아이템 타입에 맞는 슬롯 그룹을 찾습니다.
            if (itemData.itemSO is EquipmentItemSO equipmentItem)
            {
                // 장비 아이템은 EquipType에 따라 그룹을 찾습니다.
                equipSlotGroups.TryGetValue(equipmentItem.equipType, out slotsToFill);
            }
            else
            {
                // 기타 아이템은 ItemType에 따라 그룹을 찾습니다.
                itemSlotGroups.TryGetValue(itemData.itemSO.itemType, out slotsToFill);
            }

            if (slotsToFill != null)
            {
                // 슬롯 그룹 내의 비어있는 슬롯을 찾아 아이템을 채웁니다.
                for (int i = 0; i < slotsToFill.Count; i++)
                {
                    if (slotsToFill[i].GetItem() == null)
                    {
                        slotsToFill[i].UpdateSlot(itemData);
                        break; // 아이템을 채웠으니 다음 아이템으로 넘어갑니다.
                    }
                }
            }
        }
    }

    /// <summary>
    /// PlayerEquipmentManager의 onEquipmentChanged 이벤트가 발생하면 호출됩니다.
    /// 장비 패널의 UI만 갱신합니다.
    /// </summary>
    public void RefreshEquipmentUI()
    {
        if (playerCharacter == null || playerCharacter.playerEquipmentManager == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerEquipmentManager가 누락되었습니다.");
            return;
        }
        UpdateEquipmentUI(playerCharacter.playerEquipmentManager.GetEquippedItems());
    }

    /// <summary>
    /// 에디터에서 할당한 슬롯들을 기반으로 딕셔너리를 초기화합니다.
    /// </summary>
    private void InitializeSlotGroups()
    {
        // 일반 아이템 슬롯 그룹화
        itemSlotGroups.Add(ItemType.Consumable, consumableSlots);
        itemSlotGroups.Add(ItemType.Material, materialSlots);
        itemSlotGroups.Add(ItemType.Quest, questSlots);
        itemSlotGroups.Add(ItemType.Special, specialSlots);

        // 장비 아이템 슬롯 그룹화
        equipSlotGroups.Add(EquipType.Weapon, weaponInventorySlots);
        equipSlotGroups.Add(EquipType.Armor, armorInventorySlots);
        equipSlotGroups.Add(EquipType.Accessory, accessoryInventorySlots);
    }

    /// <summary>
    /// 실제 장비 착용 데이터에 기반하여 장비 착용 슬롯 UI를 업데이트합니다.
    /// </summary>
    /// <param name="equippedItems">업데이트할 장비 목록입니다.</param>
    private void UpdateEquipmentUI(Dictionary<EquipSlot, EquipmentItemSO> equippedItems)
    {
        // 모든 장비 슬롯을 먼저 비웁니다.
        foreach (var slot in equippedSlots)
        {
            slot.UpdateSlot(null);
        }

        // 장착된 아이템 데이터로 슬롯을 갱신합니다.
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
    /// ItemSlotUI.UpdateSlot()에 맞는 형태로 업데이트합니다.
    /// </summary>
    private void ClearAllInventorySlots()
    {
        // 모든 ItemSlotUI 리스트를 순회하며 슬롯을 비웁니다.
        foreach (var slotList in itemSlotGroups.Values)
        {
            foreach (var slot in slotList)
            {
                slot.UpdateSlot(new ItemData(null, 0));
            }
        }
        foreach (var slotList in equipSlotGroups.Values)
        {
            foreach (var slot in slotList)
            {
                slot.UpdateSlot(new ItemData(null, 0));
            }
        }
    }

    /// <summary>
    /// 아이템 버리기 확인창을 활성화하고 아이템 정보를 전달합니다.
    /// </summary>
    /// <param name="item">버릴 아이템의 데이터 (BaseItemSO)</param>
    /// <param name="count">버릴 아이템의 개수</param>
    public void ShowDiscardConfirmPanel(BaseItemSO item, int count)
    {
        // 1. 아이템 타입에 따라 목표 확인창을 결정합니다.
        GameObject targetPanel = null;
        if (item is EquipmentItemSO)
        {
            // 장비 아이템일 경우, 장비 버리기 확인창을 선택합니다.
            targetPanel = equipDiscardConfirmPanel;
        }
        else
        {
            // 그 외의 일반 아이템일 경우, 일반 아이템 버리기 확인창을 선택합니다.
            targetPanel = itemDiscardConfirmPanel;
        }

        // 2. 목표 확인창이 제대로 할당되었는지 확인합니다.
        if (targetPanel != null)
        {
            // 3. 해당 확인창 오브젝트를 활성화합니다.
            targetPanel.SetActive(true);

            // 4. 확인창 스크립트(ConfirmPanel.cs)를 찾아 초기화 메서드를 호출합니다.
            ConfirmPanel confirmPanel = targetPanel.GetComponent<ConfirmPanel>();
            if (confirmPanel != null)
            {
                // 확인창에 아이템 정보와 개수를 전달합니다.
                confirmPanel.Initialize(item, count);
            }
        }
    }
}