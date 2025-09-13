using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 이 스크립트는 PlayerSkillController의 스킬 슬롯 변경 이벤트를 구독하여 UI를 업데이트하는 중개자 역할을 합니다.
public class SkillSlotPanel : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [Tooltip("개별 스킬 슬롯 UI(이미지 등)를 담당하는 SkillSlotUI 컴포넌트 배열입니다.")]
    public SkillSlotUI[] skillUIs;

    [Header("참조 스크립트")]
    [Tooltip("스킬 데이터를 관리하는 PlayerSkillController를 할당하세요.")]
    public PlayerSkillController playerSkillController;

    private void Awake()
    {
        if (playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
            return;
        }

        // PlayerSkillController의 스킬 슬롯 변경 이벤트를 구독합니다.
        playerSkillController.OnSkillSlotChanged += UpdateSkillSlotUI;
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
            skillUIs[slotIndex].UpdateUI(data);
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
        }
    }
}