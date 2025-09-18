using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action을 사용하기 위해 추가

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
    private PlayerCharacter playerCharacter; // PlayerCharacter 참조 추가

    /// <summary>
    /// ItemSlotUI에서 호출되어 현재 아이템 정보를 받고, 버튼을 초기화합니다.
    /// </summary>
    /// <param name="item">현재 슬롯에 있는 아이템 데이터</param>
    /// <param name="count">현재 슬롯에 있는 아이템 개수</param>
    public void Initialize(BaseItemSO item, int count)
    {
        currentItem = item;
        currentItemCount = count;

        // PlayerCharacter 인스턴스를 가져옵니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("ButtonPanel: PlayerCharacter 인스턴스를 찾을 수 없습니다.");
        }

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
        // EquipButton은 EquipmentItemSO 타입일 때만 작동하도록 안전 장치 추가
        if (equipButton != null && currentItem != null && currentItem.itemType == ItemType.Equipment)
        {
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        // UseButton은 ConsumableItemSO 타입일 때만 작동하도록 안전 장치 추가
        if (useButton != null && currentItem != null && currentItem.itemType == ItemType.Consumable)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
        }

        // DiscardButton은 항상 작동하지만, currentItem이 null이 아닐 때만 유효합니다.
        if (discardButton != null && currentItem != null)
        {
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }
    }

    /// <summary>
    /// 아이템 타입에 따라 버튼들의 활성화 상태를 설정합니다.
    /// </summary>
    private void ShowButtonsByItemType()
    {
        // 모든 버튼을 일단 비활성화합니다.
        if (equipButton != null) equipButton.gameObject.SetActive(false);
        if (useButton != null) useButton.gameObject.SetActive(false);
        if (discardButton != null) discardButton.gameObject.SetActive(false);

        // 아이템 타입에 따라 필요한 버튼만 활성화합니다.
        if (currentItem != null)
        {
            switch (currentItem.itemType)
            {
                case ItemType.Equipment:
                    if (equipButton != null) equipButton.gameObject.SetActive(true);
                    if (discardButton != null) discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Consumable:
                    if (useButton != null) useButton.gameObject.SetActive(true);
                    if (discardButton != null) discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Material:
                case ItemType.Quest:
                    if (discardButton != null) discardButton.gameObject.SetActive(true);
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
    /// PlayerCharacter를 통해 PlayerEquipmentManager에게 장착을 요청합니다.
    /// </summary>
    public void OnEquipButtonClicked()
    {
        // 장비 아이템인지 확인하고 캐스팅합니다.
        EquipmentItemSO equipItem = currentItem as EquipmentItemSO;
        if (equipItem != null && playerCharacter != null && playerCharacter.playerEquipmentManager != null)
        {
            // PlayerEquipmentManager에게 장착 요청을 합니다.
            // EquipItem 메서드 내부에서 아이템 수량 감소 및 인벤토리 업데이트 로직을 처리합니다.
            // 만약 스택 가능한 장비라면 해당 로직은 별도 처리 필요.
            playerCharacter.playerEquipmentManager.EquipItem(equipItem);

            // 버튼 클릭 후 패널을 파괴합니다.
            Destroy(gameObject);
        }
        else if (equipItem == null)
        {
            Debug.LogWarning("장착하려는 아이템이 EquipmentItemSO 타입이 아닙니다.");
        }
        else if (playerCharacter == null || playerCharacter.playerEquipmentManager == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스 또는 PlayerEquipmentManager 컴포넌트가 없어 장착할 수 없습니다.");
        }
    }

    /// <summary>
    /// '사용' 버튼 클릭 시 호출됩니다.
    /// PlayerCharacter를 통해 InventoryManager에 사용을 요청합니다.
    /// </summary>
    public void OnUseButtonClicked()
    {
        // 소모품 아이템인지 확인하고 캐스팅합니다.
        ConsumableItemSO consumeItem = currentItem as ConsumableItemSO;
        if (consumeItem != null && playerCharacter != null && playerCharacter.inventoryManager != null)
        {
            // InventoryManager의 UseItem 메서드를 호출하며 PlayerCharacter를 전달합니다.
            playerCharacter.inventoryManager.UseItem(consumeItem);
            // 버튼 클릭 후 패널을 파괴합니다.
            Destroy(gameObject);
        }
        else if (consumeItem == null)
        {
            Debug.LogWarning("사용하려는 아이템이 ConsumableItemSO 타입이 아닙니다.");
        }
        else if (playerCharacter == null || playerCharacter.inventoryManager == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스 또는 InventoryManager 컴포넌트가 없어 아이템을 사용할 수 없습니다.");
        }
    }

    /// <summary>
    /// '버리기' 버튼 클릭 시 호출됩니다.
    /// InventoryUIController에 확인창 표시를 요청합니다.
    /// </summary>
    public void OnDiscardButtonClicked()
    {
        if (currentItem != null && currentItemCount > 0 && InventoryUIController.Instance != null)
        {
            // InventoryUIController는 싱글톤이므로 직접 접근합니다.
            // 실제 버리기 로직은 ConfirmPanel에서 처리됩니다.
            InventoryUIController.Instance.ShowDiscardConfirmPanel(currentItem, currentItemCount);
            // 버튼 클릭 후 패널을 파괴합니다.
            Destroy(gameObject);
        }
        else if (currentItem == null || currentItemCount <= 0)
        {
            Debug.LogWarning("버릴 아이템이 없거나 수량이 유효하지 않습니다.");
        }
        else if (InventoryUIController.Instance == null)
        {
            Debug.LogError("InventoryUIController 인스턴스가 없어 아이템을 버릴 수 없습니다.");
        }
    }
}