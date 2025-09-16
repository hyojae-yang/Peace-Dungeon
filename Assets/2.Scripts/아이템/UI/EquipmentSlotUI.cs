using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어 장비 슬롯 UI를 관리하는 스크립트입니다.
/// 마우스 오버, 클릭 등 이벤트를 처리하고, 장비 해제 로직을 PlayerEquipmentManager에 요청합니다.
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === 장비 슬롯의 착용 부위 설정 (유니티 인스펙터에서 설정) ===
    [Tooltip("이 슬롯이 담당하는 장비 착용 부위를 설정합니다. (예: Weapon, Helmet, Accessory1 등)")]
    public EquipSlot equipSlotType; // 실제 착용 부위

    // === 참조 변수 ===
    [Tooltip("장비 아이템의 아이콘을 표시할 Image 컴포넌트입니다.")]
    [SerializeField] private Image iconImage;

    [Header("UI 동적 생성")]
    [Tooltip("장비 아이템 툴팁을 표시할 프리팹입니다.")]
    [SerializeField] private GameObject tooltipPrefab;

    [Tooltip("툴팁 패널이 마우스 포인터로부터 얼마나 떨어져서 나타날지 설정합니다.")]
    private Vector3 tooltipOffset = new Vector3(-200, 50, 0);

    // === 내부 데이터 ===
    private EquipmentItemSO currentEquippedItem;
    private GameObject instantiatedTooltip;

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
    /// 장착된 아이템이 있을 때만 툴팁을 띄웁니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 장착된 아이템이 있고, 툴팁 프리팹이 할당되어 있으며, 툴팁이 아직 생성되지 않았다면
        if (currentEquippedItem != null && tooltipPrefab != null && instantiatedTooltip == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                instantiatedTooltip = Instantiate(tooltipPrefab, canvas.transform);
                instantiatedTooltip.transform.position = Input.mousePosition + tooltipOffset;

                // 생성된 툴팁 스크립트를 찾아 아이템 정보를 전달합니다.
                ItemTooltip tooltip = instantiatedTooltip.GetComponent<ItemTooltip>();
                if (tooltip != null)
                {
                    tooltip.SetupTooltip(currentEquippedItem);
                }
            }
        }
    }

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에서 벗어났을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 생성된 툴팁을 파괴합니다.
        if (instantiatedTooltip != null)
        {
            Destroy(instantiatedTooltip);
            instantiatedTooltip = null;
        }
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
    /// 장비 해제 로직을 PlayerEquipmentManager에게 요청하는 메서드입니다.
    /// </summary>
    public void OnRightClick()
    {
        if (currentEquippedItem != null)
        {
            // PlayerEquipmentManager에 장비 해제 로직을 요청합니다.
            Debug.Log($"[EquipmentSlotUI] 장비 해제 요청: {currentEquippedItem.itemName} (Slot: {equipSlotType})");
            PlayerEquipmentManager.Instance.UnEquipItem(equipSlotType);
        }
    }
}