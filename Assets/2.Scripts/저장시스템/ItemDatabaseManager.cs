using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// �κ��丮 �������� �����ͺ��̽� ������ �ϴ� �̱��� ��ũ��Ʈ�Դϴ�.
/// ���� �� ��� BaseItemSO�� �����ϰ�, ID�� ���� ������ �˻��� �� �ִ� ����� �����մϴ�.
/// </summary>
public class ItemDatabaseManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static ItemDatabaseManager Instance { get; private set; }

    // === �ʵ� ===
    /// <summary>
    /// ����Ƽ �����Ϳ��� ���� �Ҵ��� ��� BaseItemSO ������ ����Ʈ�Դϴ�.
    /// </summary>
    [SerializeField] private List<BaseItemSO> allItems = new List<BaseItemSO>();

    /// <summary>
    /// ID�� �������� ������ ã�� ���� ��ųʸ��Դϴ�.
    /// </summary>
    private Dictionary<int, BaseItemSO> itemDictionary = new Dictionary<int, BaseItemSO>();

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            // ���� ����Ǿ �ı����� �ʵ��� �����մϴ�.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // ������ �����ͺ��̽��� �ʱ�ȭ�մϴ�.
        InitializeDatabase();
    }

    // === �޼��� ===
    /// <summary>
    /// �Ҵ�� ��� ������ �����͸� ����Ͽ� �����ͺ��̽��� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeDatabase()
    {
        // ��ųʸ��� �ʱ�ȭ�ϰ� �������� �߰��մϴ�.
        itemDictionary = allItems.ToDictionary(item => item.itemID, item => item);
    }

    /// <summary>
    /// ������ ID�� ����Ͽ� BaseItemSO�� ã�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="id">ã�� �������� ���� ID</param>
    /// <returns>�ش� ID�� ���� BaseItemSO ��ü</returns>
    public BaseItemSO GetItemByID(int id)
    {
        if (itemDictionary.TryGetValue(id, out BaseItemSO item))
        {
            return item;
        }
        Debug.LogWarning($"������ ID '{id}'�� �ش��ϴ� �������� ã�� �� �����ϴ�.");
        return null;
    }
}