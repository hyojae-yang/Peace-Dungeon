using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 상점의 구매 로직을 관리하는 매니저.
/// 재화와 인벤토리를 연결하여 구매를 처리합니다.
/// </summary>
public class DungeonShopManager : MonoBehaviour
{
    // 인스펙터에서 할당할 아이템 데이터베이스
    [SerializeField] private ItemDatabase itemDatabase;
    // 인스펙터에서 할당할 던전 인벤토리 매니저
    [SerializeField] private DungeonInventoryManager dungeonInventoryManager;
    // 던전 코인 재화를 관리하는 매니저
    [SerializeField] private DungeonCoinCurrency dungeonCoinCurrency;

    // 던전 상점에서 판매할 아이템 ID 목록
    [SerializeField] private List<string> shopItemIDs;

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
            DungeonShopUIManager uiManager = FindFirstObjectByType<DungeonShopUIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateDungeonCoinText();
            }
        }
        else
        {
            Debug.Log("구매 실패: 던전 코인이 부족합니다.");
        }
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