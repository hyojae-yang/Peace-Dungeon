using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic; // List<T>를 사용하기 위해 추가

/// <summary>
/// 아이템 툴팁 패널의 UI를 관리하는 스크립트입니다.
/// 아이템 정보를 받아와 패널의 내용을 채웁니다.
/// 이 스크립트는 툴팁 패널 프리팹에 부착됩니다.
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    // === 인스펙터에 할당할 UI 컴포넌트 ===
    [Header("공통 툴팁 UI 요소")]
    [Tooltip("아이템 아이콘을 표시할 Image 컴포넌트입니다.")]
    [SerializeField] private Image itemIconImage;

    [Tooltip("아이템 이름을 표시할 Text 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("아이템 설명을 표시할 Text 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    // 이 아래 변수들은 장비 아이템 툴팁 프리팹에만 할당될 수 있습니다. 
    // 일반 아이템 툴팁 프리팹에서는 null이 됩니다.

    [Header("장비 전용 툴팁 UI 요소")]
    [Tooltip("아이템 등급을 표시할 Text 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI itemGradeText;

    [Tooltip("기본 능력치(공격력, 방어력 등)를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI baseStatsText;

    [Tooltip("추가 능력치 1을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI additionalStat1Text;

    [Tooltip("추가 능력치 2를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI additionalStat2Text;

    [Tooltip("추가 능력치 3을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI additionalStat3Text;

    [Tooltip("추가 능력치 4를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI additionalStat4Text;

    [Tooltip("특수 능력치를 표시할 텍스트입니다. (추후 구현 예정)")]
    [SerializeField] private TextMeshProUGUI specialAbilityText;

    /// <summary>
    /// 아이템 정보를 받아 툴팁의 내용을 설정합니다.
    /// 이 메서드는 장비 아이템 전용 UI가 할당되어 있지 않더라도
    /// Null 체크를 통해 안전하게 작동합니다.
    /// </summary>
    /// <param name="item">표시할 아이템의 BaseItemSO 데이터</param>
    public void SetupTooltip(BaseItemSO item)
    {
        // 1. 공통 UI 설정 (이 부분은 모든 툴팁에 존재한다고 가정)
        if (itemIconImage != null) itemIconImage.sprite = item.itemIcon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescription;

        // 2. 장비 아이템일 경우, 추가 UI 요소들을 설정합니다.
        if (item is EquipmentItemSO equipmentItem)
        {
            // 장비 전용 UI 요소들이 null인지 체크 후 설정
            if (itemGradeText != null)
            {
                itemGradeText.text = GetGradeName(equipmentItem.itemGrade);
                itemGradeText.color = GetGradeColor(equipmentItem.itemGrade);
            }

            // 기본 능력치 설정
            if (baseStatsText != null)
            {
                if (equipmentItem.baseStats != null && equipmentItem.baseStats.Count > 0)
                {
                    baseStatsText.text = FormatStats(equipmentItem.baseStats);
                }
                else
                {
                    baseStatsText.text = string.Empty;
                }
            }

            // 추가 능력치 텍스트 4개에 각각 설정
            if (equipmentItem.additionalStats != null)
            {
                if (additionalStat1Text != null) additionalStat1Text.text = equipmentItem.additionalStats.Count > 0 ? FormatStat(equipmentItem.additionalStats[0]) : string.Empty;
                if (additionalStat2Text != null) additionalStat2Text.text = equipmentItem.additionalStats.Count > 1 ? FormatStat(equipmentItem.additionalStats[1]) : string.Empty;
                if (additionalStat3Text != null) additionalStat3Text.text = equipmentItem.additionalStats.Count > 2 ? FormatStat(equipmentItem.additionalStats[2]) : string.Empty;
                if (additionalStat4Text != null) additionalStat4Text.text = equipmentItem.additionalStats.Count > 3 ? FormatStat(equipmentItem.additionalStats[3]) : string.Empty;
            }
            else
            {
                if (additionalStat1Text != null) additionalStat1Text.text = string.Empty;
                if (additionalStat2Text != null) additionalStat2Text.text = string.Empty;
                if (additionalStat3Text != null) additionalStat3Text.text = string.Empty;
                if (additionalStat4Text != null) additionalStat4Text.text = string.Empty;
            }

            // 특수 능력치 설정 (추후 구현 예정)
            if (specialAbilityText != null)
            {
                specialAbilityText.text = "특수 능력치: (추후 구현)";
            }
        }
    }

    /// <summary>
    /// StatModifier 리스트의 정보를 보기 좋게 포맷팅하여 반환합니다.
    /// </summary>
    /// <param name="stats">StatModifier 리스트</param>
    /// <returns>포맷팅된 문자열</returns>
    private string FormatStats(List<StatModifier> stats)
    {
        if (stats == null || stats.Count == 0) return string.Empty;

        StringBuilder sb = new StringBuilder();
        foreach (var stat in stats)
        {
            sb.AppendFormat("{0}: {1}{2}\n", GetStatName(stat.statType), stat.value, stat.isPercentage ? "%" : "");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 단일 StatModifier의 정보를 포맷팅하여 반환합니다.
    /// </summary>
    /// <param name="stat">StatModifier</param>
    /// <returns>포맷팅된 문자열</returns>
    private string FormatStat(StatModifier stat)
    {
        return $"{GetStatName(stat.statType)}: {stat.value}{(stat.isPercentage ? "%" : "")}";
    }

    /// <summary>
    /// 아이템 등급에 따른 색상을 반환합니다.
    /// </summary>
    /// <param name="grade">아이템 등급</param>
    /// <returns>색상</returns>
    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return Color.gray;
            case ItemGrade.Rare: return new Color(0.2f, 0.5f, 1f);
            case ItemGrade.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case ItemGrade.Legendary: return new Color(1f, 0.8f, 0.2f);
            default: return Color.white;
        }
    }

    /// <summary>
    /// 아이템 등급 열거형에 따른 한글 이름을 반환합니다.
    /// </summary>
    /// <param name="grade">아이템 등급</param>
    /// <returns>등급 이름</returns>
    private string GetGradeName(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return "일반";
            case ItemGrade.Rare: return "희귀";
            case ItemGrade.Epic: return "영웅";
            case ItemGrade.Legendary: return "전설";
            default: return "알 수 없음";
        }
    }

    /// <summary>
    /// StatType 열거형에 따른 한글 이름을 반환합니다.
    /// 새로운 스탯이 추가되면 여기에 추가해주어야 합니다.
    /// </summary>
    /// <param name="statType">스탯 종류 열거형</param>
    /// <returns>스탯 한글 이름</returns>
    private string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHealth: return "체력";
            case StatType.MaxMana: return "마나";
            case StatType.AttackPower: return "공격력";
            case StatType.MagicAttackPower: return "마법 공격력";
            case StatType.Defense: return "방어력";
            case StatType.MagicDefense: return "마법 방어력";
            case StatType.CriticalChance: return "치명타 확률";
            case StatType.CriticalDamage: return "치명타 피해량";
            case StatType.MoveSpeed: return "이동 속도";
            case StatType.EvasionChance: return "회피 확률";
            case StatType.Strength: return "힘";
            case StatType.Intelligence: return "지능";
            case StatType.Constitution: return "체질";
            case StatType.Agility: return "민첩";
            case StatType.Focus: return "집중력";
            case StatType.Endurance: return "인내력";
            case StatType.Vitality: return "활력";
            // 필요에 따라 추가 스탯을 여기에 매핑
            default: return statType.ToString(); // 정의되지 않은 경우 영어 이름 반환
        }
    }
}