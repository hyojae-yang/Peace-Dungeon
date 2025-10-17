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
    [Tooltip("NPC 초상화 이미지 컴포넌트")] // 이름 변경: npcSprite에서 npcImageComponent로 명확하게!
    [SerializeField] private Image npcImageComponent;
    [Tooltip("대화 내용 텍스트")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [Tooltip("다음 대화로 넘어가는 버튼")]
    [SerializeField] private Button nextButton;

    // 현재 대화 진행 상태
    private string[] currentDialogues;
    private int dialogueIndex = 0;
    private Action onDialogueEndAction;

    // 현재 NPC의 초상화 Sprite를 저장하는 필드 추가 (다음 대사로 넘어갈 때 필요)
    private Sprite currentNpcSprite;

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
    /// 이름 텍스트와 같이 초상화 **데이터**인 Sprite를 파라미터로 받습니다.
    /// SOLID: StartDialogue는 초기 설정 및 검증 책임만 가집니다.
    /// </summary>
    /// <param name="npcName">대화하는 NPC의 이름</param>
    /// <param name="npcSprite">표시할 NPC의 초상화 Sprite **데이터**</param> // 💡 Image가 아닌 Sprite 타입으로 수정
    /// <param name="dialogues">표시할 대사 배열</param>
    /// <param name="onDialogueEnd">대화가 끝난 후 실행할 액션</param>
    public void StartDialogue(string npcName, Sprite npcSprite, string[] dialogues, Action onDialogueEnd = null) // 💡 파라미터 타입 변경: Image -> Sprite
    {
        currentDialogues = dialogues;
        dialogueIndex = 0;
        onDialogueEndAction = onDialogueEnd;
        currentNpcSprite = npcSprite; // 현재 Sprite를 필드에 저장

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
        // NPC 이름과 Sprite를 함께 전달하여 UI를 업데이트합니다.
        UpdateDialogueUI(npcName, currentNpcSprite, currentDialogues[dialogueIndex]);
    }

    /// <summary>
    /// '다음' 버튼 클릭 시 다음 대사로 넘어가는 메서드입니다.
    /// SOLID: 다음 대사로 인덱스를 증가시키고 UI 업데이트를 요청하는 책임만 가집니다.
    /// </summary>
    private void OnNextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < currentDialogues.Length)
        {
            // NPC 이름과 Sprite는 대화가 진행되어도 바뀌지 않으므로, 
            // 저장된 값(npcNameText.text와 currentNpcSprite)을 사용하여 UI를 업데이트합니다.
            UpdateDialogueUI(npcNameText.text, currentNpcSprite, currentDialogues[dialogueIndex]);
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
    /// SOLID: 단일 책임 원칙 (UI 활성화/비활성화)
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
    /// SOLID: 단일 책임 원칙 (UI 활성화/비활성화)
    /// </summary>
    public void HideDialogueUI()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    /// <summary>
    /// UI의 텍스트와 이미지를 업데이트합니다.
    /// SOLID: 단일 책임 원칙 (UI 요소 업데이트)
    /// </summary>
    /// <param name="npcName">표시할 NPC 이름</param>
    /// <param name="npcSprite">표시할 NPC 초상화 Sprite **데이터**</param> // Sprite 타입으로 수정
    /// <param name="dialogueTextContent">표시할 대사 내용</param>
    private void UpdateDialogueUI(string npcName, Sprite npcSprite, string dialogueTextContent) // 파라미터 타입 변경: Image -> Sprite
    {
        // 1. NPC 이름 텍스트 할당 (string 데이터)
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }

        // 2. NPC 초상화 이미지 할당 (Sprite 데이터)
        if (npcImageComponent != null)
        {
            // 핵심 수정: Image 컴포넌트 자체를 대입하는 대신, Image 컴포넌트의 'sprite' 속성에 Sprite 데이터를 할당합니다.
            npcImageComponent.sprite = npcSprite;

            // Sprite가 할당되면 이미지를 활성화합니다.
            // (null 체크는 Image 컴포넌트 자체를 비활성화/활성화하는 용도로 사용)
            // npcImageComponent.gameObject.SetActive(npcSprite != null); // 필요에 따라 추가
        }

        // 3. 대화 내용 텍스트 할당 (string 데이터)
        if (dialogueText != null)
        {
            dialogueText.text = dialogueTextContent;
        }

        // '다음' 버튼 활성화/비활성화 로직 유지
        if (nextButton != null)
        {
            // 대사 배열의 길이가 1보다 클 때만 버튼을 활성화합니다.
            nextButton.gameObject.SetActive(currentDialogues != null && currentDialogues.Length > 1);
        }
    }
}