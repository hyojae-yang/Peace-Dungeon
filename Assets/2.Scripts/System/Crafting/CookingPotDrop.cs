// 파일명: CookingPotDrop.cs (기존 스크립트 수정)
using UnityEngine;
using UnityEngine.EventSystems;

public class CookingPotDrop : MonoBehaviour, IDropHandler
{
    private CookingUIManager cookingUIManager;

    private void Start()
    {
        cookingUIManager = CookingUIManager.Instance;
        if (cookingUIManager == null)
        {
            Debug.LogError("CookingUIManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // **추가된 코드: 냄비에 재료가 4개 이상이면 드롭을 막습니다.**
        if (cookingUIManager.currentIngredients.Count >= 4)
        {
            Debug.Log("냄비가 가득 찼습니다. 더 이상 재료를 넣을 수 없습니다.");
            return; // 메서드 즉시 종료
        }

        // 드래그된 UI 오브젝트에서 DraggableItem 컴포넌트를 가져옵니다.
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (droppedItem != null)
        {
            // 드롭된 아이템이 재료 아이템인지 확인합니다.
            ItemData itemData = droppedItem.GetItemData();
            if (itemData.itemSO.itemType == ItemType.Material || itemData.itemSO.itemType == ItemType.Consumable)
            {
                // 드롭이 성공했음을 DraggableItem 스크립트에 알려줍니다.
                droppedItem.SetDroppedOnSlot(true);
            }

            // UI 관리자에게 아이템이 냄비에 드롭되었음을 알립니다.
            if (cookingUIManager != null)
            {
                // 드롭된 아이템 데이터와 게임 오브젝트를 함께 전달합니다.
                cookingUIManager.OnItemDroppedInPot(itemData, eventData.pointerDrag);
            }
        }
    }
}