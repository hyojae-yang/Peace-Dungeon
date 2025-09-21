using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// NPC�� ����Ʈ ���� ��ȣ�ۿ��� ó���ϴ� ��ũ��Ʈ�Դϴ�.
/// QuestGiver ������Ʈ�� �����Ͽ� ����Ʈ ���¿� ���� ��縦 �����ϰ�,
/// UI �Ŵ����� ���� ����Ʈ ����/��� �г��� �����մϴ�.
/// SOLID: ���� å�� ��Ģ (����Ʈ ���� ����).
/// </summary>
[RequireComponent(typeof(NPC))]
public class NPCQuestHandler : MonoBehaviour
{
    private NPC npc;
    private NPCInteraction npcInteraction; // ��ȣ�ۿ� ���Ḧ ���� ����

    private void Awake()
    {
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("NPC ��ũ��Ʈ�� �����ϴ�. NPCQuestHandler�� NPC ��ũ��Ʈ�� �Բ� ���Ǿ�� �մϴ�.");
        }
    }

    private void Start()
    {
        npcInteraction = GetComponent<NPCInteraction>();
        if (npcInteraction == null)
        {
            Debug.LogWarning("NPCInteraction ��ũ��Ʈ�� ã�� �� �����ϴ�. ��ȣ�ۿ� ���� ����� ���ѵ˴ϴ�.");
        }
    }

    /// <summary>
    /// ����Ʈ ��Ͽ��� Ư�� ����Ʈ�� �������� �� ȣ��˴ϴ�.
    /// ���õ� ����Ʈ�� ���¿� ���� ��ȭ�� �����ϰ� ������ UI �г��� ���ϴ�.
    /// </summary>
    /// <param name="selectedQuest">���õ� ����Ʈ ������</param>
    /// <param name="state">���� ����Ʈ�� ����</param>
    public void HandleQuestFlow(QuestData selectedQuest, QuestState state)
    {
        string[] dialogues = GetDialogueBasedOnQuestState(state, selectedQuest.questID);

        Action onDialogueEnd = () =>
        {
            HandleQuestAfterDialogue(selectedQuest, state);
        };

        NPCDialogueController.Instance.StartDialogue(npc.Data.npcName, dialogues, onDialogueEnd);
    }

    /// <summary>
    /// ����Ʈ ���� ��ȭ�� ���� �� ȣ��Ǿ� ����Ʈ ���¿� �´� UI�� ǥ���մϴ�.
    /// </summary>
    private void HandleQuestAfterDialogue(QuestData data, QuestState state)
    {
        if (NPCUIManager.Instance == null)
        {
            Debug.LogError("NPCUIManager �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        switch (state)
        {
            case QuestState.Available:
                NPCUIManager.Instance.ShowQuestAcceptPanel(data, this, npcInteraction);
                break;
            case QuestState.Accepted:
                // ����Ʈ ��Ҹ� ���� ��ȭ�� ���� ��, ����Ʈ ��� �г��� ���ϴ�.
                NPCUIManager.Instance.ShowQuestCancelPanel(data, this, npcInteraction);
                break;
            // ����Ʈ ��ǥ �޼� �� ���� �г��� ���� ���ο� ����
            case QuestState.ReadyToComplete:
                // '�Ϸ� ����' ������ ��ȭ�� ��� ���� �Ŀ� ���� �г��� ���ϴ�.
                OnQuestComplete(data);
                break;
            case QuestState.Completed:
                // �̹� �Ϸ��� ����Ʈ�̹Ƿ�, ������ UI ���� ��ȣ�ۿ��� �����մϴ�.
                if (npcInteraction != null) npcInteraction.EndInteraction();
                break;
            default:
                if (npcInteraction != null) npcInteraction.EndInteraction();
                break;
        }
    }

    /// <summary>
    /// ����Ʈ ���¿� ���� ��ȭ ������ �����ɴϴ�. Ư�� ����Ʈ�� ID�� ���ڷ� �޽��ϴ�.
    /// </summary>
    private string[] GetDialogueBasedOnQuestState(QuestState state, int questID)
    {
        if (npc?.Data?.dialogueGroups == null)
        {
            return new string[] { "..." };
        }

        DialogueGroup dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == state && dg.questID == questID);

        if (dialogueGroup == null)
        {
            DialogueGroup generalDialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == QuestState.None);
            if (generalDialogueGroup != null)
            {
                return generalDialogueGroup.generalDialogues.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
            }
            else
            {
                return new string[] { "..." };
            }
        }
        else
        {
            int currentAffection = npc.GetAffection();
            AffectionDialogue affectionDialogue = dialogueGroup.generalDialogues.FirstOrDefault(ad => currentAffection >= ad.minAffection && currentAffection < ad.maxAffection);

            if (affectionDialogue != null)
            {
                return affectionDialogue.dialogueTexts;
            }
            else
            {
                return dialogueGroup.generalDialogues.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
            }
        }
    }

    /// <summary>
    /// '����Ʈ ����' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// ����Ʈ ���� �� ���� ��� ���� ��� UI�� ����� ��ȣ�ۿ��� �����մϴ�.
    /// </summary>
    public void OnAcceptQuest(QuestData data)
    {
        QuestManager.Instance.AcceptQuest(data.questID);

        NPCUIManager.Instance.HideAllUI();
        if (npcInteraction != null) npcInteraction.EndInteraction();
    }

    /// <summary>
    /// '����Ʈ ���' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// ����Ʈ ��� �� ��� UI�� ����� ��ȣ�ۿ��� �����մϴ�.
    /// </summary>
    public void OnCancelQuest(QuestData data)
    {
        QuestManager.Instance.CancelQuest(data.questID);

        NPCUIManager.Instance.HideAllUI();
        if (npcInteraction != null) npcInteraction.EndInteraction();
    }

    /// <summary>
    /// ����Ʈ ��ǥ�� �޼��� ���¿��� ���������� ������ �ް� ����Ʈ�� �Ϸ� ó���մϴ�.
    /// �� �޼���� 'ReadyToComplete' ������ ��ȭ�� ��� ���� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnQuestComplete(QuestData data)
    {
        // QuestManager�� ����Ʈ �ϷḦ �˸��� ������ �����մϴ�.
        QuestManager.Instance.CompleteQuest(data.questID, data);

        // ���� �г��� ���ϴ�.
        // ���� �г��� 'Ȯ��' ��ư�� ������ ��ȭ�� ���������� ����˴ϴ�.
        NPCUIManager.Instance.ShowQuestRewardPanel(data, npcInteraction);
    }
}