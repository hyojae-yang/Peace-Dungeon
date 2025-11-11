using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Queue, Dictionary 사용을 위해 추가
using System.Linq; // 큐 맨 앞에 재삽입 로직을 위해 추가

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
    General,        // 일반 시스템 메시지 (예: 저장 완료, 장비 교체)
    Success,        // 긍정적 메시지 (예: 레벨업, 보스 처치, 퀘스트 완료)
    Warning,        // 경고 메시지 (예: 인벤토리 가득 참)
    Interaction     // 상호작용 프롬프트 (자동 숨김 없음)
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

    // ------------------------------------------------------------------------------------
    // [추가] 우선순위 및 대기열 관리 변수
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// 알림의 내용과 우선순위 등을 캡슐화한 내부 구조체입니다.
    /// </summary>
    private struct NotificationData
    {
        public string Message;
        public NotificationType Type;
        public float Duration;
        public int Priority; // 높을수록 중요한 알림
    }

    /// <summary>
    /// 알림 유형별 우선순위를 매핑합니다.
    /// </summary>
    private Dictionary<NotificationType, int> priorityMap;

    /// <summary>
    /// 처리해야 할 일반 알림들을 보관하는 FIFO 대기열입니다.
    /// </summary>
    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();

    /// <summary>
    /// 현재 화면에 표시 중인 알림의 데이터입니다. Interaction 프롬프트가 아닌 경우에만 사용됩니다.
    /// </summary>
    private NotificationData? currentDisplayingData = null;

    /// <summary>
    /// 알림 대기열 처리를 담당하는 코루틴입니다.
    /// </summary>
    private Coroutine processingCoroutine;

    // 기존의 hideCoroutine 변수는 이 스크립트에서는 더 이상 직접 사용되지 않습니다.
    // 대신 processingCoroutine이 지연 처리를 관리합니다.
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
            // [추가] 우선순위 맵 초기화
            InitializePriorityMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 알림 유형별 우선순위를 초기화합니다. (높을수록 중요)
    /// SOLID: OCP를 위해 Dictionary로 관리됩니다.
    /// </summary>
    private void InitializePriorityMap()
    {
        priorityMap = new Dictionary<NotificationType, int>
        {
            { NotificationType.Interaction, 99 }, // Interaction은 항상 최우선 (수동 숨김)
            { NotificationType.Success, 3 },
            { NotificationType.Warning, 2 },
            { NotificationType.General, 1 }
        };
    }

    // ----------------------------------------------------------------------------------------------------------------
    // 공개 API: 알림 표시/숨기기
    // ----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 일정 시간 후 자동으로 사라지는 알림을 표시합니다.
    /// (General, Success, Warning 타입에 사용)
    /// 이 메서드는 알림을 즉시 표시하지 않고 대기열에 추가하거나 선점 로직을 실행합니다.
    /// </summary>
    /// <param name="message">표시할 알림 내용</param>
    /// <param name="type">알림 유형 (기본값: General)</param>
    public void ShowNotification(string message, NotificationType type = NotificationType.General)
    {
        // [수정] 기존의 즉시 표시 로직 대신, 알림 데이터 생성 및 우선순위 처리 로직을 수행합니다.

        // 1. 알림 데이터 생성
        if (!priorityMap.TryGetValue(type, out int newPriority))
        {
            newPriority = 1; // 기본값
        }

        NotificationData newNotification = new NotificationData
        {
            Message = message,
            Type = type,
            Duration = displayDuration,
            Priority = newPriority
        };

        // 2. 현재 표시 중인 알림이 있는지 확인
        if (currentDisplayingData.HasValue)
        {
            NotificationData currentData = currentDisplayingData.Value;

            // 3. 우선순위 비교 (선점 로직)
            if (newNotification.Priority > currentData.Priority)
            {
                // **선점 발생**: 현재 표시 중인 알림을 중단하고 새로운 알림을 즉시 표시합니다.

                // A. 현재 실행 중인 코루틴 중단 (WaitForSeconds 취소)
                if (processingCoroutine != null)
                {
                    StopCoroutine(processingCoroutine);
                    processingCoroutine = null;
                }

                // B. 인터럽트된 알림을 큐의 맨 앞으로 다시 삽입 (순서 보장)
                ReQueueToFront(currentData);

                // C. 새로운 알림을 현재 표시 데이터로 지정 (ProcessQueue가 즉시 시작할 수 있도록)
                currentDisplayingData = newNotification;

                // D. 즉시 알림 표시를 시작합니다.
                processingCoroutine = StartCoroutine(ProcessQueue());
                return;
            }
        }

        // 4. 선점 실패 또는 화면이 비어있는 경우, 일반 큐에 추가
        notificationQueue.Enqueue(newNotification);

        // 5. 큐 처리가 멈춰있다면 시작
        if (processingCoroutine == null)
        {
            processingCoroutine = StartCoroutine(ProcessQueue());
        }
    }

    /// <summary>
    /// NPC 상호작용 프롬프트처럼 자동으로 사라지지 않고 수동으로 숨겨야 하는 알림을 표시합니다.
    /// (Interaction 타입 전용) - 일반 알림 대기열 처리를 일시 정지합니다.
    /// </summary>
    /// <param name="message">표시할 상호작용 프롬프트 내용</param>
    /// <param name="caller">프롬프트를 표시하는 것을 요청한 GameObject입니다.</param>
    public void ShowInteractionPrompt(string message, GameObject caller)
    {
        // [수정] Interaction 타입은 최우선이므로 일반 알림 처리를 중단하고 표시해야 합니다.
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
            processingCoroutine = null;
        }
        // 현재 표시 중이던 알림은 Interaction이 끝난 후 다시 표시되도록 currentDisplayingData를 null로 설정하지 않습니다. 
        // ProcessQueue의 시작 조건에서 currentDisplayingData를 다시 사용할 것입니다.

        // 현재 요청자(caller)를 무조건 새로운 요청자로 업데이트하여 권한을 부여합니다.
        currentInteractionCaller = caller;

        // Interaction 스타일 적용
        SetNotificationStyle(NotificationType.Interaction);

        // 텍스트 및 패널 업데이트
        notificationText.text = message;
        notificationPanel.SetActive(true);
    }

    /// <summary>
    /// ShowInteractionPrompt로 표시된 알림을 수동으로 숨깁니다.
    /// 숨김 처리 후, 대기열에 알림이 남아있다면 처리를 재개합니다.
    /// </summary>
    /// <param name="caller">프롬프트 숨김을 요청한 GameObject입니다.</param>
    public void HideInteractionPrompt(GameObject caller)
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

            // [추가] Interaction이 끝났으므로, 대기열 처리를 재개합니다.
            if (processingCoroutine == null && (currentDisplayingData.HasValue || notificationQueue.Count > 0))
            {
                processingCoroutine = StartCoroutine(ProcessQueue());
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------------------
    // 내부 로직
    // ----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 대기열에 있는 알림을 순차적으로 처리하고 표시하는 코루틴입니다.
    /// 선점 실패로 큐에 추가된 알림이나, 선점되어 중단된 알림을 순서대로 표시합니다.
    /// </summary>
    private IEnumerator ProcessQueue()
    {
        while (currentDisplayingData.HasValue || notificationQueue.Count > 0)
        {
            // 1. 표시할 알림 데이터 가져오기 (currentDisplayingData는 선점되거나 다음 큐 항목일 수 있음)
            if (!currentDisplayingData.HasValue)
            {
                // 큐에서 다음 항목 가져오기
                if (notificationQueue.Count > 0)
                {
                    currentDisplayingData = notificationQueue.Dequeue();
                }
                else
                {
                    // 큐도 비어있으면 루프 종료
                    break;
                }
            }

            NotificationData data = currentDisplayingData.Value;

            // 2. 알림 표시
            SetNotificationStyle(data.Type);
            notificationText.text = data.Message;
            notificationPanel.SetActive(true);

            // 3. 지정된 시간만큼 대기 (이 대기 중에 고우선순위 알림이 들어오면 StopCoroutine으로 중단됩니다)
            yield return new WaitForSeconds(data.Duration);

            // 4. 대기 시간 완료 후 숨김 처리
            if (currentDisplayingData.HasValue && currentDisplayingData.Value.Message == data.Message)
            {
                // 숨김을 요청한 알림이 그 사이에 다른 알림으로 대체되지 않았는지 확인 후 숨김
                notificationPanel.SetActive(false);
            }

            // 5. 다음 항목을 준비하기 위해 현재 데이터 초기화
            currentDisplayingData = null;
        }

        // 6. 모든 알림 처리가 완료되면 코루틴 참조를 해제합니다.
        processingCoroutine = null;
    }

    /// <summary>
    /// 현재 표시 중이던 알림이 고우선순위 알림에게 선점당했을 때, 
    /// 기존 알림을 대기열의 맨 앞으로 다시 넣는 헬퍼 메서드입니다.
    /// </summary>
    /// <param name="data">선점당한 알림 데이터</param>
    private void ReQueueToFront(NotificationData data)
    {
        // C# 기본 Queue는 front 삽입을 지원하지 않으므로 List를 활용하여 재정렬합니다.
        List<NotificationData> tempQueueList = notificationQueue.ToList();
        notificationQueue.Clear();

        // 1. 선점당한 알림을 큐의 맨 앞에 다시 삽입
        tempQueueList.Insert(0, data);

        // 2. List에 있는 모든 항목을 Queue에 다시 넣어 순서를 확정합니다.
        foreach (var item in tempQueueList)
        {
            notificationQueue.Enqueue(item);
        }
    }


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
                /*if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Success, 0.5f);
                }*/
                break;
            case NotificationType.Warning:
                targetColor = warningColor;
                /*if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Warning, 0.5f);
                }*/
                break;
            case NotificationType.Interaction:
                targetColor = interactionColor;
                break;
            case NotificationType.General:
            default:
                /*if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.General, 0.5f);
                }*/
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

    // [삭제됨] 기존의 HideAfterDelay 코루틴은 ProcessQueue 코루틴에 통합되어 사용되지 않습니다.
}