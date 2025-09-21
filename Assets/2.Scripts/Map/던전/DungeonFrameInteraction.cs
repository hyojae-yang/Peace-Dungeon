using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class DungeonFrameInteraction : MonoBehaviour
{
    // === 기존 변수들 ===
    public GameObject interactionUI;
    public GameObject inventoryUI;
    private PlayerController playerController;
    public CinemachineCamera dungeonCamera;
    private bool isInventoryOpen = false;
    [Tooltip("인벤토리 UI가 나타나기까지의 딜레이 시간(초)을 설정합니다.")]
    [SerializeField] private float uiActivationDelay = 0.5f;

    // === 새로 추가된 변수들 ===
    // 상점 UI의 아이템 목록을 관리하는 매니저
    [SerializeField] private DungeonShopUIManager dungeonShopUIManager;

    // 인벤토리 UI에 던전 조각들을 표시하는 매니저
    [SerializeField] private DungeonInventoryManager dungeonInventoryManager;

    private void Start()
    {
        // 기존 Start() 로직
        if (interactionUI != null) interactionUI.SetActive(false);
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
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (interactionUI.activeSelf && Input.GetKeyDown(KeyCode.E))
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

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // === 핵심 수정 부분 ===
        // 상점 UI의 아이템 목록을 갱신하는 메서드 호출
        if (dungeonShopUIManager != null)
        {
            dungeonShopUIManager.InitializeShopUI();
        }

        // 인벤토리 UI 활성화를 코루틴으로 실행하여 딜레이 적용
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
        // === 핵심 수정 부분 ===
        // 상점 UI의 동적 아이템 슬롯들을 정리하는 메서드 호출
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
    }
}