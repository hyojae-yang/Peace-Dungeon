using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// NPC�� ���õ� UI�� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// �ٸ� ��ũ��Ʈ(NPCInteraction, NPCQuestHandler)�� ��û�� ���� UI�� ǥ��/����ϴ�.
/// SOLID: ���� å�� ��Ģ (UI ǥ��/�����).
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
    [Tooltip("����Ʈ ���� �г��Դϴ�.")]
    public GameObject questRewardPanel;

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

    // ����Ʈ ����/��� �г��� ��ư��
    [Header("Quest Panels Buttons")]
    [Tooltip("����Ʈ ���� �г��� '����' ��ư�Դϴ�.")]
    public Button acceptQuestButton;
    [Tooltip("����Ʈ ���� �г��� '����' ��ư�Դϴ�.")]
    public Button rejectQuestButton;
    [Tooltip("����Ʈ ��� �г��� 'Ȯ��' ��ư�Դϴ�.")]
    public Button confirmCancelButton;
    [Tooltip("����Ʈ ��� �г��� '���' ��ư�Դϴ�.")]
    public Button cancelQuestButton;
    [Tooltip("����Ʈ ���� �г��� 'Ȯ��' ��ư�Դϴ�.")]
    public Button rewardPanelConfirmButton;

    [Header("Quest Reward UI")]
    [Tooltip("���� ������ �̸� �ؽ�Ʈ")]
    public TextMeshProUGUI rewardItemNameText;
    [Tooltip("���� ����ġ �ؽ�Ʈ")]
    public TextMeshProUGUI rewardExpText;
    [Tooltip("���� ��� �ؽ�Ʈ")]
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
    // UI ǥ��/�����
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
    /// ����Ʈ �Ϸ� ���� �г��� ǥ���մϴ�.
    /// </summary>
    /// <param name="data">����Ʈ ������(���� ���� ����)</param>
    /// <param name="interaction">��ȣ�ۿ� ���Ḧ ���� NPCInteraction ������Ʈ</param>
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
    /// NPC�� ���� Ư�� ��ɿ� ���� Ư�� ��ư�� �����մϴ�.
    /// </summary>
    /// <param name="npc">���� ��ȣ�ۿ� ���� NPC ������Ʈ</param>
    public void SetSpecialButton(NPC npc)
    {
        // 1. NPC���� Ư�� ��� ����� ��û�մϴ�.
        List<INPCFunction> functions = npc.GetSpecialFunctions();

        // 2. Ư�� ����� �ϳ��� �����ϴ��� Ȯ���մϴ�.
        if (functions != null && functions.Count > 0)
        {
            // 3. ����� �ִٸ� ��ư�� Ȱ��ȭ�ϰ�, ù ��° ����� �̸����� �ؽ�Ʈ�� �����մϴ�.
            // ����� ���� ��� �� ù ��° ��ɸ� ǥ���մϴ�.
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = functions[0].FunctionButtonName;
        }
        else
        {
            // 4. ����� ���ٸ� ��ư�� ��Ȱ��ȭ�մϴ�.
            specialButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ���� �г��� �ؽ�Ʈ�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="data">����Ʈ ������(���� ���� ����)</param>
    private void UpdateRewardTexts(QuestData data)
    {
        // ���� ������ ����
        if (rewardItemNameText != null)
        {
            if (data.rewardItems.Count > 0)
            {
                string itemString = "";
                for (int i = 0; i < data.rewardItems.Count; i++)
                {
                    // ItemSO�� null���� Ȯ���Ͽ� ������ �����մϴ�.
                    if (data.rewardItems[i].itemSO != null)
                    {
                        string itemName = data.rewardItems[i].itemSO.itemName;
                        itemString += data.rewardItems[i].itemCount > 0 ? $"{itemName} ({data.rewardItems[i].itemCount}��)" : itemName;
                    }
                    else
                    {
                        itemString += "��ȿ���� ���� ������";
                    }

                    if (i < data.rewardItems.Count - 1)
                    {
                        itemString += ", ";
                    }
                }
                rewardItemNameText.text = $"���� ������: {itemString}";
            }
            else
            {
                rewardItemNameText.text = "���� ������: ����";
            }
        }

        // ����ġ ���� ������Ʈ
        if (rewardExpText != null)
        {
            rewardExpText.text = data.experienceReward > 0 ? $"���� ����ġ: +{data.experienceReward}" : "���� ����ġ: ����";
        }

        // ��� ���� ������Ʈ
        if (rewardGoldText != null)
        {
            rewardGoldText.text = data.goldReward > 0 ? $"���� ���: +{data.goldReward}" : "���� ���: ����";
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // ��ư �̺�Ʈ ������ �߰�/���� (���� ����)
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