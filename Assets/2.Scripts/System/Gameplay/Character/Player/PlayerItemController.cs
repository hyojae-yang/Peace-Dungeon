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

        // 초기화 시점에 itemSlots 배열 크기 확인 및 초기화
        if (itemSlots.Length != SLOT_COUNT)
        {
            itemSlots = new ConsumableItemSO[SLOT_COUNT];
        }

        // Awake에서는 데이터를 건드리지 않습니다.
    }

    private void Start()
    {
        // SaveManager가 존재하면 등록합니다.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);

            // [핵심 수정]: 이어하기(Load Game)인 경우에만 UI 동기화 로직을 실행합니다.
            // SaveManager.IsNewGame이 false일 때 (즉, 이어하기 성공) 로드된 데이터를 UI에 반영해야 합니다.
            if (!SaveManager.Instance.IsNewGame)
            {
                // UI 컴포넌트들이 Start()를 완료할 시간을 준 후 UI 동기화를 시작합니다.
                StartCoroutine(Co_InitializeQuickSlotUI());
            }
        }

        // InventoryManager의 아이템 수량 변경 이벤트를 구독합니다.
        if (inventoryManager != null)
        {
            inventoryManager.OnItemQuantityChanged += CheckQuickSlotItemQuantity;
        }

        // [기존 Start()에서 초기 UI 동기화 이벤트 호출 로직 제거됨]: 
        // 새 게임에서는 필요 없으며, 이어하기에서는 Co_InitializeQuickSlotUI()가 처리합니다.
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

    /// <summary>
    /// 지정된 퀵슬롯 인덱스에 현재 등록된 아이템 데이터를 반환합니다.
    /// QuickSlotItemPanel의 초기 로드 시 수량 동기화를 위해 사용됩니다.
    /// </summary>
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
    /// 특정 슬롯에 소모품 아이템을 등록합니다. (외부 UI/인벤토리에서 호출)
    /// </summary>
    public void RegisterItem(int slotIndex, ConsumableItemSO itemToRegister)
    {
        if (slotIndex >= 0 && slotIndex < SLOT_COUNT)
        {
            itemSlots[slotIndex] = itemToRegister;
            // 아이템이 실제로 등록/변경될 때만 이벤트를 발생시켜 UI를 즉시 업데이트합니다.
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

    /// <summary>
    /// InventoryManager에서 아이템 수량이 변경될 때 호출되며,
    /// 퀵슬롯에 등록된 아이템의 수량이 0이 되었는지 확인하여 자동 해제합니다.
    /// </summary>
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
            if (itemSlots[i] == consumableItem)
            {
                Debug.Log($"[퀵슬롯 자동 해제] 아이템 ({consumableItem.itemName})의 수량이 0이 되어 퀵슬롯 {i + 5}에서 해제됩니다.");
                UnregisterItem(i); // 아이템 해제 로직 호출
            }
        }
    }

    // === UI 동기화 로직 (이어하기 시에만 사용) ===

    /// <summary>
        /// UI 컴포넌트들이 이벤트를 구독할 시간을 준 후, 현재 퀵슬롯 데이터를 UI에 동기화합니다.
        /// Start()에서 SaveManager.IsNewGame이 false일 때만 호출됩니다.
        /// </summary>
    private IEnumerator Co_InitializeQuickSlotUI()
    {
        // UI 컴포넌트들의 Start() 실행 완료를 보장하기 위해 한 프레임 지연합니다.
        yield return null;

        // itemSlots에 로드된 데이터를 UI에 알립니다.
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            // OnSlotItemChanged 이벤트 호출: 등록된 아이템 데이터로 UI 업데이트를 요청합니다.
            OnSlotItemChanged?.Invoke(i, itemSlots[i]);
        }
        Debug.Log("[PlayerItemController] 저장된 데이터 기반 퀵슬롯 UI 동기화 완료 (이어하기).");
    }


    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 퀵슬롯 할당 정보를 ItemControllerSaveData 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        int[] assignedItemIds = new int[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            // 아이템이 있으면 ID를, 없으면 0을 저장합니다.
            assignedItemIds[i] = itemSlots[i] != null ? itemSlots[i].itemID : 0;
        }

        ItemControllerSaveData data = new ItemControllerSaveData
        {
            assignedItemIds = assignedItemIds
        };
        return data;
    }

    /// <summary>
        /// 로드된 데이터를 현재 퀵슬롯에 적용합니다. 
    /// **오직 내부 데이터(`itemSlots`)만 업데이트**하고 UI 업데이트는 Start()에서 Co_InitializeQuickSlotUI()가 담당합니다.
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
                            // [수정]: RegisterItem 대신 직접 할당하여 UI 이벤트 발생을 막고 데이터만 로드합니다.
                            itemSlots[i] = itemToAssign;
                        }
                        else
                        {
                            Debug.LogWarning($"아이템 ID {itemId}의 데이터를 찾을 수 없거나 소모품이 아닙니다. 퀵슬롯 {i + 5} 등록 실패.");
                            itemSlots[i] = null; // 실패 시 데이터만 초기화
                        }
                    }
                    else
                    {
                        itemSlots[i] = null; // ID가 0이면 슬롯을 비웁니다.
                    }
                }
            }
        }
    }
}