using System;
using System.Collections.Generic;
using UnityEngine;

// --------------------------------------------------------------------------------------
// 0. 몬스터 데이터 구조 (외부에 존재함을 가정합니다.)
// --------------------------------------------------------------------------------------
// public class MonsterData { ... } // (실제 코드는 이 파일에 없음을 가정)


// --------------------------------------------------------------------------------------
// 1. 캐릭터 능력치 관리 (SRP: CharacterStat Management)
// --------------------------------------------------------------------------------------
public class CharacterStats
{
    // === 상수: 레벨업 및 경험치 ===
    private const float BASE_XP_REQUIRED = 10f;
    private const float XP_INCREASE_RATE = 1.4f;

    // === 상수: 스탯 포인트당 능력치 상승 값 (사용자님의 정확한 정의 반영) ===
    // Strength (힘)
    private const float STR_TO_AP = 2f; // 힘 1포인트당 공격력 증가량
    private const float STR_TO_CRITICAL_DAMAGE = 0.01f; // 힘 1포인트당 치명타 피해량 증가량 (0.01 = 1%)

    // Intelligence (지능)
    private const float INT_TO_MAP = 2.5f; // 지능 1포인트당 마법 공격력 증가량

    // Constitution (체질)
    private const float CON_TO_HP = 10f; // 체질 1포인트당 최대 체력 증가량
    private const float CON_TO_DEFENSE = 1f; // 체질 1포인트당 방어력 증가량

    // Agility (민첩)
    private const float AGI_TO_MOVE_SPEED = 0.2f; // 민첩 1포인트당 이동 속도 증가량

    // Focus (집중력)
    private const float FOCUS_TO_MAP = 0.5f; // 집중력 1포인트당 마법 공격력 증가량
    private const float FOCUS_TO_CRITICAL_CHANCE = 0.001f; // 집중력 1포인트당 치명타 확률 증가량 (0.001 = 0.1%)

    // Endurance (인내력)
    private const float END_TO_MAX_MANA = 5f; // 인내력 1포인트당 최대 마나 증가량
    private const float END_TO_MDEF = 1f; // 인내력 1포인트당 마법 방어력 증가량

    // Vitality (활력)
    private const float VITA_TO_MAX_HP = 5f; // 활력 1포인트당 최대 체력 증가량
    private const float VITA_TO_MOVE_SPEED = 0.1f; // 활력 1포인트당 이동 속도 증가량

