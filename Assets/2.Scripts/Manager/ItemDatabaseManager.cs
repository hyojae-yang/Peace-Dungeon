using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 인벤토리 아이템의 데이터베이스 역할을 하는 싱글톤 스크립트입니다.
/// 게임 내 모든 BaseItemSO를 관리하고, ID를 통해 빠르게 검색할 수 있는 기능을 제공합니다.
/// [수정 원칙]: 빌드 후 LoadData 시점보다 Awake()가 늦어 발생하는 '아이템 없음' 문제를 해결하기 위해
/// GetItemByID 호출 시점에 데이터베이스 초기화를 보장하는 '지연 초기화'를 적용합니다.
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

        // [수정] Awake()에서 InitializeDatabase() 호출을 제거합니다. 
        // 대신 GetItemByID 호출 시점에 초기화를 보장합니다.
    }

    // === 메서드 ===
    /// <summary>
    /// 할당된 모든 아이템 데이터를 사용하여 데이터베이스를 초기화합니다.
    /// (외부에서 직접 호출되지 않으며, GetItemByID 호출 시에만 내부적으로 호출됩니다.)
    /// </summary>
    private void InitializeDatabase()
    {
        // 딕셔너리를 초기화하고 아이템을 추가합니다.
        // allItems 리스트에 에디터에서 할당된 아이템이 있다고 가정합니다.
        itemDictionary = allItems.ToDictionary(item => item.itemID, item => item);
        //Debug.Log($"[IDM] 아이템 데이터베이스가 초기화되었습니다. 총 아이템 수: {itemDictionary.Count}");
    }

    /// <summary>
    /// 아이템 ID를 사용하여 BaseItemSO를 찾아 반환합니다.
    /// 이 메서드가 호출될 때 데이터베이스 초기화를 보장합니다.
    /// </summary>
    /// <param name="id">찾을 아이템의 고유 ID</param>
    /// <returns>해당 ID를 가진 BaseItemSO 객체 또는 null</returns>
    public BaseItemSO GetItemByID(int id)
    {
        // [수정] 지연 초기화 로직: 딕셔너리가 비어있다면, 데이터를 찾기 전에 초기화합니다.
        // 이 시점은 PlayerItemController의 LoadData에서 호출되는 시점보다 앞서게 됩니다.
        if (itemDictionary.Count == 0 && allItems.Count > 0)
        {
            InitializeDatabase();
            // 만약 초기화 후에도 Count가 0이라면, allItems 할당 자체가 문제인 것입니다.
            if (itemDictionary.Count == 0)
            {
                Debug.LogError("[IDM] 초기화 후에도 아이템이 없습니다. allItems 필드에 SO가 할당되었는지 확인해주세요.");
            }
        }

        if (itemDictionary.TryGetValue(id, out BaseItemSO item))
        {
            return item;
        }
        Debug.LogWarning($"[IDM] 아이템 ID '{id}'에 해당하는 아이템을 찾을 수 없습니다. (데이터베이스에 해당 ID 없음)");
        return null;
    }

    /// <summary>
        /// 데이터베이스에 있는 모든 BaseItemSO 리스트를 반환합니다.
        /// SOLID 원칙: 외부에서 데이터베이스의 내용을 안전하게 읽을 수 있도록 제공합니다.
        /// </summary>
    public List<BaseItemSO> GetItemList()
    {
        // [추가] 리스트 반환 전에도 초기화를 보장합니다.
        if (itemDictionary.Count == 0 && allItems.Count > 0)
        {
            InitializeDatabase();
        }
        // 딕셔너리의 Value를 리스트로 반환하는 것이 더 효율적입니다.
        return itemDictionary.Values.ToList();
    }

    /// <summary>
    /// 아이템 이름을 사용하여 BaseItemSO를 찾아 반환합니다.
    /// Test 스크립트에서 드롭다운의 선택된 이름으로 실제 아이템 데이터를 찾을 때 사용됩니다.
    /// </summary>
    /// <param name="itemName">찾을 아이템의 이름</param>
    /// <returns>해당 이름을 가진 BaseItemSO 객체</returns>
    public BaseItemSO GetItemByName(string itemName)
    {
        // [추가] 탐색 전에도 초기화를 보장합니다.
        if (itemDictionary.Count == 0 && allItems.Count > 0)
        {
            InitializeDatabase();
        }
        // 딕셔너리의 값(아이템) 중에서 이름이 일치하는 첫 번째 아이템을 찾습니다.
        // BaseItemSO에 'itemName' 필드가 있다고 가정합니다.
        return itemDictionary.Values.FirstOrDefault(item => item.itemName == itemName);
    }
}