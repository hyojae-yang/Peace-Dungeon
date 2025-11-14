using UnityEngine;
using System;
using System.Collections.Generic; // Dictionary를 사용하기 위해 추가

/// <summary>
/// KillCountManager 클래스는 게임 내 몬스터의 종류별 처치 마릿수를 추적하고 관리하는 싱글톤입니다.
/// ISavable을 구현하여 처치 기록을 저장하고 불러올 수 있습니다. (엔딩 크레딧 통계용)
/// </summary>
public class KillCountManager : MonoBehaviour, ISavable
{
    // === 싱글톤 인스턴스 ===
    private static KillCountManager _instance;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다.
    /// </summary>
    public static KillCountManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에서 인스턴스를 찾습니다.
                _instance = FindFirstObjectByType<KillCountManager>();
                if (_instance == null)
                {
                    Debug.LogError("KillCountManager 인스턴스가 씬에 없습니다. 게임 오브젝트에 컴포넌트를 추가해야 합니다.");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// [수정] 몬스터 종류별 처치 횟수를 저장합니다. (Key: 몬스터 ID (int), Value: 처치 횟수)
    /// </summary>
    private Dictionary<int, int> _typeKills = new Dictionary<int, int>();

    /// <summary>
    /// [추가] 몬스터 종류별 처치 기록을 외부에서 읽을 수 있도록 읽기 전용 딕셔너리로 제공합니다.
    /// </summary>
    public IReadOnlyDictionary<int, int> TypeKills => _typeKills;

    /// <summary>
    /// [수정] 모든 몬스터의 총 처치 횟수를 딕셔너리를 기반으로 계산하여 반환합니다.
    /// </summary>
    public int TotalKills
    {
        get
        {
            int total = 0;
            // 딕셔너리의 모든 값(처치 횟수)을 합산합니다.
            foreach (var count in _typeKills.Values)
            {
                total += count;
            }
            return total;
        }
    }

    // 로드된 데이터가 성공적으로 적용되었는지 확인하는 플래그입니다. 
    private bool _isLoaded = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    /// <summary>
    /// SaveManager에 자신을 등록하고, 로드되지 않았다면 처치 기록을 초기화합니다.
    /// </summary>
    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }
        else
        {
            Debug.LogError("SaveManager 인스턴스가 없어 처치 기록 저장/로드 기능을 사용할 수 없습니다.");
        }

        // 로드되지 않은 경우 (새 게임 시작 등): 처치 기록 초기화
        if (!_isLoaded)
        {
            _typeKills.Clear(); // 딕셔너리 초기화
        }
    }

    /// <summary>
    /// [수정] 몬스터 처치 시 호출되어 특정 타입의 처치 마릿수를 1 증가시킵니다.
    /// </summary>
    /// <param name="monsterID">처치된 몬스터의 고유 ID입니다.</param>
    public void AddKillCount(int monsterID)
    {
        if (_typeKills.ContainsKey(monsterID))
        {
            _typeKills[monsterID]++;
        }
        else
        {
            _typeKills.Add(monsterID, 1);
        }
        // Debug.Log($"몬스터 ID {monsterID} 처치! 현재 누적 수: {_typeKills[monsterID]}");
    }

    // === ISavable 구현을 위한 데이터 구조 ===

    [Serializable]
    public class KillEntry
    {
        [Tooltip("몬스터의 고유 ID")]
        public int monsterID;
        [Tooltip("처치 횟수")]
        public int count;
    }

    [Serializable] // JSON 직렬화를 위해 필요합니다.
    private class KillCountSaveData
    {
        /// <summary>
        /// [수정] 몬스터 종류별 처치 기록 리스트를 저장합니다.
        /// Dictionary는 직렬화되지 않으므로 List 형태로 변환합니다.
        /// </summary>
        public List<KillEntry> killEntries = new List<KillEntry>();
    }

    // === ISavable 구현 ===

    /// <summary>
    /// ISavable 인터페이스 구현: 현재 처치 기록을 저장 데이터 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        var saveData = new KillCountSaveData();

        // 딕셔너리를 List<KillEntry>로 변환하여 저장합니다.
        foreach (var pair in _typeKills)
        {
            saveData.killEntries.Add(new KillEntry
            {
                monsterID = pair.Key,
                count = pair.Value
            });
        }
        return saveData;
    }

    /// <summary>
    /// ISavable 인터페이스 구현: 저장된 데이터 객체의 처치 기록을 현재 스크립트에 로드합니다.
    /// </summary>
    /// <param name="data">저장된 KillCountSaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is KillCountSaveData saveData)
        {
            _typeKills.Clear(); // 기존 데이터 초기화

            // 로드된 List<KillEntry>를 딕셔너리로 재구성합니다.
            foreach (var entry in saveData.killEntries)
            {
                if (!_typeKills.ContainsKey(entry.monsterID))
                {
                    _typeKills.Add(entry.monsterID, entry.count);
                }
            }

            _isLoaded = true; // 데이터 로드 성공 플래그 설정
            // Debug.Log($"[KillCountManager] 몬스터 처치 기록을 로드했습니다. 총 처치 수: {TotalKills}마리");
        }
        else
        {
            Debug.LogError("[KillCountManager] 로드된 데이터가 KillCountSaveData 타입이 아닙니다.");
        }
    }
}