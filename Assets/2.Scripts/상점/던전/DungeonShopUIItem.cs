using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro�� ����Ѵٸ� �� ���ӽ����̽��� �߰��մϴ�.

public class DungeonShopUIItem : MonoBehaviour
{
    // === UI ��� ���۷��� ===
    [SerializeField] private Image itemImage;        // ������ �̹���
    [SerializeField] private TextMeshProUGUI itemNameText; // ������ �̸� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI itemPriceText; // ������ ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI itemDescriptionText; // ������ ���� �ؽ�Ʈ
    [SerializeField] private Button buyButton;      // ���� ��ư

    // === ������ ���� ���� ===
    private DungeonItemData assignedItemData; // �� ���Կ� �Ҵ�� ������ ������
    private DungeonShopManager shopManager;   // ���� ��û�� ������ ���� �Ŵ���

    /// <summary>
    /// ���� UI ������ ������ �����Ϳ� �°� �����ϴ� �޼���.
    /// </summary>
    /// <param name="itemData">�� ���Կ� ǥ���� ���� ������ ������.</param>
    /// <param name="manager">���� ��û�� ������ ���� ���� �Ŵ���.</param>
    public void Setup(DungeonItemData itemData, DungeonShopManager manager)
    {
        // 1. ������ �Ҵ� �� �Ŵ��� ����
        assignedItemData = itemData;
        shopManager = manager;

        // 2. UI ��ҿ� ������ ǥ��
        itemImage.sprite = itemData.itemImage;
        itemNameText.text = itemData.itemName;
        itemPriceText.text = itemData.price.ToString(); // ������ ���ڿ��� ��ȯ
        itemDescriptionText.text = itemData.description;

        // 3. ���� ��ư ������ ����
        buyButton.onClick.RemoveAllListeners(); // ���� ������ ���� (���� �� �ʿ�)
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    /// <summary>
    /// ���� ��ư Ŭ�� �� ȣ��Ǵ� �޼���.
    /// </summary>
    private void OnBuyButtonClick()
    {
        if (assignedItemData != null && shopManager != null)
        {
            // ���� ���� �Ŵ������� ���� ��û�� ����
            shopManager.BuyDungeonFragment(assignedItemData.itemID);
        }
    }
}