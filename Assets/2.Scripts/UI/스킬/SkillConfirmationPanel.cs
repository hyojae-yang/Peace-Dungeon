using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // 정규 표현식 사용을 위해 추가합니다.

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

    // === 외부 스크립트 참조 ===
    [Header("매니저 참조")]
    [Tooltip("스킬 포인트 로직을 관리하는 SkillPointManager 스크립트")]
    public SkillPointManager skillPointManager;

    void Awake()
    {
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

        // SkillPointManager에서 현재 스킬의 실제 레벨을 가져와 임시 레벨로 초기화합니다.
        tempLevel = skillPointManager.GetTempSkillLevel(currentSkillData.skillId);

        // UI 업데이트
        UpdatePanelUI();
    }

    /// <summary>
    /// UI 텍스트들을 현재 임시 레벨에 맞춰 업데이트합니다.
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
                // 스탯 타입과 값을 저장할 딕셔너리 생성
                Dictionary<StatType, float> statValues = new Dictionary<StatType, float>();

                // 현재 레벨의 모든 스탯을 딕셔너리에 저장합니다.
                foreach (var stat in currentLevelInfo.stats)
                {
                    statValues[stat.statType] = stat.value;
                }

                // 정규 표현식을 사용하여 템플릿의 {스탯이름}을 찾아서 값으로 대체합니다.
                string formattedText = Regex.Replace(currentSkillData.statFormatString, @"\{(\w+)\}", match =>
                {
                    string statName = match.Groups[1].Value;
                    StatType statType;

                    // StatType 열거형으로 변환 성공 여부 확인
                    if (System.Enum.TryParse(statName, out statType) && statValues.ContainsKey(statType))
                    {
                        // 해당 스탯의 값을 소수점 2자리로 포맷하여 반환
                        return statValues[statType].ToString("F2");
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
        if (skillPointManager.GetTempSkillPoints() > 0 && tempLevel < currentSkillData.levelInfo.Length)
        {
            // 스킬 포인트 사용 (임시 감소)
            skillPointManager.SpendPoint();
            // 스킬 임시 레벨 증가
            tempLevel++;
            // 스킬 레벨 변경 사항을 SkillPointManager에 통지
            skillPointManager.UpdateTempSkillLevel(currentSkillData.skillId, tempLevel);
            // UI 업데이트
            UpdatePanelUI();
        }
    }

    /// <summary>
    /// 스킬 레벨다운 버튼 클릭 시 호출됩니다.
    /// 스킬 포인트를 반환하고 임시 레벨을 내립니다.
    /// </summary>
    private void OnLevelDownButtonClick()
    {
        // 수정된 로직: SkillPointManager에 레벨 다운이 가능한지 문의합니다.
        if (skillPointManager.CanLevelDown(currentSkillData.skillId))
        {
            // 스킬 포인트 반환 (임시 증가)
            skillPointManager.RefundPoint();
            // 스킬 임시 레벨 감소
            tempLevel--;
            // 스킬 레벨 변경 사항을 SkillPointManager에 통지
            skillPointManager.UpdateTempSkillLevel(currentSkillData.skillId, tempLevel);
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
        bool canLevelUp = skillPointManager.GetTempSkillPoints() > 0 && tempLevel < currentSkillData.levelInfo.Length;
        levelUpButton.interactable = canLevelUp;

        // 레벨 다운 버튼 상태: SkillPointManager에 레벨 다운 가능 여부를 문의합니다.
        bool canLevelDown = skillPointManager.CanLevelDown(currentSkillData.skillId);
        levelDownButton.interactable = canLevelDown;
    }
}