using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 장비 제거 시 LINQ를 사용하기 위해 추가

/// <summary>
/// 인벤토리의 모든 핵심 로직을 처리하는 클래스입니다.
/// InventoryData의 7개 독립된 저장소를 직접 조작하며, MonoBehaviour가 아닙니다.
/// SOLID: 단일 책임 원칙 (SRP) 및 개방-폐쇄 원칙 (OCP)을 준수합니다.
/// </summary>
public class InventoryLogic
{
    // =======================================================================
    // === 상수: 인벤토리 패널별 최대 크기 정의 ===
    // =======================================================================

    /// <summary> 장비 아이템 패널의 최대 크기입니다. </summary>
    private const int EQUIPMENT_PANEL_SIZE = 64;

    /// <summary> 일반 아이템 패널의 최대 크기입니다. </summary>
    private const int GENERAL_PANEL_SIZE = 80;

    // =======================================================================
    // === 헬퍼 메서드: 아이템-인벤토리 매핑 (SOLID: SRP) ===
    // =======================================================================

    /// <summary>
    /// BaseItemSO를 기반으로 해당 아이템이 속해야 할 List와 그 List의 최대 크기를 반환합니다.
    /// 이 메서드는 7개로 분리된 인벤토리 구조를 추상화합니다.
    /// </summary>
    /// <param name="data">아이템 데이터 SO</param>
    /// <param name="itemSO">매핑할 아이템 SO</param>
    /// <returns>대상 List<ItemData>와 최대 크기(maxSize) 튜플</returns>
    private (List<ItemData> list, int maxSize) GetInventoryMapping(InventoryData data, BaseItemSO itemSO)
    {
        // 1. 일반 아이템 (ItemType 기준 분기)
        switch (itemSO.itemType)
        {
            case ItemType.Consumable: return (data.consumableItems, GENERAL_PANEL_SIZE);
            case ItemType.Material: return (data.materialItems, GENERAL_PANEL_SIZE);
            case ItemType.Quest: return (data.questItems, GENERAL_PANEL_SIZE);
            case ItemType.Special: return (data.specialItems, GENERAL_PANEL_SIZE);

            // 2. 장비 아이템 (EquipmentItemSO 캐스팅 후 EquipType 기준 분기)
            case ItemType.Equipment:
                // 장비 아이템 SO가 맞는지 확인하고, 맞다면 EquipType으로 한 번 더 분기합니다.
                if (itemSO is EquipmentItemSO equipmentSO)
                {
                    switch (equipmentSO.equipType)
                    {
                        case EquipType.Weapon: return (data.weaponItems, EQUIPMENT_PANEL_SIZE);
                        case EquipType.Armor: return (data.armorItems, EQUIPMENT_PANEL_SIZE);
                        case EquipType.Accessory: return (data.accessoryItems, EQUIPMENT_PANEL_SIZE);
                        // 장비 타입이 정의되지 않은 경우
                        default: return (null, 0);
                    }
                }
                // ItemType은 Equipment인데, EquipmentItemSO가 아닌 경우 (데이터 오류)
                return (null, 0);

            // 처리되지 않은 ItemType
            default: return (null, 0);
        }
    }

    /// <summary>
    /// GetInventoryMapping의 결과를 기반으로 ItemData 리스트만 반환하는 헬퍼 메서드입니다.
    /// </summary>
    private List<ItemData> GetTargetList(InventoryData data, BaseItemSO itemSO)
    {
        return GetInventoryMapping(data, itemSO).list;
    }

    // =======================================================================
    // === 인벤토리에 아이템을 추가하는 로직 (단일 AddItem로 OCP 구현) ===
    // =======================================================================

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다. (7개 독립된 패널 처리)
    /// 아이템의 maxStack을 고려하여 겹치기 및 새 슬롯 추가를 처리합니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToAdd">추가할 아이템 정보입니다.</param>
    /// <param name="amount">추가할 아이템의 개수입니다.</param>
    /// <returns>아이템 추가에 성공했는지 여부를 반환합니다.</returns>
    public bool AddItem(InventoryData data, BaseItemSO itemToAdd, int amount)
    {
        if (itemToAdd == null || amount <= 0) return false;

        // 1. 아이템이 들어갈 타겟 리스트와 최대 크기를 결정합니다.
        var mapping = GetInventoryMapping(data, itemToAdd);
        List<ItemData> targetList = mapping.list;
        int maxSize = mapping.maxSize;

        if (targetList == null || maxSize == 0)
        {
            Debug.LogError($"[InventoryLogic] 아이템 {itemToAdd.itemName} ({itemToAdd.itemType})에 해당하는 유효한 인벤토리 패널을 찾을 수 없습니다.");
            return false;
        }

        int remainingAmount = amount;

        // 2. 겹쳐질 수 있는 아이템 슬롯을 찾습니다. (장비(maxStack=1)는 이 루프를 건너뜁니다.)
        if (itemToAdd.maxStack > 1)
        {
            // 이 로직은 오직 targetList에 대해서만 실행됩니다.
            var existingItems = targetList.Where(i =>
                i.itemSO.itemID == itemToAdd.itemID && i.stackCount < itemToAdd.maxStack
            );

            foreach (var existingItem in existingItems.ToList()) // ToList()로 복사하여 안전하게 순회
            {
                int spaceLeft = itemToAdd.maxStack - existingItem.stackCount;
                int addAmount = Mathf.Min(remainingAmount, spaceLeft);
                existingItem.stackCount += addAmount;
                remainingAmount -= addAmount;

                if (remainingAmount <= 0) break;
            }
        }

        // 3. 남은 아이템을 새 슬롯에 추가합니다.
        while (remainingAmount > 0)
        {
            // 타겟 인벤토리가 꽉 찼는지 확인합니다.
            if (targetList.Count >= maxSize)
            {
                return false; // 해당 패널이 꽉 찼으므로 실패를 반환합니다.
            }

            int newStackAmount = Mathf.Min(remainingAmount, itemToAdd.maxStack);

            // 장비 아이템은 항상 1스택으로 추가됩니다. (maxStack=1이므로 newStackAmount=1)
            targetList.Add(new ItemData(itemToAdd, newStackAmount));
            remainingAmount -= newStackAmount;
        }

        return true;
    }