    // === 캐릭터 능력치 변수 (자동 구현 속성으로 유지) ===
    public int Level { get; private set; }
    public float MaxHealth { get; private set; }
    public float MaxMana { get; private set; }
    public float AttackPower { get; private set; }
    public float Defense { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackRate { get; private set; }
    public float MagicDefense { get; private set; }
    public float MagicAttackPower { get; private set; }
    public float CriticalChance { get; private set; }
    public float CriticalDamageMultiplier { get; private set; } = 1.5f;
    public float BaseAttackRate { get; private set; }


    // === 빌드 정보 출력용 (CS0206 오류 해결을 위해 private set을 제거하고 public field로 변경) ===
    // 주의: 외부에서 직접 대입하지 않도록 관리되어야 합니다.
    public float StrengthStat;
    public float ConstitutionStat;
    public float AgilityStat;
    public float IntelligenceStat;
    public float FocusStat;
    public float EnduranceStat;
    public float VitalityStat;


    /// <summary>
    /// 목표 레벨과 스탯 분배에 따라 캐릭터의 최종 능력치를 계산합니다.
    /// </summary>
    /// <param name="targetLevel">계산할 목표 레벨입니다.</param>
    /// <param name="statDistribution">스탯 이름과 투자 포인트가 담긴 딕셔너리입니다. (8개 스탯 포함)</param>
    /// <param name="baseAttackRate">기본 공격 속도입니다.</param>
    public CharacterStats(int targetLevel, Dictionary<string, int> statDistribution, float baseAttackRate = 1.0f)
    {
        Level = targetLevel;
        AttackRate = baseAttackRate;
        BaseAttackRate = baseAttackRate;
        CriticalDamageMultiplier = 1.5f; // 기본 치명타 배율 초기화

        // 초기 능력치 (Lvl 1 기준)
        MaxHealth = 100f; MaxMana = 50f; AttackPower = 10f; Defense = 5f; MoveSpeed = 5f;
        MagicDefense = 5f; MagicAttackPower = 5f; CriticalChance = 0.05f;

        // 1. 레벨업 시 자동 증가분 계산
        int levelIncreaseCount = targetLevel - 1;
        MaxHealth += levelIncreaseCount * 10f;
        AttackPower += levelIncreaseCount * 2f;
        Defense += levelIncreaseCount * 1f;
        MagicDefense += levelIncreaseCount * 1f;
        MagicAttackPower += levelIncreaseCount * 1f;
        MaxMana += levelIncreaseCount * 5f;

        // 2. 스탯 포인트 분배 적용 및 스탯 저장
        ApplyStatDistribution(statDistribution);
    }

    /// <summary>
    /// 전달받은 스탯 배분 딕셔너리를 사용하여 캐릭터의 능력치를 업데이트합니다. (OCP 준수)
    /// </summary>
    /// <param name="statDistribution">스탯 이름과 투자 포인트 딕셔너리입니다.</param>
    private void ApplyStatDistribution(Dictionary<string, int> statDistribution)
    {
        // 헬퍼 함수: 딕셔너리에서 값을 안전하게 가져와 포인트에 할당하고, 총합에 반영합니다.
        // statField를 ref로 전달하기 위해, 해당 변수는 필드(Field)여야 합니다.
        int GetStatPoint(string statName, ref float statField)
        {
            if (statDistribution.TryGetValue(statName, out int points))
            {
                statField = points;
                return points;
            }
            statField = 0f;
            return 0;
        }

        // Strength (힘)
        int str = GetStatPoint("Strength", ref StrengthStat);
        AttackPower += str * STR_TO_AP;
        CriticalDamageMultiplier += str * STR_TO_CRITICAL_DAMAGE;

        // Constitution (체질)
        int con = GetStatPoint("Constitution", ref ConstitutionStat);
        MaxHealth += con * CON_TO_HP;
        Defense += con * CON_TO_DEFENSE;

        // Agility (민첩)
        int agi = GetStatPoint("Agility", ref AgilityStat);
        MoveSpeed += agi * AGI_TO_MOVE_SPEED;

        // Intelligence (지능)
        int intel = GetStatPoint("Intelligence", ref IntelligenceStat);
        MagicAttackPower += intel * INT_TO_MAP;

        // Focus (집중력)
        int focus = GetStatPoint("Focus", ref FocusStat);
        MagicAttackPower += focus * FOCUS_TO_MAP;
        CriticalChance += focus * FOCUS_TO_CRITICAL_CHANCE;

        // Endurance (인내력)
        int end = GetStatPoint("Endurance", ref EnduranceStat);
        MaxMana += end * END_TO_MAX_MANA;
        MagicDefense += end * END_TO_MDEF;

        // Vitality (활력)
        int vita = GetStatPoint("Vitality", ref VitalityStat);
        MaxHealth += vita * VITA_TO_MAX_HP;
        MoveSpeed += vita * VITA_TO_MOVE_SPEED;
    }


    /// <summary>
    /// 지정된 시작 레벨부터 목표 레벨까지 도달하는 데 필요한 총 경험치를 계산합니다.
    /// </summary>
    public static float CalculateTotalXPRequired(int startLevel, int targetLevel)
    {
        if (startLevel >= targetLevel) return 0f;

        float totalXP = 0f;
        for (int i = startLevel; i < targetLevel; i++)
        {
            totalXP += BASE_XP_REQUIRED * Mathf.Pow(XP_INCREASE_RATE, i - 1);
        }
        return totalXP;
    }
}

// --------------------------------------------------------------------------------------
// 2. 몬스터 전투 로직 (SRP: Combat Logic)
// (변경 사항 없음)
// --------------------------------------------------------------------------------------

public class BalanceCombatModel
{
    // MonsterData 타입은 외부 클래스를 참조
    private MonsterData monsterData;

    private struct Potion
    {
        public string Name;
        public float Cost;
        public float HealAmount;
    }

    private readonly Potion[] potionList = new Potion[]
    {
        new Potion { Name = "소짜리", Cost = 120f, HealAmount = 100f },
        new Potion { Name = "중짜리", Cost = 360f, HealAmount = 300f },
        new Potion { Name = "대짜리", Cost = 600f, HealAmount = 500f }
    };

    private const float TREE_SPIRIT_BIND_TIME_COST = 5.0f;
    private const float WOLF_REINFORCEMENT_COUNT = 0.5f;
    private const float BEAR_AOE_COOLDOWN = 10.0f;

