using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableUIItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private DungeonInventoryManager inventoryManager;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector3 originalPosition;
    private RectTransform dungeonInventoryRect;

    private DungeonUIItem dungeonUIItem;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dungeonUIItem = GetComponent<DungeonUIItem>();
        inventoryManager = FindFirstObjectByType<DungeonInventoryManager>();

        if (inventoryManager == null)
        {
            Debug.LogError("Error: DungeonInventoryManager�� ������ ã�� �� �����ϴ�.");
        }
    }

    private void Start()
    {
        if (inventoryManager != null)
        {
            dungeonInventoryRect = inventoryManager.GetInventoryRect();
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dungeonUIItem == null) return;

        originalParent = transform.parent;
        originalPosition = transform.position;
        // �巡�׸� ���� �θ� �ֻ����� ����
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dungeonUIItem == null) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dungeonUIItem == null) return;
        if (dungeonInventoryRect == null) return;

        bool isDroppedOutside = !RectTransformUtility.RectangleContainsScreenPoint(
            dungeonInventoryRect,
            eventData.position,
            eventData.pressEventCamera
        );

        if (isDroppedOutside)
        {
            string itemID = dungeonUIItem.GetItemID();
            inventoryManager.Activate3DObject(itemID);
            // �Ŵ������� ���� ID�� �����Ͽ� ������ ����Ʈ���� ���� ��û
            inventoryManager.RemovePlayerItem(dungeonUIItem.uniqueID);
            // �������� �κ��丮 ������ ������ ��� �ı�
            Destroy(gameObject);
        }
        else
        {
            // �κ��丮 ������ ���ƿ��� ���� ��ġ�� ����
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }
}