    // =======================================================================
    // === 인벤토리에서 아이템을 제거하는 로직 ===
    // =======================================================================

    /// <summary>
    /// 인벤토리에서 특정 아이템 ID를 가진 아이템을 제거합니다. (7개 패널 모두 처리)
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToRemoveSO">제거할 아이템 정보입니다.</param>
    /// <param name="amount">제거할 아이템의 개수입니다.</param>
    /// <returns>아이템 제거에 성공했는지 여부를 반환합니다.</returns>
    public bool RemoveItem(InventoryData data, BaseItemSO itemToRemoveSO, int amount)
    {
        if (itemToRemoveSO == null || amount <= 0) return false;

        // 1. 타겟 리스트를 결정합니다. (7개 중 1개)
        List<ItemData> targetList = GetTargetList(data, itemToRemoveSO);
        if (targetList == null) return false;

        // 2. 해당 리스트 내에서 충분한 수량이 있는지 확인합니다. (성능을 위해 Count 전에 검사)
        int currentCount = targetList.Where(i => i.itemSO.itemID == itemToRemoveSO.itemID).Sum(i => i.stackCount);
        if (currentCount < amount)
        {
            //Debug.LogWarning($"제거 실패: {itemToRemoveSO.itemName} ({itemToRemoveSO.itemID}) 재고 부족. (필요: {amount}, 현재: {currentCount})");
            return false;
        }

        int remainingAmount = amount;

        // 3. 역순으로 순회하며 제거합니다. (리스트 중간 제거 시 인덱스 오류 방지)
        for (int i = targetList.Count - 1; i >= 0 && remainingAmount > 0; i--)
        {
            ItemData itemData = targetList[i];

            // 장비 아이템(maxStack=1)은 BaseItemSO 비교로 제거하면 안 됩니다. 
            // 장비는 항상 RemoveItem(uniqueID)로만 제거되도록 유도합니다.
            if (itemData.itemSO is EquipmentItemSO) continue;

            if (itemData.itemSO.itemID == itemToRemoveSO.itemID)
            {
                int removeAmount = Mathf.Min(remainingAmount, itemData.stackCount);
                itemData.stackCount -= removeAmount;
                remainingAmount -= removeAmount;

                if (itemData.stackCount <= 0)
                {
                    targetList.RemoveAt(i);
                }
            }
        }

        // 장비 아이템의 경우, RemoveItem(uniqueID)를 사용해야 정확한 인스턴스를 찾을 수 있습니다.
        // 따라서 이 메서드는 일반 아이템(소모품, 재료 등) 제거에만 주로 사용됩니다.
        return remainingAmount == 0;
    }

    /// <summary>
    /// 특정 고유 ID를 가진 장비 아이템 인스턴스를 인벤토리에서 제거하는 메서드입니다. (장비 전용)
    /// 이 메서드는 장비 착용 또는 버리기(Unique 장비) 시 호출됩니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="uniqueID">제거할 장비 아이템의 고유 ID</param>
    /// <returns>아이템 제거 성공 여부</returns>
    public bool RemoveItem(InventoryData data, string uniqueID)
    {
        if (string.IsNullOrEmpty(uniqueID)) return false;

        // 1. 3개의 장비 리스트를 순회하며 해당 uniqueID를 가진 아이템을 찾습니다.
        List<List<ItemData>> equipmentLists = new List<List<ItemData>>
        {
            data.weaponItems,
            data.armorItems,
            data.accessoryItems
        };

        foreach (var list in equipmentLists)
        {
            // 2. 각 리스트를 역순으로 순회하며 제거합니다.
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ItemData itemData = list[i];

                // ItemData의 itemSO가 EquipmentItemSO 타입인지 확인
                if (itemData.itemSO is EquipmentItemSO equipmentSO)
                {
                    if (equipmentSO.uniqueID == uniqueID)
                    {
                        // 장비는 항상 1스택이므로, 바로 제거합니다.
                        list.RemoveAt(i);
                        //Debug.Log($"장비 제거 성공: ID {uniqueID} from {list}");
                        return true; // 제거 성공
                    }
                }
            }
        }

