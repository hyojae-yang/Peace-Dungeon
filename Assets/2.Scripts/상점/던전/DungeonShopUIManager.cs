using UnityEngine;
using UnityEngine.UI;
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
            dungeonCoinText.text = currentCoins.ToString();
        }
    }
}