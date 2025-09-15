// PlayerEquipmentManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 장비 착용 상태를 관리하고, 장비 스탯을 최종 능력치에 반영하는 스크립트입니다.
/// 싱글톤 패턴으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// </summary>
public class PlayerEquipmentManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    private static PlayerEquipmentManager _instance;
    public static PlayerEquipmentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerEquipmentManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("PlayerEquipmentManagerSingleton");
                    _instance = singletonObject.AddComponent<PlayerEquipmentManager>();
                    Debug.Log("새로운 'PlayerEquipmentManagerSingleton' 게임 오브젝트를 생성했습니다.");
                }
            }
            return _instance;
        }
    }

    [Tooltip("장비 해제 시 아이템을 되돌릴 인벤토리 컴포넌트입니다. 인스펙터에서 직접 할당해주세요.")]
    [SerializeField] private Inventory _inventory;

    /// <summary>
    /// 현재 플레이어가 장착하고 있는 장비 아이템 목록입니다.
    /// EquipSlot 열거형을 키로 사용하여 장비 부위별로 아이템을 관리합니다.
    /// </summary>
    public Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject); // 필요에 따라 주석 해제
        }
        // 인벤토리 컴포넌트가 할당되었는지 확인합니다.
        if (_inventory == null)
        {
            Debug.LogError("인벤토리(Inventory) 컴포넌트가 PlayerEquipmentManager에 할당되지 않았습니다. 인스펙터 창에서 할당해주세요!");
        }
    }

    /// <summary>
    /// 장비 아이템을 장착하는 메서드입니다.
    /// </summary>
    /// <param name="itemToEquip">장착할 장비 아이템 스크립터블 오브젝트</param>
    /// <returns>장착 성공 여부</returns>
    public bool EquipItem(EquipmentItemSO itemToEquip)
    {
        if (itemToEquip == null)
        {
            Debug.LogError("장착할 아이템이 null입니다.");
            return false;
        }

        // 장착할 아이템의 슬롯을 가져옵니다.
        EquipSlot slot = itemToEquip.equipSlot;

        // 해당 슬롯에 이미 장비가 있다면 먼저 해제합니다.
        if (equippedItems.ContainsKey(slot))
        {
            // UnEquipItem 메서드가 인벤토리에 아이템을 되돌려주므로, 반환값을 확인하지 않습니다.
            UnEquipItem(slot);
        }

        // 새로운 아이템을 장착합니다.
        equippedItems[slot] = itemToEquip;
        Debug.Log($"{itemToEquip.itemName}을(를) {slot} 부위에 장착했습니다!");

        // 장비 변경 후 최종 스탯을 업데이트합니다.
        UpdatePlayerStats();

        return true;
    }

    /// <summary>
    /// 특정 슬롯의 장비를 해제하는 메서드입니다.
    /// </summary>
    /// <param name="slot">해제할 장비 슬롯</param>
    /// <returns>해제 성공 여부</returns>
    public bool UnEquipItem(EquipSlot slot)
    {
        if (!equippedItems.ContainsKey(slot))
        {
            Debug.LogWarning("해당 슬롯에 장비된 아이템이 없습니다.");
            return false;
        }

        EquipmentItemSO itemToUnequip = equippedItems[slot];

        // 장착 해제된 아이템을 인벤토리에 다시 추가합니다.
        // 인벤토리가 가득 찼을 경우를 대비해 예외 처리가 필요합니다.
        if (_inventory.AddItem(itemToUnequip, 1))
        {
            // 인벤토리 추가에 성공하면 장비 슬롯에서 제거합니다.
            equippedItems.Remove(slot);
            Debug.Log($"{itemToUnequip.itemName}을(를) 장비 해제했습니다.");

            // 장비 해제 후 최종 스탯을 업데이트합니다.
            UpdatePlayerStats();
            return true;
        }
        else
        {
            Debug.LogWarning("인벤토리가 가득 차서 장비를 해제할 수 없습니다!");
            // 이 경우, 아이템을 장비 해제하지 않고 원래 상태를 유지합니다.
            return false;
        }
    }

    /// <summary>
    /// 현재 장착된 모든 장비의 스탯 보너스를 합산하여 PlayerStatSystem에 전달합니다.
    /// </summary>
    private void UpdatePlayerStats()
    {
        // 고정 스탯과 백분율 스탯을 따로 관리할 딕셔너리
        Dictionary<StatType, float> finalFlatStats = new Dictionary<StatType, float>();
        Dictionary<StatType, float> finalPercentageStats = new Dictionary<StatType, float>();

        // 모든 장착 아이템을 순회합니다.
        foreach (var item in equippedItems.Values)
        {
            // 기본 능력치 합산
            foreach (var stat in item.baseStats)
            {
                if (!finalFlatStats.ContainsKey(stat.statType))
                {
                    finalFlatStats[stat.statType] = 0f;
                }
                finalFlatStats[stat.statType] += stat.value;
            }

            // 추가 능력치(옵션) 합산
            foreach (var stat in item.additionalStats)
            {
                if (stat.isPercentage)
                {
                    if (!finalPercentageStats.ContainsKey(stat.statType))
                    {
                        finalPercentageStats[stat.statType] = 0f;
                    }
                    finalPercentageStats[stat.statType] += stat.value;
                }
                else
                {
                    if (!finalFlatStats.ContainsKey(stat.statType))
                    {
                        finalFlatStats[stat.statType] = 0f;
                    }
                    finalFlatStats[stat.statType] += stat.value;
                }
            }
        }

        // PlayerStatSystem에 최종 합산된 장비 스탯을 전달하여 업데이트를 요청합니다.
        PlayerStatSystem.Instance.ApplyEquipmentBonuses(finalFlatStats, finalPercentageStats);

        Debug.Log("장비 스탯이 업데이트되었습니다!");
    }
}