using System;
using System.Collections.Generic;
using UnityEngine;

// =======================================================
// [참고] 인터페이스 및 SaveData 클래스는 변경 없이 유지합니다.
// =======================================================
/// <summary>
/// 보스 처치 이벤트를 DungeonManager에 알리는 역할을 담당하는 인터페이스입니다.
/// </summary>
public interface IBossNotifier
{
    /// <summary>
    /// 보스 몬스터가 사망했을 때 호출될 알림 메서드입니다.
    /// </summary>
    void NotifyBossDefeated();
}
/// <summary>
/// 보스 몬스터에게 DungeonManager(IBossNotifier 구현체)를 주입하는 역할을 정의하는 인터페이스입니다.
/// 이 인터페이스는 보스의 초기화(Notifier 설정 및 환경 설정) 책임만 가집니다. (단일 책임 원칙 준수)
/// </summary>
public interface IBossInitializer
{
    /// <summary>
    /// 이 보스 몬스터에게 처치 알림을 받을 객체(Notifier)를 설정합니다.
    /// DungeonManager.SpawnBoss()에서 호출되며, Notifier 객체를 주입합니다.
    /// </summary>
    /// <param name="notifier">IBossNotifier 인터페이스를 구현한 객체 (예: DungeonManager 인스턴스)</param>
    void SetNotifier(IBossNotifier notifier);

    /// <summary>
    /// 이 보스 몬스터에게 특수 공격(뿌리 소환)에 사용될 영역 BoxCollider를 주입합니다.
    /// (DungeonManager.SpawnBoss()에서 호출되어 소환 영역 Collider 객체를 주입합니다. DIP 준수)
    /// </summary>
    /// <param name="collider">씬에 존재하는 뿌리 소환 영역 BoxCollider 컴포넌트</param>
    void SetSummonArea(Collider collider); // <-- [추가] DIP를 위한 계약 확장
    // =========================================================================
    // [핵심 추가] 강제 파괴를 알리는 계약 확장
    // =========================================================================
    /// <summary>
    /// DungeonManager에서 강제로 보스를 파괴하기 직전에 호출됩니다.
    /// 보스에게 던전 클리어 알림(NotifyBossDefeated)을 생략하도록 지시합니다.
    /// </summary>
    void PrepareForForcedDestroy(); // 이 계약이 추가되어야 합니다.
    // =========================================================================
}


public class DungeonManager : MonoBehaviour, IBossNotifier, ISavable
{
    public static DungeonManager Instance { get; private set; }
    /// <summary>
    /// 현재 씬에 존재하는 모든 활성 DungeonSpawnManager 인스턴스들을 추적하는 리스트입니다.
    /// 플레이어가 던전에 진입하면 이 리스트의 모든 매니저에게 몬스터 스폰을 명령합니다.
    /// (다중 던전 구역 동시 지원을 위해 List로 변경)
    /// </summary>
    private List<DungeonSpawnManager> activeSpawnManagers = new List<DungeonSpawnManager>();

    // =======================================================
    // [핵심 추가] 펫/UI/기타 시스템을 위한 던전 상태 이벤트 (Publisher 역할)
    // =======================================================
    /// <summary>
    /// 플레이어가 던전에 진입했을 때(IsInDungeon이 true로 설정될 때) 호출되는 이벤트입니다.
    /// MangChi 펫의 파밍 코루틴 시작 등에 사용됩니다.
    /// </summary>
    public static event Action OnDungeonEnter;

