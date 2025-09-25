using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾� ���� ����Ʈ ���� �� ���� �ɷ�ġ ����� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� �� �̻� �̱����� �ƴϸ�, PlayerCharacter�� ����� �����˴ϴ�.
/// </summary>
public class PlayerStatSystem : MonoBehaviour, ISavable
{
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    [Header("���� ����Ʈ")]
    [Tooltip("������ �� ȹ���ϴ� ���� ����Ʈ�Դϴ�. �ɷ�ġ�� �����Ͽ� ������ �ø� �� �ֽ��ϴ�.")]
    public int statPoints = 5;

    [Header("�÷��̾� ���� ����")]
    [Tooltip("���� ���ݷ¿� ������ �ݴϴ�.")]
    public int strength = 0; // ��

    [Tooltip("���� ���ݷ°� ġ��Ÿ Ȯ���� ������ �ݴϴ�.")]
    public int intelligence = 0; // ����

    [Tooltip("�ִ� ü�°� ���¿� ������ �ݴϴ�.")]
    public int constitution = 0; // ü��

    [Tooltip("�̵� �ӵ��� ȸ������ ������ �ݴϴ�.")]
    public int agility = 0; // ��ø

    [Tooltip("���� ���ݷ°� ġ��Ÿ Ȯ���� ������ �ݴϴ�.")]
    public int focus = 0; // ���߷�

    [Tooltip("�ִ� ������ ���� ���¿� ������ �ݴϴ�.")]
    public int endurance = 0; // �γ���

    [Tooltip("�ִ� ü�°� �̵� �ӵ��� ������ �ݴϴ�.")]
    public int vitality = 0; // Ȱ��

    [Header("���� ����Ʈ�� �ɷ�ġ ��� ��")]
    [Tooltip("�� 1����Ʈ�� ���ݷ� ������")]
    [SerializeField] private float strengthToAttackPower = 2f;
    [Tooltip("���� 1����Ʈ�� ���� ���ݷ� ������")]
    [SerializeField] private float intelligenceToMagicAttackPower = 2.5f;
    [Tooltip("ü�� 1����Ʈ�� �ִ� ü�°� ���� ������")]
    [SerializeField] private float constitutionToMaxHealth = 10f;
    [SerializeField] private float constitutionToDefense = 1f;
    [Tooltip("��ø 1����Ʈ�� �̵� �ӵ� ������")]
    [SerializeField] private float agilityToMoveSpeed = 0.2f;
    [Tooltip("���߷� 1����Ʈ�� ���� ���ݷ°� ġ��Ÿ Ȯ�� ������")]
    [SerializeField] private float focusToMagicAttackPower = 0.5f;
    [SerializeField] private float focusToCriticalChance = 0.001f;
    [Tooltip("�γ��� 1����Ʈ�� �ִ� ������ ���� ���� ������")]
    [SerializeField] private float enduranceToMaxMana = 5f;
    [SerializeField] private float enduranceToMagicDefense = 1f;
    [Tooltip("Ȱ�� 1����Ʈ�� �ִ� ü�°� �̵� �ӵ� ������")]
    [SerializeField] private float vitalityToMaxHealth = 5f;
    [SerializeField] private float vitalityToMoveSpeed = 0.1f;
    [Tooltip("�� 1����Ʈ�� ġ��Ÿ ���ط� ������ (1 = 100%)")]
    [SerializeField] private float strengthToCriticalDamage = 0.01f; // 1% ���� (����)

    // === �ӽ� ���� ���� ===
    [Header("�ӽ� ���� (���� �� �̸������)")]
    public int tempStatPoints;
    public int tempStrength = 0;
    public int tempIntelligence = 0;
    public int tempConstitution = 0;
    public int tempAgility = 0;
    public int tempFocus = 0;
    public int tempEndurance = 0;
    public int tempVitality = 0;

    // === �нú� ��ų ���ʽ� ��ųʸ� ===
    private Dictionary<StatType, float> passiveFlatBonuses = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> passivePercentageBonuses = new Dictionary<StatType, float>();

    // === ��� ���� ���ʽ� ��ųʸ� ===
    [Header("��� ���� ���ʽ�")]
    // ��� ���������κ��� �޴� ���� ���� ���ʽ�
    private Dictionary<StatType, float> equipmentFlatBonuses = new Dictionary<StatType, float>();
    // ��� ���������κ��� �޴� ����� ���� ���ʽ�
    private Dictionary<StatType, float> equipmentPercentageBonuses = new Dictionary<StatType, float>();

