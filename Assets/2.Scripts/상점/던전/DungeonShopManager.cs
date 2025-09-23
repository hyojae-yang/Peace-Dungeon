using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� ������ ���� ������ �����ϴ� �Ŵ���.
/// ��ȭ�� �κ��丮�� �����Ͽ� ���Ÿ� ó���մϴ�.
/// </summary>
public class DungeonShopManager : MonoBehaviour
{
    // �ν����Ϳ��� �Ҵ��� ������ �����ͺ��̽�
    [SerializeField] private ItemDatabase itemDatabase;
    // �ν����Ϳ��� �Ҵ��� ���� �κ��丮 �Ŵ���
    [SerializeField] private DungeonInventoryManager dungeonInventoryManager;
    // ���� ���� ��ȭ�� �����ϴ� �Ŵ���
    [SerializeField] private DungeonCoinCurrency dungeonCoinCurrency;

    // ���� �������� �Ǹ��� ������ ID ���
    [SerializeField] private List<string> shopItemIDs;

    /// <summary>
    /// ���� ���� �������� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="itemID">������ �������� ���� ID</param>
    public void BuyDungeonFragment(string itemID)
    {
        DungeonItemData itemData = itemDatabase.GetItem(itemID);
        if (itemData == null)
        {
            Debug.LogError($"Error: ID '{itemID}'�� �ش��ϴ� ������ �����͸� ã�� �� �����ϴ�.");
            return;
        }

        // �������� ��ȭ Ÿ���� ���� ������ �´��� Ȯ�� (����� �ڵ�)
        if (itemData.currencyType != CurrencyType.DungeonCoin)
        {
            return;
        }

        // ���� ���� �Ŵ����� ���� ���� ���� ���� Ȯ�� �� ���� ����
        if (dungeonCoinCurrency.SubtractCoins(itemData.price))
        {
            // ���� ���� �� �κ��丮�� ������ �߰�
            dungeonInventoryManager.AddPlayerItem(itemID);

            // UI�� �����Ͽ� ���� ���� ������ ��� �ݿ��մϴ�.
            DungeonShopUIManager uiManager = FindFirstObjectByType<DungeonShopUIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateDungeonCoinText();
            }
        }
        else
        {
            Debug.Log("���� ����: ���� ������ �����մϴ�.");
        }
    }

    /// <summary>
    /// ���� UI�� ǥ���� ������ ����� DungeonItemData Ÿ������ ��ȯ�մϴ�.
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
    /// ���� ���� ���� ������ ��ȯ�մϴ�.
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