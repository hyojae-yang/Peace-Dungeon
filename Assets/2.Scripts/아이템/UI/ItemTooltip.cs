using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;

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

    [Tooltip("아이템 등급에 따라 색상이 변경될 Image 컴포넌트입니다. (예: 아이콘 배경이나 테두리)")]
    [SerializeField] private Image itemGradeFrameImage; // 추가된 변수: 등급 색상을 반영할 이미지

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

    [Tooltip("세트 효과를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI setBonusText;

    /// <summary>
    /// 아이템 정보를 받아 툴팁의 내용을 설정합니다.
    /// 이 메서드는 장비 아이템 전용 UI가 할당되어 있지 않더라도
    /// Null 체크를 통해 안전하게 작동합니다.
    /// SOLID 원칙: OCP(개방-폐쇄 원칙)를 위해 아이템 타입별 확장 시에도 기존 로직에 미치는 영향을 최소화합니다.
    /// </summary>
    /// <param name="item">표시할 아이템의 BaseItemSO 데이터</param>
    public void SetupTooltip(BaseItemSO item)
    {
        // 1. 공통 UI 설정 (이 부분은 모든 툴팁에 존재한다고 가정)
        if (itemIconImage != null) itemIconImage.sprite = item.itemIcon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescription;

        // 임시 변수: 등급 색상 초기화 (비장비 아이템 대비)
        Color gradeColor = Color.white;

        // 2. 장비 아이템일 경우, 추가 UI 요소들을 설정합니다.
        if (item is EquipmentItemSO equipmentItem)
        {
            gradeColor = GetGradeColor(equipmentItem.itemGrade);

            // 장비 전용 UI 요소들이 null인지 체크 후 설정
            if (itemGradeText != null)
            {
                itemGradeText.text = GetGradeName(equipmentItem.itemGrade);
                itemGradeText.color = gradeColor;
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
                // [리팩토링 제안] 텍스트 필드를 리스트로 관리하면 더욱 깔끔한 반복문 처리가 가능합니다. (현재는 기존 구조 유지)
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

            // 세트 효과 설정
            if (setBonusText != null)
            {
                // 아이템에 세트 ID가 있는지 확인
                if (!string.IsNullOrEmpty(equipmentItem.setID))
                {
                    // SetBonusDataManager로부터 세트 데이터를 가져옵니다.
                    SetBonusDataSO setBonusData = SetBonusDataManager.Instance.GetSetBonusData(equipmentItem.setID);

                    if (setBonusData != null)
                    {
                        // 세트 이름과 단계별 보너스를 보기 좋게 포맷팅합니다.
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"<color=#FFD700>{setBonusData.setName}</color>");

                        // 세트 단계별 보너스를 출력
                        if (setBonusData.bonusSteps != null)
                        {
                            foreach (var step in setBonusData.bonusSteps.OrderBy(s => s.requiredCount))
                            {
                                sb.AppendLine($"<color=#7CFC00>[{step.requiredCount}개 효과]</color>");
                                sb.AppendLine(FormatStats(step.bonusStats));
                            }
                        }

                        setBonusText.text = sb.ToString();
                    }
                    else
                    {
                        setBonusText.text = "세트 데이터를 찾을 수 없습니다.";
                    }
                }
                else
                {
                    // 세트 아이템이 아닐 경우 빈 문자열로 설정
                    setBonusText.text = string.Empty;
                }
            }
        }
        else
        {
            // 장비 아이템이 아닐 경우, 기본 아이템 처리 (등급 색상 초기값인 White 사용)
            gradeColor = Color.white;
        }

        // 3. 등급 프레임 이미지에 색상 적용 (장비든 아니든, 색상은 설정되도록)
        // [핵심 추가] itemGradeFrameImage가 할당되어 있다면 등급 색상(장비: 등급별, 일반: 흰색)을 적용합니다.
        if (itemGradeFrameImage != null)
        {
            itemGradeFrameImage.color = gradeColor;
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
            sb.AppendLine(FormatStat(stat));
        }
        return sb.ToString().TrimEnd(); // 마지막 줄바꿈 제거
    }

    /// <summary>
    /// 단일 StatModifier의 정보를 포맷팅하여 반환합니다.
    /// 이 메서드는 모든 스탯 포맷팅의 단일 책임 원칙을 가집니다.
    /// </summary>
    /// <param name="stat">StatModifier</param>
    /// <returns>포맷팅된 문자열</returns>
    private string FormatStat(StatModifier stat)
    {

        // 퍼센트로 표시할 특정 스탯들을 명시적으로 지정
        bool isSpecialPercentageStat = stat.statType == StatType.CriticalChance ||
                                        stat.statType == StatType.CriticalDamage ||
                                        stat.statType == StatType.MoveSpeed;

        if (isSpecialPercentageStat)
        {
            // 값에 100을 곱하고 소수점 첫째 자리까지 표시
            float displayValue = stat.value * 100f;

            return $"{GetStatName(stat.statType)}: {displayValue.ToString("F1")}%";
        }
        else
        {
            // 그 외의 스탯은 기존 방식대로 표시 (isPercentage 변수 활용)
            return $"{GetStatName(stat.statType)}: {stat.value}{(stat.isPercentage ? "%" : "")}";
        }
    }

    /// <summary>
    /// 아이템 등급에 따른 색상을 반환합니다.
    /// SOLID 원칙: LSP(리스코프 치환 원칙)를 위한 기반 컬러 제공 메서드입니다.
    /// </summary>
    /// <param name="grade">아이템 등급</param>
    /// <returns>색상</returns>
    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return Color.gray;
            case ItemGrade.Uncommon: return new Color(0.1f, 0.6f, 0.1f); // 녹색 계열
            case ItemGrade.Rare: return new Color(0.2f, 0.5f, 1f); // 파란색 계열
            case ItemGrade.Epic: return new Color(0.6f, 0.2f, 0.8f); // 보라색 계열
            case ItemGrade.Legendary: return new Color(1f, 0.8f, 0.2f); // 주황/금색 계열
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
            case ItemGrade.Uncommon: return "고급";
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
            case StatType.Strength: return "힘";
            case StatType.Intelligence: return "지능";
            case StatType.Constitution: return "체질";
            case StatType.Agility: return "민첩";
            case StatType.Focus: return "집중력";
            case StatType.Endurance: return "인내력";
            case StatType.Vitality: return "활력";
            default: return statType.ToString(); // 정의되지 않은 경우 영어 이름 반환
        }
    }
}