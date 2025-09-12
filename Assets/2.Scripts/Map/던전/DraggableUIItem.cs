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
            Debug.LogError("Error: DungeonInventoryManager를 씬에서 찾을 수 없습니다.");
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
        // 드래그를 위해 부모를 최상위로 변경
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
            // 매니저에게 고유 ID만 전달하여 데이터 리스트에서 제거 요청
            inventoryManager.RemovePlayerItem(dungeonUIItem.uniqueID);
            // 아이템이 인벤토리 밖으로 나가면 즉시 파괴
            Destroy(gameObject);
        }
        else
        {
            // 인벤토리 안으로 돌아오면 원래 위치로 복귀
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }
}