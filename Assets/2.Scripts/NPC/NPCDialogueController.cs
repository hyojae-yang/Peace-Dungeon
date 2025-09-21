using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC 대화 UI의 로직을 관리하는 싱글턴 클래스입니다.
/// 대사 출력, 다음 대사로 넘기기 등을 처리합니다.
/// SOLID: 단일 책임 원칙 (UI 제어 및 대화 로직).
/// </summary>
public class NPCDialogueController : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static NPCDialogueController Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("대화 패널")]
    [SerializeField] private GameObject dialoguePanel;
    [Tooltip("NPC 이름 텍스트")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [Tooltip("대화 내용 텍스트")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [Tooltip("다음 대화로 넘어가는 버튼")]
    [SerializeField] private Button nextButton;

    // 현재 대화 진행 상태
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
    /// 대화 시작을 요청하는 메서드입니다.
    /// </summary>
    /// <param name="npcName">대화하는 NPC의 이름</param>
    /// <param name="dialogues">표시할 대사 배열</param>
    /// <param name="onDialogueEnd">대화가 끝난 후 실행할 액션</param>
    public void StartDialogue(string npcName, string[] dialogues, Action onDialogueEnd = null)
    {
        currentDialogues = dialogues;
        dialogueIndex = 0;
        onDialogueEndAction = onDialogueEnd;

        // 대사 배열이 비어있으면 바로 종료
        if (currentDialogues == null || currentDialogues.Length == 0)
        {
            onDialogueEndAction?.Invoke();
            HideDialogueUI();
            return;
        }

        // 대화 UI 활성화 및 버튼 리스너 추가
        ShowDialogueUI();
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextDialogue);

        // 첫 대사 표시
        UpdateDialogueUI(npcName, currentDialogues[dialogueIndex]);
    }

    /// <summary>
    /// '다음' 버튼 클릭 시 다음 대사로 넘어가는 메서드입니다.
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
            // 모든 대화가 끝나면
            onDialogueEndAction?.Invoke();
            HideDialogueUI();
        }
    }

    /// <summary>
    /// 대화 UI를 활성화합니다.
    /// </summary>
    private void ShowDialogueUI()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    /// <summary>
    /// 대화 UI를 비활성화합니다.
    /// </summary>
    public void HideDialogueUI()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    /// <summary>
    /// UI의 텍스트를 업데이트합니다.
    /// </summary>
    /// <param name="npcName">표시할 NPC 이름</param>
    /// <param name="dialogueTextContent">표시할 대사 내용</param>
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

        // 대사 배열의 길이가 1일 경우, '다음' 버튼을 비활성화하여 대화 종료를 유도합니다.
        // 다음 버튼을 누르면 OnNextDialogue 메서드가 호출되어 대화가 종료됩니다.
        if (nextButton != null)
        {
            bool isLastDialogue = (currentDialogues.Length > 1 && dialogueIndex == currentDialogues.Length - 1);
            nextButton.gameObject.SetActive(currentDialogues.Length > 1);
        }
    }
}