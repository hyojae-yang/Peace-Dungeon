using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// 이 스크립트는 스킬 포인트 시스템의 핵심 로직을 관리합니다.
// 스킬 포인트를 올리고 내리는 기능, 변경 사항을 확정하거나 취소하는 기능을 담당합니다.
public class SkillPointManager : MonoBehaviour
{
    // === UI 및 스크립트 참조 ===
    [Header("UI 및 스크립트 참조")]
    [Tooltip("스킬 포인트를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI skillPointText;
    [Tooltip("플레이어의 PlayerStats 컴포넌트를 할당하세요.")]
    public PlayerStats playerStats;
    [Tooltip("패시브 스킬 효과를 업데이트할 PassiveSkillManager 컴포넌트를 할당하세요.")]
    public PassiveSkillManager passiveSkillManager;

    // === 스킬 포인트 시스템 데이터 ===
    // 'currentSkillPoints'는 현재 플레이어가 보유한 스킬 포인트입니다.
    private int currentSkillPoints;

    // 'tempSkillLevels'는 패널에서 임시로 조작하는 스킬 레벨을 저장합니다.
    private Dictionary<int, int> tempSkillLevels;

    // 스킬 포인트 변경을 외부에 알리는 이벤트
    public event System.Action<int> OnSkillPointsChanged;

    void Awake()
    {
        // PlayerStats가 할당되었는지 확인
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
            return;
        }

        // 스크립트가 시작될 때 임시 스킬 레벨을 초기화
        InitializePoints();
    }

    // === 외부에서 호출되는 메서드 ===

    /// <summary>
    /// 스킬 패널이 열릴 때마다 호출되어, 스킬 포인트를 초기화하고 임시 데이터를 설정합니다.
    /// </summary>
    public void InitializePoints()
    {
        // 최종 스킬 포인트와 스킬 레벨을 임시 데이터로 가져와 초기화합니다.
        currentSkillPoints = playerStats.skillPoints;
        // 깊은 복사(Deep Copy)를 통해 원본 딕셔너리를 보호합니다.
        tempSkillLevels = new Dictionary<int, int>(playerStats.skillLevels);

        // UI 업데이트
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 포인트를 반환합니다.
    /// </summary>
    public int GetTempSkillPoints()
    {
        return currentSkillPoints;
    }

    /// <summary>
    /// 특정 스킬의 현재 임시 레벨을 가져옵니다.
    /// </summary>
    /// <param name="skillId">확인할 스킬의 ID</param>
    /// <returns>임시 레벨, 스킬이 없으면 0을 반환</returns>
    public int GetSkillCurrentLevel(int skillId)
    {
        // tempSkillLevels가 null인 경우 초기화
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

        if (tempSkillLevels.ContainsKey(skillId))
        {
            return tempSkillLevels[skillId];
        }
        return 0; // 스킬을 배우지 않았을 경우
    }

    /// <summary>
    /// 임시 스킬 포인트를 1 감소시킵니다.
    /// </summary>
    public void SpendPoint()
    {
        currentSkillPoints--;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 포인트를 1 증가시킵니다.
    /// </summary>
    public void RefundPoint()
    {
        currentSkillPoints++;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 레벨을 업데이트합니다.
    /// </summary>
    /// <param name="skillId">업데이트할 스킬 ID</param>
    /// <param name="tempLevel">업데이트할 임시 레벨</param>
    public void UpdateTempSkillLevel(int skillId, int tempLevel)
    {
        // tempSkillLevels가 null인 경우 초기화
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

        // 임시 딕셔너리에 스킬 레벨을 업데이트합니다.
        if (tempLevel > 0)
        {
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels[skillId] = tempLevel;
            }
            else
            {
                tempSkillLevels.Add(skillId, tempLevel);
            }
        }
        else
        {
            // 레벨이 0이 되면 딕셔너리에서 제거
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels.Remove(skillId);
            }
        }
    }

    // === 변경사항 최종 적용 및 취소 메서드 ===

    /// <summary>
    /// 변경된 스킬 레벨을 최종적으로 적용합니다.
    /// </summary>
    public void ApplyChanges()
    {
        // === 최종 반영 로직 ===
        // PlayerStats의 딕셔너리에 직접 접근하여 임시 데이터를 복사합니다.
        playerStats.skillLevels = new Dictionary<int, int>(tempSkillLevels);
        playerStats.skillPoints = currentSkillPoints;

        // 최종 반영 후 스킬 효과를 업데이트합니다.
        if (passiveSkillManager != null)
        {
            passiveSkillManager.UpdatePassiveBonuses();
        }

        Debug.Log("스킬 레벨 변경사항이 적용되었습니다.");
    }

    /// <summary>
    /// 변경 사항을 취소하고 원래 상태로 되돌립니다.
    /// </summary>
    public void DiscardChanges()
    {
        // 기존의 최종 데이터를 다시 임시 데이터로 복사합니다.
        InitializePoints();
        Debug.Log("스킬 변경사항이 취소되었습니다.");
    }

    /// <summary>
    /// 스킬 포인트 UI를 업데이트하고 이벤트를 발생시킵니다.
    /// </summary>
    private void UpdateSkillPointUI()
    {
        // UI가 할당되어 있다면 텍스트를 업데이트합니다.
        if (skillPointText != null)
        {
            skillPointText.text = $"스킬포인트: \n{currentSkillPoints}";
        }
        // 외부 구독자(SkillPanel)에게 변경 사항을 알립니다.
        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }
}