using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action�� ����ϱ� ���� �߰�

/// <summary>
/// ������ ��Ŭ�� �� ��Ÿ���� ��ư �г��� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ Ÿ�Կ� ���� ������ ��ư�� Ȱ��ȭ/��Ȱ��ȭ�ϰ� ����� �Ҵ��մϴ�.
/// </summary>
public class ButtonPanel : MonoBehaviour
{
    // === �ν����Ϳ� �Ҵ��� ���� ���� ===
    [Tooltip("��� ������ ���� ��ư�Դϴ�.")]
    [SerializeField] private Button equipButton;

    [Tooltip("�Ҹ�ǰ ������ ��� ��ư�Դϴ�.")]
    [SerializeField] private Button useButton;

    [Tooltip("������ ������ ��ư�Դϴ�.")]
    [SerializeField] private Button discardButton;

    // === ���� ������ ���� ===
    private BaseItemSO currentItem;
    private int currentItemCount;
    private PlayerCharacter playerCharacter; // PlayerCharacter ���� �߰�

    /// <summary>
    /// ItemSlotUI���� ȣ��Ǿ� ���� ������ ������ �ް�, ��ư�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="item">���� ���Կ� �ִ� ������ ������</param>
    /// <param name="count">���� ���Կ� �ִ� ������ ����</param>
    public void Initialize(BaseItemSO item, int count)
    {
        currentItem = item;
        currentItemCount = count;

        // PlayerCharacter �ν��Ͻ��� �����ɴϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("ButtonPanel: PlayerCharacter �ν��Ͻ��� ã�� �� �����ϴ�.");
        }

