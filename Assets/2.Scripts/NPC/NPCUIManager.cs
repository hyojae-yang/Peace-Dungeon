using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// NPC와 관련된 UI를 관리하는 싱글턴 클래스입니다.
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 모든 UI 패널 숨기기
        HideAllUI();
    }

    //----------------------------------------------------------------------------------------------------------------
    // NPC Interaction에서 호출될 메서드들
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

        npcNameText.text = npcName;
        dialogueText.text = dialogue;
    }

    public void ShowMainButtons(NPC npc)
    {
        dialoguePanel.SetActive(true);
        mainButtonsPanel.SetActive(true);
        questAcceptPanel.SetActive(false);
        questCancelPanel.SetActive(false);
        questListPanel.SetActive(false);

        // 퀘스트 버튼 활성화/비활성화
        bool hasQuests = npc != null && npc.QuestGiver != null && npc.QuestGiver.GetQuestDatas().Count > 0;
        if (questButton != null)
        {
            questButton.gameObject.SetActive(hasQuests);
        }

        // 특수 버튼 활성화/비활성화 및 텍스트 변경
        if (specialButton != null && specialButtonText != null)
        {
            SetSpecialButton(npc);
        }

        // 초기 대사 후에는 다음 버튼을 비활성화
        ToggleNextButton(false);
    }

    public void ShowQuestAcceptPanel()
    {
        questAcceptPanel.SetActive(true);
        mainButtonsPanel.SetActive(false);
    }

    public void ShowQuestCancelPanel()
    {
        questCancelPanel.SetActive(true);
        mainButtonsPanel.SetActive(false);
    }

    public void HideAllUI()
    {
        interactionPrompt.SetActive(false);
        dialoguePanel.SetActive(false);
        mainButtonsPanel.SetActive(false);
        questAcceptPanel.SetActive(false);
        questCancelPanel.SetActive(false);
        questListPanel.SetActive(false);
    }

    /// <summary>
    /// NPC의 컴포넌트에 따라 특수 버튼을 설정합니다.
    /// </summary>
    /// <param name="npc">현재 상호작용 중인 NPC</param>
    public void SetSpecialButton(NPC npc)
    {
        specialButton.gameObject.SetActive(false); // 기본적으로 비활성화

        /*
        // TODO: 나중에 Shopkeeper 컴포넌트 추가 후 주석을 해제하고 사용
        if (npc.TryGetComponent<Shopkeeper>(out var shopkeeper))
        {
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = "상점";
            specialButton.onClick.RemoveAllListeners();
            specialButton.onClick.AddListener(() => shopkeeper.OpenShop());
        }
        // TODO: 나중에 Blacksmith 컴포넌트 추가 후 주석을 해제하고 사용
        else if (npc.TryGetComponent<Blacksmith>(out var blacksmith))
        {
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = "대장간";
            specialButton.onClick.RemoveAllListeners();
            specialButton.onClick.AddListener(() => blacksmith.OpenBlacksmith());
        }
        */
        // 다른 특수 컴포넌트가 있다면 여기에 추가
    }

    //----------------------------------------------------------------------------------------------------------------
    // 버튼 이벤트 리스너 추가/제거
    //----------------------------------------------------------------------------------------------------------------

    public void AddDialogueButtonListener(UnityEngine.Events.UnityAction action)
    {
        dialogueButton.onClick.RemoveAllListeners();
        dialogueButton.onClick.AddListener(action);
    }

    public void AddQuestButtonListener(UnityEngine.Events.UnityAction action)
    {
        questButton.onClick.RemoveAllListeners();
        questButton.onClick.AddListener(action);
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