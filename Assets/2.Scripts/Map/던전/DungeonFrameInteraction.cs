using Unity.Cinemachine;
using UnityEngine;

public class DungeonFrameInteraction : MonoBehaviour
{
    // 던전 액자 위에 띄울 "상호작용 E" UI
    public GameObject interactionUI;

    // 인벤토리 UI를 담을 변수 (별도의 캔버스에 있다고 하셨죠)
    public GameObject inventoryUI;

    // 플레이어의 움직임을 제어할 스크립트 변수
    private PlayerController playerController;

    public CinemachineCamera dungeonCamera;

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
        // 플레이어 콜라이더와 접촉했는지 확인
        if (other.gameObject.CompareTag("Player"))
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
        // 상호작용 UI가 활성화된 상태에서 E키를 누르면
        if (interactionUI.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            // 인벤토리 UI 활성화 및 플레이어 제어
            if (inventoryUI != null)
            {
                inventoryUI.SetActive(true);
            }
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            dungeonCamera.Priority = 2;
            // 상호작용 UI는 비활성화
            interactionUI.SetActive(false);
        }
    }

    // 이 메서드는 UI 버튼에 연결할 겁니다.
    public void CloseInventoryUI()
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
        dungeonCamera.Priority = 0;
    }
}