    private void Awake()
    {
        // PlayerCharacter�� �ν��Ͻ��� �����ͼ� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� ã�� �� �����ϴ�. ��ũ��Ʈ�� ����� �������� ���� �� �ֽ��ϴ�.");
            return;
        }
        
    }
    void Start()
    {
        // ISavable �������̽��� ������ �� ��ü�� SaveManager�� ����մϴ�.
        SaveManager.Instance.RegisterSavable(this);

        // SaveManager�� �ε�� �����Ͱ� �ִ��� Ȯ���ϰ�, ������ �����մϴ�.
        if (SaveManager.Instance.HasLoadedData)
        {
            // SaveManager�κ��� PlayerStatSystem�� �ش��ϴ� �����͸� �����ɴϴ�.
            // TryGetData �޼���� �����͸� ã���� ��� true�� ��ȯ�ϰ� loadedData ������ �����͸� ����ϴ�.
            if (SaveManager.Instance.TryGetData(this.GetType().Name, out object loadedData))
            {
                // ������ �����͸� PlayerStatSystem�� �����մϴ�.
                LoadData(loadedData);
            }
        }

        // �ʱ� ���� ��� �� ����
        //UpdateFinalStats();
    }

    /// <summary>
    /// ���� ���� ������ �ӽ� ������ �����մϴ�. UI ��� �� ������.
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
    /// �ӽ� ���� ������ ���� ���� ���� ������ �����ϰ� ���� �ɷ�ġ�� ������Ʈ�մϴ�.
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
        Debug.Log("������ ���������� ����Ǿ����ϴ�!");
    }

    /// <summary>
    /// �ӽ� ���� ������ ���� ���� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public void ResetTempStats()
    {
        StoreTempStats();
    }

    /// <summary>
    /// PassiveSkillManager�κ��� �нú� ��ų ���ʽ��� �޾Ƽ� �����մϴ�.
    /// </summary>
    /// <param name="flatBonuses">�нú� ��ų�� ���� ���� ���� ���ʽ� ��ųʸ�</param>
    /// <param name="percentageBonuses">�нú� ��ų�� ���� ����� ���� ���ʽ� ��ųʸ�</param>
    public void ApplyPassiveBonuses(Dictionary<StatType, float> flatBonuses, Dictionary<StatType, float> percentageBonuses)
    {
        passiveFlatBonuses = flatBonuses;
        passivePercentageBonuses = percentageBonuses;
        UpdateFinalStats();
    }

    /// <summary>
    /// PlayerEquipmentManager�κ��� ��� ���� ���ʽ��� �޾Ƽ� �����մϴ�.
    /// </summary>
    /// <param name="flatBonuses">���� ���� ���� ���� ���ʽ� ��ųʸ�</param>
    /// <param name="percentageBonuses">���� ���� ����� ���� ���ʽ� ��ųʸ�</param>
    public void ApplyEquipmentBonuses(Dictionary<StatType, float> flatBonuses, Dictionary<StatType, float> percentageBonuses)
    {
        equipmentFlatBonuses = flatBonuses;
        equipmentPercentageBonuses = percentageBonuses;
        UpdateFinalStats();
    }

    /// <summary>
    /// �÷��̾��� ���� ���ȿ� ����Ͽ� ���� �ɷ�ġ�� ����ϰ� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateFinalStats()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerStats�� �ʱ�ȭ���� �ʾҽ��ϴ�. ���� ������ ����� �� �����ϴ�.");
            return;
        }

        PlayerStats playerStats = playerCharacter.playerStats;

        // ������ ���� �⺻ ���� ������ ���
        float levelHealthBonus = (playerStats.level - 1) * 10f;
        float levelManaBonus = (playerStats.level - 1) * 5f;
        float levelAttackBonus = (playerStats.level - 1) * 2f;
        float levelMagicAttackBonus = (playerStats.level - 1) * 1f;
        float levelDefenseBonus = (playerStats.level - 1) * 1f;
        float levelMagicDefenseBonus = (playerStats.level - 1) * 1f;

        // �⺻ ���� + ���� ���ʽ�
        float baseMaxHealth = playerStats.baseMaxHealth + levelHealthBonus;
        float baseMaxMana = playerStats.baseMaxMana + levelManaBonus;
        float baseAttackPower = playerStats.baseAttackPower + levelAttackBonus;
        float baseMagicAttackPower = playerStats.baseMagicAttackPower + levelMagicAttackBonus;
        float baseDefense = playerStats.baseDefense + levelDefenseBonus;
        float baseMagicDefense = playerStats.baseMagicDefense + levelMagicDefenseBonus;
        float baseCriticalChance = playerStats.baseCriticalChance;
        float baseCriticalDamageMultiplier = playerStats.baseCriticalDamageMultiplier;
        float baseMoveSpeed = playerStats.baseMoveSpeed;

        // ���� ���� ��� (������ �ջ�)
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

        // ���� ���� ����� ���ʽ��� �����մϴ�.
        playerStats.MaxHealth = finalMaxHealth * (1 + GetPassiveBonus(StatType.MaxHealth, passivePercentageBonuses) + GetEquipmentBonus(StatType.MaxHealth, equipmentPercentageBonuses));
        playerStats.MaxMana = finalMaxMana * (1 + GetPassiveBonus(StatType.MaxMana, passivePercentageBonuses) + GetEquipmentBonus(StatType.MaxMana, equipmentPercentageBonuses));
        playerStats.attackPower = finalAttackPower * (1 + GetPassiveBonus(StatType.AttackPower, passivePercentageBonuses) + GetEquipmentBonus(StatType.AttackPower, equipmentPercentageBonuses));
        playerStats.magicAttackPower = finalMagicAttackPower * (1 + GetPassiveBonus(StatType.MagicAttackPower, passivePercentageBonuses) + GetEquipmentBonus(StatType.MagicAttackPower, equipmentPercentageBonuses));
        playerStats.defense = finalDefense * (1 + GetPassiveBonus(StatType.Defense, passivePercentageBonuses) + GetEquipmentBonus(StatType.Defense, equipmentPercentageBonuses));
        playerStats.magicDefense = finalMagicDefense * (1 + GetPassiveBonus(StatType.MagicDefense, passivePercentageBonuses) + GetEquipmentBonus(StatType.MagicDefense, equipmentPercentageBonuses));
        playerStats.criticalChance = finalCriticalChance * (1 + GetPassiveBonus(StatType.CriticalChance, passivePercentageBonuses) + GetEquipmentBonus(StatType.CriticalChance, equipmentPercentageBonuses));
        playerStats.criticalDamageMultiplier = finalCriticalDamageMultiplier * (1 + GetPassiveBonus(StatType.CriticalDamage, passivePercentageBonuses) + GetEquipmentBonus(StatType.CriticalDamage, equipmentPercentageBonuses));
        playerStats.moveSpeed = finalMoveSpeed * (1 + GetPassiveBonus(StatType.MoveSpeed, passivePercentageBonuses) + GetEquipmentBonus(StatType.MoveSpeed, equipmentPercentageBonuses));

        // ü��/������ �ִ�ġ�� �ʰ����� �ʵ��� �����մϴ�.
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
    /// �нú� ��ų ���ʽ� ��ųʸ����� Ư�� ������ ���� �������� ����� �޼����Դϴ�.
    /// </summary>
    /// <param name="type">������ ������ ����</param>
    /// <param name="bonuses">�нú� ���ʽ� ��ųʸ�</param>
    /// <returns>�ش� ������ ���ʽ� ��. ������ 0�� ��ȯ�մϴ�.</returns>
    private float GetPassiveBonus(StatType type, Dictionary<StatType, float> bonuses)
    {
        return bonuses.ContainsKey(type) ? bonuses[type] : 0f;
    }

    /// <summary>
    /// ��� ���ʽ� ��ųʸ����� Ư�� ������ ���� �������� ����� �޼����Դϴ�.
    /// </summary>
    /// <param name="type">������ ������ ����</param>
    /// <param name="bonuses">��� ���ʽ� ��ųʸ�</param>
    /// <returns>�ش� ������ ���ʽ� ��. ������ 0�� ��ȯ�մϴ�.</returns>
    private float GetEquipmentBonus(StatType type, Dictionary<StatType, float> bonuses)
    {
        return bonuses.ContainsKey(type) ? bonuses[type] : 0f;
    }

    /// <summary>
    /// UI���� �̸����� ���� ����ϱ� ���� �ӽ� ���� ����
    /// ���� ��� ������ ������ �������� ����մϴ�.
    /// </summary>
    /// <param name="statType">����� ������ ����</param>
    /// <param name="tempStrength">�ӽ� ��</param>
    /// ... (�ٸ� �ӽ� ���ȵ�)
    public float GetPreviewFinalStat(
        StatType statType,
        int tempStrength, int tempIntelligence, int tempConstitution, int tempAgility,
        int tempFocus, int tempEndurance, int tempVitality)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerStats�� �ʱ�ȭ���� �ʾҽ��ϴ�. �̸����� ������ ����� �� �����ϴ�.");
            return 0f;
        }

        PlayerStats playerStats = playerCharacter.playerStats;

        // ������ ���� �⺻ ���� ������ ���
        float levelHealthBonus = (playerStats.level - 1) * 10f;
        float levelManaBonus = (playerStats.level - 1) * 5f;
        float levelAttackBonus = (playerStats.level - 1) * 2f;
        float levelMagicAttackBonus = (playerStats.level - 1) * 1f;
        float levelDefenseBonus = (playerStats.level - 1) * 1f;
        float levelMagicDefenseBonus = (playerStats.level - 1) * 1f;

        // �̸����� ����� �������� �Ǵ� �⺻ ����
        float baseMaxHealth = playerStats.baseMaxHealth + levelHealthBonus;
        float baseMaxMana = playerStats.baseMaxMana + levelManaBonus;
        float baseAttackPower = playerStats.baseAttackPower + levelAttackBonus;
        float baseMagicAttackPower = playerStats.baseMagicAttackPower + levelMagicAttackBonus;
        float baseDefense = playerStats.baseDefense + levelDefenseBonus;
        float baseMagicDefense = playerStats.baseMagicDefense + levelMagicDefenseBonus;
        float baseCriticalChance = playerStats.baseCriticalChance;
        float baseCriticalDamageMultiplier = playerStats.baseCriticalDamageMultiplier;
        float baseMoveSpeed = playerStats.baseMoveSpeed;

        // �ӽ� ���� ���ʽ��� ������ �߰��� ���
        float tempMaxHealth = baseMaxHealth + (tempConstitution * constitutionToMaxHealth) + (tempVitality * vitalityToMaxHealth);
        float tempMaxMana = baseMaxMana + (tempEndurance * enduranceToMaxMana);
        float tempAttackPower = baseAttackPower + (tempStrength * strengthToAttackPower);
        float tempMagicAttackPower = baseMagicAttackPower + (tempIntelligence * intelligenceToMagicAttackPower) + (tempFocus * focusToMagicAttackPower);
        float tempDefense = baseDefense + (tempConstitution * constitutionToDefense);
        float tempMagicDefense = baseMagicDefense + (tempEndurance * enduranceToMagicDefense);
        float tempCriticalChance = baseCriticalChance + (tempFocus * focusToCriticalChance);
        float tempCriticalDamageMultiplier = baseCriticalDamageMultiplier + (tempStrength * strengthToCriticalDamage);
        float tempMoveSpeed = baseMoveSpeed + (tempAgility * agilityToMoveSpeed) + (tempVitality * vitalityToMoveSpeed);

        // ���� ���� �нú� ���ʽ��� ��� ���ʽ��� ��� �����Ͽ� ��ȯ
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
                Debug.LogError($"�������� �ʴ� ���� Ÿ��: {statType}");
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
            Debug.LogWarning("���� ����Ʈ�� �����մϴ�!");
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
            Debug.LogWarning("������ �� �̻� ���� �� �����ϴ�! ����� ���� ������ ������ �� �����ϴ�.");
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
    // === ISavable �������̽� ���� ===
    /// <summary>
    /// ���� ��ũ��Ʈ�� �����͸� SaveData ��ü�� ��ȯ�Ͽ� ��ȯ�մϴ�.
    /// �� �޼���� SaveManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <returns>PlayerStatSystemSaveData Ÿ���� ���� ������ ������ ��ü</returns>
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
    /// SaveData ��ü�� �����͸� ���� ��ũ��Ʈ�� �����մϴ�.
    /// �� �޼���� SaveManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="data">�ε��� �����Ͱ� ��� PlayerStatSystemSaveData ��ü</param>
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

            // ���� ����Ʈ�� �ε��� �Ŀ��� ���� ������ �ٽ� ����մϴ�.
            // �̷ν� ����� ���� ����Ʈ�� �׻� �ֽ� �������� ���� �ɷ�ġ�� �����մϴ�.
            UpdateFinalStats();
        }
        else
        {
            // �� ���� �ٸ� ��ũ��Ʈ �����Ͱ� �ε�� �� �߻��մϴ�. �������� �����Դϴ�.
            Debug.LogWarning("�ε�� ������ Ÿ���� PlayerStatSystemSaveData�� ��ġ���� �ʽ��ϴ�. (�ٸ� ��ũ��Ʈ �������Դϴ�.)");
        }
    }
}