using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// PlaytimeManager 클래스는 게임의 총 플레이 시간을 추적하고 관리하는 싱글톤이며,
/// ISavable 인터페이스를 구현하여 플레이 시간을 저장하고 불러올 수 있습니다.
/// 단일 책임 원칙(SRP)에 따라 시간 측정 기능만을 담당합니다.
/// </summary>
public class PlaytimeManager : MonoBehaviour, ISavable
{
    // [1] 싱글톤 인스턴스 정의
    private static PlaytimeManager _instance;

    /// <summary>
    /// 로드된 데이터가 성공적으로 적용되었는지 확인하는 플래그입니다. 
    /// </summary>
    private bool _isLoaded = false;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다.
    /// </summary>
    public static PlaytimeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // FindFirstObjectByType을 사용하여 경고를 피하고 인스턴스를 찾습니다.
                _instance = FindFirstObjectByType<PlaytimeManager>();

                if (_instance == null)
                {
                    Debug.LogError("PlaytimeManager 인스턴스가 씬에 없습니다. 게임 오브젝트에 컴포넌트를 추가해야 합니다.");
                }
            }
            return _instance;
        }
    }

    // 게임 시작 또는 로드 시점의 Time.time 값을 기록하는 변수
    // (Time.time) - (_startTime) = (총 플레이 시간) 이 되도록 설정됩니다.
    private float _startTime;

    /// <summary>
    /// 싱글톤 패턴의 무결성을 보장합니다.
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
    /// SaveManager에 등록하고, 로드된 데이터가 없으면 새 게임으로 시간을 초기화합니다.
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
            Debug.LogError("SaveManager 인스턴스가 씬에 없습니다. 플레이타임 저장/로드 기능을 사용할 수 없습니다.");
        }

        // RegisterSavable 호출 후, 만약 LoadData가 호출되지 않았다면 (_isLoaded가 false라면)
        // 이는 새 게임 시작 또는 저장된 데이터가 없음을 의미합니다.
        if (!_isLoaded)
        {
            // 새로 시작: 현재 시점부터 시간을 재기 시작합니다.
            _startTime = Time.time;
            //Debug.Log($"플레이타임 측정을 새로 시작합니다. 시작 시간: {_startTime}");
        }
        // 로드된 경우 (LoadData가 호출된 경우), _startTime은 이미 LoadData에서 설정되었습니다.
    }

    /// <summary>
    /// 현재까지의 총 플레이 시간을 초(Seconds) 단위로 반환합니다.
    /// </summary>
    public float CurrentPlayTimeInSeconds
    {
        get
        {
            // 현재 시간에서 시작 시간을 빼서 총 경과 시간을 계산합니다.
            return Time.time - _startTime;
        }
    }

    /// <summary>
    /// 현재 플레이 시간을 "hh:mm:ss" 형식의 문자열로 반환합니다.
    /// </summary>
    /// <returns>시(Hour), 분(Minute), 초(Second) 형식의 문자열</returns>
    public string GetFormattedPlayTime()
    {
        float totalSeconds = CurrentPlayTimeInSeconds;

        // TimeSpan 구조체를 사용하여 초를 시/분/초로 변환합니다.
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        // 시간을 00:00:00 형태로 포맷합니다.
        string formattedTime = string.Format("{0:00}:{1:00}:{2:00}",
            (int)time.TotalHours, // 총 시간(24시간 이상 포함)
            time.Minutes,
            time.Seconds);

        return formattedTime;
    }

    // === ISavable 구현을 위한 데이터 구조 ===

    [Serializable] // JSON 직렬화를 위해 필요합니다.
    private class PlaytimeSaveData
    {
        /// <summary>
        /// 총 플레이 시간을 초 단위로 저장합니다.
        /// </summary>
        public float totalPlayTimeInSeconds;
    }

    // === ISavable 구현 ===

    /// <summary>
    /// ISavable 인터페이스 구현: 현재 플레이 시간을 저장 데이터 객체로 변환하여 반환합니다.
    /// </summary>
    /// <returns>플레이 시간 데이터 객체</returns>
    public object SaveData()
    {
        // 현재까지의 총 플레이 시간을 SaveData 객체에 담아 반환합니다.
        return new PlaytimeSaveData
        {
            totalPlayTimeInSeconds = CurrentPlayTimeInSeconds
        };
    }

    /// <summary>
    /// ISavable 인터페이스 구현: 저장된 데이터 객체의 플레이 시간을 현재 스크립트에 로드합니다.
    /// </summary>
    /// <param name="data">저장된 PlaytimeSaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is PlaytimeSaveData saveData)
        {
            // [핵심 로직] 로드된 총 시간부터 현재 시점까지의 차이를 계산하여 _startTime을 설정합니다.
            // (Time.time) - (_startTime) = (로드된 시간)이 되도록 설정하여 이어서 시간을 잽니다.
            _startTime = Time.time - saveData.totalPlayTimeInSeconds;

            _isLoaded = true; // 데이터 로드 성공 플래그 설정

            //Debug.Log($"[PlaytimeManager] 플레이타임을 로드했습니다. 이어서 시작 시간: {_startTime}, 총 로드 시간: {saveData.totalPlayTimeInSeconds}초");
        }
        else
        {
            Debug.LogError("[PlaytimeManager] 로드된 데이터가 PlaytimeSaveData 타입이 아닙니다.");
        }
    }
}