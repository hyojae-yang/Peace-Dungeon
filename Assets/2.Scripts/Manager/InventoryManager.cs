using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug; // StackTrace를 사용하기 위해 추가

/// <summary>
/// 플레이어의 인벤토리 아이템 관리를 담당하는 스크립트입니다.
/// 아이템 추가, 제거, 소모품 사용, 버리기 등의 로직을 처리하며,
/// PlayerEquipmentManager와 협력하여 장비 해제 시 아이템을 인벤토리로 되돌립니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static InventoryManager Instance { get; private set; }

    // === 이벤트 ===
    /// <summary>
    /// 인벤토리 내용이 변경될 때마다 호출되는 이벤트입니다.
    /// </summary>
    public event Action onInventoryChanged;

    // === 데이터 저장용 변수 ===
    [Header("인벤토리 데이터")]
    [Tooltip("에셋 파일로 저장된 인벤토리 데이터를 할당합니다.")]
    [SerializeField] private InventoryData inventoryData;

    [Tooltip("인벤토리의 최대 슬롯 개수입니다.")]
    [SerializeField] private int inventorySize = 20;

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            if (inventoryData != null)
            {
                inventoryData.Initialize(); // 데이터를 초기화합니다.
            }
            else
            {
                Debug.LogError("InventoryData SO가 InventoryManager에 할당되지 않았습니다!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 아이템을 인벤토리에 추가하는 메서드입니다.
    /// </summary>
    /// <param name="item">추가할 아이템 데이터</param>
    /// <param name="amount">추가할 아이템의 개수</param>
    /// <returns>아이템이 모두 추가되었는지 여부</returns>
    public bool AddItem(BaseItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // 1. 겹칠 수 있는 아이템을 찾습니다.
        // 문제 해결: itemID로 동일한 아이템인지 확인합니다.
        for (int i = 0; i < inventoryData.inventoryItems.Count; i++)
        {
            if (inventoryData.inventoryItems[i].itemSO.itemID == item.itemID && inventoryData.inventoryItems[i].stackCount < item.maxStack)
            {
                int spaceLeft = item.maxStack - inventoryData.inventoryItems[i].stackCount;
                int addAmount = Mathf.Min(amount, spaceLeft);
                inventoryData.inventoryItems[i].stackCount += addAmount;

                // 남은 수량이 있다면, 재귀 호출하여 빈 슬롯에 추가합니다.
                if (addAmount < amount)
                {
                    return AddItem(item, amount - addAmount);
                }

                onInventoryChanged?.Invoke(); // 인벤토리 변경 이벤트 호출
                return true;
            }
        }

        // 2. 남은 수량이 있다면, 빈 슬롯을 찾아 새로 추가합니다.
        if (amount > 0)
        {
            if (inventoryData.inventoryItems.Count >= inventorySize)
            {
                Debug.LogWarning($"인벤토리가 가득 차서 아이템 '{item.itemName}'({amount}개)을(를) 추가할 수 없습니다.");
                return false;
            }
            // 이 시점에서는 item 자체가 아니라 새로운 ItemData를 생성해야 합니다.
            // item이 복제된 인스턴스이므로, 원본 SO의 itemID를 사용하여 ItemData를 생성하는 것이 더 안전합니다.
            inventoryData.inventoryItems.Add(new ItemData(item, amount));
            Debug.Log($"<color=green>아이템 추가:</color> {item.itemName} ({amount}개) 새 슬롯에 추가.");
        }

        onInventoryChanged?.Invoke(); // 인벤토리 변경 이벤트 호출
        return true;
    }

    /// <summary>
    /// 인벤토리에서 아이템을 제거하는 메서드입니다.
    /// </summary>
    /// <param name="item">제거할 아이템 SO</param>
    /// <param name="amount">제거할 개수</param>
    /// <returns>아이템 제거 성공 여부</returns>
    public bool RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // 제거할 아이템을 인벤토리에서 찾습니다. (가장 마지막에 추가된 동일 아이템부터 처리)
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
                    }
                    else
                    {
                        Debug.Log($"<color=red>아이템 제거:</color> {item.itemName} (-{amount}, 남은 수량: {inventoryData.inventoryItems[i].stackCount})");
                    }
                    onInventoryChanged?.Invoke();
                    return true; // 성공적으로 제거
                }
                else
                {
                    // 이 경우, 요청된 개수만큼 제거할 수 없으므로 경고만 하고 종료합니다.
                    Debug.LogWarning($"인벤토리에서 '{item.itemName}' 아이템을 {amount}개 제거하려 했으나, 현재 수량이 부족합니다. (현재: {inventoryData.inventoryItems[i].stackCount}개)");
                    return false; // 제거 실패
                }
            }
        }
        Debug.LogWarning($"인벤토리에 '{item.itemName}' 아이템이 없습니다. 제거할 수 없습니다.");
        return false; // 제거 실패
    }

    /// <summary>
    /// 소모 아이템을 사용하고 인벤토리에서 제거합니다.
    /// </summary>
    public void UseItem(ConsumableItemSO itemToUse, PlayerCharacter playerCharacter)
    {

        if (itemToUse == null || playerCharacter == null)
        {
            Debug.LogError("아이템 또는 플레이어 캐릭터가 유효하지 않습니다.");
            return;
        }

        itemToUse.Use(playerCharacter);
        RemoveItem(itemToUse, 1);
    }

    /// <summary>
    /// 아이템을 인벤토리에서 버립니다.
    /// </summary>
    /// <param name="itemToRemove">버릴 아이템 데이터</param>
    /// <param name="amount">버릴 아이템 개수</param>
    public void DiscardItem(BaseItemSO itemToRemove, int amount)
    {
        RemoveItem(itemToRemove, amount);
        onInventoryChanged?.Invoke();
        Debug.Log($"<color=red>아이템 버리기:</color> {itemToRemove.itemName} (수량: {amount})");
    }

    /// <summary>
    /// 현재 인벤토리의 아이템 리스트를 반환합니다.
    /// </summary>
    /// <returns>인벤토리 아이템 데이터 리스트</returns>
    public List<ItemData> GetInventoryItems()
    {
        return inventoryData.inventoryItems;
    }

    /// <summary>
    /// 현재 장착된 아이템 데이터를 가져옵니다. (PlayerEquipmentManager에서 참조할 때 사용)
    /// </summary>
    /// <returns>장착 아이템 딕셔너리</returns>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        // InventoryManager가 직접 장착 아이템 정보를 관리하지 않지만,
        // PlayerEquipmentManager가 필요할 때 호출할 수 있도록 참조를 제공합니다.
        // 실제 장비 정보는 PlayerEquipmentManager가 관리합니다.
        return PlayerEquipmentManager.Instance.GetEquippedItems();
    }
}