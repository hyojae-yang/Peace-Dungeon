using UnityEngine;

// IPassiveEffect 인터페이스는 모든 동적 패시브 스킬 스크립트가 구현해야 하는 규칙을 정의합니다.
public interface IPassiveEffect
{
    /// <summary>
    /// 이 스킬의 동적 효과를 활성화하고 설정합니다.
    /// 스킬 레벨에 따라 효과의 강도를 조절합니다.
    /// </summary>
    /// <param name="skillLevel">현재 스킬 레벨</param>
    void ApplyEffect(int skillLevel);

    /// <summary>
    /// 이 스킬의 동적 효과를 비활성화하고 제거합니다.
    /// </summary>
    void RemoveEffect();

    /// <summary>
    /// 스킬의 레벨이 변경될 때 효과를 업데이트합니다.
    /// 패시브 스킬 레벨업 시 호출됩니다.
    /// </summary>
    /// <param name="newSkillLevel">변경된 스킬 레벨</param>
    void UpdateLevel(int newSkillLevel);
}