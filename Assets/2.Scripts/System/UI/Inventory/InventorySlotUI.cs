// 파일명: InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 요리 UI 내 인벤토리 패널의 개별 아이템 슬롯을 관리하는 스크립트입니다.
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    // === UI 참조 ===
    [Header("UI References")]
    [Tooltip("아이템 이미지를 표시하는 UI 컴포넌트입니다.")]
    [SerializeField]
    private Image itemImage;
    [Tooltip("아이템 이름을 표시하는 UI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI itemNameText;

    private DraggableItem draggableItem;

    private void Awake()
    {
        draggableItem = GetComponent<DraggableItem>();
    }

    public void SetData(ItemData itemData)
    {
        if (itemData != null && itemData.itemSO != null)
        {
            itemImage.sprite = itemData.itemSO.itemIcon;
            itemNameText.text = itemData.itemSO.itemName;

            // 추가된 로직: DraggableItem 컴포넌트에 데이터 전달
            if (draggableItem != null)
            {
                draggableItem.SetItemData(itemData);
            }
        }
    }
    // === 데이터 할당 메서드 ===
    /// <summary>
    /// 아이템 데이터를 받아와 UI를 업데이트하는 메서드입니다.
    /// </summary>
    /// <param name="itemData">표시할 아이템 데이터입니다.</param>
   
}