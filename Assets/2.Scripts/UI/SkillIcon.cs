using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // IPointerClickHandler를 위해 추가

// 이 스크립트는 개별 스킬 아이콘의 시각적 표현과 사용자 입력을 관리합니다.
// 마우스 오버 시 툴팁을 표시하고, 클릭 시 확인 창을 띄웁니다.
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

    // === 외부 스크립트 참조 ===
    [Tooltip("스킬 확인 패널 스크립트")]
    public SkillConfirmationPanel skillConfirmationPanel;

    // === 데이터 참조 ===
    [Tooltip("이 아이콘이 나타내는 스킬의 데이터")]
    public SkillData skillData;

    /// <summary>
    /// 스킬 아이콘을 특정 스킬 데이터로 초기화합니다.
    /// </summary>
    /// <param name="data">이 아이콘이 대표할 스킬의 SkillData</param>
    public void Initialize(SkillData data)
    {
        skillData = data;

        if (skillData != null)
        {
            // 스킬 이미지를 업데이트합니다.
            skillIconImage.sprite = skillData.skillImage;

            // 초기화 시 툴팁 패널은 비활성화 상태여야 합니다.
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
    /// 마우스 포인터가 스킬 아이콘 위로 올라왔을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillTooltip != null && skillData != null)
        {
            // 툴팁 패널을 활성화하고 데이터를 전달합니다.
            int currentLevel = 1; // 임시로 1로 설정 (추후 수정)
            skillTooltip.SetTooltipData(skillData, currentLevel);

            // 마우스 포인터의 화면상 위치를 기반으로 툴팁의 위치를 조정합니다.
            skillTooltip.AdjustPosition(eventData.position);

            skillTooltip.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 마우스 포인터가 스킬 아이콘에서 벗어났을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (skillTooltip != null)
        {
            // 툴팁 패널을 비활성화합니다.
            skillTooltip.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 마우스 클릭이 발생했을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (skillConfirmationPanel != null && skillData != null)
        {
            // 스킬 확인 패널을 활성화하고 현재 스킬 데이터를 전달합니다.
            skillConfirmationPanel.ShowPanel(skillData);
        }
        else
        {
            Debug.LogWarning("Skill Confirmation Panel이 할당되지 않았거나 스킬 데이터가 없습니다.");
        }
    }
}