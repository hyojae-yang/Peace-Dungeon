// Inventory.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 인벤토리에 들어갈 아이템과 수량을 한 쌍으로 묶는 클래스입니다.
/// 클래스로 변경하여 리스트 내의 객체를 직접 수정할 수 있게 합니다.
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public BaseItemSO itemData;
    public int stackSize;

    /// <summary>
    /// 아이템과 수량을 지정하여 초기화하는 생성자입니다.
    /// </summary>
    public InventorySlot(BaseItemSO item, int quantity)
    {
        this.itemData = item;
        this.stackSize = quantity;
    }
}

/// <summary>
/// 플레이어의 아이템 소지품을 관리하는 인벤토리 시스템 스크립트입니다.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Tooltip("인벤토리의 최대 슬롯 개수입니다.")]
    public int inventorySize = 20;

    [Tooltip("플레이어가 현재 소지하고 있는 아이템 목록입니다.")]
    public List<InventorySlot> inventorySlots;

    void Awake()
    {
        // 게임 시작 시 인벤토리 슬롯 리스트를 null로 채워 초기화합니다.
        inventorySlots = new List<InventorySlot>(new InventorySlot[inventorySize]);
    }

    /// <summary>
    /// 아이템을 인벤토리에 추가하는 메서드입니다.
    /// </summary>
    /// <param name="itemToAdd">추가할 아이템 데이터</param>
    /// <param name="quantity">추가할 수량</param>
    /// <returns>아이템이 모두 추가되었는지 여부</returns>
    public bool AddItem(BaseItemSO itemToAdd, int quantity)
    {
        // 소모품 및 재료 아이템은 기존 슬롯에 합칠 수 있는지 확인합니다.
        // 현재는 ConsumableItemSO에만 maxStackCount를 정의했으므로,
        // 해당 아이템 타입에만 스택 로직을 적용합니다.
        if (itemToAdd.itemType == ItemType.Consumable)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                if (inventorySlots[i] != null && inventorySlots[i].itemData != null && inventorySlots[i].itemData.itemID == itemToAdd.itemID)
                {
                    int maxStack = ((ConsumableItemSO)itemToAdd).maxStackCount;
                    if (inventorySlots[i].stackSize < maxStack)
                    {
                        int spaceLeft = maxStack - inventorySlots[i].stackSize;
                        int addAmount = Mathf.Min(quantity, spaceLeft);

                        inventorySlots[i].stackSize += addAmount; // 클래스이므로 직접 수정 가능

                        quantity -= addAmount;
                        if (quantity <= 0)
                        {
                            Debug.Log($"{itemToAdd.itemName} {addAmount}개를 인벤토리에 추가했습니다.");
                            return true;
                        }
                    }
                }
            }
        }

        // 기존 슬롯에 합치지 못했거나 장비 아이템일 경우, 빈 슬롯을 찾아 추가합니다.
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventorySlots[i] == null || inventorySlots[i].itemData == null)
            {
                inventorySlots[i] = new InventorySlot(itemToAdd, quantity); // 새로운 InventorySlot 인스턴스 생성
                Debug.Log($"{itemToAdd.itemName} {quantity}개를 새로운 슬롯에 추가했습니다.");
                return true;
            }
        }

        Debug.LogWarning("인벤토리가 가득 차서 아이템을 추가할 수 없습니다!");
        return false;
    }

    /// <summary>
    /// 인벤토리에서 특정 슬롯의 아이템을 제거하는 메서드입니다.
    /// </summary>
    /// <param name="slotIndex">제거할 슬롯의 인덱스</param>
    /// <param name="quantity">제거할 수량</param>
    public void RemoveItem(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다.");
            return;
        }

        if (inventorySlots[slotIndex] == null || inventorySlots[slotIndex].itemData == null)
        {
            Debug.LogWarning("해당 슬롯에 아이템이 없습니다.");
            return;
        }

        // 제거할 수량을 뺀 후, 0 이하가 되면 슬롯을 비웁니다.
        inventorySlots[slotIndex].stackSize -= quantity;
        if (inventorySlots[slotIndex].stackSize <= 0)
        {
            Debug.Log($"{inventorySlots[slotIndex].itemData.itemName}이(가) 인벤토리에서 제거되었습니다.");
            inventorySlots[slotIndex] = null; // null로 할당하여 슬롯을 비웁니다.
        }
        else
        {
            Debug.Log($"{inventorySlots[slotIndex].itemData.itemName} {quantity}개가 제거되었습니다. 남은 수량: {inventorySlots[slotIndex].stackSize}");
        }
    }
}