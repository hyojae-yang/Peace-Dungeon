using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 던전 내 인벤토리 관리를 담당하는 싱글톤 클래스입니다.
/// ISavable 인터페이스를 구현하여 아이템 데이터를 저장하고 로드합니다.
/// </summary>
public class DungeonInventoryManager : MonoBehaviour, ISavable
{
    // === 싱글톤 인스턴스 ===
    /// <summary>
    /// DungeonInventoryManager의 유일한 인스턴스입니다.
    /// </summary>
    public static DungeonInventoryManager Instance { get; private set; }

    // === 필드 ===
    // ObjectPool 의존성 제거
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private Transform contentParent;
    [SerializeField] private RectTransform dungeonInventoryRect;

    // 플레이어가 보유한 아이템 ID와 고유 ID 리스트
    public List<Tuple<string, int>> playerItems = new List<Tuple<string, int>>();
    private int nextUniqueID = 0; // 고유 ID를 생성하는 카운터

    /// <summary>
    /// 스크립트 인스턴스가 로드될 때 호출되어 싱글톤 인스턴스를 설정하고 초기화합니다.
    /// </summary>
    private void Awake()
    {
        // 1. 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            // 씬 전환 시 파괴되지 않게 하려면 아래 주석을 해제하세요.
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // 이미 인스턴스가 있다면 새로운 인스턴스를 파괴합니다.
            Destroy(gameObject);
            return; // 중복 인스턴스는 즉시 작업을 중단합니다.
        }

