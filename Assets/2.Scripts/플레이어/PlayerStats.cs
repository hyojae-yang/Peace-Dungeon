using UnityEngine;
using System.Collections.Generic; // Dictionary를 사용하기 위해 추가합니다.

// 스탯 데이터를 저장하고 관리하는 스크립트입니다.
// 이 스크립트는 MonoBehaviour를 상속받아 게임 오브젝트에 부착하여 사용할 수 있습니다.
public class PlayerStats : MonoBehaviour
{
    // === 기본 능력치 ===
    [Header("기본 능력치")]
    [Tooltip("플레이어의 시작 체력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMaxHealth = 100f;
    [Tooltip("플레이어의 시작 마나입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMaxMana = 50f;
    [Tooltip("플레이어의 시작 공격력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseAttackPower = 10f;
    [Tooltip("플레이어의 시작 마법 공격력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMagicAttackPower = 5f;
    [Tooltip("플레이어의 시작 방어력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseDefense = 5f;
    [Tooltip("플레이어의 시작 마법 방어력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMagicDefense = 5f;

    [Header("기본 특수 능력치")]
    [Tooltip("치명타가 발생할 기본 확률입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseCriticalChance = 0.05f;
    [Tooltip("치명타 발생 시 추가되는 기본 피해량 배율입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseCriticalDamageMultiplier = 1.5f;
    [Tooltip("캐릭터의 기본 이동 속도입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMoveSpeed = 5f;
    [Tooltip("공격 회피 기본 확률입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    [Range(0.0f, 1.0f)]
    public float baseEvasionChance = 0.02f;

    // === 실시간 능력치 (게임 플레이 중 변하는 스탯) ===
    [Header("실시간 능력치")]
    [Tooltip("캐릭터 이름")]
    public string characterName = "Hero";
    [Tooltip("소지 금액")]
    public int gold = 0;
    [Tooltip("현재 레벨")]
    public int level = 1;
    [Tooltip("현재 경험치")]
    public int experience = 0;
    [Tooltip("다음 레벨에 필요한 총 경험치")]
    public float requiredExperience = 10f;

    // PlayerStatSystem 스크립트에서 계산되어 최종적으로 적용되는 스탯입니다.
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
    [Tooltip("공격 회피 최종 확률입니다. (0.0 ~ 1.0)")]
    [Range(0.0f, 1.0f)]
    public float evasionChance = 0.02f; // 회피율 (2%)

    // === 스킬 시스템 ===
    [Header("스킬 시스템")]
    [Tooltip("플레이어가 보유한 최종 스킬 포인트입니다.")]
    public int skillPoints;
    [Tooltip("플레이어가 습득한 스킬의 최종 레벨 데이터입니다. (스킬ID, 스킬레벨)")]
    public Dictionary<int, int> skillLevels = new Dictionary<int, int>();
}