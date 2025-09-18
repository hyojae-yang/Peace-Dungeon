using UnityEngine;
using System.Collections; // �ڷ�ƾ�� ����ϱ� ���� �߰�
using Unity.Cinemachine;

/// <summary>
/// ���� ������ ������Ʈ���� ��ȣ�ۿ��� �����ϴ� ��ũ��Ʈ.
/// �÷��̾���� �Ÿ��� ���� ��ȣ�ۿ� UI�� ǥ���ϰ�,
/// E Ű �Է� �� �κ��丮 UI�� Ȱ��ȭ�ϸ� �÷��̾��� �������� �����մϴ�.
/// ESC Ű �Է� �� �κ��丮 UI�� �ݰ� ���� ���¸� ���󺹱� ��ŵ�ϴ�.
/// </summary>
public class DungeonFrameInteraction : MonoBehaviour
{
    // ���� ���� ���� ��� "��ȣ�ۿ� E" UI
    public GameObject interactionUI;

    // �κ��丮 UI�� ���� ����
    public GameObject inventoryUI;

    // �÷��̾��� �������� ������ ��ũ��Ʈ ����
    private PlayerController playerController;

    // ���� ī�޶�
    public CinemachineCamera dungeonCamera;

    /// <summary>
    /// ���� �κ��丮 UI�� �����ִ��� ���θ� �����ϴ� �Ҹ��� ����.
    /// EŰ�� �����ϸ� true, ESCŰ�� ������ false�� �����˴ϴ�.
    /// </summary>
    private bool isInventoryOpen = false;

    /// <summary>
    /// �κ��丮 UI�� Ȱ��ȭ�Ǳ���� ��ٸ� �ð�(��)�� �����ϴ� ����.
    /// �� ���� �ν����Ϳ��� �����Ͽ� UI Ȱ��ȭ �����̸� ������ �� �ֽ��ϴ�.
    /// </summary>
    [Tooltip("�κ��丮 UI�� ��Ÿ��������� ������ �ð�(��)�� �����մϴ�.")]
    [SerializeField] private float uiActivationDelay = 0.5f;

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
        // �÷��̾� �ݶ��̴��� �����ߴ��� Ȯ���ϰ�, �κ��丮�� �������� ���� UI Ȱ��ȭ
        if (other.gameObject.CompareTag("Player") && !isInventoryOpen)
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
        // ��ȣ�ۿ� UI�� Ȱ��ȭ�� ���¿��� EŰ�� ������ �κ��丮 ����
        if (interactionUI.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            OpenInventory();
        }

        // �κ��丮�� �����ִ� ���¿��� ESCŰ�� ������ �κ��丮 �ݱ�
        if (isInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }

    /// <summary>
    /// �κ��丮 UI�� ���� ���� ���¸� �����ϴ� �޼���.
    /// �÷��̾� �̵��� ���߰�, ���콺 Ŀ���� Ȱ��ȭ�ϸ�, �κ��丮 ���¸� true�� �����մϴ�.
    /// </summary>
    private void OpenInventory()
    {
        // �κ��丮�� �̹� ������ �ִٸ� �ߺ� ȣ�� ����
        if (isInventoryOpen)
        {
            return;
        }

        // ���� ���� ����: �κ��丮�� ������
        isInventoryOpen = true;

        // �÷��̾� ���� ��Ȱ��ȭ �� ���콺 Ŀ�� ����
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // �ó׸��� ī�޶� �켱���� ����
        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 20; // �ٸ� ī�޶󺸴� �켱������ ���� Ȱ��ȭ
        }

        // ��ȣ�ۿ� UI�� ��Ȱ��ȭ
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // �κ��丮 UI Ȱ��ȭ�� �ڷ�ƾ���� �����Ͽ� ������ ����
        StartCoroutine(ActivateUIWithDelay());
    }

    /// <summary>
    /// ������ ������ �ð� �Ŀ� �κ��丮 UI�� Ȱ��ȭ�ϴ� �ڷ�ƾ.
    /// `uiActivationDelay` ������ ������ �ð���ŭ ��ٸ� �� UI�� �մϴ�.
    /// </summary>
    private IEnumerator ActivateUIWithDelay()
    {
        // uiActivationDelay ������ ������ �ð���ŭ ���
        yield return new WaitForSeconds(uiActivationDelay);

        // ��� �� �κ��丮 UI Ȱ��ȭ
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
    }

    /// <summary>
    /// �κ��丮 UI�� �ݰ� ���� ���¸� ���󺹱���Ű�� �޼���.
    /// �÷��̾� �̵��� �ٽ� �����ϰ� �ϰ�, ���콺 Ŀ���� �����, �κ��丮 ���¸� false�� �����մϴ�.
    /// �� �޼���� UI ��ư ��� ESCŰ �Է¿� ���� ȣ��˴ϴ�.
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

        // �ó׸��� ī�޶� �켱���� ����
        if (dungeonCamera != null)
        {
            dungeonCamera.Priority = 0; // ī�޶� �켱������ ���� ��Ȱ��ȭ
        }

        // ���� ���� ����: �κ��丮�� ������
        isInventoryOpen = false;
    }
}