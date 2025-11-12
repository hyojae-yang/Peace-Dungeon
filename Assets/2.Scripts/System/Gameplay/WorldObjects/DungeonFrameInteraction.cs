using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq; // List.Cast<SmallMap>() 등을 위해 Linq 추가

// 이 스크립트가 SmallMap과 TownMap을 모두 처리할 수 있는 MapPiece 인터페이스를 가정하고 수정합니다.
// TownMap이 SmallMap을 상속하거나, 인벤토리 매니저가 두 타입을 모두 처리한다고 가정합니다.

public class DungeonFrameInteraction : MonoBehaviour
{
    // === 기존 변수들 ===
    public GameObject inventoryUI;
    private PlayerController playerController;
    public CinemachineCamera dungeonCamera;
    private bool isInventoryOpen = false;
    [Tooltip("인벤토리 UI가 나타나기까지의 딜레이 시간(초)을 설정합니다.")]
    [SerializeField] private float uiActivationDelay = 0.5f;

    // === 새로 추가된 변수들 ===
    [SerializeField] private DungeonShopUIManager dungeonShopUIManager;
    [SerializeField] private DungeonInventoryManager dungeonInventoryManager;

    // [추가] 4. 트리거 영역 내 플레이어 유무를 추적하는 플래그
    private bool isPlayerInZone = false;

    private void Start()
    {
        // 기존 Start() 로직
        if (inventoryUI != null) inventoryUI.SetActive(false);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }

        // 새로 추가된 매니저 레퍼런스 검증
        if (dungeonShopUIManager == null) Debug.LogError("DungeonShopUIManager가 할당되지 않았습니다.");
        if (dungeonInventoryManager == null) Debug.LogError("DungeonInventoryManager가 할당되지 않았습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isInventoryOpen)
        {
            // [추가] 4. 플래그 설정
            isPlayerInZone = true;

            // [수정] 2. NotificationManager를 사용하여 상호작용 프롬프트 표시
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowInteractionPrompt("E 키를 눌러 조각 배치 및 구매", this.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // [추가] 4. 플래그 해제
            isPlayerInZone = false;

            // [수정] 3. NotificationManager를 사용하여 상호작용 프롬프트 숨김
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.HideInteractionPrompt(this.gameObject);
            }
        }
    }

    private void Update()
    {
        // [수정] 4. Update의 E키 감지 조건을 isPlayerInZone 플래그로 대체
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            OpenInventory();
        }

        if (isInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }

    private void OpenInventory()
    {
        if (isInventoryOpen) return;

        // UITutorialHandler.Instance가 null일 수 있으므로 null 체크 추가
        if (UITutorialHandler.Instance != null)
        {
            // TownMap을 위한 이벤트이더라도 일단 호출 (DungeonFrameInteraction이 Village에서도 사용될 수 있으므로)
            UITutorialHandler.Instance.OnFrameUIOpened.Invoke();
        }

        isInventoryOpen = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 20;
        }
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.HideInteractionPrompt(this.gameObject);
        }

        // === 핵심 수정 부분 (유지) ===
        if (dungeonShopUIManager != null)
        {
            dungeonShopUIManager.InitializeShopUI();
        }

        StartCoroutine(ActivateUIWithDelay());
    }

    private IEnumerator ActivateUIWithDelay()
    {
        yield return new WaitForSeconds(uiActivationDelay);

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
    }

    /// <summary>
    /// 인벤토리를 닫을 때, 그리드에 유효하게 배치되지 않은 맵 조각들을 인벤토리로 회수합니다.
    /// ViligeMap과 DungeonMap 시스템을 모두 지원합니다. (OCP 준수)
    /// </summary>
    private void CloseInventory()
    {
        // === 핵심 수정 부분 (유지) ===
        if (dungeonShopUIManager != null)
        {
            dungeonShopUIManager.ClearShopUI();
        }

        // ==========================================================
        // [핵심 수정] 유효하지 않은 맵 조각 회수 요청 로직
        // ==========================================================
        int reclaimedCount = 0;

        // 1. DungeonMap 시스템의 맵 조각 회수 시도
        // DungeonMap.Instance가 존재한다면 던전 씬임을 가정합니다.
        if (DungeonMap.Instance != null && DungeonInventoryManager.Instance != null)
        {
            // SmallMap 타입의 리스트를 받습니다.
            List<SmallMap> invalidDungeonMaps = DungeonMap.Instance.GetInvalidlyPlacedMaps();

            // 맵 리스트를 순회하며 회수합니다.
            foreach (SmallMap map in invalidDungeonMaps)
            {
                // ReclaimMapPiece가 SmallMap (또는 TownMap의 상위 타입)을 받도록 가정
                DungeonInventoryManager.Instance.ReclaimMapPiece(map);
                reclaimedCount++;
            }
        }

        // 2. ViligeMap 시스템의 맵 조각 회수 시도 (던전이 아닌 씬을 위해)
        // ViligeMap.Instance가 존재하고, 던전 맵 회수 과정에서 회수가 없었을 경우에만 시도하도록 로직 조정 가능.
        // 하지만 여기서는 두 시스템을 모두 지원하도록 병렬 처리 (두 인스턴스가 동시에 존재하지 않는다고 가정)
        // ViligeMap이 TownMap 리스트를 반환한다고 가정합니다.
        if (ViligeMap.Instance != null && DungeonInventoryManager.Instance != null)
        {
            // TownMap 리스트를 받습니다.
            // TownMap이 SmallMap을 상속하지 않는다면, DungeonInventoryManager.ReclaimMapPiece(object map) 같은 유연한 메서드가 필요합니다.
            // 여기서는 TownMap이 SmallMap을 상속하거나, ReclaimMapPiece가 TownMap을 오버로드했다고 가정하고 원래 코드를 유지합니다.
            List<TownMap> invalidViligeMaps = ViligeMap.Instance.GetInvalidlyPlacedMaps();

            // 기존 코드: TownMap을 순회하여 회수
            foreach (TownMap map in invalidViligeMaps)
            {
                // TownMap 타입의 맵을 회수합니다.
                // 만약 이 맵들이 Dungeon 맵과 동일한 SmallMap 타입이라면, 위의 DungeonMap 로직에 통합할 수 있습니다.
                DungeonInventoryManager.Instance.ReclaimMapPiece(map);
                reclaimedCount++; // TownMap 회수 카운트도 합산
            }
        }

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 0;
        }

        isInventoryOpen = false;

        // 인벤토리를 닫은 후, 플레이어가 아직 영역 내에 있다면 알림을 다시 띄워줍니다.
        if (isPlayerInZone && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowInteractionPrompt("E 키를 눌러 조각 배치 및 구매", this.gameObject);
        }
        if (UITutorialHandler.Instance != null)
        { UITutorialHandler.Instance.OnDungeonPlacementUIClose.Invoke(); }

    }
}