    public BalanceCombatModel(MonsterData data, float currentPotionPricePlaceholder)
    {
        if (data == null)
        {
            Debug.LogError("MonsterData가 Null입니다. 시뮬레이션을 실행할 수 없습니다.");
            return;
        }
        monsterData = data;
    }

    /// <summary>
    /// 플레이어의 스탯을 기준으로 몬스터 한 마리를 잡는 데 걸리는 평균 시간을 계산합니다. (패턴 비용 포함)
    /// </summary>
    public float CalculateAverageTimeToKill(CharacterStats playerStats)
    {
        if (monsterData == null) return float.MaxValue;

        // 1. 기본 TTK (공격 시간) 계산 - 치명타 계산 로직 포함
        float rawDamage = playerStats.AttackPower - monsterData.defense;
        float criticalHitDamage = rawDamage * playerStats.CriticalDamageMultiplier;

        float avgDamage = (Mathf.Max(1f, rawDamage) * (1f - playerStats.CriticalChance)) +
                          (Mathf.Max(1f, criticalHitDamage) * playerStats.CriticalChance);

        int hitsToKill = Mathf.CeilToInt(monsterData.maxHealth / avgDamage);
        float attackDelay = 1f / playerStats.AttackRate;
        float tAttack = hitsToKill * attackDelay;
        float finalATTK = tAttack;

        // 2. 몬스터 행동 패턴에 따른 시간 비용 추가 (로직 변경 없음)
        if (monsterData.monsterName.Contains("토끼"))
        {
            float stealthSuccessRate = 0.3f;
            float chaseTimeAssumption = 4.0f;
            float tSuccess = tAttack;
            float tFailure = chaseTimeAssumption + tAttack;
            finalATTK = (stealthSuccessRate * tSuccess) + ((1f - stealthSuccessRate) * tFailure);
        }
        else if (monsterData.monsterName.Contains("나무정령"))
        {
            finalATTK += TREE_SPIRIT_BIND_TIME_COST;
        }
        else if (monsterData.monsterName.Contains("늑대"))
        {
            float reinforcementDamage = avgDamage * WOLF_REINFORCEMENT_COUNT;
            float reinforcementHits = Mathf.CeilToInt(monsterData.maxHealth * 0.7f / reinforcementDamage);

            float reinforcementTimeCost = (reinforcementHits * attackDelay) * WOLF_REINFORCEMENT_COUNT * 0.3f;
            finalATTK += reinforcementTimeCost;
        }
        else if (monsterData.monsterName.Contains("사슴") || monsterData.monsterName.Contains("산양"))
        {
            finalATTK *= 1.2f;
        }

        return finalATTK;
    }

    public float CalculateXPPerMinute(CharacterStats playerStats)
    {
        if (monsterData == null) return 0f;
        float attk = CalculateAverageTimeToKill(playerStats);
        float killsPerMinute = 60f / attk;
        float avgXP = (monsterData.minExpReward + monsterData.maxExpReward) / 2f;
        return killsPerMinute * avgXP;
    }

