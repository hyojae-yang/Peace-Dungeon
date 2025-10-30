using UnityEngine;
using UnityEngine.Events;
using System.Collections; // 코루틴 사용을 위해 추가

/// <summary>
/// [Tutorial Event Bus & UI Handler]
/// 튜토리얼 진행을 위한 모든 핵심 이벤트를 모아두고, 안내 UI 관리를 담당하는 싱글톤 컴포넌트입니다. (SRP & DIP 준수)
/// - 이벤트를 통해 TutorialManager에 단계를 진행시키고, 게임 로직은 이벤트를 호출(Invoke)만 합니다.
/// - 모든 MonoBehaviour 기반의 개별 트리거 스크립트를 대체합니다.
/// </summary>
public class UITutorialHandler : MonoBehaviour
{
    // [싱글톤 인스턴스]
    public static UITutorialHandler Instance { get; private set; }

    // =================================================================================
    // [핵심] 튜토리얼 진행 이벤트 버스 (TutorialManager가 구독합니다.)
    // =================================================================================
    [Header("Tutorial Advance Events (Invoke from Game Code)")]
    [Tooltip("인벤토리 열기 감지 이벤트. TutorialManager.AdvanceStep()과 연결됩니다.")]
    public UnityEvent OnInventoryOpened = new UnityEvent();          // GuideOpenInventory 단계 완료

    [Tooltip("장비 장착 완료 감지 이벤트.")]
    public UnityEvent OnGearEquipped = new UnityEvent();             // GuideEquipGear 단계 완료

    [Tooltip("액자 UI 열림 감지 이벤트. (상호작용 E 키 누름 등)")]
    public UnityEvent OnFrameUIOpened = new UnityEvent();            // Init 단계 완료

    [Tooltip("인벤토리에서 던전 조각을 꺼내 드래그 시작 감지 이벤트.")]
    public UnityEvent OnPieceRetrieved = new UnityEvent();          // GuideRetrievePiece 단계 완료

    [Tooltip("유효한 위치에 던전 조각 배치 완료 감지 이벤트.")]
    public UnityEvent OnPlacementComplete = new UnityEvent();       // WaitPlacementComplete 단계 완료

    // =================================================================================
    // [UI 컴포넌트 및 메시지 관리]
    // =================================================================================
    [Header("UI Components")]
    [Tooltip("인벤토리 열기 등 메인 캔버스에 표시되는 안내 패널입니다.")]
    [SerializeField] private TutorialInstructionPanel instructionPanel;

    [Tooltip("액자 UI 옆 등 보조적인 위치에 표시되는 안내 패널입니다.")]
    [SerializeField] private TutorialInstructionPanel secondInstructionPanel;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 초기 상태: 모든 패널 비활성화
        instructionPanel?.gameObject.SetActive(false);
        secondInstructionPanel?.gameObject.SetActive(false);
    }

    // === Public UI Control Methods (TutorialManager에서 호출됨) ===

    /// <summary>
    /// 메인 안내 패널을 활성화하고 메시지를 표시합니다.
    /// </summary>
    public void ShowPrimaryInstruction(string message)
    {
        // 상호 배타적 활성화
        secondInstructionPanel?.gameObject.SetActive(false);
        // instructionPanel의 ShowInstruction 내부에 패널 활성화 로직이 포함되어야 합니다.
        instructionPanel?.ShowInstruction(message);
    }

    /// <summary>
    /// 보조 안내 패널을 활성화하고 메시지를 표시합니다.
    /// </summary>
    public void ShowSecondInstruction(string message)
    {
        // 상호 배타적 활성화
        instructionPanel?.gameObject.SetActive(false);
        // secondInstructionPanel의 ShowInstruction 내부에 패널 활성화 로직이 포함되어야 합니다.
        secondInstructionPanel?.ShowInstruction(message);
    }

    /// <summary>
    /// 튜토리얼 완료 메시지를 표시하고, 완료 후 시스템 종료를 요청합니다.
    /// </summary>
    public void ShowCompletionUI()
    {
        // 완료 단계에서는 모든 안내 패널 비활성화
        instructionPanel?.gameObject.SetActive(false);

        // 보조 패널을 사용하여 완료 메시지 표시 요청
        // ShowCompletionMessage 내부에 최종 종료(FinalizeSystemShutdown) 로직이 연결되어야 합니다.
        secondInstructionPanel?.ShowCompletionMessage();
    }

    /// <summary>
    /// 모든 안내 패널을 즉시 숨깁니다. (스킵 시 사용)
    /// </summary>
    public void HideAllUI()
    {
        // 진행 중이던 모든 코루틴을 중지 (예: 임시 메시지, 완료 메시지 코루틴)
        instructionPanel?.StopAllCoroutines();
        secondInstructionPanel?.StopAllCoroutines();

        instructionPanel?.gameObject.SetActive(false);
        secondInstructionPanel?.gameObject.SetActive(false);
    }

    /// <summary>
    /// 던전 배치 중 유효하지 않은 배치 경고를 표시합니다. (DungeonMap.cs에서 호출 가능)
    /// </summary>
    /// <param name="message">표시할 경고 메시지</param>
    /// <param name="duration">메시지가 유지될 시간</param>
    public void ShowInvalidPlacementNotification(string message, float duration)
    {
        // 보조 패널을 사용하여 경고 메시지를 잠시 표시합니다.
        // 이 메서드는 TutorialInstructionPanel의 ShowTemporaryInstruction을 호출합니다.
        secondInstructionPanel?.ShowTemporaryInstruction(message, duration);
    }

    // === Public Event Invokers (Game Code에서 호출되어야 합니다.) ===
    // 이 메서드들은 실제로 게임 로직(예: Inventory.cs, DungeonMap.cs)에서 호출되어야 합니다.

    /// <summary>
    /// 인벤토리가 열리는 순간 게임 로직에서 호출되어야 합니다.
    /// </summary>
    public void NotifyInventoryOpened()
    {
        OnInventoryOpened.Invoke();
    }

    /// <summary>
    /// 장비 장착이 완료되는 순간 게임 로직에서 호출되어야 합니다.
    /// </summary>
    public void NotifyGearEquipped()
    {
        OnGearEquipped.Invoke();
    }

    // ... 나머지 이벤트도 필요하다면 Invoker 메서드를 추가할 수 있습니다.
}