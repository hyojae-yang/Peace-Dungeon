using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class DungeonFrameInteraction : MonoBehaviour
{
    // === 기존 변수들 ===
    // public GameObject interactionUI; // <--- 1. 이 필드를 제거합니다.
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
        // if (interactionUI != null) interactionUI.SetActive(false); // <--- interactionUI 제거로 인해 이 줄도 제거합니다.
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
            // if (interactionUI != null) { interactionUI.SetActive(true); } // <--- 기존 로직 제거
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
            // if (interactionUI != null) { interactionUI.SetActive(false); } // <--- 기존 로직 제거
        }
    }

    private void Update()
    {
        // [수정] 4. Update의 E키 감지 조건을 isPlayerInZone 플래그로 대체
        // if (interactionUI.activeSelf && Input.GetKeyDown(KeyCode.E)) // <--- 기존 조건
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E)) // <--- 수정된 조건
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

        UITutorialHandler.Instance.OnFrameUIOpened.Invoke();

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

    private void CloseInventory()
    {
        // === 핵심 수정 부분 (유지) ===
        if (dungeonShopUIManager != null)
        {
            dungeonShopUIManager.ClearShopUI();
        }

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        //마우스 커서 잠금
        //Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

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
    }
}