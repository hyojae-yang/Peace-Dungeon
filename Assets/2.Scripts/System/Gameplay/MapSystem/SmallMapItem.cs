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
            Debug.LogError("Error: DungeonInventoryManager를 씬에서 찾을 수 없습니다.");
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
            Debug.LogError("매니저가 할당되지 않아 UI 아이템을 생성할 수 없습니다.");
        }
    }
}