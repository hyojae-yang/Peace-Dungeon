using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가
using System.Collections.Generic;
using System;
using System.Linq; // LINQ를 사용하기 위해 추가
using UnityEngine.UI; // Button, Image 등을 사용하기 위해 추가

/// <summary>
/// 상점 UI를 관리하는 싱글턴 클래스입니다.
/// 상점 UI의 활성화, 비활성화 및 데이터 표시를 담당합니다.
/// SOLID: 단일 책임 원칙 (UI 관리)
/// </summary>
public class ShopUIManager : MonoBehaviour
{
    // ShopUIManager의 싱글턴 인스턴스
    public static ShopUIManager Instance { get; private set; }

    [Header("Main UI Components")]
    [Tooltip("상점 UI 전체를 담고 있는 패널입니다.")]
    [SerializeField]
    private GameObject shopPanel;
    [Tooltip("상점 메인 UI 패널입니다. '구매', '판매' 버튼이 포함됩니다.")]
    [SerializeField]
    private GameObject mainShopPanel;
    [Tooltip("구매 아이템 목록을 표시할 패널입니다.")]
    [SerializeField]
    private GameObject buyPanel;
    [Tooltip("판매 아이템 목록을 표시할 패널입니다.")]
    [SerializeField]
    private GameObject sellPanel;
    [Tooltip("NPC의 이름을 표시할 텍스트 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI npcNameText;

    [Header("Item Containers")]
    [Tooltip("구매 아이템 목록을 배치할 Scroll View의 Content입니다.")]
    [SerializeField]
    private Transform buyItemsContainer;
    [Tooltip("판매 아이템 목록을 배치할 Scroll View의 Content입니다.")]
    [SerializeField]
    private Transform sellItemsContainer;
    [Tooltip("상점 아이템 UI 프리팹입니다. (ShopItemUI 스크립트가 부착되어 있어야 합니다)")]
    [SerializeField]
    private GameObject shopItemUIPrefab;

    // ====================================================================================
    // 🚨 새로 추가된 부분: 확인 창(Confirmation Panel) 관련 변수
    // ====================================================================================
    [Header("Confirmation Panel")]
    [Tooltip("아이템 구매/판매 확인 창 전체를 담는 패널입니다.")]
    [SerializeField]
    private GameObject confirmationPanel;
    [Tooltip("확인 창에 표시할 아이템 이름 및 메시지 텍스트입니다.")]
    [SerializeField]
    private TextMeshProUGUI confirmText;
    [Tooltip("수량을 입력받을 InputField입니다.")]
    [SerializeField]
    private TMP_InputField quantityInputField;
    [Tooltip("확인 버튼입니다.")]
    [SerializeField]
    private Button confirmButton;
    [Tooltip("취소 버튼입니다.")]
    [SerializeField]
    private Button cancelButton;

    private void Awake()
    {
        // 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 모든 상점 패널을 초기 상태에서 비활성화
        InitializePanels();
    }

    private void Start()
    {
        // 확인 창의 취소 버튼에 리스너를 추가합니다.
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CloseConfirmationPanel);
        }

        // 입력 필드에 값 변경 시 호출될 메서드 리스너를 추가합니다.
        if (quantityInputField != null)
        {
            quantityInputField.onValueChanged.AddListener(OnQuantityChanged);
        }
    }

