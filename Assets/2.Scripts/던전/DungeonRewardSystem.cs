using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List.OrderByDescending을 사용하기 위해 추가

/// <summary>
/// 던전 클리어 시 획득한 모든 보상 정보를 담는 데이터 구조체입니다.
/// </summary>
public struct DungeonRewardData
{
    public int finalScore;
    public int finalGold;
    public int finalExp;
    public List<string> acquiredItemNames;
}

// 이 스크립트는 던전 클리어 점수를 기반으로 보상을 지급하는 역할을 맡습니다.
public class DungeonRewardSystem : MonoBehaviour
{
    public static DungeonRewardSystem Instance { get; private set; }

    [System.Serializable]
    public class RewardTier
    {
        [Tooltip("해당 티어를 위한 최소 점수입니다.")]
        public int minScore;
        [Tooltip("해당 티어에서 적용될 골드 배율입니다.")]
        public float goldMultiplier;
        [Tooltip("해당 티어에서 적용될 경험치 배율입니다.")]
        public float expMultiplier;
    }

    [System.Serializable]
    public class ItemReward
    {
        [Tooltip("이 아이템을 얻기 위한 목표 점수입니다.")]
        public int targetScore;
        [Tooltip("지급할 아이템의 스크립터블 오브젝트입니다.")]
        public BaseItemSO itemData;
        [Tooltip("장비 아이템일 경우, 부여할 등급을 설정합니다. 일반 아이템은 None으로 둡니다.")]
        public ItemGrade itemGrade;
    }

    [Header("점수 및 경험치 보상 설정")]
    [Tooltip("점수 구간별 골드 및 경험치 배율을 설정합니다.")]
    [SerializeField] private List<RewardTier> rewardTiers;

    [Header("아이템 보상 설정")]
    [Tooltip("목표 점수 달성 시 지급할 아이템을 설정합니다.")]
    [SerializeField] private List<ItemReward> itemRewards;

    /// <summary>
    /// Awake 메서드는 스크립트 인스턴스가 로드될 때 호출됩니다.
    /// 싱글톤 패턴을 구현하여 DungeonRewardSystem의 유일한 인스턴스를 보장합니다.
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
    /// 최종 점수를 받아 보상을 계산하고 지급합니다.
    /// 이 메서드는 DungeonManager에 의해 호출됩니다.
    /// </summary>
    /// <param name="finalScore">던전 클리어 시 획득한 최종 점수</param>
    public void GrantReward(int finalScore)
    {
        // 1. 점수 티어에 따른 골드/경험치 배율을 결정합니다.
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

        // 2. 아이템들을 보상 정책에 따라 처리합니다.
        List<string> acquiredItemNames = new List<string>();

        // 최고 점수에 해당하는 장비 아이템을 찾기 위한 변수
        ItemReward bestEquipmentReward = null;

        foreach (var itemReward in itemRewards)
        {
            if (finalScore >= itemReward.targetScore)
            {
                BaseItemSO itemData = itemReward.itemData;
                if (itemData != null)
                {
                    // 장비 아이템인지 확인합니다.
                    if (itemData is EquipmentItemSO)
                    {
                        // 현재까지 찾은 가장 좋은 장비 보상보다 점수가 높으면 갱신
                        if (bestEquipmentReward == null || itemReward.targetScore > bestEquipmentReward.targetScore)
                        {
                            bestEquipmentReward = itemReward;
                        }
                    }
                    else // 장비 아이템이 아닌 일반 아이템은 누적하여 지급합니다.
                    {
                        PlayerCharacter.Instance.inventoryManager.AddItem(itemData);
                        acquiredItemNames.Add(itemData.itemName);
                    }
                }
                else
                {
                    Debug.LogWarning($"목표 점수 {itemReward.targetScore}에 해당하는 아이템이 할당되지 않았습니다.");
                }
            }
        }

        // 3. 최고 점수에 해당하는 장비 아이템을 지급합니다.
        if (bestEquipmentReward != null)
        {
            EquipmentItemSO equipmentItem = bestEquipmentReward.itemData as EquipmentItemSO;
            EquipmentItemSO generatedItem = ItemGenerator.Instance.GenerateItem(equipmentItem, bestEquipmentReward.itemGrade);
            PlayerCharacter.Instance.inventoryManager.AddItem(generatedItem);
            acquiredItemNames.Add(generatedItem.itemName);
        }

        // 4. 골드, 경험치를 플레이어에게 적용합니다.
        PlayerCharacter.Instance.playerStats.gold += finalGold;
        PlayerCharacter.Instance.playerLevelUp.AddExperience(finalExp);

        // 5. 모든 보상 데이터를 UI 매니저로 전달하여 화면에 표시합니다.
        if (DungeonUIManager.Instance != null)
        {
            DungeonUIManager.Instance.ShowResultsScreen(finalScore, finalGold, finalExp, acquiredItemNames);
        }
        else
        {
            Debug.LogWarning("DungeonUIManager가 존재하지 않아 결과 화면을 표시할 수 없습니다.");
        }

    }
}