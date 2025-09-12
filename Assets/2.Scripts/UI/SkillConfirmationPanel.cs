using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // StringBuilder를 위해 추가합니다.
using System.Collections.Generic;

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
        // 이 로직은 SkillPointManager가 모든 스킬 레벨 데이터를 관리한다는 전제하에 작동합니다.
        tempLevel = skillPointManager.GetSkillCurrentLevel(currentSkillData.skillId);

        // UI 업데이트
        UpdatePanelUI();
    }

    /// <summary>
    /// UI 텍스트들을 현재 임시 레벨에 맞춰 업데이트합니다.
    /// </summary>
    private void UpdatePanelUI()
    {
        // StringBuilder를 사용하여 효율적으로 텍스트를 만듭니다.
        StringBuilder statStringBuilder = new StringBuilder();

        if (currentSkillData != null)
        {
            // 스킬 레벨이 유효한 범위 내에 있는지 확인합니다.
            // tempLevel이 0인 경우 (스킬 미습득)도 포함하여 처리합니다.
            if (tempLevel >= 0 && tempLevel <= currentSkillData.levelInfo.Length)
            {
                skillNameText.text = currentSkillData.skillName;

                // 레벨이 0일 때와 아닐 때를 분리하여 표시합니다.
                if (tempLevel == 0)
                {
                    skillLevelText.text = "Lv. 0 (미습득)";
                    // 다음 레벨(1레벨)의 능력치를 미리 보여줍니다.
                    if (currentSkillData.levelInfo.Length > 0)
                    {
                        SkillLevelInfo nextLevelInfo = currentSkillData.levelInfo[0];
                        statStringBuilder.AppendLine("<color=#FFFF00>다음 레벨 (Lv.1) 능력치:</color>");
                        foreach (SkillStat stat in nextLevelInfo.stats)
                        {
                            statStringBuilder.AppendLine(GetStatText(stat.statType, stat.value));
                        }
                    }
                    else
                    {
                        statStringBuilder.AppendLine("능력치 정보가 없습니다.");
                    }
                }
                else
                {
                    skillLevelText.text = $"Lv. {tempLevel}";
                    // 현재 레벨의 스킬 정보 가져오기
                    SkillLevelInfo currentLevelInfo = currentSkillData.levelInfo[tempLevel - 1];
                    // 능력치 텍스트를 동적으로 생성합니다.
                    if (currentLevelInfo.stats != null && currentLevelInfo.stats.Length > 0)
                    {
                        foreach (SkillStat stat in currentLevelInfo.stats)
                        {
                            statStringBuilder.AppendLine(GetStatText(stat.statType, stat.value));
                        }
                    }
                    else
                    {
                        statStringBuilder.AppendLine("능력치 정보가 없습니다.");
                    }
                }
                skillStatText.text = statStringBuilder.ToString();
            }
            else
            {
                Debug.LogWarning("스킬 레벨이 유효한 범위를 벗어났습니다. 툴팁 업데이트 실패.");
            }
        }
        // 버튼 활성화/비활성화 상태를 업데이트합니다.
        UpdateButtonStates();
    }

    /// <summary>
    /// StatType에 따라 표시될 텍스트를 포맷합니다.
    /// </summary>
    /// <param name="type">스탯 종류</param>
    /// <param name="value">스탯 값</param>
    /// <returns>포맷된 문자열</returns>
    private string GetStatText(StatType type, float value)
    {
        switch (type)
        {
            case StatType.BaseDamage:
                return $"기본 데미지: {value}";
            case StatType.Cooldown:
                return $"쿨타임: {value}초";
            case StatType.ManaCost:
                return $"마나 소모: {value}";
            default:
                return $"{type}: {value}";
        }
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
        // 최소 레벨을 0으로 제한합니다.
        if (tempLevel > 0)
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

        // 레벨 다운 버튼 상태: 현재 레벨이 0보다 클 때 활성화
        bool canLevelDown = tempLevel > 0;
        levelDownButton.interactable = canLevelDown;
    }
}