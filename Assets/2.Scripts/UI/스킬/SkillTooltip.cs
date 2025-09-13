using UnityEngine;
using TMPro;

// �� ��ũ��Ʈ�� ��ų ���� UI �г��� ������ �����ϰ�,
// �������� ��ġ�� ���� ������ ��ġ�� �������� �����մϴ�.
public class SkillTooltip : MonoBehaviour
{
    // === UI ������Ʈ ===
    [Header("UI ������Ʈ")]
    [Tooltip("���� �г��� RectTransform")]
    public RectTransform tooltipRectTransform;
    [Tooltip("��ų �̸��� ǥ���� Text ������Ʈ")]
    public TextMeshProUGUI skillNameText;
    [Tooltip("��ų ������ ǥ���� Text ������Ʈ")]
    public TextMeshProUGUI skillDescriptionText;
    [Tooltip("��ų ������ ǥ���� Text ������Ʈ")]
    public TextMeshProUGUI skillLevelText;

    [Header("��ġ ���� ����")]
    [Tooltip("������ �����ܿ��� �󸶳� �������� �����ϴ� �������Դϴ�.")]
    public float offset = 10f;

    void Awake()
    {
        // RectTransform�� �Ҵ���� �ʾҴٸ�, �� ���ӿ�����Ʈ�� RectTransform�� �����ɴϴ�.
        if (tooltipRectTransform == null)
        {
            tooltipRectTransform = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// �ܺο��� ��ų �����͸� �޾ƿ� ������ ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="data">�� �������� ��ǥ�� ��ų�� SkillData</param>
    /// <param name="currentLevel">���� ��ų�� ����</param>
    public void SetTooltipData(SkillData data, int currentLevel)
    {
        if (data != null)
        {
            skillNameText.text = data.skillName;
            skillDescriptionText.text = data.skillDescription;
            skillLevelText.text = $"Lv. {currentLevel}"; // currentLevel ���� ���� ���
        }
        else
        {
            Debug.LogWarning("�Ҵ�� ��ų �����Ͱ� �����ϴ�. ���� ������Ʈ ����.");
        }
    }

    /// <summary>
    /// ��ų �������� ��ġ�� �������� ������ ��ġ�� �����մϴ�.
    /// </summary>
    /// <param name="iconPosition">��ų �������� ȭ��� ��ġ (���� ��ǥ)</param>
    public void AdjustPosition(Vector3 iconPosition)
    {
        // �������� ȭ�� ��ǥ�� ���մϴ�.
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(iconPosition);

        // ������ RectTransform�� �����մϴ�.
        // ��ų �������� ȭ���� ���ʿ� ������ ������ �����ʿ�,
        // �����ʿ� ������ ������ ���ʿ� ��Ÿ���� �մϴ�.
        if (screenPoint.x < Screen.width / 2)
        {
            // �������� ���ʿ� ���� ���: ������ �����ʿ� ǥ��
            tooltipRectTransform.pivot = new Vector2(0, 0.5f);
            tooltipRectTransform.anchoredPosition = new Vector2(offset, 0);
        }
        else
        {
            // �������� �����ʿ� ���� ���: ������ ���ʿ� ǥ��
            tooltipRectTransform.pivot = new Vector2(1, 0.5f);
            tooltipRectTransform.anchoredPosition = new Vector2(-offset, 0);
        }
    }
}