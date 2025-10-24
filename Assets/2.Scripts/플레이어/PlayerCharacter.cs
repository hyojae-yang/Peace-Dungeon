using UnityEngine;
using System.Collections.Generic;
using System; // System.Action을 사용하기 위해 using System 추가

/// <summary>
/// 플레이어와 관련된 모든 주요 시스템을 관리하는 중앙 허브 스크립트입니다.
/// 싱글턴 패턴으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// 이 스크립트는 자신이 부착된 게임 오브젝트에 존재하는 다른 시스템 스크립트들의 참조를 통합하여 관리하는 역할만 수행합니다.
/// SOLID: 단일 책임 원칙 (시스템 참조 허브 역할).
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    // PlayerCharacter 클래스의 유일한 인스턴스를 저장하는 정적 속성입니다.
    public static PlayerCharacter Instance;

    // === 상태 추적 필드 (새로 추가됨) ===
    /// <summary>
    /// 플레이어의 모든 하위 시스템(Stats, Attack, Equipment 등)의 Start() 초기화가
    /// 완료되었는지 여부를 나타냅니다. (Start() 메서드 마지막에 true로 설정됨)
    /// </summary>
    // 💡 [SOLID: 개방-폐쇄 원칙] 외부에서 읽을 수는 있지만(get), 외부에서 설정할 수 없도록(private set) 보호합니다.
    public bool IsInitialized { get; private set; } = false;


    // === 참조 시스템 ===
    [Header("핵심 시스템 참조")]
    [Tooltip("플레이어의 스탯 데이터를 저장 및 관리하는 PlayerStats 컴포넌트입니다.")]
    public PlayerStats playerStats;

    [Tooltip("플레이어의 스탯 시스템을 제어하는 PlayerStatSystem 컴포넌트입니다.")]
    public PlayerStatSystem playerStatSystem;

    [Tooltip("플레이어의 인벤토리 시스템을 참조합니다.")]
    public InventoryManager inventoryManager;

    [Tooltip("플레이어의 장비 관리 시스템을 제어하는 PlayerEquipmentManager 컴포넌트입니다.")]
    public PlayerEquipmentManager playerEquipmentManager;

    [Tooltip("플레이어의 이동을 제어하는 PlayerController 컴포넌트입니다.")]
    public PlayerController playerController;

    [Tooltip("플레이어의 공격을 제어하는 PlayerAttack 컴포넌트입니다.")]
    public PlayerAttack playerAttack;

    [Tooltip("플레이어의 체력 및 데미지 로직을 처리하는 PlayerHealth 컴포넌트입니다.")]
    public PlayerHealth playerHealth;

    [Tooltip("플레이어의 레벨업을 관리하는 PlayerLevelUp 컴포넌트입니다.")]
    public PlayerLevelUp playerLevelUp;

    [Tooltip("플레이어의 스킬 사용 및 관리를 담당하는 PlayerSkillController 컴포넌트입니다.")]
    public PlayerSkillController playerSkillController;

    [Tooltip("플레이어가 습득한 패시브 스킬의 효과를 관리하는 PassiveSkillManager 컴포넌트입니다.")]
    public PassiveSkillManager passiveSkillManager;

    [Tooltip("플레이어 애니메이터")]
    public Animator animator;

    /// <summary>
    /// 모든 하위 시스템(Inventory, Attack, Stats 등)의 초기화가 완료되었을 때 호출되는 이벤트입니다.
    /// 이 이벤트는 IsInitialized 플래그와 동기화됩니다.
    /// </summary>
    public event Action OnAllSystemsInitialized;

    /// <summary>
    /// 이 스크립트가 Awake될 때 호출되며, 싱글턴 인스턴스를 초기화하고 모든 시스템 컴포넌트를 할당합니다.
    /// </summary>
    private void Awake()
    {
        // 1. 싱글턴 인스턴스 할당 및 중복 인스턴스 파괴
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요하다면 주석 해제
        }
        else
        {
            Debug.LogWarning("[PlayerCharacter] PlayerCharacter의 인스턴스가 이미 존재합니다. 새 오브젝트를 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        // 2. 모든 시스템 컴포넌트 자동 할당
        // 모든 스크립트가 같은 게임 오브젝트에 부착되어 있다는 가정하에 GetComponent를 사용합니다.
        playerStats = GetComponent<PlayerStats>();
        playerStatSystem = GetComponent<PlayerStatSystem>();
        inventoryManager = GetComponent<InventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealth = GetComponent<PlayerHealth>();
        playerLevelUp = GetComponent<PlayerLevelUp>();
        playerSkillController = GetComponent<PlayerSkillController>();
        passiveSkillManager = GetComponent<PassiveSkillManager>();
        animator = GetComponent<Animator>();

        // 3. 필수 컴포넌트 누락 여부 확인 (디버깅 목적)
        ValidateSystemReferences();
    }

    private void Start()
    {
        // Start()에서는 코루틴을 시작하여 모든 컴포넌트의 Start()가 완료되기를 기다립니다.
        StartCoroutine(InitializeAfterStart());
    }

    /// <summary>
    /// 모든 컴포넌트의 Start() 메서드가 호출된 후, 다음 프레임에서 최종적으로 초기화를 완료합니다.
    /// </summary>
    private System.Collections.IEnumerator InitializeAfterStart()
    {
        // 1. 최소한 한 프레임을 기다려 이 GameObject에 붙어있는 모든 컴포넌트의 Start() 실행을 보장합니다.
        yield return null;

        // 2. 초기화 완료 플래그를 설정하고 이벤트를 발생시켜 대기 중인 로드 로직을 실행합니다.
        // 이 시점은 PlayerAttack.Start()를 포함한 모든 Start()가 완료된 후이므로 가장 안전합니다.
        IsInitialized = true;
        OnAllSystemsInitialized?.Invoke();
    }

    /// <summary>
    /// 모든 시스템 컴포넌트가 정상적으로 할당되었는지 확인합니다.
    /// </summary>
    private void ValidateSystemReferences()
    {
        if (playerStats == null) Debug.LogError("[PlayerCharacter]: 'PlayerStats' 컴포넌트가 누락되었습니다.");
        if (playerStatSystem == null) Debug.LogError("[PlayerCharacter]: 'PlayerStatSystem' 컴포넌트가 누락되었습니다.");
        if (inventoryManager == null) Debug.LogError("[PlayerCharacter]: 'InventoryManager' 컴포넌트가 누락되었습니다.");
        if (playerEquipmentManager == null) Debug.LogError("[PlayerCharacter]: 'PlayerEquipmentManager' 컴포넌트가 누락되었습니다.");
        if (playerController == null) Debug.LogError("[PlayerCharacter]: 'PlayerController' 컴포넌트가 누락되었습니다.");
        if (playerAttack == null) Debug.LogError("[PlayerCharacter]: 'PlayerAttack' 컴포넌트가 누락되었습니다.");
        if (playerHealth == null) Debug.LogError("[PlayerCharacter]: 'PlayerHealth' 컴포넌트가 누락되었습니다.");
        if (playerLevelUp == null) Debug.LogError("[PlayerCharacter]: 'PlayerLevelUp' 컴포넌트가 누락되었습니다.");
        if (playerSkillController == null) Debug.LogError("[PlayerCharacter]: 'PlayerSkillController' 컴포넌트가 누락되었습니다.");
        if (animator == null) Debug.LogError("[PlayerCharacter]: 'Animator' 컴포넌트가 누락되었습니다.");

        // 필수 컴포넌트 검증이 성공했을 때만 로그를 남길 수도 있습니다.
        if (playerStats != null && playerStatSystem != null && playerController != null && playerAttack != null && playerHealth != null && playerLevelUp != null && playerSkillController != null)
        {
            // Debug.Log("[PlayerCharacter] 모든 핵심 시스템 참조 확인 완료.");
        }
    }
    public void FinalizeStatsAfterLoad()
    {
        // MaxHealth가 최종적으로 계산된 상태에서
        // 현재 체력/마나가 MaxHealth/MaxMana로 채워지도록 보정합니다.
        if (playerStats != null)
        {
            playerStats.health = playerStats.MaxHealth;
            playerStats.mana = playerStats.MaxMana;
        }
    }
    private void OnDestroy()
    {
        // 널 체크를 추가하여 안전성을 높입니다.
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.DeadDungeon();
        }
    }
}