    /// <summary>
    /// 모든 상점 UI 패널을 초기 상태로 되돌립니다.
    /// </summary>
    private void InitializePanels()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (mainShopPanel != null) mainShopPanel.SetActive(false);
        if (buyPanel != null) buyPanel.SetActive(false);
        if (sellPanel != null) sellPanel.SetActive(false);
        // 확인 창도 초기에는 비활성화 상태로 둡니다.
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }

    /// <summary>
    /// 상점 메인 UI를 활성화하고 NPC의 이름으로 초기화합니다.
    /// </summary>
    /// <param name="npcName">상점을 열어준 NPC의 이름입니다.</param>
    public void ShowShop(string npcName)
    {
        if (shopPanel == null || mainShopPanel == null)
        {
            Debug.LogWarning("Shop UI 패널이 할당되지 않았습니다.");
            return;
        }

        // NPC 이름을 UI에 표시합니다.
        if (npcNameText != null)
        {
            npcNameText.text = $"{npcName}의 잡화점";
        }

        // 메인 상점 UI를 활성화하고, 구매/판매 패널은 비활성화합니다.
        shopPanel.SetActive(true);
        mainShopPanel.SetActive(true);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        confirmationPanel.SetActive(false); // 혹시 모를 경우를 대비해 비활성화
    }

    /// <summary>
    /// 상점 UI 전체를 비활성화합니다.
    /// </summary>
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 구매 패널을 활성화하고 판매할 아이템 목록을 표시합니다.
    /// </summary>
    /// <param name="shopData">NPC가 판매하는 아이템 목록 데이터입니다.</param>
    public void OpenBuyPanel(ShopData shopData)
    {
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);

        // 기존 아이템 UI 제거
        ClearBuyItems();

        // 판매 아이템 목록 생성
        if (shopData != null && shopData.itemsToSell != null)
        {
            foreach (var item in shopData.itemsToSell)
            {
                var itemUIObject = Instantiate(shopItemUIPrefab, buyItemsContainer);
                var itemUI = itemUIObject.GetComponent<ShopItemUI>();

                if (itemUI != null)
                {
                    // 이제 확인 버튼을 열도록 리스너를 변경합니다.
                    itemUI.Setup(item, "구매", () => OpenBuyConfirmation(item));
                }
            }
        }
    }

    /// <summary>
    /// 판매 패널을 활성화하고 플레이어 인벤토리 아이템 목록을 표시합니다.
    /// </summary>
    public void OpenSellPanel()
    {
        sellPanel.SetActive(true);
        buyPanel.SetActive(false);

        // 기존 아이템 UI 제거
        ClearSellItems();

        // 플레이어 인벤토리 아이템 목록을 가져옵니다.
        var inventoryItems = PlayerCharacter.Instance.inventoryManager.GetInventoryItems();

        // LINQ의 Where 메서드와 is 연산자를 사용하여 판매 불가능한 아이템(퀘스트, 특수 아이템)을 제외합니다.
        var sellableItems = inventoryItems.Where(
            itemData => !(itemData.itemSO is QuestItemSO) && !(itemData.itemSO is SpecialItemSO)
        );

        foreach (var itemData in sellableItems)
        {
            var itemUIObject = Instantiate(shopItemUIPrefab, sellItemsContainer);
            var itemUI = itemUIObject.GetComponent<ShopItemUI>();

            if (itemUI != null)
            {
                // 수정된 부분: 이제 확인 버튼을 열도록 리스너를 변경합니다.
                itemUI.Setup(itemData.itemSO, "판매", () => OpenSellConfirmation(itemData));
            }
        }
    }

    // ====================================================================================
    // 새로 추가된 메서드: 확인 창 열기/닫기 및 로직 처리
    // ====================================================================================

    /// <summary>
    /// 구매 확인 창을 활성화하고 데이터를 설정합니다.
    /// </summary>
    /// <param name="itemToBuy">구매할 아이템의 Scriptable Object</param>
    public void OpenBuyConfirmation(BaseItemSO itemToBuy)
    {
        confirmationPanel.SetActive(true);

        // 🚨 추가된 부분: confirmText를 업데이트합니다.
        if (confirmText != null)
        {
            confirmText.text = $"{itemToBuy.itemName}을(를) 몇 개 구매하시겠습니까?";
        }

        confirmButton.onClick.RemoveAllListeners();
        quantityInputField.text = "1";

        confirmButton.onClick.AddListener(() => ShopManager.Instance.BuyItem(itemToBuy, GetInputValue()));

        quantityInputField.onEndEdit.RemoveAllListeners();
        quantityInputField.onEndEdit.AddListener(value => ValidateBuyQuantity(itemToBuy));
    }

    /// <summary>
    /// 판매 확인 창을 활성화하고 데이터를 설정합니다.
    /// </summary>
    /// <param name="itemToSell">판매할 아이템의 데이터</param>
    public void OpenSellConfirmation(ItemData itemToSell)
    {
        confirmationPanel.SetActive(true);

        // 🚨 추가된 부분: confirmText를 업데이트합니다.
        if (confirmText != null)
        {
            confirmText.text = $"{itemToSell.itemSO.itemName}을(를) 몇 개 판매하시겠습니까?";
        }

        confirmButton.onClick.RemoveAllListeners();
        quantityInputField.text = "1";

        confirmButton.onClick.AddListener(() => ShopManager.Instance.SellItem(itemToSell, GetInputValue()));

        quantityInputField.onEndEdit.RemoveAllListeners();
        quantityInputField.onEndEdit.AddListener(value => ValidateSellQuantity(itemToSell));
    }

    /// <summary>
    /// 확인 창을 닫습니다.
    /// </summary>
    public void CloseConfirmationPanel()
    {
        confirmationPanel.SetActive(false);
    }

    /// <summary>
    /// 입력 필드의 값을 정수로 반환합니다.
    /// </summary>
    private int GetInputValue()
    {
        if (int.TryParse(quantityInputField.text, out int quantity))
        {
            return quantity;
        }
        return 0;
    }

    /// <summary>
    /// 구매 시 수량의 유효성을 검사하고 조정합니다.
    /// </summary>
    private void ValidateBuyQuantity(BaseItemSO item)
    {
        int maxQuantity = Mathf.FloorToInt(PlayerCharacter.Instance.playerStats.gold / item.itemPrice);
        int currentQuantity = GetInputValue();

        if (currentQuantity > maxQuantity)
        {
            quantityInputField.text = maxQuantity.ToString();
        }
    }

    /// <summary>
    /// 판매 시 수량의 유효성을 검사하고 조정합니다.
    /// </summary>
    private void ValidateSellQuantity(ItemData itemData)
    {
        int currentQuantity = GetInputValue();

        if (currentQuantity > itemData.stackCount)
        {
            quantityInputField.text = itemData.stackCount.ToString();
        }
    }

    /// <summary>
    /// 입력 필드 값이 변경될 때마다 호출되는 메서드입니다.
    /// </summary>
    private void OnQuantityChanged(string value)
    {
        if (!int.TryParse(value, out int parsedValue))
        {
            quantityInputField.text = "0";
        }
    }

    /// <summary>
    /// 구매 아이템 컨테이너의 모든 자식 아이템 UI를 삭제합니다.
    /// </summary>
    private void ClearBuyItems()
    {
        if (buyItemsContainer == null) return;
        for (int i = buyItemsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buyItemsContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 판매 아이템 컨테이너의 모든 자식 아이템 UI를 삭제합니다.
    /// </summary>
    private void ClearSellItems()
    {
        if (sellItemsContainer == null) return;
        for (int i = sellItemsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(sellItemsContainer.GetChild(i).gameObject);
        }
    }
}