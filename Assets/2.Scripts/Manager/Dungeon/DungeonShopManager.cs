using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 상점의 구매 로직을 관리하는 매니저.
/// 재화와 인벤토리를 연결하여 구매를 처리합니다.
/// </summary>
public class DungeonShopManager : MonoBehaviour
{
    /// <summary>
    /// 위험도 초기화 기능의 기본 가격 (던전 코인)
    /// </summary>
    private const int BASE_RESET_COST = 5;
    /// <summary>
    /// 위험도 레벨 1당 추가되는 가격 (던전 코인)
    /// </summary>
    private const int COST_INCREASE_PER_LEVEL = 1;
    // 인스펙터에서 할당할 아이템 데이터베이스
    [SerializeField] private ItemDatabase itemDatabase;
    // 인스펙터에서 할당할 던전 인벤토리 매니저
    [SerializeField] private DungeonInventoryManager dungeonInventoryManager;
    // 던전 코인 재화를 관리하는 매니저
    [SerializeField] private DungeonCoinCurrency dungeonCoinCurrency;
    [SerializeField] private DungeonShopUIManager dungeonShopUIManager;

    // 던전 상점에서 판매할 아이템 ID 목록
    [SerializeField] private List<string> shopItemIDs;

    private void Awake()
    {
        dungeonShopUIManager = FindFirstObjectByType<DungeonShopUIManager>();
    }
    /// <summary>
    /// 던전 조각 아이템을 구매하는 메서드입니다.
    /// </summary>
    /// <param name="itemID">구매할 아이템의 고유 ID</param>
    public void BuyDungeonFragment(string itemID)
    {
        DungeonItemData itemData = itemDatabase.GetItem(itemID);
        if (itemData == null)
        {
            Debug.LogError($"Error: ID '{itemID}'에 해당하는 아이템 데이터를 찾을 수 없습니다.");
            return;
        }

        // 아이템의 재화 타입이 던전 코인이 맞는지 확인 (방어적 코드)
        if (itemData.currencyType != CurrencyType.DungeonCoin)
        {
            return;
        }

        // 던전 코인 매니저를 통해 구매 가능 여부 확인 및 코인 차감
        if (dungeonCoinCurrency.SubtractCoins(itemData.price))
        {
            // 구매 성공 시 인벤토리에 아이템 추가
            dungeonInventoryManager.AddPlayerItem(itemID);

            // UI를 갱신하여 현재 코인 개수를 즉시 반영합니다.
            if (dungeonShopUIManager != null)
            {
                dungeonShopUIManager.UpdateDungeonCoinText();
            }
        }
        else
        {
            Debug.Log("구매 실패: 던전 코인이 부족합니다.");
        }
    }
    /// <summary>
    /// 던전 위험도 시스템을 초기화하는 기능을 던전 코인을 지불하고 구매합니다.
    /// </summary>
    /// <returns>구매 및 초기화 성공 여부 (true/false)</returns>
    public bool BuyLevelReset()
    {
        // 1. DungeonRiskManager 유효성 검사 및 현재 위험도 레벨 확인
        if (DungeonRiskManager.Instance == null)
        {
            Debug.LogError("DungeonRiskManager를 찾을 수 없습니다. 초기화 불가.");
            return false;
        }

        int currentLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();

        // 2. 가격 계산: 가격 = 기본 가격 (10) + (현재 레벨 * 1)
        int requiredCost = BASE_RESET_COST + (currentLevel * COST_INCREASE_PER_LEVEL);

        // 3. 던전 코인 차감 시도
        if (dungeonCoinCurrency != null && dungeonCoinCurrency.SubtractCoins(requiredCost))
        {
            // 4. 구매 성공: 위험도 시스템 초기화 진행
            DungeonRiskManager.Instance.ResetRiskSystem();

            // 5. UI 갱신 (선택 사항: 코인 개수 및 상점 UI)
            if (dungeonShopUIManager != null)
            {
                // 차감된 코인 개수를 UI에 반영
                dungeonShopUIManager.UpdateDungeonCoinText();
            }

           // Debug.Log($"[ShopManager] 위험도 초기화 기능 구매 성공. 레벨 {currentLevel} 기준, 던전 코인 {requiredCost}개 차감.");
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 현재 위험도 레벨에 따른 초기화 기능의 던전 코인 가격을 계산하여 반환합니다.
    /// 이 메서드는 UI 갱신을 위해 DungeonShopUIManager에서 호출됩니다.
    /// </summary>
    /// <returns>초기화 기능 구매에 필요한 던전 코인 수량</returns>
    public int GetRiskResetPrice()
    {
        // 1. DungeonRiskManager 유효성 검사
        if (DungeonRiskManager.Instance == null)
        {
            Debug.LogError("DungeonRiskManager를 찾을 수 없습니다. 가격 계산 불가.");
            return -1; // 유효하지 않은 값 반환
        }

        int currentLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();

        // 2. 가격 계산 로직 (BuyLevelReset과 동일)
        // 가격 = 기본 가격 (10) + (현재 레벨 * 1)
        int requiredCost = BASE_RESET_COST + (currentLevel * COST_INCREASE_PER_LEVEL);

        return requiredCost;
    }
    /// <summary>
    /// 상점 UI에 표시할 아이템 목록을 DungeonItemData 타입으로 반환합니다.
    /// </summary>
    public List<DungeonItemData> GetShopItems()
    {
        List<DungeonItemData> shopItems = new List<DungeonItemData>();
        foreach (string id in shopItemIDs)
        {
            DungeonItemData item = itemDatabase.GetItem(id);
            if (item != null)
            {
                shopItems.Add(item);
            }
        }
        return shopItems;
    }

    /// <summary>
    /// 현재 던전 코인 개수를 반환합니다.
    /// </summary>
    public int GetDungeonCoinCount()
    {
        if (dungeonCoinCurrency != null)
        {
            return dungeonCoinCurrency.currentDungeonCoins;
        }
        return 0;
    }
}