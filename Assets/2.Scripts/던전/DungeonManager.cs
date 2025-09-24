using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    private DungeonSpawnManager currentSpawnManager;

    private bool _isInDungeon = false;
    /// <summary>
    /// 현재 플레이어가 던전 안에 있는지(true) 밖에 있는지(false)를 나타냅니다.
    /// 이 프로퍼티에 값을 할당하면 던전 진입에 필요한 로직이 자동으로 실행됩니다.
    /// </summary>
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
        if (currentSpawnManager != null)
        {
            // 던전에서 나갈 때 몬스터 정리 메서드를 호출합니다.
            currentSpawnManager.DestroyAllMonsters();
        }

        if (DungeonScoreManager.Instance != null)
        {
            // 몬스터 파괴가 완료된 후 점수를 계산합니다.
            int finalScore = DungeonScoreManager.Instance.CalculateFinalScore();

            // 계산된 점수에 따라 보상 시스템을 호출합니다.
            if (DungeonRewardSystem.Instance != null)
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
    }
}