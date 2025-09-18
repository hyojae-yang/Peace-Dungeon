using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어와 관련된 모든 주요 시스템을 관리하는 중앙 허브 스크립트입니다.
/// 싱글턴 패턴으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// 이 스크립트는 자신이 부착된 게임 오브젝트에 존재하는 다른 시스템 스크립트들의 참조를 통합하여 관리하는 역할만 수행합니다.
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    // PlayerCharacter 클래스의 유일한 인스턴스를 저장하는 정적 속성입니다.
    // 외부에서 PlayerCharacter.Instance를 통해 접근할 수 있습니다.
    public static PlayerCharacter Instance;

    // === 참조 시스템 ===
    // 플레이어 시스템의 핵심 컴포넌트들을 담는 공개 멤버 변수들입니다.
    // 이 변수들은 Inspector에서 직접 할당하거나, Awake 메서드에서 자동으로 할당됩니다.
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

    // SkillPointManager는 싱글톤으로 유지됩니다.
    // 따라서 직접 참조 변수는 필요하지 않습니다.

    /// <summary>
    /// 이 스크립트가 Awake될 때 호출되며, 싱글턴 인스턴스를 초기화하고 모든 시스템 컴포넌트를 할당합니다.
    /// </summary>
    private void Awake()
    {
        // 1. 싱글턴 인스턴스 할당 및 중복 인스턴스 파괴
        if (Instance == null)
        {
            Instance = this;
            // 씬이 변경되어도 파괴되지 않게 하려면 아래 주석을 해제하세요.
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter의 인스턴스가 이미 존재합니다. 새 오브젝트를 파괴합니다.");
            Destroy(gameObject);
            return; // 중복 인스턴스이므로 아래 코드는 실행하지 않습니다.
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

        // 3. 필수 컴포넌트 누락 여부 확인 (디버깅 목적)
        ValidateSystemReferences();
    }

    /// <summary>
    /// 모든 시스템 컴포넌트가 정상적으로 할당되었는지 확인합니다.
    /// </summary>
    private void ValidateSystemReferences()
    {
        if (playerStats == null) Debug.LogError("PlayerCharacter: 'PlayerStats' 컴포넌트가 누락되었습니다.");
        if (playerStatSystem == null) Debug.LogError("PlayerCharacter: 'PlayerStatSystem' 컴포넌트가 누락되었습니다.");
        if (inventoryManager == null) Debug.LogError("PlayerCharacter: 'InventoryManager' 컴포넌트가 누락되었습니다.");
        if (playerEquipmentManager == null) Debug.LogError("PlayerCharacter: 'PlayerEquipmentManager' 컴포넌트가 누락되었습니다.");
        if (playerController == null) Debug.LogError("PlayerCharacter: 'PlayerController' 컴포넌트가 누락되었습니다.");
        if (playerAttack == null) Debug.LogError("PlayerCharacter: 'PlayerAttack' 컴포넌트가 누락되었습니다.");
        if (playerHealth == null) Debug.LogError("PlayerCharacter: 'PlayerHealth' 컴포넌트가 누락되었습니다.");
        if (playerLevelUp == null) Debug.LogError("PlayerCharacter: 'PlayerLevelUp' 컴포넌트가 누락되었습니다.");
        if (playerSkillController == null) Debug.LogError("PlayerCharacter: 'PlayerSkillController' 컴포넌트가 누락되었습니다.");

        if (playerStats != null && playerStatSystem != null && playerController != null && playerAttack != null && playerHealth != null && playerLevelUp != null && playerSkillController != null)
        {
        }
    }
}