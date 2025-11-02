using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

/// <summary>
/// [신규] 튜토리얼 스토리 텍스트 연출을 담당하는 독립적인 컴포넌트입니다. (SRP 준수)
/// - 텍스트 타이핑, 사용자 입력에 따른 텍스트 즉시 표시/다음 메시지 전환 책임을 가집니다.
/// - 모든 메시지 출력 완료 시 UnityEvent를 통해 TutorialManager에게 다음 단계 진행을 알립니다.
/// </summary>
public class StoryPanelController : MonoBehaviour
{
    // [Serialized Fields]

    [Header("Dependencies")]
    [Tooltip("스토리 텍스트가 출력될 TextMeshPro 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI storyText;
    [Tooltip("클릭/터치 입력을 받을 UI Button 또는 Collider가 부착된 오브젝트입니다.")]
    [SerializeField] private GameObject inputBlocker;

    [Header("Settings")]
    [Tooltip("순서대로 보여줄 스토리 메시지 배열입니다.")]
    [TextArea(3, 6)]
    [SerializeField] private string[] storyMessages;

    [Tooltip("한 글자당 타이핑되는 속도 (초)")]
    [SerializeField] private float typingSpeed = 0.1f;

    [Header("Events")]
    [Tooltip("모든 스토리 메시지 연출이 완료되었을 때 호출됩니다. TutorialManager.AdvanceStep()을 연결해야 합니다.")]
    public UnityEvent OnStoryComplete = new UnityEvent();

    // [Private Fields]

    // 현재 출력 중인 메시지의 인덱스입니다.
    private int currentMessageIndex = 0;
    // 현재 타이핑 코루틴을 저장하여, 입력 시 강제 종료하는 데 사용합니다.
    private Coroutine typingCoroutine;
    // 현재 텍스트 타이핑 연출이 완료되었는지 여부를 나타냅니다. (입력 구분 기준)
    private bool isTypingComplete = false;

    // [Unity Lifecycle Methods]

    private void Start()
    {
        PlayerCharacter.Instance.playerController.canMove = false;
        // 1. 컴포넌트가 활성화될 때 (TutorialManager.ProcessStep 호출 시) 연출을 시작합니다.
        StartStorySequence();
    }

    // [Public Methods]

    /// <summary>
    /// 플레이어의 클릭/터치 입력을 처리하는 공통 메서드입니다.
    /// 이 메서드를 버튼이나 입력 시스템에 연결해야 합니다.
    /// </summary>
    public void HandlePlayerInput()
    {
        // 1. 타이핑이 진행 중이라면, 즉시 모든 텍스트를 표시합니다.
        if (!isTypingComplete)
        {
            // 타이핑 코루틴이 실행 중이라면 강제 종료합니다.
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            // 텍스트를 바로 완료합니다.
            storyText.text = storyMessages[currentMessageIndex];
            isTypingComplete = true; // 타이핑 완료 상태로 전환합니다.

            // TODO: (사운드 추천) 이 부분에 타이핑 스킵 사운드 재생 로직을 넣으면 좋습니다.
        }
        // 2. 타이핑이 완료된 상태라면, 다음 메시지로 넘어갑니다.
        else
        {
            AdvanceMessage();
        }
    }

    // [Private Methods]

    /// <summary>
    /// 스토리 시퀀스를 시작하고 첫 번째 메시지를 출력합니다.
    /// </summary>
    private void StartStorySequence()
    {
        if (storyMessages == null || storyMessages.Length == 0)
        {
            Debug.LogError("[StoryPanelController] 보여줄 메시지가 없습니다! 튜토리얼 다음 단계로 즉시 진행합니다.");
            FinishSequence();
            return;
        }

        // 초기 인덱스 설정 및 첫 메시지 출력 시작
        currentMessageIndex = 0;
        DisplayCurrentMessage();
    }

    /// <summary>
    /// 다음 메시지로 이동하거나, 메시지가 없다면 시퀀스를 종료합니다.
    /// </summary>
    private void AdvanceMessage()
    {
        currentMessageIndex++;

        // 모든 메시지를 다 보여줬다면 종료 처리합니다.
        if (currentMessageIndex >= storyMessages.Length)
        {
            FinishSequence();
        }
        // 다음 메시지 출력
        else
        {
            DisplayCurrentMessage();
        }
    }

    /// <summary>
    /// 현재 인덱스의 메시지에 대해 타이핑 연출을 시작합니다.
    /// </summary>
    private void DisplayCurrentMessage()
    {
        // 다음 메시지로 넘어갔으므로 타이핑 완료 플래그를 false로 초기화합니다.
        isTypingComplete = false;

        // 이전 코루틴이 혹시 남아있다면 정리 (안전 장치)
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 새로운 타이핑 연출 시작
        typingCoroutine = StartCoroutine(TypingCoroutine(storyMessages[currentMessageIndex]));
    }

    /// <summary>
    /// 메시지를 한 글자씩 출력하는 코루틴입니다.
    /// </summary>
    /// <param name="message">타이핑하여 보여줄 메시지 내용</param>
    private IEnumerator TypingCoroutine(string message)
    {
        storyText.text = string.Empty;

        foreach (char letter in message.ToCharArray())
        {
            storyText.text += letter;

            // TODO: (사운드 추천) 이 부분에 타이핑 사운드 재생 로직을 넣으면 좋습니다.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.text_sound, 0.5f);
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        // 코루틴이 정상적으로 완료되었으므로 플래그를 설정합니다.
        isTypingComplete = true;
        typingCoroutine = null; // 코루틴 참조 해제
    }

    /// <summary>
    /// 모든 스토리 연출이 완료된 후 호출됩니다.
    /// </summary>
    private void FinishSequence()
    {
        PlayerCharacter.Instance.playerController.canMove = true;
        // 1. 모든 UI를 숨깁니다.
        gameObject.SetActive(false);

        // 2. 완료 이벤트를 발생시켜 TutorialManager에게 알립니다.
        OnStoryComplete.Invoke();
    }
}