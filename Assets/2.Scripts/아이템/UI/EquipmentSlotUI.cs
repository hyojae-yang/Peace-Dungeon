using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// �÷��̾� ��� ���� UI�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���콺 ����, Ŭ�� �� �̺�Ʈ�� ó���ϰ�, ��� ���� ������ PlayerEquipmentManager�� ��û�մϴ�.
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === ��� ������ ���� ���� ���� (����Ƽ �ν����Ϳ��� ����) ===
    [Tooltip("�� ������ ����ϴ� ��� ���� ������ �����մϴ�. (��: Weapon, Helmet, Accessory1 ��)")]
    public EquipSlot equipSlotType; // ���� ���� ����

    // === ���� ���� ===
    [Tooltip("��� �������� �������� ǥ���� Image ������Ʈ�Դϴ�.")]
    [SerializeField] private Image iconImage;

    [Header("UI ���� ����")]
    [Tooltip("��� ������ ������ ǥ���� �������Դϴ�.")]
    [SerializeField] private GameObject tooltipPrefab;

    [Tooltip("���� �г��� ���콺 �����ͷκ��� �󸶳� �������� ��Ÿ���� �����մϴ�.")]
    private Vector3 tooltipOffset = new Vector3(-200, 50, 0);

    // === ���� ������ ===
    private EquipmentItemSO currentEquippedItem;
    private GameObject instantiatedTooltip;

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
    /// ������ �������� ���� ���� ������ ���ϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ������ �������� �ְ�, ���� �������� �Ҵ�Ǿ� ������, ������ ���� �������� �ʾҴٸ�
        if (currentEquippedItem != null && tooltipPrefab != null && instantiatedTooltip == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                instantiatedTooltip = Instantiate(tooltipPrefab, canvas.transform);
                instantiatedTooltip.transform.position = Input.mousePosition + tooltipOffset;

                // ������ ���� ��ũ��Ʈ�� ã�� ������ ������ �����մϴ�.
                ItemTooltip tooltip = instantiatedTooltip.GetComponent<ItemTooltip>();
                if (tooltip != null)
                {
                    tooltip.SetupTooltip(currentEquippedItem);
                }
            }
        }
    }

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ��� ����� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        // ������ ������ �ı��մϴ�.
        if (instantiatedTooltip != null)
        {
            Destroy(instantiatedTooltip);
            instantiatedTooltip = null;
        }
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
    /// ��� ���� ������ PlayerEquipmentManager���� ��û�ϴ� �޼����Դϴ�.
    /// </summary>
    public void OnRightClick()
    {
        if (currentEquippedItem != null)
        {
            // PlayerEquipmentManager�� ��� ���� ������ ��û�մϴ�.
            Debug.Log($"[EquipmentSlotUI] ��� ���� ��û: {currentEquippedItem.itemName} (Slot: {equipSlotType})");
            PlayerEquipmentManager.Instance.UnEquipItem(equipSlotType);
        }
    }
}