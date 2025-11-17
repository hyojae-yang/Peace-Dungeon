using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// 이 스크립트는 스킬 레벨업/레벨다운을 확인하는 UI 패널을 관리합니다.
// 스킬 상세 정보를 표시하고, 임시 스킬 레벨을 조정하는 기능을 담당합니다.
public class SkillConfirmationPanel : MonoBehaviour
{
    // === 외부 참조 (인스펙터에서 할당) ===
    [Header("UI 컴포넌트")]
    [Tooltip("스킬 이름을 표시할 Text 컴포넌트")]
    public TextMeshProUGUI skillNameText;
    [Tooltip("스킬 레벨을 표시할 Text 컴포넌트")]
    public TextMeshProUGUI skillLevelText;
    [Tooltip("스킬의 능력치를 표시할 Text 컴포넌트")]
    public TextMeshProUGUI skillStatText;

    [Header("버튼 컴포넌트")]
    [Tooltip("스킬 레벨을 올리는 버튼")]
    public Button levelUpButton;
    [Tooltip("스킬 레벨을 내리는 버튼")]
    public Button levelDownButton;
    [Tooltip("패널을 닫는 버튼")]
    public Button closeButton;

    // === 내부 데이터 ===
    [Header("데이터 참조")]
    [Tooltip("현재 패널이 다루는 스킬 데이터")]
    private SkillData currentSkillData;
    [Tooltip("현재 패널이 보여주는 스킬의 임시 레벨")]
    private int tempLevel;

    // SkillPointManager는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.

