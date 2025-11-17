using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 플레이어의 퀵슬롯 아이템 사용 및 관리를 담당하는 컨트롤러 스크립트입니다.
/// [SRP]: 퀵슬롯 아이템 할당, 5~8번 키 입력 감지 및 아이템 사용 요청 위임을 책임집니다.
/// 실제 아이템 사용 유효성 검사 및 수량 관리는 InventoryManager에 위임하여 책임을 분리합니다.
/// [SOLID]: 단일 책임 원칙 (퀵슬롯 관리 및 입력), 의존성 역전 원칙.
/// </summary>
public class PlayerItemController : MonoBehaviour, ISavable
{
    private const int SLOT_COUNT = 4;
    private const KeyCode STARTING_KEY_CODE = KeyCode.Alpha5; // 퀵슬롯 시작 키 (5번 키)

    // === 종속성 ===
    private PlayerCharacter playerCharacter; // 플레이어 캐릭터 인스턴스 (종속성 해결용)
    private InventoryManager inventoryManager; // 인벤토리 관리자 (아이템 사용 및 수량 확인 위임)
    private ItemDatabaseManager itemDatabaseManager; // 아이템 데이터베이스 (세이브 로드 시 ID로 아이템 데이터 찾기)

    // === 데이터 ===
    [Header("아이템 퀵슬롯 할당")]
    [Tooltip("5~8 키에 할당할 소모품 아이템 데이터 (ConsumableItemSO)를 할당합니다.")]
    public ConsumableItemSO[] itemSlots = new ConsumableItemSO[SLOT_COUNT]; // 퀵슬롯에 등록된 아이템 데이터 배열

    // === 이벤트 ===
    /// <summary>
    /// 퀵슬롯의 내용이 변경될 때 호출되는 이벤트입니다.
    /// int: 슬롯 인덱스 (0~3), ConsumableItemSO: 등록된 아이템 데이터 (해제 시 null)
    /// </summary>
    public event Action<int, ConsumableItemSO> OnSlotItemChanged;

    // === MonoBehaviour 메서드 ===

    private void Awake()
    {
        // 싱글톤 인스턴스 및 종속성 초기화
        playerCharacter = PlayerCharacter.Instance;
        inventoryManager = playerCharacter?.inventoryManager;
        itemDatabaseManager = ItemDatabaseManager.Instance;

        if (playerCharacter == null || inventoryManager == null || itemDatabaseManager == null)
        {
            Debug.LogError("핵심 종속성(PlayerCharacter, InventoryManager, ItemDatabaseManager) 중 하나 이상을 찾을 수 없습니다.");
            enabled = false;
            return; // 종속성 실패 시 중단
        }

        // 초기화 시점에 itemSlots 배열 크기 확인 및 초기화 (Inspector에서 잘못 설정되었을 경우 대비)
        if (itemSlots.Length != SLOT_COUNT)
        {
            itemSlots = new ConsumableItemSO[SLOT_COUNT];
        }

        // Awake에서는 데이터를 건드리지 않습니다. (데이터 로드는 LoadData에서)
    }

    private void Start()
    {
        // SaveManager가 존재하면 ISavable로 등록합니다.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);

            // [핵심 수정: UI 초기화 로직 제거]
            // QuickSlotItemPanel.Start()의 RefreshAllSlotQuantities()가 초기 UI 동기화 책임을 맡습니다.
            // 따라서 뒤늦게 중복 이벤트를 발생시켜 타이밍 충돌을 일으키는 이 코루틴 호출을 제거합니다.
            // StartCoroutine(Co_InitializeQuickSlotUI());
        }

        // InventoryManager의 아이템 수량 변경 이벤트를 구독하여 퀵슬롯 자동 해제 로직을 연결합니다.
        if (inventoryManager != null)
        {
            inventoryManager.OnItemQuantityChanged += CheckQuickSlotItemQuantity;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (inventoryManager != null)
        {
            inventoryManager.OnItemQuantityChanged -= CheckQuickSlotItemQuantity;
        }
    }

