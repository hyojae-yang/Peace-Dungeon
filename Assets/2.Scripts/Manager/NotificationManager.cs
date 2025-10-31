using UnityEngine;
using TMPro;
using System.Collections;

/*
                    if (NotificationManager.Instance != null)
                    {
                        NotificationManager.Instance.ShowNotification(
                            "던전 입장 완료",
                            NotificationType.General
                        );
                    }
*/
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
    private GameObject currentInteractionCaller = null;
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
        if (notificationPanel != null)
        { notificationPanel.SetActive(true); }

        // 4. 일정 시간 후 숨기는 코루틴 시작
        hideCoroutine = StartCoroutine(HideAfterDelay(displayDuration));
    }

    /// <summary>
    /// NPC 상호작용 프롬프트처럼 자동으로 사라지지 않고 수동으로 숨겨야 하는 알림을 표시합니다.
    /// (Interaction 타입 전용)
    /// </summary>
    /// <param name="message">표시할 상호작용 프롬프트 내용</param>
    /// <param name="caller">프롬프트를 표시하는 것을 요청한 GameObject입니다.</param>
    public void ShowInteractionPrompt(string message, GameObject caller) // ⭐️ caller 인수가 추가됨
    {
        // 일반 알림이 표시 중일 경우 방해하지 않도록 코루틴만 중지 (기존과 동일)
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        // 현재 요청자(caller)를 무조건 새로운 요청자로 업데이트하여 권한을 부여합니다.
        // Update() 기반 환경에서는 가장 최근에 호출한 객체의 프롬프트가 표시됩니다.
        currentInteractionCaller = caller;

        // Interaction 스타일 적용
        SetNotificationStyle(NotificationType.Interaction);

        // 텍스트 및 패널 업데이트
        notificationText.text = message;
        notificationPanel.SetActive(true);
    }

    /// <summary>
    /// ShowInteractionPrompt로 표시된 알림을 수동으로 숨깁니다.
    /// </summary>
    /// <param name="caller">프롬프트 숨김을 요청한 GameObject입니다.</param>
    public void HideInteractionPrompt(GameObject caller) // caller 인수가 추가됨
    {
        // 숨김을 요청한 객체가 현재 알림을 표시하고 있는 객체와 일치할 때만 숨깁니다.
        if (currentInteractionCaller == caller)
        {
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
            // 숨김 처리가 완료되면 권한도 해제합니다.
            currentInteractionCaller = null;
        }
        // 요청 객체가 다르면 (다른 NPC가 프롬프트를 띄우고 있는 중이라면) 아무것도 하지 않습니다.
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
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Success, 0.5f);
                }
                break;
            case NotificationType.Warning:
                targetColor = warningColor;
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Warning, 0.5f);
                }
                break;
            case NotificationType.Interaction:
                targetColor = interactionColor;
                break;
            case NotificationType.General:
            default:
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.General, 0.5f);
                }
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