using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// ���� ���� UI�� �������� ǥ�ø� �����ϴ� �Ŵ���.
/// ������ ����� �������� �����ϰ�, ���� ���� �ؽ�Ʈ�� �����մϴ�.
/// </summary>
public class DungeonShopUIManager : MonoBehaviour
{
    // ������ ������ ��ġ�� ��ũ�Ѻ��� Content Ʈ������
    [SerializeField] private Transform contentParent;

    // �������� ������ ������ ���� ������ (DungeonShopUIItem ��ũ��Ʈ�� ������)
    [SerializeField] private GameObject shopUIItemPrefab;

    // ���� ���� ������ ǥ���� �ؽ�Ʈ UI
    [SerializeField] private TextMeshProUGUI dungeonCoinText;

    // ���� ���� ������ ����ϴ� �Ŵ���
    private DungeonShopManager dungeonShopManager;

    // ���� ������ ǥ�õ� ������ ���Ե��� ����Ʈ (���� �� ������ ����)
    private List<GameObject> activeItemSlots = new List<GameObject>();

    private void Awake()
    {
        // ���� ���� �Ŵ��� ���۷��� �ʱ�ȭ
        dungeonShopManager = FindFirstObjectByType<DungeonShopManager>();

        if (dungeonShopManager == null)
        {
            Debug.LogError("DungeonShopManager�� ���� �������� �ʽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ܺ� ��ũ��Ʈ���� ȣ��Ǿ� ���� UI�� �����ϴ� �޼���.
    /// �� �޼���� ���� UI �г��� Ȱ��ȭ�� ���Ŀ� ȣ��Ǿ�� �մϴ�.
    /// </summary>
    public void InitializeShopUI()
    {
        // ���� ���Ե��� ���� �����մϴ�.
        ClearShopUI();

        // ���� �ؽ�Ʈ UI�� �����մϴ�.
        UpdateDungeonCoinText();

        List<DungeonItemData> shopItems = dungeonShopManager.GetShopItems();
        if (shopItems.Count == 0)
        {
            Debug.Log("������ �Ǹ��� �������� �����ϴ�.");
            return;
        }

        foreach (var itemData in shopItems)
        {
            // ������ ���� ������ �ν��Ͻ�ȭ
            GameObject newItemSlot = Instantiate(shopUIItemPrefab, contentParent);
            activeItemSlots.Add(newItemSlot);

            // DungeonShopUIItem ��ũ��Ʈ ��������
            DungeonShopUIItem uiItem = newItemSlot.GetComponent<DungeonShopUIItem>();
            if (uiItem != null)
            {
                // UIItem ��ũ��Ʈ�� Setup �޼��带 ȣ���Ͽ� ������ ����
                uiItem.Setup(itemData, dungeonShopManager);
            }
        }
    }

    /// <summary>
    /// ���� UI�� ������ ��� ������ ������ �ı��մϴ�.
    /// �� �޼���� UI�� ��Ȱ��ȭ�� �� �ܺο��� ȣ��Ǿ�� �մϴ�.
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
    /// ���� ���� �ؽ�Ʈ UI�� �����ϴ� ���� �޼���.
    /// ���� UI�� ���� ���� �������� ������ �� ȣ��˴ϴ�.
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