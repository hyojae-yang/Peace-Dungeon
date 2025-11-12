using UnityEngine;
using System;

/// <summary>
/// DeathCountManager 클래스는 플레이어의 총 사망 횟수를 추적하고 관리하는 싱글톤입니다.
/// ISavable을 구현하여 사망 기록을 저장하고 불러올 수 있습니다.
/// </summary>
public class DeathCountManager : MonoBehaviour, ISavable
{
    // === 싱글톤 인스턴스 ===
    private static DeathCountManager _instance;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다.
    /// </summary>
    public static DeathCountManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DeathCountManager>();
                if (_instance == null)
                {
                    Debug.LogError("DeathCountManager 인스턴스가 씬에 없습니다.");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 현재까지 플레이어가 사망한 총 횟수입니다.
    /// </summary>
    private int _totalDeaths = 0;

    // 사망 횟수는 외부에서 읽을 수만 있도록 프로퍼티로 제공합니다.
    public int TotalDeaths => _totalDeaths;

    // 데이터 로드 성공 여부 플래그
    private bool _isLoaded = false;

    /// <summary>
    /// 싱글톤 패턴의 무결성을 보장하고 파괴 방지 설정
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
    /// SaveManager에 등록하고, 로드된 데이터가 없으면 새 게임으로 사망 횟수를 초기화합니다.
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
            Debug.LogError("SaveManager 인스턴스가 없어 사망 기록 저장/로드 기능을 사용할 수 없습니다.");
        }

        // 로드되지 않은 경우 (새 게임): 사망 횟수 초기화
        if (!_isLoaded)
        {
            _totalDeaths = 0;
        }
    }

    /// <summary>
    /// 플레이어 사망 시 호출되어 총 사망 횟수를 1 증가시킵니다.
    /// </summary>
    public void AddDeathCount()
    {
        _totalDeaths++;

        // 사망은 중요한 이벤트이므로, 즉시 저장 로직을 추가하는 것을 고려할 수 있습니다.
        SaveManager.Instance.SaveSingleSavable(this); // 필요시 주석 해제
    }

    // === ISavable 구현을 위한 데이터 구조 ===

    [Serializable]
    private class DeathCountSaveData
    {
        /// <summary>
        /// 총 플레이어 사망 횟수를 저장합니다.
        /// </summary>
        public int totalDeaths;
    }

    // === ISavable 구현 ===

    /// <summary>
    /// ISavable 인터페이스 구현: 현재 사망 기록을 저장 데이터 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        return new DeathCountSaveData
        {
            totalDeaths = _totalDeaths
        };
    }

    /// <summary>
    /// ISavable 인터페이스 구현: 저장된 데이터 객체의 사망 기록을 현재 스크립트에 로드합니다.
    /// </summary>
    public void LoadData(object data)
    {
        if (data is DeathCountSaveData saveData)
        {
            _totalDeaths = saveData.totalDeaths;
            _isLoaded = true; // 데이터 로드 성공 플래그 설정

            //Debug.Log($"[DeathCountManager] 사망 기록을 로드했습니다. 총 사망 수: {_totalDeaths}회");
        }
        else
        {
            Debug.LogError("[DeathCountManager] 로드된 데이터가 DeathCountSaveData 타입이 아닙니다.");
        }
    }
}