using UnityEngine;

/// <summary>
/// �÷��̾�� ���õ� ��� �ֿ� �ý����� �����ϴ� �߾� ��� ��ũ��Ʈ�Դϴ�.
/// �̱��� �������� �����Ǿ� ��𼭵� ���� ������ �� �ֽ��ϴ�.
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static PlayerCharacter Instance { get; private set; }

    // === ���� �ý��� ===
    [Tooltip("�÷��̾��� �ɷ�ġ �ý����� �����մϴ�.")]
    public PlayerStats PlayerStats;

    [Tooltip("�÷��̾��� �κ��丮 �ý����� �����մϴ�.")]
    public InventoryManager InventoryManager;

    // TODO: ���� ���� �ý����� �߰��Ǹ� ���⿡ ���� ������ �߰��� ����
    // public PlayerBuffSystem PlayerBuffSystem;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� �Ҵ� �� ����
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // �ʿ信 ���� Ȱ��ȭ (�� ��ȯ �� ����)
        }
        else
        {
            Debug.LogWarning("PlayerCharacter�� �ν��Ͻ��� �̹� �����մϴ�. �� ������Ʈ�� �ı��մϴ�.");
            Destroy(gameObject);
        }

        // ������Ʈ �ڵ� �Ҵ� (null�� ���)
        if (PlayerStats == null)
        {
            PlayerStats = PlayerStats.Instance;
        }

        if (InventoryManager == null)
        {
            InventoryManager = InventoryManager.Instance;
        }

        // ��� �ý����� ���������� �����Ǿ����� Ȯ��
        if (PlayerStats == null || InventoryManager == null)
        {
            Debug.LogError("PlayerCharacter: �ʼ� �ý��� �� �ϳ� �̻��� ã�� �� �����ϴ�. (PlayerStats, InventoryManager)");
        }
    }
}