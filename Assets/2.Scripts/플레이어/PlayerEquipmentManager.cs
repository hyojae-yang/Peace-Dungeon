using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 장비 착용 상태를 관리하는 스크립트입니다.
/// 아이템을 장착하고, 해제하며, 장비 능력치를 업데이트합니다.
/// </summary>
public class PlayerEquipmentManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static PlayerEquipmentManager Instance { get; private set; }

    // === 이벤트 ===
    /// <summary>
    /// 장비 상태가 변경될 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action onEquipmentChanged;

    // === 데이터 저장용 변수 ===
    // 현재 장착된 아이템들을 관리하는 딕셔너리
    private Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // 싱글턴 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지되도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 장비 아이템을 착용하거나 교체하는 메서드입니다.
    /// </summary>
    /// <param name="itemToEquip">장착할 장비 아이템 데이터</param>
    public void EquipItem(EquipmentItemSO itemToEquip)
    {
        if (itemToEquip == null) return;

        EquipSlot slot = itemToEquip.equipSlot;
        EquipmentItemSO oldItem = null;

        // 1. 현재 슬롯에 다른 장비가 있다면, 기존 장비를 인벤토리로 되돌립니다.
        if (equippedItems.TryGetValue(slot, out oldItem))
        {
            // 기존 장비를 인벤토리로 추가합니다.
            InventoryManager.Instance.AddItem(oldItem);
            Debug.Log($"<color=yellow>장비 해제 (교체):</color> {oldItem.itemName}을(를) 인벤토리로 반환.");
        }

        // 2. 인벤토리에서 현재 장착하려는 아이템을 제거합니다.
        // 이 로직은 장착 성공 시에만 실행되어야 합니다.
        if (InventoryManager.Instance.RemoveItem(itemToEquip, 1))
        {
            // 3. 인벤토리 제거에 성공하면, 새로운 장비를 슬롯에 할당합니다.
            equippedItems[slot] = itemToEquip;
            Debug.Log($"<color=lime>장비 착용:</color> {itemToEquip.itemName}을(를) {slot} 슬롯에 장착.");
        }
        else
        {
            // 인벤토리에서 아이템을 찾지 못하면 장착 실패
            Debug.LogWarning($"<color=red>장비 착용 실패:</color> 인벤토리에 {itemToEquip.itemName} 아이템이 없습니다.");
            return; // 메서드 종료
        }

        // 4. 장비 상태가 변경되었음을 알립니다. (UI 갱신 등)
        onEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 특정 슬롯에 장비된 아이템을 해제합니다. (Void 반환 타입)
    /// </summary>
    /// <param name="slot">해제할 장비 슬롯</param>
    public void UnEquipItem(EquipSlot slot)
    {
        // 해당 슬롯에 장착된 아이템이 있는지 확인합니다.
        if (equippedItems.TryGetValue(slot, out EquipmentItemSO itemToUnequip))
        {
            // 아이템을 인벤토리로 되돌립니다.
            InventoryManager.Instance.AddItem(itemToUnequip);
            Debug.Log($"<color=yellow>장비 해제:</color> {itemToUnequip.itemName}을(를) {slot} 슬롯에서 해제하고 인벤토리로 반환했습니다.");

            // 딕셔너리에서 해당 장비 정보를 제거합니다.
            equippedItems.Remove(slot);

            // 장비 상태 변경 이벤트 호출
            onEquipmentChanged?.Invoke();
        }
        else
        {
            // 해당 슬롯에 장비가 없으면 경고 로그를 출력합니다.
            Debug.LogWarning($"<color=red>장비 해제 실패:</color> {slot} 슬롯에 장착된 아이템이 없습니다.");
        }
    }

    /// <summary>
    /// 현재 장착된 아이템 목록을 반환합니다.
    /// </summary>
    /// <returns>장비 아이템 딕셔너리</returns>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        return equippedItems;
    }
}