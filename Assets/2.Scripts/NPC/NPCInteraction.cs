using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어와 NPC 간의 상호작용을 담당하는 스크립트.
/// 플레이어의 접근을 감지하고, E키 입력 시 대화의 흐름을 제어합니다.
/// </summary>
public class NPCInteraction : MonoBehaviour
{
    // 플레이어와의 상호작용을 시작할 수 있는 최대 거리입니다.
    [Header("Interaction Settings")]
    [Tooltip("플레이어가 상호작용할 수 있는 최대 거리입니다.")]
    [SerializeField]
    private float interactionRange = 3f;

    // 플레이어의 Transform 컴포넌트입니다.
    private Transform playerTransform;
    // 상호작용 중인지 여부를 나타내는 플래그입니다.
    private bool isInteracting = false;

    // 이 NPCInteraction 스크립트가 부착된 NPC 스크립트의 참조
    private NPC npc;

    // 현재 대화 진행 상태
    private string[] currentDialogues;
    private int dialogueIndex = 0;

    //----------------------------------------------------------------------------------------------------------------
    // MonoBehaviour 생명주기 메서드
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// MonoBehaviour의 Start 메서드. 게임 시작 시 한 번 호출됩니다.
    /// </summary>
    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("NPC 스크립트가 없습니다! NPCInteraction 스크립트는 NPC 스크립트와 함께 사용되어야 합니다.");
        }
    }

    /// <summary>
    /// MonoBehaviour의 Update 메서드. 매 프레임 호출됩니다.
    /// </summary>
    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionRange)
        {
            NPCUIManager.Instance.ShowInteractionPrompt(true);

            if (Input.GetKeyDown(KeyCode.E) && !isInteracting)
            {
                StartInteraction();
            }
        }
        else
        {
            if (!isInteracting)
            {
                NPCUIManager.Instance.ShowInteractionPrompt(false);
            }
            else
            {
                EndInteraction();
            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 상호작용 로직 메서드
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 플레이어와 상호작용을 시작합니다.
    /// </summary>
    private void StartInteraction()
    {
        if (NPCUIManager.Instance == null)
        {
            Debug.LogError("NPCUIManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        isInteracting = true;

        if (npc.TryGetComponent(out NPCMovement npcMovement))
        {
            npcMovement.SetIsTalking(true);
        }

        // 초기 대사 가져오기
        string[] initialDialogue = GetInteractionDialogue();

        StartCoroutine(RunDialogue(initialDialogue, false, () =>
        {
            NPCUIManager.Instance.ShowMainButtons(this.npc);
        }));

        NPCUIManager.Instance.AddDialogueButtonListener(StartDialogue);
        NPCUIManager.Instance.AddQuestButtonListener(StartQuestInteraction);
        NPCUIManager.Instance.AddNextButtonListener(OnNextDialogue);
    }

    /// <summary>
    /// 플레이어와 상호작용을 종료합니다.
    /// </summary>
    public void EndInteraction()
    {
        isInteracting = false;
        NPCUIManager.Instance.HideAllUI();
        if (npc.TryGetComponent(out NPCMovement npcMovement))
        {
            npcMovement.SetIsTalking(false);
        }
    }

    /// <summary>
    /// '대화하기' 버튼을 누르면 일반 대화를 시작합니다.
    /// </summary>
    private void StartDialogue()
    {
        string[] dialogues = GetGeneralDialogueBasedOnStateAndAffection();
        StartCoroutine(RunDialogue(dialogues, true, () => NPCUIManager.Instance.ShowMainButtons(this.npc)));
    }

    /// <summary>
    /// '퀘스트' 버튼을 누르면 퀘스트 상호작용을 시작합니다.
    /// </summary>
    private void StartQuestInteraction()
    {
        QuestUIManager.Instance.ShowQuestList(npc.QuestGiver, HandleQuestFlow);
    }

    /// <summary>
    /// 대화 패널의 '다음' 버튼을 누르면 다음 대사로 넘어갑니다.
    /// </summary>
    private void OnNextDialogue()
    {
        dialogueIndex++;
    }

    /// <summary>
    /// 퀘스트 선택 후 대화와 패널 처리를 총괄하는 메서드입니다.
    /// </summary>
    public void HandleQuestFlow(QuestData selectedQuest, QuestState state)
    {
        string[] dialogues = GetDialogueBasedOnQuestState(state, selectedQuest.questID);
        StartCoroutine(RunDialogue(dialogues, true, () => HandleQuestAfterDialogue(selectedQuest, state)));
    }

    /// <summary>
    /// 대화 코루틴: 주어진 대화 배열을 순서대로 표시합니다.
    /// </summary>
    /// <param name="dialogues">표시할 대사 배열</param>
    /// <param name="enableNextButton">'다음' 버튼을 활성화할지 여부</param>
    /// <param name="onDialogueEnd">대화가 끝난 후 실행할 액션</param>
    private IEnumerator RunDialogue(string[] dialogues, bool enableNextButton, Action onDialogueEnd)
    {
        currentDialogues = dialogues;
        dialogueIndex = 0;

        NPCUIManager.Instance.ShowDialoguePanel(npc.Data.npcName, currentDialogues[dialogueIndex]);
        NPCUIManager.Instance.ToggleNextButton(enableNextButton && (dialogueIndex < currentDialogues.Length - 1));

        if (!enableNextButton)
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E));
            onDialogueEnd?.Invoke();
        }
        else
        {
            while (dialogueIndex < currentDialogues.Length - 1)
            {
                yield return new WaitUntil(() => dialogueIndex < currentDialogues.Length - 1);
                NPCUIManager.Instance.ShowDialoguePanel(npc.Data.npcName, currentDialogues[dialogueIndex]);
                NPCUIManager.Instance.ToggleNextButton(true);
            }

            NPCUIManager.Instance.ShowDialoguePanel(npc.Data.npcName, currentDialogues[currentDialogues.Length - 1]);
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E));
            onDialogueEnd?.Invoke();
        }
    }

    /// <summary>
    /// 퀘스트 선택 후 대화가 끝났을 때 호출되어 다음 UI를 결정합니다.
    /// </summary>
    private void HandleQuestAfterDialogue(QuestData data, QuestState state)
    {
        if (state == QuestState.Available)
        {
            NPCUIManager.Instance.ShowQuestAcceptPanel();
        }
        else if (state == QuestState.Accepted)
        {
            NPCUIManager.Instance.ShowQuestCancelPanel();
        }
        else if (state == QuestState.Complete)
        {
            OnQuestComplete(data);
        }
        else
        {
            EndInteraction();
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 기타 메서드 (최종 수정 버전)
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// NPC와의 첫 상호작용 대사를 가져옵니다. 호감도와 퀘스트 상태에 따라 대사가 결정됩니다.
    /// 여러 개의 대사 중 하나를 무작위로 선택합니다.
    /// </summary>
    private string[] GetInteractionDialogue()
    {
        QuestGiver questGiver = npc.QuestGiver;
        QuestState currentQuestState = QuestState.None;
        if (questGiver != null && questGiver.GetQuestDatas().Count > 0)
        {
            currentQuestState = questGiver.GetHighestPriorityQuestState();
        }
        DialogueGroup dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == currentQuestState);
        if (dialogueGroup == null)
        {
            dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == QuestState.None);
        }
        if (dialogueGroup == null)
        {
            return new string[] { "..." };
        }
        int currentAffection = npc.GetAffection();
        AffectionDialogue affectionDialogue = dialogueGroup.interactionDialogue.FirstOrDefault(
            ad => currentAffection >= ad.minAffection && currentAffection < ad.maxAffection);

        if (affectionDialogue != null && affectionDialogue.dialogueTexts.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, affectionDialogue.dialogueTexts.Length);
            return new string[] { affectionDialogue.dialogueTexts[randomIndex] };
        }

        return dialogueGroup.interactionDialogue.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
    }

    /// <summary>
    /// 퀘스트 상태와 호감도에 따른 일반 대화 내용을 가져옵니다.
    /// </summary>
    private string[] GetGeneralDialogueBasedOnStateAndAffection()
    {
        QuestGiver questGiver = npc.QuestGiver;
        QuestState currentQuestState = QuestState.None;
        if (questGiver != null && questGiver.GetQuestDatas().Count > 0)
        {
            currentQuestState = questGiver.GetHighestPriorityQuestState();
        }
        DialogueGroup dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == currentQuestState);
        if (dialogueGroup == null)
        {
            dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == QuestState.None);
        }
        if (dialogueGroup == null)
        {
            return new string[] { "..." };
        }
        int currentAffection = npc.GetAffection();
        AffectionDialogue affectionDialogue = dialogueGroup.generalDialogues.FirstOrDefault(
            ad => currentAffection >= ad.minAffection && currentAffection < ad.maxAffection);
        return affectionDialogue?.dialogueTexts ?? dialogueGroup.generalDialogues.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
    }

    /// <summary>
    /// 퀘스트 상태에 따른 대화 내용을 가져옵니다. 특정 퀘스트의 ID를 인자로 받습니다.
    /// </summary>
    /// **int 타입으로 매개변수를 받도록 수정되었습니다.**
    private string[] GetDialogueBasedOnQuestState(QuestState state, int questID)
    {
        if (npc?.Data?.dialogueGroups == null)
        {
            Debug.LogError("NPC 데이터 또는 대화 그룹이 없습니다.");
            return new string[] { "..." };
        }

        // npc.Data.dialogueGroups에서 questState와 questID가 모두 일치하는 DialogueGroup을 찾습니다.
        DialogueGroup dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == state && dg.questID == questID);

        if (dialogueGroup == null)
        {
            // 해당하는 퀘스트 상태가 없으면 기본 대화를 반환합니다.
            return npc.Data.dialogueGroups.FirstOrDefault(dg => dg.questState == QuestState.None)?.generalDialogues.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
        }

        int currentAffection = npc.GetAffection();
        AffectionDialogue affectionDialogue = dialogueGroup.generalDialogues.FirstOrDefault(ad => currentAffection >= ad.minAffection && currentAffection < ad.maxAffection);

        return affectionDialogue?.dialogueTexts ?? dialogueGroup.generalDialogues.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
    }

    /// <summary>
    /// '퀘스트 수락' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnAcceptQuest(QuestData data)
    {
        QuestManager.Instance.AcceptQuest(data.questID);
        string[] newDialogues = GetDialogueBasedOnQuestState(QuestState.Accepted, data.questID);
        StartCoroutine(RunDialogue(newDialogues, true, EndInteraction));
    }

    /// <summary>
    /// '퀘스트 취소' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnCancelQuest(QuestData data)
    {
        QuestManager.Instance.CancelQuest(data.questID);
        string[] newDialogues = GetDialogueBasedOnQuestState(QuestState.Available, data.questID);
        StartCoroutine(RunDialogue(newDialogues, true, EndInteraction));
    }

    /// <summary>
    /// '퀘스트 완료' 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnQuestComplete(QuestData data)
    {
        QuestManager.Instance.CompleteQuest(data.questID, data);
        string[] newDialogues = GetDialogueBasedOnQuestState(QuestState.Completed, data.questID);
        StartCoroutine(RunDialogue(newDialogues, true, EndInteraction));
    }
}