using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 이 스크립트는 PlayerItemController의 퀵슬롯 변경 이벤트를 구독하여 UI를 업데이트하는 중개자 역할을 합니다.
/// 또한, InventoryManager의 아이템 수량 변경 이벤트를 구독하여 퀵슬롯에 등록된 아이템의 수량 UI를 실시간으로 업데이트합니다.
/// [SRP]: 데이터 변경 이벤트를 감지하고 UI 컴포넌트에 전달하는 역할만 수행합니다.
/// [OCP/강건성]: 이벤트 구독에 실패할 경우, Update 루프를 통해 성공할 때까지 재등록을 시도하여 강건성을 높입니다.
/// </summary>
public class QuickSlotItemPanel : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("개별 퀵슬롯 UI를 담당하는 QuickSlotItemUI 컴포넌트 배열입니다.")]
    public QuickSlotItemUI[] quickSlotUIs; // 개별 UI 스크립트 배열

    // 중앙 허브 역할을 하는 PlayerItemController 인스턴스에 대한 참조
    private PlayerItemController playerItemController;

    // InventoryManager 참조 (수량 업데이트 이벤트를 구독하기 위함)
    private InventoryManager inventoryManager;

    // 이벤트 구독 성공 여부를 추적하여 불필요한 재구독을 방지합니다.
    private bool isSubscribed = false;

    private void Awake()
    {
        // 싱글톤 패턴으로 구현된 PlayerCharacter 인스턴스를 가져옵니다.
        PlayerCharacter playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogWarning("[QSIP] PlayerCharacter 인스턴스가 아직 존재하지 않습니다. Update에서 재시도합니다.");
            return;
        }

        // PlayerCharacter에서 핵심 컴포넌트를 가져옵니다.
        // 이때 참조가 null일 경우, Start/Update에서 재시도합니다.
        playerItemController = playerCharacter.GetComponent<PlayerItemController>();
        inventoryManager = playerCharacter.inventoryManager;
    }

    private void Start()
    {
        // 게임 시작 시 한 번 구독을 시도합니다.
        TrySubscribeToEvents();

        // Start 시점에 인벤토리 데이터가 모두 로드되었다고 가정하고 수량을 동기화합니다.
        // 구독 성공 여부와 관계없이 초기화는 시도합니다.
        RefreshAllSlotQuantities();
    }

    /// <summary>
    /// 구독에 실패했을 경우, 매 프레임 컴포넌트 참조 및 구독을 재시도합니다.
    /// 구독 성공 시, 이 로직은 더 이상 실행되지 않습니다.
    /// </summary>
    private void Update()
    {
        // 구독이 아직 성공하지 않았다면 재시도 로직을 실행합니다.
        if (!isSubscribed)
        {
            TrySubscribeToEvents();
        }
    }

    /// <summary>
    /// 핵심 컴포넌트의 참조를 확인하고, 유효할 경우 이벤트 구독을 시도합니다.
    /// </summary>
    private void TrySubscribeToEvents()
    {
        // 1. 참조가 누락된 경우, PlayerCharacter 인스턴스를 다시 확인합니다.
        if (playerItemController == null || inventoryManager == null)
        {
            PlayerCharacter playerCharacter = PlayerCharacter.Instance;
            if (playerCharacter != null)
            {
                playerItemController = playerCharacter.GetComponent<PlayerItemController>();
                inventoryManager = playerCharacter.inventoryManager;
            }
        }

        // 2. 모든 핵심 컴포넌트가 유효한지 최종 확인합니다.
        if (playerItemController != null && inventoryManager != null)
        {
            // [구독 로직]
            playerItemController.OnSlotItemChanged += UpdateQuickSlotImageUI;
            inventoryManager.OnItemQuantityChanged += UpdateQuickSlotCountUI;

            // 구독 성공 상태 플래그를 true로 변경합니다.
            isSubscribed = true;

            // 구독에 성공했으므로 초기화 수량을 다시 한번 동기화합니다.
            RefreshAllSlotQuantities();
        }
        else if (PlayerCharacter.Instance != null)
        {
            // PlayerCharacter는 있지만, 핵심 컴포넌트가 여전히 null인 경우 (설정 오류 가능성)
            Debug.LogWarning("[QSIP] 핵심 컴포넌트(PlayerItemController 또는 InventoryManager) 중 하나가 아직 PlayerCharacter에 할당되지 않아 구독을 지연합니다.");
        }
        else
        {
            // PlayerCharacter 인스턴스가 아직 준비되지 않은 경우
            // 이 경우가 가장 흔한 재시도 케이스입니다. 경고를 너무 자주 출력하지 않도록 주의합니다.
        }
    }

    /// <summary>
    /// 로드 시 또는 필요 시, PlayerItemController에 등록된 모든 퀵슬롯 아이템의 현재 수량을 조회하여 UI를 갱신합니다.
    /// 이 메서드는 세이브 로드 후 수량 동기화에 사용됩니다.
    /// </summary>
    public void RefreshAllSlotQuantities()
    {
        // 핵심 컴포넌트가 없으면 동작하지 않습니다.
        if (playerItemController == null || inventoryManager == null) return;

        for (int i = 0; i < quickSlotUIs.Length; i++)
        {
            // PlayerItemController로부터 해당 슬롯에 등록된 아이템을 가져옵니다.
            ConsumableItemSO registeredItem = playerItemController.GetItemInSlot(i);

            // 아이콘 UI를 강제로 업데이트합니다. (등록된 아이템이 있으면 표시, null이면 숨김)
            quickSlotUIs[i].UpdateUI(registeredItem);

            if (registeredItem != null)
            {
                // 인벤토리 매니저로부터 현재 수량을 조회합니다.
                int currentQuantity = inventoryManager.GetItemQuantity(registeredItem);

                // UI 수량 업데이트를 요청합니다.
                quickSlotUIs[i].UpdateStackCountUI(currentQuantity);
            }
            else
            {
                // 아이템이 등록되어 있지 않은 경우 수량을 0으로 설정하여 텍스트를 숨깁니다.
                quickSlotUIs[i].UpdateStackCountUI(0);
            }
        }
    }

    /// <summary>
    /// PlayerItemController의 OnSlotItemChanged 이벤트로부터 호출되어 퀵슬롯 UI 이미지를 업데이트합니다.
    /// (아이템 등록/해제 시 호출)
    /// </summary>
    /// <param name="slotIndex">갱신이 필요한 슬롯의 인덱스</param>
    /// <param name="data">슬롯에 등록된 소모품 아이템 데이터 (해제 시 null)</param>
    private void UpdateQuickSlotImageUI(int slotIndex, ConsumableItemSO data)
    {
        if (slotIndex >= 0 && slotIndex < quickSlotUIs.Length)
        {
            // 1. UI 스크립트에 데이터만 전달하여 이미지 업데이트를 요청합니다.
            quickSlotUIs[slotIndex].UpdateUI(data);

            // 2. 퀵슬롯에 아이템이 새로 등록되거나 해제될 때, 수량을 즉시 동기화합니다.
            if (data != null)
            {
                // 인벤토리 매니저로부터 현재 수량을 조회합니다.
                int initialQuantity = inventoryManager.GetItemQuantity(data);

                // UI 수량 업데이트를 요청합니다.
                quickSlotUIs[slotIndex].UpdateStackCountUI(initialQuantity);
            }
            else
            {
                // 아이템이 해제되었다면 수량도 0으로 업데이트하여 숨깁니다.
                quickSlotUIs[slotIndex].UpdateStackCountUI(0);
            }
        }
        else
        {
            Debug.LogError("[QSIP] 잘못된 퀵슬롯 인덱스입니다: " + slotIndex);
        }
    }

    /// <summary>
    /// InventoryManager의 OnItemQuantityChanged 이벤트로부터 호출되어 퀵슬롯에 등록된 아이템의 수량을 업데이트합니다.
    /// 이 메서드는 실시간 소모 시 호출됩니다.
    /// </summary>
    /// <param name="itemSO">수량이 변경된 아이템 데이터 (BaseItemSO)</param>
    /// <param name="newQuantity">변경 후 남은 새로운 수량</param>
    private void UpdateQuickSlotCountUI(BaseItemSO itemSO, int newQuantity)
    {
        // [수정]: BaseItemSO를 ConsumableItemSO로 안전하게 형변환합니다.
        if (itemSO is ConsumableItemSO consumableItem)
        {
            // 퀵슬롯 배열을 순회하며 변경된 소모품 아이템이 등록되어 있는지 확인합니다.
            for (int i = 0; i < quickSlotUIs.Length; i++)
            {
                // QuickSlotItemUI의 IsDisplayingItem 메서드를 활용하여 현재 아이템과 동일한지 확인
                if (quickSlotUIs[i].IsDisplayingItem(consumableItem))
                {
                    // 수량 텍스트 업데이트를 요청합니다.
                    quickSlotUIs[i].UpdateStackCountUI(newQuantity);
                }
            }
        }
    }

    private void OnDisable()
    {
        // 구독이 성공했을 때만 해제를 시도합니다.
        if (isSubscribed)
        {
            // 이벤트 구독 해제
            if (playerItemController != null)
            {
                playerItemController.OnSlotItemChanged -= UpdateQuickSlotImageUI;
            }

            if (inventoryManager != null)
            {
                inventoryManager.OnItemQuantityChanged -= UpdateQuickSlotCountUI;
            }

            // 구독 상태를 재설정하여, 혹시라도 오브젝트가 재활성화되면 다시 구독을 시도할 수 있도록 준비합니다.
            isSubscribed = false;
        }
    }
}