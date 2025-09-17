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
    private Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();
    private Dictionary<string, int> equippedSetItemCounts = new Dictionary<string, int>();

    // === PlayerAttack 및 시각적 장착 변수 ===
    // 장착된 무기 데이터를 전달할 PlayerAttack 스크립트의 참조 변수입니다.
    private PlayerAttack playerAttack;

    [Header("시각적 장착 설정")]
    [Tooltip("무기 프리팹이 장착될 플레이어 모델의 위치입니다.")]
    public Transform weaponSocket;
    private GameObject equippedWeaponGameObject;

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Awake에서 PlayerAttack 컴포넌트를 찾아서 참조를 가져옵니다.
            playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack == null)
            {
                Debug.LogError("PlayerAttack 컴포넌트를 찾을 수 없습니다! PlayerEquipmentManager와 같은 게임 오브젝트에 PlayerAttack을 추가했는지 확인해주세요.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdatePlayerStats();
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

        if (equippedItems.TryGetValue(slot, out oldItem))
        {
            InventoryManager.Instance.AddItem(oldItem);
        }

        if (InventoryManager.Instance.RemoveItem(itemToEquip, 1))
        {
            equippedItems[slot] = itemToEquip;

            // === 수정된 로직: 장착된 아이템이 무기일 경우 시각적 장착 및 PlayerAttack에 데이터 전달 ===
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

                if (playerAttack != null)
                {
                    playerAttack.UpdateEquippedWeapon(weapon);
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
        //추가할 코드: InventoryUIController에게 UI 갱신을 요청합니다.
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
        if (equippedItems.TryGetValue(slot, out EquipmentItemSO itemToUnequip))
        {
            InventoryManager.Instance.AddItem(itemToUnequip);

            // === 수정된 로직: 해제된 아이템이 무기일 경우 프리팹 파괴 및 PlayerAttack에 null 값 전달 ===
            if (itemToUnequip is WeaponItemSO)
            {
                // 무기 프리팹을 파괴합니다.
                if (equippedWeaponGameObject != null)
                {
                    Destroy(equippedWeaponGameObject);
                }

                if (playerAttack != null)
                {
                    playerAttack.UpdateEquippedWeapon(null);
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
        // 추가할 코드: InventoryUIController에게 UI 갱신을 요청합니다.
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

        if (PlayerStatSystem.Instance != null)
        {
            PlayerStatSystem.Instance.ApplyEquipmentBonuses(equipmentFlatBonuses, equipmentPercentageBonuses);
        }
        else
        {
            Debug.LogError("PlayerStatSystem 인스턴스를 찾을 수 없습니다. 장비 능력치를 갱신할 수 없습니다.");
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
}