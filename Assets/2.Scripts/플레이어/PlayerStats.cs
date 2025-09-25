using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾��� ���� �����͸� �����ϰ� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� ���� �� �̻� �̱����� �ƴϸ�,
/// PlayerCharacter ��ũ��Ʈ�� ����� ���ԵǾ� �����˴ϴ�.
/// </summary>
public class PlayerStats : MonoBehaviour, ISavable
{
    // === ���� ���� �������� �״�� �Ӵϴ�. ===
    // ���� �� �Ʒ��� �ִ� ���� �������� ��� PlayerCharacter.Instance.playerStats.���������� �����ϰ� �˴ϴ�.
    public static Dictionary<StatType, float> TimedBuffs;
    // === �⺻ �ɷ�ġ ===
    [Header("�⺻ �ɷ�ġ")]
    [Tooltip("�÷��̾��� ���� ü���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMaxHealth = 100f;
    public float baseMaxMana = 50f;
    public float baseAttackPower = 10f;
    public float baseMagicAttackPower = 5f;
    public float baseDefense = 5f;
    public float baseMagicDefense = 5f;

    [Header("�⺻ Ư�� �ɷ�ġ")]
    [Tooltip("ġ��Ÿ�� �߻��� �⺻ Ȯ���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseCriticalChance = 0.05f;
    public float baseCriticalDamageMultiplier = 1.5f;
    public float baseMoveSpeed = 5f;

    // === �ǽð� �ɷ�ġ (���� �÷��� �� ���ϴ� ����) ===
    [Header("�ǽð� �ɷ�ġ")]
    public string characterName = "Hero";
    public int gold = 0;
    public int level = 1;
    public int experience = 0;
    public float requiredExperience = 10f;

    public float MaxHealth = 100f; // �ִ� ü��
    public float health = 100f; // ���� ü��
    public float MaxMana = 50f; // �ִ� ����
    public float mana = 50f; // ���� ����
    public float attackPower = 10f; // ���ݷ�
    public float magicAttackPower = 5f; // ���� ���ݷ�
    public float defense = 5f; // ����
    public float magicDefense = 5f; // ���� ����

    // PlayerStatSystem ��ũ��Ʈ���� ���Ǿ� ���������� ����Ǵ� �����Դϴ�.
    [Header("���� Ư�� �ɷ�ġ")]
    [Tooltip("ġ��Ÿ�� �߻��� ���� Ȯ���Դϴ�. (0.0 ~ 1.0)")]
    [Range(0.0f, 1.0f)]
    public float criticalChance = 0.05f; // ġ��Ÿ Ȯ�� (5%)
    [Tooltip("ġ��Ÿ �߻� �� �߰��Ǵ� ���� ���ط� �����Դϴ�.")]
    public float criticalDamageMultiplier = 1.5f; // ġ��Ÿ ������ (150%)
    [Tooltip("ĳ������ ���� �̵� �ӵ��Դϴ�.")]
    public float moveSpeed = 5f; // �̵� �ӵ�

    // === ��ų �ý��� ===
    [Header("��ų �ý���")]
    [Tooltip("�÷��̾ ������ ���� ��ų ����Ʈ�Դϴ�.")]
    public int skillPoints;
    [Tooltip("�÷��̾ ������ ��ų�� ���� ���� �������Դϴ�. (��ųID, ��ų����)")]
    public Dictionary<int, int> skillLevels = new Dictionary<int, int>();

    private void Awake()
    {
        // ISavable �������̽��� ������ �� ��ü�� SaveManager�� ����մϴ�.
        SaveManager.Instance.RegisterSavable(this);

        // SaveManager�� �ε�� �����Ͱ� �ִ��� Ȯ���ϰ�, ������ �����մϴ�.
        if (SaveManager.Instance.HasLoadedData)
        {
            // SaveManager�κ��� PlayerStats�� �ش��ϴ� �����͸� �����ɴϴ�.
            // TryGetData �޼���� �����͸� ã���� ��� true�� ��ȯ�ϰ� loadedData ������ �����͸� ����ϴ�.
            if (SaveManager.Instance.TryGetData(this.GetType().Name, out object loadedData))
            {
                // ������ �����͸� PlayerStats�� �����մϴ�.
                LoadData(loadedData);
            }
        }
    }
    // === ISavable �������̽� ���� ===
    /// <summary>
    /// ���� ��ũ��Ʈ�� �����͸� SaveData ��ü�� ��ȯ�Ͽ� ��ȯ�մϴ�.
    /// �� �޼���� SaveManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <returns>PlayerStatsSaveData Ÿ���� ���� ������ ������ ��ü</returns>
    public object SaveData()
    {
        PlayerStatsSaveData data = new PlayerStatsSaveData
        {
            // === �ǽð� �ٽ� �ɷ�ġ ===
            health = this.health,
            mana = this.mana,
            attackPower = this.attackPower,
            magicAttackPower = this.magicAttackPower,
            defense = this.defense,
            magicDefense = this.magicDefense,
            criticalChance = this.criticalChance,
            criticalDamageMultiplier = this.criticalDamageMultiplier,
            moveSpeed = this.moveSpeed,

            // === ĳ���� ���� ��Ȳ ===
            characterName = this.characterName,
            gold = this.gold,
            level = this.level,
            experience = this.experience,
            skillPoints = this.skillPoints,

            // === ��ų ��ųʸ� ===
            skillLevels = this.skillLevels
        };
        return data;
    }

    /// <summary>
    /// SaveData ��ü�� �����͸� ���� ��ũ��Ʈ�� �����մϴ�.
    /// �� �޼���� SaveManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="data">�ε��� �����Ͱ� ��� PlayerStatsSaveData ��ü</param>
    public void LoadData(object data)
    {
        // �ε�� �����Ͱ� �ùٸ� Ÿ������ Ȯ���մϴ�.
        if (data is PlayerStatsSaveData loadedData)
        {
            // === �ǽð� �ٽ� �ɷ�ġ ���� ===
            this.health = loadedData.health;
            this.mana = loadedData.mana;
            this.attackPower = loadedData.attackPower;
            this.magicAttackPower = loadedData.magicAttackPower;
            this.defense = loadedData.defense;
            this.magicDefense = loadedData.magicDefense;
            this.criticalChance = loadedData.criticalChance;
            this.criticalDamageMultiplier = loadedData.criticalDamageMultiplier;
            this.moveSpeed = loadedData.moveSpeed;

            // === ĳ���� ���� ��Ȳ ���� ===
            this.characterName = loadedData.characterName;
            this.gold = loadedData.gold;
            this.level = loadedData.level;
            this.experience = loadedData.experience;
            this.skillPoints = loadedData.skillPoints;

            // === ��ų ��ųʸ� ���� ===
            this.skillLevels = loadedData.skillLevels;

            // ü��/������ �ִ�ġ�� �ʰ����� �ʵ��� �����ϴ� ������ �߰��ϴ� �͵� ����� �� �ֽ��ϴ�.
            // ��: this.health = Mathf.Min(loadedData.health, this.MaxHealth);
        }
        else
        {
            Debug.LogError("�ε�� ������ Ÿ���� PlayerStatsSaveData�� ��ġ���� �ʽ��ϴ�. (������ �ƴմϴ�. ���� ��ũ��Ʈ �������Դϴ�.)");
        }
    }
}
