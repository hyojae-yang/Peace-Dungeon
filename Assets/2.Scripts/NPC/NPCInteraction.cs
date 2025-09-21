using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어와 NPC 간의 물리적 상호작용을 담당하는 스크립트입니다.
/// 플레이어의 접근을 감지하고, E키 입력 시 상호작용을 시작합니다.
/// SOLID: 단일 책임 원칙 (물리적 상호작용).
/// </summary>
[RequireComponent(typeof(NPC))]
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

    // 이 NPC 스크립트와 퀘스트 핸들러 참조
    private NPC npc;
    private NPCQuestHandler questHandler;

    //----------------------------------------------------------------------------------------------------------------
    // MonoBehaviour 생명주기 메서드
    //----------------------------------------------------------------------------------------------------------------

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

        questHandler = GetComponent<NPCQuestHandler>();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionRange)
        {
            if (NPCUIManager.Instance != null)
            {
                NPCUIManager.Instance.ShowInteractionPrompt(true);
            }

            if (Input.GetKeyDown(KeyCode.E) && !isInteracting)
            {
                StartInteraction();
            }
        }
        else
        {
            if (!isInteracting)
            {
                if (NPCUIManager.Instance != null)
                {
                    NPCUIManager.Instance.ShowInteractionPrompt(false);
                }
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
    /// 플레이어와 상호작용을 시작합니다. 이제 버튼 패널이 즉시 활성화됩니다.
    /// </summary>
    private void StartInteraction()
    {
        if (NPCUIManager.Instance == null || NPCDialogueController.Instance == null)
        {
            Debug.LogError("필수 매니저 스크립트(NPCUIManager 또는 NPCDialogueController)를 찾을 수 없습니다.");
            return;
        }

        isInteracting = true;

        // 마우스 커서를 보이게 하고 잠금을 해제하는 부분은 유지합니다.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (npc.TryGetComponent(out NPCMovement npcMovement))
        {
            npcMovement.SetIsTalking(true);
        }

        // 1. NPC와 상호작용 시작 시 메인 버튼 패널을 즉시 활성화합니다.
        NPCUIManager.Instance.ShowMainButtons(this.npc);
        NPCUIManager.Instance.AddDialogueButtonListener(StartGeneralDialogue);

        if (questHandler != null)
        {
            NPCUIManager.Instance.AddQuestButtonListener(StartQuestInteraction);
        }
        else
        {
            NPCUIManager.Instance.questButton.gameObject.SetActive(false);
        }

        // 🚨 새로 추가된 부분: NPC가 특수 기능을 가지고 있는지 확인하고 버튼 리스너를 추가합니다.
        if (npc.HasSpecialFunction())
        {
            // NPC가 가진 모든 특수 기능 리스트를 가져옵니다.
            // NPC.cs 스크립트는 이미 NPCManager로부터 이 리스트를 가져오도록 수정되었습니다.
            List<INPCFunction> specialFunctions = npc.GetSpecialFunctions();
            if (specialFunctions.Count > 0)
            {
                // 특수 버튼에 리스너를 추가합니다.
                // 첫 번째 특수 기능의 ExecuteFunction() 메서드를 클릭 이벤트에 연결합니다.
                // SOLID: 개방-폐쇄 원칙. 새로운 기능이 추가돼도 이 코드는 수정할 필요가 없습니다.
                NPCUIManager.Instance.specialButton.onClick.RemoveAllListeners();
                NPCUIManager.Instance.specialButton.onClick.AddListener(() => specialFunctions[0].ExecuteFunction());
                NPCUIManager.Instance.specialButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // NPC가 특수 기능이 없을 경우 버튼을 비활성화합니다.
            NPCUIManager.Instance.specialButton.gameObject.SetActive(false);
        }

        // 2. 초기 대사를 시작합니다.
        string[] initialDialogue = GetInteractionDialogue();
        NPCDialogueController.Instance.StartDialogue(npc.Data.npcName, initialDialogue);
    }

    /// <summary>
    /// '대화하기' 버튼 클릭 시 일반 대화를 시작합니다.
    /// </summary>
    private void StartGeneralDialogue()
    {
        // 버튼 패널을 비활성화하고 대화 패널만 활성화합니다.
        NPCUIManager.Instance.mainButtonsPanel.SetActive(false);

        string[] dialogues = GetGeneralDialogueBasedOnStateAndAffection();

        // 수정된 부분: 대화 종료 시 호감도를 올리고 상호작용을 종료하는 콜백 함수를 만듭니다.
        Action onDialogueEnd = () =>
        {
            // NPCManager에 해당 NPC의 호감도를 1만큼 올리도록 요청합니다.
            // SOLID: 단일 책임 원칙 (호감도 변경 로직을 NPCManager에 위임)
            NPCManager.Instance.ChangeAffection(npc.Data.npcName, 1);

            // 최종적으로 상호작용을 종료합니다.
            EndInteraction();
        };

        NPCDialogueController.Instance.StartDialogue(npc.Data.npcName, dialogues, onDialogueEnd);
    }

    /// <summary>
    /// '퀘스트' 버튼 클릭 시 퀘스트 핸들러에게 처리를 위임합니다.
    /// </summary>
    private void StartQuestInteraction()
    {
        NPCUIManager.Instance.mainButtonsPanel.SetActive(false);

        if (questHandler != null)
        {
            QuestUIManager.Instance.ShowQuestList(npc.QuestGiver, (selectedQuest, state) =>
            {
                NPCUIManager.Instance.questListPanel.SetActive(false);
                questHandler.HandleQuestFlow(selectedQuest, state);
            });
        }
        else
        {
            Debug.LogWarning("NPCQuestHandler가 이 NPC에 부착되어 있지 않습니다. 퀘스트 기능을 사용할 수 없습니다.");
            EndInteraction();
        }
    }

    /// <summary>
    /// 플레이어와 상호작용을 종료합니다.
    /// </summary>
    public void EndInteraction()
    {
        isInteracting = false;
        NPCUIManager.Instance.HideAllUI();
        NPCDialogueController.Instance.HideDialogueUI();

        // 마우스 커서 상태를 원래대로 복구하는 코드를 주석 처리하여 마우스가 보이도록 유지합니다.
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;

        if (npc.TryGetComponent(out NPCMovement npcMovement))
        {
            npcMovement.SetIsTalking(false);
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 대사 데이터 가져오기 (리팩토링 후 남은 부분)
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// NPC와의 첫 상호작용 대사를 가져옵니다. 호감도와 퀘스트 상태에 따라 대사가 결정됩니다.
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
    /// <summary>
    /// NPC의 퀘스트 상태와 관계없이, 오직 일상 대화(None, Completed)를 가져옵니다.
    /// 플레이어의 호감도에 따라 적절한 대사를 선택합니다.
    /// </summary>
    /// <returns>NPC가 말할 일상 대사 문자열 배열입니다.</returns>
    private string[] GetGeneralDialogueBasedOnStateAndAffection()
    {
        // 1. 일상 대화에 해당하는 QuestState 목록을 정의합니다.
        List<QuestState> casualDialogueStates = new List<QuestState> { QuestState.None, QuestState.Completed };

        // 2. 정의된 상태에 해당하는 대화 그룹을 찾습니다.
        // 이 때, 여러 개의 그룹이 있을 수 있으므로 FirstOrDefault를 사용해 첫 번째 유효한 그룹을 가져옵니다.
        DialogueGroup dialogueGroup = npc.Data.dialogueGroups.FirstOrDefault(dg => casualDialogueStates.Contains(dg.questState));

        // 3. 만약 일상 대화 그룹이 없다면, 기본 대사를 반환합니다.
        if (dialogueGroup == null)
        {
            return new string[] { "...", "별일 없으신가요?" };
        }

        // 4. 현재 NPC의 호감도를 가져옵니다.
        int currentAffection = npc.GetAffection();

        // 5. 호감도 범위에 맞는 대사를 찾습니다.
        // 호감도 조건(minAffection <= currentAffection < maxAffection)을 만족하는 첫 번째 대사를 찾습니다.
        AffectionDialogue affectionDialogue = dialogueGroup.generalDialogues.FirstOrDefault(
            ad => currentAffection >= ad.minAffection && currentAffection < ad.maxAffection);

        // 6. 호감도에 맞는 대사가 있다면 해당 대사를 반환하고,
        // 없다면 해당 그룹의 첫 번째 대사를 반환합니다.
        // 이마저도 없다면 기본 대사를 반환하여 오류를 방지합니다.
        return affectionDialogue?.dialogueTexts ?? dialogueGroup.generalDialogues.FirstOrDefault()?.dialogueTexts ?? new string[] { "..." };
    }
}