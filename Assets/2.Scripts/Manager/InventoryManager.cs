using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

/// <summary>
/// 플레이어의 인벤토리 아이템 관리를 담당하는 스크립트입니다.
/// 아이템 추가, 제거, 소모품 사용, 버리기 등의 로직을 처리하며,
/// PlayerEquipmentManager와 협력하여 장비 해제 시 아이템을 인벤토리로 되돌립니다.
/// SOLID: 단일 책임 원칙 (인벤토리 아이템 관리).
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // === 이벤트 ===
    /// <summary>
    /// 인벤토리 내용이 변경될 때마다 호출되는 이벤트입니다.
    /// UI 갱신에 사용됩니다.
    /// </summary>
    public event Action onInventoryChanged;

    /// <summary>
    /// 아이템이 인벤토리에 추가될 때 호출되는 이벤트입니다.
    /// QuestManager에 퀘스트 진행 상황을 알립니다.
    /// </summary>
    public event Action<int, int> OnItemAdded; // string -> int 변경

    /// <summary>
    /// 아이템이 인벤토리에서 제거될 때 호출되는 이벤트입니다.
    /// QuestManager에 퀘스트 진행 상황을 알립니다.
    /// </summary>
    public event Action<int, int> OnItemRemoved; // string -> int 변경

    // === 데이터 저장용 변수 ===
    [Header("인벤토리 데이터")]
    [Tooltip("에셋 파일로 저장된 인벤토리 데이터를 할당합니다.")]
    [SerializeField] private InventoryData inventoryData;

    [Tooltip("인벤토리의 최대 슬롯 개수입니다.")]
    [SerializeField] private int inventorySize = 80;

    // === MonoBehaviour 메서드 ===
    private void Start()
    {
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        if (inventoryData != null)
        {
            inventoryData.Initialize();
        }
        else
        {
            Debug.LogError("InventoryData SO가 InventoryManager에 할당되지 않았습니다!");
        }
    }

    /// <summary>
    /// 아이템을 인벤토리에 추가하는 메서드입니다.
    /// </summary>
    public bool AddItem(BaseItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int remainingAmount = amount;

        var existingItems = inventoryData.inventoryItems.Where(i => i.itemSO.itemID == item.itemID && i.stackCount < item.maxStack);
        foreach (var existingItem in existingItems)
        {
            int spaceLeft = item.maxStack - existingItem.stackCount;
            int addAmount = Mathf.Min(remainingAmount, spaceLeft);
            existingItem.stackCount += addAmount;
            remainingAmount -= addAmount;

            if (remainingAmount <= 0) break;
        }

        while (remainingAmount > 0)
        {
            if (inventoryData.inventoryItems.Count >= inventorySize)
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다. 모든 아이템을 추가할 수 없습니다.");
                onInventoryChanged?.Invoke();
                return false;
            }
            int newStackAmount = Mathf.Min(remainingAmount, item.maxStack);
            inventoryData.inventoryItems.Add(new ItemData(item, newStackAmount));
            remainingAmount -= newStackAmount;
        }

        OnItemAdded?.Invoke(item.itemID, amount); // int ID 사용

        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 인벤토리에서 BaseItemSO 객체를 사용하여 아이템을 제거하는 메서드입니다.
    /// 이 메서드는 BaseItemSO 객체를 인자로 받으며, 내부적으로 아이템 ID를 사용하는 다른 오버로드를 호출하여 중복 코드를 줄입니다.
    /// </summary>
    public bool RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // BaseItemSO의 ID를 사용하여 다른 오버로드 메서드를 호출합니다.
        return RemoveItem(item.itemID, amount);
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템 ID로 아이템을 제거하는 메서드입니다.
    /// QuestManager와 같은 외부 로직에서 아이템을 차감할 때 사용됩니다.
    /// SOLID: 단일 책임 원칙에 따라 QuestManager가 아닌 InventoryManager가 실제 아이템을 제거합니다.
    /// </summary>
    public bool RemoveItem(int itemID, int amount)
    {
        if (amount <= 0) return false;

        int totalCount = GetItemCount(itemID);
        if (totalCount < amount)
        {
            Debug.LogWarning($"인벤토리에 아이템 ID '{itemID}'가 부족합니다. (필요: {amount}, 현재: {totalCount})");
            return false;
        }

        int remainingAmount = amount;

        for (int i = inventoryData.inventoryItems.Count - 1; i >= 0 && remainingAmount > 0; i--)
        {
            if (inventoryData.inventoryItems[i].itemSO.itemID == itemID)
            {
                int removeAmount = Mathf.Min(remainingAmount, inventoryData.inventoryItems[i].stackCount);
                inventoryData.inventoryItems[i].stackCount -= removeAmount;
                remainingAmount -= removeAmount;

                if (inventoryData.inventoryItems[i].stackCount <= 0)
                {
                    inventoryData.inventoryItems.RemoveAt(i);
                }
            }
        }

        OnItemRemoved?.Invoke(itemID, amount);

        onInventoryChanged?.Invoke();
        return true;
    }


    /// <summary>
    /// 인벤토리에 특정 아이템이 필요한 개수만큼 있는지 확인합니다.
    /// QuestManager에서 퀘스트 완료 조건 확인 시 사용됩니다.
    /// </summary>
    public bool HasItem(int itemID, int requiredAmount) // string -> int 변경
    {
        return GetItemCount(itemID) >= requiredAmount;
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템의 총 개수를 계산합니다.
    /// </summary>
    public int GetItemCount(int itemID) // string -> int 변경
    {
        int totalCount = 0;
        foreach (var itemData in inventoryData.inventoryItems)
        {
            if (itemData.itemSO.itemID == itemID) // int ID 사용
            {
                totalCount += itemData.stackCount;
            }
        }
        return totalCount;
    }

    /// <summary>
    /// 소모 아이템을 사용하고 인벤토리에서 제거합니다.
    /// </summary>
    public void UseItem(ConsumableItemSO itemToUse)
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
    public void DiscardItem(BaseItemSO itemToRemove, int amount)
    {
        RemoveItem(itemToRemove, amount);
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 현재 인벤토리의 아이템 리스트를 반환합니다.
    /// </summary>
    public List<ItemData> GetInventoryItems()
    {
        return inventoryData.inventoryItems;
    }

    /// <summary>
    /// 현재 장착된 아이템 데이터를 가져옵니다. (PlayerEquipmentManager에서 참조할 때 사용)
    /// </summary>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        if (playerCharacter == null || playerCharacter.playerEquipmentManager == null)
        {
            Debug.LogError("PlayerEquipmentManager에 접근할 수 없습니다.");
            return null;
        }
        return playerCharacter.playerEquipmentManager.GetEquippedItems();
    }
}