using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "Dungeon/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<DungeonItemData> allItems = new List<DungeonItemData>();
    private Dictionary<string, DungeonItemData> itemDictionary = new Dictionary<string, DungeonItemData>();

    public void Init()
    {
        itemDictionary.Clear();
        foreach (var item in allItems)
        {
            if (!itemDictionary.ContainsKey(item.itemID))
            {
                itemDictionary.Add(item.itemID, item);
            }
            else
            {
                Debug.LogWarning($"Warning: Duplicate itemID '{item.itemID}' found in the database. Please check your data assets.");
            }
        }
    }

    public DungeonItemData GetItem(string id)
    {
        if (itemDictionary.TryGetValue(id, out DungeonItemData data))
        {
            return data;
        }
        return null;
    }

    // UI 프리팹을 통해 아이템 데이터를 찾는 메서드 (DungeonInventoryManager에서 사용)
    public DungeonItemData GetItemByPrefab(GameObject uiItemPrefab)
    {
        foreach (var itemData in allItems)
        {
            if (itemData.uiItemPrefab == uiItemPrefab)
            {
                return itemData;
            }
        }
        return null;
    }
}