    /// <summary>
    /// 플레이어가 던전에서 퇴장했을 때(ExitDungeon 또는 DeadDungeon 호출 후) 호출되는 이벤트입니다.
    /// MangChi 펫의 파밍 코루틴 중지 등에 사용됩니다.
    /// </summary>
    public static event Action OnDungeonExit;
    //보스 처치 알림 이벤트
    public event Action OnBossDefeated;
    // =======================================================
    private bool _isInDungeon = false;
    /// <summary>
    /// 현재 플레이어가 던전 안에 있는지(true) 밖에 있는지(false)를 나타냅니다.
    /// 이 프로퍼티에 값을 할당하면 던전 진입에 필요한 로직이 자동으로 실행됩니다.
    /// </summary>
    public bool _isBossRoomActive = false;
    // [추가] 던전 클리어 상태를 나타내는 프로퍼티
    private bool _isDungeonCleared = false;
    /// <summary>
    /// 보스 몬스터가 처치되어 던전 클리어가 완료된 상태인지 여부를 나타냅니다.
    /// 이 상태는 BossRoomDoor에서 퇴장 상호작용을 활성화하는 데 사용됩니다.
    /// </summary>
    public bool IsDungeonCleared
    {
        get { return _isDungeonCleared; }
        // private set으로 설정하여 DungeonManager 내부에서만 상태를 변경할 수 있도록 합니다. (캡슐화, SRP 준수)
        private set { _isDungeonCleared = value; }
    }
    [Header("보스룸 설정")]
    [Tooltip("소환할 보스 몬스터 프리팹.")]
    [SerializeField] private GameObject bossPrefab;

    [Tooltip("보스 몬스터가 소환될 위치.")]
    [SerializeField] private Transform bossSpawnPoint;
    // [추가] 보스 특수 공격(뿌리 소환) 영역 BoxCollider 필드
    [Header("보스 특수 공격 설정 (ForestBoss용)")]
    [Tooltip("뿌리 소환 공격에 사용할 평면 영역 BoxCollider를 할당합니다. (씬 오브젝트)")]
    [SerializeField] private Collider rootSummonAreaCollider; // <-- [추가] 씬 오브젝트 참조용