    void Awake()
    {
        // SkillPointManager 싱글턴 인스턴스가 존재하는지 확인합니다.
        if (SkillPointManager.Instance == null)
        {
            Debug.LogError("SkillPointManager 인스턴스가 존재하지 않습니다. 씬에 SkillPointManager를 가진 게임 오브젝트가 있는지 확인해 주세요.");
            // 버튼 이벤트 연결을 중단합니다.
            return;
        }

        // 버튼 클릭 이벤트를 연결합니다.
        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(OnLevelUpButtonClick);
        }
        if (levelDownButton != null)
        {
            levelDownButton.onClick.AddListener(OnLevelDownButtonClick);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }
    }

    /// <summary>
    /// 스킬 확인 패널을 활성화하고 데이터를 초기화합니다.
    /// 이 메서드는 SkillIcon.cs 스크립트에서 호출됩니다.
    /// </summary>
    /// <param name="data">표시할 스킬 데이터</param>
    public void ShowPanel(SkillData data)
    {
        // 패널 활성화
        gameObject.SetActive(true);

        // 현재 스킬 데이터 저장
        currentSkillData = data;

        // SkillPointManager.Instance에서 현재 스킬의 임시 레벨을 가져와 초기화합니다.
        tempLevel = SkillPointManager.Instance.GetTempSkillLevel(currentSkillData.skillId);

        // UI 업데이트
        UpdatePanelUI();
    }

    /// <summary>
    /// UI 텍스트들을 현재 임시 레벨에 맞춰 업데이트합니다.
    /// 핵심: SkillStat.isPercentage 플래그를 확인하여 스탯 표시 포맷을 동적으로 결정합니다.
    /// </summary>
    private void UpdatePanelUI()
    {
        if (currentSkillData == null) return;

        skillNameText.text = currentSkillData.skillName;

        // 스킬 레벨이 유효한 범위 내에 있는지 확인합니다.
        if (tempLevel >= 0 && tempLevel <= currentSkillData.levelInfo.Length)
        {
            SkillLevelInfo currentLevelInfo = null;

            if (tempLevel == 0)
            {
                skillLevelText.text = "Lv. 0 (미습득)";
                // 다음 레벨(1레벨)의 능력치를 미리 보여줍니다.
                if (currentSkillData.levelInfo.Length > 0)
                {
                    currentLevelInfo = currentSkillData.levelInfo[0];
                }
            }
            else
            {
                skillLevelText.text = $"Lv. {tempLevel}";
                currentLevelInfo = currentSkillData.levelInfo[tempLevel - 1];
            }

            // 스킬 능력치 텍스트를 동적으로 생성합니다.
            if (!string.IsNullOrEmpty(currentSkillData.statFormatString) && currentLevelInfo != null)
            {
                // [수정]: 스탯 타입과 SkillStat 객체 전체를 저장하여 isPercentage 정보를 보존합니다.
                // SkillStat 클래스가 SkillData 스크립트 파일에 정의되어 있다고 가정합니다.
                Dictionary<StatType, SkillStat> statInfos = new Dictionary<StatType, SkillStat>();

                // 현재 레벨의 모든 스탯을 딕셔너리에 저장합니다.
                foreach (var stat in currentLevelInfo.stats)
                {
                    // 스탯 객체 전체를 저장
                    statInfos[stat.statType] = stat;
                }

                // 정규 표현식을 사용하여 템플릿의 {스탯이름}을 찾아서 값으로 대체합니다.
                string formattedText = Regex.Replace(currentSkillData.statFormatString, @"\{(\w+)\}", match =>
                {
                    string statName = match.Groups[1].Value;
                    StatType statType;

                    // StatType 열거형으로 변환 성공 및 딕셔너리에 키가 있는지 확인
                    if (System.Enum.TryParse(statName, out statType) && statInfos.ContainsKey(statType))
                    {
                        // [수정]: 딕셔너리에서 SkillStat 전체를 가져옵니다.
                        SkillStat skillStat = statInfos[statType];
                        float value = skillStat.value;
                        string formattedValue;
                        float multiplier = 1f; // 기본 배수

                        // [핵심 로직]: SkillStat의 isPercentage 플래그 또는 LifestealRate 타입 확인
                        // LifestealRate는 항상 퍼센트로 표시합니다.
                        bool shouldFormatAsPercentage = skillStat.isPercentage || statType == StatType.LifestealRate;

                        if (shouldFormatAsPercentage)
                        {
                            // HealOverTime에 대해서는 1만 곱합니다. (0.1 -> 0.1%)
                            if (statType == StatType.HealOverTime)
                            {
                                multiplier = 1f;
                            }
                            // 그 외의 일반 퍼센트 스탯(AttackPowerRate, LifestealRate 등)은 100을 곱합니다. (0.1 -> 10.0%)
                            else
                            {
                                multiplier = 100f;
                            }

                            // 최종 값 계산 및 포맷 적용
                            formattedValue = (value * multiplier).ToString("F1") + "%";
                        }
                        else
                        {
                            // 그 외의 일반 수치 스탯은 기존대로 소수점 2자리로 포맷하여 반환합니다. (예: 10.0 -> 10.00)
                            formattedValue = value.ToString("F2");
                        }

                        // 수정된 포맷 값을 반환합니다.
                        return formattedValue;
                    }
                    else
                    {
                        // 해당하는 스탯이 없으면 N/A로 반환
                        return "N/A";
                    }
                });

                skillStatText.text = formattedText;
            }
            else
            {
                // statFormatString이 없으면 기본 설명 표시
                skillStatText.text = currentSkillData.skillDescription;
            }
        }
        else
        {
            Debug.LogWarning("스킬 레벨이 유효한 범위를 벗어났습니다.");
            skillStatText.text = "스킬 정보 불러오기 실패.";
        }

        // 버튼 활성화/비활성화 상태를 업데이트합니다.
        UpdateButtonStates();
    }

    /// <summary>
    /// 스킬 레벨업 버튼 클릭 시 호출됩니다.
    /// 스킬 포인트를 사용하여 임시 레벨을 올립니다.
    /// </summary>
    private void OnLevelUpButtonClick()
    {
        // 스킬 포인트가 충분하고, 최대 레벨에 도달하지 않았을 때만 레벨업 진행
        if (SkillPointManager.Instance.GetTempSkillPoints() > 0 && tempLevel < currentSkillData.levelInfo.Length)
        {
            // 스킬 포인트 사용 (임시 감소)
            SkillPointManager.Instance.SpendPoint();
            // 스킬 임시 레벨 증가
            tempLevel++;
            // 스킬 레벨 변경 사항을 SkillPointManager에 통지
            SkillPointManager.Instance.UpdateTempSkillLevel(currentSkillData.skillId, tempLevel);
            // UI 업데이트
            UpdatePanelUI();
            if (UITutorialHandler.Instance != null)
            { UITutorialHandler.Instance.OnSkillAllocationOpened.Invoke(); }
        }
    }

    /// <summary>
    /// 스킬 레벨다운 버튼 클릭 시 호출됩니다.
    /// 스킬 포인트를 반환하고 임시 레벨을 내립니다.
    /// </summary>
    private void OnLevelDownButtonClick()
    {
        // SkillPointManager.Instance에 레벨 다운이 가능한지 문의합니다.
        if (SkillPointManager.Instance.CanLevelDown(currentSkillData.skillId))
        {
            // 스킬 포인트 반환 (임시 증가)
            SkillPointManager.Instance.RefundPoint();
            // 스킬 임시 레벨 감소
            tempLevel--;
            // 스킬 레벨 변경 사항을 SkillPointManager에 통지
            SkillPointManager.Instance.UpdateTempSkillLevel(currentSkillData.skillId, tempLevel);
            // UI 업데이트
            UpdatePanelUI();
        }
    }

    /// <summary>
    /// 닫기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnCloseButtonClick()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 스킬 포인트와 레벨에 따라 버튼 상태를 업데이트합니다.
    /// </summary>
    private void UpdateButtonStates()
    {
        // 레벨업 버튼 상태: 임시 스킬 포인트가 1 이상이고, 최대 레벨에 도달하지 않았을 때 활성화
        bool canLevelUp = SkillPointManager.Instance.GetTempSkillPoints() > 0 && tempLevel < currentSkillData.levelInfo.Length;
        levelUpButton.interactable = canLevelUp;

        // 레벨 다운 버튼 상태: SkillPointManager.Instance에 레벨 다운 가능 여부를 문의합니다.
        bool canLevelDown = SkillPointManager.Instance.CanLevelDown(currentSkillData.skillId);
        levelDownButton.interactable = canLevelDown;
    }
}