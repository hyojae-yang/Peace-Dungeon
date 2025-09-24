using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List.OrderByDescending�� ����ϱ� ���� �߰�

/// <summary>
/// ���� Ŭ���� �� ȹ���� ��� ���� ������ ��� ������ ����ü�Դϴ�.
/// </summary>
public struct DungeonRewardData
{
    public int finalScore;
    public int finalGold;
    public int finalExp;
    public List<string> acquiredItemNames;
}

// �� ��ũ��Ʈ�� ���� Ŭ���� ������ ������� ������ �����ϴ� ������ �ý��ϴ�.
public class DungeonRewardSystem : MonoBehaviour
{
    public static DungeonRewardSystem Instance { get; private set; }

    [System.Serializable]
    public class RewardTier
    {
        [Tooltip("�ش� Ƽ� ���� �ּ� �����Դϴ�.")]
        public int minScore;
        [Tooltip("�ش� Ƽ��� ����� ��� �����Դϴ�.")]
        public float goldMultiplier;
        [Tooltip("�ش� Ƽ��� ����� ����ġ �����Դϴ�.")]
        public float expMultiplier;
    }

    [System.Serializable]
    public class ItemReward
    {
        [Tooltip("�� �������� ��� ���� ��ǥ �����Դϴ�.")]
        public int targetScore;
        [Tooltip("������ �������� ��ũ���ͺ� ������Ʈ�Դϴ�.")]
        public BaseItemSO itemData;
        [Tooltip("��� �������� ���, �ο��� ����� �����մϴ�. �Ϲ� �������� None���� �Ӵϴ�.")]
        public ItemGrade itemGrade;
    }

    [Header("���� �� ����ġ ���� ����")]
    [Tooltip("���� ������ ��� �� ����ġ ������ �����մϴ�.")]
    [SerializeField] private List<RewardTier> rewardTiers;

    [Header("������ ���� ����")]
    [Tooltip("��ǥ ���� �޼� �� ������ �������� �����մϴ�.")]
    [SerializeField] private List<ItemReward> itemRewards;

    /// <summary>
    /// Awake �޼���� ��ũ��Ʈ �ν��Ͻ��� �ε�� �� ȣ��˴ϴ�.
    /// �̱��� ������ �����Ͽ� DungeonRewardSystem�� ������ �ν��Ͻ��� �����մϴ�.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ���� ������ �޾� ������ ����ϰ� �����մϴ�.
    /// �� �޼���� DungeonManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="finalScore">���� Ŭ���� �� ȹ���� ���� ����</param>
    public void GrantReward(int finalScore)
    {
        // 1. ���� Ƽ� ���� ���/����ġ ������ �����մϴ�.
        float goldMultiplier = 1f;
        float expMultiplier = 1f;
        foreach (var tier in rewardTiers)
        {
            if (finalScore >= tier.minScore)
            {
                goldMultiplier = tier.goldMultiplier;
                expMultiplier = tier.expMultiplier;
            }
        }

        int finalGold = Mathf.RoundToInt(finalScore * goldMultiplier);
        int finalExp = Mathf.RoundToInt(finalScore * expMultiplier);

        // 2. �����۵��� ���� ��å�� ���� ó���մϴ�.
        List<string> acquiredItemNames = new List<string>();

        // �ְ� ������ �ش��ϴ� ��� �������� ã�� ���� ����
        ItemReward bestEquipmentReward = null;

        foreach (var itemReward in itemRewards)
        {
            if (finalScore >= itemReward.targetScore)
            {
                BaseItemSO itemData = itemReward.itemData;
                if (itemData != null)
                {
                    // ��� ���������� Ȯ���մϴ�.
                    if (itemData is EquipmentItemSO)
                    {
                        // ������� ã�� ���� ���� ��� ���󺸴� ������ ������ ����
                        if (bestEquipmentReward == null || itemReward.targetScore > bestEquipmentReward.targetScore)
                        {
                            bestEquipmentReward = itemReward;
                        }
                    }
                    else // ��� �������� �ƴ� �Ϲ� �������� �����Ͽ� �����մϴ�.
                    {
                        PlayerCharacter.Instance.inventoryManager.AddItem(itemData);
                        acquiredItemNames.Add(itemData.itemName);
                    }
                }
                else
                {
                    Debug.LogWarning($"��ǥ ���� {itemReward.targetScore}�� �ش��ϴ� �������� �Ҵ���� �ʾҽ��ϴ�.");
                }
            }
        }

        // 3. �ְ� ������ �ش��ϴ� ��� �������� �����մϴ�.
        if (bestEquipmentReward != null)
        {
            EquipmentItemSO equipmentItem = bestEquipmentReward.itemData as EquipmentItemSO;
            EquipmentItemSO generatedItem = ItemGenerator.Instance.GenerateItem(equipmentItem, bestEquipmentReward.itemGrade);
            PlayerCharacter.Instance.inventoryManager.AddItem(generatedItem);
            acquiredItemNames.Add(generatedItem.itemName);
        }

        // 4. ���, ����ġ�� �÷��̾�� �����մϴ�.
        PlayerCharacter.Instance.playerStats.gold += finalGold;
        PlayerCharacter.Instance.playerLevelUp.AddExperience(finalExp);

        // 5. ��� ���� �����͸� UI �Ŵ����� �����Ͽ� ȭ�鿡 ǥ���մϴ�.
        if (DungeonUIManager.Instance != null)
        {
            DungeonUIManager.Instance.ShowResultsScreen(finalScore, finalGold, finalExp, acquiredItemNames);
        }
        else
        {
            Debug.LogWarning("DungeonUIManager�� �������� �ʾ� ��� ȭ���� ǥ���� �� �����ϴ�.");
        }

    }
}