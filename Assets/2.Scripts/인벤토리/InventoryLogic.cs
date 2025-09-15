using System.Collections.Generic;

/// <summary>
/// 인벤토리의 모든 핵심 로직을 처리하는 클래스입니다.
/// InventoryData를 직접 조작하며, MonoBehaviour가 아닙니다.
/// </summary>
public class InventoryLogic
{
    // === 인벤토리에 아이템을 추가하는 로직 ===

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다.
    /// 아이템의 maxStack을 고려하여 겹치기 및 새 슬롯 추가를 처리합니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToAdd">추가할 아이템 정보입니다.</param>
    /// <param name="amount">추가할 아이템의 개수입니다.</param>
    /// <param name="inventorySize">인벤토리의 최대 슬롯 개수입니다.</param>
    /// <returns>아이템 추가에 성공했는지 여부를 반환합니다.</returns>
    public bool AddItem(InventoryData data, BaseItemSO itemToAdd, int amount, int inventorySize)
    {
        // 1. 겹쳐질 수 있는 아이템 슬롯을 찾습니다. (maxStack이 1보다 큰 경우)
        // 장비 아이템은 겹쳐지지 않으므로 이 루프를 건너뜁니다.
        for (int i = 0; i < data.inventoryItems.Count; i++)
        {
            if (data.inventoryItems[i].itemSO == itemToAdd && data.inventoryItems[i].stackCount < itemToAdd.maxStack)
            {
                // 2. 같은 아이템이 있고, 아직 가득 차지 않았다면 개수를 업데이트합니다.
                data.inventoryItems[i].stackCount += amount;
                return true;
            }
        }

        // 3. 인벤토리가 꽉 찼는지 확인합니다.
        if (data.inventoryItems.Count >= inventorySize)
        {
            return false;
        }

        // 4. 기존에 없던 아이템이거나, 겹칠 수 없는 아이템(장비 등)이라면 새로 추가합니다.
        data.inventoryItems.Add(new ItemData(itemToAdd, amount));
        return true;
    }

    // === 인벤토리에서 아이템을 제거하는 로직 ===

    /// <summary>
    /// 인벤토리에서 아이템을 제거합니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToRemove">제거할 아이템 정보입니다.</param>
    /// <param name="amount">제거할 아이템의 개수입니다.</param>
    /// <returns>아이템 제거에 성공했는지 여부를 반환합니다.</returns>
    public bool RemoveItem(InventoryData data, BaseItemSO itemToRemove, int amount)
    {
        // 제거할 아이템을 인벤토리에서 찾습니다.
        // 여러 슬롯에 나뉘어 있을 수 있으므로, 재고가 충분한 슬롯을 찾습니다.
        for (int i = 0; i < data.inventoryItems.Count; i++)
        {
            if (data.inventoryItems[i].itemSO == itemToRemove)
            {
                // 2. 개수가 충분한지 확인합니다.
                if (data.inventoryItems[i].stackCount >= amount)
                {
                    // 3. 개수를 차감하고, 0이 되면 슬롯을 제거합니다.
                    data.inventoryItems[i].stackCount -= amount;
                    if (data.inventoryItems[i].stackCount <= 0)
                    {
                        data.inventoryItems.RemoveAt(i);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    // === 장비 아이템 장착 및 해제 로직 ===

    /// <summary>
    /// 인벤토리의 장비 아이템을 장착합니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToEquip">장착할 장비 아이템 정보입니다.</param>
    /// <param name="inventorySize">인벤토리의 최대 슬롯 개수입니다.</param>
    public void EquipItem(InventoryData data, EquipmentItemSO itemToEquip, int inventorySize)
    {
        // 현재 장착된 슬롯에 이미 아이템이 있는지 확인합니다.
        if (data.equippedItems.ContainsKey(itemToEquip.equipSlot))
        {
            // 이미 장비가 있다면, 기존 장비를 해제하고 인벤토리로 되돌립니다.
            UnEquipItem(data, itemToEquip.equipSlot, inventorySize);
        }

        // 인벤토리에서 아이템을 제거합니다. (장착 시 인벤토리에서 사라져야 하므로)
        RemoveItem(data, itemToEquip, 1);

        // 장비 슬롯에 새로운 아이템을 추가합니다.
        data.equippedItems.Add(itemToEquip.equipSlot, itemToEquip);
    }

    /// <summary>
    /// 장착된 아이템을 해제하고 인벤토리로 되돌립니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="slotToUnEquip">해제할 장비 슬롯의 타입입니다.</param>
    /// <param name="inventorySize">인벤토리의 최대 슬롯 개수입니다.</param>
    public void UnEquipItem(InventoryData data, EquipSlot slotToUnEquip, int inventorySize)
    {
        if (data.equippedItems.ContainsKey(slotToUnEquip))
        {
            EquipmentItemSO itemToUnEquip = data.equippedItems[slotToUnEquip];

            // 장착된 아이템을 인벤토리로 되돌립니다.
            AddItem(data, itemToUnEquip, 1, inventorySize);

            // 장비 슬롯에서 아이템을 제거합니다.
            data.equippedItems.Remove(slotToUnEquip);
        }
    }
}