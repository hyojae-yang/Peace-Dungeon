using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq; // LINQ 사용을 위해 추가

/// <summary>
/// 인벤토리 아이템 슬롯 UI를 관리하는 스크립트입니다.
/// 아이템 정보와 개수를 표시하고, 마우스 이벤트를 처리합니다.
/// 장비 아이템의 경우 PlayerEquipmentManager에게 장착 요청을 전달하는 역할을 합니다.
/// </summary>
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === 인스펙터에 할당할 참조 변수 ===
    [Tooltip("아이템의 아이콘을 표시할 Image 컴포넌트입니다.")]
    [SerializeField] private Image iconImage;

    [Tooltip("아이템의 개수를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI countText;

    [Header("UI 동적 생성")]
    [Tooltip("아이템 우클릭 시 생성할 버튼 패널 프리팹입니다.")]
    [SerializeField] private GameObject buttonPanelPrefab;

    [Tooltip("아이템 툴팁을 표시할 프리팹입니다.")]
    [SerializeField] private GameObject tooltipPrefab;

    [Tooltip("버튼 패널이 마우스 포인터로부터 얼마나 떨어져서 나타날지 설정합니다.")]
    private Vector3 buttonPanelOffset = new Vector3(-50, 0, 0);

    [Tooltip("툴팁 패널이 마우스 포인터로부터 얼마나 떨어져서 나타날지 설정합니다.")]
    private Vector3 tooltipOffset = new Vector3(-200, 50, 0);

    // === 내부 데이터 변수 ===
    private ItemData currentItemData;

    // 현재 슬롯에 생성된 버튼 패널 인스턴스를 저장합니다.
    private static GameObject instantiatedButtonPanel;
    // 현재 슬롯에 생성된 툴팁 인스턴스를 저장합니다.
    private GameObject instantiatedTooltip;

    /// <summary>
    /// 아이템 슬롯의 시각적 정보를 업데이트하는 메서드입니다.
    /// InventoryUIController에서 ItemData를 받아와 슬롯을 갱신합니다.
    /// </summary>
    /// <param name="itemData">슬롯에 할당될 ItemData (null일 경우 슬롯을 비웁니다)</param>
    public void UpdateSlot(ItemData itemData)
    {
        currentItemData = itemData;

        // ItemData가 유효한지(null이 아닌지) 확인합니다.
        if (currentItemData != null && currentItemData.itemSO != null)
        {
            // 아이콘 및 텍스트를 업데이트합니다.
            iconImage.sprite = currentItemData.itemSO.itemIcon;
            iconImage.color = Color.white;
            iconImage.type = Image.Type.Simple;
            iconImage.enabled = true;

            // 아이템이 겹쳐질 수 있는 경우에만 개수를 표시합니다.
            if (currentItemData.itemSO.maxStack > 1)
            {
                countText.text = currentItemData.stackCount.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                // 겹쳐지지 않는 아이템은 개수를 표시하지 않습니다.
                countText.gameObject.SetActive(false);
            }
        }
        else
        {
            // 아이템이 null이면 슬롯을 비웁니다.
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
            iconImage.enabled = false;
            countText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 현재 슬롯에 할당된 아이템 정보를 반환합니다.
    /// </summary>
    /// <returns>BaseItemSO 객체. 비어있을 경우 null.</returns>
    public BaseItemSO GetItem()
    {
        return currentItemData?.itemSO;
    }

    /// <summary>
    /// 현재 슬롯에 있는 아이템의 개수를 반환합니다.
    /// </summary>
    /// <returns>아이템 개수</returns>
    public int GetItemCount()
    {
        return currentItemData?.stackCount ?? 0;
    }

    // === 마우스 이벤트 처리 ===

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에 진입했을 때 호출됩니다.
    /// 툴팁 프리팹을 생성하고 위치를 설정합니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 슬롯에 아이템 정보가 있고, 툴팁 프리팹이 할당되어 있으며, 툴팁이 아직 생성되지 않았다면
        if (currentItemData != null && currentItemData.itemSO != null && tooltipPrefab != null && instantiatedTooltip == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                instantiatedTooltip = Instantiate(tooltipPrefab, canvas.transform);

                // 마우스 위치에 오프셋을 적용하여 툴팁 위치를 설정합니다.
                instantiatedTooltip.transform.position = Input.mousePosition + tooltipOffset;

                // 생성된 툴팁 스크립트를 찾아 아이템 정보를 전달합니다.
                ItemTooltip tooltip = instantiatedTooltip.GetComponent<ItemTooltip>();
                if (tooltip != null)
                {
                    tooltip.SetupTooltip(currentItemData.itemSO);
                }
            }
        }
    }

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에서 벗어났을 때 호출됩니다.
    /// 생성된 툴팁을 파괴합니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
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
        // 1. 우클릭 시, 버튼 패널을 표시합니다.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
        // 2. 좌클릭 시, 버튼 패널을 숨깁니다.
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (instantiatedButtonPanel != null)
            {
                Destroy(instantiatedButtonPanel);
                instantiatedButtonPanel = null; // 참조 해제
            }
        }
    }

    /// <summary>
    /// 아이템 우클릭 시 버튼 패널을 활성화하고 위치를 설정합니다.
    /// 아이템 타입에 따라 버튼의 기능을 설정합니다.
    /// </summary>
    private void OnRightClick()
    {
        if (currentItemData != null && currentItemData.itemSO != null)
        {
            // 기존에 생성된 버튼 패널이 있다면 파괴합니다.
            if (instantiatedButtonPanel != null)
            {
                Destroy(instantiatedButtonPanel);
            }

            // 마우스 위치에 버튼 패널을 새로 생성합니다.
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                instantiatedButtonPanel = Instantiate(buttonPanelPrefab, canvas.transform);
            }

            // 마우스 포인터 위치에 오프셋을 적용합니다.
            instantiatedButtonPanel.transform.position = Input.mousePosition + buttonPanelOffset;
            instantiatedButtonPanel.SetActive(true);

            // 버튼 패널 스크립트의 Initialize 메서드를 호출하여 버튼을 설정합니다.
            // 아이템 타입에 따라 다른 기능을 연결합니다.
            ButtonPanel buttonPanel = instantiatedButtonPanel.GetComponent<ButtonPanel>();
            if (buttonPanel != null)
            {
                // ButtonPanel의 Initialize 메서드를 호출하여 버튼 기능을 설정합니다.
                buttonPanel.Initialize(currentItemData.itemSO, currentItemData.stackCount);
            }
        }
    }
}