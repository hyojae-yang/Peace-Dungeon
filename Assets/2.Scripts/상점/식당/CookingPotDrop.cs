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
        // **�߰��� �ڵ�: ���� ��ᰡ 4�� �̻��̸� ����� �����ϴ�.**
        if (cookingUIManager.currentIngredients.Count >= 4)
        {
            Debug.Log("���� ���� á���ϴ�. �� �̻� ��Ḧ ���� �� �����ϴ�.");
            return; // �޼��� ��� ����
        }

        // �巡�׵� UI ������Ʈ���� DraggableItem ������Ʈ�� �����ɴϴ�.
        DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (droppedItem != null)
        {
            // ��ӵ� �������� ��� ���������� Ȯ���մϴ�.
            ItemData itemData = droppedItem.GetItemData();
            if (itemData.itemSO.itemType == ItemType.Material || itemData.itemSO.itemType == ItemType.Consumable)
            {
                // ����� ���������� DraggableItem ��ũ��Ʈ�� �˷��ݴϴ�.
                droppedItem.SetDroppedOnSlot(true);
            }

            // UI �����ڿ��� �������� ���� ��ӵǾ����� �˸��ϴ�.
            if (cookingUIManager != null)
            {
                // ��ӵ� ������ �����Ϳ� ���� ������Ʈ�� �Բ� �����մϴ�.
                cookingUIManager.OnItemDroppedInPot(itemData, eventData.pointerDrag);
            }
        }
    }
}