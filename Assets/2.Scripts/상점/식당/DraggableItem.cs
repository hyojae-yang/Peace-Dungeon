// 파일명: DraggableItem.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 요소를 드래그할 수 있게 만드는 스크립트입니다.
/// </summary>
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup; // Canvas Group 컴포넌트 추가
    private Transform originalParent; // 원래 부모를 저장할 변수

    // 드래그 중인 아이템의 데이터를 저장합니다.
    private ItemData itemData;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetItemData(ItemData data)
    {
        itemData = data;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. 원래 부모를 저장합니다.
        originalParent = transform.parent;
        // 2. 드래그 중인 아이템을 최상위 Canvas의 자식으로 설정합니다.
        //    이렇게 하면 다른 패널 위에 있어도 정상적으로 렌더링됩니다.
        transform.SetParent(GetComponentInParent<Canvas>().transform);
        // 3. 레이캐스트를 비활성화하여 아래에 있는 오브젝트가 드롭 이벤트를 받을 수 있도록 합니다.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치에 따라 오브젝트를 이동시킵니다.
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. 드래그가 끝난 후, 아이템의 새로운 부모를 찾습니다.
        Transform dropTarget = eventData.pointerDrag.transform.parent;

        // 2. 만약 드롭된 위치의 부모가 null이거나 드롭이 실패했다면 원래 위치로 돌아갑니다.
        // 이 로직은 CookingPotDrop 스크립트에서 드롭 성공 시 부모를 바꾸는 로직과 함께 사용됩니다.
        if (dropTarget == null)
        {
            transform.SetParent(originalParent);
        }

        // 3. 레이캐스트를 다시 활성화하여 정상적으로 UI와 상호작용할 수 있게 합니다.
        canvasGroup.blocksRaycasts = true;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }
}