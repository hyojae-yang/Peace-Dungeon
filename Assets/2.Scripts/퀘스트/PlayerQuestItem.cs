using UnityEngine;
using TMPro;

/// <summary>
/// �÷��̾� ����Ʈ �гο� ǥ�õǴ� ���� ����Ʈ �׸��� �����ϴ� ��ũ��Ʈ.
/// QuestManager�κ��� ���� ������ UI�� ǥ���ϴ� ���� å���� �����ϴ�.
/// </summary>
public class PlayerQuestItem : MonoBehaviour
{
    [Tooltip("����Ʈ �̸��� ǥ���� �ؽ�Ʈ ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI questNameText;

    [Tooltip("����Ʈ�� �� NPC�� �̸��� ǥ���� �ؽ�Ʈ ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI giverNameText;

    [Tooltip("����Ʈ ���� ��Ȳ�� ǥ���� �ؽ�Ʈ ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI progressText;

    /// <summary>
    /// ����Ʈ ������ �����ϰ� UI�� ������Ʈ�ϴ� �޼����Դϴ�.
    /// �� �޼���� QuestManager�κ��� ������ ���ڿ��� �޾� ��� UI�� �����մϴ�.
    /// SOLID: ���� å�� ��Ģ (UI ǥ��).
    /// </summary>
    /// <param name="questName">ǥ���� ����Ʈ �̸�.</param>
    /// <param name="questGiverName">����Ʈ�� �� NPC�� �̸�.</param>
    /// <param name="progressTextContent">����Ʈ ���� ��Ȳ�� ��Ÿ���� ������ ���ڿ�.</param>
    public void SetQuestInfo(string questName, string questGiverName, string progressTextContent)
    {
        // null üũ�� ���� �������� Ȯ���մϴ�.
        if (questNameText != null)
        {
            questNameText.text = questName;
        }

        if (giverNameText != null)
        {
            giverNameText.text = questGiverName;
        }

        if (progressText != null)
        {
            progressText.text = progressTextContent;
        }
    }
}