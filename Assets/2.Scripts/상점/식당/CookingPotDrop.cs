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
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (droppedItem != null)
        {
            // 드롭된 아이템의 데이터와 게임오브젝트를 함께 전달합니다.
            ItemData itemData = droppedItem.GetItemData();

            if (cookingUIManager != null)
            {
                // OnItemDroppedInPot 메서드를 호출하여 아이템과 오브젝트를 함께 전달합니다.
                cookingUIManager.OnItemDroppedInPot(itemData, eventData.pointerDrag);

                // 드롭이 성공했으므로, 인벤토리 슬롯 오브젝트를 비활성화합니다.
                // 이 방법은 간단하지만, 인벤토리가 갱신될 때마다 다시 활성화되지 않습니다.
                // 더 나은 방법은 CookingUIManager가 인벤토리를 갱신할 때 이 오브젝트를 파괴하는 것입니다.
            }
        }
    }
}