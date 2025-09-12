using UnityEngine;

public class SmallMapItem : MonoBehaviour
{
    private DungeonInventoryManager inventoryManager;
    public string itemID;

    private void Awake()
    {
        inventoryManager = FindFirstObjectByType<DungeonInventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("Error: DungeonInventoryManager�� ������ ã�� �� �����ϴ�.");
        }
    }

    public void HandleDoubleClick()
    {
        if (inventoryManager != null)
        {
            inventoryManager.Convert3DToUI(this.gameObject, itemID);
        }
        else
        {
            Debug.LogError("�Ŵ����� �Ҵ���� �ʾ� UI �������� ������ �� �����ϴ�.");
        }
    }
}