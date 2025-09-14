using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// 이 스크립트는 개별 스킬 아이콘의 시각적 표현과 사용자 입력을 관리합니다.
// 마우스 오버 시 툴팁을 표시하고, 클릭 시 확인 창 또는 슬롯 등록 패널을 띄웁니다.
public class SkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === 외부 참조 (인스펙터에서 할당) ===
    [Header("UI 컴포넌트")]
    [Tooltip("스킬 이미지를 표시할 Image 컴포넌트")]
    public Image skillIconImage;

    // === 자식 오브젝트 참조 ===
    [Header("자식 오브젝트")]
    [Tooltip("자식 오브젝트로 배치된 툴팁 패널 스크립트")]
    public SkillTooltip skillTooltip;
    [Tooltip("자식 오브젝트로 배치된 등록/해제 패널 오브젝트")]
    public GameObject registrationPanel;

    // === 외부 스크립트 참조 ===
    [Tooltip("스킬 확인 패널 스크립트")]
    public SkillConfirmationPanel skillConfirmationPanel;
    [Tooltip("1~8번 슬롯 버튼이 담긴 패널 스크립트. RegistrationPanelHandler에서 제어")]
    public SlotSelectionPanel slotSelectionPanel; // SlotSelectionPanel 오브젝트 참조
    [Tooltip("스킬 포인트 로직을 관리하는 SkillPointManager 스크립트")]
    public SkillPointManager skillPointManager; // SkillPointManager 참조 추가

    // === 데이터 참조 ===
    [Tooltip("이 아이콘이 나타내는 스킬의 데이터")]
    public SkillData skillData;

    // --- 내부 플래그 ---
    [Tooltip("등록/해제 또는 슬롯 선택 패널이 활성화되어 툴팁 노출을 막는 상태인지 나타냅니다.")]
    private bool isPanelActive = false; // 다른 패널이 활성화되어 툴팁 노출을 막는 상태인지 확인하는 플래그
    private bool canLearn = false; // 스킬을 배울 수 있는지 여부를 추적하는 변수

    private void OnEnable()
    {
        if (skillData != null)
            Initialize(skillData); // 스킬 데이터로 초기화
    }
    /// <summary>
    /// 스킬 아이콘을 특정 스킬 데이터로 초기화합니다.
    /// </summary>
    /// <param name="data">이 아이콘이 대표할 스킬의 SkillData</param>
    public void Initialize(SkillData data)
    {
        skillData = data;
        if (skillData != null)
        {
            // SkillPointManager가 할당되었는지 확인
            if (skillPointManager == null)
            {
                Debug.LogError("SkillPointManager가 할당되지 않았습니다. 아이콘 초기화 실패.");
                return;
            }

            // 스킬 획득 가능 여부 확인
            canLearn = skillPointManager.CanLearnSkill(skillData);

            // 아이콘 이미지 설정 및 상태 업데이트
            skillIconImage.sprite = data.skillImage;
            UpdateIconState();

            // 초기화 시 툴팁은 비활성화 상태로 시작합니다.
            if (skillTooltip != null)
            {
                skillTooltip.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("할당된 스킬 데이터가 없습니다. 아이콘 초기화 실패.");
        }
    }

    /// <summary>
    /// 스킬의 획득 가능 여부에 따라 아이콘의 시각적 상태를 업데이트합니다.
    /// </summary>
    public void UpdateIconState()
    {
        if (canLearn)
        {
            // 스킬을 배울 수 있으면 정상적인 색상으로 설정
            skillIconImage.color = Color.white;
        }
        else
        {
            // 스킬을 배울 수 없으면 회색으로 설정
            skillIconImage.color = Color.gray;
        }
    }

    /// <summary>
    /// 마우스 포인터가 스킬 아이콘 위로 올라왔을 때 호출됩니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 다른 패널이 활성화되어 있지 않고, 툴팁과 스킬 데이터가 존재할 때만 툴팁을 표시합니다.
        if (!isPanelActive && skillTooltip != null && skillData != null)
        {
            // SkillPointManager에서 현재 스킬의 레벨을 가져옵니다.
            int currentLevel = skillPointManager.GetTempSkillLevel(skillData.skillId);

            // 툴팁에 최신 레벨 정보를 전달합니다.
            skillTooltip.SetTooltipData(skillData, currentLevel);
            skillTooltip.AdjustPosition(eventData.position);
            skillTooltip.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 마우스 포인터가 스킬 아이콘에서 벗어났을 때 호출됩니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 툴팁이 있다면 마우스가 벗어났을 때 비활성화합니다.
        if (skillTooltip != null)
        {
            skillTooltip.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 마우스 클릭이 발생했을 때 호출됩니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (skillData == null)
        {
            Debug.LogWarning("스킬 데이터가 없습니다. 클릭 이벤트 무시.");
            return;
        }

        // 스킬을 배울 수 없는 상태라면 클릭 이벤트 무시
        if (!canLearn)
        {
            // 디버그 메시지로 클릭이 무시되었음을 알림
            Debug.Log($"[Skill Icon] {skillData.skillName} 스킬은 레벨이 부족하여 사용할 수 없습니다.");
            return;
        }

        // 좌클릭 시: 스킬 레벨업 확인 패널 활성화
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (skillConfirmationPanel != null)
            {
                skillConfirmationPanel.ShowPanel(skillData);
                // 레벨업 확인 패널이 띄워지므로, 툴팁을 비활성화하고 플래그를 true로 설정합니다.
                if (skillTooltip != null) skillTooltip.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Skill Confirmation Panel이 할당되지 않았습니다.");
            }
        }
        // 우클릭 시: 등록/해제 패널 활성화
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (registrationPanel != null)
            {
                registrationPanel.SetActive(true);
                // 등록/해제 패널이 띄워지므로, 툴팁을 비활성화하고 플래그를 true로 설정합니다.
                if (skillTooltip != null) skillTooltip.gameObject.SetActive(false);
                isPanelActive = true;
            }
            else
            {
                Debug.LogWarning("Registration Panel이 할당되지 않았습니다.");
            }
        }
    }

    // 이하 메서드는 변경 없음

    /// <summary>
    /// 등록 패널에서 호출되어 스킬 슬롯 선택 패널을 보여줍니다.
    /// </summary>
    public void ShowSlotSelectionPanel()
    {
        if (slotSelectionPanel != null && skillData != null)
        {
            // SlotSelectionPanel의 ShowPanel 메서드를 호출하고, 현재 SkillIcon의 참조와 스킬 데이터를 전달합니다.
            slotSelectionPanel.ShowPanel(this, this.skillData);
        }
        else
        {
            Debug.LogWarning("SlotSelectionPanel 또는 SkillData가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 등록/해제 패널을 비활성화합니다.
    /// </summary>
    public void HideRegistrationPanel()
    {
        if (registrationPanel != null)
        {
            registrationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 모든 관련 UI 패널 (등록/해제, 슬롯 선택)을 닫고, 툴팁 노출 플래그를 초기화합니다.
    /// </summary>
    public void HideAllRelatedPanels()
    {
        // 툴팁을 비활성화하고, isPanelActive 플래그를 false로 재설정합니다.
        if (skillTooltip != null) skillTooltip.gameObject.SetActive(false);
        isPanelActive = false;

        // 등록/해제 패널 비활성화
        if (registrationPanel != null) registrationPanel.SetActive(false);

        // 슬롯 선택 패널 비활성화 (SlotSelectionPanel 스크립트에 정의된 HidePanel() 호출)
        if (slotSelectionPanel != null) slotSelectionPanel.HidePanel();
    }
}