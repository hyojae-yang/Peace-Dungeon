using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// [SOLID - SRP] ItemSlotUI의 자식으로 붙어 마우스 포인터 이벤트를 감지하고,
/// 부모 ItemSlotUI에게 해당 로직 실행을 위임하는 스크립트입니다.
/// 이 컴포넌트의 RectTransform 크기가 실제 마우스 인식 영역이 됩니다.
/// </summary>
public class ItemPointerDetector : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler // 모든 마우스 이벤트를 감지합니다.
{
    // === 내부 참조 변수 ===
    // 부모에 있는 ItemSlotUI 컴포넌트 참조
    private ItemSlotUI parentSlotUI;

    void Awake()
    {
        // 부모 오브젝트에서 ItemSlotUI 컴포넌트를 찾습니다.
        parentSlotUI = GetComponentInParent<ItemSlotUI>();
        if (parentSlotUI == null)
        {
            Debug.LogError("ItemSlotUI 컴포넌트를 찾을 수 없습니다. 이 스크립트는 ItemSlotUI의 자식 오브젝트에 붙어야 합니다.");
        }
    }

    // === IPointerEnterHandler 구현 ===

    /// <summary>
    /// 마우스 포인터가 감지 영역에 진입했을 때 호출됩니다. (툴팁 표시)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parentSlotUI != null)
        {
            // 부모 ItemSlotUI에게 툴팁을 표시하도록 위임합니다.
            parentSlotUI.ShowTooltip();
        }
    }

    // === IPointerExitHandler 구현 ===

    /// <summary>
    /// 마우스 포인터가 감지 영역에서 벗어났을 때 호출됩니다. (툴팁 숨김)
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (parentSlotUI != null)
        {
            // 부모 ItemSlotUI에게 툴팁을 숨기도록 위임합니다.
            parentSlotUI.HideTooltip();
        }
    }

    // === IPointerClickHandler 구현 ===

    /// <summary>
    /// 마우스 클릭 이벤트를 감지하고 부모 ItemSlotUI에게 위임합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentSlotUI == null) return;

        // 우클릭 시, 버튼 패널 활성화 로직 위임
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            parentSlotUI.HandleRightClick();
        }
        // 좌클릭 시, 버튼 패널 숨김 로직 위임
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            parentSlotUI.HandleLeftClick();
        }
    }
}