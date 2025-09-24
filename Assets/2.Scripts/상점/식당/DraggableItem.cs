// ���ϸ�: DraggableItem.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI ��Ҹ� �巡���� �� �ְ� ����� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup; // Canvas Group ������Ʈ �߰�
    private Transform originalParent; // ���� �θ� ������ ����

    // �巡�� ���� �������� �����͸� �����մϴ�.
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
        // 1. ���� �θ� �����մϴ�.
        originalParent = transform.parent;
        // 2. �巡�� ���� �������� �ֻ��� Canvas�� �ڽ����� �����մϴ�.
        //    �̷��� �ϸ� �ٸ� �г� ���� �־ ���������� �������˴ϴ�.
        transform.SetParent(GetComponentInParent<Canvas>().transform);
        // 3. ����ĳ��Ʈ�� ��Ȱ��ȭ�Ͽ� �Ʒ��� �ִ� ������Ʈ�� ��� �̺�Ʈ�� ���� �� �ֵ��� �մϴ�.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ���콺 ��ġ�� ���� ������Ʈ�� �̵���ŵ�ϴ�.
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. �巡�װ� ���� ��, �������� ���ο� �θ� ã���ϴ�.
        Transform dropTarget = eventData.pointerDrag.transform.parent;

        // 2. ���� ��ӵ� ��ġ�� �θ� null�̰ų� ����� �����ߴٸ� ���� ��ġ�� ���ư��ϴ�.
        // �� ������ CookingPotDrop ��ũ��Ʈ���� ��� ���� �� �θ� �ٲٴ� ������ �Բ� ���˴ϴ�.
        if (dropTarget == null)
        {
            transform.SetParent(originalParent);
        }

        // 3. ����ĳ��Ʈ�� �ٽ� Ȱ��ȭ�Ͽ� ���������� UI�� ��ȣ�ۿ��� �� �ְ� �մϴ�.
        canvasGroup.blocksRaycasts = true;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }
}