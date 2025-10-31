using UnityEngine;
using System.Collections.Generic;
using System; // System.Action을 사용하기 위해 using System 추가
using System.Collections; // 코루틴을 사용하기 위해 추가

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

    // === 귀환 관련 상수 및 필드 (추가된 부분) ===
    /// <summary>
    /// 귀환 주문서 사용 시 딜레이 시간 (초) 입니다.
    /// </summary>
    public const float RETURN_DELAY = 5.0f;
    private Coroutine returnCoroutine;
    [SerializeField] private GameObject returnEffectPrefab; // 귀환 효과 프리팹 참조 (Inspector에서 할당 필요)
    private GameObject currentReturnEffect;

    /// <summary>
    /// 현재 귀환 프로세스(딜레이 코루틴)가 진행 중인지 여부를 나타냅니다. (읽기 전용)
    /// </summary>
    public bool IsReturnProcessActive => returnCoroutine != null;


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

        // 4. 귀환 이펙트 오브젝트 생성 및 초기 비활성화 (추가된 부분)
        if (returnEffectPrefab != null)
        {
            currentReturnEffect.SetActive(false);
        }
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

    // ------------------------------------------------------------------
    // 귀환 딜레이/이펙트 관리 로직 (새로운 기능)
    // ------------------------------------------------------------------

    /// <summary>
    /// [SRP 준수] ReturnScrollSO로부터 요청받은 최종 로직(Action)을 딜레이 후 실행하는 역할만 수행합니다.
    /// PlayerCharacter는 귀환의 세부 로직을 알지 못하며, 단지 타이머 및 이펙트 관리자 역할만 합니다.
    /// </summary>
    /// <param name="finalCallback">딜레이 후에 실행할 ReturnScrollSO의 최종 귀환 로직</param>
    /// <returns>딜레이 시작 성공 여부</returns>
    public bool StartReturnDelay(Action finalCallback)
    {
        // 1. 중복 실행 방지
        if (returnCoroutine != null)
        {
            Debug.LogWarning("[PlayerCharacter] 이미 귀환 프로세스가 진행 중입니다.");
            return false;
        }

        // 2. 딜레이 코루틴 시작
        returnCoroutine = StartCoroutine(HandleReturnDelay(finalCallback));
        return true;
    }

    /// <summary>
    /// 귀환 딜레이를 처리하고 이펙트를 관리하는 코루틴입니다.
    /// </summary>
    private IEnumerator HandleReturnDelay(Action finalCallback)
    {
        // 1. 이펙트 활성화 및 플레이어 행동 제어 (예: 이동/공격 불가 상태 설정)
        if (currentReturnEffect != null)
        {
            currentReturnEffect.SetActive(true);
        }
        // playerController.CanMove = false; // 예시: 플레이어 제어 로직

        // 2. 지정된 시간 동안 대기 (딜레이)
        yield return new WaitForSeconds(RETURN_DELAY);

        // 3. 딜레이 종료 후 이펙트 비활성화 및 제어 해제
        if (currentReturnEffect != null)
        {
            currentReturnEffect.SetActive(false);
        }
        // playerController.CanMove = true; // 예시

        // 4. 귀환 로직 실행 (ReturnScrollSO가 정의한 콜백 실행)
        finalCallback?.Invoke();

        // 5. 코루틴 참조 해제 (프로세스 종료)
        returnCoroutine = null;
    }

    /// <summary>
    /// 딜레이 도중 피격 등으로 귀환 프로세스를 취소할 때 사용합니다.
    /// </summary>
    public void CancelReturn()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;

            // 1. 이펙트 비활성화
            if (currentReturnEffect != null)
            {
                currentReturnEffect.SetActive(false);
            }

            // 2. 플레이어 제어 복구
            playerController.canMove = true; // 예시

            // 3. 알림 표시
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    "[귀환 취소] 피격으로 귀환이 취소되었습니다.",
                    NotificationType.Warning
                );
            }
        }
    }

    // ------------------------------------------------------------------
    // 기존 로직 유지
    // ------------------------------------------------------------------

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