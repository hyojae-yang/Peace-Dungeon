using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ��� SetBonusDataSO ������ �����ϰ� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class SetBonusDataManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static SetBonusDataManager Instance { get; private set; }

    [Tooltip("SetBonusDataSO ������ ����� Resources ���� ���� ����Դϴ�.")]
    [SerializeField] private string setBonusDataPath = "SetBonuses";

    // setID�� Ű��, SetBonusDataSO�� ������ �����ϴ� ��ųʸ�
    private Dictionary<string, SetBonusDataSO> setBonusDataMap;

    private void Awake()
    {
        // �̱��� ���� ���� (DontDestroyOnLoad ����)
        if (Instance == null)
        {
            Instance = this;
            LoadSetBonusData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resources �������� ��� SetBonusDataSO ������ �ε��Ͽ� ��ųʸ��� �����մϴ�.
    /// </summary>
    private void LoadSetBonusData()
    {
        // Resources ���� ���� ������ ��ο��� ��� SetBonusDataSO ������ �ҷ��ɴϴ�.
        SetBonusDataSO[] allSetBonusData = Resources.LoadAll<SetBonusDataSO>(setBonusDataPath);

        // ��ųʸ��� �ʱ�ȭ�ϰ� �����͸� ä��ϴ�.
        setBonusDataMap = new Dictionary<string, SetBonusDataSO>();
        foreach (SetBonusDataSO data in allSetBonusData)
        {
            if (setBonusDataMap.ContainsKey(data.setID))
            {
                Debug.LogWarning($"<color=red>���:</color> �ߺ��� Set ID '{data.setID}'�� �߰ߵǾ����ϴ�. ù ��° ���¸� ���˴ϴ�.");
                continue;
            }
            setBonusDataMap.Add(data.setID, data);
        }

        Debug.Log($"<color=blue>��Ʈ ���ʽ� ������ �ε� �Ϸ�:</color> �� {setBonusDataMap.Count}���� ��Ʈ �����͸� �ε��߽��ϴ�.");
    }

    /// <summary>
    /// �־��� ��Ʈ ID�� �ش��ϴ� SetBonusDataSO ������ ��ȯ�մϴ�.
    /// </summary>
    /// <param name="setID">ã�� ��Ʈ�� ���� ID</param>
    /// <returns>SetBonusDataSO ���� �Ǵ� ã�� ������ ��� null</returns>
    public SetBonusDataSO GetSetBonusData(string setID)
    {
        if (setBonusDataMap.TryGetValue(setID, out SetBonusDataSO data))
        {
            return data;
        }

        Debug.LogWarning($"<color=orange>��Ʈ ������ ����:</color> ID '{setID}'�� �ش��ϴ� SetBonusDataSO�� ã�� �� �����ϴ�.");
        return null;
    }
}