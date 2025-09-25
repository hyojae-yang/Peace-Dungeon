using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 스탯 포인트 투자 및 최종 능력치 계산을 관리하는 스크립트입니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
/// </summary>
public class PlayerStatSystem : MonoBehaviour, ISavable
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    [Header("스탯 포인트")]
    [Tooltip("레벨업 시 획득하는 스탯 포인트입니다. 능력치에 투자하여 스탯을 올릴 수 있습니다.")]
    public int statPoints = 5;

    [Header("플레이어 투자 스탯")]
    [Tooltip("물리 공격력에 영향을 줍니다.")]
    public int strength = 0; // 힘

    [Tooltip("마법 공격력과 치명타 확률에 영향을 줍니다.")]
    public int intelligence = 0; // 지능

    [Tooltip("최대 체력과 방어력에 영향을 줍니다.")]
    public int constitution = 0; // 체질

    [Tooltip("이동 속도와 회피율에 영향을 줍니다.")]
    public int agility = 0; // 민첩

    [Tooltip("마법 공격력과 치명타 확률에 영향을 줍니다.")]
    public int focus = 0; // 집중력

    [Tooltip("최대 마나와 마법 방어력에 영향을 줍니다.")]
    public int endurance = 0; // 인내력

    [Tooltip("최대 체력과 이동 속도에 영향을 줍니다.")]
    public int vitality = 0; // 활력

    [Header("스탯 포인트당 능력치 상승 값")]
    [Tooltip("힘 1포인트당 공격력 증가량")]
    [SerializeField] private float strengthToAttackPower = 2f;
    [Tooltip("지능 1포인트당 마법 공격력 증가량")]
    [SerializeField] private float intelligenceToMagicAttackPower = 2.5f;
    [Tooltip("체질 1포인트당 최대 체력과 방어력 증가량")]
    [SerializeField] private float constitutionToMaxHealth = 10f;
    [SerializeField] private float constitutionToDefense = 1f;
    [Tooltip("민첩 1포인트당 이동 속도 증가량")]
    [SerializeField] private float agilityToMoveSpeed = 0.2f;
    [Tooltip("집중력 1포인트당 마법 공격력과 치명타 확률 증가량")]
    [SerializeField] private float focusToMagicAttackPower = 0.5f;
    [SerializeField] private float focusToCriticalChance = 0.001f;
    [Tooltip("인내력 1포인트당 최대 마나와 마법 방어력 증가량")]
    [SerializeField] private float enduranceToMaxMana = 5f;
    [SerializeField] private float enduranceToMagicDefense = 1f;
    [Tooltip("활력 1포인트당 최대 체력과 이동 속도 증가량")]
    [SerializeField] private float vitalityToMaxHealth = 5f;
    [SerializeField] private float vitalityToMoveSpeed = 0.1f;
    [Tooltip("힘 1포인트당 치명타 피해량 증가량 (1 = 100%)")]
    [SerializeField] private float strengthToCriticalDamage = 0.01f; // 1% 증가 (예시)

    // === 임시 스탯 변수 ===
    [Header("임시 스탯 (저장 전 미리보기용)")]
    public int tempStatPoints;
    public int tempStrength = 0;
    public int tempIntelligence = 0;
    public int tempConstitution = 0;
    public int tempAgility = 0;
    public int tempFocus = 0;
    public int tempEndurance = 0;
    public int tempVitality = 0;

    // === 패시브 스킬 보너스 딕셔너리 ===
    private Dictionary<StatType, float> passiveFlatBonuses = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> passivePercentageBonuses = new Dictionary<StatType, float>();

    // === 장비 스탯 보너스 딕셔너리 ===
    [Header("장비 스탯 보너스")]
    // 장비 아이템으로부터 받는 고정 스탯 보너스
    private Dictionary<StatType, float> equipmentFlatBonuses = new Dictionary<StatType, float>();
    // 장비 아이템으로부터 받는 백분율 스탯 보너스
    private Dictionary<StatType, float> equipmentPercentageBonuses = new Dictionary<StatType, float>();

    private void Awake()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }
        
    }
    void Start()
    {
        // ISavable 인터페이스를 구현한 이 객체를 SaveManager에 등록합니다.
        SaveManager.Instance.RegisterSavable(this);

        // SaveManager에 로드된 데이터가 있는지 확인하고, 있으면 적용합니다.
        if (SaveManager.Instance.HasLoadedData)
        {
            // SaveManager로부터 PlayerStatSystem에 해당하는 데이터를 가져옵니다.
            // TryGetData 메서드는 데이터를 찾았을 경우 true를 반환하고 loadedData 변수에 데이터를 담습니다.
            if (SaveManager.Instance.TryGetData(this.GetType().Name, out object loadedData))
            {
                // 가져온 데이터를 PlayerStatSystem에 적용합니다.
                LoadData(loadedData);
            }
        }

        // 초기 스탯 계산 및 적용
        //UpdateFinalStats();
    }

    /// <summary>
    /// 현재 스탯 값들을 임시 변수에 저장합니다. UI 취소 시 복원용.
    /// </summary>
    public void StoreTempStats()
    {
        tempStatPoints = statPoints;
        tempStrength = strength;
        tempIntelligence = intelligence;
        tempConstitution = constitution;
        tempAgility = agility;
        tempFocus = focus;
        tempEndurance = endurance;
        tempVitality = vitality;
    }

    /// <summary>
    /// 임시 스탯 변수의 값을 실제 스탯 변수에 적용하고 최종 능력치를 업데이트합니다.
    /// </summary>
    public void ApplyStats()
    {
        statPoints = tempStatPoints;
        strength = tempStrength;
        intelligence = tempIntelligence;
        constitution = tempConstitution;
        agility = tempAgility;
        focus = tempFocus;
        endurance = tempEndurance;
        vitality = tempVitality;

        UpdateFinalStats();
        Debug.Log("스탯이 최종적으로 적용되었습니다!");
    }

    /// <summary>
    /// 임시 스탯 변수를 실제 스탯 값으로 초기화합니다.
    /// </summary>
    public void ResetTempStats()
    {
        StoreTempStats();
    }

    /// <summary>
    /// PassiveSkillManager로부터 패시브 스킬 보너스를 받아서 저장합니다.
    /// </summary>
    /// <param name="flatBonuses">패시브 스킬로 인한 고정 스탯 보너스 딕셔너리</param>
    /// <param name="percentageBonuses">패시브 스킬로 인한 백분율 스탯 보너스 딕셔너리</param>
    public void ApplyPassiveBonuses(Dictionary<StatType, float> flatBonuses, Dictionary<StatType, float> percentageBonuses)
    {
        passiveFlatBonuses = flatBonuses;
        passivePercentageBonuses = percentageBonuses;
        UpdateFinalStats();
    }

    /// <summary>
    /// PlayerEquipmentManager로부터 장비 스탯 보너스를 받아서 저장합니다.
    /// </summary>
    /// <param name="flatBonuses">장비로 인한 고정 스탯 보너스 딕셔너리</param>
    /// <param name="percentageBonuses">장비로 인한 백분율 스탯 보너스 딕셔너리</param>
    public void ApplyEquipmentBonuses(Dictionary<StatType, float> flatBonuses, Dictionary<StatType, float> percentageBonuses)
    {
        equipmentFlatBonuses = flatBonuses;
        equipmentPercentageBonuses = percentageBonuses;
        UpdateFinalStats();
    }

    /// <summary>
    /// 플레이어의 현재 스탯에 기반하여 최종 능력치를 계산하고 업데이트합니다.
    /// </summary>
    public void UpdateFinalStats()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 최종 스탯을 계산할 수 없습니다.");
            return;
        }

        PlayerStats playerStats = playerCharacter.playerStats;

        // 레벨에 따른 기본 스탯 증가량 계산
        float levelHealthBonus = (playerStats.level - 1) * 10f;
        float levelManaBonus = (playerStats.level - 1) * 5f;
        float levelAttackBonus = (playerStats.level - 1) * 2f;
        float levelMagicAttackBonus = (playerStats.level - 1) * 1f;
        float levelDefenseBonus = (playerStats.level - 1) * 1f;
        float levelMagicDefenseBonus = (playerStats.level - 1) * 1f;

        // 기본 스탯 + 레벨 보너스
        float baseMaxHealth = playerStats.baseMaxHealth + levelHealthBonus;
        float baseMaxMana = playerStats.baseMaxMana + levelManaBonus;
        float baseAttackPower = playerStats.baseAttackPower + levelAttackBonus;
        float baseMagicAttackPower = playerStats.baseMagicAttackPower + levelMagicAttackBonus;
        float baseDefense = playerStats.baseDefense + levelDefenseBonus;
        float baseMagicDefense = playerStats.baseMagicDefense + levelMagicDefenseBonus;
        float baseCriticalChance = playerStats.baseCriticalChance;
        float baseCriticalDamageMultiplier = playerStats.baseCriticalDamageMultiplier;
        float baseMoveSpeed = playerStats.baseMoveSpeed;

        // 최종 스탯 계산 (고정값 합산)
        float finalMaxHealth = baseMaxHealth
                             + (constitution * constitutionToMaxHealth) + (vitality * vitalityToMaxHealth)
                             + GetPassiveBonus(StatType.MaxHealth, passiveFlatBonuses)
                             + GetEquipmentBonus(StatType.MaxHealth, equipmentFlatBonuses);

        float finalMaxMana = baseMaxMana
                           + (endurance * enduranceToMaxMana)
                           + GetPassiveBonus(StatType.MaxMana, passiveFlatBonuses)
                           + GetEquipmentBonus(StatType.MaxMana, equipmentFlatBonuses);

        float finalAttackPower = baseAttackPower
                               + (strength * strengthToAttackPower)
                               + GetPassiveBonus(StatType.AttackPower, passiveFlatBonuses)
                               + GetEquipmentBonus(StatType.AttackPower, equipmentFlatBonuses);

        float finalMagicAttackPower = baseMagicAttackPower
                                    + (intelligence * intelligenceToMagicAttackPower) + (focus * focusToMagicAttackPower)
                                    + GetPassiveBonus(StatType.MagicAttackPower, passiveFlatBonuses)
                                    + GetEquipmentBonus(StatType.MagicAttackPower, equipmentFlatBonuses);

        float finalDefense = baseDefense
                           + (constitution * constitutionToDefense)
                           + GetPassiveBonus(StatType.Defense, passiveFlatBonuses)
                           + GetEquipmentBonus(StatType.Defense, equipmentFlatBonuses);

        float finalMagicDefense = baseMagicDefense
                                + (endurance * enduranceToMagicDefense)
                                + GetPassiveBonus(StatType.MagicDefense, passiveFlatBonuses)
                                + GetEquipmentBonus(StatType.MagicDefense, equipmentFlatBonuses);

        float finalCriticalChance = baseCriticalChance
                                  + (focus * focusToCriticalChance)
                                  + GetPassiveBonus(StatType.CriticalChance, passiveFlatBonuses)
                                  + GetEquipmentBonus(StatType.CriticalChance, equipmentFlatBonuses);

        float finalCriticalDamageMultiplier = baseCriticalDamageMultiplier
                                            + (strength * strengthToCriticalDamage)
                                            + GetPassiveBonus(StatType.CriticalDamage, passiveFlatBonuses)
                                            + GetEquipmentBonus(StatType.CriticalDamage, equipmentFlatBonuses);

        float finalMoveSpeed = baseMoveSpeed
                             + (agility * agilityToMoveSpeed) + (vitality * vitalityToMoveSpeed)
                             + GetPassiveBonus(StatType.MoveSpeed, passiveFlatBonuses)
                             + GetEquipmentBonus(StatType.MoveSpeed, equipmentFlatBonuses);

        // 최종 값에 백분율 보너스를 적용합니다.
        playerStats.MaxHealth = finalMaxHealth * (1 + GetPassiveBonus(StatType.MaxHealth, passivePercentageBonuses) + GetEquipmentBonus(StatType.MaxHealth, equipmentPercentageBonuses));
        playerStats.MaxMana = finalMaxMana * (1 + GetPassiveBonus(StatType.MaxMana, passivePercentageBonuses) + GetEquipmentBonus(StatType.MaxMana, equipmentPercentageBonuses));
        playerStats.attackPower = finalAttackPower * (1 + GetPassiveBonus(StatType.AttackPower, passivePercentageBonuses) + GetEquipmentBonus(StatType.AttackPower, equipmentPercentageBonuses));
        playerStats.magicAttackPower = finalMagicAttackPower * (1 + GetPassiveBonus(StatType.MagicAttackPower, passivePercentageBonuses) + GetEquipmentBonus(StatType.MagicAttackPower, equipmentPercentageBonuses));
        playerStats.defense = finalDefense * (1 + GetPassiveBonus(StatType.Defense, passivePercentageBonuses) + GetEquipmentBonus(StatType.Defense, equipmentPercentageBonuses));
        playerStats.magicDefense = finalMagicDefense * (1 + GetPassiveBonus(StatType.MagicDefense, passivePercentageBonuses) + GetEquipmentBonus(StatType.MagicDefense, equipmentPercentageBonuses));
        playerStats.criticalChance = finalCriticalChance * (1 + GetPassiveBonus(StatType.CriticalChance, passivePercentageBonuses) + GetEquipmentBonus(StatType.CriticalChance, equipmentPercentageBonuses));
        playerStats.criticalDamageMultiplier = finalCriticalDamageMultiplier * (1 + GetPassiveBonus(StatType.CriticalDamage, passivePercentageBonuses) + GetEquipmentBonus(StatType.CriticalDamage, equipmentPercentageBonuses));
        playerStats.moveSpeed = finalMoveSpeed * (1 + GetPassiveBonus(StatType.MoveSpeed, passivePercentageBonuses) + GetEquipmentBonus(StatType.MoveSpeed, equipmentPercentageBonuses));

        // 체력/마나가 최대치를 초과하지 않도록 보정합니다.
        if (playerStats.health > playerStats.MaxHealth)
        {
            playerStats.health = playerStats.MaxHealth;
        }
        if (playerStats.mana > playerStats.MaxMana)
        {
            playerStats.mana = playerStats.MaxMana;
        }
        playerCharacter.playerController.walkSpeed = playerStats.moveSpeed;
    }

    /// <summary>
    /// 패시브 스킬 보너스 딕셔너리에서 특정 스탯의 값을 가져오는 도우미 메서드입니다.
    /// </summary>
    /// <param name="type">가져올 스탯의 종류</param>
    /// <param name="bonuses">패시브 보너스 딕셔너리</param>
    /// <returns>해당 스탯의 보너스 값. 없으면 0을 반환합니다.</returns>
    private float GetPassiveBonus(StatType type, Dictionary<StatType, float> bonuses)
    {
        return bonuses.ContainsKey(type) ? bonuses[type] : 0f;
    }

    /// <summary>
    /// 장비 보너스 딕셔너리에서 특정 스탯의 값을 가져오는 도우미 메서드입니다.
    /// </summary>
    /// <param name="type">가져올 스탯의 종류</param>
    /// <param name="bonuses">장비 보너스 딕셔너리</param>
    /// <returns>해당 스탯의 보너스 값. 없으면 0을 반환합니다.</returns>
    private float GetEquipmentBonus(StatType type, Dictionary<StatType, float> bonuses)
    {
        return bonuses.ContainsKey(type) ? bonuses[type] : 0f;
    }

    /// <summary>
    /// UI에서 미리보기 값을 계산하기 위한 임시 스탯 계산기
    /// 이제 모든 스탯을 동일한 로직으로 계산합니다.
    /// </summary>
    /// <param name="statType">계산할 스탯의 종류</param>
    /// <param name="tempStrength">임시 힘</param>
    /// ... (다른 임시 스탯들)
    public float GetPreviewFinalStat(
        StatType statType,
        int tempStrength, int tempIntelligence, int tempConstitution, int tempAgility,
        int tempFocus, int tempEndurance, int tempVitality)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 미리보기 스탯을 계산할 수 없습니다.");
            return 0f;
        }

        PlayerStats playerStats = playerCharacter.playerStats;

        // 레벨에 따른 기본 스탯 증가량 계산
        float levelHealthBonus = (playerStats.level - 1) * 10f;
        float levelManaBonus = (playerStats.level - 1) * 5f;
        float levelAttackBonus = (playerStats.level - 1) * 2f;
        float levelMagicAttackBonus = (playerStats.level - 1) * 1f;
        float levelDefenseBonus = (playerStats.level - 1) * 1f;
        float levelMagicDefenseBonus = (playerStats.level - 1) * 1f;

        // 미리보기 계산의 시작점이 되는 기본 스탯
        float baseMaxHealth = playerStats.baseMaxHealth + levelHealthBonus;
        float baseMaxMana = playerStats.baseMaxMana + levelManaBonus;
        float baseAttackPower = playerStats.baseAttackPower + levelAttackBonus;
        float baseMagicAttackPower = playerStats.baseMagicAttackPower + levelMagicAttackBonus;
        float baseDefense = playerStats.baseDefense + levelDefenseBonus;
        float baseMagicDefense = playerStats.baseMagicDefense + levelMagicDefenseBonus;
        float baseCriticalChance = playerStats.baseCriticalChance;
        float baseCriticalDamageMultiplier = playerStats.baseCriticalDamageMultiplier;
        float baseMoveSpeed = playerStats.baseMoveSpeed;

        // 임시 스탯 보너스를 적용한 중간값 계산
        float tempMaxHealth = baseMaxHealth + (tempConstitution * constitutionToMaxHealth) + (tempVitality * vitalityToMaxHealth);
        float tempMaxMana = baseMaxMana + (tempEndurance * enduranceToMaxMana);
        float tempAttackPower = baseAttackPower + (tempStrength * strengthToAttackPower);
        float tempMagicAttackPower = baseMagicAttackPower + (tempIntelligence * intelligenceToMagicAttackPower) + (tempFocus * focusToMagicAttackPower);
        float tempDefense = baseDefense + (tempConstitution * constitutionToDefense);
        float tempMagicDefense = baseMagicDefense + (tempEndurance * enduranceToMagicDefense);
        float tempCriticalChance = baseCriticalChance + (tempFocus * focusToCriticalChance);
        float tempCriticalDamageMultiplier = baseCriticalDamageMultiplier + (tempStrength * strengthToCriticalDamage);
        float tempMoveSpeed = baseMoveSpeed + (tempAgility * agilityToMoveSpeed) + (tempVitality * vitalityToMoveSpeed);

        // 최종 값에 패시브 보너스와 장비 보너스를 모두 적용하여 반환
        switch (statType)
        {
            case StatType.MaxHealth:
                return (tempMaxHealth + GetPassiveBonus(StatType.MaxHealth, passiveFlatBonuses) + GetEquipmentBonus(StatType.MaxHealth, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.MaxHealth, passivePercentageBonuses) + GetEquipmentBonus(StatType.MaxHealth, equipmentPercentageBonuses));
            case StatType.MaxMana:
                return (tempMaxMana + GetPassiveBonus(StatType.MaxMana, passiveFlatBonuses) + GetEquipmentBonus(StatType.MaxMana, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.MaxMana, passivePercentageBonuses) + GetEquipmentBonus(StatType.MaxMana, equipmentPercentageBonuses));
            case StatType.AttackPower:
                return (tempAttackPower + GetPassiveBonus(StatType.AttackPower, passiveFlatBonuses) + GetEquipmentBonus(StatType.AttackPower, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.AttackPower, passivePercentageBonuses) + GetEquipmentBonus(StatType.AttackPower, equipmentPercentageBonuses));
            case StatType.MagicAttackPower:
                return (tempMagicAttackPower + GetPassiveBonus(StatType.MagicAttackPower, passiveFlatBonuses) + GetEquipmentBonus(StatType.MagicAttackPower, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.MagicAttackPower, passivePercentageBonuses) + GetEquipmentBonus(StatType.MagicAttackPower, equipmentPercentageBonuses));
            case StatType.Defense:
                return (tempDefense + GetPassiveBonus(StatType.Defense, passiveFlatBonuses) + GetEquipmentBonus(StatType.Defense, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.Defense, passivePercentageBonuses) + GetEquipmentBonus(StatType.Defense, equipmentPercentageBonuses));
            case StatType.MagicDefense:
                return (tempMagicDefense + GetPassiveBonus(StatType.MagicDefense, passiveFlatBonuses) + GetEquipmentBonus(StatType.MagicDefense, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.MagicDefense, passivePercentageBonuses) + GetEquipmentBonus(StatType.MagicDefense, equipmentPercentageBonuses));
            case StatType.CriticalChance:
                return (tempCriticalChance + GetPassiveBonus(StatType.CriticalChance, passiveFlatBonuses) + GetEquipmentBonus(StatType.CriticalChance, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.CriticalChance, passivePercentageBonuses) + GetEquipmentBonus(StatType.CriticalChance, equipmentPercentageBonuses));
            case StatType.CriticalDamage:
                return (tempCriticalDamageMultiplier + GetPassiveBonus(StatType.CriticalDamage, passiveFlatBonuses) + GetEquipmentBonus(StatType.CriticalDamage, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.CriticalDamage, passivePercentageBonuses) + GetEquipmentBonus(StatType.CriticalDamage, equipmentPercentageBonuses));
            case StatType.MoveSpeed:
                return (tempMoveSpeed + GetPassiveBonus(StatType.MoveSpeed, passiveFlatBonuses) + GetEquipmentBonus(StatType.MoveSpeed, equipmentFlatBonuses)) * (1 + GetPassiveBonus(StatType.MoveSpeed, passivePercentageBonuses) + GetEquipmentBonus(StatType.MoveSpeed, equipmentPercentageBonuses));
            default:
                Debug.LogError($"지원되지 않는 스탯 타입: {statType}");
                return 0f;
        }
    }

    public void IncreaseTempStrength() { IncreaseTempStat("strength"); }
    public void IncreaseTempIntelligence() { IncreaseTempStat("intelligence"); }
    public void IncreaseTempConstitution() { IncreaseTempStat("constitution"); }
    public void IncreaseTempAgility() { IncreaseTempStat("agility"); }
    public void IncreaseTempFocus() { IncreaseTempStat("focus"); }
    public void IncreaseTempEndurance() { IncreaseTempStat("endurance"); }
    public void IncreaseTempVitality() { IncreaseTempStat("vitality"); }

    public void DecreaseTempStrength() { DecreaseTempStat("strength"); }
    public void DecreaseTempIntelligence() { DecreaseTempStat("intelligence"); }
    public void DecreaseTempConstitution() { DecreaseTempStat("constitution"); }
    public void DecreaseTempAgility() { DecreaseTempStat("agility"); }
    public void DecreaseTempFocus() { DecreaseTempStat("focus"); }
    public void DecreaseTempEndurance() { DecreaseTempStat("endurance"); }
    public void DecreaseTempVitality() { DecreaseTempStat("vitality"); }

    private void IncreaseTempStat(string statName)
    {
        if (tempStatPoints <= 0)
        {
            Debug.LogWarning("스탯 포인트가 부족합니다!");
            return;
        }
        tempStatPoints--;
        switch (statName)
        {
            case "strength": tempStrength++; break;
            case "intelligence": tempIntelligence++; break;
            case "constitution": tempConstitution++; break;
            case "agility": tempAgility++; break;
            case "focus": tempFocus++; break;
            case "endurance": tempEndurance++; break;
            case "vitality": tempVitality++; break;
        }
    }

    private void DecreaseTempStat(string statName)
    {
        int tempStatValue = 0;
        int realStatValue = 0;
        switch (statName)
        {
            case "strength": tempStatValue = tempStrength; realStatValue = strength; break;
            case "intelligence": tempStatValue = tempIntelligence; realStatValue = intelligence; break;
            case "constitution": tempStatValue = tempConstitution; realStatValue = constitution; break;
            case "agility": tempStatValue = tempAgility; realStatValue = agility; break;
            case "focus": tempStatValue = tempFocus; realStatValue = focus; break;
            case "endurance": tempStatValue = tempEndurance; realStatValue = endurance; break;
            case "vitality": tempStatValue = tempVitality; realStatValue = vitality; break;
        }

        if (tempStatValue <= realStatValue)
        {
            Debug.LogWarning("스탯을 더 이상 내릴 수 없습니다! 저장된 스탯 값보다 낮아질 수 없습니다.");
            return;
        }

        switch (statName)
        {
            case "strength": tempStrength--; break;
            case "intelligence": tempIntelligence--; break;
            case "constitution": tempConstitution--; break;
            case "agility": tempAgility--; break;
            case "focus": tempFocus--; break;
            case "endurance": tempEndurance--; break;
            case "vitality": tempVitality--; break;
        }
        tempStatPoints++;
    }
    // === ISavable 인터페이스 구현 ===
    /// <summary>
    /// 현재 스크립트의 데이터를 SaveData 객체로 변환하여 반환합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// </summary>
    /// <returns>PlayerStatSystemSaveData 타입의 저장 가능한 데이터 객체</returns>
    public object SaveData()
    {
        PlayerStatSystemSaveData data = new PlayerStatSystemSaveData
        {
            statPoints = this.statPoints,
            strength = this.strength,
            intelligence = this.intelligence,
            constitution = this.constitution,
            agility = this.agility,
            focus = this.focus,
            endurance = this.endurance,
            vitality = this.vitality
        };
        return data;
    }
    /// <summary>
    /// SaveData 객체의 데이터를 현재 스크립트에 적용합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// </summary>
    /// <param name="data">로드할 데이터가 담긴 PlayerStatSystemSaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is PlayerStatSystemSaveData loadedData)
        {
            this.statPoints = loadedData.statPoints;
            this.strength = loadedData.strength;
            this.intelligence = loadedData.intelligence;
            this.constitution = loadedData.constitution;
            this.agility = loadedData.agility;
            this.focus = loadedData.focus;
            this.endurance = loadedData.endurance;
            this.vitality = loadedData.vitality;

            // 스탯 포인트를 로드한 후에는 최종 스탯을 다시 계산합니다.
            // 이로써 저장된 스탯 포인트가 항상 최신 로직으로 최종 능력치를 갱신합니다.
            UpdateFinalStats();
        }
        else
        {
            // 이 경고는 다른 스크립트 데이터가 로드될 때 발생합니다. 정상적인 동작입니다.
            Debug.LogWarning("로드된 데이터 타입이 PlayerStatSystemSaveData와 일치하지 않습니다. (다른 스크립트 데이터입니다.)");
        }
    }
}