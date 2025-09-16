using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 모든 SetBonusDataSO 에셋을 관리하고 제공하는 스크립트입니다.
/// </summary>
public class SetBonusDataManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static SetBonusDataManager Instance { get; private set; }

    [Tooltip("SetBonusDataSO 에셋이 저장된 Resources 폴더 내의 경로입니다.")]
    [SerializeField] private string setBonusDataPath = "SetBonuses";

    // setID를 키로, SetBonusDataSO를 값으로 저장하는 딕셔너리
    private Dictionary<string, SetBonusDataSO> setBonusDataMap;

    private void Awake()
    {
        // 싱글턴 패턴 구현 (DontDestroyOnLoad 제거)
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
    /// Resources 폴더에서 모든 SetBonusDataSO 에셋을 로드하여 딕셔너리에 저장합니다.
    /// </summary>
    private void LoadSetBonusData()
    {
        // Resources 폴더 내의 지정된 경로에서 모든 SetBonusDataSO 에셋을 불러옵니다.
        SetBonusDataSO[] allSetBonusData = Resources.LoadAll<SetBonusDataSO>(setBonusDataPath);

        // 딕셔너리를 초기화하고 데이터를 채웁니다.
        setBonusDataMap = new Dictionary<string, SetBonusDataSO>();
        foreach (SetBonusDataSO data in allSetBonusData)
        {
            if (setBonusDataMap.ContainsKey(data.setID))
            {
                Debug.LogWarning($"<color=red>경고:</color> 중복된 Set ID '{data.setID}'가 발견되었습니다. 첫 번째 에셋만 사용됩니다.");
                continue;
            }
            setBonusDataMap.Add(data.setID, data);
        }

        Debug.Log($"<color=blue>세트 보너스 데이터 로드 완료:</color> 총 {setBonusDataMap.Count}개의 세트 데이터를 로드했습니다.");
    }

    /// <summary>
    /// 주어진 세트 ID에 해당하는 SetBonusDataSO 에셋을 반환합니다.
    /// </summary>
    /// <param name="setID">찾을 세트의 고유 ID</param>
    /// <returns>SetBonusDataSO 에셋 또는 찾지 못했을 경우 null</returns>
    public SetBonusDataSO GetSetBonusData(string setID)
    {
        if (setBonusDataMap.TryGetValue(setID, out SetBonusDataSO data))
        {
            return data;
        }

        Debug.LogWarning($"<color=orange>세트 데이터 없음:</color> ID '{setID}'에 해당하는 SetBonusDataSO를 찾을 수 없습니다.");
        return null;
    }
}