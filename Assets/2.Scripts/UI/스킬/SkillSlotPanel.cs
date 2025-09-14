using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 이 스크립트는 PlayerSkillController의 스킬 슬롯 변경 이벤트를 구독하여 UI를 업데이트하는 중개자 역할을 합니다.
public class SkillSlotPanel : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("개별 스킬 슬롯 UI(이미지 등)를 담당하는 SkillSlotUI 컴포넌트 배열입니다.")]
    public SkillSlotUI[] skillUIs;

    // === 참조 스크립트 ===
    [Header("참조 스크립트")]
    [Tooltip("스킬 데이터를 관리하는 PlayerSkillController를 할당하세요.")]
    public PlayerSkillController playerSkillController;
    [Tooltip("스킬 포인트 및 스킬 레벨 데이터를 관리하는 SkillPointManager를 할당하세요.")]
    public SkillPointManager skillPointManager;

    private void Awake()
    {
        if (playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
            return;
        }
        if (skillPointManager == null)
        {
            Debug.LogError("SkillPointManager가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
            return;
        }

        // PlayerSkillController의 스킬 슬롯 변경 이벤트와 쿨타임 업데이트 이벤트를 구독합니다.
        playerSkillController.OnSkillSlotChanged += UpdateSkillSlotUI;
        playerSkillController.OnCooldownUpdated += UpdateCooldownUI; // <-- 쿨타임 이벤트 구독 추가
    }

    /// <summary>
    /// PlayerSkillController의 OnSkillSlotChanged 이벤트로부터 호출되어 UI를 업데이트합니다.
    /// </summary>
    /// <param name="slotIndex">변경이 발생한 슬롯의 인덱스</param>
    /// <param name="data">새롭게 등록된 스킬 데이터. 해제 시에는 null입니다.</param>
    private void UpdateSkillSlotUI(int slotIndex, SkillData data)
    {
        if (slotIndex >= 0 && slotIndex < skillUIs.Length)
        {
            float manaCost = 0f;
            if (data != null)
            {
                int currentLevel = skillPointManager.GetTempSkillLevel(data.skillId);
                if (currentLevel > 0 && currentLevel <= data.levelInfo.Length)
                {
                    SkillLevelInfo levelInfo = data.levelInfo[currentLevel - 1];
                    foreach (var stat in levelInfo.stats)
                    {
                        if (stat.statType == StatType.ManaCost)
                        {
                            manaCost = stat.value;
                            break;
                        }
                    }
                }
            }
            skillUIs[slotIndex].UpdateUI(data, manaCost);
        }
        else
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다: " + slotIndex);
        }
    }

    /// <summary>
    /// PlayerSkillController의 OnCooldownUpdated 이벤트로부터 호출되어 쿨타임 UI를 업데이트합니다.
    /// </summary>
    /// <param name="slotIndex">쿨타임이 갱신된 슬롯의 인덱스</param>
    /// <param name="remainingCooldown">남은 쿨타임 시간 (초)</param>
    /// <param name="maxCooldown">스킬의 최대 쿨타임 시간 (초)</param>
    private void UpdateCooldownUI(int slotIndex, float remainingCooldown, float maxCooldown)
    {
        if (slotIndex >= 0 && slotIndex < skillUIs.Length)
        {
            skillUIs[slotIndex].UpdateCooldownUI(remainingCooldown, maxCooldown);
        }
        else
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다: " + slotIndex);
        }
    }

    private void OnDestroy()
    {
        if (playerSkillController != null)
        {
            // 이 스크립트가 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
            playerSkillController.OnSkillSlotChanged -= UpdateSkillSlotUI;
            playerSkillController.OnCooldownUpdated -= UpdateCooldownUI;
        }
    }
}