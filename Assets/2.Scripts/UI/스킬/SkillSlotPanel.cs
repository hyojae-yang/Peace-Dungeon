using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 이 스크립트는 PlayerSkillController의 스킬 슬롯 변경 이벤트를 구독하여 UI를 업데이트하는 중개자 역할을 합니다.
/// </summary>
public class SkillSlotPanel : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("개별 스킬 슬롯 UI(이미지 등)를 담당하는 SkillSlotUI 컴포넌트 배열입니다.")]
    public SkillSlotUI[] skillUIs;

    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스가 존재하지 않습니다. 씬에 해당 컴포넌트가 있는지 확인해 주세요.");
            return;
        }

        // SkillPointManager는 싱글톤이므로 직접 접근합니다.
        if (SkillPointManager.Instance == null)
        {
            Debug.LogError("SkillPointManager 인스턴스가 존재하지 않습니다. 씬에 해당 컴포넌트가 있는지 확인해 주세요.");
            return;
        }

        // PlayerCharacter를 통해 PlayerSkillController에 접근하여 이벤트에 구독합니다.
        if (playerCharacter.playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController가 PlayerCharacter에 할당되지 않았습니다.");
            return;
        }

        playerCharacter.playerSkillController.OnSkillSlotChanged += UpdateSkillSlotUI;
        playerCharacter.playerSkillController.OnCooldownUpdated += UpdateCooldownUI;
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
                // SkillPointManager의 싱글턴 인스턴스를 통해 스킬 레벨을 가져옵니다.
                int currentLevel = SkillPointManager.Instance.GetTempSkillLevel(data.skillId);
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
        if (playerCharacter != null && playerCharacter.playerSkillController != null)
        {
            // 이 스크립트가 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
            playerCharacter.playerSkillController.OnSkillSlotChanged -= UpdateSkillSlotUI;
            playerCharacter.playerSkillController.OnCooldownUpdated -= UpdateCooldownUI;
        }
    }
}