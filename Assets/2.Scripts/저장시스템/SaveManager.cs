using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// 게임 데이터의 저장 및 로드를 관리하는 중앙 스크립트입니다.
/// 싱글톤 패턴을 사용하여 씬 전체에서 쉽게 접근할 수 있도록 합니다.
/// ISavable 인터페이스를 구현한 모든 스크립트를 자동으로 찾아 데이터를 처리합니다.
/// </summary>
public class SaveManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    /// <summary>
    /// SaveManager의 유일한 인스턴스입니다.
    /// </summary>
    public static SaveManager Instance { get; private set; }

    // === 필드 ===
    /// <summary>
    /// 저장 파일의 경로입니다.
    /// </summary>
    public string saveFilePath { get; private set; }
    /// <summary>
    /// 로드된 게임 데이터를 임시로 보관하는 딕셔너리입니다.
    /// 씬이 로드된 후 각 ISavable 객체가 데이터를 요청할 때 사용됩니다.
    /// </summary>
    private Dictionary<string, SaveDataContainer> loadedSaveData;
    /// <summary>
    /// 현재 게임 세션이 '새로하기'로 시작되었는지 여부를 나타냅니다. (True: 새로 시작, False: 이어하기)
    /// </summary>
    public bool IsNewGame { get; private set; } = true; // 기본값은 True (파일이 없을 수도 있으므로)
    /// <summary>
    /// Awake는 스크립트 인스턴스가 로드될 때 호출됩니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 전환되어도 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 인스턴스가 있다면 새로운 인스턴스를 파괴합니다.
            Destroy(gameObject);
            return;
        }

        // 유니티가 제공하는 안전한 저장 경로를 사용하여 파일 경로를 설정합니다.
        // 이는 운영체제별로 경로를 자동으로 지정해주는 편리한 기능입니다.
        saveFilePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        //Debug.Log($"저장 파일 경로: {saveFilePath}");
    }

    // === 메서드 ===
    /// <summary>
    /// 게임 데이터를 저장합니다.
    /// 씬에 있는 모든 ISavable 객체를 찾아 데이터를 추출하고 JSON으로 직렬화합니다.
    /// </summary>
    public void SaveGame()
    {
        // 이름표가 붙은 데이터 상자를 담을 딕셔너리를 생성합니다.
        Dictionary<string, SaveDataContainer> saveData = new Dictionary<string, SaveDataContainer>();

        // 씬에서 ISavable 인터페이스를 구현한 모든 객체를 찾습니다.
        var savables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISavable>();

        // 각 객체의 데이터를 저장 상자에 담고 딕셔너리에 추가합니다.
        foreach (var savable in savables)
        {
            // 각 스크립트의 데이터에 고유한 이름표를 붙입니다.
            string key = ((MonoBehaviour)savable).GetType().Name;

            // 데이터 저장 상자를 생성하고 데이터를 담습니다.
            SaveDataContainer container = new SaveDataContainer
            {
                typeName = key,
                data = savable.SaveData()
            };
            saveData[key] = container;
        }

        // --- 여기부터 수정해야 할 코드입니다. ---
        // Json 직렬화 설정을 위한 객체를 생성합니다.
        // 이것이 'Self referencing loop' 에러를 해결하는 핵심입니다.
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            // 순환 참조가 발견되면 무시하도록 설정합니다.
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            // 데이터 타입 정보를 포함시켜 정확한 타입으로 역직렬화되도록 합니다.
            TypeNameHandling = TypeNameHandling.Auto
        };

        // 딕셔너리를 JSON 문자열로 변환할 때, settings 객체를 인자로 전달합니다.
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented, settings);

        // --- 여기까지 수정해야 할 코드입니다. ---

        // 지정된 경로에 JSON 파일을 작성합니다.
        File.WriteAllText(saveFilePath, json);
        // [추가/수정 로직] 저장 완료 알림을 NotificationManager를 통해 표시
        if (NotificationManager.Instance != null)
        {
            // 성공 타입 알림을 사용하여 "게임 저장 완료!" 메시지를 띄웁니다.
            NotificationManager.Instance.ShowNotification("게임 저장 완료!", NotificationType.Success);
        }
    }

    /// <summary>
    /// 저장된 게임 데이터를 로드합니다.
    /// JSON 파일을 읽어와 역직렬화하고, 씬에 있는 해당 스크립트에 데이터를 전달합니다.
    /// </summary>
    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("저장 파일이 없습니다. 로드할 수 없습니다.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);

        // --- 여기부터 수정해야 할 코드입니다. ---
        // Json 역직렬화 설정을 위한 객체를 생성합니다.
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            // 저장할 때와 동일한 설정을 사용해야 합니다.
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        // 로드된 게임 데이터를 임시 저장소에 역직렬화합니다.
        // settings 객체를 인자로 전달합니다.
        loadedSaveData = JsonConvert.DeserializeObject<Dictionary<string, SaveDataContainer>>(json, settings);
        IsNewGame = false; // 성공적으로 로드했으므로 '새로하기'가 아닙니다.
        // --- 여기까지 수정해야 할 코드입니다. ---
        Debug.Log("게임 로드 완료!");
    }
    /// <summary>
    /// 저장 파일이 존재하는지 확인하는 메서드입니다.
    /// </summary>
    /// <returns>저장 파일이 존재하면 true, 아니면 false</returns>
    public bool DoesSaveFileExist()
    {
        // saveFilePath 변수를 직접 참조하여 경로 일치시키기
        return System.IO.File.Exists(saveFilePath);
    }
    /// <summary>
    /// 새로 생성된 ISavable 객체를 등록하고, 저장된 데이터가 있으면 로드합니다.
    /// 이 메서드는 씬이 로드된 후 각 ISavable 객체의 Awake()나 Start()에서 호출됩니다.
    /// </summary>
    public void RegisterSavable(ISavable savable)
    {
        // 스크립트 타입을 키로 사용
        string key = ((MonoBehaviour)savable).GetType().Name;

        // 이전에 로드된 데이터가 있고, 해당 키에 데이터가 존재하는지 확인합니다.
        // 이 로직은 씬에 있는 모든 ISavable 스크립트가 자신을 등록할 때 실행됩니다.
        if (HasLoadedData && loadedSaveData.ContainsKey(key))
        {
            SaveDataContainer container = loadedSaveData[key];
            savable.LoadData(container.data);
        }
    }
    /// <summary>
    /// 로드된 게임 데이터가 메모리에 존재하는지 확인하는 속성입니다.
    /// TitleScene에서 이어하기 버튼 클릭 후 MainScene으로 넘어갈 때 true가 됩니다.
    /// </summary>
    public bool HasLoadedData
    {
        get { return loadedSaveData != null; }
    }
    /// <summary>
    /// 로드된 데이터 임시 저장소에서 특정 타입의 데이터를 가져오는 메서드입니다.
    /// </summary>
    /// <typeparam name="T">가져올 데이터의 타입입니다. (예: PlayerStatsSaveData)</typeparam>
    /// <param name="key">데이터를 식별하는 고유 키 (일반적으로 스크립트 이름)</param>
    /// <param name="data">데이터를 받을 변수</param>
    /// <returns>데이터가 존재하면 true, 아니면 false</returns>
    public bool TryGetData<T>(string key, out T data)
    {
        data = default(T); // 데이터를 찾지 못할 경우 기본값으로 초기화

        // 로드된 데이터가 있고, 해당 키에 데이터가 존재하는지 확인
        if (loadedSaveData != null && loadedSaveData.ContainsKey(key))
        {
            // 컨테이너에서 데이터를 가져와 올바른 타입으로 변환 시도
            if (loadedSaveData[key].data is T typedData)
            {
                data = typedData;
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 로드된 데이터 임시 저장소에서 DungeonManager의 데이터를 가져와 
    /// 특정 보스의 최초 처치 기록을 조회합니다.
    /// </summary>
    /// <param name="bossID">조회할 보스의 고유 ID (MonsterData.monsterID).</param>
    /// <returns>최초 처치 기록이 있으면 true, 없거나 데이터 로드에 실패하면 false.</returns>
    public bool GetBossFirstKillStatus(int bossID)
    {
        // DungeonManager가 저장한 데이터를 메모리에서 찾습니다.
        if (TryGetData<DungeonManagerSaveData>("DungeonManager", out DungeonManagerSaveData data))
        {
            // 데이터가 있다면, 딕셔너리에서 해당 보스 ID의 기록을 조회합니다.
            if (data.bossFirstKillRecords.TryGetValue(bossID, out bool isKilled))
            {
                return isKilled;
            }
        }
        // 기록이 없거나 로드된 데이터가 없으면 '아직 처치 안 함'으로 간주합니다.
        return false;
    }

    /// <summary>
    /// DungeonManager의 데이터에 특정 보스의 최초 처치 기록을 설정합니다.
    /// 이 메서드는 메모리 내의 로드된 데이터에만 변경을 가하며, 영구 저장하려면 SaveGame()을 호출해야 합니다.
    /// </summary>
    /// <param name="bossID">기록할 보스의 고유 ID (MonsterData.monsterID).</param>
    /// <param name="status">설정할 상태 (true = 처치 완료).</param>
    public void SetBossFirstKillStatus(int bossID, bool status)
    {
        // DungeonManager가 저장한 데이터를 메모리에서 찾거나 새로 생성합니다.
        if (!TryGetData<DungeonManagerSaveData>("DungeonManager", out DungeonManagerSaveData data))
        {
            // 데이터가 없으면 새로 만들어서 로드된 데이터 딕셔너리에 추가해야 합니다.
            data = new DungeonManagerSaveData();
            // SaveManager의 loadedSaveData 딕셔너리에 수동으로 추가하는 로직이 필요합니다.
            // 이는 DungeonManager가 ISavable을 구현하고 RegisterSavable을 호출했다는 가정 하에 복잡해지므로,
            // 현재는 'DungeonManager는 이미 ISavable을 구현하고 있다'고 가정하고,
            // **실제 DungeonManager 스크립트가 SaveManager에 의해 관리되도록 구현되어야 함**을 명시합니다.
            // (간단화를 위해, 여기서는 data 객체의 딕셔너리만 직접 업데이트합니다.)

            // ******************************************************************************
            // *주의: 이 코드는 DungeonManager가 SaveManager에 의해 올바르게 로드/관리되고 있음을 가정합니다.
            // * 실제 구현에서는 DungeonManager의 SaveData()가 이 Dictionary를 반환해야 합니다.
            // ******************************************************************************
        }

        // 기록을 업데이트합니다.
        data.bossFirstKillRecords[bossID] = status;

        // **핵심:** 이 변경 사항을 영구화하려면 DungeonManager가 SaveData()를 호출하고 
        // SaveManager.SaveGame()이 실행되어야 합니다.
    }

    /// <summary>
    /// 게임 데이터를 초기 상태로 리셋합니다.
    /// '새 게임 시작' 버튼에 연결하여 사용합니다.
    /// </summary>
    public void ResetGameData()
    {
        // 1. 저장 파일 삭제
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("기존 세이브 파일 삭제 완료!");
        }

        // 2. 메모리 내 데이터 초기화
        loadedSaveData = null;
        IsNewGame = true; // 새로하기 기능을 수행했으므로 True로 설정합니다.
        // 3. 씬의 저장 가능한 오브젝트 초기화
        // 월드에 있는 모든 SavableEntity 오브젝트들을 제거합니다.
        // 현재 씬에서 SavableEntity 컴포넌트를 가진 모든 오브젝트를 찾습니다.
        // **에러 수정 지점:** FindObjectsSortMode.None 인자를 추가해야 합니다.
        var savableObjects = FindObjectsByType<SavableEntity>(FindObjectsSortMode.None);


        // 순회 중 리스트가 변경되는 것을 방지하기 위해 ToList()를 사용합니다.
        foreach (var obj in savableObjects.ToList())
        {
            Destroy(obj.gameObject);
        }
    }
    /// <summary>
    /// 저장 파일에서 모든 데이터를 읽어와 loadedSaveData 딕셔너리에 채웁니다.
    /// 파일이 없으면 딕셔너리를 null로 설정합니다.
    /// SOLID: SRP (파일 읽기 책임)
    /// </summary>
    private void LoadGameDataFromFileToMemory()
    {
        if (!File.Exists(saveFilePath))
        {
            loadedSaveData = null; // 파일이 없으면 메모리 데이터도 없음
            Debug.Log("[SaveManager] 저장 파일이 없어 메모리에 데이터를 로드할 수 없습니다.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);

        // Json 역직렬화 설정 (SaveGame()과 동일)
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        try
        {
            // 로드된 게임 데이터를 임시 저장소에 역직렬화
            loadedSaveData = JsonConvert.DeserializeObject<Dictionary<string, SaveDataContainer>>(json, settings);
            IsNewGame = false; // 성공적으로 로드했으므로 '새로하기'가 아닙니다.
                               // Debug.Log("[SaveManager] 저장 파일 내용이 메모리(loadedSaveData)에 로드되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 데이터 로드 중 오류 발생: {e.Message}");
            loadedSaveData = null;
        }
    }
    /// <summary>
    /// 특정 ISavable 객체의 데이터만 수집하여 현재 로드된 데이터(loadedSaveData)를 업데이트하고,
    /// 즉시 파일에 덮어씁니다. (플레이어 사망 시 펫 상태 기록 등에 사용)
    /// SOLID: SRP (특정 데이터 업데이트 및 파일 기록 책임)
    /// </summary>
    /// <param name="savable">데이터를 저장할 ISavable 객체입니다.</param>
    public void SaveSingleSavable(ISavable savable)
    {
        // 1. 메모리에 로드된 데이터 딕셔너리를 **현재 파일 상태**로 준비합니다.
        // 기존 데이터 손실을 막기 위해 무조건 파일에서 전체를 로드합니다.
        LoadGameDataFromFileToMemory();

        // 로드에 실패했더라도, 새로운 빈 딕셔너리를 만들어 데이터를 저장할 준비를 합니다.
        if (loadedSaveData == null)
        {
            loadedSaveData = new Dictionary<string, SaveDataContainer>();
        }

        // 2. 키(Key) 생성
        string key = ((MonoBehaviour)savable).GetType().Name;

        // 3. 요청된 객체의 데이터만 수집
        SaveDataContainer container = new SaveDataContainer
        {
            typeName = key,
            data = savable.SaveData() // PetManager의 'false' 상태를 가져옴
        };

        // 4. 특정 데이터만 딕셔너리에 덮어쓰기 (다른 데이터는 이미 로드되어 존재)
        loadedSaveData[key] = container;

        // 5. 메모리에 있는 **전체 딕셔너리** (기존 데이터 + 업데이트된 펫 데이터)를 파일에 기록합니다.
        // Json 직렬화 설정을 SaveGame()과 동일하게 사용합니다.
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        string json = JsonConvert.SerializeObject(loadedSaveData, Formatting.Indented, settings);
        File.WriteAllText(saveFilePath, json);
    }
}