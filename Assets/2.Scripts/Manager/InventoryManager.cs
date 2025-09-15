// InventoryManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 인벤토리 시스템의 핵심 로직을 담당하는 매니저 클래스입니다.
/// 아이템 데이터 관리, 추가, 제거, 장비 장착/해제 및 관련 이벤트를 관리합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    public static InventoryManager Instance { get; private set; }

    // === 이벤트 (델리게이트) 정의 ===
    public event Action<BaseItemSO, int> onInventoryItemAdded;
    public event Action<BaseItemSO, int> onInventoryItemRemoved;
    public event Action onInventoryChanged; // 인벤토리 전체 변경 시 호출 (장비 장착/해제 등)
    public event Action onEquipmentChanged; // 장비 착용 상태 변경 시 호출

    // === 데이터 저장용 변수 ===
    [Header("인벤토리 데이터")]
    [Tooltip("에셋 파일로 저장된 인벤토리 데이터를 할당합니다.")]
    [SerializeField] private InventoryData inventoryData;

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (inventoryData != null)
            {
                // 인벤토리 데이터를 로드합니다.
                inventoryData.Initialize();
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
    /// 개수가 0개 이하면 추가하지 않습니다.
    /// </summary>
    /// <param name="item">추가할 아이템 SO</param>
    /// <param name="amount">추가할 개수</param>
    public void AddItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning("유효하지 않은 아이템 또는 개수입니다. 아이템을 추가할 수 없습니다.");
            return;
        }

        // === 핵심 수정 부분: 데이터 업데이트를 먼저 진행합니다. ===
        // 아이템이 이미 인벤토리에 있는지 확인합니다.
        if (inventoryData.inventoryItems.ContainsKey(item))
        {
            inventoryData.inventoryItems[item] += amount;
        }
        else
        {
            inventoryData.inventoryItems.Add(item, amount);
        }

        // 데이터 업데이트가 완료된 후, 이벤트가 발동됩니다.
        onInventoryItemAdded?.Invoke(item, amount);
    }

    /// <summary>
    /// 아이템을 인벤토리에서 제거하는 메서드입니다.
    /// </summary>
    /// <param name="item">제거할 아이템 SO</param>
    /// <param name="amount">제거할 개수</param>
    public void RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning("유효하지 않은 아이템 또는 개수입니다. 아이템을 제거할 수 없습니다.");
            return;
        }

        if (!inventoryData.inventoryItems.ContainsKey(item))
        {
            Debug.LogWarning("인벤토리에 해당 아이템이 없습니다.");
            return;
        }

        inventoryData.inventoryItems[item] -= amount;

        if (inventoryData.inventoryItems[item] <= 0)
        {
            inventoryData.inventoryItems.Remove(item);
        }

        // 아이템 제거가 완료된 후, 이벤트가 발동됩니다.
        onInventoryItemRemoved?.Invoke(item, amount);
    }

    /// <summary>
    /// 인벤토리 아이템을 장비 슬롯에 장착하는 메서드입니다.
    /// </summary>
    /// <param name="item">장착할 장비 아이템</param>
    public void EquipItem(EquipmentItemSO item)
    {
        if (item == null) return;

        // 이미 해당 슬롯에 장착된 아이템이 있는지 확인합니다.
        if (inventoryData.equippedItems.ContainsKey(item.equipSlot))
        {
            // 기존 장비를 해제합니다.
            UnEquipItem(item.equipSlot);
        }

        // 인벤토리에서 아이템을 제거하고 장비 슬롯에 추가합니다.
        inventoryData.equippedItems.Add(item.equipSlot, item);
        RemoveItem(item, 1);

        // 장비 변경 이벤트를 발동합니다.
        onEquipmentChanged?.Invoke();
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 장비 슬롯의 아이템을 해제하고 인벤토리로 되돌리는 메서드입니다.
    /// </summary>
    /// <param name="slotType">해제할 장비 슬롯의 타입</param>
    public void UnEquipItem(EquipSlot slotType)
    {
        if (inventoryData.equippedItems.ContainsKey(slotType))
        {
            // 장비 슬롯에서 아이템을 제거합니다.
            EquipmentItemSO unequippedItem = inventoryData.equippedItems[slotType];
            inventoryData.equippedItems.Remove(slotType);

            // 아이템을 인벤토리에 다시 추가합니다.
            AddItem(unequippedItem, 1);

            // 장비 변경 이벤트를 발동합니다.
            onEquipmentChanged?.Invoke();
            onInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// 현재 인벤토리의 모든 아이템 데이터를 반환합니다.
    /// </summary>
    /// <returns>인벤토리 아이템 딕셔너리</returns>
    public Dictionary<BaseItemSO, int> GetInventoryData()
    {
        return inventoryData.inventoryItems;
    }

    /// <summary>
    /// 현재 장착된 아이템 데이터를 반환합니다.
    /// </summary>
    /// <returns>장착 아이템 딕셔너리</returns>
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedData()
    {
        return inventoryData.equippedItems;
    }
}