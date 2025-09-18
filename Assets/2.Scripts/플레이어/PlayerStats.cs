using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 스탯 데이터를 저장하고 관리하는 스크립트입니다.
/// 이 스크립트는 이제 더 이상 싱글턴이 아니며,
/// PlayerCharacter 스크립트의 멤버로 포함되어 관리됩니다.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // === 기존 스탯 변수들은 그대로 둡니다. ===
    // 이제 이 아래에 있는 기존 변수들은 모두 PlayerCharacter.Instance.playerStats.변수명으로 접근하게 됩니다.

    // === 기본 능력치 ===
    [Header("기본 능력치")]
    [Tooltip("플레이어의 시작 체력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMaxHealth = 100f;
    public float baseMaxMana = 50f;
    public float baseAttackPower = 10f;
    public float baseMagicAttackPower = 5f;
    public float baseDefense = 5f;
    public float baseMagicDefense = 5f;

    [Header("기본 특수 능력치")]
    [Tooltip("치명타가 발생할 기본 확률입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseCriticalChance = 0.05f;
    public float baseCriticalDamageMultiplier = 1.5f;
    public float baseMoveSpeed = 5f;

    // === 실시간 능력치 (게임 플레이 중 변하는 스탯) ===
    [Header("실시간 능력치")]
    public string characterName = "Hero";
    public int gold = 0;
    public int level = 1;
    public int experience = 0;
    public float requiredExperience = 10f;

    public float MaxHealth = 100f; // 최대 체력
    public float health = 100f; // 현재 체력
    public float MaxMana = 50f; // 최대 마나
    public float mana = 50f; // 현재 마나
    public float attackPower = 10f; // 공격력
    public float magicAttackPower = 5f; // 마법 공격력
    public float defense = 5f; // 방어력
    public float magicDefense = 5f; // 마법 방어력

    // PlayerStatSystem 스크립트에서 계산되어 최종적으로 적용되는 스탯입니다.
    [Header("최종 특수 능력치")]
    [Tooltip("치명타가 발생할 최종 확률입니다. (0.0 ~ 1.0)")]
    [Range(0.0f, 1.0f)]
    public float criticalChance = 0.05f; // 치명타 확률 (5%)
    [Tooltip("치명타 발생 시 추가되는 최종 피해량 배율입니다.")]
    public float criticalDamageMultiplier = 1.5f; // 치명타 데미지 (150%)
    [Tooltip("캐릭터의 최종 이동 속도입니다.")]
    public float moveSpeed = 5f; // 이동 속도

    // === 스킬 시스템 ===
    [Header("스킬 시스템")]
    [Tooltip("플레이어가 보유한 최종 스킬 포인트입니다.")]
    public int skillPoints;
    [Tooltip("플레이어가 습득한 스킬의 최종 레벨 데이터입니다. (스킬ID, 스킬레벨)")]
    public Dictionary<int, int> skillLevels = new Dictionary<int, int>();
}