// ItemManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq�� ����ϱ� ���� �ʿ��մϴ�.

/// <summary>
/// ��� ������ �����͸� �����ϴ� �߾� �����ͺ��̽� ��ũ��Ʈ�Դϴ�.
/// �̱��� �������� �����Ǿ� ��𼭵� ���� ������ �� �ֽ��ϴ�.
/// </summary>
public class ItemManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
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
                    Debug.Log("���ο� 'ItemManagerSingleton' ���� ������Ʈ�� �����߽��ϴ�.");
                }
            }
            return _instance;
        }
    }

    // ������ ID�� Ű�� �Ͽ� BaseItemSO�� �����ϴ� ��ųʸ��Դϴ�.
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
            // DontDestroyOnLoad(gameObject); // �ʿ信 ���� �ּ� ����
        }

        LoadAllItems();
    }

    /// <summary>
    /// Resources ������ �ִ� ��� ������ ��ũ���ͺ� ������Ʈ�� �ε��մϴ�.
    /// </summary>
    private void LoadAllItems()
    {
        // "Items" ������ �ִ� ��� BaseItemSO�� �ε��մϴ�.
        BaseItemSO[] allItems = Resources.LoadAll<BaseItemSO>("Items");

        foreach (BaseItemSO item in allItems)
        {
            if (_itemDictionary.ContainsKey(item.itemID))
            {
                Debug.LogWarning($"������ ID �浹�� �߻��߽��ϴ�: {item.itemID} ({item.itemName})");
                continue;
            }
            _itemDictionary.Add(item.itemID, item);
        }
        Debug.Log($"�� {_itemDictionary.Count}���� ������ �����͸� �ε��߽��ϴ�.");
    }

    /// <summary>
    /// ������ ID�� ���� ������ ��ũ���ͺ� ������Ʈ�� �����ɴϴ�.
    /// </summary>
    /// <param name="id">ã�� �������� ���� ID</param>
    /// <returns>�ش� ID�� BaseItemSO �Ǵ� null</returns>
    public BaseItemSO GetItem(int id)
    {
        if (_itemDictionary.ContainsKey(id))
        {
            return _itemDictionary[id];
        }
        Debug.LogWarning($"ID {id}�� �ش��ϴ� �������� ã�� �� �����ϴ�!");
        return null;
    }
}