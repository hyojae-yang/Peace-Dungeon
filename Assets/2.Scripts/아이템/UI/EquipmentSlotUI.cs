// EquipmentSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어 장비 슬롯 UI를 관리하는 스크립트입니다.
/// 마우스 오버, 클릭 등 이벤트를 처리하고, 장비 해제 로직을 InventoryManager에 요청합니다.
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === 장비 슬롯의 착용 부위 설정 (유니티 인스펙터에서 설정) ===
    [Tooltip("이 슬롯이 담당하는 장비 착용 부위를 설정합니다. (예: Weapon, Helmet, Accessory1 등)")]
    public EquipSlot equipSlotType; // 실제 착용 부위

    // === 참조 변수 ===
    [Tooltip("장비 아이템의 아이콘을 표시할 Image 컴포넌트입니다.")]
    [SerializeField] private Image iconImage;

    // === 내부 데이터 ===
    private EquipmentItemSO currentEquippedItem;

    /// <summary>
    /// 장비 슬롯에 아이템을 시각적으로 업데이트하는 메서드입니다.
    /// </summary>
    /// <param name="item">장착된 아이템 데이터 (null일 경우 슬롯을 비웁니다)</param>
    public void UpdateSlot(EquipmentItemSO item)
    {
        currentEquippedItem = item;

        if (currentEquippedItem != null)
        {
            // 아이템이 존재하면 아이콘을 표시하고 이미지의 색상을 불투명하게 만듭니다.
            iconImage.sprite = currentEquippedItem.itemIcon;
            iconImage.color = Color.white; // 아이콘이 보이도록 색상 변경
        }
        else
        {
            // 아이템이 없으면 아이콘을 지우고 투명하게 만듭니다.
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // 완전히 투명하게 만듭니다.
        }
    }

    // === 마우스 이벤트 처리 ===

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에 진입했을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 장착된 아이템이 있을 때만 툴팁을 띄웁니다.
        if (currentEquippedItem != null)
        {
            // TODO: 추후 TooltipManager를 호출하여 툴팁을 표시합니다.
            Debug.Log($"[EquipmentSlotUI] 툴팁 표시 요청: {currentEquippedItem.itemName} (Slot: {equipSlotType})");
        }
    }

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에서 벗어났을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        // TooltipManager를 호출하여 툴팁을 숨깁니다.
        // 추후 구현 예정: TooltipManager.Instance.HideTooltip();
        Debug.Log($"[EquipmentSlotUI] 툴팁 숨김 요청");
    }

    /// <summary>
    /// 마우스 클릭 이벤트를 처리하는 메서드입니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 마우스 오른쪽 버튼 클릭을 감지합니다.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    /// <summary>
    /// 장비 해제 로직을 InventoryManager에게 요청하는 메서드입니다.
    /// </summary>
    public void OnRightClick()
    {
        if (currentEquippedItem != null)
        {
            // InventoryManager에 장비 해제 로직을 요청합니다.
            InventoryManager.Instance.UnEquipItem(equipSlotType);
            Debug.Log($"[EquipmentSlotUI] 장비 해제 요청: {currentEquippedItem.itemName} (Slot: {equipSlotType})");
        }
    }
}