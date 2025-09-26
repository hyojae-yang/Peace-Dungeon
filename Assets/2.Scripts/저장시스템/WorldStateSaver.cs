using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 씬에 있는 모든 SavableEntity의 위치와 회전값을 저장하고 로드합니다.
/// 실시간으로 생성/제거되는 오브젝트를 동적으로 관리합니다.
/// </summary>
public class WorldStateSaver : MonoBehaviour, ISavable
{
    // === 1. 싱글톤 패턴 ===
    private static WorldStateSaver instance;

    // === 2. 오브젝트 리스트 관리 ===
    private Dictionary<string, SavableEntity> savableEntities = new Dictionary<string, SavableEntity>();

    // === 3. 프리팹 관리 변수 추가 ===
    // 이 리스트에 저장 및 로드할 모든 프리팹을 할당합니다.
    [SerializeField]
    private List<GameObject> savablePrefabs;

    // 프리팹의 이름을 키로 사용하여 빠르게 찾기 위한 딕셔너리입니다.
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // 필요에 따라 주석 처리

            // 프리팹 리스트를 딕셔너리로 변환하여 빠르게 접근할 수 있도록 합니다.
            foreach (var prefab in savablePrefabs)
            {
                if (prefab != null)
                {
                    prefabLookup[prefab.name] = prefab;
                    // 예: "Cube"라는 이름의 프리팹을 prefabLookup["Cube"]에 저장합니다.
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

    // === 4. 외부에서 접근 가능한 정적(Static) 메서드 ===
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

    // === 5. 저장 및 로드 메서드 ===
    public object SaveData()
    {
        WorldSaveData data = new WorldSaveData();
        foreach (var entity in savableEntities.Values)
        {
            data.objectPositions.Add(new PositionSaveData
            {
                uniqueID = entity.UniqueID,
                prefabID = entity.PrefabID, // <-- 수정: prefabID 추가
                position = entity.transform.position,
                rotation = entity.transform.rotation
            });
        }
        return data;
    }

    public void LoadData(object data)
    {
        // 데이터가 WorldSaveData 타입인지 확인
        if (data is WorldSaveData loadedData)
        {
            // 1. 기존 SavableEntity 오브젝트들을 모두 삭제하고 딕셔너리를 비웁니다.
            // 이 과정은 씬의 상태를 완전히 초기화하는 역할을 합니다.
            // savableEntities.Values는 딕셔너리의 값들을 참조하므로,
            // 순회 중 요소를 제거하면 오류가 발생할 수 있습니다.
            // 따라서 ToList()를 사용하여 임시 리스트를 만든 후 순회합니다.
            foreach (var entity in savableEntities.Values.ToList())
            {
                // Deregister를 호출하여 딕셔너리에서 먼저 제거합니다.
                // 이렇게 하면 Destroy(entity.gameObject) 호출 시 OnDisable()에서
                // Deregister()가 다시 호출되는 중복을 방지할 수 있습니다.
                Deregister(entity);

                // 오브젝트를 파괴합니다.
                Destroy(entity.gameObject);
            }

            // 모든 오브젝트가 제거되었으므로, 딕셔너리도 완전히 비워줍니다.
            savableEntities.Clear();

            // 2. 저장된 데이터를 기반으로 오브젝트들을 새롭게 생성하고 배치합니다.
            foreach (var savedData in loadedData.objectPositions)
            {
                // prefabLookup 딕셔너리에서 prefabID에 해당하는 프리팹을 찾습니다.
                if (prefabLookup.TryGetValue(savedData.prefabID, out GameObject prefabToSpawn))
                {
                    // 프리팹을 인스턴스화하고 위치와 회전값을 설정합니다.
                    GameObject newObject = Instantiate(prefabToSpawn, savedData.position, savedData.rotation);

                    // 새로 생성된 오브젝트의 SavableEntity 컴포넌트를 가져옵니다.
                    SavableEntity newEntity = newObject.GetComponent<SavableEntity>();

                    // 새로 생성된 오브젝트에 저장된 uniqueID를 설정합니다.
                    // 이렇게 해야 다음 저장 시 동일한 ID로 인식됩니다.
                    if (newEntity != null)
                    {
                        newEntity.SetUniqueID(savedData.uniqueID);
                        // WorldStateSaver에 새로 생성된 엔티티를 등록합니다.
                        // (이 로직은 newEntity.OnEnable()에서 자동 호출되므로 이 부분은 필요 없을 수 있습니다.)
                        // WorldStateSaver.Register(newEntity);
                    }
                }
                else
                {
                    // 프리팹을 찾지 못했을 경우 경고 메시지를 출력합니다.
                    Debug.LogWarning($"<color=red>WorldStateSaver:</color> ID '{savedData.uniqueID}'의 오브젝트 프리팹을 찾을 수 없습니다: '{savedData.prefabID}'");
                }
            }
        }
    }

    // === 6. 데이터 클래스 ===
    [System.Serializable]
    public class PositionSaveData
    {
        public string uniqueID;
        public string prefabID; // <-- 추가
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class WorldSaveData
    {
        public List<PositionSaveData> objectPositions = new List<PositionSaveData>();
    }
}