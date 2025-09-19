using UnityEngine;
using TMPro;

/// <summary>
/// ����Ʈ ��� UI�� ǥ�õǴ� ���� �׸��� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ����Ʈ �̸��� ���� ���¸� ǥ���ϰ�, ��ư Ŭ�� �̺�Ʈ�� ó���մϴ�.
/// </summary>
public class QuestListItem : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("����Ʈ �̸��� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI questTitleText;

    [Tooltip("����Ʈ ���¸� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI questStatusText;

    // �� �׸��� ��Ÿ���� ����Ʈ�� �����͸� �����մϴ�.
    private QuestData questData;
    // �� �׸��� ��Ÿ���� ����Ʈ�� ���� ���¸� �����մϴ�.
    private QuestState questState;

    /// <summary>
    /// ����Ʈ �׸��� �����͸� �����ϰ� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="data">ǥ���� ����Ʈ ������.</param>
    /// <param name="state">����Ʈ�� ���� ����.</param>
    public void SetQuestData(QuestData data, QuestState state)
    {
        // ����Ʈ �����͸� �����մϴ�.
        this.questData = data;
        this.questState = state;

        // UI�� ����Ʈ �����Ϳ� ���¿� �°� ������Ʈ�մϴ�.
        if (questTitleText != null)
        {
            questTitleText.text = data.questTitle;
        }

        if (questStatusText != null)
        {
            // ����Ʈ ���¿� ���� �ٸ� �ؽ�Ʈ�� ������ �����մϴ�.
            string statusText = GetStatusText(state);
            questStatusText.text = statusText;
            questStatusText.color = GetStatusColor(state);
        }
    }

    /// <summary>
    /// ����Ʈ ���¿� ���� �ؽ�Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="state">����Ʈ ����.</param>
    /// <returns>���¿� �´� ���ڿ�.</returns>
    private string GetStatusText(QuestState state)
    {
        switch (state)
        {
            case QuestState.Available:
                return "���� ����";
            case QuestState.Accepted:
                return "���� ��";
            case QuestState.Completed:
                return "�Ϸ�";
            default:
                return "���� �� �� ����";
        }
    }

    /// <summary>
    /// ����Ʈ ���¿� ���� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <param name="state">����Ʈ ����.</param>
    /// <returns>���¿� �´� ����.</returns>
    private Color GetStatusColor(QuestState state)
    {
        switch (state)
        {
            case QuestState.Available:
                return Color.green; // ���� ����
            case QuestState.Accepted:
                return Color.yellow; // ���� ��
            case QuestState.Completed:
                return Color.cyan; // �Ϸ�
            default:
                return Color.white;
        }
    }
}