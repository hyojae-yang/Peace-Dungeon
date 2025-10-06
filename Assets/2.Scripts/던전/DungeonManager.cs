using UnityEngine;
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
}
public class DungeonManager : MonoBehaviour, IBossNotifier
{
    public static DungeonManager Instance { get; private set; }

    private DungeonSpawnManager currentSpawnManager;

    private bool _isInDungeon = false;
    /// <summary>
    /// 현재 플레이어가 던전 안에 있는지(true) 밖에 있는지(false)를 나타냅니다.
    /// 이 프로퍼티에 값을 할당하면 던전 진입에 필요한 로직이 자동으로 실행됩니다.
    /// </summary>
    public bool _isBossRoomActive = false;
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
                }
                // 기존의 HandleDungeonExit() 호출 로직은 DungeonDoor.cs로 이동되었습니다.
                // 던전 퇴장 시 필요한 로직(몬스터 정리, 보상)은 ExitDungeon() 메서드에서 처리됩니다.
            }
        }
    }
    
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

    /// <summary>
    /// 현재 던전에 맞는 DungeonSpawnManager를 등록합니다.
    /// </summary>
    /// <param name="manager">현재 던전의 스폰 매니저 오브젝트.</param>
    public void RegisterSpawnManager(DungeonSpawnManager manager)
    {
        currentSpawnManager = manager;
    }

    /// <summary>
    /// 현재 등록된 DungeonSpawnManager의 등록을 해제합니다.
    /// </summary>
    /// <param name="manager">해제할 스폰 매니저 오브젝트.</param>
    public void UnregisterSpawnManager(DungeonSpawnManager manager)
    {
        if (currentSpawnManager == manager)
        {
            currentSpawnManager = null;
        }
    }

    /// <summary>
    /// 플레이어가 던전에 진입했을 때 실행되는 로직입니다.
    /// </summary>
    private void HandleDungeonEntry()
    {
        if (currentSpawnManager != null)
        {
            currentSpawnManager.SpawnAllMonsters();
        }
        else
        {
            Debug.LogWarning("현재 활성화된 DungeonSpawnManager가 없습니다!");
        }
    }

    /// <summary>
    /// 플레이어가 던전에서 나갈 때 호출되는 메서드입니다.
    /// 몬스터를 정리하고, 점수를 계산하여 보상을 지급합니다.
    /// </summary>
    public void ExitDungeon()
    {
        if (DungeonScoreManager.Instance != null)
        {
            // 몬스터 파괴가 완료된 후 점수를 계산합니다.
            int finalScore = DungeonScoreManager.Instance.CalculateFinalScore();

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
        
        if (currentSpawnManager != null)
        {
            // 던전에서 나갈 때 몬스터 정리 메서드를 호출합니다.
            currentSpawnManager.DestroyAllMonsters();
        }
    }
    /// <summary>
    /// 플레이어가 던전에서 나갈 때 호출되는 메서드입니다.
    /// 몬스터를 정리하고, 점수를 계산하여 보상을 지급합니다.
    /// </summary>
    public void DeadDungeon()
    {
        if (currentSpawnManager != null)
        {
            // 던전에서 나갈 때 몬스터 정리 메서드를 호출합니다.
            currentSpawnManager.DestroyAllMonsters();
        }
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

        // 인터페이스 bossInitializer 컴포넌트를 찾습니다.
        if (currentBossInstance.TryGetComponent(out IBossInitializer bossInitializer)) // OCP/DIP 준수
        {
            // DungeonManager는 IBossNotifier를 구현했으므로 'this'를 넘겨줄 수 있습니다.
            // bossInitializer(IBossInitializer 타입)를 통해 SetNotifier를 호출합니다.
            bossInitializer.SetNotifier(this);

            // 2. [추가] 소환 영역 주입 (새로운 로직)
            // DungeonManager에 할당된 씬 오브젝트 Collider 정보를 보스에게 전달합니다.
            bossInitializer.SetSummonArea(rootSummonAreaCollider);
        }
        else
        {
            // 디버그 메시지 수정: 실제 스크립트 이름과 찾고 있는 컴포넌트 이름을 명확히 합니다.
            Debug.LogError("bossInitializer 컴포넌트를 찾을 수 없습니다. 보스 프리팹에 해당 스크립트가 붙어 있는지 확인하세요!");
        }
    }
    // IBossNotifier 인터페이스의 구현부
    /// <summary>
    /// IBossNotifier 인터페이스를 통해 보스 몬스터가 사망했음을 알림 받습니다.
    /// 단일 책임 원칙(SRP)에 따라, 보스룸 전투 종료 후 던전 퇴장 처리를 위임합니다.
    /// </summary>
    public void NotifyBossDefeated()
    {
        // 1. 상태 변경: 보스룸 전투 종료 상태로 되돌립니다.
        // BossRoomDoor의 충돌 로직이 다시 활성화되는 것을 막기 위해 상태를 false로 설정합니다.
        this.IsBossRoomActive = false;
        IsInDungeon = false; // 던전 밖으로 나감
        PlayerCharacter.Instance.playerController.outDungeon();
        // 2. 핵심 로직: 기존에 구현된 던전 퇴장 처리 메서드를 호출하여 책임을 위임합니다.
        ExitDungeon();
        
    }
}