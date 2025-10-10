using UnityEngine;
using TMPro;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가
using System.Collections;

/// <summary>
/// 알림 메시지의 유형을 정의합니다. 텍스트 색상 또는 기타 스타일을 구분하는 데 사용됩니다.
/// SOLID: OCP(개방-폐쇄 원칙) - 새로운 알림 유형이 추가되어도 ShowNotification 메서드 자체는 닫혀있습니다.
/// </summary>
public enum NotificationType
{
    General,        // 일반 시스템 메시지 (예: 저장 완료, 장비 교체)
    Success,        // 긍정적 메시지 (예: 레벨업, 보스 처치, 퀘스트 완료)
    Warning,        // 경고 메시지 (예: 인벤토리 가득 참)
    Interaction     // 상호작용 프롬프트 (자동 숨김 없음)
}

/// <summary>
/// 게임 전반의 모든 일회성 알림을 관리하는 싱글턴 클래스입니다.
/// (NPC 상호작용 프롬프트도 이 시스템을 통해 표시됩니다.)
/// SOLID: 단일 책임 원칙 (SRP) - 오직 알림창 표시 및 숨김 책임만 가집니다.
/// </summary>
public class NotificationManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static NotificationManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("알림창 전체를 감싸는 최상위 UI 패널 (배경 이미지 포함)")]
    public GameObject notificationPanel;
    [Tooltip("알림 내용이 표시될 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI notificationText;

    [Header("Behavior Settings")]
    [Tooltip("General, Success, Warning 타입 알림의 기본 표시 시간 (초)")]
    public float displayDuration = 3f;

    [Header("Notification Styles")]
    [Tooltip("General 타입 알림의 텍스트 색상")]
    public Color generalColor = Color.white;
    [Tooltip("Success 타입 알림의 텍스트 색상 (예: 녹색)")]
    public Color successColor = Color.green;
    [Tooltip("Warning 타입 알림의 텍스트 색상 (예: 빨간색)")]
    public Color warningColor = Color.red;
    [Tooltip("Interaction 타입 알림의 텍스트 색상 (예: 노란색)")]
    public Color interactionColor = Color.yellow;

    // 현재 실행 중인 자동 숨김 코루틴을 추적하는 변수
    // 새로운 알림이 들어오면 기존 코루틴을 중단하고 새 코루틴을 시작합니다.
    private Coroutine hideCoroutine;

    /// <summary>
    /// 싱글턴 인스턴스를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 게임 시작 시 알림창은 숨겨진 상태로 시작합니다.
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------------------------------------------------------------------
    // 공개 API: 알림 표시/숨기기
    // ----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 일정 시간 후 자동으로 사라지는 알림을 표시합니다.
    /// (General, Success, Warning 타입에 사용)
    /// </summary>
    /// <param name="message">표시할 알림 내용</param>
    /// <param name="type">알림 유형 (기본값: General)</param>
    public void ShowNotification(string message, NotificationType type = NotificationType.General)
    {
        // 1. 기존 자동 숨김 코루틴 중지
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 2. 스타일 적용
        SetNotificationStyle(type);

        // 3. 텍스트 및 패널 업데이트
        notificationText.text = message;
        notificationPanel.SetActive(true);

        // 4. 일정 시간 후 숨기는 코루틴 시작
        hideCoroutine = StartCoroutine(HideAfterDelay(displayDuration));
    }

    /// <summary>
    /// NPC 상호작용 프롬프트처럼 자동으로 사라지지 않고 수동으로 숨겨야 하는 알림을 표시합니다.
    /// (Interaction 타입 전용)
    /// </summary>
    /// <param name="message">표시할 상호작용 프롬프트 내용</param>
    public void ShowInteractionPrompt(string message)
    {
        // 일반 알림이 표시 중일 경우 방해하지 않도록 코루틴만 중지 (패널은 놔둡니다)
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        // Interaction 스타일 적용
        SetNotificationStyle(NotificationType.Interaction);

        // 텍스트 및 패널 업데이트
        notificationText.text = message;
        notificationPanel.SetActive(true);
    }

    /// <summary>
    /// ShowInteractionPrompt로 표시된 알림을 수동으로 숨깁니다.
    /// (NPCUIManager에서 상호작용 종료 시 호출됨)
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    // ----------------------------------------------------------------------------------------------------------------
    // 내부 로직
    // ----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 알림 유형에 따라 텍스트의 색상을 변경합니다.
    /// </summary>
    /// <param name="type">적용할 알림 유형</param>
    private void SetNotificationStyle(NotificationType type)
    {
        Color targetColor;
        switch (type)
        {
            case NotificationType.Success:
                targetColor = successColor;
                break;
            case NotificationType.Warning:
                targetColor = warningColor;
                break;
            case NotificationType.Interaction:
                targetColor = interactionColor;
                break;
            case NotificationType.General:
            default:
                targetColor = generalColor;
                break;
        }

        // 텍스트 색상 적용
        if (notificationText != null)
        {
            notificationText.color = targetColor;
        }

        // TODO: (나중에) 필요하다면 여기서 배경 이미지의 색상이나 테두리 이미지를 변경하는 로직을 추가할 수 있습니다.
    }

    /// <summary>
    /// 지정된 지연 시간 후 알림 패널을 비활성화하는 코루틴입니다.
    /// </summary>
    /// <param name="delay">대기할 시간 (초)</param>
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        hideCoroutine = null; // 코루틴이 완료되었음을 표시
    }
}