    public float CalculateNetGoldPerMinute(CharacterStats playerStats)
    {
        if (monsterData == null) return 0f;
        float attk = CalculateAverageTimeToKill(playerStats);
        float killsPerMinute = 60f / attk;
        float avgGold = (monsterData.minGoldReward + monsterData.maxGoldReward) / 2f;
        float totalGold = killsPerMinute * avgGold;

        // --- 위험 비용 (물약 비용) 계산 ---
        float effectiveDamageTaken = 0f;

        // 1. 일반 물리 공격 피해 (공격 1회당)
        float physicalDamage = Mathf.Max(0f, monsterData.attackPower - playerStats.Defense);
        effectiveDamageTaken += physicalDamage;

        // 2. 곰의 마법 범위 공격 피해 (TTK당)
        if (monsterData.monsterName.Contains("곰"))
        {
            float magicDamagePerAttack = Mathf.Max(1f, monsterData.magicAttackPower - playerStats.MagicDefense);
            float numAoEAttacks = attk / BEAR_AOE_COOLDOWN;
            effectiveDamageTaken += numAoEAttacks * magicDamagePerAttack;
        }

        // 3. 산양의 돌진 피해 (TTK당)
        if (monsterData.monsterName.Contains("산양"))
        {
            float ramTotalDamage = (monsterData.attackPower * 1.5f) - playerStats.Defense;
            float ramExtraDamage = Mathf.Max(0f, ramTotalDamage - physicalDamage);
            effectiveDamageTaken += ramExtraDamage * 0.25f;
        }

        // --- 피해량 기반 물약 계산 로직 (SRP: 물약/회복 책임) ---
        if (effectiveDamageTaken <= 0f)
        {
            return totalGold; // 무위험 파밍
        }

        float damagePerKill = effectiveDamageTaken;
        Potion bestPotion = potionList[0];

        // 가장 큰 물약부터 순회하여 필요한 최소 횟수를 계산
        for (int i = potionList.Length - 1; i >= 0; i--)
        {
            if (potionList[i].HealAmount >= damagePerKill)
            {
                bestPotion = potionList[i];
                break;
            }
        }

        // 만약 소짜리(100)로도 회복이 안 된다면, 가장 저렴한 소짜리로 여러 번 쓴다고 가정 (물약 사용 횟수 증가)
        if (damagePerKill > bestPotion.HealAmount)
        {
            bestPotion = potionList[0];
            float numUsesPerKill = Mathf.Ceil(damagePerKill / bestPotion.HealAmount);
            float cost = (numUsesPerKill * bestPotion.Cost) * killsPerMinute; // 분당 비용
            return totalGold - cost;
        }

        // 몬스터 처치당 1번 사용 비용
        float costPerKill = bestPotion.Cost;
        float totalCost = costPerKill * killsPerMinute; // 분당 비용

        return totalGold - totalCost;
    }

    public int MonsterAttackPower => monsterData.attackPower;
    public string MonsterName => monsterData.monsterName;
}

// --------------------------------------------------------------------------------------
// 3. 메인 시뮬레이션 실행 클래스
// (변경 사항 없음)
// --------------------------------------------------------------------------------------
public class BalanceSimulator : MonoBehaviour
{
    [Header("시뮬레이션 레벨 범위 설정")]
    [Tooltip("시뮬레이션 시작 레벨 (포함)")]
    [SerializeField] public int startLevel = 1;
    [Tooltip("시뮬레이션 종료 레벨 (포함)")]
    [SerializeField] public int endLevel = 20;

    [Header("몬스터 데이터 에셋 연결")]
    [Tooltip("비교할 첫 번째 몬스터 데이터 에셋")]
    [SerializeField] private MonsterData monsterData1;
    [Tooltip("비교할 두 번째 몬스터 데이터 에셋")]
    [SerializeField] private MonsterData monsterData2;

    private readonly Dictionary<string, int> agilityBuild = new Dictionary<string, int>(8)
    {
        { "Strength", 30 }, { "Agility", 65 }, { "Constitution", 0 }, { "Intelligence", 0 },
        { "Focus", 0 }, { "Endurance", 0 }, { "Vitality", 0 }
    };
    private readonly Dictionary<string, int> constitutionBuild = new Dictionary<string, int>(8)
    {
        { "Strength", 40 }, { "Constitution", 55 }, { "Agility", 0 }, { "Intelligence", 0 },
        { "Focus", 0 }, { "Endurance", 0 }, { "Vitality", 0 }
    };
    private readonly Dictionary<string, int> intelligenceBuild = new Dictionary<string, int>(8)
    {
        { "Strength", 10 }, { "Constitution", 30 }, { "Agility", 0 }, { "Intelligence", 55 },
        { "Focus", 0 }, { "Endurance", 0 }, { "Vitality", 0 }
    };
    private readonly Dictionary<string, int> balancedBuild = new Dictionary<string, int>(8)
    {
        { "Strength", 25 }, { "Constitution", 25 }, { "Agility", 25 }, { "Intelligence", 20 },
        { "Focus", 0 }, { "Endurance", 0 }, { "Vitality", 0 }
    };
    private readonly Dictionary<string, int> magicBuild = new Dictionary<string, int>(8)
    {
        { "Strength", 0 }, { "Constitution", 15 }, { "Agility", 0 }, { "Intelligence", 40 },
        { "Focus", 30 }, { "Endurance", 10 }, { "Vitality", 0 }
    };


    public void Start()
    {
        RunSimulation();
    }

