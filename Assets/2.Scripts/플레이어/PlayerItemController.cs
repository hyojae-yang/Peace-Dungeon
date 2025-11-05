using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 플레이어의 퀵슬롯 아이템 사용 및 관리를 담당하는 컨트롤러 스크립트입니다.
/// SRP (단일 책임 원칙): 퀵슬롯 아이템 할당, 5~8번 키 입력 감지 및 아이템 사용 요청 위임을 책임집니다.
/// 실제 아이템 사용 유효성 검사 및 수량 관리는 InventoryManager에 위임하여 책임을 분리합니다.
/// SOLID: 단일 책임 원칙 (퀵슬롯 관리 및 입력), 의존성 역전 원칙 (InventoryManager, ItemDatabaseManager 의존).
/// </summary>
public class PlayerItemController : MonoBehaviour, ISavable
{
    private const int SLOT_COUNT = 4;
    private const KeyCode STARTING_KEY_CODE = KeyCode.Alpha5;

    // === 종속성 ===
    private PlayerCharacter playerCharacter;
    // InventoryManager에는 public event Action<BaseItemSO, int> OnItemQuantityChanged; 이벤트가 정의되어 있다고 가정합니다.
    private InventoryManager inventoryManager;
    private ItemDatabaseManager itemDatabaseManager;

    // === 데이터 ===
    [Header("아이템 퀵슬롯 할당")]
    [Tooltip("5~8 키에 할당할 소모품 아이템 데이터 (ConsumableItemSO)를 할당합니다.")]
    public ConsumableItemSO[] itemSlots = new ConsumableItemSO[SLOT_COUNT];

    // === 이벤트 ===
    /// <summary>
    /// 퀵슬롯의 내용이 변경될 때 호출되는 이벤트입니다.
    /// int: 슬롯 인덱스 (0~3), ConsumableItemSO: 등록된 아이템 데이터 (해제 시 null)
    /// </summary>
    public event Action<int, ConsumableItemSO> OnSlotItemChanged;

    // === MonoBehaviour 메서드 ===

    private void Awake()
    {
        playerCharacter = PlayerCharacter.Instance;
        inventoryManager = playerCharacter.inventoryManager;
        itemDatabaseManager = ItemDatabaseManager.Instance;

        if (playerCharacter == null || inventoryManager == null || itemDatabaseManager == null)
        {
            Debug.LogError("핵심 종속성(PlayerCharacter, InventoryManager, ItemDatabaseManager) 중 하나 이상을 찾을 수 없습니다.");
            enabled = false;
        }
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }

        // InventoryManager의 아이템 수량 변경 이벤트를 구독합니다.
        if (inventoryManager != null)
        {
            inventoryManager.OnItemQuantityChanged += CheckQuickSlotItemQuantity;
        }

