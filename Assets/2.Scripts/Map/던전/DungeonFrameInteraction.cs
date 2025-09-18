using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using Unity.Cinemachine;

/// <summary>
/// 던전 프레임 오브젝트와의 상호작용을 관리하는 스크립트.
/// 플레이어와의 거리에 따라 상호작용 UI를 표시하고,
/// E 키 입력 시 인벤토리 UI를 활성화하며 플레이어의 움직임을 제어합니다.
/// ESC 키 입력 시 인벤토리 UI를 닫고 게임 상태를 원상복구 시킵니다.
/// </summary>
public class DungeonFrameInteraction : MonoBehaviour
{
    // 던전 액자 위에 띄울 "상호작용 E" UI
    public GameObject interactionUI;

    // 인벤토리 UI를 담을 변수
    public GameObject inventoryUI;

    // 플레이어의 움직임을 제어할 스크립트 변수
    private PlayerController playerController;

    // 던전 카메라
    public CinemachineCamera dungeonCamera;

    /// <summary>
    /// 현재 인벤토리 UI가 열려있는지 여부를 추적하는 불리언 변수.
    /// E키로 접근하면 true, ESC키로 나가면 false로 설정됩니다.
    /// </summary>
    private bool isInventoryOpen = false;

    /// <summary>
    /// 인벤토리 UI가 활성화되기까지 기다릴 시간(초)을 설정하는 변수.
    /// 이 값을 인스펙터에서 조절하여 UI 활성화 딜레이를 제어할 수 있습니다.
    /// </summary>
    [Tooltip("인벤토리 UI가 나타나기까지의 딜레이 시간(초)을 설정합니다.")]
    [SerializeField] private float uiActivationDelay = 0.5f;

    private void Start()
    {
        // UI 오브젝트가 처음에 비활성화되도록 확실하게 설정
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }

        // 플레이어 오브젝트의 PlayerController 스크립트를 찾아둡니다.
        // 플레이어 오브젝트에 Player 태그를 부여하면 좋습니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 콜라이더와 접촉했는지 확인하고, 인벤토리가 닫혀있을 때만 UI 활성화
        if (other.gameObject.CompareTag("Player") && !isInventoryOpen)
        {
            // 상호작용 UI 활성화
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어 콜라이더와 접촉이 끝났는지 확인
        if (other.gameObject.CompareTag("Player"))
        {
            // 상호작용 UI 비활성화
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // 상호작용 UI가 활성화된 상태에서 E키를 누르면 인벤토리 열기
        if (interactionUI.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            OpenInventory();
        }

        // 인벤토리가 열려있는 상태에서 ESC키를 누르면 인벤토리 닫기
        if (isInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }

    /// <summary>
    /// 인벤토리 UI를 열고 관련 상태를 설정하는 메서드.
    /// 플레이어 이동을 멈추고, 마우스 커서를 활성화하며, 인벤토리 상태를 true로 변경합니다.
    /// </summary>
    private void OpenInventory()
    {
        // 인벤토리가 이미 열리고 있다면 중복 호출 방지
        if (isInventoryOpen)
        {
            return;
        }

        // 상태 변수 변경: 인벤토리가 열렸음
        isInventoryOpen = true;

        // 플레이어 제어 비활성화 및 마우스 커서 설정
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 시네마신 카메라 우선순위 변경
        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 20; // 다른 카메라보다 우선순위를 높여 활성화
        }

        // 상호작용 UI는 비활성화
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // 인벤토리 UI 활성화를 코루틴으로 실행하여 딜레이 적용
        StartCoroutine(ActivateUIWithDelay());
    }

    /// <summary>
    /// 지정된 딜레이 시간 후에 인벤토리 UI를 활성화하는 코루틴.
    /// `uiActivationDelay` 변수에 설정된 시간만큼 기다린 후 UI를 켭니다.
    /// </summary>
    private IEnumerator ActivateUIWithDelay()
    {
        // uiActivationDelay 변수에 설정된 시간만큼 대기
        yield return new WaitForSeconds(uiActivationDelay);

        // 대기 후 인벤토리 UI 활성화
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
    }

    /// <summary>
    /// 인벤토리 UI를 닫고 게임 상태를 원상복구시키는 메서드.
    /// 플레이어 이동을 다시 가능하게 하고, 마우스 커서를 숨기며, 인벤토리 상태를 false로 변경합니다.
    /// 이 메서드는 UI 버튼 대신 ESC키 입력에 의해 호출됩니다.
    /// </summary>
    private void CloseInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 시네마신 카메라 우선순위 복구
        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 0; // 카메라 우선순위를 낮춰 비활성화
        }

        // 상태 변수 변경: 인벤토리가 닫혔음
        isInventoryOpen = false;
    }
}