        //Debug.LogWarning($"장비 제거 실패: 인벤토리에서 고유 ID '{uniqueID}'를 가진 장비를 찾을 수 없습니다.");
        return false; // 제거 실패
    }

    // =======================================================================
    // === 장비 아이템 장착 및 해제 로직 ===
    // =======================================================================

    /// <summary>
    /// 인벤토리의 장비 아이템을 장착합니다. (7개 패널 대응)
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToEquip">장착할 장비 아이템 정보입니다.</param>
    /// <param name="inventorySize">기존 호환성 유지를 위해 남겨둔 매개변수 (이제 사용되지 않음)</param>
    public void EquipItem(InventoryData data, EquipmentItemSO itemToEquip, int inventorySize)
    {
        if (itemToEquip == null) return;

        // 1. 현재 장착된 슬롯에 이미 아이템이 있는지 확인합니다.
        if (data.equippedItems.ContainsKey(itemToEquip.equipSlot))
        {
            // 2. 이미 장비가 있다면, 기존 장비를 해제하고 인벤토리로 되돌립니다.
            // AddItem의 시그니처가 변경되었으므로, UnEquipItem도 변경되어야 합니다.
            // (UnEquipItem의 로직은 아래에서 수정됩니다.)
            UnEquipItem(data, itemToEquip.equipSlot);
        }

        // 3. 인벤토리에서 아이템을 제거합니다. (장착 시 인벤토리에서 사라져야 하므로)
        // 장비 아이템은 고유 ID로 정확하게 제거합니다.
        // inventorySize 매개변수는 호환성 유지를 위해 제거했습니다.
        RemoveItem(data, itemToEquip.uniqueID);

        // 4. 장비 슬롯에 새로운 아이템을 추가합니다.
        if (data.equippedItems.ContainsKey(itemToEquip.equipSlot))
        {
            // 이전에 UnEquipItem에서 제거된 후, 다시 Add하는 경우에 대비
            data.equippedItems[itemToEquip.equipSlot] = itemToEquip;
        }
        else
        {
            data.equippedItems.Add(itemToEquip.equipSlot, itemToEquip);
        }
    }

    /// <summary>
    /// 장착된 아이템을 해제하고 인벤토리로 되돌립니다. (7개 패널 대응)
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="slotToUnEquip">해제할 장비 슬롯의 타입입니다.</param>
    // 기존 매개변수였던 int inventorySize를 제거하고 오버로딩 또는 기본값 처리를 통해 호환성을 유지해야 합니다.
    // 여기서는 매개변수를 제거하는 것이 로직상 자연스러워 제거합니다. (Manager에서 호출할 때 조정 필요)
    public void UnEquipItem(InventoryData data, EquipSlot slotToUnEquip)
    {
        if (data.equippedItems.ContainsKey(slotToUnEquip))
        {
            EquipmentItemSO itemToUnEquip = data.equippedItems[slotToUnEquip];

            // 1. 장착된 아이템을 인벤토리로 되돌립니다.
            // AddItem 메서드의 시그니처가 변경되었으므로, 이제 inventorySize 매개변수 없이 호출합니다.
            // AddItem(data, itemToUnEquip, 1)만 호출하면 7개 패널 로직에 의해 자동으로 적절한 장비 패널에 추가됩니다.
            bool success = AddItem(data, itemToUnEquip, 1);

            if (success)
            {
                // 2. 인벤토리에 되돌리는 데 성공했다면, 장비 슬롯에서 아이템을 제거합니다.
                data.equippedItems.Remove(slotToUnEquip);
            }
            // 인벤토리 공간 부족으로 해제 실패 시 (장착 상태 유지)
            else
            {
                Debug.LogWarning($"장비 해제 실패: 인벤토리 공간 부족으로 {itemToUnEquip.itemName}을 되돌릴 수 없습니다.");
            }
        }
    }

    // =======================================================================
    // === 아이템 카운트 로직 추가 (InventoryManager에서 위임받을 예정) ===
    // =======================================================================

    /// <summary>
    /// 인벤토리 내의 모든 7개 패널에서 특정 아이템 ID의 총 개수를 계산합니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemID">개수를 확인할 아이템 ID입니다.</param>
    /// <returns>총 개수</returns>
    public int GetItemCount(InventoryData data, int itemID)
    {
        int totalCount = 0;

        // 7개의 모든 리스트를 포함하는 리스트를 만듭니다. (반복되는 코드 줄이기)
        List<List<ItemData>> allLists = new List<List<ItemData>>
        {
            data.weaponItems, data.armorItems, data.accessoryItems,
            data.consumableItems, data.materialItems, data.questItems, data.specialItems
        };

        foreach (var list in allLists)
        {
            // 각 리스트에서 아이템 ID가 일치하는 모든 스택의 개수를 합산합니다.
            totalCount += list
                .Where(itemData => itemData.itemSO.itemID == itemID)
                .Sum(itemData => itemData.stackCount);
        }

        return totalCount;
    }
}