using UnityEngine;
using System.Collections.Generic;

// 이 스크립트를 기반으로 유니티 에디터에서 스크립터블 오브젝트를 생성할 수 있습니다.
// 경로: Assets/Create/Skill/Passive/New Passive Skill Data
[CreateAssetMenu(fileName = "NewPassiveSkillData", menuName = "Skill/Passive/New Passive Skill Data")]
public class PassiveSkillData : SkillData
{
    // 패시브 스킬의 종류를 나타내기 위한 열거형
    public enum PassiveType
    {
        None, // 효과 없음
        StatBoost, // 스탯 증가
        HealthRegen, // 체력 재생
        Invincibility, // 무적 상태
        Revive // 부활
    }

    [Header("패시브 스킬 타입 및 정보")]
    [Tooltip("이 패시브 스킬의 타입입니다.")]
    public PassiveType passiveType;

    [Header("동적(Dynamic) 효과: 기능 스크립트")]
    [Tooltip("이 스킬에 해당하는 동적 효과 프리팹을 할당합니다. 해당 프리팹은 IPassiveEffect 인터페이스를 구현해야 합니다.")]
    public GameObject dynamicEffectPrefab;

    // 이 스크립터블 오브젝트가 생성될 때 스킬 타입을 자동으로 Passive로 설정합니다.
    private void Awake()
    {
        skillType = SkillType.Passive;
    }

    /// <summary>
    /// 패시브 스킬의 효과를 플레이어에게 적용하는 메서드입니다.
    /// SkillData 부모 클래스의 Execute 메서드를 재정의합니다.
    /// </summary>
    /// <param name="spawnPoint">패시브 스킬은 이 인자를 사용하지 않습니다.</param>
    /// <param name="playerStats">패시브 효과를 적용할 플레이어의 PlayerStats 컴포넌트입니다.</param>
    /// <param name="skillLevel">현재 스킬의 레벨</param>
    public override void Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel)
    {
        // 패시브 스킬은 일반적으로 플레이어가 획득하는 시점(Start)이나 레벨업 시에
        // PlayerStatSystem과 같은 스탯 관리 시스템에서 호출되어야 합니다.
        // 이 Execute 메서드 내에서 스탯을 적용하거나 동적 효과 프리팹을 생성하는 로직을 구현합니다.

        Debug.Log(skillName + " 패시브 스킬이 발동되었습니다.");

        // 예시: StatBoost 타입의 패시브 스킬 효과 적용
        if (passiveType == PassiveType.StatBoost)
        {
            // 스탯 적용 로직을 여기에 구현합니다.
            // 예: playerStats.AddStatBoost(this);
        }

        // 예시: HealthRegen과 같은 동적 효과 적용
        if (dynamicEffectPrefab != null)
        {
            // 동적 효과 프리팹을 생성하고 플레이어에게 부착하는 로직을 구현합니다.
            // GameObject effectInstance = Instantiate(dynamicEffectPrefab, playerStats.transform);
        }
    }
}