        // 초기화 시점에 모든 슬롯의 UI 업데이트 이벤트를 호출하여 UI를 동기화합니다.
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            OnSlotItemChanged?.Invoke(i, itemSlots[i]);
        }
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnItemQuantityChanged -= CheckQuickSlotItemQuantity;
        }
    }

    void Update()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (Input.GetKeyDown(STARTING_KEY_CODE + i))
            {
                UseQuickSlotItem(i);
            }
        }
    }

    // [필수 추가] QuickSlotItemPanel이 로드 시 수량을 동기화하기 위해 필요한 메서드입니다.
    /// <summary>
    /// 지정된 퀵슬롯 인덱스에 현재 등록된 아이템 데이터를 반환합니다.
    /// QuickSlotItemPanel의 초기 로드 시 수량 동기화를 위해 사용됩니다.
    /// </summary>
    /// <param name="index">조회할 퀵슬롯의 인덱스</param>
    /// <returns>등록된 ConsumableItemSO (슬롯이 비어있으면 null)</returns>
    public ConsumableItemSO GetItemInSlot(int index)
    {
        if (index >= 0 && index < itemSlots.Length)
        {
            return itemSlots[index];
        }
        return null;
    }

    // === 핵심 아이템 사용 로직 ===

    /// <summary>
    /// 지정된 퀵슬롯 인덱스의 아이템 사용을 시도합니다.
    /// 모든 사용 가능 여부 검사 및 수량 감소는 InventoryManager에 위임합니다.
    /// </summary>
    public void UseQuickSlotItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SLOT_COUNT)
        {
            Debug.LogError($"잘못된 퀵슬롯 인덱스입니다: {slotIndex}");
            return;
        }

        ConsumableItemSO itemToUse = itemSlots[slotIndex];

        if (itemToUse == null)
        {
            return;
        }
        inventoryManager.UseItem(itemToUse);
    }

    // === 퀵슬롯 등록 및 해제 로직 ===

    /// <summary>
    /// 특정 슬롯에 소모품 아이템을 등록합니다.
    /// </summary>
    public void RegisterItem(int slotIndex, ConsumableItemSO itemToRegister)
    {
        if (slotIndex >= 0 && slotIndex < SLOT_COUNT)
        {
            itemSlots[slotIndex] = itemToRegister;
            OnSlotItemChanged?.Invoke(slotIndex, itemToRegister);
        }
        else
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다: " + slotIndex);
        }
    }

    /// <summary>
    /// 특정 슬롯의 아이템을 해제합니다.
    /// </summary>
    public void UnregisterItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < SLOT_COUNT)
        {
            itemSlots[slotIndex] = null;
            OnSlotItemChanged?.Invoke(slotIndex, null);
        }
        else
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다: " + slotIndex);
        }
    }

    // [수정 3: 이벤트 핸들러 추가] 인벤토리 아이템 수량 변경 시 호출되는 메서드입니다.
    /// <summary>
    /// InventoryManager에서 아이템 수량이 변경될 때 호출되며,
    /// 퀵슬롯에 등록된 아이템의 수량이 0이 되었는지 확인하여 자동 해제합니다.
    /// </summary>
    /// <param name="itemSO">수량이 변경된 아이템 데이터</param>
    /// <param name="newQuantity">변경된 아이템의 새로운 수량</param>
    private void CheckQuickSlotItemQuantity(BaseItemSO itemSO, int newQuantity)
    {
        // 수량이 변경된 아이템이 소모품이 아니거나, 새 수량이 0보다 크면 퀵슬롯 해제 로직을 수행할 필요가 없습니다.
        if (newQuantity > 0 || !(itemSO is ConsumableItemSO consumableItem))
        {
            return;
        }

        // 모든 퀵슬롯을 순회하며 수량이 0이 된 아이템이 등록되어 있는지 확인합니다.
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            // 현재 슬롯의 아이템이 수량이 0이 된 아이템과 동일한 경우
            if (itemSlots[i] == consumableItem)
            {
                Debug.Log($"[퀵슬롯 자동 해제] 아이템 ({consumableItem.itemName})의 수량이 0이 되어 퀵슬롯 {i + 5}에서 해제됩니다.");
                UnregisterItem(i); // 아이템 해제 로직 호출
            }
        }
    }

    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 퀵슬롯 할당 정보를 InventoryControllerSaveData 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        int[] assignedItemIds = new int[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            assignedItemIds[i] = itemSlots[i] != null ? itemSlots[i].itemID : 0;
        }

        // ItemControllerSaveData가 별도 파일에 정의되어 있다고 가정하고 사용합니다.
        ItemControllerSaveData data = new ItemControllerSaveData
        {
            assignedItemIds = assignedItemIds
        };
        return data;
    }

    /// <summary>
    /// 로드된 데이터를 현재 퀵슬롯에 적용합니다.
    /// </summary>
    public void LoadData(object data)
    {
        if (data is ItemControllerSaveData loadedData)
        {
            if (loadedData.assignedItemIds != null && loadedData.assignedItemIds.Length == SLOT_COUNT)
            {
                for (int i = 0; i < SLOT_COUNT; i++)
                {
                    int itemId = loadedData.assignedItemIds[i];

                    if (itemId > 0)
                    {
                        BaseItemSO baseItem = itemDatabaseManager.GetItemByID(itemId);

                        if (baseItem is ConsumableItemSO itemToAssign)
                        {
                            RegisterItem(i, itemToAssign);
                        }
                        else
                        {
                            Debug.LogWarning($"아이템 ID {itemId}의 데이터를 찾을 수 없거나 소모품이 아닙니다. 퀵슬롯 {i + 5} 등록 실패.");
                            UnregisterItem(i);
                        }
                    }
                    else
                    {
                        UnregisterItem(i);
                    }
                }
            }
        }
    }
}