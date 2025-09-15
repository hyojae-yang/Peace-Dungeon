using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 이 스크립트는 스킬 등록을 위해 1~8번 슬롯을 선택하는 UI를 관리합니다.
public class SlotSelectionPanel : MonoBehaviour
{
    [Header("슬롯 선택 버튼")]
    [Tooltip("1~8번 슬롯 버튼들을 순서대로 할당하세요.")]
    public Button[] slotButtons;

    // PlayerSkillController는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.
    // [Header("참조 스크립트")]
    // [Tooltip("스킬 데이터를 관리하는 PlayerSkillController를 할당하세요.")]
    // public PlayerSkillController playerSkillController;

    // --- 내부 변수 ---
    private SkillData currentSkillData; // 현재 등록하려는 스킬 데이터를 임시로 저장
    private SkillIcon parentSkillIcon;  // 이 패널을 활성화시킨 SkillIcon 참조

    private void Awake()
    {
        if (PlayerSkillController.Instance == null)
        {
            Debug.LogError("PlayerSkillController 인스턴스가 존재하지 않습니다. 씬에 해당 컴포넌트가 있는지 확인해 주세요.");
            return;
        }

        // 각 슬롯 버튼에 클릭 이벤트 리스너를 추가합니다.
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i; // 클로저 이슈 방지를 위해 로컬 변수 사용
            slotButtons[i].onClick.AddListener(() => OnSlotButtonClick(slotIndex));
        }

        // 초기에는 이 패널을 비활성화 상태로 둡니다.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 등록 요청이 들어왔을 때 이 패널을 활성화하고 스킬 데이터 및 부모 SkillIcon을 받습니다.
    /// </summary>
    /// <param name="skillIcon">이 패널을 활성화시킨 SkillIcon 참조</param>
    /// <param name="skillDataToRegister">등록할 스킬 데이터</param>
    public void ShowPanel(SkillIcon skillIcon, SkillData skillDataToRegister)
    {
        this.parentSkillIcon = skillIcon; // SkillIcon 참조를 저장
        this.currentSkillData = skillDataToRegister;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 스킬 슬롯 버튼 클릭 시 호출됩니다.
    /// </summary>
    /// <param name="slotIndex">클릭된 슬롯의 인덱스</param>
    private void OnSlotButtonClick(int slotIndex)
    {
        if (currentSkillData != null && parentSkillIcon != null)
        {
            // PlayerSkillController.Instance의 스킬 등록 메서드를 호출하여 데이터를 전달합니다.
            PlayerSkillController.Instance.RegisterSkill(slotIndex, currentSkillData);
        }
        else
        {
            Debug.LogWarning("등록할 스킬 데이터 또는 부모 SkillIcon이 없습니다. 스킬 아이콘을 다시 선택해 주세요.");
        }

        // 스킬 등록이 완료되면 모든 관련 UI를 닫고 isPanelActive 플래그를 초기화합니다.
        if (parentSkillIcon != null)
        {
            parentSkillIcon.HideAllRelatedPanels();
        }
        else
        {
            // parentSkillIcon이 null인 예외적인 경우를 대비하여 패널만 닫습니다.
            HidePanel();
        }
    }

    /// <summary>
    /// 패널을 비활성화하고 임시 데이터를 초기화합니다.
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false);
        // 패널이 닫힐 때 임시 변수를 초기화하여 메모리를 정리합니다.
        currentSkillData = null;
        parentSkillIcon = null; // 참조도 초기화
    }
}