        // 2. ItemDatabase 초기화 (기존 로직 유지)
        if (itemDatabase != null)
        {
            itemDatabase.Init();
        }
        else
        {
            Debug.LogError("Error: ItemDatabase가 할당되지 않았습니다.");
        }
    }

    private void Start()
    {
        // SaveManager에 자신을 등록합니다. (기존 로직 유지)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }
        else
        {
            Debug.LogError("SaveManager.Instance를 찾을 수 없습니다. 저장/로드 기능이 작동하지 않습니다.");
        }


        // === 추가된 코드: 저장된 데이터가 없을 경우에만 초기 아이템을 추가합니다. ===
        if (SaveManager.Instance != null && !SaveManager.Instance.HasLoadedData)
        {
            // 테스트용 초기 아이템 ID를 직접 리스트에 추가
            playerItems.Add(new Tuple<string, int>("2", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("4", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("8", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("9", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("10", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("11", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("12", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("13", nextUniqueID++));
            playerItems.Add(new Tuple<string, int>("14", nextUniqueID++));
            // 초기화 시에만 전체 새로고침
            RefreshInventoryUI();
        }
    }

    public RectTransform GetInventoryRect()
    {
        return dungeonInventoryRect;
    }

    /// <summary>
    /// 외부에서 아이템을 획득했을 때 호출됩니다.
    /// 새 고유 ID를 할당하고 아이템을 목록에 추가합니다.
    /// </summary>
    /// <param name="itemID">획득할 아이템의 ID입니다.</param>
    public void AddPlayerItem(string itemID)
    {
        int newUniqueID = nextUniqueID++;
        playerItems.Add(new Tuple<string, int>(itemID, newUniqueID));

        // 새로 추가된 아이템 하나만 UI에 추가
        AddUIItem(itemID, newUniqueID);
    }

    /// <summary>
    /// 3D 오브젝트를 인벤토리 아이템으로 전환할 때 호출됩니다.
    /// 3D 오브젝트를 파괴하고 아이템을 인벤토리에 추가합니다.
    /// </summary>
    /// <param name="smallMapObj">파괴할 3D 오브젝트입니다.</param>
    /// <param name="itemID">획득할 아이템의 ID입니다.</param>
    public void Convert3DToUI(GameObject smallMapObj, string itemID)
    {
        // 오브젝트 풀에 반환하는 대신 즉시 파괴
        Destroy(smallMapObj);
        AddPlayerItem(itemID);
    }

    /// <summary>
    /// DraggableUIItem으로부터 호출되어 특정 UI 아이템을 제거하고 3D 오브젝트를 생성합니다.
    /// </summary>
    /// <param name="uniqueID">제거할 아이템의 고유 ID입니다.</param>
    public void ReturnUIItemToPool(int uniqueID)
    {
        // 3D 오브젝트 활성화 로직은 ReturnUIItemToPool이 아닌, UI 아이템 사용 로직에서 발생해야 하지만,
        // 기존 코드의 흐름에 따라 RemovePlayerItem만 호출합니다.
        RemovePlayerItem(uniqueID);
    }

    /// <summary>
    /// 지정된 아이템 ID를 사용하여 3D 오브젝트를 씬에 생성하고 활성화합니다.
    /// </summary>
    /// <param name="itemID">활성화할 아이템의 ID입니다.</param>
    public void Activate3DObject(string itemID)
    {
        DungeonItemData data = itemDatabase.GetItem(itemID);
        if (data == null)
        {
            Debug.LogError($"Error: ID '{itemID}'에 해당하는 아이템 데이터를 찾을 수 없습니다.");
            return;
        }

        // 오브젝트 풀 대신 Instantiate()를 사용하여 오브젝트 생성
        GameObject smallMapGO = Instantiate(data.smallMapPrefab);
        smallMapGO.transform.position = new Vector3(500, 0, 200);
    }

    /// <summary>
    /// 인벤토리 UI에 아이템 요소를 하나씩 추가하는 메서드입니다.
    /// </summary>
    /// <param name="itemID">아이템의 종류 ID입니다.</param>
    /// <param name="uniqueID">아이템의 고유 ID입니다.</param>
    private void AddUIItem(string itemID, int uniqueID)
    {
        DungeonItemData data = itemDatabase.GetItem(itemID);

        // 오브젝트 풀 대신 Instantiate()를 사용하여 오브젝트 생성
        GameObject uiItemGO = Instantiate(data.uiItemPrefab);

        if (uiItemGO != null)
        {
            uiItemGO.transform.SetParent(contentParent, false);
            DungeonUIItem uiItem = uiItemGO.GetComponent<DungeonUIItem>();
            if (uiItem != null)
            {
                uiItem.Setup(data);
                uiItem.uniqueID = uniqueID; // UI 아이템에 고유 ID 할당
            }
        }
    }

    /// <summary>
    /// 아이템 데이터를 리스트에서 제거합니다. UI 제거는 호출부에서 담당해야 합니다.
    /// </summary>
    /// <param name="uniqueID">제거할 아이템의 고유 ID입니다.</param>
    public void RemovePlayerItem(int uniqueID)
    {
        // 고유 ID를 사용하여 리스트에서 아이템 데이터 제거
        int index = playerItems.FindIndex(item => item.Item2 == uniqueID);
        if (index != -1)
        {
            playerItems.RemoveAt(index);
        }
    }

    /// <summary>
    /// 인벤토리 UI를 완전히 비우고, 현재 보유 아이템 목록을 바탕으로 다시 그립니다.
    /// 주로 로드 완료 또는 초기화 시에만 호출됩니다.
    /// </summary>
    private void RefreshInventoryUI()
    {
        // 기존 UI 아이템 제거
        while (contentParent.childCount > 0)
        {
            GameObject child = contentParent.GetChild(0).gameObject;
            // 오브젝트 풀에 반환하는 대신 즉시 파괴
            Destroy(child);
        }

        if (playerItems.Count == 0)
        {
            Debug.Log("플레이어가 보유한 아이템이 없습니다.");
            return;
        }

        // 아이템 목록을 UI로 변환하여 추가
        foreach (var itemTuple in playerItems)
        {
            AddUIItem(itemTuple.Item1, itemTuple.Item2);
        }
    }

    // === ISavable 인터페이스 구현 ===
    /// <summary>
    /// 현재 인벤토리 상태를 저장할 데이터 객체로 변환합니다.
    /// </summary>
    /// <returns>저장 데이터 객체 (DungeonInventorySaveData)</returns>
    public object SaveData()
    {
        DungeonInventorySaveData data = new DungeonInventorySaveData();

        foreach (var itemTuple in playerItems)
        {
            string itemID = itemTuple.Item1;
            int uniqueID = itemTuple.Item2;

            DungeonItemSaveData itemData = new DungeonItemSaveData
            {
                itemID = itemID,
                uniqueID = uniqueID
            };
            data.dungeonItems.Add(itemData);
        }

        data.nextUniqueID = this.nextUniqueID;

        return data;
    }

    /// <summary>
    /// 저장된 데이터 객체로부터 인벤토리 상태를 로드하고 UI를 갱신합니다.
    /// </summary>
    /// <param name="data">로드할 데이터 객체 (object 타입)</param>
    public void LoadData(object data)
    {
        if (data is DungeonInventorySaveData loadedData)
        {
            playerItems.Clear();

            foreach (var item in loadedData.dungeonItems)
            {
                playerItems.Add(new Tuple<string, int>(item.itemID, item.uniqueID));
            }

            this.nextUniqueID = loadedData.nextUniqueID;

            RefreshInventoryUI();
        }
        else
        {
            Debug.LogWarning($"<color=red>LoadData() 실패: 유효한 데이터가 없습니다.</color>");
        }
    }
}