using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 스탯 데이터를 저장하고 관리하는 스크립트입니다.
/// 이 스크립트는 이제 더 이상 싱글턴이 아니며,
/// PlayerCharacter 스크립트의 멤버로 포함되어 관리됩니다.
/// </summary>
public class PlayerStats : MonoBehaviour, ISavable
{
    // === 이벤트 선언 ===
    /// <summary>
    /// 플레이어에게 골드가 추가되거나 차감되었을 때 외부에 알리는 이벤트입니다.
    /// 인수로 골드의 변화량(획득:+양수, 소비:-음수)을 전달합니다.
    /// </summary>
    public event System.Action<int> OnGoldAdded;

    // === 기존 스탯 변수들은 그대로 둡니다. ===
    // 이제 이 아래에 있는 기존 변수들은 모두 PlayerCharacter.Instance.playerStats.변수명으로 접근하게 됩니다.
    public static Dictionary<StatType, float> TimedBuffs;
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
    public string characterName = "홍길동";

    // public int gold = 0; -> private int _gold = 0; 으로 캡슐화
    [SerializeField] // 인스펙터 창에서 값을 확인하기 위해 SerializeField를 사용합니다.
    private int _gold = 0;

    /// <summary>
    /// 플레이어가 보유한 현재 골드량입니다. (읽기 전용)
    /// 외부에서는 이 속성을 통해 현재 골드량을 조회해야 합니다.
    /// </summary>
    public int Gold => _gold;

    public int level = 1;
    public long experience = 0;
    public long requiredExperience = 10;

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

    private void Awake()
    {
        // ①. ISavable 인터페이스를 구현한 이 객체를 SaveManager에 등록합니다.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }
    }

    // 골드를 획득할 때 호출하는 메서드
    /// <summary>
    /// 골드를 획득할 때 호출하는 메서드입니다.
    /// 이 메서드는 골드 값을 변경하고, UI 피드백을 위해 이벤트를 발생시키는 책임을 가집니다.
    /// </summary>
    /// <param name="amount">추가할 골드량 (양수여야 합니다.)</param>
    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            //Debug.LogWarning("AddGold 메서드에는 양수만 전달해야 합니다. 지출 시에는 SpendGold를 사용하세요.");
            return;
        }

        _gold += amount;
        OnGoldAdded?.Invoke(amount); // UI 알림을 위해 이벤트 발생
    }

    // 골드를 지출할 수 있는지 확인하는 메서드
    /// <summary>
    /// 골드를 지출할 수 있는지 확인합니다. (상점/제작 등)
    /// </summary>
    /// <param name="cost">지출할 비용</param>
    /// <returns>골드가 비용보다 크거나 같으면 true</returns>
    public bool CanAfford(int cost)
    {
        return _gold >= cost;
    }

    // 골드를 지출할 때 호출하는 메서드
    /// <summary>
    /// 골드를 지출할 때 호출하는 메서드입니다. (상점/제작 등)
    /// </summary>
    /// <param name="amount">지출할 골드량 (양수여야 합니다.)</param>
    /// <returns>지출에 성공하면 true, 골드 부족 등으로 실패하면 false</returns>
    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("SpendGold 메서드에는 양수만 전달해야 합니다. 획득 시에는 AddGold를 사용하세요.");
            return false;
        }

        if (!CanAfford(amount))
        {
            Debug.LogWarning($"골드 지출 실패: 현재 골드({_gold})가 요구 비용({amount})보다 부족합니다.");
            return false;
        }

        _gold -= amount;
        OnGoldAdded?.Invoke(-amount); // 차감된 골드량은 음수로 전달

        return true;
    }

    // === ISavable 인터페이스 구현 ===
    /// <summary>
    /// 현재 스크립트의 데이터를 SaveData 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        PlayerStatsSaveData data = new PlayerStatsSaveData
        {
            // === 실시간 핵심 능력치 ===
            health = this.health,
            mana = this.mana,
            attackPower = this.attackPower,
            magicAttackPower = this.magicAttackPower,
            defense = this.defense,
            magicDefense = this.magicDefense,
            criticalChance = this.criticalChance,
            criticalDamageMultiplier = this.criticalDamageMultiplier,
            moveSpeed = this.moveSpeed,

            // === 캐릭터 진행 상황 ===
            characterName = this.characterName,
            // 캡슐화된 _gold 변수 값을 저장합니다. (기존: this.gold -> this._gold)
            gold = this._gold,
            level = this.level,
            experience = this.experience,
            skillPoints = this.skillPoints,

            // === 스킬 딕셔너리 ===
            skillLevels = this.skillLevels
        };
        return data;
    }

    /// <summary>
    /// SaveData 객체의 데이터를 현재 스크립트에 적용합니다.
    /// </summary>
    public void LoadData(object data)
    {
        // 로드된 데이터가 올바른 타입인지 확인합니다.
        if (data is PlayerStatsSaveData loadedData)
        {
            // === 실시간 핵심 능력치 복구 ===
            this.health = loadedData.health;
            this.mana = loadedData.mana;
            this.attackPower = loadedData.attackPower;
            this.magicAttackPower = loadedData.magicAttackPower;
            this.defense = loadedData.defense;
            this.magicDefense = loadedData.magicDefense;
            this.criticalChance = loadedData.criticalChance;
            this.criticalDamageMultiplier = loadedData.criticalDamageMultiplier;
            this.moveSpeed = loadedData.moveSpeed;

            // === 캐릭터 진행 상황 복구 ===
            this.characterName = loadedData.characterName;
            // 로드된 데이터를 _gold 변수에 복구합니다. (기존: this.gold -> this._gold)
            this._gold = loadedData.gold;
            this.level = loadedData.level;
            this.experience = loadedData.experience;
            this.skillPoints = loadedData.skillPoints;

            // === 스킬 딕셔너리 복구 ===
            this.skillLevels = loadedData.skillLevels;
        }
        else
        {
            Debug.LogError("로드된 데이터 타입이 PlayerStatsSaveData와 일치하지 않습니다. (오류가 아닙니다. 다음 스크립트 데이터입니다.)");
        }
    }
}