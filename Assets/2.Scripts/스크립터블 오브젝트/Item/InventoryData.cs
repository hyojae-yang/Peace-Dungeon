using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 및 장비 데이터를 담는 ScriptableObject 클래스입니다.
/// 이 클래스는 오직 데이터 저장 역할만 하며, 로직을 포함하지 않습니다.
/// </summary>
[CreateAssetMenu(fileName = "InventoryData", menuName = "Data/Inventory Data", order = 1)]
public class InventoryData : ScriptableObject
{
    // === 인벤토리 데이터 ===
    // ItemData 리스트로 변경하여 아이템의 고유성과 개수를 함께 관리합니다.
    [Tooltip("플레이어의 인벤토리에 있는 모든 아이템과 그 개수를 저장합니다.")]
    public List<ItemData> inventoryItems = new List<ItemData>();

    // === 장착 장비 데이터 ===
    // Key: 장착 슬롯 (EquipSlot), Value: 장비 아이템 정보 (EquipmentItemSO)
    [Tooltip("플레이어가 현재 장착하고 있는 장비 아이템을 저장합니다.")]
    public Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();

    /// <summary>
    /// 인벤토리 및 장비 딕셔너리를 초기화하는 메서드입니다.
    /// 에디터에서 플레이 모드에 진입하거나 게임을 재시작할 때 데이터를 리셋하는 데 사용됩니다.
    /// </summary>
    public void Initialize()
    {
        inventoryItems.Clear();
        equippedItems.Clear();
    }
}