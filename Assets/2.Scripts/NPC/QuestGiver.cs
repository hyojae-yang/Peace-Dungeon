using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ư�� ����Ʈ�� �÷��̾�� �����ϴ� NPC�� �����Ǵ� ������Ʈ�Դϴ�.
/// QuestGiver�� ����Ʈ ������ ���µ��� �����ϰ�, �÷��̾��� ����Ʈ ���¿� ����
/// � ����Ʈ�� �������� �Ǵ��ϴ� ������ ����մϴ�.
/// </summary>
public class QuestGiver : MonoBehaviour
{
    // === ����Ʈ ������ ===
    [Tooltip("�� NPC�� �����ϴ� ����Ʈ ������ ����Դϴ�. �����Ϳ��� �Ҵ��մϴ�.")]
    [SerializeField]
    private List<QuestData> questDatas = new List<QuestData>();

    // === NPC ���� ���� ===
    private NPC npc;

    /// <summary>
    /// MonoBehaviour�� Awake �޼���.
    /// </summary>
    private void Awake()
    {
        // ���� ���� ������Ʈ�� ������ NPC ������Ʈ ����
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("QuestGiver ��ũ��Ʈ�� ���� ������Ʈ�� NPC ��ũ��Ʈ�� �ʿ��մϴ�!");
        }
    }

    /// <summary>
    /// �� NPC�� �����ϴ� ��� ����Ʈ �����͸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>NPC�� �����ϴ� ����Ʈ ������ ���.</returns>
    public List<QuestData> GetQuestDatas()
    {
        return questDatas;
    }

    /// <summary>
    /// �÷��̾ ���� �� NPC�� ��ȣ�ۿ����� �� ������ ����Ʈ�� ���¸� �����մϴ�.
    /// ����Ʈ ���� �켱����: Complete > Accepted > Available
    /// </summary>
    /// <returns>���� ���� �켱������ ����Ʈ ���� (������ QuestState.None)</returns>
    public QuestState GetHighestPriorityQuestState()
    {
        QuestData currentQuest = null;

        // 1. �Ϸ� ������ ����Ʈ�� �ִ��� Ȯ��
        currentQuest = questDatas.FirstOrDefault(q =>
            QuestManager.Instance.IsQuestAccepted(q.questID) && QuestManager.Instance.CheckQuestCompletion(q));
        if (currentQuest != null)
        {
            return QuestState.Complete;
        }

        // 2. �̹� ������ ����Ʈ�� �ִ��� Ȯ��
        currentQuest = questDatas.FirstOrDefault(q => QuestManager.Instance.IsQuestAccepted(q.questID));
        if (currentQuest != null)
        {
            return QuestState.Accepted;
        }

        // 3. ���� ������ ����Ʈ�� �ִ��� Ȯ��
        int currentAffection = (npc != null) ? npc.GetAffection() : 0;
        currentQuest = questDatas.FirstOrDefault(q =>
            !QuestManager.Instance.IsQuestAccepted(q.questID) && !QuestManager.Instance.IsQuestCompleted(q.questID) && currentAffection >= q.requiredAffection);
        if (currentQuest != null)
        {
            return QuestState.Available;
        }

        // ���� ��� ���ǿ� �ش����� ������ �⺻ ���¸� ��ȯ
        return QuestState.None;
    }
}