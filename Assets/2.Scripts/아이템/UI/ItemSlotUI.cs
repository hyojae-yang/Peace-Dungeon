using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// �κ��丮 ������ ���� UI�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ ������ ������ ǥ���ϰ�, ���콺 �̺�Ʈ�� ó���մϴ�.
/// ��� �������� ��� PlayerEquipmentManager���� ���� ��û�� �����ϴ� ������ �մϴ�.
/// </summary>
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // === �ν����Ϳ� �Ҵ��� ���� ���� ===
    [Tooltip("�������� �������� ǥ���� Image ������Ʈ�Դϴ�.")]
    [SerializeField] private Image iconImage;

    [Tooltip("�������� ������ ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI countText;

    [Header("UI ���� ����")]
    [Tooltip("������ ��Ŭ�� �� ������ ��ư �г� �������Դϴ�.")]
    [SerializeField] private GameObject buttonPanelPrefab;

    [Tooltip("������ ������ ǥ���� �������Դϴ�.")]
    [SerializeField] private GameObject tooltipPrefab;

    [Tooltip("��ư �г��� ���콺 �����ͷκ��� �󸶳� �������� ��Ÿ���� �����մϴ�.")]
    private Vector3 buttonPanelOffset = new Vector3(-50, 0, 0);

    [Tooltip("���� �г��� ���콺 �����ͷκ��� �󸶳� �������� ��Ÿ���� �����մϴ�.")]
    private Vector3 tooltipOffset = new Vector3(-200, 50, 0);

    // === ���� ������ ���� ===
    private ItemData currentItemData;

    // ���� ���Կ� ������ ��ư �г� �ν��Ͻ��� �����մϴ�.
    private static GameObject instantiatedButtonPanel;
    // ���� ���Կ� ������ ���� �ν��Ͻ��� �����մϴ�.
    private GameObject instantiatedTooltip;

    /// <summary>
    /// ������ ������ �ð��� ������ ������Ʈ�ϴ� �޼����Դϴ�.
    /// InventoryUIController���� ItemData�� �޾ƿ� ������ �����մϴ�.
    /// </summary>
    /// <param name="itemData">���Կ� �Ҵ�� ItemData (null�� ��� ������ ���ϴ�)</param>
    public void UpdateSlot(ItemData itemData)
    {
        currentItemData = itemData;

        // ItemData�� ��ȿ����(null�� �ƴ���) Ȯ���մϴ�.
        if (currentItemData != null && currentItemData.itemSO != null)
        {
            // ������ �� �ؽ�Ʈ�� ������Ʈ�մϴ�.
            iconImage.sprite = currentItemData.itemSO.itemIcon;
            iconImage.color = Color.white;
            iconImage.type = Image.Type.Simple;
            iconImage.enabled = true;

            // �������� ������ �� �ִ� ��쿡�� ������ ǥ���մϴ�.
            if (currentItemData.itemSO.maxStack > 1)
            {
                countText.text = currentItemData.stackCount.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                // �������� �ʴ� �������� ������ ǥ������ �ʽ��ϴ�.
                countText.gameObject.SetActive(false);
            }
        }
        else
        {
            // �������� null�̸� ������ ���ϴ�.
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
            iconImage.enabled = false;
            countText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ���� ���Կ� �Ҵ�� ������ ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>BaseItemSO ��ü. ������� ��� null.</returns>
    public BaseItemSO GetItem()
    {
        return currentItemData?.itemSO;
    }

    /// <summary>
    /// ���� ���Կ� �ִ� �������� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>������ ����</returns>
    public int GetItemCount()
    {
        return currentItemData?.stackCount ?? 0;
    }

    // === ���콺 �̺�Ʈ ó�� ===

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ� �������� �� ȣ��˴ϴ�.
    /// ���� �������� �����ϰ� ��ġ�� �����մϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���Կ� ������ ������ �ְ�, ���� �������� �Ҵ�Ǿ� ������, ������ ���� �������� �ʾҴٸ�
        if (currentItemData != null && currentItemData.itemSO != null && tooltipPrefab != null && instantiatedTooltip == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                instantiatedTooltip = Instantiate(tooltipPrefab, canvas.transform);

                // ���콺 ��ġ�� �������� �����Ͽ� ���� ��ġ�� �����մϴ�.
                instantiatedTooltip.transform.position = Input.mousePosition + tooltipOffset;

                // ������ ���� ��ũ��Ʈ�� ã�� ������ ������ �����մϴ�.
                ItemTooltip tooltip = instantiatedTooltip.GetComponent<ItemTooltip>();
                if (tooltip != null)
                {
                    tooltip.SetupTooltip(currentItemData.itemSO);
                }
            }
        }
    }

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ��� ����� �� ȣ��˴ϴ�.
    /// ������ ������ �ı��մϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerExit(PointerEventData eventData)
    {
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
        // 1. ��Ŭ�� ��, ��ư �г��� ǥ���մϴ�.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
        // 2. ��Ŭ�� ��, ��ư �г��� ����ϴ�.
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (instantiatedButtonPanel != null)
            {
                Destroy(instantiatedButtonPanel);
                instantiatedButtonPanel = null; // ���� ����
            }
        }
    }

    /// <summary>
    /// ������ ��Ŭ�� �� ��ư �г��� Ȱ��ȭ�ϰ� ��ġ�� �����մϴ�.
    /// ������ Ÿ�Կ� ���� ��ư�� ����� �����մϴ�.
    /// </summary>
    private void OnRightClick()
    {
        if (currentItemData != null && currentItemData.itemSO != null)
        {
            // ������ ������ ��ư �г��� �ִٸ� �ı��մϴ�.
            if (instantiatedButtonPanel != null)
            {
                Destroy(instantiatedButtonPanel);
            }

            // ���콺 ��ġ�� ��ư �г��� ���� �����մϴ�.
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                instantiatedButtonPanel = Instantiate(buttonPanelPrefab, canvas.transform);
            }

            // ���콺 ������ ��ġ�� �������� �����մϴ�.
            instantiatedButtonPanel.transform.position = Input.mousePosition + buttonPanelOffset;
            instantiatedButtonPanel.SetActive(true);

            // ��ư �г� ��ũ��Ʈ�� Initialize �޼��带 ȣ���Ͽ� ��ư�� �����մϴ�.
            // ������ Ÿ�Կ� ���� �ٸ� ����� �����մϴ�.
            ButtonPanel buttonPanel = instantiatedButtonPanel.GetComponent<ButtonPanel>();
            if (buttonPanel != null)
            {
                // ButtonPanel�� Initialize �޼��带 ȣ���Ͽ� ��ư ����� �����մϴ�.
                buttonPanel.Initialize(currentItemData.itemSO, currentItemData.stackCount);
            }
        }
    }
}