using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class DungeonFrameInteraction : MonoBehaviour
{
    // === ���� ������ ===
    public GameObject interactionUI;
    public GameObject inventoryUI;
    private PlayerController playerController;
    public CinemachineCamera dungeonCamera;
    private bool isInventoryOpen = false;
    [Tooltip("�κ��丮 UI�� ��Ÿ��������� ������ �ð�(��)�� �����մϴ�.")]
    [SerializeField] private float uiActivationDelay = 0.5f;

    // === ���� �߰��� ������ ===
    // ���� UI�� ������ ����� �����ϴ� �Ŵ���
    [SerializeField] private DungeonShopUIManager dungeonShopUIManager;

    // �κ��丮 UI�� ���� �������� ǥ���ϴ� �Ŵ���
    [SerializeField] private DungeonInventoryManager dungeonInventoryManager;

    private void Start()
    {
        // ���� Start() ����
        if (interactionUI != null) interactionUI.SetActive(false);
        if (inventoryUI != null) inventoryUI.SetActive(false);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }

        // ���� �߰��� �Ŵ��� ���۷��� ����
        if (dungeonShopUIManager == null) Debug.LogError("DungeonShopUIManager�� �Ҵ���� �ʾҽ��ϴ�.");
        if (dungeonInventoryManager == null) Debug.LogError("DungeonInventoryManager�� �Ҵ���� �ʾҽ��ϴ�.");
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

        // === �ٽ� ���� �κ� ===
        // ���� UI�� ������ ����� �����ϴ� �޼��� ȣ��
        if (dungeonShopUIManager != null)
        {
            dungeonShopUIManager.InitializeShopUI();
        }

        // �κ��丮 UI Ȱ��ȭ�� �ڷ�ƾ���� �����Ͽ� ������ ����
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
        // === �ٽ� ���� �κ� ===
        // ���� UI�� ���� ������ ���Ե��� �����ϴ� �޼��� ȣ��
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
        //���콺 Ŀ�� ���
        //Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 0;
        }

        isInventoryOpen = false;
    }
}