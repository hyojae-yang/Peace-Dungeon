using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 이 스크립트는 스킬 아이콘 내부의 등록/해제 패널을 관리합니다.
// '등록' 버튼 클릭 시 스킬 슬롯 선택 UI를 활성화하고,
// '해제' 버튼 클릭 시 스킬 해제를 요청하며,
// '취소' 버튼 클릭 시 모든 관련 UI를 닫습니다.
public class RegistrationPanelHandler : MonoBehaviour
{
    [Header("UI 버튼")]
    [Tooltip("스킬 등록 버튼을 할당하세요.")]
    public Button registerButton;
    [Tooltip("스킬 해제 버튼을 할당하세요.")]
    public Button unregisterButton;
    [Tooltip("패널을 닫는 취소 버튼을 할당하세요.")]
    public Button cancelButton; // 취소 버튼 변수 추가

    [Header("참조 스크립트")]
    [Tooltip("부모 SkillIcon 스크립트를 할당하세요.")]
    private SkillIcon parentSkillIcon; // 부모 스킬 아이콘 스크립트 참조

    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    void Awake()
    {
        // 부모 오브젝트에서 SkillIcon 컴포넌트를 찾습니다.
        parentSkillIcon = GetComponentInParent<SkillIcon>();
        if (parentSkillIcon == null)
        {
            Debug.LogError("RegistrationPanelHandler는 SkillIcon의 자식으로 배치되어야 합니다.");
            return;
        }

        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스가 존재하지 않습니다. 씬에 해당 컴포넌트가 있는지 확인해 주세요.");
            return;
        }

        // PlayerCharacter를 통해 PlayerSkillController에 접근합니다.
        if (playerCharacter.playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController가 PlayerCharacter에 할당되지 않았습니다.");
            return;
        }

        // '등록' 버튼 클릭 이벤트 설정
        registerButton.onClick.AddListener(OnRegisterButtonClick);

        // '해제' 버튼 클릭 이벤트 설정
        unregisterButton.onClick.AddListener(OnUnregisterButtonClick);

        // '취소' 버튼 클릭 이벤트 설정
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }
        else
        {
            Debug.LogWarning("Cancel Button이 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 등록 버튼 클릭 시 호출됩니다.
    /// 스킬 슬롯 선택 패널 활성화를 부모 SkillIcon에 요청합니다.
    /// </summary>
    private void OnRegisterButtonClick()
    {
        // 등록/해제 패널은 그대로 둔 채, SlotSelectionPanel 활성화를 SkillIcon에 요청합니다.
        parentSkillIcon.ShowSlotSelectionPanel();
    }

    /// <summary>
    /// 스킬 해제 버튼 클릭 시 호출됩니다.
    /// PlayerSkillController에게 스킬 해제를 요청하고, 모든 관련 UI를 닫습니다.
    /// </summary>
    private void OnUnregisterButtonClick()
    {
        if (parentSkillIcon.skillData != null && playerCharacter.playerSkillController != null)
        {
            // PlayerCharacter를 통해 PlayerSkillController의 UnregisterSkill 메서드를 호출하여 스킬 데이터로 해제 요청
            playerCharacter.playerSkillController.UnregisterSkill(parentSkillIcon.skillData);
        }
        else
        {
            Debug.LogWarning("이 아이콘에 할당된 스킬 데이터 또는 PlayerSkillController가 없어 해제할 수 없습니다.");
        }

        // 스킬 해제 후 모든 관련 UI를 닫고 isPanelActive 플래그를 초기화합니다.
        parentSkillIcon.HideAllRelatedPanels();
    }

    /// <summary>
    /// 취소 버튼 클릭 시 호출됩니다.
    /// 모든 관련 UI를 닫고 isPanelActive 플래그를 초기화합니다.
    /// </summary>
    private void OnCancelButtonClick()
    {
        parentSkillIcon.HideAllRelatedPanels();
    }
}