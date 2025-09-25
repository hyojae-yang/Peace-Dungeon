using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 장비 착용 상태를 관리하는 스크립트입니다.
/// 이 스크립트는 이제 더 이상 싱글턴이 아니며, PlayerCharacter 스크립트의 멤버로 포함되어 관리됩니다.
/// </summary>
public class PlayerEquipmentManager : MonoBehaviour
{
    // === PlayerCharacter 참조 ===
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // === 이벤트 ===
    /// <summary>
    /// 장비 상태가 변경될 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action onEquipmentChanged;

    // === 데이터 저장용 변수 ===
    private Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();
    private Dictionary<string, int> equippedSetItemCounts = new Dictionary<string, int>();

    [Header("시각적 장착 설정")]
    [Tooltip("무기 프리팹이 장착될 플레이어 모델의 위치입니다.")]
    public Transform weaponSocket;
    private GameObject equippedWeaponGameObject;

    // === MonoBehaviour 메서드 ===
    private void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 모든 시스템에 대한 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다!");
            return;
        }

        // 초기 스탯을 업데이트합니다.
        UpdatePlayerStats();
    }

    /// <summary>
    /// 장비 아이템을 착용하거나 교체하는 메서드입니다.
    /// </summary>
    /// <param name="itemToEquip">장착할 장비 아이템 데이터</param>
    public void EquipItem(EquipmentItemSO itemToEquip)
    {
        if (itemToEquip == null) return;
        if (playerCharacter.inventoryManager == null)
        {
            Debug.LogError("InventoryManager가 PlayerCharacter에 할당되지 않았습니다.");
            return;
        }

        EquipSlot slot = itemToEquip.equipSlot;
        EquipmentItemSO oldItem = null;

        if (equippedItems.TryGetValue(slot, out oldItem))
        {
            playerCharacter.inventoryManager.AddItem(oldItem);
        }

        if (playerCharacter.inventoryManager.RemoveItem(itemToEquip.uniqueID))
        {
            equippedItems[slot] = itemToEquip;

            // 장착된 아이템이 무기일 경우 시각적 장착 및 PlayerAttack에 데이터 전달
            if (itemToEquip is WeaponItemSO weapon)
            {
                // 기존 무기 프리팹이 있다면 파괴합니다.
                if (equippedWeaponGameObject != null)
                {
                    Destroy(equippedWeaponGameObject);
                }

                // 무기 프리팹을 생성하고, weaponSocket의 자식으로 설정합니다.
                if (weapon.weaponPrefab != null && weaponSocket != null)
                {
                    equippedWeaponGameObject = Instantiate(weapon.weaponPrefab, weaponSocket);
                }

                // PlayerAttack 스크립트의 참조를 통해 무기 데이터를 업데이트합니다.
                if (playerCharacter.playerAttack != null)
                {
                    playerCharacter.playerAttack.UpdateEquippedWeapon(weapon);
                }
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>장비 착용 실패:</color> 인벤토리에 {itemToEquip.itemName} 아이템이 없습니다.");
            return;
        }

        onEquipmentChanged?.Invoke();
        UpdatePlayerStats();

        // InventoryUIController에 직접 접근합니다.
        if (InventoryUIController.Instance != null)
        {
            InventoryUIController.Instance.RefreshEquipmentUI();
        }
    }

    /// <summary>
    /// 특정 슬롯에 장비된 아이템을 해제합니다.
    /// </summary>
    /// <param name="slot">해제할 장비 슬롯</param>
    public void UnEquipItem(EquipSlot slot)
    {
        if (playerCharacter.inventoryManager == null)
        {
            Debug.LogError("InventoryManager가 PlayerCharacter에 할당되지 않았습니다.");
            return;
        }

        if (equippedItems.TryGetValue(slot, out EquipmentItemSO itemToUnequip))
        {
            playerCharacter.inventoryManager.AddItem(itemToUnequip);

            // 해제된 아이템이 무기일 경우 프리팹 파괴 및 PlayerAttack에 null 값 전달
            if (itemToUnequip is WeaponItemSO)
            {
                // 무기 프리팹을 파괴합니다.
                if (equippedWeaponGameObject != null)
                {
                    Destroy(equippedWeaponGameObject);
                }

                // PlayerAttack 스크립트의 참조를 통해 무기 데이터를 null로 업데이트합니다.
                if (playerCharacter.playerAttack != null)
                {
                    playerCharacter.playerAttack.UpdateEquippedWeapon(null);
                }
            }

            equippedItems.Remove(slot);
            onEquipmentChanged?.Invoke();
            UpdatePlayerStats();
        }
        else
        {
            Debug.LogWarning($"<color=red>장비 해제 실패:</color> {slot} 슬롯에 장착된 아이템이 없습니다.");
        }

        // InventoryUIController에 직접 접근합니다.
        if (InventoryUIController.Instance != null)
        {
            InventoryUIController.Instance.RefreshEquipmentUI();
        }
    }

    // === (이하 기존 메서드 동일) ===
    public Dictionary<EquipSlot, EquipmentItemSO> GetEquippedItems()
    {
        return equippedItems;
    }
    public int GetEquippedSetCount(string setID)
    {
        if (equippedSetItemCounts.TryGetValue(setID, out int count))
        {
            return count;
        }
        return 0;
    }

    private void UpdatePlayerStats()
    {
        Dictionary<StatType, float> equipmentFlatBonuses = new Dictionary<StatType, float>();
        Dictionary<StatType, float> equipmentPercentageBonuses = new Dictionary<StatType, float>();

        equippedSetItemCounts.Clear();

        foreach (var item in equippedItems.Values)
        {
            AddStatsToDictionary(item.baseStats, equipmentFlatBonuses, equipmentPercentageBonuses);
            AddStatsToDictionary(item.additionalStats, equipmentFlatBonuses, equipmentPercentageBonuses);

            if (!string.IsNullOrEmpty(item.setID))
            {
                if (equippedSetItemCounts.ContainsKey(item.setID))
                {
                    equippedSetItemCounts[item.setID]++;
                }
                else
                {
                    equippedSetItemCounts.Add(item.setID, 1);
                }
            }
        }

        foreach (var setEntry in equippedSetItemCounts)
        {
            string setID = setEntry.Key;
            int equippedCount = setEntry.Value;

            if (SetBonusDataManager.Instance == null)
            {
                Debug.LogError("SetBonusDataManager 인스턴스를 찾을 수 없습니다.");
                continue;
            }

            SetBonusDataSO setBonusData = SetBonusDataManager.Instance.GetSetBonusData(setID);

            if (setBonusData != null)
            {
                foreach (var step in setBonusData.bonusSteps)
                {
                    if (equippedCount >= step.requiredCount)
                    {
                        AddStatsToDictionary(step.bonusStats, equipmentFlatBonuses, equipmentPercentageBonuses);
                    }
                }
            }
        }

        if (playerCharacter.playerStatSystem != null)
        {
            playerCharacter.playerStatSystem.ApplyEquipmentBonuses(equipmentFlatBonuses, equipmentPercentageBonuses);
        }
        else
        {
            Debug.LogError("PlayerStatSystem이 PlayerCharacter에 할당되지 않았습니다. 장비 능력치를 갱신할 수 없습니다.");
        }
    }

    private void AddStatsToDictionary(List<StatModifier> statsToApply, Dictionary<StatType, float> flatBonuses, Dictionary<StatType, float> percentageBonuses)
    {
        foreach (var statBonus in statsToApply)
        {
            if (!statBonus.isPercentage)
            {
                if (flatBonuses.ContainsKey(statBonus.statType))
                {
                    flatBonuses[statBonus.statType] += statBonus.value;
                }
                else
                {
                    flatBonuses.Add(statBonus.statType, statBonus.value);
                }
            }
            else
            {
                if (percentageBonuses.ContainsKey(statBonus.statType))
                {
                    percentageBonuses[statBonus.statType] += statBonus.value;
                }
                else
                {
                    percentageBonuses.Add(statBonus.statType, statBonus.value);
                }
            }
        }
    }
    /// <summary>
    /// 저장된 데이터 로드 시, 장비 아이템을 직접 받아 착용시키는 메서드입니다.
    /// 이 메서드는 인벤토리에서 아이템을 제거하는 로직을 포함하지 않습니다.
    /// SOLID: 개방-폐쇄 원칙 (LoadData() 로직에 대한 확장).
    /// </summary>
    /// <param name="itemToEquip">장착할 장비 아이템 데이터</param>
    /// <param name="slot">장착할 슬롯 위치</param>
    public void EquipItem(EquipmentItemSO itemToEquip, EquipSlot slot)
    {
        if (itemToEquip == null)
        {
            Debug.LogWarning("장착할 아이템이 유효하지 않습니다.");
            return;
        }

        // 해당 슬롯에 이미 아이템이 있는지 확인
        if (equippedItems.TryGetValue(slot, out EquipmentItemSO oldItem))
        {
            // 기존 아이템이 있을 경우, 무기 프리팹을 파괴합니다.
            if (oldItem is WeaponItemSO)
            {
                if (equippedWeaponGameObject != null)
                {
                    Destroy(equippedWeaponGameObject);
                    equippedWeaponGameObject = null;
                }
            }
        }

        // 새로운 아이템을 장착합니다.
        equippedItems[slot] = itemToEquip;

        // 장착된 아이템이 무기일 경우 시각적 장착 및 PlayerAttack에 데이터 전달
        if (itemToEquip is WeaponItemSO weapon)
        {
            // 무기 프리팹을 생성하고, weaponSocket의 자식으로 설정합니다.
            if (weapon.weaponPrefab != null && weaponSocket != null)
            {
                equippedWeaponGameObject = Instantiate(weapon.weaponPrefab, weaponSocket);
            }

            // PlayerAttack 스크립트의 참조를 통해 무기 데이터를 업데이트합니다.
            if (playerCharacter.playerAttack != null)
            {
                playerCharacter.playerAttack.UpdateEquippedWeapon(weapon);
            }
        }

        onEquipmentChanged?.Invoke();
        UpdatePlayerStats();

        // InventoryUIController에 직접 접근합니다.
        if (InventoryUIController.Instance != null)
        {
            InventoryUIController.Instance.RefreshEquipmentUI();
        }
    }

    /// <summary>
    /// 모든 장비 아이템을 해제하고 인벤토리로 되돌립니다.
    /// 로드 시 기존 장비를 초기화할 때 사용됩니다.
    /// SOLID: 단일 책임 원칙 (장비 해제 책임).
    /// </summary>
    public void UnequipAll()
    {
        // === 변경된 코드 ===
        // Dictionary를 직접 순회하며 요소를 제거하면 오류가 발생하므로,
        // 임시 리스트에 제거할 키를 담아두고 나중에 일괄 제거합니다.
        List<EquipSlot> slotsToUnequip = new List<EquipSlot>();
        foreach (var slot in equippedItems.Keys)
        {
            slotsToUnequip.Add(slot);
        }

        foreach (var slot in slotsToUnequip)
        {
            // 실제 장비 해제 로직
            UnEquipItem(slot);
        }
        // ==================
    }
}