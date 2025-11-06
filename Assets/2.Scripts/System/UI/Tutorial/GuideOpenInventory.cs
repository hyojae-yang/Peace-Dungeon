using UnityEngine;

public class GuideOpenInventory : MonoBehaviour
{
    private void OnEnable()
    {
        // 인벤토리 열기 안내 UI 활성화
        UITutorialHandler.Instance.OnInventoryOpened.Invoke();
    }
}
