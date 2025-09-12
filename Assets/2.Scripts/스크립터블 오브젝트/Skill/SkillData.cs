using UnityEngine;
using UnityEngine.UI;
using System;

// 스킬의 공격 타입을 정의하는 열거형 (Enum)
public enum DamageType
{
    Physical,
    Magic,
    True // 방어력 무시 고정 피해
}

// 스킬 종류를 정의하는 열거형 (Enum)
public enum SkillType
{
    Active,
    Passive
}

// 스킬 능력치의 종류를 정의하는 열거형 (Enum)
// 기존 스탯 시스템의 모든 스탯들을 추가하여 통합했습니다.
[Serializable]
public enum StatType
{
    BaseDamage,
    Cooldown,
    ManaCost,
    DamageOverTime,
    DOTDuration,
    HealOverTime,
    HPRegenDuration,
    AttackPowerIncrease,

    // 플레이어의 최종 스탯과 직접적으로 관련된 StatType 추가
    Defense,
    MagicDefense,
    AttackPower,
    MagicAttackPower,
    MaxHealth,
    MaxMana,
    MoveSpeed,
    CriticalChance,
    CriticalDamage,
    EvasionChance,

    // 스탯 포인트 투자와 관련된 StatType 추가
    Strength,
    Intelligence,
    Constitution,
    Agility,
    Focus,
    Endurance,
    Vitality
}

// 스킬 능력치 하나를 나타내는 클래스입니다.
[Serializable]
public class SkillStat
{
    [Tooltip("능력치의 종류")]
    public StatType statType;
    [Tooltip("해당 능력치의 값")]
    public float value;
    [Tooltip("이 스탯이 백분율로 적용되는지 여부 (예: 0.1 = 10% 증가)")]
    public bool isPercentage;
}

// 스킬 레벨별 정보를 담는 클래스입니다.
[Serializable]
public class SkillLevelInfo
{
    [Tooltip("해당 레벨의 마나 소모량입니다. (필요 시에만 사용)")]
    public float manaCost;
    [Tooltip("해당 레벨의 재사용 대기시간입니다. (필요 시에만 사용)")]
    public float cooldown;
    [Tooltip("해당 레벨에서 적용되는 능력치 목록입니다.")]
    public SkillStat[] stats;
}

// 이 스크립트를 기반으로 유니티 에디터에서 스크립터블 오브젝트를 생성합니다.
[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skill/New SkillData", order = 1)]
public abstract class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("스킬의 고유 식별자입니다.")]
    public int skillId;

    [Tooltip("스킬의 이름입니다.")]
    public string skillName;

    [Tooltip("스킬 아이콘으로 사용될 이미지입니다.")]
    public Sprite skillImage;

    [Tooltip("스킬의 종류(액티브 또는 패시브)입니다.")]
    public SkillType skillType;

    [Tooltip("스킬 공격의 타입입니다. 물리, 마법, 고정 피해 등을 구분합니다.")]
    public DamageType damageType;

    [Tooltip("스킬에 대한 간결한 설명입니다. UI에 표시됩니다.")]
    [TextArea(3, 5)]
    public string skillDescription;

    [Tooltip("스킬을 습득하기 위해 필요한 최소 레벨입니다.")]
    public int requiredLevel;

    [Header("레벨별 스킬 정보")]
    [Tooltip("스킬 레벨별로 달라지는 모든 정보(마나, 쿨타임, 스탯)의 배열입니다.")]
    public SkillLevelInfo[] levelInfo;

    /// <summary>
    /// 스킬의 고유한 효과를 발동시키는 추상 메서드입니다.
    /// 모든 자식 스크립트가 이 메서드를 반드시 구현해야 합니다.
    /// </summary>
    /// <param name="spawnPoint">스킬 효과가 발동될 위치</param>
    /// <param name="playerStats">스킬 발동 시 필요한 플레이어의 현재 능력치</param>
    /// <param name="skillLevel">현재 스킬의 레벨</param>
    public abstract void Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel);
}