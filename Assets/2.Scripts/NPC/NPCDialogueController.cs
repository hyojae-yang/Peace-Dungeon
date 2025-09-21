using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC ��ȭ UI�� ������ �����ϴ� �̱��� Ŭ�����Դϴ�.
/// ��� ���, ���� ���� �ѱ�� ���� ó���մϴ�.
/// SOLID: ���� å�� ��Ģ (UI ���� �� ��ȭ ����).
/// </summary>
public class NPCDialogueController : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static NPCDialogueController Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("��ȭ �г�")]
    [SerializeField] private GameObject dialoguePanel;
    [Tooltip("NPC �̸� �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [Tooltip("��ȭ ���� �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [Tooltip("���� ��ȭ�� �Ѿ�� ��ư")]
    [SerializeField] private Button nextButton;

    // ���� ��ȭ ���� ����
    private string[] currentDialogues;
    private int dialogueIndex = 0;
    private Action onDialogueEndAction;

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

    /// <summary>
    /// ��ȭ ������ ��û�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="npcName">��ȭ�ϴ� NPC�� �̸�</param>
    /// <param name="dialogues">ǥ���� ��� �迭</param>
    /// <param name="onDialogueEnd">��ȭ�� ���� �� ������ �׼�</param>
    public void StartDialogue(string npcName, string[] dialogues, Action onDialogueEnd = null)
    {
        currentDialogues = dialogues;
        dialogueIndex = 0;
        onDialogueEndAction = onDialogueEnd;

        // ��� �迭�� ��������� �ٷ� ����
        if (currentDialogues == null || currentDialogues.Length == 0)
        {
            onDialogueEndAction?.Invoke();
            HideDialogueUI();
            return;
        }

        // ��ȭ UI Ȱ��ȭ �� ��ư ������ �߰�
        ShowDialogueUI();
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextDialogue);

        // ù ��� ǥ��
        UpdateDialogueUI(npcName, currentDialogues[dialogueIndex]);
    }

    /// <summary>
    /// '����' ��ư Ŭ�� �� ���� ���� �Ѿ�� �޼����Դϴ�.
    /// </summary>
    private void OnNextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < currentDialogues.Length)
        {
            UpdateDialogueUI(npcNameText.text, currentDialogues[dialogueIndex]);
        }
        else
        {
            // ��� ��ȭ�� ������
            onDialogueEndAction?.Invoke();
            HideDialogueUI();
        }
    }

    /// <summary>
    /// ��ȭ UI�� Ȱ��ȭ�մϴ�.
    /// </summary>
    private void ShowDialogueUI()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    /// <summary>
    /// ��ȭ UI�� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void HideDialogueUI()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    /// <summary>
    /// UI�� �ؽ�Ʈ�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="npcName">ǥ���� NPC �̸�</param>
    /// <param name="dialogueTextContent">ǥ���� ��� ����</param>
    private void UpdateDialogueUI(string npcName, string dialogueTextContent)
    {
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        if (dialogueText != null)
        {
            dialogueText.text = dialogueTextContent;
        }

        // ��� �迭�� ���̰� 1�� ���, '����' ��ư�� ��Ȱ��ȭ�Ͽ� ��ȭ ���Ḧ �����մϴ�.
        // ���� ��ư�� ������ OnNextDialogue �޼��尡 ȣ��Ǿ� ��ȭ�� ����˴ϴ�.
        if (nextButton != null)
        {
            bool isLastDialogue = (currentDialogues.Length > 1 && dialogueIndex == currentDialogues.Length - 1);
            nextButton.gameObject.SetActive(currentDialogues.Length > 1);
        }
    }
}