    void Update()
    {
        // 5, 6, 7, 8 키 입력 감지 및 아이템 사용 요청
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            KeyCode quickSlotKey = STARTING_KEY_CODE + i;
            if (Input.GetKeyDown(quickSlotKey))
            {
                UseQuickSlotItem(i);
            }
        }
    }

    /// <summary>
    /// 지정된 퀵슬롯 인덱스에 현재 등록된 아이템 데이터를 반환합니다.
    /// QuickSlotItemPanel의 초기 로드 시 수량 동기화를 위해 사용됩니다.
    /// </summary>
    /// <param name="index">퀵슬롯 인덱스 (0~3)</param>
    /// <returns>등록된 ConsumableItemSO 또는 null</returns>
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
    /// <param name="slotIndex">퀵슬롯 인덱스 (0~3)</param>
    public void UseQuickSlotItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SLOT_COUNT)
        {
            Debug.LogError($"[QuickSlot Error] 잘못된 퀵슬롯 인덱스입니다: {slotIndex}");
            return;
        }

        ConsumableItemSO itemToUse = itemSlots[slotIndex];

        // InventoryManager에 아이템 사용을 요청합니다.
        inventoryManager?.UseItem(itemToUse);
    }

    // === 퀵슬롯 등록 및 해제 로직 ===

    /// <summary>
    /// 특정 슬롯에 소모품 아이템을 등록합니다. (외부 UI/인벤토리에서 호출)
    /// </summary>
    /// <param name="slotIndex">등록할 퀵슬롯 인덱스 (0~3)</param>
    /// <param name="itemToRegister">등록할 ConsumableItemSO</param>
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
    /// <param name="slotIndex">해제할 퀵슬롯 인덱스 (0~3)</param>
    public void UnregisterItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < SLOT_COUNT)
        {
            itemSlots[slotIndex] = null;
            // 아이템이 해제되었음을 UI에 알립니다.
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
    /// <param name="itemSO">수량이 변경된 아이템 데이터</param>
    /// <param name="newQuantity">변경 후 새로운 수량</param>
    private void CheckQuickSlotItemQuantity(BaseItemSO itemSO, int newQuantity)
    {
        // 수량이 0이 된 소모품 아이템만 처리합니다.
        if (newQuantity > 0 || !(itemSO is ConsumableItemSO consumableItem))
        {
            return;
        }

        // 모든 퀵슬롯을 순회하며 해당 아이템을 해제합니다.
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (itemSlots[i] == consumableItem)
            {
                UnregisterItem(i); // 아이템 해제 로직 호출 (이때 UI 이벤트도 발생)
            }
        }
    }

    // === UI 동기화 로직 (제거됨) ===
    // [핵심 수정: 제거] Co_InitializeQuickSlotUI() 코루틴이 제거되었습니다.
    // 초기 UI 동기화는 QuickSlotItemPanel.Start()의 RefreshAllSlotQuantities()가 전적으로 담당합니다.

    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 퀵슬롯 상태를 저장할 데이터 객체를 생성합니다.
    /// </summary>
    public object SaveData()
    {
        int[] assignedItemIds = new int[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            // 아이템이 있으면 ID를, 없으면 0을 저장합니다. (0은 유효하지 않은 ID로 간주)
            assignedItemIds[i] = itemSlots[i] != null ? itemSlots[i].itemID : 0;
        }

        ItemControllerSaveData data = new ItemControllerSaveData
        {
            assignedItemIds = assignedItemIds
        };
        return data;
    }

    /// <summary>
    /// 저장된 데이터로부터 퀵슬롯 상태를 복원합니다. (Start() 이전에 LoadManager에서 호출됨)
    /// </summary>
    /// <param name="data">복원할 ItemControllerSaveData 객체</param>
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
                            // UI 이벤트 없이 데이터만 로드합니다.
                            itemSlots[i] = itemToAssign;
                        }
                        else
                        {
                            Debug.LogWarning($"아이템 ID {itemId}의 데이터를 찾을 수 없거나 소모품이 아닙니다. 퀵슬롯 {i + 5} 등록 실패.");
                            itemSlots[i] = null;
                        }
                    }
                    else
                    {
                        itemSlots[i] = null;
                    }
                }
            }
        }
    }
}