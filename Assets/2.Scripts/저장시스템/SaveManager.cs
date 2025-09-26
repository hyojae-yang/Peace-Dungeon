using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// ���� �������� ���� �� �ε带 �����ϴ� �߾� ��ũ��Ʈ�Դϴ�.
/// �̱��� ������ ����Ͽ� �� ��ü���� ���� ������ �� �ֵ��� �մϴ�.
/// ISavable �������̽��� ������ ��� ��ũ��Ʈ�� �ڵ����� ã�� �����͸� ó���մϴ�.
/// </summary>
public class SaveManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    /// <summary>
    /// SaveManager�� ������ �ν��Ͻ��Դϴ�.
    /// </summary>
    public static SaveManager Instance { get; private set; }

    // === �ʵ� ===
    /// <summary>
    /// ���� ������ ����Դϴ�.
    /// </summary>
    private string saveFilePath;
    /// <summary>
    /// �ε�� ���� �����͸� �ӽ÷� �����ϴ� ��ųʸ��Դϴ�.
    /// ���� �ε�� �� �� ISavable ��ü�� �����͸� ��û�� �� ���˴ϴ�.
    /// </summary>
    private Dictionary<string, SaveDataContainer> loadedSaveData;

    /// <summary>
    /// Awake�� ��ũ��Ʈ �ν��Ͻ��� �ε�� �� ȣ��˴ϴ�.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // ���� ��ȯ�Ǿ �ı����� �ʵ��� �����մϴ�.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �̹� �ν��Ͻ��� �ִٸ� ���ο� �ν��Ͻ��� �ı��մϴ�.
            Destroy(gameObject);
            return;
        }

        // ����Ƽ�� �����ϴ� ������ ���� ��θ� ����Ͽ� ���� ��θ� �����մϴ�.
        // �̴� �ü������ ��θ� �ڵ����� �������ִ� ���� ����Դϴ�.
        saveFilePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        Debug.Log($"���� ���� ���: {saveFilePath}");
    }

    // === �޼��� ===
    /// <summary>
    /// ���� �����͸� �����մϴ�.
    /// ���� �ִ� ��� ISavable ��ü�� ã�� �����͸� �����ϰ� JSON���� ����ȭ�մϴ�.
    /// </summary>
    public void SaveGame()
    {
        // �̸�ǥ�� ���� ������ ���ڸ� ���� ��ųʸ��� �����մϴ�.
        Dictionary<string, SaveDataContainer> saveData = new Dictionary<string, SaveDataContainer>();

        // ������ ISavable �������̽��� ������ ��� ��ü�� ã���ϴ�.
        var savables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISavable>();

        // �� ��ü�� �����͸� ���� ���ڿ� ��� ��ųʸ��� �߰��մϴ�.
        foreach (var savable in savables)
        {
            // �� ��ũ��Ʈ�� �����Ϳ� ������ �̸�ǥ�� ���Դϴ�.
            string key = ((MonoBehaviour)savable).GetType().Name;

            // ������ ���� ���ڸ� �����ϰ� �����͸� ����ϴ�.
            SaveDataContainer container = new SaveDataContainer
            {
                typeName = key,
                data = savable.SaveData()
            };
            saveData[key] = container;
        }

        // --- ������� �����ؾ� �� �ڵ��Դϴ�. ---
        // Json ����ȭ ������ ���� ��ü�� �����մϴ�.
        // �̰��� 'Self referencing loop' ������ �ذ��ϴ� �ٽ��Դϴ�.
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            // ��ȯ ������ �߰ߵǸ� �����ϵ��� �����մϴ�.
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            // ������ Ÿ�� ������ ���Խ��� ��Ȯ�� Ÿ������ ������ȭ�ǵ��� �մϴ�.
            TypeNameHandling = TypeNameHandling.Auto
        };

        // ��ųʸ��� JSON ���ڿ��� ��ȯ�� ��, settings ��ü�� ���ڷ� �����մϴ�.
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented, settings);

        // --- ������� �����ؾ� �� �ڵ��Դϴ�. ---

        // ������ ��ο� JSON ������ �ۼ��մϴ�.
        File.WriteAllText(saveFilePath, json);
        Debug.Log("���� ���� �Ϸ�!");
    }

    /// <summary>
    /// ����� ���� �����͸� �ε��մϴ�.
    /// JSON ������ �о�� ������ȭ�ϰ�, ���� �ִ� �ش� ��ũ��Ʈ�� �����͸� �����մϴ�.
    /// </summary>
    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("���� ������ �����ϴ�. �ε��� �� �����ϴ�.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);

        // --- ������� �����ؾ� �� �ڵ��Դϴ�. ---
        // Json ������ȭ ������ ���� ��ü�� �����մϴ�.
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            // ������ ���� ������ ������ ����ؾ� �մϴ�.
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        // �ε�� ���� �����͸� �ӽ� ����ҿ� ������ȭ�մϴ�.
        // settings ��ü�� ���ڷ� �����մϴ�.
        loadedSaveData = JsonConvert.DeserializeObject<Dictionary<string, SaveDataContainer>>(json, settings);
        // --- ������� �����ؾ� �� �ڵ��Դϴ�. ---
        Debug.Log("���� �ε� �Ϸ�!");
    }
    /// <summary>
    /// ���� ������ �����ϴ��� Ȯ���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>���� ������ �����ϸ� true, �ƴϸ� false</returns>
    public bool DoesSaveFileExist()
    {
        // saveFilePath ������ ���� �����Ͽ� ��� ��ġ��Ű��
        return System.IO.File.Exists(saveFilePath);
    }
    /// <summary>
    /// ���� ������ ISavable ��ü�� ����ϰ�, ����� �����Ͱ� ������ �ε��մϴ�.
    /// �� �޼���� ���� �ε�� �� �� ISavable ��ü�� Awake()�� Start()���� ȣ��˴ϴ�.
    /// </summary>
    public void RegisterSavable(ISavable savable)
    {
        // ��ũ��Ʈ Ÿ���� Ű�� ���
        string key = ((MonoBehaviour)savable).GetType().Name;

        // ������ �ε�� �����Ͱ� �ְ�, �ش� Ű�� �����Ͱ� �����ϴ��� Ȯ���մϴ�.
        // �� ������ ���� �ִ� ��� ISavable ��ũ��Ʈ�� �ڽ��� ����� �� ����˴ϴ�.
        if (HasLoadedData && loadedSaveData.ContainsKey(key))
        {
            SaveDataContainer container = loadedSaveData[key];
            savable.LoadData(container.data);
        }
    }
    /// <summary>
    /// �ε�� ���� �����Ͱ� �޸𸮿� �����ϴ��� Ȯ���ϴ� �Ӽ��Դϴ�.
    /// TitleScene���� �̾��ϱ� ��ư Ŭ�� �� MainScene���� �Ѿ �� true�� �˴ϴ�.
    /// </summary>
    public bool HasLoadedData
    {
        get { return loadedSaveData != null; }
    }
    /// <summary>
    /// �ε�� ������ �ӽ� ����ҿ��� Ư�� Ÿ���� �����͸� �������� �޼����Դϴ�.
    /// </summary>
    /// <typeparam name="T">������ �������� Ÿ���Դϴ�. (��: PlayerStatsSaveData)</typeparam>
    /// <param name="key">�����͸� �ĺ��ϴ� ���� Ű (�Ϲ������� ��ũ��Ʈ �̸�)</param>
    /// <param name="data">�����͸� ���� ����</param>
    /// <returns>�����Ͱ� �����ϸ� true, �ƴϸ� false</returns>
    public bool TryGetData<T>(string key, out T data)
    {
        data = default(T); // �����͸� ã�� ���� ��� �⺻������ �ʱ�ȭ

        // �ε�� �����Ͱ� �ְ�, �ش� Ű�� �����Ͱ� �����ϴ��� Ȯ��
        if (loadedSaveData != null && loadedSaveData.ContainsKey(key))
        {
            // �����̳ʿ��� �����͸� ������ �ùٸ� Ÿ������ ��ȯ �õ�
            if (loadedSaveData[key].data is T typedData)
            {
                data = typedData;
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// ���� �����͸� �ʱ� ���·� �����մϴ�.
    /// '�� ���� ����' ��ư�� �����Ͽ� ����մϴ�.
    /// </summary>
    public void ResetGameData()
    {
        // 1. ���� ���� ����
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("���� ���̺� ���� ���� �Ϸ�!");
        }

        // 2. �޸� �� ������ �ʱ�ȭ
        loadedSaveData = null;

        // 3. ���� ���� ������ ������Ʈ �ʱ�ȭ
        // ���忡 �ִ� ��� SavableEntity ������Ʈ���� �����մϴ�.
        // ���� ������ SavableEntity ������Ʈ�� ���� ��� ������Ʈ�� ã���ϴ�.
        // **���� ���� ����:** FindObjectsSortMode.None ���ڸ� �߰��ؾ� �մϴ�.
        var savableObjects = FindObjectsByType<SavableEntity>(FindObjectsSortMode.None);


        // ��ȸ �� ����Ʈ�� ����Ǵ� ���� �����ϱ� ���� ToList()�� ����մϴ�.
        foreach (var obj in savableObjects.ToList())
        {
            Destroy(obj.gameObject);
        }

        Debug.Log("���� ������ �ʱ�ȭ �Ϸ�!");
    }
}