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

        // ��ųʸ��� JSON ���ڿ��� ��ȯ�մϴ�.
        // TypeNameHandling.Auto�� ����Ͽ� ������ Ÿ�� ������ ���Խ��� ������ȭ �� ��Ȯ�� Ÿ������ ��ȯ�ǵ��� �մϴ�.
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

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

        // LoadGame()�� ȣ��� ������ �ӽ� ����Ҹ� ���� �ʱ�ȭ�մϴ�.
        loadedSaveData = JsonConvert.DeserializeObject<Dictionary<string, SaveDataContainer>>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
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
    /// ���� ������ ISavable ��ü�� ����ϴ� �޼����Դϴ�.
    /// ���� �ε�� �� �� ISavable ��ü�� Awake()���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="savable">����� ISavable ��ũ��Ʈ</param>
    public void RegisterSavable(ISavable savable)
    {
        // ��ũ��Ʈ Ÿ���� Ű�� ���
        string key = ((MonoBehaviour)savable).GetType().Name;

        // ������ ����� �����Ͱ� �ִ��� Ȯ���ϰ� �ε�
        if (loadedSaveData != null && loadedSaveData.ContainsKey(key))
        {
            SaveDataContainer container = loadedSaveData[key];
            savable.LoadData(container.data);
            Debug.Log($"'{key}' ��ũ��Ʈ�� �����Ͱ� ���������� �ε�Ǿ����ϴ�.");
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
}