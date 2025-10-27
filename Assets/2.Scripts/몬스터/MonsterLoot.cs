using UnityEngine;
using System.Collections.Generic;
using System; // Math.Min을 사용하기 위해 추가

/// <summary>
/// 몬스터 사망 시 보상(경험치, 골드, 아이템)을 지급하는 클래스입니다.
/// 단일 책임 원칙에 따라 보상 지급의 책임만 가집니다.
/// </summary>
public class MonsterLoot : MonoBehaviour
{
    private MonsterBase monsterBase;

    [Header("아이템 등급 드롭 확률 설정")]
    [Tooltip("ItemGrade별 드롭 가중치를 설정합니다. 가중치가 높을수록 해당 등급이 드롭될 확률이 높습니다.")]
    private List<GradeDropWeight> gradeDropWeights = new List<GradeDropWeight>()
    {
        // 예시 가중치. 실제 프로젝트에 맞게 수정해주세요.
        new GradeDropWeight { grade = ItemGrade.Common, weight = 50 },
        new GradeDropWeight { grade = ItemGrade.Uncommon, weight = 40 },
        new GradeDropWeight { grade = ItemGrade.Rare, weight = 30 },
        new GradeDropWeight { grade = ItemGrade.Epic, weight = 20 },
        new GradeDropWeight { grade = ItemGrade.Legendary, weight = 10 }
    };

    /// <summary>
    /// 인스펙터 설정을 위한 ItemGrade와 가중치 구조체입니다.
    /// </summary>
    [Serializable]
    public struct GradeDropWeight
    {
        public ItemGrade grade;
        public int weight; // 드롭 가중치 (0보다 커야 합니다)
    }

