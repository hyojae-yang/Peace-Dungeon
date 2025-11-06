using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 및 장비 데이터를 담는 ScriptableObject 클래스입니다.
/// 이 클래스는 오직 데이터 저장 역할만 하며, 로직을 포함하지 않습니다.
/// SOLID: 단일 책임 원칙 (SRP - Data Storage).
/// </summary>
[CreateAssetMenu(fileName = "InventoryData", menuName = "Data/Inventory Data", order = 1)]
public class InventoryData : ScriptableObject
{
    // =======================================================================
    // === 인벤토리 데이터: 7개의 독립된 저장소 (고객님 요구사항) ===
    // =======================================================================

    [Header("1. 장비 아이템 저장소 (각 64칸)")]
    [Tooltip("무기류 장비 아이템 리스트입니다.")]
    public List<ItemData> weaponItems = new List<ItemData>();

    [Tooltip("방어구류 장비 아이템 리스트입니다.")]
    public List<ItemData> armorItems = new List<ItemData>();

    [Tooltip("장신구류 장비 아이템 리스트입니다.")]
    public List<ItemData> accessoryItems = new List<ItemData>();

    [Header("2. 일반 아이템 저장소 (각 80칸)")]
    [Tooltip("소모품 아이템 리스트입니다.")]
    public List<ItemData> consumableItems = new List<ItemData>();

    [Tooltip("재료 아이템 리스트입니다.")]
    public List<ItemData> materialItems = new List<ItemData>();

    [Tooltip("퀘스트 아이템 리스트입니다.")]
    public List<ItemData> questItems = new List<ItemData>();

    [Tooltip("특수 아이템 리스트입니다.")]
    public List<ItemData> specialItems = new List<ItemData>();

    // =======================================================================
    // === 장착 장비 데이터 (기존 유지) ===
    // =======================================================================

    // Key: 장착 슬롯 (EquipSlot), Value: 장비 아이템 정보 (EquipmentItemSO)
    [Tooltip("플레이어가 현재 장착하고 있는 장비 아이템을 저장합니다.")]
    public Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();

    // =======================================================================
    // === 초기화 ===
    // =======================================================================

    /// <summary>
    /// 인벤토리 및 장비 딕셔너리를 초기화하는 메서드입니다.
    /// 에디터에서 플레이 모드에 진입하거나 게임을 재시작할 때 데이터를 리셋하는 데 사용됩니다.
    /// </summary>
    public void Initialize()
    {
        // 🚨 7개의 모든 독립된 아이템 리스트를 비웁니다.
        weaponItems.Clear();
        armorItems.Clear();
        accessoryItems.Clear();
        consumableItems.Clear();
        materialItems.Clear();
        questItems.Clear();
        specialItems.Clear();

        // 장착 슬롯 데이터도 비웁니다.
        equippedItems.Clear();
    }
}