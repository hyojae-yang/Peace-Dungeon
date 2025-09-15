// ButtonPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ������ ��Ŭ�� �� ��Ÿ���� ��ư �г��� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ Ÿ�Կ� ���� ������ ��ư�� Ȱ��ȭ/��Ȱ��ȭ�մϴ�.
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

    /// <summary>
    /// ItemSlotUI���� ȣ��Ǿ� ���� ������ ������ �ް�, ��ư�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="item">���� ���Կ� �ִ� ������ ������</param>
    public void Initialize(BaseItemSO item)
    {
        currentItem = item;
        ShowButtonsByItemType();
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

    // === ��ư Ŭ�� �� ȣ��� �޼��� (���� InventoryManager�� ����) ===

    /// <summary>
    /// '����' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnEquipButtonClicked()
    {
        Debug.Log($"���� ��ư Ŭ����: {currentItem.itemName}");
        // InventoryManager.Instance.EquipItem((EquipmentItemSO)currentItem);
    }

    /// <summary>
    /// '���' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnUseButtonClicked()
    {
        Debug.Log($"��� ��ư Ŭ����: {currentItem.itemName}");
        // InventoryManager.Instance.UseItem((ConsumableItemSO)currentItem);
    }

    /// <summary>
    /// '������' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnDiscardButtonClicked()
    {
        Debug.Log($"������ ��ư Ŭ����: {currentItem.itemName}");
        // InventoryUIController.Instance.ShowConfirmPanel(currentItem, OnConfirmDiscard);
    }

    // ���� ������ Ȯ��â���� ȣ��� �޼���
    // private void OnConfirmDiscard(int amount)
    // {
    //     Debug.Log($"{amount}�� ������ Ȯ��");
    //     // InventoryManager.Instance.RemoveItem(currentItem, amount);
    // }
}