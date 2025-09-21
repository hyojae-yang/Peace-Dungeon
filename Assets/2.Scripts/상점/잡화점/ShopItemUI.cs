using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// ���� ��Ͽ� ǥ�õǴ� ���� ������ UI�� ������ �����մϴ�.
/// ����/�Ǹ� ��ư�� �ؽ�Ʈ�� ����� �������� �����մϴ�.
/// SOLID: ���� å�� ��Ģ (���� ������ UI ����).
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    // === UI ������Ʈ ���� ===
    [Tooltip("������ �̹����� ǥ���� Image ������Ʈ�Դϴ�.")]
    [SerializeField]
    private Image itemIcon;

    [Tooltip("������ �̸��� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI itemName;

    [Tooltip("������ ������ ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI itemPrice;

    [Tooltip("���� �Ǵ� �Ǹ� ������ ������ ��ư�Դϴ�.")]
    [SerializeField]
    private Button actionButton;

    [Tooltip("��ư�� �ؽ�Ʈ�� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI actionButtonText;

    /// <summary>
    /// ������ UI�� �־��� �����ͷ� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// �� �޼���� ShopUIManager���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="itemSO">ǥ���� �������� BaseItemSO �������Դϴ�.</param>
    /// <param name="buttonText">��ư�� ǥ���� �ؽ�Ʈ�Դϴ�. (��: "����", "�Ǹ�")</param>
    /// <param name="onButtonClick">��ư Ŭ�� �� ����� �����Դϴ�.</param>
    public void Setup(BaseItemSO itemSO, string buttonText, Action onButtonClick)
    {
        // Null üũ: �Ű������� ��ȿ���� Ȯ���մϴ�.
        if (itemSO == null)
        {
            Debug.LogError("ShopItemUI: Setup �޼��忡 ��ȿ���� ���� BaseItemSO�� ���޵Ǿ����ϴ�.");
            return;
        }

        // ������ ���� ǥ��
        if (itemIcon != null)
        {
            itemIcon.sprite = itemSO.itemIcon;
        }
        if (itemName != null)
        {
            itemName.text = itemSO.itemName;
        }
        if (itemPrice != null)
        {
            // G�� Gold�� ���ڷ� �����մϴ�.
            itemPrice.text = $"{itemSO.itemPrice} G";
        }

        // ��ư �ʱ�ȭ �� �̺�Ʈ ����
        if (actionButton != null)
        {
            // ���� �����ʸ� ��� �����Ͽ� �ߺ� ������ �����մϴ�.
            actionButton.onClick.RemoveAllListeners();
            // ���ο� �����ʸ� �߰��մϴ�.
            actionButton.onClick.AddListener(() => onButtonClick?.Invoke());
        }

        // ��ư �ؽ�Ʈ ����
        if (actionButtonText != null)
        {
            actionButtonText.text = buttonText;
        }
    }
}