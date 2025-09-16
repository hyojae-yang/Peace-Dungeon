using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// �÷��̾��� ��� ���� ���¸� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �������� �����ϰ�, �����ϸ�, ��� �ɷ�ġ�� ������Ʈ�մϴ�.
/// </summary>
public class PlayerEquipmentManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static PlayerEquipmentManager Instance { get; private set; }

    // === �̺�Ʈ ===
    /// <summary>
    /// ��� ���°� ����� �� ȣ��Ǵ� �̺�Ʈ�Դϴ�.
    /// </summary>
    public event Action onEquipmentChanged;

    // === ������ ����� ���� ===
    private Dictionary<EquipSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipSlot, EquipmentItemSO>();
    private Dictionary<string, int> equippedSetItemCounts = new Dictionary<string, int>();

    // === PlayerAttack �� �ð��� ���� ���� ===
    // ������ ���� �����͸� ������ PlayerAttack ��ũ��Ʈ�� ���� �����Դϴ�.
    private PlayerAttack playerAttack;

    [Header("�ð��� ���� ����")]
    [Tooltip("���� �������� ������ �÷��̾� ���� ��ġ�Դϴ�.")]
    public Transform weaponSocket;
    private GameObject equippedWeaponGameObject;

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Awake���� PlayerAttack ������Ʈ�� ã�Ƽ� ������ �����ɴϴ�.
            playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack == null)
            {
                Debug.LogError("PlayerAttack ������Ʈ�� ã�� �� �����ϴ�! PlayerEquipmentManager�� ���� ���� ������Ʈ�� PlayerAttack�� �߰��ߴ��� Ȯ�����ּ���.");
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
    /// ��� �������� �����ϰų� ��ü�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="itemToEquip">������ ��� ������ ������</param>
    public void EquipItem(EquipmentItemSO itemToEquip)
    {
        if (itemToEquip == null) return;

        EquipSlot slot = itemToEquip.equipSlot;
        EquipmentItemSO oldItem = null;

        if (equippedItems.TryGetValue(slot, out oldItem))
        {
            InventoryManager.Instance.AddItem(oldItem);
            Debug.Log($"<color=yellow>��� ���� (��ü):</color> {oldItem.itemName}��(��) �κ��丮�� ��ȯ.");
        }

        if (InventoryManager.Instance.RemoveItem(itemToEquip, 1))
        {
            equippedItems[slot] = itemToEquip;
            Debug.Log($"<color=lime>��� ����:</color> {itemToEquip.itemName}��(��) {slot} ���Կ� ����.");

            // === ������ ����: ������ �������� ������ ��� �ð��� ���� �� PlayerAttack�� ������ ���� ===
            if (itemToEquip is WeaponItemSO weapon)
            {
                // ���� ���� �������� �ִٸ� �ı��մϴ�.
                if (equippedWeaponGameObject != null)
                {
                    Destroy(equippedWeaponGameObject);
                }

                // ���� �������� �����ϰ�, weaponSocket�� �ڽ����� �����մϴ�.
                if (weapon.weaponPrefab != null && weaponSocket != null)
                {
                    equippedWeaponGameObject = Instantiate(weapon.weaponPrefab, weaponSocket);
                    // === Ʈ������ �ʱ�ȭ �ڵ� ���� ===
                    // equippedWeaponGameObject.transform.localPosition = Vector3.zero;
                    // equippedWeaponGameObject.transform.localRotation = Quaternion.identity;
                    // equippedWeaponGameObject.transform.localScale = Vector3.one;
                }

                if (playerAttack != null)
                {
                    playerAttack.UpdateEquippedWeapon(weapon);
                    Debug.Log($"<color=cyan>���� ������ ����:</color> {weapon.itemName}�� �����͸� PlayerAttack�� �����߽��ϴ�.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>��� ���� ����:</color> �κ��丮�� {itemToEquip.itemName} �������� �����ϴ�.");
            return;
        }

        onEquipmentChanged?.Invoke();
        UpdatePlayerStats();
    }

    /// <summary>
    /// Ư�� ���Կ� ���� �������� �����մϴ�.
    /// </summary>
    /// <param name="slot">������ ��� ����</param>
    public void UnEquipItem(EquipSlot slot)
    {
        if (equippedItems.TryGetValue(slot, out EquipmentItemSO itemToUnequip))
        {
            InventoryManager.Instance.AddItem(itemToUnequip);
            Debug.Log($"<color=yellow>��� ����:</color> {itemToUnequip.itemName}��(��) {slot} ���Կ��� �����ϰ� �κ��丮�� ��ȯ�߽��ϴ�.");

            // === ������ ����: ������ �������� ������ ��� ������ �ı� �� PlayerAttack�� null �� ���� ===
            if (itemToUnequip is WeaponItemSO)
            {
                // ���� �������� �ı��մϴ�.
                if (equippedWeaponGameObject != null)
                {
                    Destroy(equippedWeaponGameObject);
                }

                if (playerAttack != null)
                {
                    playerAttack.UpdateEquippedWeapon(null);
                    Debug.Log("<color=cyan>���� ������ ����:</color> ���Ⱑ �����Ǿ� PlayerAttack �����͸� �ʱ�ȭ�մϴ�.");
                }
            }

            equippedItems.Remove(slot);
            onEquipmentChanged?.Invoke();
            UpdatePlayerStats();
        }
        else
        {
            Debug.LogWarning($"<color=red>��� ���� ����:</color> {slot} ���Կ� ������ �������� �����ϴ�.");
        }
    }

    // === (���� ���� �޼��� ����) ===
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
            Debug.Log("<color=purple>�ɷ�ġ ����:</color> ��� �� ��Ʈ �ɷ�ġ ���ʽ��� PlayerStatSystem�� �����߽��ϴ�.");
        }
        else
        {
            Debug.LogError("PlayerStatSystem �ν��Ͻ��� ã�� �� �����ϴ�. ��� �ɷ�ġ�� ������ �� �����ϴ�.");
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