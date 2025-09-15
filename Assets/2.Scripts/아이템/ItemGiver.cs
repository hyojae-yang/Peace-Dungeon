// ItemGiver.cs
using UnityEngine;

/// <summary>
/// 플레이어에게 아이템을 지급하여 인벤토리 시스템을 테스트하기 위한 스크립트입니다.
/// </summary>
public class ItemGiver : MonoBehaviour
{
    [Header("테스트 아이템 설정")]
    [Tooltip("Q 키를 누를 때 인벤토리에 추가할 아이템을 할당하세요.")]
    [SerializeField] private BaseItemSO itemToGive;

    [Tooltip("추가할 아이템의 개수를 설정하세요.")]
    [SerializeField] private int amountToGive = 1;

    /// <summary>
    /// 매 프레임마다 키 입력을 확인합니다.
    /// </summary>
    private void Update()
    {
        // P 키를 누르면 아이템을 지급합니다.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GiveItem();
        }
    }

    /// <summary>
    /// 설정된 아이템을 InventoryManager를 통해 인벤토리에 추가합니다.
    /// </summary>
    private void GiveItem()
    {
        if (itemToGive != null && InventoryManager.Instance != null)
        {
            // InventoryManager의 AddItem 메서드를 호출하여 아이템을 추가합니다.
            InventoryManager.Instance.AddItem(itemToGive, amountToGive);
            Debug.Log($"<color=green>[System]</color> {itemToGive.itemName} (x{amountToGive}) 아이템을 인벤토리에 추가했습니다.");
        }
        else
        {
            Debug.LogError("<color=red>아이템을 지급할 수 없습니다! itemToGive 또는 InventoryManager가 할당되지 않았습니다.</color>");
        }
    }
}