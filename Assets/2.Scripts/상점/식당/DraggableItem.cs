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

    // ��� ���� ���θ� ������ �����Դϴ�.
    public bool droppedOnSlot;
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
        // **������ �κ�: ����� �����ߴ��� ���θ� Ȯ���մϴ�.**
        if (!droppedOnSlot)
        {
            // ����� �����߰ų�, ���� ������ �ƴ� ���� ������� ��� ���� ��ġ�� ���ư��ϴ�.
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero; // ���� ��ġ�� �̵� (��Ŀ�� ������ �ʱ�ȭ)
        }

        // 3. ����ĳ��Ʈ�� �ٽ� Ȱ��ȭ�Ͽ� ���������� UI�� ��ȣ�ۿ��� �� �ְ� �մϴ�.
        canvasGroup.blocksRaycasts = true;
    }
    // ��� ���� ���θ� �ܺο��� ������ �� �ִ� �޼����Դϴ�.
    public void SetDroppedOnSlot(bool value)
    {
        droppedOnSlot = value;
    }
    public ItemData GetItemData()
    {
        return itemData;
    }
}