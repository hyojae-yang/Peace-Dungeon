// ItemSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 인벤토리 아이템 슬롯 UI를 관리하는 스크립트입니다.
/// 아이템 정보와 개수를 표시하고, 마우스 이벤트를 처리합니다.
/// InventoryManager에게 아이템 사용/장착 요청을 전달하는 역할을 합니다.
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

    [Tooltip("버튼 패널이 마우스 포인터로부터 얼마나 떨어져서 나타날지 설정합니다.")]
    private Vector3 buttonPanelOffset = new Vector3(-50, 0, 0);

    // === 내부 데이터 변수 ===
    [SerializeField]
    private BaseItemSO currentItem;
    private int itemCount;

    // 현재 슬롯에 생성된 버튼 패널 인스턴스를 저장합니다.
    private static GameObject instantiatedButtonPanel;

    /// <summary>
    /// 아이템 슬롯의 시각적 정보를 업데이트하는 메서드입니다.
    /// </summary>
    /// <param name="item">슬롯에 할당될 아이템 데이터 (null일 경우 슬롯을 비웁니다)</param>
    /// <param name="count">아이템의 개수</param>
    public void UpdateSlot(BaseItemSO item, int count)
    {
        currentItem = item;
        itemCount = count; // 수정: itemCount += count; -> itemCount = count;

        // 아이템 개수가 0 이하면 슬롯을 비웁니다.
        if (itemCount <= 0)
        {
            item = null;
            currentItem = null;
        }

        if (currentItem != null)
        {
            iconImage.sprite = currentItem.itemIcon;
            iconImage.color = Color.white;
            iconImage.type = Image.Type.Simple;
            iconImage.enabled = true;
            countText.text = itemCount.ToString();
            countText.gameObject.SetActive(true);
        }
        else
        {
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
        return currentItem;
    }

    /// <summary>
    /// 현재 슬롯에 있는 아이템의 개수를 반환합니다.
    /// </summary>
    /// <returns>아이템 개수</returns>
    public int GetItemCount()
    {
        return itemCount;
    }

    // === 마우스 이벤트 처리 ===

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에 진입했을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            Debug.Log($"툴팁 표시: {currentItem.itemName}");
        }
    }

    /// <summary>
    /// 마우스 포인터가 UI 슬롯에서 벗어났을 때 호출됩니다.
    /// </summary>
    /// <param name="eventData">마우스 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("툴팁 숨김");
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
            }
        }
    }

    /// <summary>
    /// 아이템 우클릭 시 버튼 패널을 활성화하고 위치를 설정합니다.
    /// </summary>
    private void OnRightClick()
    {
        if (currentItem != null)
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
            instantiatedButtonPanel.GetComponent<ButtonPanel>().Initialize(currentItem);
        }
    }
}