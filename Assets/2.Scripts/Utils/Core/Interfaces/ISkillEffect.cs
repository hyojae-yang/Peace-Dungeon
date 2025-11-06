using UnityEngine;

// 모든 액티브 스킬 효과가 구현해야 하는 규칙을 정의합니다.
public interface ISkillEffect
{
    /// <summary>
    /// 스킬의 고유한 효과를 실행합니다.
    /// </summary>
    /// <param name="levelInfo">현재 스킬 레벨에 해당하는 데이터</param>
    /// <param name="spawnPoint">스킬 효과가 시작될 위치</param>
    /// <param name="playerStats">플레이어의 스탯</param>
    void ExecuteEffect(SkillLevelInfo levelInfo, Transform spawnPoint, PlayerStats playerStats);
}