// ItemSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// �κ��丮 ������ ���� UI�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ ������ ������ ǥ���ϰ�, ���콺 �̺�Ʈ�� ó���մϴ�.
/// InventoryManager���� ������ ���/���� ��û�� �����ϴ� ������ �մϴ�.
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

    [Tooltip("��ư �г��� ���콺 �����ͷκ��� �󸶳� �������� ��Ÿ���� �����մϴ�.")]
    private Vector3 buttonPanelOffset = new Vector3(-50, 0, 0);

    // === ���� ������ ���� ===
    [SerializeField]
    private BaseItemSO currentItem;
    private int itemCount;

    // ���� ���Կ� ������ ��ư �г� �ν��Ͻ��� �����մϴ�.
    private static GameObject instantiatedButtonPanel;

    /// <summary>
    /// ������ ������ �ð��� ������ ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="item">���Կ� �Ҵ�� ������ ������ (null�� ��� ������ ���ϴ�)</param>
    /// <param name="count">�������� ����</param>
    public void UpdateSlot(BaseItemSO item, int count)
    {
        currentItem = item;
        itemCount = count; // ����: itemCount += count; -> itemCount = count;

        // ������ ������ 0 ���ϸ� ������ ���ϴ�.
        if (itemCount <= 0)
        {
            item = null;
            currentItem = null;
        }

        if (currentItem != null)
        {
            iconImage.sprite = currentItem.itemIcon;
            iconImage.color = Color.white;
            iconImage.type = Image.Type.Simple;
            iconImage.enabled = true;
            countText.text = itemCount.ToString();
            countText.gameObject.SetActive(true);
        }
        else
        {
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
        return currentItem;
    }

    /// <summary>
    /// ���� ���Կ� �ִ� �������� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>������ ����</returns>
    public int GetItemCount()
    {
        return itemCount;
    }

    // === ���콺 �̺�Ʈ ó�� ===

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ� �������� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            Debug.Log($"���� ǥ��: {currentItem.itemName}");
        }
    }

    /// <summary>
    /// ���콺 �����Ͱ� UI ���Կ��� ����� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="eventData">���콺 �̺�Ʈ ������</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("���� ����");
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
            }
        }
    }

    /// <summary>
    /// ������ ��Ŭ�� �� ��ư �г��� Ȱ��ȭ�ϰ� ��ġ�� �����մϴ�.
    /// </summary>
    private void OnRightClick()
    {
        if (currentItem != null)
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
            instantiatedButtonPanel.GetComponent<ButtonPanel>().Initialize(currentItem);
        }
    }
}