using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    /// <summary>
    /// ItemSlotUI���� ȣ��Ǿ� ���� ������ ������ �ް�, ��ư�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="item">���� ���Կ� �ִ� ������ ������</param>
    public void Initialize(BaseItemSO item, int count)
    {
        currentItem = item;
        currentItemCount = count;
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
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        useButton.onClick.AddListener(OnUseButtonClicked);
        discardButton.onClick.AddListener(OnDiscardButtonClicked);
    }

    /// <summary>
    /// ������ Ÿ�Կ� ���� ��ư���� Ȱ��ȭ ���¸� �����մϴ�.
    /// </summary>
    private void ShowButtonsByItemType()
    {
        // ��� ��ư�� �ϴ� ��Ȱ��ȭ�մϴ�.
        equipButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);

        // ������ Ÿ�Կ� ���� �ʿ��� ��ư�� Ȱ��ȭ�մϴ�.
        if (currentItem != null)
        {
            switch (currentItem.itemType)
            {
                case ItemType.Equipment:
                    equipButton.gameObject.SetActive(true);
                    discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Consumable:
                    useButton.gameObject.SetActive(true);
                    discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Material:
                case ItemType.Quest:
                    discardButton.gameObject.SetActive(true);
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
    /// PlayerEquipmentManager���� ������ ��û�մϴ�.
    /// </summary>
    public void OnEquipButtonClicked()
    {
        // ��� ���������� Ȯ���ϰ� ĳ�����մϴ�.
        EquipmentItemSO equipItem = currentItem as EquipmentItemSO;
        if (equipItem != null)
        {
            // PlayerEquipmentManager���� ���� ��û�� �մϴ�.
            // �κ��丮���� �������� �����ϴ� ������ PlayerEquipmentManager�� ó���մϴ�.
            PlayerEquipmentManager.Instance.EquipItem(equipItem);

            // ��ư Ŭ�� �� �г��� �ı��մϴ�.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// '���' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// InventoryManager�� ����� ��û�մϴ�.
    /// </summary>
    public void OnUseButtonClicked()
    {
        // �Ҹ�ǰ ���������� Ȯ���ϰ� ĳ�����մϴ�.
        ConsumableItemSO consumeItem = currentItem as ConsumableItemSO;
        if (consumeItem != null)
        {
            InventoryManager.Instance.UseItem(consumeItem);
            // ��ư Ŭ�� �� �г��� �ı��մϴ�.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// '������' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// InventoryUIController�� Ȯ��â ǥ�ø� ��û�մϴ�.
    /// </summary>
    public void OnDiscardButtonClicked()
    {
        if (currentItem != null && currentItemCount > 0)
        {
            // InventoryUIController�� Ȯ��â�� ��쵵�� ��û�մϴ�.
            // ���� ������ ������ ConfirmPanel���� ó���˴ϴ�.
            InventoryUIController.Instance.ShowDiscardConfirmPanel(currentItem, currentItemCount);
            // ��ư Ŭ�� �� �г��� �ı��մϴ�.
            Destroy(gameObject);
        }
    }
}