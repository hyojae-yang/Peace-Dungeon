using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 필요합니다.
using UnityEngine.Events;
using System.Collections;
using TMPro; // TextMeshProUGUI 사용을 위해 추가합니다.

/// <summary>
/// [신규] 튜토리얼의 도입부(IntroFade) 연출을 담당하는 독립적인 UI 컴포넌트입니다.
/// - 검은색 패널의 페이드 아웃 및 텍스트 타이핑 연출 책임을 가집니다. (SRP 준수)
/// - 연출 완료 시 UnityEvent를 통해 TutorialManager에게 다음 단계 진행을 알립니다. (느슨한 결합)
/// </summary>
public class IntroFadePanel : MonoBehaviour
{
    // [Serialized Fields]

    [Header("Dependencies")]
    [Tooltip("페이드 아웃 연출을 할 Image 컴포넌트입니다. (배경색은 검은색)")]
    [SerializeField] private Image blackPanelImage;

    // [추가됨]
    [Tooltip("타이핑 연출을 할 TextMeshPro 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI typingText;
    // [/추가됨]

    [Header("Settings")]
    [Tooltip("페이드 아웃에 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 1.5f;
    [Tooltip("패널에 텍스트를 모두 표시한 후 페이드 아웃 시작까지의 대기 시간 (초)")]
    [SerializeField] private float delayBeforeFade = 3.0f; // 3~5초 논의에 따라 기본값 3.0f 설정

    // [추가됨]
    [Tooltip("타이핑하여 보여줄 메시지 내용입니다.")]
    [TextArea(5, 8)]
    [SerializeField] private string introMessage = "기억의 조각들이 흩어져 있습니다.\n새로운 던전을 만들 준비를 하세요.";

    [Tooltip("한 글자당 타이핑되는 속도 (초)")]
    [SerializeField] private float typingSpeed = 0.05f;
    // [/추가됨]

    [Header("Events")]
    [Tooltip("페이드 아웃 연출이 완료되었을 때 호출됩니다. TutorialManager.AdvanceStep()을 연결해야 합니다.")]
    public UnityEvent OnFadeComplete = new UnityEvent();

    // [Unity Lifecycle Methods]

    private void Start()
    {
        // Manager의 StartTutorial() 호출 이후, 이 패널이 활성화되어 있다고 가정합니다.
        PlayerCharacter.Instance.playerController.canMove = false;
        // 1. 초기 투명도 설정 (완전히 불투명)
        Color color = blackPanelImage.color;
        color.a = 1f;
        blackPanelImage.color = color;

        // 2. 텍스트 초기화 (빈 문자열)
        if (typingText != null)
        {
            typingText.text = string.Empty;
        }

        // 3. 연출 시작
        StartIntroSequence();
    }

    private void OnDisable()
    {
        PlayerCharacter.Instance.playerController.canMove = true;
    }

    // [Private Methods]

    /// <summary>
    /// 페이드 아웃 시퀀스를 시작합니다. (Typing Coroutine 호출)
    /// </summary>
    private void StartIntroSequence()
    {
        StartCoroutine(TypingCoroutine()); // 타이핑 코루틴 호출로 변경
    }

    /// <summary>
    /// [신규] 메시지를 한 글자씩 출력하고 대기 시간을 가집니다.
    /// </summary>
    private IEnumerator TypingCoroutine()
    {
        if (typingText == null)
        {
            Debug.LogError("[IntroFadePanel] Typing Text 컴포넌트가 할당되지 않았습니다!");
            // 텍스트 없이 바로 페이드 아웃 로직으로 넘어갑니다.
            StartCoroutine(FadeOutCoroutine(0f));
            yield break;
        }

        // 타이핑 연출
        typingText.text = string.Empty;
        foreach (char letter in introMessage.ToCharArray())
        {
            typingText.text += letter;

            // TODO: (사운드 추천) 이 부분에 타이핑 사운드 재생 로직을 넣으면 좋습니다.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.text_sound, 0.5f);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // 텍스트 표시 완료 후, 설정된 시간만큼 대기합니다.
        yield return new WaitForSeconds(delayBeforeFade);

        // 대기 후, 페이드 아웃 코루틴 시작
        StartCoroutine(FadeOutCoroutine(fadeDuration));
    }


    /// <summary>
    /// 검은색 패널을 서서히 사라지게 하고 완료 시 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="duration">페이드 아웃에 걸릴 시간</param>
    private IEnumerator FadeOutCoroutine(float duration)
    {
        float timer = 0f;
        // blackPanelImage의 초기 색상(알파 값 1f)을 기준으로 시작합니다.
        Color startColor = blackPanelImage.color;

        // 텍스트를 먼저 숨깁니다.
        if (typingText != null) typingText.gameObject.SetActive(false);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 시간에 따른 알파값 계산 (1.0 -> 0.0)
            float alpha = 1f - Mathf.Clamp01(timer / duration);

            // Image와 Text의 색상(알파)을 함께 조절해야 한다면 여기에 추가 로직이 필요합니다.
            // 현재는 Image만 조절하고 Text는 미리 비활성화했습니다.

            Color newColor = startColor;
            newColor.a = alpha;
            blackPanelImage.color = newColor;

            yield return null;
        }

        // 투명도를 확실히 0으로 설정
        Color finalColor = startColor;
        finalColor.a = 0f;
        blackPanelImage.color = finalColor;

        // 연출 완료 후, 자신을 비활성화하여 메모리를 정리
        gameObject.SetActive(false);

        // 연출 완료 이벤트 발생 -> TutorialManager.AdvanceStep() 호출
        OnFadeComplete.Invoke();
    }
}