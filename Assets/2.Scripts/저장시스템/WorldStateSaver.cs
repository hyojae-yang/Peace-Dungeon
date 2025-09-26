using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ���� �ִ� ��� SavableEntity�� ��ġ�� ȸ������ �����ϰ� �ε��մϴ�.
/// �ǽð����� ����/���ŵǴ� ������Ʈ�� �������� �����մϴ�.
/// </summary>
public class WorldStateSaver : MonoBehaviour, ISavable
{
    // === 1. �̱��� ���� ===
    private static WorldStateSaver instance;

    // === 2. ������Ʈ ����Ʈ ���� ===
    private Dictionary<string, SavableEntity> savableEntities = new Dictionary<string, SavableEntity>();

    // === 3. ������ ���� ���� �߰� ===
    // �� ����Ʈ�� ���� �� �ε��� ��� �������� �Ҵ��մϴ�.
    [SerializeField]
    private List<GameObject> savablePrefabs;

    // �������� �̸��� Ű�� ����Ͽ� ������ ã�� ���� ��ųʸ��Դϴ�.
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // �ʿ信 ���� �ּ� ó��

            // ������ ����Ʈ�� ��ųʸ��� ��ȯ�Ͽ� ������ ������ �� �ֵ��� �մϴ�.
            foreach (var prefab in savablePrefabs)
            {
                if (prefab != null)
                {
                    prefabLookup[prefab.name] = prefab;
                    // ��: "Cube"��� �̸��� �������� prefabLookup["Cube"]�� �����մϴ�.
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SaveManager.Instance.RegisterSavable(this);
    }

    // === 4. �ܺο��� ���� ������ ����(Static) �޼��� ===
    public static void Register(SavableEntity entity)
    {
        if (instance != null && !instance.savableEntities.ContainsKey(entity.UniqueID))
        {
            instance.savableEntities.Add(entity.UniqueID, entity);
        }
    }

    public static void Deregister(SavableEntity entity)
    {
        if (instance != null && instance.savableEntities.ContainsKey(entity.UniqueID))
        {
            instance.savableEntities.Remove(entity.UniqueID);
        }
    }

    // === 5. ���� �� �ε� �޼��� ===
    public object SaveData()
    {
        WorldSaveData data = new WorldSaveData();
        foreach (var entity in savableEntities.Values)
        {
            data.objectPositions.Add(new PositionSaveData
            {
                uniqueID = entity.UniqueID,
                prefabID = entity.PrefabID, // <-- ����: prefabID �߰�
                position = entity.transform.position,
                rotation = entity.transform.rotation
            });
        }
        return data;
    }

    public void LoadData(object data)
    {
        // �����Ͱ� WorldSaveData Ÿ������ Ȯ��
        if (data is WorldSaveData loadedData)
        {
            // 1. ���� SavableEntity ������Ʈ���� ��� �����ϰ� ��ųʸ��� ���ϴ�.
            // �� ������ ���� ���¸� ������ �ʱ�ȭ�ϴ� ������ �մϴ�.
            // savableEntities.Values�� ��ųʸ��� ������ �����ϹǷ�,
            // ��ȸ �� ��Ҹ� �����ϸ� ������ �߻��� �� �ֽ��ϴ�.
            // ���� ToList()�� ����Ͽ� �ӽ� ����Ʈ�� ���� �� ��ȸ�մϴ�.
            foreach (var entity in savableEntities.Values.ToList())
            {
                // Deregister�� ȣ���Ͽ� ��ųʸ����� ���� �����մϴ�.
                // �̷��� �ϸ� Destroy(entity.gameObject) ȣ�� �� OnDisable()����
                // Deregister()�� �ٽ� ȣ��Ǵ� �ߺ��� ������ �� �ֽ��ϴ�.
                Deregister(entity);

                // ������Ʈ�� �ı��մϴ�.
                Destroy(entity.gameObject);
            }

            // ��� ������Ʈ�� ���ŵǾ����Ƿ�, ��ųʸ��� ������ ����ݴϴ�.
            savableEntities.Clear();

            // 2. ����� �����͸� ������� ������Ʈ���� ���Ӱ� �����ϰ� ��ġ�մϴ�.
            foreach (var savedData in loadedData.objectPositions)
            {
                // prefabLookup ��ųʸ����� prefabID�� �ش��ϴ� �������� ã���ϴ�.
                if (prefabLookup.TryGetValue(savedData.prefabID, out GameObject prefabToSpawn))
                {
                    // �������� �ν��Ͻ�ȭ�ϰ� ��ġ�� ȸ������ �����մϴ�.
                    GameObject newObject = Instantiate(prefabToSpawn, savedData.position, savedData.rotation);

                    // ���� ������ ������Ʈ�� SavableEntity ������Ʈ�� �����ɴϴ�.
                    SavableEntity newEntity = newObject.GetComponent<SavableEntity>();

                    // ���� ������ ������Ʈ�� ����� uniqueID�� �����մϴ�.
                    // �̷��� �ؾ� ���� ���� �� ������ ID�� �νĵ˴ϴ�.
                    if (newEntity != null)
                    {
                        newEntity.SetUniqueID(savedData.uniqueID);
                        // WorldStateSaver�� ���� ������ ��ƼƼ�� ����մϴ�.
                        // (�� ������ newEntity.OnEnable()���� �ڵ� ȣ��ǹǷ� �� �κ��� �ʿ� ���� �� �ֽ��ϴ�.)
                        // WorldStateSaver.Register(newEntity);
                    }
                }
                else
                {
                    // �������� ã�� ������ ��� ��� �޽����� ����մϴ�.
                    Debug.LogWarning($"<color=red>WorldStateSaver:</color> ID '{savedData.uniqueID}'�� ������Ʈ �������� ã�� �� �����ϴ�: '{savedData.prefabID}'");
                }
            }
        }
    }

    // === 6. ������ Ŭ���� ===
    [System.Serializable]
    public class PositionSaveData
    {
        public string uniqueID;
        public string prefabID; // <-- �߰�
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class WorldSaveData
    {
        public List<PositionSaveData> objectPositions = new List<PositionSaveData>();
    }
}