    private void Awake()
    {
        // 몬스터의 기본 데이터를 가져옵니다.
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            // 의존성 주입 실패 시 오류 로깅
            Debug.LogError("MonsterLoot: MonsterBase 컴포넌트를 찾을 수 없습니다.", this);
        }
    }

    /// <summary>
    /// 몬스터가 죽었을 때 플레이어에게 보상을 지급하는 메서드입니다.
    /// MonsterBase의 Die() 메서드에서 호출됩니다.
    /// </summary>
    public void GiveReward()
    {
        // 몬스터 데이터 유효성 검사 (계약 조건)
        if (monsterBase == null || monsterBase.monsterData == null)
        {
            Debug.LogError("MonsterLoot: MonsterData가 할당되지 않았거나 MonsterBase를 찾을 수 없습니다.", this);
            return;
        }

        // 몬스터 사망 시 경험치와 골드를 랜덤하게 계산하고 지급합니다.
        int expReward = UnityEngine.Random.Range(monsterBase.monsterData.minExpReward, monsterBase.monsterData.maxExpReward + 1);
        int goldReward = UnityEngine.Random.Range(monsterBase.monsterData.minGoldReward, monsterBase.monsterData.maxGoldReward + 1);

        // 플레이어에게 경험치와 골드를 지급하는 책임 분리
        PlayerCharacter.Instance.playerLevelUp.AddExperience(expReward);
        PlayerCharacter.Instance.playerStats.gold += goldReward;

        // 아이템 드롭 기능 호출
        DropItem();
        // 던전 코인 지급 기능 호출
        GiveDungeonCoinReward();
    }

    /// <summary>
    /// 몬스터 사망 시 던전 코인을 계산하고 지급하는 메서드입니다.
    /// DungeonCoinCurrency에 대한 의존성을 가집니다.
    /// </summary>
    private void GiveDungeonCoinReward()
    {
        // 몬스터 데이터 유효성 검사는 GiveReward에서 이미 수행되었습니다.
        if (monsterBase.monsterData.minDungeonCoinReward <= 0 && monsterBase.monsterData.maxDungeonCoinReward <= 0)
        {
            return;
        }

        // 던전 코인 보상을 랜덤하게 계산합니다.
        int coinReward = UnityEngine.Random.Range(
            monsterBase.monsterData.minDungeonCoinReward,
            monsterBase.monsterData.maxDungeonCoinReward + 1);

        if (coinReward > 0)
        {
            // DungeonCoinCurrency 싱글톤을 사용하여 코인을 추가합니다.
            if (DungeonCoinCurrency.Instance != null)
            {
                DungeonCoinCurrency.Instance.AddCoins(coinReward);
            }
            else
            {
                Debug.LogError("DungeonCoinCurrency 인스턴스를 찾을 수 없습니다! 코인 지급 실패.");
            }
        }
    }

    /// <summary>
    /// 몬스터 사망 시 아이템을 드롭하고 플레이어 인벤토리에 추가하는 로직입니다.
    /// 장비 아이템 드롭 시 ItemGenerator를 사용하여 무작위 등급을 부여하여 생성합니다.
    /// </summary>
    private void DropItem()
    {
        var lootTable = monsterBase.monsterData.lootTable;

        // 1. 드롭할 아이템의 총 개수를 결정합니다.
        int dropCount = UnityEngine.Random.Range(monsterBase.monsterData.minItemDropCount, monsterBase.monsterData.maxItemDropCount + 1);

        // LootTable이 비어 있거나 null이면 처리를 종료합니다.
        if (lootTable == null || lootTable.Count == 0)
        {
            return;
        }

        // 2. 루프를 돌며 아이템을 드롭 개수만큼 선택합니다.
        for (int i = 0; i < dropCount; i++)
        {
            // 3. 모든 아이템의 총 가중치를 계산합니다. (기존 로직 유지)
            int totalWeight = 0;
            foreach (var lootItem in lootTable)
            {
                if (lootItem.weight > 0)
                {
                    totalWeight += lootItem.weight;
                }
            }

            if (totalWeight <= 0)
            {
                break;
            }

            // 4. 총 가중치 범위 내에서 랜덤 값(Drop Point)을 선택합니다. (기존 로직 유지)
            int dropPoint = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;
            BaseItemSO selectedItemData = null; // 당첨 아이템을 저장할 변수

            // 5. 드롭 포인트가 속하는 아이템을 찾아 당첨 아이템으로 결정합니다. (기존 로직 유지)
            foreach (var lootItem in lootTable)
            {
                if (lootItem.weight <= 0) continue;

                currentWeight += lootItem.weight;

                if (dropPoint < currentWeight)
                {
                    selectedItemData = lootItem.itemData;
                    break; // 아이템을 찾았으므로 루프 종료
                }
            }

            // 6. 당첨 아이템이 있다면 처리합니다.
            if (selectedItemData != null)
            {
                // **[핵심 수정]** 장비 아이템인지 확인하고 ItemGenerator를 사용하여 동적으로 생성합니다.
                if (selectedItemData is EquipmentItemSO equipmentItemTemplate)
                {
                    // 6-1. 무작위 등급 결정
                    ItemGrade randomGrade = GetRandomItemGrade();

                    // 6-2. ItemGenerator를 사용하여 아이템 생성
                    if (ItemGenerator.Instance != null)
                    {
                        EquipmentItemSO generatedItem = ItemGenerator.Instance.GenerateItem(equipmentItemTemplate, randomGrade);

                        // 6-3. 생성된 아이템을 인벤토리에 추가
                        PlayerCharacter.Instance.inventoryManager.AddItem(generatedItem, 1);
                        if (NotificationManager.Instance != null)
                        {
                            NotificationManager.Instance.ShowNotification($"{generatedItem.itemName}를(을)\n 획득하였습니다.", NotificationType.General);
                        }
                        // Debug.Log($"장비 아이템 드롭: {generatedItem.itemName} ({randomGrade})");
                    }
                    else
                    {
                        Debug.LogError("ItemGenerator 인스턴스를 찾을 수 없습니다. 장비 아이템을 생성할 수 없습니다.");
                        // 생성 실패 시, 최소한 원본 템플릿이라도 지급하거나, 더 안전한 처리가 필요할 수 있습니다.
                        // 여기서는 오류 로깅 후 다음 드롭으로 넘어갑니다.
                    }
                }
                else
                {
                    // 장비 아이템이 아닌 일반 아이템은 바로 인벤토리에 추가합니다.
                    PlayerCharacter.Instance.inventoryManager.AddItem(selectedItemData, 1);
                    if (NotificationManager.Instance != null)
                    {
                        NotificationManager.Instance.ShowNotification($"{selectedItemData.itemName}를(을) 획득하였습니다.", NotificationType.General);
                    }
                    // Debug.Log($"일반 아이템 드롭: {selectedItemData.itemName}");
                }
            }
        }
    }

    /// <summary>
    /// 설정된 가중치에 따라 무작위 ItemGrade를 결정하는 메서드입니다.
    /// </summary>
    /// <returns>무작위로 결정된 ItemGrade</returns>
    private ItemGrade GetRandomItemGrade()
    {
        // 1. 모든 등급 가중치의 총합을 계산합니다.
        int totalWeight = 0;
        foreach (var dropWeight in gradeDropWeights)
        {
            if (dropWeight.weight > 0)
            {
                totalWeight += dropWeight.weight;
            }
        }

        // 2. 총 가중치가 0 이하라면 기본 등급을 반환하고 경고합니다.
        if (totalWeight <= 0)
        {
            Debug.LogWarning("아이템 등급 드롭 가중치 설정이 유효하지 않습니다. 기본 등급(Normal)을 반환합니다.");
            return ItemGrade.Common;
        }

        // 3. 총 가중치 범위 내에서 랜덤 값을 선택합니다. (0부터 totalWeight-1)
        int dropPoint = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        // 4. 랜덤 값이 속하는 ItemGrade를 찾습니다. (가중치 기반 추첨 로직)
        foreach (var dropWeight in gradeDropWeights)
        {
            if (dropWeight.weight <= 0) continue;

            currentWeight += dropWeight.weight;

            if (dropPoint < currentWeight)
            {
                // 당첨된 등급 반환
                return dropWeight.grade;
            }
        }

        // 예외적인 상황(예: 리스트는 있으나 가중치가 모두 0)을 대비한 기본값 반환
        return ItemGrade.Common;
    }
}