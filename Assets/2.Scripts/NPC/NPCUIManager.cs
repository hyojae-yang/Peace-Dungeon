using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// NPC�� ���õ� UI�� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// </summary>
public class NPCUIManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static NPCUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("��ȣ�ۿ� ������Ʈ UI (��: 'E' Ű)")]
    public GameObject interactionPrompt;
    [Tooltip("��ȭ �г� UI")]
    public GameObject dialoguePanel;
    [Tooltip("���� ��ȭ ��ư �г� (��ȭ�ϱ�, ����Ʈ)")]
    public GameObject mainButtonsPanel;
    [Tooltip("����Ʈ ���� �г�")]
    public GameObject questAcceptPanel;
    [Tooltip("����Ʈ ��� �г�")]
    public GameObject questCancelPanel;
    [Tooltip("����Ʈ ��� �г�")]
    public GameObject questListPanel;

    [Header("UI Elements")]
    [Tooltip("NPC �̸� �ؽ�Ʈ")]
    public TextMeshProUGUI npcNameText;
    [Tooltip("��ȭ ���� �ؽ�Ʈ")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("��ȭ�ϱ� ��ư")]
    public Button dialogueButton;
    [Tooltip("����Ʈ ��ư")]
    public Button questButton;
    [Tooltip("��ȭ �г��� '����' ��ư")]
    public Button nextButton;
    [Tooltip("����, ���尣 �� Ư�� ��� ��ư")]
    public Button specialButton;
    [Tooltip("Ư�� ��ư�� �ؽ�Ʈ")]
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
        // ��� UI �г� �����
        HideAllUI();
    }

    //----------------------------------------------------------------------------------------------------------------
    // NPC Interaction���� ȣ��� �޼����
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

        // ����Ʈ ��ư Ȱ��ȭ/��Ȱ��ȭ
        bool hasQuests = npc != null && npc.QuestGiver != null && npc.QuestGiver.GetQuestDatas().Count > 0;
        if (questButton != null)
        {
            questButton.gameObject.SetActive(hasQuests);
        }

        // Ư�� ��ư Ȱ��ȭ/��Ȱ��ȭ �� �ؽ�Ʈ ����
        if (specialButton != null && specialButtonText != null)
        {
            SetSpecialButton(npc);
        }

        // �ʱ� ��� �Ŀ��� ���� ��ư�� ��Ȱ��ȭ
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
    /// NPC�� ������Ʈ�� ���� Ư�� ��ư�� �����մϴ�.
    /// </summary>
    /// <param name="npc">���� ��ȣ�ۿ� ���� NPC</param>
    public void SetSpecialButton(NPC npc)
    {
        specialButton.gameObject.SetActive(false); // �⺻������ ��Ȱ��ȭ

        /*
        // TODO: ���߿� Shopkeeper ������Ʈ �߰� �� �ּ��� �����ϰ� ���
        if (npc.TryGetComponent<Shopkeeper>(out var shopkeeper))
        {
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = "����";
            specialButton.onClick.RemoveAllListeners();
            specialButton.onClick.AddListener(() => shopkeeper.OpenShop());
        }
        // TODO: ���߿� Blacksmith ������Ʈ �߰� �� �ּ��� �����ϰ� ���
        else if (npc.TryGetComponent<Blacksmith>(out var blacksmith))
        {
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = "���尣";
            specialButton.onClick.RemoveAllListeners();
            specialButton.onClick.AddListener(() => blacksmith.OpenBlacksmith());
        }
        */
        // �ٸ� Ư�� ������Ʈ�� �ִٸ� ���⿡ �߰�
    }

    //----------------------------------------------------------------------------------------------------------------
    // ��ư �̺�Ʈ ������ �߰�/����
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