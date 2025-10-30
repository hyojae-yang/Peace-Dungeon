using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 추가

/// <summary>
/// 특정 안내 UI를 표시하고, 플레이어의 특정 행동 완료를 기다리는 범용 컴포넌트입니다. (SRP 준수)
/// - 이 스크립트는 여러 튜토리얼 단계에서 재사용되며, 패널의 활성화/비활성화 및 텍스트 제어를 담당합니다.
/// - [추가] 튜토리얼 완료 메시지 표시 및 임시 알림 기능도 담당합니다.
/// </summary>
public class TutorialInstructionPanel : MonoBehaviour
{
    // [Dependencies]
    [Header("Dependencies")]
    [Tooltip("안내 텍스트가 표시될 TextMeshPro 컴포넌트입니다. (패널의 자식)")]
    [SerializeField] private TextMeshProUGUI instructionText;

    // [New Field] 완료 메시지 유지 시간
    [Header("Completion Settings")]
    [Tooltip("튜토리얼 완료 메시지가 화면에 유지될 시간 (초)")]
    [SerializeField] private float completionMessageDuration = 5f; // 5초로 기본 설정

    // [Private Fields]
    // 임시 메시지 표시 전, 원래의 튜토리얼 메시지를 저장할 필드
    private string originalInstructionMessage = "";

    // [Events]
    [Header("Events")]
    [Tooltip("안내 패널의 임무가 완료되었을 때 호출됩니다. TutorialManager.AdvanceStep()을 연결해야 합니다.")]
    public UnityEvent OnInstructionComplete = new UnityEvent();

    // [Public Methods]

    /// <summary>
    /// TutorialManager로부터 호출되어 패널을 활성화하고 메시지를 설정합니다.
    /// </summary>
    /// <param name="message">화면에 표시할 안내 텍스트</param>
    public void ShowInstruction(string message)
    {
        // 진행 중이던 코루틴이 있다면 중지 (안전 장치)
        StopAllCoroutines();

        // [핵심 수정] 새로운 메시지를 원래 메시지로 저장합니다.
        // 이 메시지는 임시 경고 메시지 표시 후 복구될 때 사용됩니다.
        originalInstructionMessage = message;

        // 1. 메시지 설정 (텍스트 컴포넌트 유효성 검사)
        if (instructionText != null)
        {
            instructionText.text = message;
        }

        // 2. 게임 오브젝트(패널) 활성화
        gameObject.SetActive(true);
    }

    // ------------------- [신규 기능: 임시 알림 메시지 처리] -------------------

    /// <summary>
    /// [CS1061 오류 해결] 튜토리얼 중간에 특정 경고(예: 유효하지 않은 배치)를 임시로 표시합니다.
    /// 경고 표시 후, 일정 시간이 지나면 이전 메시지(originalInstructionMessage)로 복구합니다.
    /// (TutorialManager에서 ShowInvalidPlacementNotification() 호출을 통해 간접 호출됨)
    /// </summary>
    /// <param name="message">임시로 표시할 경고 텍스트</param>
    /// <param name="duration">경고 메시지가 화면에 유지될 시간 (초)</param>
    public void ShowTemporaryInstruction(string message, float duration)
    {
        // 이미 진행 중인 모든 코루틴을 중지합니다.
        StopAllCoroutines();

        // 1. 메시지 설정 (임시 경고 메시지)
        if (instructionText != null)
        {
            instructionText.text = message;
        }

        // 2. 패널이 비활성화되어 있었다면 활성화 (TutorialManager에서 이미 활성화되었어야 함)
        gameObject.SetActive(true);

        // 3. 코루틴을 시작하여 지연 후 원래 메시지로 복구합니다.
        StartCoroutine(RestoreInstructionAfterDelay(duration));
    }

    /// <summary>
    /// 임시 알림 메시지를 일정 시간 보여준 후, ShowInstruction()에서 저장한 원래 메시지로 복구합니다.
    /// </summary>
    /// <param name="delay">대기 시간 (초)</param>
    private IEnumerator RestoreInstructionAfterDelay(float delay)
    {
        // 설정된 시간(delay) 동안 기다립니다.
        yield return new WaitForSeconds(delay);

        // 1. 원래의 텍스트로 복구
        if (instructionText != null)
        {
            // Debug.Log("[TutorialInstructionPanel] 임시 메시지 만료. 원래 메시지로 복구.");
            instructionText.text = originalInstructionMessage;
        }

        // Note: 패널 자체는 비활성화하지 않고, 계속 활성화된 상태를 유지합니다.
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// 튜토리얼의 마지막 단계(Complete)에서 호출되어 완료 메시지를 표시하고,
    /// 일정 시간 후 자동으로 숨김 처리 후 시스템 최종 종료를 요청합니다.
    /// </summary>
    public void ShowCompletionMessage()
    {
        // 진행 중이던 코루틴이 있다면 중지 (안전 장치)
        StopAllCoroutines();

        // 1. 메시지 설정
        string completionMessage = "축하합니다! 던전 배치에 성공하셨습니다. \n이제 문으로 가서 첫 던전을 탐험하세요!";

        if (instructionText != null)
        {
            instructionText.text = completionMessage;
        }

        // 2. 게임 오브젝트(패널) 활성화
        gameObject.SetActive(true);

        // 3. 코루틴을 시작하여 지연 후 숨김 및 시스템 종료를 요청합니다.
        StartCoroutine(HideCompletionMessageAfterDelay());
    }

    /// <summary>
    /// 완료 메시지를 일정 시간 보여준 후 숨기고, Manager에게 시스템 종료를 알립니다.
    /// </summary>
    private IEnumerator HideCompletionMessageAfterDelay()
    {
        // 설정된 시간(예: 5초) 동안 기다립니다.
        yield return new WaitForSeconds(completionMessageDuration);

        // 메시지 패널을 숨깁니다.
        gameObject.SetActive(false);

        // 4. Manager에게 최종적으로 시스템을 종료하라고 알립니다.
        // [수정: 싱글톤 접근 안전성 강화]
        // 튜토리얼 최종 종료는 이 코루틴에서만 호출되어야 합니다.
        if (TutorialManager.Instance != null)
        {
            Debug.Log("[TutorialInstructionPanel] 완료 메시지 숨김. 최종 시스템 종료 요청.");
            TutorialManager.Instance.FinalizeSystemShutdown();
        }
        else
        {
            Debug.LogError("[TutorialInstructionPanel] TutorialManager 싱글톤을 찾을 수 없습니다! 최종 종료 실패.");
        }
    }


    /// <summary>
    /// 외부 트리거(예: InventoryOpenTrigger.cs)에 의해 호출되어 튜토리얼 단계를 진행시킵니다.
    /// 이 메서드가 호출되면 패널을 비활성화하고 완료 이벤트를 발생시킵니다.
    /// </summary>
    public void CompleteInstruction()
    {
        // 안전 장치: 활성화 상태일 때만 처리
        if (!gameObject.activeSelf)
        {
            return;
        }

        // [수정] 진행 중이던 코루틴이 있다면 중지 (예: 임시 메시지 코루틴이 완료 메시지 로직을 방해하는 것을 방지)
        StopAllCoroutines();

        // 1. 패널 비활성화
        gameObject.SetActive(false);

        // 2. 이벤트 발생 (TutorialManager에게 다음 단계로 넘어가라고 알림)
        OnInstructionComplete.Invoke();
    }
}
