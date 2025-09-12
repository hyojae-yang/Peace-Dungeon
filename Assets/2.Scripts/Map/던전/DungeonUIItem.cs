using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonUIItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI itemNameText;

    private DungeonItemData itemData;
    public int uniqueID; // <-- 이 부분이 새로 추가되었습니다.

    public void Setup(DungeonItemData data)
    {
        itemData = data;

        if (itemImage != null && data.itemImage != null)
        {
            itemImage.sprite = data.itemImage;
        }
        if (backgroundImage != null)
        {
            backgroundImage.color = data.backgroundColor;
        }
        if (itemNameText != null)
        {
            itemNameText.text = data.itemName;
        }
    }

    public string GetItemID()
    {
        if (itemData != null)
        {
            return itemData.itemID;
        }
        return string.Empty;
    }
}