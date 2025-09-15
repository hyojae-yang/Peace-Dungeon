using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 아이템 우클릭 시 나타나는 버튼 패널을 관리하는 스크립트입니다.
/// 아이템 타입에 따라 적절한 버튼을 활성화/비활성화하고 기능을 할당합니다.
/// </summary>
public class ButtonPanel : MonoBehaviour
{
    // === 인스펙터에 할당할 참조 변수 ===
    [Tooltip("장비 아이템 장착 버튼입니다.")]
    [SerializeField] private Button equipButton;

    [Tooltip("소모품 아이템 사용 버튼입니다.")]
    [SerializeField] private Button useButton;

    [Tooltip("아이템 버리기 버튼입니다.")]
    [SerializeField] private Button discardButton;

    // === 내부 데이터 변수 ===
    private BaseItemSO currentItem;
    private int currentItemCount;

    /// <summary>
    /// ItemSlotUI에서 호출되어 현재 아이템 정보를 받고, 버튼을 초기화합니다.
    /// </summary>
    /// <param name="item">현재 슬롯에 있는 아이템 데이터</param>
    public void Initialize(BaseItemSO item, int count)
    {
        currentItem = item;
        currentItemCount = count;
        ShowButtonsByItemType();
        AddButtonListeners();
    }

    /// <summary>
    /// 버튼에 클릭 이벤트를 연결합니다.
    /// Initialize 메서드에서 한 번만 호출됩니다.
    /// </summary>
    private void AddButtonListeners()
    {
        // 중복 할당 방지를 위해 리스너를 먼저 제거합니다.
        equipButton.onClick.RemoveAllListeners();
        useButton.onClick.RemoveAllListeners();
        discardButton.onClick.RemoveAllListeners();

        // 각 버튼에 맞는 기능을 연결합니다.
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        useButton.onClick.AddListener(OnUseButtonClicked);
        discardButton.onClick.AddListener(OnDiscardButtonClicked);
    }

    /// <summary>
    /// 아이템 타입에 따라 버튼들의 활성화 상태를 설정합니다.
    /// </summary>
    private void ShowButtonsByItemType()
    {
        // 모든 버튼을 일단 비활성화합니다.
        equipButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);

        // 아이템 타입에 따라 필요한 버튼만 활성화합니다.
        if (currentItem != null)
        {
            switch (currentItem.itemType)
            {
                case ItemType.Equipment:
                    equipButton.gameObject.SetActive(true);
                    discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Consumable:
                    useButton.gameObject.SetActive(true);
                    discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Material:
                case ItemType.Quest:
                    discardButton.gameObject.SetActive(true);
                    break;
                default:
                    // 특수 아이템 등은 아무 버튼도 표시하지 않습니다.
                    break;
            }
        }
    }

    // === 버튼 클릭 시 호출될 메서드 (로직 분담) ===

    /// <summary>
    /// '장착' 버튼 클릭 시 호출됩니다.
    /// PlayerEquipmentManager에게 장착을 요청합니다.
    /// </summary>
    public void OnEquipButtonClicked()
    {
        // 장비 아이템인지 확인하고 캐스팅합니다.
        EquipmentItemSO equipItem = currentItem as EquipmentItemSO;
        if (equipItem != null)
        {
            // PlayerEquipmentManager에게 장착 요청만 합니다.
            // 인벤토리에서 아이템을 제거하는 로직은 PlayerEquipmentManager가 처리합니다.
            PlayerEquipmentManager.Instance.EquipItem(equipItem);

            // 버튼 클릭 후 패널을 파괴합니다.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// '사용' 버튼 클릭 시 호출됩니다.
    /// InventoryManager에 사용을 요청합니다.
    /// </summary>
    public void OnUseButtonClicked()
    {
        // 소모품 아이템인지 확인하고 캐스팅합니다.
        ConsumableItemSO consumeItem = currentItem as ConsumableItemSO;
        if (consumeItem != null)
        {
            InventoryManager.Instance.UseItem(consumeItem);
            // 버튼 클릭 후 패널을 파괴합니다.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// '버리기' 버튼 클릭 시 호출됩니다.
    /// InventoryUIController에 확인창 표시를 요청합니다.
    /// </summary>
    public void OnDiscardButtonClicked()
    {
        if (currentItem != null && currentItemCount > 0)
        {
            // InventoryUIController에 확인창을 띄우도록 요청합니다.
            // 실제 버리기 로직은 ConfirmPanel에서 처리됩니다.
            InventoryUIController.Instance.ShowDiscardConfirmPanel(currentItem, currentItemCount);
            // 버튼 클릭 후 패널을 파괴합니다.
            Destroy(gameObject);
        }
    }
}