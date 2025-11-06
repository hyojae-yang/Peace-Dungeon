using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// [Tutorial Event Bus & UI Handler]
/// 튜토리얼 진행을 위한 모든 핵심 이벤트를 모아두고, 안내 UI 관리를 담당하는 싱글톤 컴포넌트입니다. (SRP & DIP 준수)
/// - 이벤트를 통해 TutorialManager에 단계를 진행시키고, 게임 로직은 이벤트를 호출(Invoke)만 합니다.
/// - 모든 MonoBehaviour 기반의 개별 트리거 스크립트를 대체합니다.
/// </summary>
public class UITutorialHandler : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UITutorialHandler Instance { get; private set; }

    // =================================================================================
    // [핵심] 튜토리얼 진행 이벤트 버스 (TutorialManager가 구독합니다.)
    // =================================================================================
    [Header("Tutorial Advance Events (Invoke from Game Code)")]

    // 장비 및 UI 관련 이벤트
    public UnityEvent OnInventoryOpened = new UnityEvent();
    public UnityEvent OnGearEquipped = new UnityEvent();
    public UnityEvent OnBasicAttack = new UnityEvent(); // [추가] 좌클릭 기본 공격 감지
    public UnityEvent OnAimingPerformed = new UnityEvent(); // [추가] 우클릭 조준/방향 전환 감지
    public UnityEvent OnZoomChanged = new UnityEvent();
    public UnityEvent OnFrameUIOpened = new UnityEvent();

    // 던전 배치 관련 이벤트
    public UnityEvent OnPieceRetrieved = new UnityEvent();
    public UnityEvent OnPlacementComplete = new UnityEvent();
    public UnityEvent OnDungeonPlacementUIClose = new UnityEvent();
    public UnityEvent OnDungeonEntryDetected = new UnityEvent();

    // 성장 및 스킬 관련 이벤트
    public UnityEvent OnLevelUpDetected = new UnityEvent();
    public UnityEvent OnStatAllocated = new UnityEvent();
    public UnityEvent OnSkillAllocationOpened = new UnityEvent();
    public UnityEvent OnSkillPointsApplied = new UnityEvent();
    public UnityEvent OnSkillRegisteredToSlot = new UnityEvent();
    public UnityEvent OnSkillUsed = new UnityEvent(); // [최종 추가] 스킬 사용 감지

    // =================================================================================
    // [UI 컴포넌트 및 메시지 관리]
    // =================================================================================
    [Header("UI Components")]
    [Tooltip("인벤토리 열기 등 메인 캔버스에 표시되는 주 안내 패널입니다.")]
    [SerializeField] private TutorialInstructionPanel instructionPanel;

    [Tooltip("액자 UI 옆 등 보조적인 위치에 표시되는 부 안내 패널입니다.")]
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
    /// (보조 패널이 활성화되어 있다면 비활성화합니다.)
    /// </summary>
    public void ShowPrimaryInstruction(string message)
    {
        secondInstructionPanel?.gameObject.SetActive(false);
        instructionPanel?.ShowInstruction(message); // 패널 활성화 로직은 ShowInstruction 내부에 포함 가정
    }

    /// <summary>
    /// 보조 안내 패널을 활성화하고 메시지를 표시합니다.
    /// (메인 패널이 활성화되어 있다면 비활성화합니다.)
    /// </summary>
    public void ShowSecondInstruction(string message)
    {
        instructionPanel?.gameObject.SetActive(false);
        secondInstructionPanel?.ShowInstruction(message); // 패널 활성화 로직은 ShowInstruction 내부에 포함 가정
    }

    /// <summary>
    /// 튜토리얼 완료 메시지를 표시하고, 완료 후 시스템 종료를 요청합니다.
    /// </summary>
    public void ShowCompletionUI()
    {
        secondInstructionPanel?.gameObject.SetActive(false);
        // ShowCompletionMessage 내부에 TutorialManager.FinalizeSystemShutdown() 호출이 연결되어야 합니다.
        instructionPanel?.ShowCompletionMessage();
    }

    /// <summary>
    /// 모든 안내 패널을 즉시 숨깁니다. (스킵 또는 단계 전환 시 사용)
    /// </summary>
    public void HideAllUI()
    {
        instructionPanel?.StopAllCoroutines();
        secondInstructionPanel?.StopAllCoroutines();

        instructionPanel?.gameObject.SetActive(false);
        secondInstructionPanel?.gameObject.SetActive(false);
    }

    /// <summary>
    /// 던전 배치 중 유효하지 않은 배치 경고를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 경고 메시지</param>
    /// <param name="duration">메시지가 유지될 시간</param>
    public void ShowInvalidPlacementNotification(string message, float duration)
    {
        // 보조 패널을 사용하여 경고 메시지를 잠시 표시합니다.
        secondInstructionPanel?.ShowTemporaryInstruction(message, duration);
    }

    // === Public Event Invokers (게임 로직에서 호출되어야 합니다.) ===

    // 장비 및 UI 관련 Invoker
    public void NotifyInventoryOpened() => OnInventoryOpened.Invoke();
    public void NotifyGearEquipped() => OnGearEquipped.Invoke();
    public void NotifyBasicAttack() => OnBasicAttack.Invoke(); // [추가]
    public void NotifyAimingPerformed() => OnAimingPerformed.Invoke(); // [추가]
    public void NotifyZoomChanged() => OnZoomChanged.Invoke();
    // 던전 배치 관련 Invoker
    public void NotifyDungeonPlacementUIClose() => OnDungeonPlacementUIClose.Invoke();
    public void NotifyDungeonEntryDetected() => OnDungeonEntryDetected.Invoke();

    // 성장 및 스킬 관련 Invoker
    public void NotifyLevelUpDetected() => OnLevelUpDetected.Invoke();
    public void NotifyStatAllocated() => OnStatAllocated.Invoke();
    public void NotifySkillAllocationOpened() => OnSkillAllocationOpened.Invoke();
    public void NotifySkillPointsApplied() => OnSkillPointsApplied.Invoke();
    public void NotifySkillRegisteredToSlot() => OnSkillRegisteredToSlot.Invoke();

    /// <summary>
    /// [최종 추가] 퀵 슬롯에 등록된 스킬을 플레이어가 실제로 사용했을 때 호출됩니다.
    /// </summary>
    public void NotifySkillUsed() => OnSkillUsed.Invoke();
}