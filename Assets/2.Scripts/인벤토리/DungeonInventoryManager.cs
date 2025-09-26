using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonInventoryManager : MonoBehaviour, ISavable
{
    // ObjectPool 의존성 제거
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private Transform contentParent;
    [SerializeField] private RectTransform dungeonInventoryRect;

    // 플레이어가 보유한 아이템 ID와 고유 ID 리스트
    private List<Tuple<string, int>> playerItems = new List<Tuple<string, int>>();
    private int nextUniqueID = 0; // 고유 ID를 생성하는 카운터

    private void Awake()
    {
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
        SaveManager.Instance.RegisterSavable(this);

        // === 추가된 코드: 저장된 데이터가 없을 경우에만 초기 아이템을 추가합니다. ===
        // SaveManager.Instance.HasLoadedData는 로드할 데이터가 있는지 알려주는 속성입니다.
        if (!SaveManager.Instance.HasLoadedData)
        {
            // 테스트용 초기 아이템 ID를 직접 리스트에 추가
            playerItems.Add(new Tuple<string, int>("2", nextUniqueID++));
        playerItems.Add(new Tuple<string, int>("4", nextUniqueID++));
        playerItems.Add(new Tuple<string, int>("12", nextUniqueID++));

        // 초기화 시에만 전체 새로고침
        RefreshInventoryUI();
        }
    }

    public RectTransform GetInventoryRect()
    {
        return dungeonInventoryRect;
    }

    // 외부에서 아이템을 획득했을 때 호출
    public void AddPlayerItem(string itemID)
    {
        int newUniqueID = nextUniqueID++;
        playerItems.Add(new Tuple<string, int>(itemID, newUniqueID));

        // 새로 추가된 아이템 하나만 UI에 추가
        AddUIItem(itemID, newUniqueID);
    }

    // 이 메서드는 3D 오브젝트를 다시 UI로 만들 때 호출됩니다.
    public void Convert3DToUI(GameObject smallMapObj, string itemID)
    {
        // 오브젝트 풀에 반환하는 대신 즉시 파괴
        Destroy(smallMapObj);
        AddPlayerItem(itemID);
    }

    // DraggableUIItem으로부터 호출되어 특정 UI 아이템을 제거
    public void ReturnUIItemToPool(int uniqueID)
    {
        RemovePlayerItem(uniqueID);
    }

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
        smallMapGO.transform.position = new Vector3(-350, 0, 150);
    }

    // UI 아이템을 하나씩 추가하는 메서드
    // DungeonInventoryManager.cs - AddUIItem() 메서드 수정
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

    // 아이템 데이터를 리스트에서 제거하고 UI도 제거
    public void RemovePlayerItem(int uniqueID)
    {
        // 고유 ID를 사용하여 리스트에서 아이템 데이터 제거
        int index = playerItems.FindIndex(item => item.Item2 == uniqueID);
        if (index != -1)
        {
            playerItems.RemoveAt(index);
        }
    }

    // 초기화 시에만 호출되는 전체 새로고침
    private void RefreshInventoryUI()
    {
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

        foreach (var itemTuple in playerItems)
        {
            AddUIItem(itemTuple.Item1, itemTuple.Item2);
        }
    }
    // === ISavable 인터페이스 구현 ===
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
            // === 추가된 디버그 로그: 데이터 로드 실패 알림 ===
            Debug.LogWarning($"<color=red>LoadData() 실패: 유효한 데이터가 없습니다.</color>");
        }
    }
}