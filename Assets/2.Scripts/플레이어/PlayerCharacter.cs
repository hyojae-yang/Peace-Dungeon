using UnityEngine;

/// <summary>
/// 플레이어와 관련된 모든 주요 시스템을 관리하는 중앙 허브 스크립트입니다.
/// 싱글턴 패턴으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static PlayerCharacter Instance { get; private set; }

    // === 참조 시스템 ===
    [Tooltip("플레이어의 능력치 시스템을 참조합니다.")]
    public PlayerStats PlayerStats;

    [Tooltip("플레이어의 인벤토리 시스템을 참조합니다.")]
    public InventoryManager InventoryManager;

    // TODO: 추후 버프 시스템이 추가되면 여기에 참조 변수를 추가할 예정
    // public PlayerBuffSystem PlayerBuffSystem;

    private void Awake()
    {
        // 싱글턴 인스턴스 할당 및 관리
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요에 따라 활성화 (씬 전환 시 유지)
        }
        else
        {
            Debug.LogWarning("PlayerCharacter의 인스턴스가 이미 존재합니다. 새 오브젝트를 파괴합니다.");
            Destroy(gameObject);
        }

        // 컴포넌트 자동 할당 (null일 경우)
        if (PlayerStats == null)
        {
            PlayerStats = PlayerStats.Instance;
        }

        if (InventoryManager == null)
        {
            InventoryManager = InventoryManager.Instance;
        }

        // 모든 시스템이 정상적으로 참조되었는지 확인
        if (PlayerStats == null || InventoryManager == null)
        {
            Debug.LogError("PlayerCharacter: 필수 시스템 중 하나 이상을 찾을 수 없습니다. (PlayerStats, InventoryManager)");
        }
    }
}