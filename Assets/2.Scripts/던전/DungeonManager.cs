using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    private DungeonSpawnManager currentSpawnManager;

    private bool _isInDungeon = false;
    /// <summary>
    /// ���� �÷��̾ ���� �ȿ� �ִ���(true) �ۿ� �ִ���(false)�� ��Ÿ���ϴ�.
    /// �� ������Ƽ�� ���� �Ҵ��ϸ� ���� ���Կ� �ʿ��� ������ �ڵ����� ����˴ϴ�.
    /// </summary>
    public bool IsInDungeon
    {
        get { return _isInDungeon; }
        set
        {
            // ���� ���¿� �ٸ� ������ ����� ���� ������ �����Ͽ� ���ʿ��� ȣ���� �����մϴ�.
            if (_isInDungeon != value)
            {
                _isInDungeon = value;

                if (_isInDungeon)
                {
                    // ���� ���� �� ���͸� �����ϴ� �޼��带 ȣ���մϴ�.
                    HandleDungeonEntry();
                }
                // ������ HandleDungeonExit() ȣ�� ������ DungeonDoor.cs�� �̵��Ǿ����ϴ�.
                // ���� ���� �� �ʿ��� ����(���� ����, ����)�� ExitDungeon() �޼��忡�� ó���˴ϴ�.
            }
        }
    }

    /// <summary>
    /// ���� ���� �� �� �� ȣ��Ǹ�, �̱��� ������ �ʱ�ȭ�մϴ�.
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
    /// ���� ������ �´� DungeonSpawnManager�� ����մϴ�.
    /// </summary>
    /// <param name="manager">���� ������ ���� �Ŵ��� ������Ʈ.</param>
    public void RegisterSpawnManager(DungeonSpawnManager manager)
    {
        currentSpawnManager = manager;
    }

    /// <summary>
    /// ���� ��ϵ� DungeonSpawnManager�� ����� �����մϴ�.
    /// </summary>
    /// <param name="manager">������ ���� �Ŵ��� ������Ʈ.</param>
    public void UnregisterSpawnManager(DungeonSpawnManager manager)
    {
        if (currentSpawnManager == manager)
        {
            currentSpawnManager = null;
        }
    }

    /// <summary>
    /// �÷��̾ ������ �������� �� ����Ǵ� �����Դϴ�.
    /// </summary>
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

    /// <summary>
    /// �÷��̾ �������� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// ���͸� �����ϰ�, ������ ����Ͽ� ������ �����մϴ�.
    /// </summary>
    public void ExitDungeon()
    {
        if (currentSpawnManager != null)
        {
            // �������� ���� �� ���� ���� �޼��带 ȣ���մϴ�.
            currentSpawnManager.DestroyAllMonsters();
        }

        if (DungeonScoreManager.Instance != null)
        {
            // ���� �ı��� �Ϸ�� �� ������ ����մϴ�.
            int finalScore = DungeonScoreManager.Instance.CalculateFinalScore();

            // ���� ������ ���� ���� �ý����� ȣ���մϴ�.
            if (DungeonRewardSystem.Instance != null)
            {
                DungeonRewardSystem.Instance.GrantReward(finalScore);
            }
            else
            {
                Debug.LogWarning("DungeonRewardSystem�� �������� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("DungeonScoreManager�� �������� �ʽ��ϴ�.");
        }
    }
}