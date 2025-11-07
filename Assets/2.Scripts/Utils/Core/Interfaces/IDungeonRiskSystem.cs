using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [IDungeonRiskSystem]
/// 던전 위험도 시스템이 외부 클래스에 제공하는 핵심 계약을 정의합니다. (DIP 준수)
/// 현재는 던전 입장 횟수 추적 기능만 계약합니다.
/// </summary>
public interface IDungeonRiskSystem
{
    /// <summary>
    /// [핵심 기능 1: 입장 횟수 증가]
    /// 던전 진입 시 DungeonManager에 의해 호출되며, 내부적으로 입장 횟수(게이지)를 증가시킵니다.
    /// 현재는 던전 종류별 가중치 적용 없이 단순 1회 증가만 구현합니다.
    /// </summary>
    /// <param name="activeDungeonSpawnData">
    /// 현재 활성화된 던전 스폰 정보 리스트입니다. (향후 복잡한 가중치 계산을 위해 시그니처 유지)
    /// </param>
    void IncreaseExplorationCount(List<DungeonSpawnManager.MonsterSpawnData> activeDungeonSpawnData);

    // =========================================================================
    // [향후 확장 예정 기능 (현재는 구현하지 않음)]
    // - float GetMonsterStatMultiplier(); (몬스터 능력치 보정치)
    // - float GetSpawnCountMultiplier(); (스폰 몬스터 수 보정치)
    // - float GetScoreMultiplier(); (최종 보상 점수 보정치)
    // - int GetRiskLevel(); (현재 위험도 레벨)
    // =========================================================================
}