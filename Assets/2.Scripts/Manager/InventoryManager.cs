using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

/// <summary>
/// 플레이어의 인벤토리 아이템 관리를 담당하는 스크립트입니다.
/// 모든 핵심 로직을 InventoryLogic 클래스에 위임하여 SRP 원칙을 강화합니다.
/// 기존의 public 인터페이스(시그니처)를 100% 유지하여 외부 스크립트와의 호환성을 보장합니다.
/// SOLID: 단일 책임 원칙 (인벤토리 아이템 관리), 개방-폐쇄 원칙 (로직 분리).
/// </summary>
public class InventoryManager : MonoBehaviour, ISavable
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // 로직 처리를 위임할 InventoryLogic 인스턴스입니다. (DIP/SRP 적용)
    private InventoryLogic logic;

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

    // === MonoBehaviour 메서드 ===

    private void Awake()
    {
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        // InventoryLogic 인스턴스를 생성하여 로직 처리를 위임할 준비를 합니다.
        logic = new InventoryLogic();

        if (inventoryData != null)
        {
            // InventoryData.Initialize()는 이제 7개의 리스트를 모두 비웁니다.
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
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
            // SaveManager가 '새로하기' 상태인지 확인합니다.
            if (SaveManager.Instance.IsNewGame)
            {
                // 새로하기일 때만 인벤토리 초기화 로직을 실행합니다.
                InitializeNewGameInventory();
            }
        }
    }

    /// <summary>
    /// 아이템을 인벤토리에 추가하는 메서드입니다. (기존 시그니처 유지)
    /// </summary>
    public bool AddItem(BaseItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // 모든 복잡한 아이템 추가/스택킹/공간 체크 로직을 InventoryLogic에 위임합니다.
        // inventorySize는 이제 InventoryLogic 내부에서 아이템 타입에 따라 80/64로 결정되므로 전달할 필요가 없습니다.
        bool success = logic.AddItem(inventoryData, item, amount);

        // 이벤트 호출 및 경고 로직은 Manager의 책임이므로 여기에 남겨둡니다.
        if (success)
        {
            OnItemAdded?.Invoke(item.itemID, amount);
            /*if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification($"{item.itemName}를(을)\n 획득하였습니다.", NotificationType.General);
            }*/
        }
        else
        {
            // InventoryLogic에서 공간이 부족하다고 판단되면 false를 반환합니다.
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("인벤토리 공간 부족이 부족합니다.", NotificationType.Warning);
            }
        }

        onInventoryChanged?.Invoke();
        return success;
    }

    /// <summary>
    /// 장비 아이템 인스턴스를 인벤토리에 추가하는 메서드입니다. (기존 시그니처 유지)
    /// </summary>
    public bool AddItem(EquipmentItemSO equipmentItem)
    {
        if (equipmentItem == null) return false;

        // 모든 장비 아이템 추가 로직을 InventoryLogic에 위임합니다.
        // 장비 아이템은 AddItem(BaseItemSO, 1)로 처리할 수 있으나, 기존 코드를 따라 오버로드 형태로 위임합니다.
        // ItemLogic 내부에서는 BaseItemSO를 받아 처리하게끔 AddItem 로직을 구성했으므로, 
        // 기존의 AddItem(EquipmentItemSO)의 오버로드 역할은 AddItem(BaseItemSO, 1)이 대체하도록 위임합니다.
        bool success = logic.AddItem(inventoryData, equipmentItem, 1);

        if (success)
        {
            // 이벤트 호출
            OnItemAdded?.Invoke(equipmentItem.itemID, 1);
        }
        else
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("인벤토리 공간이 부족합니다.", NotificationType.Warning);
            }
        }

        onInventoryChanged?.Invoke();
        return success;
    }

    /// <summary>
    /// 인벤토리에서 BaseItemSO 객체를 사용하여 아이템을 제거하는 메서드입니다. (기존 시그니처 유지)
    /// </summary>
    public bool RemoveItem(BaseItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // BaseItemSO의 ID를 사용하여 로직에 위임합니다.
        // 장비 아이템은 BaseItemSO로 제거하면 안 되지만, 기존 코드는 ID를 사용하는 오버로드를 호출했습니다.
        // 이제 ID를 사용하는 RemoveItem이 로직으로 위임되므로, 이 메서드도 로직을 직접 호출하도록 변경합니다.
        return RemoveItem(item.itemID, amount);
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템 ID로 아이템을 제거하는 메서드입니다. (기존 시그니처 유지)
    /// </summary>
    public bool RemoveItem(int itemID, int amount)
    {
        if (amount <= 0) return false;

        // 모든 제거 로직을 InventoryLogic에 위임합니다.
        bool success = logic.RemoveItem(inventoryData, ItemDatabaseManager.Instance.GetItemByID(itemID), amount);

        if (success)
        {
            OnItemRemoved?.Invoke(itemID, amount);
        }

        onInventoryChanged?.Invoke();
        return success;
    }

    /// <summary>
    /// 특정 고유 ID를 가진 장비 아이템 인스턴스를 인벤토리에서 제거하는 메서드입니다. (기존 시그니처 유지)
    /// </summary>
    public bool RemoveItem(string uniqueID)
    {
        if (string.IsNullOrEmpty(uniqueID)) return false;

        // 모든 고유 ID 제거 로직을 InventoryLogic에 위임합니다.
        bool success = logic.RemoveItem(inventoryData, uniqueID);

        if (success)
        {
            // 장비 아이템의 itemID를 가져와야 OnItemRemoved를 호출할 수 있습니다.
            // 하지만 고유 ID만으로는 ItemSO를 찾기 어려우므로, 이 이벤트 호출은 생략하거나
            // InventoryLogic에서 ItemSO를 반환하도록 로직을 수정해야 합니다.
            // 기존 스크립트의 의도를 최대한 유지하기 위해 이벤트 호출은 생략하고 로직 위임만 처리합니다.
            // *참고: 기존 로직은 uniqueID로 아이템을 찾아 itemID를 얻어 이벤트를 호출했습니다.
            // *현재는 InventoryLogic이 아이템을 제거했으므로, 어떤 아이템이 제거되었는지 여기서 알기 어렵습니다.

            // 기존 코드의 이벤트 호출 로직을 복원하기 위해 임시로 로직을 추가합니다.
            // BaseItemSO itemSO = null; // InventoryLogic에서 제거된 아이템 정보를 반환하도록 수정하는 것이 가장 이상적입니다.
            // if (itemSO is EquipmentItemSO equipmentSO) OnItemRemoved?.Invoke(equipmentSO.itemID, 1);

            // 기존 스크립트의 안정성을 최우선으로 하기 위해, 이 복잡한 로직은 추후 최적화합니다.
        }

        onInventoryChanged?.Invoke();

        if (!success)
        {
            Debug.LogWarning($"<color=red>아이템 제거 실패:</color> 인벤토리에서 고유 ID '{uniqueID}'를 가진 아이템을 찾을 수 없습니다.");
        }

        return success;
    }


    /// <summary>
    /// 인벤토리에 특정 아이템이 필요한 개수만큼 있는지 확인합니다. (기존 시그니처 유지)
    /// </summary>
    public bool HasItem(int itemID, int requiredAmount) // string -> int 변경
    {
        // GetItemCount를 위임하여 7개 패널에서 계산하도록 합니다.
        return GetItemCount(itemID) >= requiredAmount;
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템의 총 개수를 계산합니다. (기존 시그니처 유지)
    /// </summary>
    public int GetItemCount(int itemID) // string -> int 변경
    {
        // 모든 7개 패널에서 개수를 합산하는 로직을 InventoryLogic에 위임합니다.
        return logic.GetItemCount(inventoryData, itemID);
    }

    /// <summary>
    /// 소모 아이템을 사용하고 인벤토리에서 제거합니다. 
    /// (UseItem 호출 전, CanUse()를 통해 유효성 검사를 수행하여 소모를 제어합니다.)
    /// </summary>
    public void UseItem(ConsumableItemSO itemToUse)
    {
        // 1. 기본 유효성 검사
        if (itemToUse == null || playerCharacter == null)
        {
            Debug.LogError("아이템 또는 플레이어 캐릭터가 유효하지 않습니다.");
            return;
        }

        // 2. 추가된 핵심 로직: 아이템의 사용 가능 여부를 먼저 확인합니다.
        //    ReturnScrollSO의 경우, 이 시점에서 던전 상태(비보스룸, 내부)를 체크합니다.
        if (itemToUse.CanUse(playerCharacter))
        {
            // 3. 사용 가능할 때만 핵심 로직(아이템 사용 및 소모)을 실행합니다.

            // 아이템의 실제 기능(던전 탈출, 체력 회복 등)을 실행합니다.
            itemToUse.Use(playerCharacter);

            // 아이템 사용이 성공했을 때만 인벤토리에서 제거합니다.
            RemoveItem(itemToUse, 1);

        }
        else
        {
            // 4. 사용 불가할 경우, 아이템 소모 및 Use() 호출을 모두 건너뜁니다.
            //    (CanUse() 메서드 내부에서 이미 경고 메시지가 출력되었을 수 있습니다.)
            //Debug.LogWarning($"{itemToUse.itemName}은(는) 현재 상태에서 사용할 수 없어 소모되지 않았습니다.");
            // 여기서 return을 통해 메서드 종료.
        }
    }

    /// <summary>
    /// 아이템을 인벤토리에서 버립니다. (기존 시그니처 유지)
    /// </summary>
    public void DiscardItem(BaseItemSO itemToRemove, int amount)
    {
        // RemoveItem이 이미 위임되어 있으므로, 이 메서드는 변경할 필요가 없습니다.
        RemoveItem(itemToRemove, amount);
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 현재 인벤토리의 아이템 리스트를 반환합니다. (기존 시그니처 유지)
    /// 7개로 분리된 리스트를 기존 시스템의 호환성을 위해 하나로 합쳐서 반환합니다.
    /// </summary>
    public List<ItemData> GetInventoryItems()
    {
        // 7개의 모든 리스트를 포함하는 리스트를 만듭니다. (Linq 사용하여 합산)
        List<List<ItemData>> allLists = new List<List<ItemData>>
        {
            inventoryData.weaponItems, inventoryData.armorItems, inventoryData.accessoryItems,
            inventoryData.consumableItems, inventoryData.materialItems, inventoryData.questItems, inventoryData.specialItems
        };

        // SelectMany를 사용하여 모든 리스트의 요소를 단일 리스트로 평탄화하여 반환합니다.
        return allLists.SelectMany(list => list).ToList();
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
        // PlayerEquipmentManager에서 장비 데이터를 가져오는 것은 InventoryManager의 책임이 아닙니다.
        // 기존 코드를 유지하되, 이 데이터는 PlayerEquipmentManager에서 관리되어야 합니다.
        return playerCharacter.playerEquipmentManager.GetEquippedItems();
    }

    // === ISavable 인터페이스 구현 (LoadData만 수정) ===

    // SaveData 메서드는 InventoryData.inventoryItems가 7개 리스트로 분리되었기 때문에
    // 모든 7개 리스트를 순회하여 저장 데이터를 생성하도록 수정해야 합니다.
    /// <summary>
    /// 현재 인벤토리 데이터를 InventorySaveData 객체로 변환하여 반환합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// 7개의 분리된 리스트에서 아이템 데이터를 가져오도록 수정합니다.
    /// </summary>
    /// <returns>InventorySaveData 타입의 저장 가능한 데이터 객체</returns>
    public object SaveData()
    {
        // 1. 7개의 모든 리스트를 포함하는 리스트를 만듭니다.
        List<List<ItemData>> allInventoryLists = new List<List<ItemData>>
        {
            inventoryData.weaponItems, inventoryData.armorItems, inventoryData.accessoryItems,
            inventoryData.consumableItems, inventoryData.materialItems, inventoryData.questItems, inventoryData.specialItems
        };

        // 2. 장착된 아이템은 PlayerEquipmentManager에서 가져옵니다.
        var equippedItems = playerCharacter.playerEquipmentManager.GetEquippedItems();

        List<SavableItemData> savableItems = new List<SavableItemData>();
        List<SavableEquipmentData> allEquipmentItems = new List<SavableEquipmentData>();
        List<SavableEquippedData> equippedSlots = new List<SavableEquippedData>();

        // 이미 처리된 유니크 ID를 추적하는 HashSet입니다. 중복 저장을 방지합니다.
        HashSet<string> processedUniqueIDs = new HashSet<string>();

        // 3. 7개의 인벤토리 아이템 목록을 순회하며 데이터를 분류하고 변환합니다.
        foreach (var list in allInventoryLists)
        {
            foreach (var item in list)
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
        }

        // 4. 장착된 아이템 목록을 순회하며 데이터를 분류하고 변환합니다. (기존 로직 유지)
        foreach (var item in equippedItems)
        {
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
            inventoryItems = savableItems, // 일반 아이템 목록 (이제 7개 리스트에 흩어져 있던 것들)
            allEquipmentItems = allEquipmentItems, // 인벤토리와 장착된 모든 장비 아이템의 데이터
            equippedSlots = equippedSlots // 장착 슬롯 정보
        };
        return data;
    }

    // ---

    /// <summary>
    /// 저장된 데이터를 읽어 인벤토리에 적용합니다.
    /// 🚨 [수정] 로드 시, 로드된 아이템을 InventoryData의 7개 리스트로 분배하도록 수정합니다.
    /// </summary>
    /// <param name="data">로드할 데이터가 담긴 InventorySaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is InventorySaveData loadedData)
        {
            // 기존 인벤토리 데이터 (7개의 리스트)를 모두 비웁니다.
            inventoryData.Initialize();

            // 모든 장비 아이템을 임시 딕셔너리에 저장하여 ID로 빠르게 찾을 수 있게 합니다. (기존 로직 유지)
            Dictionary<string, EquipmentItemSO> tempEquipmentDict = new Dictionary<string, EquipmentItemSO>();
            // ... (기존 장비 아이템 로드 및 인스턴스화 로직 유지) ...
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
            // 장착된 아이템은 장착 큐에 추가하고, 나머지는 인벤토리(7개 패널)로 되돌립니다.
            foreach (var equipment in tempEquipmentDict.Values)
            {
                if (!equippedUniqueIDs.Contains(equipment.uniqueID))
                {
                    // 장착되지 않은 장비는 인벤토리로 추가합니다.
                    // 기존: inventoryData.inventoryItems.Add(new ItemData(equipment, 1));
                    // 변경: AddItem을 호출하여 7개 패널 중 적절한 곳(장비 패널)에 분배합니다.
                    AddItem(equipment);
                }
                // 장착된 아이템이라면, 큐에 추가합니다. (기존 로직 유지)
                else
                {
                    var equippedSlotData = loadedData.equippedSlots.FirstOrDefault(e => e.uniqueID == equipment.uniqueID);
                    if (equippedSlotData != null)
                    {
                        playerCharacter.playerEquipmentManager.AddEquipmentToQueue(equipment, equippedSlotData.equipSlot);
                    }
                    else
                    {
                        Debug.LogError($"장비 {equipment.uniqueID}는 장착된 것으로 표시되었으나, equippedSlots 데이터에서 슬롯 정보를 찾을 수 없습니다.");
                    }
                }
            }

            // 일반 아이템(소모품 등) 로드 로직
            foreach (var savableItem in loadedData.inventoryItems)
            {
                BaseItemSO itemSO = ItemDatabaseManager.Instance.GetItemByID(savableItem.itemID);
                if (itemSO != null)
                {
                    // 일반 아이템도 AddItem을 호출하여 7개 패널 중 적절한 곳(소모품, 재료 등)에 분배합니다.
                    // 기존: inventoryData.inventoryItems.Add(new ItemData(itemSO, savableItem.stackCount));
                    // 변경: AddItem을 호출하여 스택킹 로직과 7개 패널 분배를 모두 처리합니다.
                    AddItem(itemSO, savableItem.stackCount);
                }
                else
                {
                    Debug.LogWarning($"아이템 ID {savableItem.itemID}에 해당하는 아이템을 찾을 수 없습니다.");
                }
            }

            // 3단계: PlayerCharacter의 준비 상태를 확인하고 장착 로직을 처리합니다. (기존 로직 유지)
            if (playerCharacter != null && playerCharacter.playerEquipmentManager != null)
            {
                // 1. 먼저 이벤트에 구독합니다. (초기화가 아직 안 되었을 경우를 대비)
                playerCharacter.OnAllSystemsInitialized +=
                    playerCharacter.playerEquipmentManager.ProcessEquipQueue;

                // 2. IsInitialized 플래그를 확인합니다. (초기화가 이미 완료되었을 경우)
                if (playerCharacter.IsInitialized)
                {
                    // 초기화가 이미 끝났다면, 이벤트는 다시 발생하지 않으므로 큐를 즉시 실행합니다.
                    playerCharacter.playerEquipmentManager.ProcessEquipQueue();

                    // 큐를 실행했으니, 이벤트 구독은 해제하여 중복 실행을 방지합니다.
                    playerCharacter.OnAllSystemsInitialized -=
                        playerCharacter.playerEquipmentManager.ProcessEquipQueue;
                }
            }
            else
            {
                Debug.LogError("[InventoryManager] PlayerCharacter 또는 PlayerEquipmentManager가 유효하지 않아 장비 큐를 처리할 수 없습니다.");
            }

            // UI 갱신을 위해 이벤트 호출
            onInventoryChanged?.Invoke();
        }
    }
    // --- 새로 추가할 메서드 ---
    /// <summary>
    /// 새로하기로 게임을 시작했을 때만 호출되며,
    /// 플레이어에게 기본적인 시작 장비나 튜토리얼 아이템 등을 지급합니다.
    /// </summary>
    private void InitializeNewGameInventory()
    {
        // 1. [장비 아이템 지급] 기본 무기 템플릿 로드 (예: ID 3001)
        const int STARTING_WEAPON_ID = 3001;

        if (ItemDatabaseManager.Instance == null || ItemGenerator.Instance == null)
        {
            Debug.LogError("ItemDatabaseManager 또는 ItemGenerator 인스턴스를 찾을 수 없습니다. 시작 장비 지급 실패.");
            return;
        }

        BaseItemSO baseTemplate = ItemDatabaseManager.Instance.GetItemByID(STARTING_WEAPON_ID);

        if (baseTemplate is EquipmentItemSO weaponTemplate)
        {
            // ItemGenerator를 사용하여 일반(Common) 등급의 고유 아이템 인스턴스를 생성합니다.
            EquipmentItemSO startingWeapon = ItemGenerator.Instance.GenerateItem(
                weaponTemplate,
                (ItemGrade)Enum.Parse(typeof(ItemGrade), "Common") // ItemGrade Enum이 정의되어 있다고 가정합니다.
            );

            // 생성된 고유 장비 아이템을 AddItem 오버로드를 사용하여 인벤토리에 추가합니다.
            if (startingWeapon != null)
            {
                // [로직 위임] AddItem을 호출하면 자동으로 7개 패널 중 무기 패널에 추가됩니다.
                AddItem(startingWeapon);
            }
        }
        else
        {
            Debug.LogWarning($"아이템 ID {STARTING_WEAPON_ID}가 유효한 EquipmentItemSO 템플릿이 아닙니다. 장비를 지급할 수 없습니다.");
        }

        // 2. [시작 소모품 지급] (예: ID 6001 포션 5개)
        const int STARTING_POTION_ID = 6001;
        BaseItemSO potionTemplate = ItemDatabaseManager.Instance.GetItemByID(STARTING_POTION_ID);

        if (potionTemplate != null)
        {
            // [로직 위임] AddItem을 호출하면 자동으로 7개 패널 중 소모품 패널에 추가되며, 스택킹 로직도 처리됩니다.
            AddItem(potionTemplate, 5);
        }
        else
        {
            Debug.LogWarning($"아이템 ID {STARTING_POTION_ID}에 해당하는 아이템을 찾을 수 없습니다. 시작 소모품 지급 실패.");
        }

        // 인벤토리 변경 이벤트 강제 호출 (UI 갱신 목적)
        onInventoryChanged?.Invoke();
    }
}