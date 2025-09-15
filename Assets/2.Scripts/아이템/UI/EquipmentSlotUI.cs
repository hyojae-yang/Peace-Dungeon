// EquipmentSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// �÷��̾� ��� ���� UI�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���콺 ����, Ŭ�� �� �̺�Ʈ�� ó���ϰ�, ��� ���� ������ InventoryManager�� ��û�մϴ�.
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === ��� ������ ���� ���� ���� (����Ƽ �ν����Ϳ��� ����) ===
    [Tooltip("�� ������ ����ϴ� ��� ���� ������ �����մϴ�. (��: Weapon, Helmet, Accessory1 ��)")]
    public EquipSlot equipSlotType; // ���� ���� ����

    // === ���� ���� ===
    [Tooltip("��� �������� �������� ǥ���� Image ������Ʈ�Դϴ�.")]
    [SerializeField] private Image iconImage;

    // === ���� ������ ===
    private EquipmentItemSO currentEquippedItem;

    /// <summary>
    /// ��� ���Կ� �������� �ð������� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="item">������ ������ ������ (null�� ��� ������ ���ϴ�)</param>
    public void UpdateSlot(EquipmentItemSO item)
    {
        currentEquippedItem = item;

        if (currentEquippedItem != null)
        {
            // �������� �����ϸ� �������� ǥ���ϰ� �̹����� ������ �������ϰ� ����ϴ�.
            iconImage.sprite = currentEquippedItem.itemIcon;
            iconImage.color = Color.white; // �������� ���̵��� ���� ����
        }
        else
        {
            // �������� ������ �������� ����� �����ϰ� ����ϴ�.
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // ������ �����ϰ� ����ϴ�.
        }
    }

    // === ���콺 �̺�Ʈ ó�� ===

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ� �������� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ������ �������� ���� ���� ������ ���ϴ�.
        if (currentEquippedItem != null)
        {
            // TODO: ���� TooltipManager�� ȣ���Ͽ� ������ ǥ���մϴ�.
            Debug.Log($"[EquipmentSlotUI] ���� ǥ�� ��û: {currentEquippedItem.itemName} (Slot: {equipSlotType})");
        }
    }

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ��� ����� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        // TooltipManager�� ȣ���Ͽ� ������ ����ϴ�.
        // ���� ���� ����: TooltipManager.Instance.HideTooltip();
        Debug.Log($"[EquipmentSlotUI] ���� ���� ��û");
    }

    /// <summary>
    /// ���콺 Ŭ�� �̺�Ʈ�� ó���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // ���콺 ������ ��ư Ŭ���� �����մϴ�.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    /// <summary>
    /// ��� ���� ������ InventoryManager���� ��û�ϴ� �޼����Դϴ�.
    /// </summary>
    public void OnRightClick()
    {
        if (currentEquippedItem != null)
        {
            // InventoryManager�� ��� ���� ������ ��û�մϴ�.
            InventoryManager.Instance.UnEquipItem(equipSlotType);
            Debug.Log($"[EquipmentSlotUI] ��� ���� ��û: {currentEquippedItem.itemName} (Slot: {equipSlotType})");
        }
    }
}