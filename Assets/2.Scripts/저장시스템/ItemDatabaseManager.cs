using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 인벤토리 아이템의 데이터베이스 역할을 하는 싱글톤 스크립트입니다.
/// 게임 내 모든 BaseItemSO를 관리하고, ID를 통해 빠르게 검색할 수 있는 기능을 제공합니다.
/// </summary>
public class ItemDatabaseManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    public static ItemDatabaseManager Instance { get; private set; }

    // === 필드 ===
    /// <summary>
    /// 유니티 에디터에서 직접 할당할 모든 BaseItemSO 에셋의 리스트입니다.
    /// </summary>
    [SerializeField] private List<BaseItemSO> allItems = new List<BaseItemSO>();

    /// <summary>
    /// ID로 아이템을 빠르게 찾기 위한 딕셔너리입니다.
    /// </summary>
    private Dictionary<int, BaseItemSO> itemDictionary = new Dictionary<int, BaseItemSO>();

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            // 씬이 변경되어도 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 아이템 데이터베이스를 초기화합니다.
        InitializeDatabase();
    }

    // === 메서드 ===
    /// <summary>
    /// 할당된 모든 아이템 데이터를 사용하여 데이터베이스를 초기화합니다.
    /// </summary>
    private void InitializeDatabase()
    {
        // 딕셔너리를 초기화하고 아이템을 추가합니다.
        itemDictionary = allItems.ToDictionary(item => item.itemID, item => item);
    }

    /// <summary>
    /// 아이템 ID를 사용하여 BaseItemSO를 찾아 반환합니다.
    /// </summary>
    /// <param name="id">찾을 아이템의 고유 ID</param>
    /// <returns>해당 ID를 가진 BaseItemSO 객체</returns>
    public BaseItemSO GetItemByID(int id)
    {
        if (itemDictionary.TryGetValue(id, out BaseItemSO item))
        {
            return item;
        }
        Debug.LogWarning($"아이템 ID '{id}'에 해당하는 아이템을 찾을 수 없습니다.");
        return null;
    }
}