    /// <summary>
    /// 시뮬레이션을 실행하고 결과를 콘솔에 출력합니다. (5개 빌드 vs 2개 몬스터)
    /// </summary>
    public void RunSimulation()
    {
        if (monsterData1 == null || monsterData2 == null)
        {
            Debug.LogError("비교할 몬스터 데이터 에셋 2개를 모두 연결해주세요!");
            return;
        }

        if (startLevel >= endLevel)
        {
            Debug.LogError("시작 레벨은 종료 레벨보다 작아야 합니다.");
            return;
        }

        float totalXP = CharacterStats.CalculateTotalXPRequired(startLevel, endLevel);

        Debug.Log($"=== 복합 패턴 밸런스 시뮬레이션 결과 (Lvl {startLevel} -> Lvl {endLevel}) ===");
        Debug.Log($"* 총 필요 경험치: {totalXP:F2} XP");
        Debug.Log("----------------------------------------------------------");

        var simulations = new List<(string buildName, Dictionary<string, int> stats)>
        {
            ("물리: 민첩/힘 (AGI/STR)", agilityBuild),
            ("물리: 체질/힘 (CON/STR)", constitutionBuild),
            ("물리: 균형 (Balanced)", balancedBuild),
            ("혼합: 지능/체질 (INT/CON)", intelligenceBuild),
            ("마법: 집중력/인내력 (FOCUS/END)", magicBuild)
        };

        foreach (var sim in simulations)
        {
            CharacterStats playerStats = new CharacterStats(endLevel, sim.stats);

            SimulateAndLog(playerStats, new BalanceCombatModel(monsterData1, 0), sim.buildName);

            Debug.Log("------------------ 몬스터 전환 -------------------");
            SimulateAndLog(playerStats, new BalanceCombatModel(monsterData2, 0), sim.buildName);
            Debug.Log("==========================================================");
        }

        Debug.Log($"[최종 진단] {monsterData1.monsterName} 공격력({monsterData1.attackPower}), {monsterData2.monsterName} 공격력({monsterData2.attackPower}), {monsterData2.monsterName} 마법 공격력({monsterData2.magicAttackPower})");

        CharacterStats finalPlayerCon = new CharacterStats(endLevel, constitutionBuild);
        if (finalPlayerCon.Defense < monsterData2.attackPower)
        {
            Debug.LogWarning($"[경고] 체질 빌드(방어력 {finalPlayerCon.Defense:F0})도 {monsterData2.monsterName}의 일반 공격({monsterData2.attackPower})에 크게 노출됩니다!");
        }
    }

    /// <summary>
    /// 단일 빌드와 단일 몬스터에 대한 시뮬레이션을 실행하고 결과를 콘솔에 출력합니다.
    /// </summary>
    private void SimulateAndLog(CharacterStats playerStats, BalanceCombatModel combatModel, string buildName)
    {
        float totalXP = CharacterStats.CalculateTotalXPRequired(startLevel, endLevel);
        float xpPM = combatModel.CalculateXPPerMinute(playerStats);
        float timeToLevel = totalXP / xpPM;
        float goldPM = combatModel.CalculateNetGoldPerMinute(playerStats);

        string defenseDiagnosis = (playerStats.Defense >= combatModel.MonsterAttackPower)
                                ? "(물리 안전)"
                                : $"(물리 위험! 피해 {Mathf.Max(1f, combatModel.MonsterAttackPower - playerStats.Defense):F0})";

        Debug.Log($"[** 몬스터: {combatModel.MonsterName} | 빌드: {buildName} **] {defenseDiagnosis}");
        Debug.Log($"  - 분당 XP 획득량: {xpPM:F2} XP/분 (최종 레벨 효율)");
        Debug.Log($"  - Lvl {startLevel} -> Lvl {endLevel} 도달 시간: {timeToLevel:F2} 분");
        Debug.Log($"  - 분당 순수익 골드: {goldPM:F2} 골드/분 (최종 레벨 효율)");
        Debug.Log($"  - 최종 스탯: STR {playerStats.StrengthStat}, CON {playerStats.ConstitutionStat}, AGI {playerStats.AgilityStat}, INT {playerStats.IntelligenceStat}, FOC {playerStats.FocusStat}, END {playerStats.EnduranceStat}, VITA {playerStats.VitalityStat}");
        Debug.Log($"  - 최종 능력치: 공격력 {playerStats.AttackPower:F0}, 마법공격력 {playerStats.MagicAttackPower:F0}, 방어력 {playerStats.Defense:F0}, 마법방어력 {playerStats.MagicDefense:F0}, 치명타율 {playerStats.CriticalChance:P1}, 치명타피해 {playerStats.CriticalDamageMultiplier:F2}, 최대체력 {playerStats.MaxHealth:F0}");
    }
}