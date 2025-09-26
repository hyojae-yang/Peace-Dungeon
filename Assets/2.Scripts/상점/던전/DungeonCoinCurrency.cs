using UnityEngine;
using System.Collections.Generic;
using System;

// 던전 코인 데이터를 저장하기 위한 컨테이너 클래스입니다.
// SaveManager의 SaveDataContainer 구조에 맞춰 데이터를 담기 위한 용도입니다.
[Serializable]
public class DungeonCoinSaveData
{
    public int coins; // 던전 코인 수량
}

/// <summary>
/// 던전 코인 재화를 관리하는 싱글톤 클래스입니다.
/// 이 클래스는 씬이 전환되어도 파괴되지 않으며, 저장 시스템과 연동됩니다.
/// SOLID: 단일 책임 원칙 (던전 코인 재화 관리).
/// </summary>
public class DungeonCoinCurrency : MonoBehaviour, ISavable
{
    // 싱글톤 인스턴스를 외부에 노출하는 정적 프로퍼티입니다.
    public static DungeonCoinCurrency Instance { get; private set; }

    // 현재 플레이어가 보유한 던전 코인 수량입니다.
    // 이 변수가 저장 및 로드 대상이 됩니다.
    public int currentDungeonCoins { get; private set; } = 10;

    /// <summary>
    /// 이 객체가 생성될 때 한 번 호출되며, 싱글톤 패턴을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        // 인스턴스가 아직 존재하지 않는 경우
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // 이미 인스턴스가 존재하면 새로 생성된 객체는 파괴하여 중복을 방지합니다.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 객체가 활성화될 때 한 번 호출되며, SaveManager에 자신을 등록합니다.
    /// </summary>
    private void Start()
    {
        // 게임의 저장/로드 시스템에 이 객체를 등록합니다.
        SaveManager.Instance.RegisterSavable(this);
    }

    /// <summary>
    /// 던전 코인을 추가하는 메서드입니다. (예: 몬스터 처치 시 호출)
    /// </summary>
    /// <param name="amount">추가할 코인 양</param>
    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            currentDungeonCoins += amount;
            Debug.Log($"<color=yellow>코인 획득:</color> 던전 코인 {amount}개를 획득하여 현재 {currentDungeonCoins}개입니다.");
        }
    }

    /// <summary>
    /// 던전 코인을 사용하는 메서드입니다.
    /// </summary>
    /// <param name="amount">사용할 코인 양</param>
    /// <returns>코인 사용 성공 여부</returns>
    public bool SubtractCoins(int amount)
    {
        if (amount > 0 && currentDungeonCoins >= amount)
        {
            currentDungeonCoins -= amount;
            Debug.Log($"<color=red>코인 사용:</color> 던전 코인 {amount}개를 사용하여 현재 {currentDungeonCoins}개입니다.");
            return true;
        }
        Debug.LogWarning("던전 코인이 부족합니다.");
        return false;
    }

    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 던전 코인 수량을 반환하여 저장합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// </summary>
    public object SaveData()
    {
        // 저장용 데이터 컨테이너 객체를 생성하고 현재 코인 수량을 담아 반환합니다.
        return new DungeonCoinSaveData { coins = currentDungeonCoins };
    }

    /// <summary>
    /// 저장된 데이터(던전 코인 수량)를 불러와 적용합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// </summary>
    /// <param name="data">저장된 던전 코인 수량을 담은 DungeonCoinSaveData 객체</param>
    public void LoadData(object data)
    {
        // object 타입으로 받은 데이터를 DungeonCoinSaveData로 캐스팅하여 적용합니다.
        if (data is DungeonCoinSaveData loadedData)
        {
            currentDungeonCoins = loadedData.coins;
        }
    }
}