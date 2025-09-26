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
    private string saveFilePath;
    /// <summary>
    /// 로드된 게임 데이터를 임시로 보관하는 딕셔너리입니다.
    /// 씬이 로드된 후 각 ISavable 객체가 데이터를 요청할 때 사용됩니다.
    /// </summary>
    private Dictionary<string, SaveDataContainer> loadedSaveData;

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
        Debug.Log($"저장 파일 경로: {saveFilePath}");
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
        Debug.Log("게임 저장 완료!");
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

        Debug.Log("게임 데이터 초기화 완료!");
    }
}