using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용한다면 이 네임스페이스를 추가합니다.

public class DungeonShopUIItem : MonoBehaviour
{
    // === UI 요소 레퍼런스 ===
    [SerializeField] private Image itemImage;        // 아이템 이미지
    [SerializeField] private TextMeshProUGUI itemNameText; // 아이템 이름 텍스트
    [SerializeField] private TextMeshProUGUI itemPriceText; // 아이템 가격 텍스트
    [SerializeField] private TextMeshProUGUI itemDescriptionText; // 아이템 설명 텍스트
    [SerializeField] private Button buyButton;      // 구매 버튼

    // === 데이터 저장 변수 ===
    private DungeonItemData assignedItemData; // 이 슬롯에 할당된 아이템 데이터
    private DungeonShopManager shopManager;   // 구매 요청을 전달할 상점 매니저

    /// <summary>
    /// 상점 UI 슬롯을 아이템 데이터에 맞게 설정하는 메서드.
    /// </summary>
    /// <param name="itemData">이 슬롯에 표시할 던전 아이템 데이터.</param>
    /// <param name="manager">구매 요청을 전달할 던전 상점 매니저.</param>
    public void Setup(DungeonItemData itemData, DungeonShopManager manager)
    {
        // 1. 데이터 할당 및 매니저 연결
        assignedItemData = itemData;
        shopManager = manager;

        // 2. UI 요소에 데이터 표시
        itemImage.sprite = itemData.itemImage;
        itemNameText.text = itemData.itemName;
        itemPriceText.text = itemData.price.ToString(); // 가격을 문자열로 변환
        itemDescriptionText.text = itemData.description;

        // 3. 구매 버튼 리스너 연결
        buyButton.onClick.RemoveAllListeners(); // 기존 리스너 제거 (재사용 시 필요)
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출되는 메서드.
    /// </summary>
    private void OnBuyButtonClick()
    {
        if (assignedItemData != null && shopManager != null)
        {
            // 던전 상점 매니저에게 구매 요청을 전달
            shopManager.BuyDungeonFragment(assignedItemData.itemID);
        }
    }
}