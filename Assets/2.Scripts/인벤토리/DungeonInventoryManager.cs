using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonInventoryManager : MonoBehaviour, ISavable
{
    // ObjectPool ������ ����
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private Transform contentParent;
    [SerializeField] private RectTransform dungeonInventoryRect;

    // �÷��̾ ������ ������ ID�� ���� ID ����Ʈ
    private List<Tuple<string, int>> playerItems = new List<Tuple<string, int>>();
    private int nextUniqueID = 0; // ���� ID�� �����ϴ� ī����

    private void Awake()
    {
        if (itemDatabase != null)
        {
            itemDatabase.Init();
        }
        else
        {
            Debug.LogError("Error: ItemDatabase�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
    private void Start()
    {
        SaveManager.Instance.RegisterSavable(this);

        // === �߰��� �ڵ�: ����� �����Ͱ� ���� ��쿡�� �ʱ� �������� �߰��մϴ�. ===
        // SaveManager.Instance.HasLoadedData�� �ε��� �����Ͱ� �ִ��� �˷��ִ� �Ӽ��Դϴ�.
        if (!SaveManager.Instance.HasLoadedData)
        {
            // �׽�Ʈ�� �ʱ� ������ ID�� ���� ����Ʈ�� �߰�
            playerItems.Add(new Tuple<string, int>("2", nextUniqueID++));
        playerItems.Add(new Tuple<string, int>("4", nextUniqueID++));
        playerItems.Add(new Tuple<string, int>("12", nextUniqueID++));

        // �ʱ�ȭ �ÿ��� ��ü ���ΰ�ħ
        RefreshInventoryUI();
        }
    }

    public RectTransform GetInventoryRect()
    {
        return dungeonInventoryRect;
    }

    // �ܺο��� �������� ȹ������ �� ȣ��
    public void AddPlayerItem(string itemID)
    {
        int newUniqueID = nextUniqueID++;
        playerItems.Add(new Tuple<string, int>(itemID, newUniqueID));

        // ���� �߰��� ������ �ϳ��� UI�� �߰�
        AddUIItem(itemID, newUniqueID);
    }

    // �� �޼���� 3D ������Ʈ�� �ٽ� UI�� ���� �� ȣ��˴ϴ�.
    public void Convert3DToUI(GameObject smallMapObj, string itemID)
    {
        // ������Ʈ Ǯ�� ��ȯ�ϴ� ��� ��� �ı�
        Destroy(smallMapObj);
        AddPlayerItem(itemID);
    }

    // DraggableUIItem���κ��� ȣ��Ǿ� Ư�� UI �������� ����
    public void ReturnUIItemToPool(int uniqueID)
    {
        RemovePlayerItem(uniqueID);
    }

    public void Activate3DObject(string itemID)
    {
        DungeonItemData data = itemDatabase.GetItem(itemID);
        if (data == null)
        {
            Debug.LogError($"Error: ID '{itemID}'�� �ش��ϴ� ������ �����͸� ã�� �� �����ϴ�.");
            return;
        }

        // ������Ʈ Ǯ ��� Instantiate()�� ����Ͽ� ������Ʈ ����
        GameObject smallMapGO = Instantiate(data.smallMapPrefab);
        smallMapGO.transform.position = new Vector3(-350, 0, 150);
    }

    // UI �������� �ϳ��� �߰��ϴ� �޼���
    // DungeonInventoryManager.cs - AddUIItem() �޼��� ����
    private void AddUIItem(string itemID, int uniqueID)
    {
        DungeonItemData data = itemDatabase.GetItem(itemID);

        // ������Ʈ Ǯ ��� Instantiate()�� ����Ͽ� ������Ʈ ����
        GameObject uiItemGO = Instantiate(data.uiItemPrefab);

        if (uiItemGO != null)
        {
            uiItemGO.transform.SetParent(contentParent, false);
            DungeonUIItem uiItem = uiItemGO.GetComponent<DungeonUIItem>();
            if (uiItem != null)
            {
                uiItem.Setup(data);
                uiItem.uniqueID = uniqueID; // UI �����ۿ� ���� ID �Ҵ�
            }
        }
    }

    // ������ �����͸� ����Ʈ���� �����ϰ� UI�� ����
    public void RemovePlayerItem(int uniqueID)
    {
        // ���� ID�� ����Ͽ� ����Ʈ���� ������ ������ ����
        int index = playerItems.FindIndex(item => item.Item2 == uniqueID);
        if (index != -1)
        {
            playerItems.RemoveAt(index);
        }
    }

    // �ʱ�ȭ �ÿ��� ȣ��Ǵ� ��ü ���ΰ�ħ
    private void RefreshInventoryUI()
    {
        while (contentParent.childCount > 0)
        {
            GameObject child = contentParent.GetChild(0).gameObject;
            // ������Ʈ Ǯ�� ��ȯ�ϴ� ��� ��� �ı�
            Destroy(child);
        }

        if (playerItems.Count == 0)
        {
            Debug.Log("�÷��̾ ������ �������� �����ϴ�.");
            return;
        }

        foreach (var itemTuple in playerItems)
        {
            AddUIItem(itemTuple.Item1, itemTuple.Item2);
        }
    }
    // === ISavable �������̽� ���� ===
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
            // === �߰��� ����� �α�: ������ �ε� ���� �˸� ===
            Debug.LogWarning($"<color=red>LoadData() ����: ��ȿ�� �����Ͱ� �����ϴ�.</color>");
        }
    }
}