    // 소환된 보스 인스턴스를 추적하기 위한 변수 (나중에 보스 처치 여부를 알기 위해 사용)
    public GameObject currentBossInstance;
    // [추가] 현재 소환된 보스의 ID를 추적하기 위한 필드
    [Header("보스 추적")]
    [Tooltip("현재 소환된 보스 몬스터의 고유 ID입니다. (MonsterData.monsterID)")]
    private int currentBossID = 0; // 초기값은 0 또는 유효하지 않은 값으로 설정
    // [추가] 보스 최초 처치 기록을 메모리에 임시로 보관하는 딕셔너리
    // 이 데이터가 SaveManager를 통해 영구 저장됩니다.
    private Dictionary<int, bool> bossFirstKillRecords = new Dictionary<int, bool>();
    private IDungeonRiskSystem riskSystem;
    public bool IsInDungeon
    {
        get { return _isInDungeon; }
        set
        {
            // 현재 상태와 다른 값으로 변경될 때만 로직을 실행하여 불필요한 호출을 방지합니다.
            if (_isInDungeon != value)
            {
                _isInDungeon = value;

                if (_isInDungeon)
                {
                    // 던전 진입 시 몬스터를 스폰하는 메서드를 호출합니다.
                    HandleDungeonEntry();
                    // =======================================================
                    // [핵심 추가] 던전 진입 이벤트 호출
                    // =======================================================
                    OnDungeonEnter?.Invoke();
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayBGM(BGMType.Main_B, 1.0f);
                    }
                    // =======================================================
                }
                // 기존의 HandleDungeonExit() 호출 로직은 DungeonDoor.cs로 이동되었습니다.
                // 던전 퇴장 시 필요한 로직(몬스터 정리, 보상)은 ExitDungeon() 메서드에서 처리됩니다.
            }
        }
    }
    public GameObject me;
    /// <summary>
    /// 게임 시작 시 한 번 호출되며, 싱글톤 패턴을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // SaveManager에 자신을 등록 (LoadData를 위함)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }
        if (DungeonRiskManager.Instance != null)
        {
            riskSystem = DungeonRiskManager.Instance;
            // Debug.Log("[DungeonManager] DungeonRiskManager 시스템 연결 완료.");
        }
        else
        {
            Debug.LogError("[DungeonManager] DungeonRiskManager 인스턴스를 찾을 수 없습니다! 위험도 시스템 비활성화됨.");
        }
        me.SetActive(false);
        if (this.bossFirstKillRecords.Count > 0) // 키 '0'을 찾아 에러를 낼 필요 없이, 데이터의 존재 유무만 확인
        {
            me.SetActive(true);
        }
    }
    /// <summary>
    /// 현재 던전에 맞는 DungeonSpawnManager를 **리스트에 추가**로 등록합니다.
    /// 이 메서드는 DungeonSpawnManager의 Awake나 Start에서 호출되어 자신을 등록합니다.
    /// (중복 등록 방지 로직 포함)
    /// </summary>
    /// <param name="manager">현재 던전의 스폰 매니저 오브젝트.</param>
    public void RegisterSpawnManager(DungeonSpawnManager manager)
    {
        // 중복 등록 방지 로직 추가 (방어적 프로그래밍)
        if (!activeSpawnManagers.Contains(manager))
        {
            activeSpawnManagers.Add(manager);
        }
        else
        {
            // 이미 등록된 경우 중복 등록 메시지를 출력할 수도 있습니다.
            Debug.LogWarning($"DungeonSpawnManager '{manager.name}'은(는) 이미 등록되어 있습니다.");
        }
    }
    /// <summary>
    /// 현재 등록된 DungeonSpawnManager를 **리스트에서 해제**합니다.
    /// 이 메서드는 DungeonSpawnManager의 OnDestroy 등에서 호출되어 자신을 해제합니다.
    /// </summary>
    /// <param name="manager">해제할 스폰 매니저 오브젝트.</param>
    public void UnregisterSpawnManager(DungeonSpawnManager manager)
    {
        if (activeSpawnManagers.Contains(manager))
        {
            activeSpawnManagers.Remove(manager);
        }
    }

    /// <summary>
    /// 플레이어가 던전에 진입했을 때 실행되는 로직입니다.
    /// 등록된 **모든** 스폰 매니저에게 몬스터 스폰을 명령합니다.
    /// </summary>
    private void HandleDungeonEntry()
    {
        // =======================================================
        // [핵심 추가] 1. 던전 입장 시 위험도 시스템에 횟수 증가 요청 (단 하나의 기능)
        // =======================================================
        if (riskSystem != null)
        {
            // 몬스터 스폰 데이터를 취합하는 임시 로직 (DungeonSpawnManager가 던전 종류를 알아야 하므로 다음 단계에서 개선 필요)
            List<DungeonSpawnManager.MonsterSpawnData> currentSpawnData = new List<DungeonSpawnManager.MonsterSpawnData>();
            foreach (var manager in activeSpawnManagers)
            {
                // DungeonSpawnManager에 MonsterSpawnData를 가져오는 getter가 필요하지만, 
                // 지금은 빈 리스트를 전달하여 최소 기능을 구현합니다.
            }

            riskSystem.IncreaseExplorationCount(currentSpawnData);
        }
        if (DungeonScoreManager.Instance != null)
        {
            DungeonScoreManager.Instance.ResetScore();
        }
        else
        {
            Debug.LogError("DungeonScoreManager 인스턴스를 찾을 수 없어 점수 시스템 초기화에 실패했습니다!");
        }

        if (activeSpawnManagers.Count > 0)
        {
            foreach (DungeonSpawnManager manager in activeSpawnManagers)
            {
                manager.SpawnAllMonsters();
            }
        }
        else
        {
            Debug.LogWarning("현재 활성화된 DungeonSpawnManager가 없습니다. 몬스터 스폰이 발생하지 않았습니다!");
        }
        SoundManager.Instance.PlayBGM(BGMType.Main_B, 1.0f);
    }

    /// <summary>
    /// 플레이어가 던전에서 나갈 때 호출되는 메서드입니다.
    /// 점수를 계산하고 보상을 지급하며, 몬스터를 정리합니다.
    /// </summary>
    public void ExitDungeon()
    {
        SoundManager.Instance.PlayBGM(BGMType.Main_A, 1.0f);
        int finalScore = 0;

        if (DungeonScoreManager.Instance != null)
        {
            finalScore = DungeonScoreManager.Instance.CalculateFinalScore();

            // 계산된 점수에 따라 보상 시스템을 호출합니다.
            if (DungeonRewardSystem.Instance != null && !MainSceneManager.Instance.isGameOver)
            {
                DungeonRewardSystem.Instance.GrantReward(finalScore);
            }
            else
            {
                Debug.LogWarning("DungeonRewardSystem이 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogWarning("DungeonScoreManager가 존재하지 않습니다.");
        }

        // 몬스터 정리는 점수 계산 및 보상 지급 이후에 수행합니다.
        if (activeSpawnManagers.Count > 0)
        {
            foreach (DungeonSpawnManager manager in activeSpawnManagers)
            {
                manager.DestroyAllMonsters();
            }
        }

        OnDungeonExit?.Invoke();
        if (UITutorialHandler.Instance != null)
        { UITutorialHandler.Instance.OnDungeonExitDetected.Invoke(); }
        // =======================================================
        // [핵심 추가 1: 로직 한 줄] 던전 정상 퇴장 시 게이지 상승 모드 종료
        // =======================================================
        if (riskSystem is DungeonRiskManager riskManager)
        {
            riskManager.StopExploration();
        }
        // =======================================================
    }
    /// <summary>
    /// 플레이어가 죽어서 던전에서 나갈 때 호출되는 메서드입니다.
    /// </summary>
    public void DeadDungeon()
    {
        // 보스 강제 파괴 인텐트 주입 로직
        if (currentBossInstance != null)
        {
            IBossInitializer bossInitializer = currentBossInstance.GetComponent<IBossInitializer>();

            if (bossInitializer != null)
            {
                // 보스에게 "이번 파괴는 강제 파괴이니, NotifyBossDefeated()를 호출하지 마라"고 알립니다.
                bossInitializer.PrepareForForcedDestroy();
            }
            else
            {
                Debug.LogError("DungeonManager: 보스 인스턴스에서 IBossInitializer 컴포넌트를 찾을 수 없습니다! 강제 파괴 알림 실패.");
            }
        }

        // 몬스터 정리 로직
        if (activeSpawnManagers.Count > 0)
        {
            foreach (DungeonSpawnManager manager in activeSpawnManagers)
            {
                manager.DestroyAllMonsters();
            }
        }

        // 보스 인스턴스 정리 로직 추가
        if (currentBossInstance != null)
        {
            // PrepareForForcedDestroy()가 호출된 후 파괴가 진행됩니다.
            Destroy(currentBossInstance);
            currentBossInstance = null;
        }

        // 던전 퇴장 이벤트 호출 (사망 퇴장)
        OnDungeonExit?.Invoke();
        
        // =======================================================
        // [핵심 추가 2: 로직 한 줄] 던전 사망 퇴장 시 게이지 상승 모드 종료
        // =======================================================
        if (riskSystem is DungeonRiskManager riskManager)
        {
            riskManager.StopExploration();
        }
        // =======================================================
    }
    /// <summary>
    /// 현재 보스룸 전투가 활성화/진행 중인지 여부를 나타냅니다.
    /// 이 상태는 BossRoomDoor의 상호작용 및 보스룸 관련 로직을 제어합니다.
    /// </summary>
    public bool IsBossRoomActive
    {
        get { return _isBossRoomActive; }
        // Setter를 구현하여 상태 변경 시 후속 로직을 추가할 수 있습니다.
        // 사용자님의 의견에 따라 DungeonManager 내부에서만 변경 가능하도록 private set으로 설정합니다.
        private set
        {
            // 현재 상태와 다른 값으로 변경될 때만 로직을 실행하여 불필요한 호출을 방지합니다.
            if (_isBossRoomActive != value)
            {
                _isBossRoomActive = value;

                // TODO: (나중에) 상태 변경이 완료된 후 필요한 로직을 이곳에 추가합니다.
                // 예: 보스룸 진입 후 BGM 변경, 전체 UI 변경 등
            }
        }
    }
    // DungeonManager.cs 에 추가될 내용입니다.

    /// <summary>
    /// BossRoomDoor에서 호출되어, 보스룸 전투 상태를 활성화합니다.
    /// 단일 책임 원칙(SRP)에 따라, 이 메서드는 현재 'IsBossRoomActive' 상태 변경 책임만 가집니다.
    /// </summary>
    /// <param name="player">현재는 사용되지 않으나, 추후 로깅/상태 추적을 위해 시그니처를 유지합니다.</param>
    public void HandleBossRoomEntry(GameObject player)
    {
        // 핵심 로직: 내부의 private set을 통해 IsBossRoomActive 프로퍼티의 값을 변경합니다.
        this.IsBossRoomActive = true;
        SpawnBoss();
        // TODO: (나중에) 상태 변경 후 보스 소환, BGM 변경 등 후속 로직이 여기에 추가됩니다.
    }
    /// <summary>
    /// 보스 몬스터 프리팹을 지정된 위치에 생성하고 인스턴스를 추적합니다.
    /// 단일 책임 원칙(SRP)에 따라, 오직 소환 작업만을 수행합니다.
    /// </summary>
    private void SpawnBoss()
    {
        // 유효성 검사
        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogError("보스 프리팹 또는 소환 위치가 설정되지 않았습니다. 보스 소환 실패!");
            return;
        }

        // 보스 생성 및 추적
        currentBossInstance = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        // [핵심 로직 1] Notifier 주입 (기존 IBossInitializer를 통한 주입 로직)
        if (currentBossInstance.TryGetComponent(out IBossInitializer bossInitializer))
        {
            bossInitializer.SetNotifier(this);
            bossInitializer.SetSummonArea(rootSummonAreaCollider);
        }
        else
        {
            Debug.LogError($" Fatal Error: 보스 프리팹 '{bossPrefab.name}'에서 IBossInitializer 컴포넌트(ForestBoss.cs 등)를 찾을 수 없습니다! 주입 실패.");
            currentBossID = -1;
            // 주입 실패 시 ID 추적도 의미 없으므로 여기서 return 처리하는 것도 고려 가능
        }

        // [핵심 로직 2] ID 추적 (Monster 컴포넌트를 통해 MonsterData 접근으로 변경 ⭐)
        // DungeonManager.cs:252 라인의 원래 로직을 이 블록으로 대체합니다.
        if (currentBossInstance.TryGetComponent(out Monster monsterComponent))
        {
            // Monster 컴포넌트는 MonsterData ScriptableObject를 참조하고 있을 것입니다.
            if (monsterComponent.monsterData != null)
            {
                // MonsterData가 public 필드라면 직접 접근하여 ID를 가져옵니다.
                currentBossID = monsterComponent.monsterData.monsterID;
            }
            else
            {
                Debug.LogError($" Fatal Error: {monsterComponent.name}의 MonsterData가 Null입니다! ID 추적 실패.");
                currentBossID = -1;
            }
        }
        else
        {
            // Monster 컴포넌트는 ForestBoss의 [RequireComponent]로 필수이므로 이 에러는 거의 발생하지 않아야 합니다.
            Debug.LogError($" Fatal Error: 보스 프리팹 '{bossPrefab.name}'에서 필수 컴포넌트인 Monster를 찾을 수 없습니다!");
            currentBossID = -1;
        }
        SoundManager.Instance.PlayBGM(BGMType.Main_C, 1.0f);
    }
    // IBossNotifier 인터페이스의 구현부
    /// <summary>
    /// IBossNotifier 인터페이스를 통해 보스 몬스터가 사망했음을 알림 받습니다.
    /// 이 메서드는 **던전 상태를 클리어로 변경**하고, **실제 퇴장은 BossRoomDoor에 위임**합니다.
    /// </summary>
    public void NotifyBossDefeated()
    {
        // 상태 변경: 전투 종료 및 클리어 상태 설정
        this.IsBossRoomActive = false; // 전투 상태 종료
        this.IsDungeonCleared = true;  // <--- 클리어 상태를 true로 설정

        // ==============================================================
        // [핵심 로직] 보스 최초 처치 시 1회성 아이템 지급 처리
        // ==============================================================
        if (SaveManager.Instance != null && currentBossID > 0)
        {
            // 1. SaveManager를 거치지 않고, DungeonManager가 직접 자신의 딕셔너리를 사용합니다.
            bool isAlreadyKilled = this.bossFirstKillRecords.ContainsKey(currentBossID); // 수정된 부분!

            if (!isAlreadyKilled)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(
                        "보스 처치 완료! \n 마을 조각을 획득했습니다.",
                        NotificationType.Success // Success 타입으로 호출
                    );
                }
                me.SetActive(true);
                // ==========================================================
                if (currentBossID == 2001)
                {
                    DungeonInventoryManager.Instance.AddPlayerItem("2"); // 요리마을 조각
                }
                // ==========================================================

                // 2. 내부 딕셔너리에 기록을 업데이트합니다.
                this.bossFirstKillRecords[currentBossID] = true; // 수정된 부분!
                // 3. 변경 사항을 영구 저장하려면 SaveManager.SaveGame()을 호출합니다.
                SaveManager.Instance.SaveGame();
            }
            else
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(
                        "보스 처치 완료!",
                        NotificationType.Success // Success 타입으로 호출
                    );
                }
                Debug.Log($"보스 ID {currentBossID}는 이미 처치 기록이 있습니다. 1회성 보상은 지급되지 않습니다.");
            }
            OnBossDefeated?.Invoke();
        }

    }
    // [추가] 던전 상태 초기화 메서드
    /// <summary>
    /// 던전 클리어 후 BossRoomDoor에서 호출되어, 던전의 핵심 상태를 초기화합니다.
    /// 이 메서드는 클리어 상태를 해제하고 다음 던전 진입을 준비합니다. (SRP 준수)
    /// </summary>
    public void ResetDungeonState()
    {
        this.IsDungeonCleared = false; // 클리어 상태 해제
        this.IsInDungeon = false;      // 던전 밖으로 나감 상태 설정 (BossRoomDoor가 아닌 여기서 InDungeon을 변경합니다)

        // 현재 보스 인스턴스 참조를 해제하고 파괴합니다.
        if (currentBossInstance != null)
        {
            Destroy(currentBossInstance);
            currentBossInstance = null;
        }
    }
    // ===============================================
    // ISavable 인터페이스 구현 (데이터 영속성 확보)
    // ===============================================

    /// <summary>
    /// 현재 DungeonManager의 저장 가능한 상태(보스 처치 기록 등)를 반환합니다.
    /// </summary>
    public object SaveData()
    {
        DungeonManagerSaveData data = new DungeonManagerSaveData
        {
            // 메모리 내의 기록을 저장 데이터로 복사
            bossFirstKillRecords = this.bossFirstKillRecords
        };
        return data;
    }

    /// <summary>
    /// 로드된 저장 데이터를 DungeonManager의 상태에 적용합니다.
    /// </summary>
    /// <param name="data">로드된 데이터 객체 (DungeonManagerSaveData 타입)</param>
    public void LoadData(object data)
    {
        if (data is DungeonManagerSaveData loadedData)
        {
            // 로드된 기록을 메모리 딕셔너리에 적용
            this.bossFirstKillRecords = loadedData.bossFirstKillRecords;
        }
    }
}