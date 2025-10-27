// TutorialUIHandler.cs

using UnityEngine;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 추가합니다.

/// <summary>
/// ITutorialView 인터페이스를 구현하여 튜토리얼의 시각적 안내를 담당합니다.
/// - UI 요소(패널, 텍스트, 하이라이트 등)를 직접 제어하는 책임을 가집니다. (SRP 준수)
/// </summary>
public class TutorialUIHandler : MonoBehaviour, ITutorialView
{
    // [Dependencies]

    [Header("UI Components")]
    [Tooltip("튜토리얼 팝업 메시지를 담을 메인 패널입니다.")]
    [SerializeField] private GameObject mainPopupPanel;

    [Tooltip("팝업 패널에 들어갈 텍스트 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI popupText;

    [Header("Settings")]
    [Tooltip("튜토리얼 완료 메시지가 화면에 유지될 시간 (초)")]
    [SerializeField] private float completionMessageDuration = 5f; // 5초로 기본 설정

    // [Data]

    [System.Serializable]
    public class TutorialStepData
    {
        public TutorialStep step;
        [TextArea(3, 5)]
        public string message;
    }

    [Header("Tutorial Messages")]
    [SerializeField] private TutorialStepData[] stepMessages;

    // [Public Methods - ITutorialView Implementation]

    /// <summary>
    /// TutorialManager로부터 단계 진행 알림을 받아 시각적 안내를 활성화합니다.
    /// </summary>
    /// <param name="step">현재 튜토리얼 단계</param>
    public void ShowInstruction(TutorialStep step)
    {
        HideAllInstruction();

        string message = GetMessageForStep(step);

        if (string.IsNullOrEmpty(message))
        {
            if (step != TutorialStep.Init)
            {
                Debug.LogWarning($"[TutorialUIHandler] 단계 {step}에 해당하는 메시지가 정의되지 않았습니다.");
            }
            return;
        }

        popupText.text = message;
        mainPopupPanel.SetActive(true);
        // TODO: GuidePlace 단계에서는 실제 UI 하이라이트 애니메이션을 시작하는 로직이 들어갑니다.
    }

    /// <summary>
    /// 모든 튜토리얼 UI 요소를 비활성화합니다.
    /// </summary>
    public void HideAllInstruction()
    {
        mainPopupPanel.SetActive(false);
        // TODO: 모든 하이라이트 및 화살표를 비활성화하는 로직이 들어갑니다.
    }

    /// <summary>
    /// 튜토리얼 완료 메시지를 보여주고, 설정된 시간 후에 자동으로 숨깁니다. (수정됨)
    /// </summary>
    public void ShowCompletionMessage()
    {
        string completionMessage = "축하합니다! 던전 배치에 성공하셨습니다. \n이제 문으로 가서 첫 던전을 탐험하세요!";
        popupText.text = completionMessage;
        mainPopupPanel.SetActive(true);


        // 코루틴을 시작하여 지연 후 숨김 및 시스템 종료를 요청합니다.
        StartCoroutine(HideCompletionMessageAfterDelay());
    }

    // [Private Methods]

    /// <summary>
    /// 설정된 데이터에서 현재 단계에 맞는 메시지를 찾습니다.
    /// </summary>
    private string GetMessageForStep(TutorialStep step)
    {
        if (step == TutorialStep.Init) return string.Empty;

        foreach (var data in stepMessages)
        {
            if (data.step == step)
            {
                return data.message;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// 완료 메시지를 일정 시간 보여준 후 숨기고, Manager에게 시스템 종료를 알립니다. (추가된 메서드)
    /// </summary>
    private IEnumerator HideCompletionMessageAfterDelay()
    {
        // 설정된 시간(예: 5초) 동안 기다립니다.
        yield return new WaitForSeconds(completionMessageDuration);

        // 메시지 패널을 숨깁니다.
        HideAllInstruction();


        // 메시지가 완전히 사라진 후, Manager에게 최종적으로 시스템을 종료하라고 알립니다.
        TutorialManager manager = FindFirstObjectByType<TutorialManager>();
        if (manager != null)
        {
            // TutorialManager의 새로운 최종 종료 메서드를 호출합니다.
            manager.FinalizeSystemShutdown();
        }
    }
}