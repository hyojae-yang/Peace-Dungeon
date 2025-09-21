using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// NPC의 퀘스트 관련 상호작용을 처리하는 스크립트입니다.
/// QuestGiver 컴포넌트와 연동하여 퀘스트 상태에 따른 대사를 제공하고,
/// UI 매니저를 통해 퀘스트 수락/취소 패널을 제어합니다.
/// SOLID: 단일 책임 원칙 (퀘스트 관련 로직).
/// </summary>
[RequireComponent(typeof(NPC))]
public class NPCQuestHandler : MonoBehaviour
{
    private NPC npc;
    private NPCInteraction npcInteraction; // 상호작용 종료를 위한 참조

    private void Awake()
    {
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("NPC 스크립트가 없습니다. NPCQuestHandler는 NPC 스크립트와 함께 사용되어야 합니다.");
        }
    }

    private void Start()
    {
        npcInteraction = GetComponent<NPCInteraction>();
        if (npcInteraction == null)
        {
            Debug.LogWarning("NPCInteraction 스크립트를 찾을 수 없습니다. 상호작용 종료 기능이 제한됩니다.");
        }
    }

    /// <summary>
    /// 퀘스트 목록에서 특정 퀘스트를 선택했을 때 호출됩니다.
    /// 선택된 퀘스트의 상태에 따라 대화를 시작하고 적절한 UI 패널을 띄웁니다.
    /// </summary>
    /// <param name="selectedQuest">선택된 퀘스트 데이터</param>
    /// <param name="state">현재 퀘스트의 상태</param>
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
    /// 퀘스트 관련 대화가 끝난 후 호출되어 퀘스트 상태에 맞는 UI를 표시합니다.
    /// </summary>
    private void HandleQuestAfterDialogue(QuestData data, QuestState state)
    {
        if (NPCUIManager.Instance == null)
        {
            Debug.LogError("NPCUIManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        switch (state)
        {
            case QuestState.Available:
                NPCUIManager.Instance.ShowQuestAcceptPanel(data, this, npcInteraction);
                break;
            case QuestState.Accepted:
                // 퀘스트 취소를 위한 대화가 끝난 후, 퀘스트 취소 패널을 띄웁니다.
                NPCUIManager.Instance.ShowQuestCancelPanel(data, this, npcInteraction);
                break;
            // 퀘스트 목표 달성 후 보상 패널을 띄우는 새로운 로직
            case QuestState.ReadyToComplete:
                // '완료 가능' 상태의 대화가 모두 끝난 후에 보상 패널을 띄웁니다.
                OnQuestComplete(data);
                break;
            case QuestState.Completed:
                // 이미 완료한 퀘스트이므로, 별도의 UI 없이 상호작용을 종료합니다.
                if (npcInteraction != null) npcInteraction.EndInteraction();
                break;
            default:
                if (npcInteraction != null) npcInteraction.EndInteraction();
                break;
        }
    }

    /// <summary>
    /// 퀘스트 상태에 따른 대화 내용을 가져옵니다. 특정 퀘스트의 ID를 인자로 받습니다.
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
    /// '퀘스트 수락' 버튼 클릭 시 호출됩니다.
    /// 퀘스트 수락 후 감사 대사 없이 즉시 UI를 숨기고 상호작용을 종료합니다.
    /// </summary>
    public void OnAcceptQuest(QuestData data)
    {
        QuestManager.Instance.AcceptQuest(data.questID);

        NPCUIManager.Instance.HideAllUI();
        if (npcInteraction != null) npcInteraction.EndInteraction();
    }

    /// <summary>
    /// '퀘스트 취소' 버튼 클릭 시 호출됩니다.
    /// 퀘스트 취소 후 즉시 UI를 숨기고 상호작용을 종료합니다.
    /// </summary>
    public void OnCancelQuest(QuestData data)
    {
        QuestManager.Instance.CancelQuest(data.questID);

        NPCUIManager.Instance.HideAllUI();
        if (npcInteraction != null) npcInteraction.EndInteraction();
    }

    /// <summary>
    /// 퀘스트 목표를 달성한 상태에서 최종적으로 보상을 받고 퀘스트를 완료 처리합니다.
    /// 이 메서드는 'ReadyToComplete' 상태의 대화가 모두 끝난 후 호출됩니다.
    /// </summary>
    private void OnQuestComplete(QuestData data)
    {
        // QuestManager에 퀘스트 완료를 알리고 보상을 지급합니다.
        QuestManager.Instance.CompleteQuest(data.questID, data);

        // 보상 패널을 띄웁니다.
        // 보상 패널의 '확인' 버튼을 누르면 대화가 최종적으로 종료됩니다.
        NPCUIManager.Instance.ShowQuestRewardPanel(data, npcInteraction);
    }
}