// InventoryLogic.cs
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
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="itemToAdd">추가할 아이템 정보입니다.</param>
    /// <param name="amount">추가할 아이템의 개수입니다.</param>
    public void AddItem(InventoryData data, BaseItemSO itemToAdd, int amount)
    {
        // 이미 인벤토리에 같은 아이템이 있는지 확인합니다.
        if (data.inventoryItems.ContainsKey(itemToAdd))
        {
            // 아이템이 이미 있다면 개수를 늘려줍니다.
            data.inventoryItems[itemToAdd] += amount;
            // TODO: UI에 업데이트를 알리는 이벤트 호출 로직 추가 예정
        }
        else
        {
            // 아이템이 없다면 새로 추가합니다.
            data.inventoryItems.Add(itemToAdd, amount);
            // TODO: UI에 업데이트를 알리는 이벤트 호출 로직 추가 예정
        }
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
        // 인벤토리에 해당 아이템이 있는지 확인합니다.
        if (data.inventoryItems.ContainsKey(itemToRemove))
        {
            // 개수가 충분한지 확인합니다.
            if (data.inventoryItems[itemToRemove] >= amount)
            {
                data.inventoryItems[itemToRemove] -= amount;

                // 아이템 개수가 0이 되면 딕셔너리에서 완전히 제거합니다.
                if (data.inventoryItems[itemToRemove] <= 0)
                {
                    data.inventoryItems.Remove(itemToRemove);
                }

                // TODO: UI에 업데이트를 알리는 이벤트 호출 로직 추가 예정
                return true;
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
    public void EquipItem(InventoryData data, EquipmentItemSO itemToEquip)
    {
        // 현재 장착된 슬롯에 이미 아이템이 있는지 확인합니다.
        if (data.equippedItems.ContainsKey(itemToEquip.equipSlot))
        {
            // 이미 장비가 있다면, 기존 장비를 해제하고 인벤토리로 되돌립니다.
            UnEquipItem(data, itemToEquip.equipSlot);
        }

        // 인벤토리에서 아이템을 제거합니다. (장착 시 인벤토리에서 사라져야 하므로)
        RemoveItem(data, itemToEquip, 1);

        // 장비 슬롯에 새로운 아이템을 추가합니다.
        data.equippedItems.Add(itemToEquip.equipSlot, itemToEquip);

        // TODO: UI에 업데이트를 알리는 이벤트 호출 로직 추가 예정
    }

    /// <summary>
    /// 장착된 아이템을 해제하고 인벤토리로 되돌립니다.
    /// </summary>
    /// <param name="data">아이템 데이터를 담고 있는 InventoryData ScriptableObject입니다.</param>
    /// <param name="slotToUnEquip">해제할 장비 슬롯의 타입입니다.</param>
    public void UnEquipItem(InventoryData data, EquipSlot slotToUnEquip)
    {
        if (data.equippedItems.ContainsKey(slotToUnEquip))
        {
            EquipmentItemSO itemToUnEquip = data.equippedItems[slotToUnEquip];

            // 장착된 아이템을 인벤토리로 되돌립니다.
            AddItem(data, itemToUnEquip, 1);

            // 장비 슬롯에서 아이템을 제거합니다.
            data.equippedItems.Remove(slotToUnEquip);

            // TODO: UI에 업데이트를 알리는 이벤트 호출 로직 추가 예정
        }
    }
}