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
public class InventoryManager : MonoBehaviour, ISavable
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
    
    private void Awake()
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
    private void Start()
    {
        // ISavable 인터페이스를 구현한 이 객체를 SaveManager에 등록합니다.
        SaveManager.Instance.RegisterSavable(this);

        // SaveManager에 로드된 데이터가 있는지 확인하고, 있으면 적용합니다.
        if (SaveManager.Instance.HasLoadedData)
        {
            // SaveManager로부터 PlayerStats에 해당하는 데이터를 가져옵니다.
            // TryGetData 메서드는 데이터를 찾았을 경우 true를 반환하고 loadedData 변수에 데이터를 담습니다.
            if (SaveManager.Instance.TryGetData(this.GetType().Name, out object loadedData))
            {
                // 가져온 데이터를 PlayerStats에 적용합니다.
                LoadData(loadedData);
            }
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
    /// 장비 아이템 인스턴스를 인벤토리에 추가하는 메서드입니다.
    /// 이 메서드는 장비 해제 시 호출되며, 아이템의 고유성을 보존합니다.
    /// SOLID: 개방-폐쇄 원칙 (Open-Closed Principle). 기존 메서드를 수정하지 않고 확장합니다.
    /// </summary>
    /// <param name="equipmentItem">추가할 장비 아이템의 인스턴스</param>
    /// <returns>아이템 추가 성공 여부</returns>
    public bool AddItem(EquipmentItemSO equipmentItem)
    {
        // 인벤토리 공간이 가득 찼는지 확인합니다.
        if (inventoryData.inventoryItems.Count >= inventorySize)
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다. 아이템을 추가할 수 없습니다.");
            return false;
        }

        // 장비 아이템은 스택이 불가능하므로, 스택 수를 1로 고정하여 ItemData를 생성합니다.
        ItemData newItemData = new ItemData(equipmentItem, 1);
        inventoryData.inventoryItems.Add(newItemData);

        // 이벤트 호출
        OnItemAdded?.Invoke(equipmentItem.itemID, 1);
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
    /// 특정 고유 ID를 가진 장비 아이템 인스턴스를 인벤토리에서 제거하는 메서드입니다.
    /// 이 메서드는 장비 착용 시 호출되며, 정확한 아이템 인스턴스를 제거합니다.
    /// SOLID: 단일 책임 원칙 (Single Responsibility Principle). 특정 ID 제거라는 단일 책임을 가집니다.
    /// </summary>
    /// <param name="uniqueID">제거할 장비 아이템의 고유 ID</param>
    /// <returns>아이템 제거 성공 여부</returns>
    public bool RemoveItem(string uniqueID)
    {
        // 인벤토리 리스트를 역순으로 순회하며 해당 아이템을 찾습니다.
        // 리스트를 역순으로 순회하는 이유는, 중간에 아이템을 제거해도 인덱스 오류가 발생하지 않기 때문입니다.
        for (int i = inventoryData.inventoryItems.Count - 1; i >= 0; i--)
        {
            ItemData itemData = inventoryData.inventoryItems[i];

            // ItemData의 itemSO가 EquipmentItemSO 타입인지 확인합니다.
            // 장비 아이템이 아닌 다른 종류의 아이템(예: 소모품)은 이 로직에 해당하지 않습니다.
            if (itemData.itemSO is EquipmentItemSO equipmentSO)
            {
                // 고유 ID가 일치하는지 확인합니다.
                if (equipmentSO.uniqueID == uniqueID)
                {
                    // 찾았다면 리스트에서 해당 아이템을 제거합니다.
                    inventoryData.inventoryItems.RemoveAt(i);

                    // 이벤트 호출: 인벤토리의 내용이 변경되었음을 알립니다.
                    OnItemRemoved?.Invoke(equipmentSO.itemID, 1);
                    onInventoryChanged?.Invoke();

                    return true; // 제거 성공
                }
            }
        }

        // 루프를 모두 돌았는데도 아이템을 찾지 못했다면 경고 메시지를 출력하고 false를 반환합니다.
        Debug.LogWarning($"<color=red>아이템 제거 실패:</color> 인벤토리에서 고유 ID '{uniqueID}'를 가진 아이템을 찾을 수 없습니다.");
        return false; // 제거 실패
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
    // === ISavable 인터페이스 구현 ===
    /// <summary>
    /// 현재 인벤토리 데이터를 InventorySaveData 객체로 변환하여 반환합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// SOLID: 단일 책임 원칙 (InventoryManager는 아이템 관리에만 집중).
    /// </summary>
    /// <returns>InventorySaveData 타입의 저장 가능한 데이터 객체</returns>
    public object SaveData()
    {
        // 인벤토리와 장착된 아이템의 데이터를 가져옵니다.
        var currentItems = inventoryData.inventoryItems;
        var equippedItems = playerCharacter.playerEquipmentManager.GetEquippedItems();

        // 일반 아이템, 모든 장비 아이템, 장착된 슬롯 정보를 담을 리스트를 만듭니다.
        List<SavableItemData> savableItems = new List<SavableItemData>();
        List<SavableEquipmentData> allEquipmentItems = new List<SavableEquipmentData>();
        List<SavableEquippedData> equippedSlots = new List<SavableEquippedData>();

        // === 변경된 코드 ===
        // 이미 처리된 유니크 ID를 추적하는 HashSet입니다. 중복 저장을 방지합니다.
        HashSet<string> processedUniqueIDs = new HashSet<string>();
        // ==================

        // 인벤토리 아이템 목록을 순회하며 데이터를 분류하고 변환합니다.
        foreach (var item in currentItems)
        {
            // 장비 아이템이라면, 유니크 ID가 중복되지 않을 때만 저장합니다.
            if (item.itemSO is EquipmentItemSO equipmentSO)
            {
                if (processedUniqueIDs.Add(equipmentSO.uniqueID))
                {
                    allEquipmentItems.Add(new SavableEquipmentData
                    {
                        uniqueID = equipmentSO.uniqueID,
                        itemID = equipmentSO.itemID,
                        itemGrade = equipmentSO.itemGrade,
                        baseStats = equipmentSO.baseStats,
                        additionalStats = equipmentSO.additionalStats
                    });
                }
            }
            // 일반 아이템이라면, ID와 스택 수만 저장합니다.
            else
            {
                savableItems.Add(new SavableItemData
                {
                    itemID = item.itemSO.itemID,
                    stackCount = item.stackCount
                });
            }
        }

        // 장착된 아이템 목록을 순회하며 데이터를 분류하고 변환합니다.
        foreach (var item in equippedItems)
        {
            // 장비 아이템의 모든 정보를 allEquipmentItems 리스트에 저장합니다.
            // === 변경된 코드 ===
            // 인벤토리에 있던 아이템이 장착되었을 수 있으므로, 중복 체크 후 추가합니다.
            if (processedUniqueIDs.Add(item.Value.uniqueID))
            {
                allEquipmentItems.Add(new SavableEquipmentData
                {
                    uniqueID = item.Value.uniqueID,
                    itemID = item.Value.itemID,
                    itemGrade = item.Value.itemGrade,
                    baseStats = item.Value.baseStats,
                    additionalStats = item.Value.additionalStats
                });
            }
            // ==================

            // 장착된 슬롯의 위치와 아이템의 유니크 ID만 equippedSlots 리스트에 저장합니다.
            equippedSlots.Add(new SavableEquippedData
            {
                equipSlot = item.Key,
                uniqueID = item.Value.uniqueID
            });
        }

        // 변환된 모든 리스트를 InventorySaveData 객체에 담아 반환합니다.
        InventorySaveData data = new InventorySaveData
        {
            inventoryItems = savableItems,
            allEquipmentItems = allEquipmentItems,
            equippedSlots = equippedSlots
        };
        return data;
    }

    // ---

    /// <summary>
    /// 저장된 데이터를 읽어 인벤토리에 적용합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// SOLID: Open-Closed Principle (개방-폐쇄 원칙)
    /// </summary>
    /// <param name="data">로드할 데이터가 담긴 InventorySaveData 객체</param>
    public void LoadData(object data)
    {
        // 로드된 데이터를 InventorySaveData 타입으로 변환합니다.
        if (data is InventorySaveData loadedData)
        {
            // 기존 인벤토리 데이터를 비웁니다.
            inventoryData.inventoryItems.Clear();

            // 모든 장비 아이템을 임시 딕셔너리에 저장하여 ID로 빠르게 찾을 수 있게 합니다.
            Dictionary<string, EquipmentItemSO> tempEquipmentDict = new Dictionary<string, EquipmentItemSO>();
            foreach (var savableEquipment in loadedData.allEquipmentItems)
            {
                BaseItemSO baseSO = ItemDatabaseManager.Instance.GetItemByID(savableEquipment.itemID);
                if (baseSO is EquipmentItemSO templateSO)
                {
                    EquipmentItemSO newEquipment = Instantiate(templateSO);
                    newEquipment.uniqueID = savableEquipment.uniqueID;
                    newEquipment.itemGrade = savableEquipment.itemGrade;
                    newEquipment.baseStats = savableEquipment.baseStats;
                    newEquipment.additionalStats = savableEquipment.additionalStats;

                    tempEquipmentDict.Add(newEquipment.uniqueID, newEquipment);
                }
                else
                {
                    Debug.LogWarning($"장비 아이템 ID {savableEquipment.itemID}에 해당하는 아이템을 찾을 수 없습니다.");
                }
            }

            // === 장착 슬롯 로드 및 장비 아이템 분리 로직 ===
            playerCharacter.playerEquipmentManager.UnequipAll();

            // 장착된 아이템의 유니크 ID 목록을 HashSet에 담아 빠르게 확인합니다.
            HashSet<string> equippedUniqueIDs = new HashSet<string>();
            foreach (var equippedSlot in loadedData.equippedSlots)
            {
                equippedUniqueIDs.Add(equippedSlot.uniqueID);
            }

            // 임시 딕셔너리에 있는 장비 아이템들을 순회하며
            // 장착된 아이템은 장착시키고, 나머지는 인벤토리로 되돌립니다.
            foreach (var equipment in tempEquipmentDict.Values)
            {
                if (!equippedUniqueIDs.Contains(equipment.uniqueID))
                {
                    // 장착되지 않은 장비는 인벤토리로 추가합니다.
                    inventoryData.inventoryItems.Add(new ItemData(equipment, 1));
                }
            }

            // 일반 아이템(소모품 등) 로드 로직
            foreach (var savableItem in loadedData.inventoryItems)
            {
                BaseItemSO itemSO = ItemDatabaseManager.Instance.GetItemByID(savableItem.itemID);
                if (itemSO != null)
                {
                    inventoryData.inventoryItems.Add(new ItemData(itemSO, savableItem.stackCount));
                }
                else
                {
                    Debug.LogWarning($"아이템 ID {savableItem.itemID}에 해당하는 아이템을 찾을 수 없습니다.");
                }
            }

            // 마지막으로, 장착된 아이템 슬롯 정보를 기반으로 장착을 완료합니다.
            foreach (var equippedSlot in loadedData.equippedSlots)
            {
                if (tempEquipmentDict.TryGetValue(equippedSlot.uniqueID, out EquipmentItemSO equipmentToEquip))
                {
                    playerCharacter.playerEquipmentManager.EquipItem(equipmentToEquip, equippedSlot.equipSlot);
                }
            }

            // UI 갱신을 위해 이벤트 호출
            onInventoryChanged?.Invoke();
        }
    }
}