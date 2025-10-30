using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq; // LINQ 사용을 위해 추가

/// <summary>
/// 인벤토리 아이템 슬롯 UI를 관리하는 스크립트입니다.
/// [수정] 모든 마우스 이벤트 처리(감지) 역할은 자식 스크립트(ItemPointerDetector)에게 위임하고, 
/// 이 스크립트는 순수한 데이터 및 동적 UI 생성(툴팁/버튼) 로직만 담당합니다.
/// </summary>
public class ItemSlotUI : MonoBehaviour // IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler 제거됨
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
    private Vector3 buttonPanelOffset = new Vector3(50, -25, 0);

    [Tooltip("툴팁 패널이 마우스 포인터로부터 얼마나 떨어져서 나타날지 설정합니다.")]
    private Vector3 tooltipOffset = new Vector3(-200, 50, 0);

    // === 내부 데이터 변수 ===
    private ItemData currentItemData;

    /// <summary>
    /// 현재 슬롯에 생성된 버튼 패널 인스턴스를 저장합니다.
    /// 버튼 패널은 씬에 하나만 존재하므로 static으로 관리합니다.
    /// </summary>
    public static GameObject instantiatedButtonPanel;

    /// <summary>
    /// 현재 활성화된 툴팁 인스턴스입니다.
    /// 툴팁은 씬에 단 하나만 활성화되므로 static으로 관리하여 모든 슬롯에서 공유합니다.
    /// </summary>
    public static GameObject currentActiveTooltip;

    /// <summary>
    /// 아이템 슬롯의 시각적 정보를 업데이트하는 메서드입니다.
    /// InventoryUIController에서 ItemData를 받아와 슬롯을 갱신합니다.
    /// </summary>
    /// <param name="itemData">슬롯에 할당될 ItemData (null일 경우 슬롯을 비웁니다)</param>
    public void UpdateSlot(ItemData itemData)
    {
        // ... (기존 UpdateSlot 로직은 변경 없음) ...
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

    // === 툴팁 및 버튼 패널 관련 퍼블릭 메서드 (새로운 감지 스크립트에서 호출됨) ===

    /// <summary>
    /// [SRP] 툴팁 생성 로직을 수행합니다. ItemPointerDetector.cs의 OnPointerEnter에서 호출됩니다.
    /// </summary>
    public void ShowTooltip()
    {
        // 슬롯에 아이템 정보가 있고, 툴팁 프리팹이 할당되어 있다면
        if (currentItemData != null && currentItemData.itemSO != null && tooltipPrefab != null)
        {
            // 툴팁 생성 전, 현재 활성화된 다른 툴팁이 있다면 파괴하여 중복 생성을 방지합니다.
            if (currentActiveTooltip != null)
            {
                Destroy(currentActiveTooltip);
                currentActiveTooltip = null;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // 툴팁을 새로 생성하고 static 변수에 할당합니다.
                currentActiveTooltip = Instantiate(tooltipPrefab, canvas.transform);

                // 마우스 위치에 오프셋을 적용하여 툴팁 위치를 설정합니다. (Input.mousePosition은 감지 스크립트가 아닌, 이 로직이 실행될 당시의 마우스 위치를 사용)
                currentActiveTooltip.transform.position = Input.mousePosition + tooltipOffset;

                // 생성된 툴팁 스크립트를 찾아 아이템 정보를 전달합니다.
                ItemTooltip tooltip = currentActiveTooltip.GetComponent<ItemTooltip>();
                if (tooltip != null)
                {
                    tooltip.SetupTooltip(currentItemData.itemSO);
                }
            }
        }
    }

    /// <summary>
    /// [SRP] 툴팁 파괴 로직을 수행합니다. ItemPointerDetector.cs의 OnPointerExit에서 호출됩니다.
    /// </summary>
    public void HideTooltip()
    {
        // static 변수를 사용하여 툴팁을 파괴하고 참조를 해제합니다.
        if (currentActiveTooltip != null)
        {
            Destroy(currentActiveTooltip);
            currentActiveTooltip = null;
        }
    }

    /// <summary>
    /// [SRP] 좌클릭 시 버튼 패널을 숨기는 로직을 수행합니다. ItemPointerDetector.cs에서 호출됩니다.
    /// </summary>
    public void HandleLeftClick()
    {
        if (instantiatedButtonPanel != null)
        {
            Destroy(instantiatedButtonPanel);
            instantiatedButtonPanel = null; // 참조 해제
        }
    }

    /// <summary>
    /// [SRP] 우클릭 시 버튼 패널을 활성화하는 로직을 수행합니다. ItemPointerDetector.cs에서 호출됩니다.
    /// </summary>
    public void HandleRightClick()
    {
        // 실제 버튼 패널 생성 및 초기화 로직을 수행합니다.
        OnRightClick();
    }

    /// <summary>
    /// 아이템 우클릭 시 버튼 패널을 활성화하고 위치를 설정합니다.
    /// (HandleRightClick 내부에서만 호출되는 내부 구현 메서드입니다.)
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
            ButtonPanel buttonPanel = instantiatedButtonPanel.GetComponent<ButtonPanel>();
            if (buttonPanel != null)
            {
                // ButtonPanel의 Initialize 메서드를 호출하여 버튼 기능을 설정합니다.
                buttonPanel.Initialize(currentItemData.itemSO, currentItemData.stackCount);
            }
        }
    }
}