using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// NPC와 관련된 UI를 관리하는 싱글턴 클래스입니다.
/// 다른 스크립트(NPCInteraction, NPCQuestHandler)의 요청에 따라 UI를 표시/숨깁니다.
/// SOLID: 단일 책임 원칙 (UI 표시/숨기기).
/// </summary>
public class NPCUIManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static NPCUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("상호작용 프롬프트 UI (예: 'E' 키)")]
    public GameObject interactionPrompt;
    [Tooltip("대화 패널 UI")]
    public GameObject dialoguePanel;
    [Tooltip("메인 대화 버튼 패널 (대화하기, 퀘스트)")]
    public GameObject mainButtonsPanel;
    [Tooltip("퀘스트 수락 패널")]
    public GameObject questAcceptPanel;
    [Tooltip("퀘스트 취소 패널")]
    public GameObject questCancelPanel;
    [Tooltip("퀘스트 목록 패널")]
    public GameObject questListPanel;
    [Tooltip("퀘스트 보상 패널입니다.")]
    public GameObject questRewardPanel;

    [Header("UI Elements")]
    [Tooltip("NPC 이름 텍스트")]
    public TextMeshProUGUI npcNameText;
    [Tooltip("대화 내용 텍스트")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("대화하기 버튼")]
    public Button dialogueButton;
    [Tooltip("퀘스트 버튼")]
    public Button questButton;
    [Tooltip("대화 패널의 '다음' 버튼")]
    public Button nextButton;
    [Tooltip("상점, 대장간 등 특수 기능 버튼")]
    public Button specialButton;
    [Tooltip("특수 버튼의 텍스트")]
    public TextMeshProUGUI specialButtonText;

    // 퀘스트 수락/취소 패널의 버튼들
    [Header("Quest Panels Buttons")]
    [Tooltip("퀘스트 수락 패널의 '수락' 버튼입니다.")]
    public Button acceptQuestButton;
    [Tooltip("퀘스트 수락 패널의 '거절' 버튼입니다.")]
    public Button rejectQuestButton;
    [Tooltip("퀘스트 취소 패널의 '확인' 버튼입니다.")]
    public Button confirmCancelButton;
    [Tooltip("퀘스트 취소 패널의 '취소' 버튼입니다.")]
    public Button cancelQuestButton;
    [Tooltip("퀘스트 보상 패널의 '확인' 버튼입니다.")]
    public Button rewardPanelConfirmButton;

    [Header("Quest Reward UI")]
    [Tooltip("보상 아이템 이름 텍스트")]
    public TextMeshProUGUI rewardItemNameText;
    [Tooltip("보상 경험치 텍스트")]
    public TextMeshProUGUI rewardExpText;
    [Tooltip("보상 골드 텍스트")]
    public TextMeshProUGUI rewardGoldText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideAllUI();
    }

    //----------------------------------------------------------------------------------------------------------------
    // UI 표시/숨기기
    //----------------------------------------------------------------------------------------------------------------

    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }

    public void ShowDialoguePanel(string npcName, string dialogue)
    {
        dialoguePanel.SetActive(true);
        mainButtonsPanel.SetActive(false);
        questAcceptPanel.SetActive(false);
        questCancelPanel.SetActive(false);
        questListPanel.SetActive(false);
        questRewardPanel.SetActive(false);

        if (npcNameText != null) npcNameText.text = npcName;
        if (dialogueText != null) dialogueText.text = dialogue;
    }

    public void ShowMainButtons(NPC npc)
    {
        dialoguePanel.SetActive(true);
        mainButtonsPanel.SetActive(true);
        questAcceptPanel.SetActive(false);
        questCancelPanel.SetActive(false);
        questListPanel.SetActive(false);
        questRewardPanel.SetActive(false);

        bool hasQuests = npc != null && npc.QuestGiver != null && npc.QuestGiver.GetQuestDatas().Count > 0;
        if (questButton != null)
        {
            questButton.gameObject.SetActive(hasQuests);
        }

        if (specialButton != null && specialButtonText != null)
        {
            SetSpecialButton(npc);
        }
    }

    public void ShowQuestAcceptPanel(QuestData data, NPCQuestHandler handler, NPCInteraction interaction)
    {

        questAcceptPanel.SetActive(true);
        mainButtonsPanel.SetActive(false);
        questRewardPanel.SetActive(false);

        if (acceptQuestButton != null && rejectQuestButton != null)
        {
            acceptQuestButton.onClick.RemoveAllListeners();
            rejectQuestButton.onClick.RemoveAllListeners();

            acceptQuestButton.onClick.AddListener(() => handler.OnAcceptQuest(data));
            rejectQuestButton.onClick.AddListener(interaction.EndInteraction);
        }
    }

    public void ShowQuestCancelPanel(QuestData data, NPCQuestHandler handler, NPCInteraction interaction)
    {

        questCancelPanel.SetActive(true);
        mainButtonsPanel.SetActive(false);
        questRewardPanel.SetActive(false);

        if (confirmCancelButton != null && cancelQuestButton != null)
        {
            confirmCancelButton.onClick.RemoveAllListeners();
            cancelQuestButton.onClick.RemoveAllListeners();

            confirmCancelButton.onClick.AddListener(() => handler.OnCancelQuest(data));
            cancelQuestButton.onClick.AddListener(interaction.EndInteraction);
        }
    }

    /// <summary>
    /// 퀘스트 완료 보상 패널을 표시합니다.
    /// </summary>
    /// <param name="data">퀘스트 데이터(보상 정보 포함)</param>
    /// <param name="interaction">상호작용 종료를 위한 NPCInteraction 컴포넌트</param>
    public void ShowQuestRewardPanel(QuestData data, NPCInteraction interaction)
    {
        HideAllUI();
        questRewardPanel.SetActive(true);

        UpdateRewardTexts(data);

        if (rewardPanelConfirmButton != null)
        {
            rewardPanelConfirmButton.onClick.RemoveAllListeners();
            rewardPanelConfirmButton.onClick.AddListener(() => interaction.EndInteraction());
        }
    }

    public void HideAllUI()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (questAcceptPanel != null) questAcceptPanel.SetActive(false);
        if (questCancelPanel != null) questCancelPanel.SetActive(false);
        if (questListPanel != null) questListPanel.SetActive(false);
        if (questRewardPanel != null) questRewardPanel.SetActive(false);
    }

    /// <summary>
    /// NPC가 가진 특수 기능에 따라 특수 버튼을 설정합니다.
    /// </summary>
    /// <param name="npc">현재 상호작용 중인 NPC 컴포넌트</param>
    public void SetSpecialButton(NPC npc)
    {
        // 1. NPC에게 특수 기능 목록을 요청합니다.
        List<INPCFunction> functions = npc.GetSpecialFunctions();

        // 2. 특수 기능이 하나라도 존재하는지 확인합니다.
        if (functions != null && functions.Count > 0)
        {
            // 3. 기능이 있다면 버튼을 활성화하고, 첫 번째 기능의 이름으로 텍스트를 설정합니다.
            // 현재는 여러 기능 중 첫 번째 기능만 표시합니다.
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = functions[0].FunctionButtonName;
        }
        else
        {
            // 4. 기능이 없다면 버튼을 비활성화합니다.
            specialButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 보상 패널의 텍스트를 업데이트합니다.
    /// </summary>
    /// <param name="data">퀘스트 데이터(보상 정보 포함)</param>
    private void UpdateRewardTexts(QuestData data)
    {
        // 보상 아이템 정보
        if (rewardItemNameText != null)
        {
            if (data.rewardItems.Count > 0)
            {
                string itemString = "";
                for (int i = 0; i < data.rewardItems.Count; i++)
                {
                    // ItemSO가 null인지 확인하여 에러를 방지합니다.
                    if (data.rewardItems[i].itemSO != null)
                    {
                        string itemName = data.rewardItems[i].itemSO.itemName;
                        itemString += data.rewardItems[i].itemCount > 0 ? $"{itemName} ({data.rewardItems[i].itemCount}개)" : itemName;
                    }
                    else
                    {
                        itemString += "유효하지 않은 아이템";
                    }

                    if (i < data.rewardItems.Count - 1)
                    {
                        itemString += ", ";
                    }
                }
                rewardItemNameText.text = $"보상 아이템: {itemString}";
            }
            else
            {
                rewardItemNameText.text = "보상 아이템: 없음";
            }
        }

        // 경험치 보상 업데이트
        if (rewardExpText != null)
        {
            rewardExpText.text = data.experienceReward > 0 ? $"보상 경험치: +{data.experienceReward}" : "보상 경험치: 없음";
        }

        // 골드 보상 업데이트
        if (rewardGoldText != null)
        {
            rewardGoldText.text = data.goldReward > 0 ? $"보상 골드: +{data.goldReward}" : "보상 골드: 없음";
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 버튼 이벤트 리스너 추가/제거 (변경 없음)
    //----------------------------------------------------------------------------------------------------------------

    public void AddDialogueButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (dialogueButton != null)
        {
            dialogueButton.onClick.RemoveAllListeners();
            dialogueButton.onClick.AddListener(action);
        }
    }

    public void AddQuestButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(action);
        }
    }

    public void AddNextButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(action);
        }
    }

    public void ToggleNextButton(bool active)
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(active);
        }
    }
}