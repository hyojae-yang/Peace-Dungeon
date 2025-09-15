using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ������ ������ Ȯ��â�� UI�� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ����ڿ��� ���� ������ �Է¹ް� ���������� InventoryManager�� ���Ÿ� ��û�մϴ�.
/// </summary>
public class ConfirmPanel : MonoBehaviour
{
    // === �ν����Ϳ� �Ҵ��� UI ������Ʈ ===
    [Tooltip("�������� �������� ǥ���� Image ������Ʈ�Դϴ�.")]
    [SerializeField] private Image itemIconImage;

    [Tooltip("�������� �̸��� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("���� ������ �Է¹��� InputField ������Ʈ�Դϴ�. ��� �������� ��� null�� �� �ֽ��ϴ�.")]
    [SerializeField] private TMP_InputField countInputField;

    [Tooltip("'Ȯ��' ��ư ������Ʈ�Դϴ�.")]
    [SerializeField] private Button confirmButton;

    [Tooltip("'���' ��ư ������Ʈ�Դϴ�.")]
    [SerializeField] private Button cancelButton;

    // === ���� ������ ���� ===
    private BaseItemSO currentItem;
    private int currentItemCount;

    private void Awake()
    {
        // ��ư�� Ŭ�� �̺�Ʈ �����ʸ� �߰��մϴ�.
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    /// <summary>
    /// InventoryUIController�κ��� ������ ������ �޾� UI�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="item">���� �������� ������</param>
    /// <param name="count">�����ϰ� �ִ� �������� �� ����</param>
    public void Initialize(BaseItemSO item, int count)
    {
        // ���޹��� �����͸� ���� ������ �����մϴ�.
        this.currentItem = item;
        this.currentItemCount = count;

        // UI�� ������Ʈ�մϴ�.
        itemIconImage.sprite = item.itemIcon;
        itemNameText.text = item.itemName;

        // countInputField�� �Ҵ�Ǿ� �ִٸ�(null�� �ƴ϶��) �ؽ�Ʈ�� �ʱ�ȭ�մϴ�.
        if (countInputField != null)
        {
            countInputField.text = "1"; // �ʱ� �Է°��� 1�� �����մϴ�.
        }
    }

    /// <summary>
    /// 'Ȯ��' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// �Էµ� ������ŭ �������� ������ ������ �����մϴ�.
    /// </summary>
    private void OnConfirmButtonClicked()
    {
        int countToDiscard = 0;

        // countInputField�� �Ҵ�Ǿ� �ִٸ� ���� ��������, �ƴϸ� �⺻���� 1�� ����մϴ�.
        if (countInputField != null)
        {
            // �Է� �ʵ��� �ؽ�Ʈ�� ������ ��ȯ�մϴ�.
            if (!int.TryParse(countInputField.text, out countToDiscard))
            {
                Debug.LogWarning("���ڸ� �Է����ּ���.");
                return;
            }
        }
        else
        {
            // ��� �����۰� ���� InputField�� ���� ���, ���� ������ 1�� �����մϴ�.
            countToDiscard = 1;
        }

        // ��ȿ�� �˻�: 0���� ũ�� ���� �������� �۰ų� ������ Ȯ���մϴ�.
        if (countToDiscard > 0 && countToDiscard <= currentItemCount)
        {
            // InventoryManager�� ������ ���Ÿ� ��û�մϴ�.
            InventoryManager.Instance.RemoveItem(currentItem, countToDiscard);

            // �۾� �Ϸ� �� Ȯ��â�� �ݽ��ϴ�.
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("�߸��� �Է°��Դϴ�! ���� ������ �ٽ� Ȯ�����ּ���.");
            // ����ڿ��� ��� �޽����� ǥ���ϴ� UI�� �߰��� �� �ֽ��ϴ�.
        }
    }

    /// <summary>
    /// '���' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnCancelButtonClicked()
    {
        // Ȯ��â�� �ݽ��ϴ�.
        gameObject.SetActive(false);
    }
}