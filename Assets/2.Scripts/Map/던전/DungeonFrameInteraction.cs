using Unity.Cinemachine;
using UnityEngine;

public class DungeonFrameInteraction : MonoBehaviour
{
    // ���� ���� ���� ��� "��ȣ�ۿ� E" UI
    public GameObject interactionUI;

    // �κ��丮 UI�� ���� ���� (������ ĵ������ �ִٰ� �ϼ���)
    public GameObject inventoryUI;

    // �÷��̾��� �������� ������ ��ũ��Ʈ ����
    private PlayerController playerController;

    public CinemachineCamera dungeonCamera;

    private void Start()
    {
        // UI ������Ʈ�� ó���� ��Ȱ��ȭ�ǵ��� Ȯ���ϰ� ����
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }

        // �÷��̾� ������Ʈ�� PlayerController ��ũ��Ʈ�� ã�ƵӴϴ�.
        // �÷��̾� ������Ʈ�� Player �±׸� �ο��ϸ� �����ϴ�.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾� �ݶ��̴��� �����ߴ��� Ȯ��
        if (other.gameObject.CompareTag("Player"))
        {
            // ��ȣ�ۿ� UI Ȱ��ȭ
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �÷��̾� �ݶ��̴��� ������ �������� Ȯ��
        if (other.gameObject.CompareTag("Player"))
        {
            // ��ȣ�ۿ� UI ��Ȱ��ȭ
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // ��ȣ�ۿ� UI�� Ȱ��ȭ�� ���¿��� EŰ�� ������
        if (interactionUI.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            // �κ��丮 UI Ȱ��ȭ �� �÷��̾� ����
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
            // ��ȣ�ۿ� UI�� ��Ȱ��ȭ
            interactionUI.SetActive(false);
        }
    }

    // �� �޼���� UI ��ư�� ������ �̴ϴ�.
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