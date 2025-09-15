// ItemManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq를 사용하기 위해 필요합니다.

/// <summary>
/// 모든 아이템 데이터를 관리하는 중앙 데이터베이스 스크립트입니다.
/// 싱글톤 패턴으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// </summary>
public class ItemManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    private static ItemManager _instance;
    public static ItemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("ItemManagerSingleton");
                    _instance = singletonObject.AddComponent<ItemManager>();
                    Debug.Log("새로운 'ItemManagerSingleton' 게임 오브젝트를 생성했습니다.");
                }
            }
            return _instance;
        }
    }

    // 아이템 ID를 키로 하여 BaseItemSO를 저장하는 딕셔너리입니다.
    private Dictionary<int, BaseItemSO> _itemDictionary = new Dictionary<int, BaseItemSO>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject); // 필요에 따라 주석 해제
        }

        LoadAllItems();
    }

    /// <summary>
    /// Resources 폴더에 있는 모든 아이템 스크립터블 오브젝트를 로드합니다.
    /// </summary>
    private void LoadAllItems()
    {
        // "Items" 폴더에 있는 모든 BaseItemSO를 로드합니다.
        BaseItemSO[] allItems = Resources.LoadAll<BaseItemSO>("Items");

        foreach (BaseItemSO item in allItems)
        {
            if (_itemDictionary.ContainsKey(item.itemID))
            {
                Debug.LogWarning($"아이템 ID 충돌이 발생했습니다: {item.itemID} ({item.itemName})");
                continue;
            }
            _itemDictionary.Add(item.itemID, item);
        }
        Debug.Log($"총 {_itemDictionary.Count}개의 아이템 데이터를 로드했습니다.");
    }

    /// <summary>
    /// 아이템 ID를 통해 아이템 스크립터블 오브젝트를 가져옵니다.
    /// </summary>
    /// <param name="id">찾을 아이템의 고유 ID</param>
    /// <returns>해당 ID의 BaseItemSO 또는 null</returns>
    public BaseItemSO GetItem(int id)
    {
        if (_itemDictionary.ContainsKey(id))
        {
            return _itemDictionary[id];
        }
        Debug.LogWarning($"ID {id}에 해당하는 아이템을 찾을 수 없습니다!");
        return null;
    }
}