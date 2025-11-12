using UnityEngine;
using System;

/// <summary>
/// KillCountManager 클래스는 게임 내 몬스터의 총 처치 마릿수를 추적하고 관리하는 싱글톤입니다.
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
    /// 현재까지 플레이어가 처치한 몬스터의 총 마릿수입니다. (저장 대상)
    /// </summary>
    private int _totalKills = 0;

    // 처치 횟수는 외부에서 읽을 수만 있도록 프로퍼티로 제공합니다.
    public int TotalKills => _totalKills;

    // 로드된 데이터가 성공적으로 적용되었는지 확인하는 플래그입니다. 
    private bool _isLoaded = false;

    /// <summary>
    /// 싱글톤 패턴의 무결성을 보장하고, 씬 로드 시 파괴되지 않도록 설정합니다.
    /// </summary>
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
            // SaveManager에 자신을 등록하여 로드된 데이터가 있으면 LoadData를 호출하도록 합니다.
            SaveManager.Instance.RegisterSavable(this);
        }
        else
        {
            Debug.LogError("SaveManager 인스턴스가 없어 처치 기록 저장/로드 기능을 사용할 수 없습니다.");
        }

        // 로드되지 않은 경우 (새 게임 시작 등): 처치 기록 초기화
        if (!_isLoaded)
        {
            _totalKills = 0;
        }
    }

    /// <summary>
    /// 몬스터 처치 시 DungeonScoreManager로부터 호출되어 총 처치 마릿수를 1 증가시킵니다.
    /// </summary>
    public void AddKillCount()
    {
        _totalKills++;
        // Debug.Log($"몬스터 처치! 총 처치 수: {_totalKills}");
        // 몬스터 처치는 자주 발생하므로, 저장 로직은 별도의 SaveGame() 호출 시점에 하는 것이 효율적입니다.
    }

    // === ISavable 구현을 위한 데이터 구조 ===

    [Serializable] // JSON 직렬화를 위해 필요합니다.
    private class KillCountSaveData
    {
        /// <summary>
        /// 총 몬스터 처치 마릿수를 저장합니다.
        /// </summary>
        public int totalKills;
    }

    // === ISavable 구현 ===

    /// <summary>
    /// ISavable 인터페이스 구현: 현재 처치 기록을 저장 데이터 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        // 현재까지의 총 처치 마릿수를 SaveData 객체에 담아 반환합니다.
        return new KillCountSaveData
        {
            totalKills = _totalKills
        };
    }

    /// <summary>
    /// ISavable 인터페이스 구현: 저장된 데이터 객체의 처치 기록을 현재 스크립트에 로드합니다.
    /// </summary>
    /// <param name="data">저장된 KillCountSaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is KillCountSaveData saveData)
        {
            // 로드된 처치 기록으로 현재 총 처치 마릿수를 설정합니다.
            _totalKills = saveData.totalKills;
            _isLoaded = true; // 데이터 로드 성공 플래그 설정

            //Debug.Log($"[KillCountManager] 몬스터 처치 기록을 로드했습니다. 총 처치 수: {_totalKills}마리");
        }
        else
        {
            Debug.LogError("[KillCountManager] 로드된 데이터가 KillCountSaveData 타입이 아닙니다.");
        }
    }
}