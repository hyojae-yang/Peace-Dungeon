using UnityEngine;
using System.Collections.Generic;

// 플레이어의 스탯 데이터를 저장하고 관리하는 스크립트입니다.
// 싱글턴 패턴으로 변경하여 어디서든 접근 가능하도록 만듭니다.
public class PlayerStats : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    // private static으로 외부에서 직접 인스턴스를 생성하지 못하게 막습니다.
    private static PlayerStats _instance;

    // public static으로 외부에서 PlayerStats.Instance를 통해 접근할 수 있도록 합니다.
    public static PlayerStats Instance
    {
        get
        {
            // 인스턴스가 아직 생성되지 않았을 때
            if (_instance == null)
            {
                // 씬에서 PlayerStats 컴포넌트를 가진 게임 오브젝트를 찾습니다.
                _instance = FindFirstObjectByType<PlayerStats>();

                // 만약 찾지 못했다면 새로운 게임 오브젝트를 만들고 컴포넌트를 추가합니다.
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("PlayerStatsSingleton");
                    _instance = singletonObject.AddComponent<PlayerStats>();
                    Debug.Log("새로운 'PlayerStatsSingleton' 게임 오브젝트를 생성했습니다.");
                }
            }
            return _instance;
        }
    }

    // === 스크립트가 Awake될 때 싱글턴 초기화 ===
    // 이 스크립트가 Awake될 때마다 호출됩니다.
    void Awake()
    {
        // 만약 이미 인스턴스가 존재하고 이 객체가 그 인스턴스가 아니라면
        if (_instance != null && _instance != this)
        {
            // 중복된 객체이므로 파괴합니다.
            Destroy(gameObject);
        }
        else
        {
            // 이 객체를 유일한 인스턴스로 설정합니다.
            _instance = this;
            // 씬이 변경되어도 이 객체가 파괴되지 않도록 설정합니다.
            // DontDestroyOnLoad(gameObject);
            // 만약 게임 시작 시 이 객체를 이미 씬에 배치했다면 DontDestroyOnLoad는
            // 필요 없을 수도 있습니다. 프로젝트의 구조에 맞게 선택해 주세요.
        }
    }

    // === 기존 스탯 변수들은 그대로 둡니다. ===
    // 이제 이 아래에 있는 기존 변수들은 모두 PlayerStats.Instance.변수명으로 접근하게 됩니다.

    // === 기본 능력치 ===
    [Header("기본 능력치")]
    [Tooltip("플레이어의 시작 체력입니다. PlayerStatSystem 스크립트에서 참조합니다.")]
    public float baseMaxHealth = 100f;
    // ... (나머지 기존 변수들) ...
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
    [Range(0.0f, 1.0f)]
    public float baseEvasionChance = 0.02f;

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