        ShowButtonsByItemType();
        AddButtonListeners();
    }

    /// <summary>
    /// ��ư�� Ŭ�� �̺�Ʈ�� �����մϴ�.
    /// Initialize �޼��忡�� �� ���� ȣ��˴ϴ�.
    /// </summary>
    private void AddButtonListeners()
    {
        // �ߺ� �Ҵ� ������ ���� �����ʸ� ���� �����մϴ�.
        equipButton.onClick.RemoveAllListeners();
        useButton.onClick.RemoveAllListeners();
        discardButton.onClick.RemoveAllListeners();

        // �� ��ư�� �´� ����� �����մϴ�.
        // EquipButton�� EquipmentItemSO Ÿ���� ���� �۵��ϵ��� ���� ��ġ �߰�
        if (equipButton != null && currentItem != null && currentItem.itemType == ItemType.Equipment)
        {
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        // UseButton�� ConsumableItemSO Ÿ���� ���� �۵��ϵ��� ���� ��ġ �߰�
        if (useButton != null && currentItem != null && currentItem.itemType == ItemType.Consumable)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
        }

        // DiscardButton�� �׻� �۵�������, currentItem�� null�� �ƴ� ���� ��ȿ�մϴ�.
        if (discardButton != null && currentItem != null)
        {
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }
    }

    /// <summary>
    /// ������ Ÿ�Կ� ���� ��ư���� Ȱ��ȭ ���¸� �����մϴ�.
    /// </summary>
    private void ShowButtonsByItemType()
    {
        // ��� ��ư�� �ϴ� ��Ȱ��ȭ�մϴ�.
        if (equipButton != null) equipButton.gameObject.SetActive(false);
        if (useButton != null) useButton.gameObject.SetActive(false);
        if (discardButton != null) discardButton.gameObject.SetActive(false);

        // ������ Ÿ�Կ� ���� �ʿ��� ��ư�� Ȱ��ȭ�մϴ�.
        if (currentItem != null)
        {
            switch (currentItem.itemType)
            {
                case ItemType.Equipment:
                    if (equipButton != null) equipButton.gameObject.SetActive(true);
                    if (discardButton != null) discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Consumable:
                    if (useButton != null) useButton.gameObject.SetActive(true);
                    if (discardButton != null) discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Material:
                case ItemType.Quest:
                    if (discardButton != null) discardButton.gameObject.SetActive(true);
                    break;
                default:
                    // Ư�� ������ ���� �ƹ� ��ư�� ǥ������ �ʽ��ϴ�.
                    break;
            }
        }
    }

    // === ��ư Ŭ�� �� ȣ��� �޼��� (���� �д�) ===

    /// <summary>
    /// '����' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// PlayerCharacter�� ���� PlayerEquipmentManager���� ������ ��û�մϴ�.
    /// </summary>
    public void OnEquipButtonClicked()
    {
        // ��� ���������� Ȯ���ϰ� ĳ�����մϴ�.
        EquipmentItemSO equipItem = currentItem as EquipmentItemSO;
        if (equipItem != null && playerCharacter != null && playerCharacter.playerEquipmentManager != null)
        {
            // PlayerEquipmentManager���� ���� ��û�� �մϴ�.
            // EquipItem �޼��� ���ο��� ������ ���� ���� �� �κ��丮 ������Ʈ ������ ó���մϴ�.
            // ���� ���� ������ ����� �ش� ������ ���� ó�� �ʿ�.
            playerCharacter.playerEquipmentManager.EquipItem(equipItem);

            // ��ư Ŭ�� �� �г��� �ı��մϴ�.
            Destroy(gameObject);
        }
        else if (equipItem == null)
        {
            Debug.LogWarning("�����Ϸ��� �������� EquipmentItemSO Ÿ���� �ƴմϴ�.");
        }
        else if (playerCharacter == null || playerCharacter.playerEquipmentManager == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ� �Ǵ� PlayerEquipmentManager ������Ʈ�� ���� ������ �� �����ϴ�.");
        }
    }

    /// <summary>
    /// '���' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// PlayerCharacter�� ���� InventoryManager�� ����� ��û�մϴ�.
    /// </summary>
    public void OnUseButtonClicked()
    {
        // �Ҹ�ǰ ���������� Ȯ���ϰ� ĳ�����մϴ�.
        ConsumableItemSO consumeItem = currentItem as ConsumableItemSO;
        if (consumeItem != null && playerCharacter != null && playerCharacter.inventoryManager != null)
        {
            // InventoryManager�� UseItem �޼��带 ȣ���ϸ� PlayerCharacter�� �����մϴ�.
            playerCharacter.inventoryManager.UseItem(consumeItem);
            // ��ư Ŭ�� �� �г��� �ı��մϴ�.
            Destroy(gameObject);
        }
        else if (consumeItem == null)
        {
            Debug.LogWarning("����Ϸ��� �������� ConsumableItemSO Ÿ���� �ƴմϴ�.");
        }
        else if (playerCharacter == null || playerCharacter.inventoryManager == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ� �Ǵ� InventoryManager ������Ʈ�� ���� �������� ����� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// '������' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// InventoryUIController�� Ȯ��â ǥ�ø� ��û�մϴ�.
    /// </summary>
    public void OnDiscardButtonClicked()
    {
        if (currentItem != null && currentItemCount > 0 && InventoryUIController.Instance != null)
        {
            // InventoryUIController�� �̱����̹Ƿ� ���� �����մϴ�.
            // ���� ������ ������ ConfirmPanel���� ó���˴ϴ�.
            InventoryUIController.Instance.ShowDiscardConfirmPanel(currentItem, currentItemCount);
            // ��ư Ŭ�� �� �г��� �ı��մϴ�.
            Destroy(gameObject);
        }
        else if (currentItem == null || currentItemCount <= 0)
        {
            Debug.LogWarning("���� �������� ���ų� ������ ��ȿ���� �ʽ��ϴ�.");
        }
        else if (InventoryUIController.Instance == null)
        {
            Debug.LogError("InventoryUIController �ν��Ͻ��� ���� �������� ���� �� �����ϴ�.");
        }
    }
}