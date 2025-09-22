using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    private DungeonSpawnManager currentSpawnManager;

    private bool _isInDungeon = false;
    public bool IsInDungeon
    {
        get { return _isInDungeon; }
        set
        {
            if (_isInDungeon != value)
            {
                _isInDungeon = value;

                if (_isInDungeon)
                {
                    HandleDungeonEntry();
                }
                else
                {
                    HandleDungeonExit();
                }
            }
        }
    }

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

    public void RegisterSpawnManager(DungeonSpawnManager manager)
    {
        currentSpawnManager = manager;
    }

    public void UnregisterSpawnManager(DungeonSpawnManager manager)
    {
        if (currentSpawnManager == manager)
        {
            currentSpawnManager = null;
        }
    }

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

    private void HandleDungeonExit()
    {
        if (DungeonScoreManager.Instance != null)
        {
            // 점수 계산은 몬스터 파괴가 완료된 후 진행합니다.
            int finalScore = DungeonScoreManager.Instance.CalculateFinalScore();
            // **추가된 부분: 보상 시스템 호출**
            if (DungeonRewardSystem.Instance != null)
            {
                DungeonRewardSystem.Instance.GrantReward(finalScore);
            }
            else
            {
                Debug.LogWarning("DungeonRewardSystem이 존재하지 않습니다.");
            }
        }
        if (currentSpawnManager != null)
        {
            // 던전에서 나갈 때 몬스터 정리 메서드를 호출합니다.
            currentSpawnManager.DestroyAllMonsters();
        }

        else
        {
            Debug.LogWarning("DungeonScoreManager가 존재하지 않습니다.");
        }
    }
}