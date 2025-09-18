using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 아이템 등급에 따른 생성 규칙을 정의하는 구조체입니다.
/// </summary>
[Serializable]
public struct ItemGradeRule
{
    public ItemGrade itemGrade;
    [Tooltip("기본 능력치에 적용할 배율의 최솟값(x)과 최댓값(y)입니다.")]
    public Vector2 baseStatMultiplierRange;
    [Tooltip("무작위로 부여할 추가 능력치의 개수입니다.")]
    public int additionalStatCount;
    [Tooltip("추가 능력치의 값에 적용할 배율의 최솟값(x)과 최댓값(y)입니다.")]
    public Vector2 additionalStatMultiplierRange;
}

/// <summary>
/// 장비 아이템을 등급에 따라 동적으로 생성하는 스크립트입니다.
/// </summary>
public class ItemGenerator : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static ItemGenerator Instance { get; private set; }

    [Header("등급별 아이템 생성 규칙")]
    [Tooltip("각 등급에 맞는 규칙을 설정해 주세요.")]
    public List<ItemGradeRule> gradeRules = new List<ItemGradeRule>();

    // 이 딕셔너리는 인스펙터 리스트를 룩업 테이블로 변환하여 성능을 최적화합니다.
    private Dictionary<ItemGrade, ItemGradeRule> gradeRuleMap;

    private void Awake()
    {
        // 싱글턴 패턴 구현 (DontDestroyOnLoad 제거)
        if (Instance == null)
        {
            Instance = this;
            // 규칙 딕셔너리를 초기화하여 O(1) 시간 복잡도로 규칙을 찾을 수 있게 합니다.
            gradeRuleMap = gradeRules.ToDictionary(rule => rule.itemGrade);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 지정된 등급과 기본 템플릿으로 새로운 장비 아이템을 생성합니다.
    /// </summary>
    /// <param name="templateItem">기본 데이터를 가진 EquipmentItemSO 템플릿</param>
    /// <param name="itemGrade">생성할 아이템의 등급</param>
    /// <returns>무작위 옵션이 부여된 새로운 EquipmentItemSO 인스턴스</returns>
    public EquipmentItemSO GenerateItem(EquipmentItemSO templateItem, ItemGrade itemGrade)
    {
        // 1. 유효성 검사 및 규칙 확인
        if (templateItem == null)
        {
            Debug.LogError("아이템 템플릿이 null이므로 아이템을 생성할 수 없습니다.");
            return null;
        }

        if (!gradeRuleMap.TryGetValue(itemGrade, out ItemGradeRule rule))
        {
            Debug.LogWarning($"등급 '{itemGrade}'에 대한 규칙이 ItemGenerator에 설정되지 않았습니다. 기본 값으로 아이템을 생성합니다.");
            return Instantiate(templateItem);
        }

        // 2. 원본 템플릿을 복제하여 새로운 인스턴스를 만듭니다.
        //    Instantiate는 ScriptableObject의 리스트를 깊은 복사하지 않으므로, 아래에서 새로 생성합니다.
        EquipmentItemSO newItem = Instantiate(templateItem);
        newItem.itemGrade = itemGrade;

        // 3. 능력치 리스트를 새로 할당하여 원본 SO와의 참조를 끊고, 데이터 손실을 방지합니다.
        newItem.baseStats = new List<StatModifier>(templateItem.baseStats);
        newItem.additionalStats = new List<StatModifier>(templateItem.additionalStats);

        // 4. 기본 능력치를 등급 규칙에 따라 향상시킵니다.
        UpgradeStats(newItem.baseStats, rule.baseStatMultiplierRange);

        // 5. 추가 능력치를 무작위로 선택하고 등급 규칙에 따라 향상시킵니다.
        SelectAndUpgradeAdditionalStats(newItem.additionalStats, rule);

        return newItem;
    }

    // ---

    /// <summary>
    /// StatModifier 리스트의 값들을 등급 규칙에 따라 무작위로 향상시킵니다.
    /// </summary>
    /// <param name="stats">향상시킬 StatModifier 리스트</param>
    /// <param name="multiplierRange">적용할 배율 범위</param>
    private void UpgradeStats(List<StatModifier> stats, Vector2 multiplierRange)
    {
        float multiplier = UnityEngine.Random.Range(multiplierRange.x, multiplierRange.y);
        for (int i = 0; i < stats.Count; i++)
        {
            StatModifier originalStat = stats[i];
            float upgradedValue = originalStat.value * multiplier;
            // 소수점 첫째 자리로 반올림하여 부동 소수점 오차를 줄입니다.
            float roundedValue = Mathf.Round(upgradedValue * 100f) / 100f;

            stats[i] = new StatModifier(originalStat.statType, roundedValue, originalStat.isPercentage);
        }
    }

    /// <summary>
    /// 추가 능력치 리스트에서 규칙에 맞는 개수만큼 무작위로 선택하고 값을 향상시킵니다.
    /// </summary>
    /// <param name="additionalStats">추가 능력치 리스트</param>
    /// <param name="rule">적용할 등급 규칙</param>
    private void SelectAndUpgradeAdditionalStats(List<StatModifier> additionalStats, ItemGradeRule rule)
    {
        List<StatModifier> newAdditionalStats = new List<StatModifier>();
        // 현재 추가 능력치 리스트를 섞습니다.
        List<StatModifier> shuffledOptions = additionalStats.OrderBy(x => Guid.NewGuid()).ToList();

        // 추가 능력치에 적용할 무작위 배율을 계산합니다.
        float additionalStatMultiplier = UnityEngine.Random.Range(rule.additionalStatMultiplierRange.x, rule.additionalStatMultiplierRange.y);

        int statsToSelect = Math.Min(rule.additionalStatCount, shuffledOptions.Count);
        for (int i = 0; i < statsToSelect; i++)
        {
            StatModifier selectedStat = shuffledOptions[i];
            float upgradedValue = selectedStat.value * additionalStatMultiplier;
            // 소수점 첫째 자리로 반올림
            float roundedValue = Mathf.Round(upgradedValue * 100f) / 100f;

            StatModifier upgradedStat = new StatModifier(selectedStat.statType, roundedValue, selectedStat.isPercentage);
            newAdditionalStats.Add(upgradedStat);
        }

        // 새로운 리스트로 덮어씌웁니다.
        additionalStats.Clear();
        additionalStats.AddRange(newAdditionalStats);
    }
}