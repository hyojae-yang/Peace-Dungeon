using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 데이터를 저장하고 불러올 수 있는 모든 클래스가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface ISavable
{
    /// <summary>
    /// 현재 스크립트의 데이터를 SaveData 객체로 변환하여 반환합니다.
    /// </summary>
    /// <returns>저장 가능한 데이터 객체</returns>
    object SaveData();

    /// <summary>
    /// SaveData 객체의 데이터를 현재 스크립트에 적용합니다.
    /// </summary>
    /// <param name="data">저장된 데이터 객체</param>
    void LoadData(object data);
}