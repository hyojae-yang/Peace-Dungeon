// PlayerEquipmentManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾��� ��� ���� ���¸� �����ϰ�, ��� ������ ���� �ɷ�ġ�� �ݿ��ϴ� ��ũ��Ʈ�Դϴ�.
/// �̱��� �������� �����Ǿ� ��𼭵� ���� ������ �� �ֽ��ϴ�.
/// </summary>
public class PlayerEquipmentManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
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
                    Debug.Log("���ο� 'PlayerEquipmentManagerSingleton' ���� ������Ʈ�� �����߽��ϴ�.");
                }
            }
            return _instance;
        }
    }

    [Tooltip("��� ���� �� �������� �ǵ��� �κ��丮 ������Ʈ�Դϴ�. �ν����Ϳ��� ���� �Ҵ����ּ���.")]
    [SerializeField] private Inventory _inventory;

    /// <summary>
    /// ���� �÷��̾ �����ϰ� �ִ� ��� ������ ����Դϴ�.
    /// EquipSlot �������� Ű�� ����Ͽ� ��� �������� �������� �����մϴ�.
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
            // DontDestroyOnLoad(gameObject); // �ʿ信 ���� �ּ� ����
        }
        // �κ��丮 ������Ʈ�� �Ҵ�Ǿ����� Ȯ���մϴ�.
        if (_inventory == null)
        {
            Debug.LogError("�κ��丮(Inventory) ������Ʈ�� PlayerEquipmentManager�� �Ҵ���� �ʾҽ��ϴ�. �ν����� â���� �Ҵ����ּ���!");
        }
    }

    /// <summary>
    /// ��� �������� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="itemToEquip">������ ��� ������ ��ũ���ͺ� ������Ʈ</param>
    /// <returns>���� ���� ����</returns>
    public bool EquipItem(EquipmentItemSO itemToEquip)
    {
        if (itemToEquip == null)
        {
            Debug.LogError("������ �������� null�Դϴ�.");
            return false;
        }

        // ������ �������� ������ �����ɴϴ�.
        EquipSlot slot = itemToEquip.equipSlot;

        // �ش� ���Կ� �̹� ��� �ִٸ� ���� �����մϴ�.
        if (equippedItems.ContainsKey(slot))
        {
            // UnEquipItem �޼��尡 �κ��丮�� �������� �ǵ����ֹǷ�, ��ȯ���� Ȯ������ �ʽ��ϴ�.
            UnEquipItem(slot);
        }

        // ���ο� �������� �����մϴ�.
        equippedItems[slot] = itemToEquip;
        Debug.Log($"{itemToEquip.itemName}��(��) {slot} ������ �����߽��ϴ�!");

        // ��� ���� �� ���� ������ ������Ʈ�մϴ�.
        UpdatePlayerStats();

        return true;
    }

    /// <summary>
    /// Ư�� ������ ��� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="slot">������ ��� ����</param>
    /// <returns>���� ���� ����</returns>
    public bool UnEquipItem(EquipSlot slot)
    {
        if (!equippedItems.ContainsKey(slot))
        {
            Debug.LogWarning("�ش� ���Կ� ���� �������� �����ϴ�.");
            return false;
        }

        EquipmentItemSO itemToUnequip = equippedItems[slot];

        // ���� ������ �������� �κ��丮�� �ٽ� �߰��մϴ�.
        // �κ��丮�� ���� á�� ��츦 ����� ���� ó���� �ʿ��մϴ�.
        if (_inventory.AddItem(itemToUnequip, 1))
        {
            // �κ��丮 �߰��� �����ϸ� ��� ���Կ��� �����մϴ�.
            equippedItems.Remove(slot);
            Debug.Log($"{itemToUnequip.itemName}��(��) ��� �����߽��ϴ�.");

            // ��� ���� �� ���� ������ ������Ʈ�մϴ�.
            UpdatePlayerStats();
            return true;
        }
        else
        {
            Debug.LogWarning("�κ��丮�� ���� ���� ��� ������ �� �����ϴ�!");
            // �� ���, �������� ��� �������� �ʰ� ���� ���¸� �����մϴ�.
            return false;
        }
    }

    /// <summary>
    /// ���� ������ ��� ����� ���� ���ʽ��� �ջ��Ͽ� PlayerStatSystem�� �����մϴ�.
    /// </summary>
    private void UpdatePlayerStats()
    {
        // ���� ���Ȱ� ����� ������ ���� ������ ��ųʸ�
        Dictionary<StatType, float> finalFlatStats = new Dictionary<StatType, float>();
        Dictionary<StatType, float> finalPercentageStats = new Dictionary<StatType, float>();

        // ��� ���� �������� ��ȸ�մϴ�.
        foreach (var item in equippedItems.Values)
        {
            // �⺻ �ɷ�ġ �ջ�
            foreach (var stat in item.baseStats)
            {
                if (!finalFlatStats.ContainsKey(stat.statType))
                {
                    finalFlatStats[stat.statType] = 0f;
                }
                finalFlatStats[stat.statType] += stat.value;
            }

            // �߰� �ɷ�ġ(�ɼ�) �ջ�
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

        // PlayerStatSystem�� ���� �ջ�� ��� ������ �����Ͽ� ������Ʈ�� ��û�մϴ�.
        PlayerStatSystem.Instance.ApplyEquipmentBonuses(finalFlatStats, finalPercentageStats);

        Debug.Log("��� ������ ������Ʈ�Ǿ����ϴ�!");
    }
}