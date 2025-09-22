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
            Debug.LogWarning("���� Ȱ��ȭ�� DungeonSpawnManager�� �����ϴ�!");
        }
    }

    private void HandleDungeonExit()
    {
        if (DungeonScoreManager.Instance != null)
        {
            // ���� ����� ���� �ı��� �Ϸ�� �� �����մϴ�.
            int finalScore = DungeonScoreManager.Instance.CalculateFinalScore();
            // **�߰��� �κ�: ���� �ý��� ȣ��**
            if (DungeonRewardSystem.Instance != null)
            {
                DungeonRewardSystem.Instance.GrantReward(finalScore);
            }
            else
            {
                Debug.LogWarning("DungeonRewardSystem�� �������� �ʽ��ϴ�.");
            }
        }
        if (currentSpawnManager != null)
        {
            // �������� ���� �� ���� ���� �޼��带 ȣ���մϴ�.
            currentSpawnManager.DestroyAllMonsters();
        }

        else
        {
            Debug.LogWarning("DungeonScoreManager�� �������� �ʽ��ϴ�.");
        }
    }
}