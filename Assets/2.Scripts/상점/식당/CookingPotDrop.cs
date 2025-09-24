// ���ϸ�: CookingPotDrop.cs (���� ��ũ��Ʈ ����)
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
            Debug.LogError("CookingUIManager �ν��Ͻ��� ã�� �� �����ϴ�.");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (droppedItem != null)
        {
            // ��ӵ� �������� �����Ϳ� ���ӿ�����Ʈ�� �Բ� �����մϴ�.
            ItemData itemData = droppedItem.GetItemData();

            if (cookingUIManager != null)
            {
                // OnItemDroppedInPot �޼��带 ȣ���Ͽ� �����۰� ������Ʈ�� �Բ� �����մϴ�.
                cookingUIManager.OnItemDroppedInPot(itemData, eventData.pointerDrag);

                // ����� ���������Ƿ�, �κ��丮 ���� ������Ʈ�� ��Ȱ��ȭ�մϴ�.
                // �� ����� ����������, �κ��丮�� ���ŵ� ������ �ٽ� Ȱ��ȭ���� �ʽ��ϴ�.
                // �� ���� ����� CookingUIManager�� �κ��丮�� ������ �� �� ������Ʈ�� �ı��ϴ� ���Դϴ�.
            }
        }
    }
}