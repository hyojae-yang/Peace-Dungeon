using UnityEngine;
using UnityEngine.UI; // Button 사용을 위해 추가
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 던전 상점 UI의 전반적인 표시를 관리하는 매니저.
/// 아이템 목록을 동적으로 생성하고, 던전 코인 텍스트를 갱신합니다.
/// </summary>
public class DungeonShopUIManager : MonoBehaviour
{
    // 아이템 슬롯이 배치될 스크롤뷰의 Content 트랜스폼
    [SerializeField] private Transform contentParent;

    // 동적으로 생성할 아이템 슬롯 프리팹 (DungeonShopUIItem 스크립트가 부착된)
    [SerializeField] private GameObject shopUIItemPrefab;

    // 던전 코인 개수를 표시할 텍스트 UI
    [SerializeField] private TextMeshProUGUI dungeonCoinText;

    // 위험도 초기화 버튼 UI 요소
    [Header("위험도 초기화 UI")]
    [Tooltip("위험도 초기화 버튼")]
    [SerializeField] private Button riskResetButton;

    [Tooltip("위험도 초기화 가격을 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI riskResetPriceText;

    // 던전 상점 로직을 담당하는 매니저
    private DungeonShopManager dungeonShopManager;

    // 현재 상점에 표시된 아이템 슬롯들의 리스트 (재사용 및 정리를 위함)
    private List<GameObject> activeItemSlots = new List<GameObject>();

    private void Awake()
    {
        // 던전 상점 매니저 레퍼런스 초기화
        dungeonShopManager = FindFirstObjectByType<DungeonShopManager>();

        if (dungeonShopManager == null)
        {
            Debug.LogError("DungeonShopManager가 씬에 존재하지 않습니다.");
            return;
        }

        // 초기화 버튼 클릭 이벤트 리스너 등록
        if (riskResetButton != null)
        {
            // 버튼 클릭 시 OnRiskResetButtonClicked 메서드 호출
            riskResetButton.onClick.AddListener(OnRiskResetButtonClicked);
        }
        else
        {
            Debug.LogWarning("riskResetButton이 할당되지 않았습니다. UI 기능을 사용할 수 없습니다.");
        }
    }

    /// <summary>
    /// 외부 스크립트에서 호출되어 상점 UI를 갱신하는 메서드.
    /// 이 메서드는 상점 UI 패널이 활성화된 직후에 호출되어야 합니다.
    /// </summary>
    public void InitializeShopUI()
    {
        // 기존 슬롯들을 먼저 정리합니다.
        ClearShopUI();

        // 코인 텍스트 UI를 갱신합니다.
        UpdateDungeonCoinText();

        // 위험도 초기화 UI 갱신
        UpdateRiskResetUI();

        List<DungeonItemData> shopItems = dungeonShopManager.GetShopItems();
        if (shopItems.Count == 0)
        {
            Debug.Log("상점에 판매할 아이템이 없습니다.");
            return;
        }

        foreach (var itemData in shopItems)
        {
            // 아이템 슬롯 프리팹 인스턴스화
            GameObject newItemSlot = Instantiate(shopUIItemPrefab, contentParent);
            activeItemSlots.Add(newItemSlot);

            // DungeonShopUIItem 스크립트 가져오기
            DungeonShopUIItem uiItem = newItemSlot.GetComponent<DungeonShopUIItem>();
            if (uiItem != null)
            {
                // UIItem 스크립트의 Setup 메서드를 호출하여 데이터 주입
                uiItem.Setup(itemData, dungeonShopManager);
            }
        }
    }

    /// <summary>
    /// 위험도 초기화 버튼이 클릭되었을 때 호출되는 메서드입니다.
    /// DungeonShopManager에 구매를 요청합니다.
    /// </summary>
    private void OnRiskResetButtonClicked()
    {
        if (dungeonShopManager != null)
        {
            // 구매 요청 및 초기화 진행
            // BuyLevelReset 내부에서 던전 코인 차감 및 위험도 초기화가 모두 처리됩니다.
            bool success = dungeonShopManager.BuyLevelReset();

            if (success)
            {
                // 초기화 성공 시, 가격 텍스트 및 버튼 상태를 갱신합니다. (레벨 0 기준으로 가격이 바뀜)
                // UpdateDungeonCoinText 내부에서 UpdateRiskResetUI()를 호출하므로 별도 호출 불필요
                //Debug.Log("위험도 초기화 구매 성공! 시스템 리셋됨.");
            }
            else
            {
                // 구매 실패 (코인 부족 등)
                //Debug.Log("위험도 초기화 구매 실패. 코인이 부족하거나 시스템 오류.");
            }
        }
    }

    /// <summary>
    /// DungeonShopManager로부터 가격을 조회하여 UI에 표시하고, 구매 가능 여부에 따라 버튼을 활성화/비활성화합니다.
    /// 이 메서드는 DungeonShopManager의 GetRiskResetPrice()에 의존합니다.
    /// </summary>
    public void UpdateRiskResetUI()
    {
        // UI 요소 및 매니저 유효성 검사
        if (riskResetPriceText == null || riskResetButton == null || dungeonShopManager == null)
        {
            // 경고 로그는 Awake에서 했으므로, 여기서는 조용히 종료합니다.
            return;
        }

        // 1. DungeonShopManager를 통해 필요한 코인 가격을 가져옵니다. (책임 분리)
        int requiredCost = dungeonShopManager.GetRiskResetPrice();

        // 2. 가격 유효성 검사 (DungeonRiskManager 오류 시 -1 반환)
        if (requiredCost < 0)
        {
            riskResetPriceText.text = "Error";
            riskResetButton.interactable = false;
            return;
        }

        // 3. UI 텍스트 업데이트
        riskResetPriceText.text = requiredCost.ToString();
    }


    /// <summary>
    /// 상점 UI에 생성된 모든 아이템 슬롯을 파괴합니다.
    /// 이 메서드는 UI가 비활성화될 때 외부에서 호출되어야 합니다.
    /// </summary>
    public void ClearShopUI()
    {
        foreach (GameObject slot in activeItemSlots)
        {
            Destroy(slot);
        }
        activeItemSlots.Clear();
    }

    /// <summary>
    /// 던전 코인 텍스트 UI를 갱신하는 전용 메서드.
    /// 상점 UI가 열릴 때와 아이템을 구매할 때 호출됩니다.
    /// </summary>
    public void UpdateDungeonCoinText()
    {
        if (dungeonCoinText != null)
        {
            int currentCoins = dungeonShopManager.GetDungeonCoinCount();
            dungeonCoinText.text = $"{currentCoins.ToString()}개";

            // 코인 개수가 변경되면 초기화 버튼 상태도 갱신
            UpdateRiskResetUI();
        }
    }
}