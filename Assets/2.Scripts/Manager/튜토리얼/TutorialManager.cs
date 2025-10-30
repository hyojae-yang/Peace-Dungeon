using UnityEngine;
using UnityEngine.Events;
using System.Collections; // 코루틴 사용을 위해 추가 (기존 코드에 없었으나 안전을 위해 추가)

// ITutorialTrigger 인터페이스는 이제 사용되지 않으므로 제거하거나 그대로 두셔도 됩니다.
// UITutorialHandler 클래스가 TutorialManager와 같은 씬에 존재하고 싱글톤으로 초기화되었다고 가정합니다.

/// <summary>
/// 튜토리얼의 각 단계를 정의합니다.
/// 새로운 튜토리얼 내용이 추가될 경우 이 Enum을 확장합니다. (OCP 준수)
/// <summary>
public enum TutorialStep
{
    // 0: 게임 시작 직후, 검은색 패널 페이드 아웃 연출을 담당합니다.
    IntroFade = 0,

    // 1: IntroFade 완료 후 스토리 메시지를 보여주는 단계입니다.
    StorySequence = 1,

    // 2: 인벤토리를 열도록 유도하는 안내 패널을 띄우는 단계입니다.
    GuideOpenInventory = 2,

    // 3: 장비 장착을 유도하는 안내 패널을 띄우는 단계입니다.
    GuideEquipGear = 3,

    // 4: 유저가 액자 UI를 열기 전까지 대기하는 초기 상태입니다.
    Init = 4,

    // [수정/신규] 5: 액자 UI가 열렸을 때, 던전 조각을 인벤토리에서 '꺼내는' 것을 안내하는 단계입니다.
    GuideRetrievePiece = 5,

    // [신규] 6: 유효한 위치에 던전 조각 '배치'가 완료되기를 기다리는 단계입니다.
    WaitPlacementComplete = 6,

    // [수정] 7: 튜토리얼이 성공적으로 완료되고, 자유 플레이로 넘어가는 단계입니다. 
    Complete = 7
}

/// <summary>
/// 튜토리얼의 전체 진행 상태와 로직을 관리하는 싱글톤 컴포넌트입니다.
/// - 튜토리얼 단계 진행(AdvanceStep)의 유일한 책임을 가집니다. (SRP 준수)
/// - UI 및 이벤트 감지 책임은 UITutorialHandler에 위임합니다. (DIP 준수)
/// </summary>
public class TutorialManager : MonoBehaviour
{
    // [추가] 싱글톤 인스턴스입니다.
    public static TutorialManager Instance { get; private set; }

    // [Serialized Fields] - UI 관련 컴포넌트는 UITutorialHandler로 이동했습니다.
    [Tooltip("스토리 연출을 담당하는 StoryPanelController 컴포넌트입니다.")]
    [SerializeField] private StoryPanelController storyPanelController;

    // [Private Fields] - 이벤트 버스 및 UI 핸들러 참조
    private UITutorialHandler uiHandler;
    private TutorialStep currentStep = TutorialStep.IntroFade;

    // [Public Properties]
    /// <summary>
    /// 현재 튜토리얼 단계입니다.
    /// 외부에서 읽기 전용으로 접근하여 현재 상태를 파악할 수 있습니다.
    /// </summary>
    public TutorialStep CurrentStep => currentStep;

    // InstructionPanel 프로퍼티는 제거되었습니다. UI 접근은 UITutorialHandler를 통해서만 이루어집니다.

    // [Unity Lifecycle Methods]
    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        

    }

    private void Start()
    {// [핵심 변경] UITutorialHandler (이벤트 버스) 참조 획득
        uiHandler = UITutorialHandler.Instance;
        if (uiHandler == null)
        {
            Debug.LogError("[TutorialManager] UITutorialHandler 인스턴스를 씬에서 찾을 수 없습니다! 튜토리얼이 작동하지 않습니다.");
        }

        // StoryPanelController 의존성 검사 및 이벤트 연결
        if (storyPanelController != null)
        {
            // 스토리 완료 시 AdvanceStep()을 호출합니다.
            storyPanelController.OnStoryComplete.AddListener(AdvanceStep);
            storyPanelController.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TutorialManager: Story Panel Controller가 할당되지 않았습니다. 스토리 단계를 스킵합니다.");
        }
        StartTutorial();
    }

    /// <summary>
    /// 튜토리얼을 시작합니다.
    /// </summary>
    public void StartTutorial()
    {
        // 1. 저장 시스템 확인 로직 (기존 로직 유지)
        bool shouldSkip = false;
        if (shouldSkip)
        {
            currentStep = TutorialStep.Complete;
            FinalizeSystemShutdown();
            return;
        }

        // 2. '새로하기' 세션일 경우, 튜토리얼을 IntroFade 단계부터 시작합니다.
        currentStep = TutorialStep.IntroFade;
        ProcessStep(currentStep);
    }

    /// <summary>
    /// 외부 게임 이벤트(트리거)에 의해 호출되어 다음 단계로 진행합니다.
    /// </summary>
    public void AdvanceStep()
    {
        if (currentStep == TutorialStep.Complete)
        {
            Debug.LogWarning("[TutorialManager] 이미 완료된 튜토리얼입니다. 추가 진행 무시.");
            return;
        }

        // [핵심 변경] 다음 단계로 진행하기 전에 이전 단계에서 구독한 이벤트를 모두 해제합니다.
        UnsubscribeFromAllEvents();

        // [수정]: Enum이 커졌으므로 인덱스 7을 넘어 8(Complete)로 이동합니다.
        currentStep = (TutorialStep)((int)currentStep + 1);

        ProcessStep(currentStep);
    }

    /// <summary>
    /// 튜토리얼을 강제로 스킵하고 종료합니다.
    /// </summary>
    public void SkipTutorial()
    {
        currentStep = TutorialStep.Complete;

        // [변경] 모든 안내 UI 제어를 UITutorialHandler에 위임
        uiHandler?.HideAllUI();
        storyPanelController?.gameObject.SetActive(false); // 스토리 패널만 직접 제어

        // [변경] 모든 이벤트 구독을 해제합니다.
        UnsubscribeFromAllEvents();

        FinalizeSystemShutdown(); // 즉시 시스템 종료 호출
    }

    /// <summary>
    /// UI Handler가 완료 메시지를 숨긴 후, 최종적으로 시스템을 종료하기 위해 호출하는 메서드입니다.
    /// </summary>
    public void FinalizeSystemShutdown()
    {
        this.enabled = false;
        // TODO: (추후 추가) 게임의 모든 기능 활성화
    }

    /// <summary>
    /// 던전 배치 튜토리얼 중, 유효하지 않은 위치에 조각을 놓았을 때 경고 메시지를 표시합니다.
    /// (DungeonMap.cs에서 호출됩니다.)
    /// </summary>
    public void ShowInvalidPlacementNotification()
    {
        // 현재 단계가 유효성 검사가 필요한 'WaitPlacementComplete' 단계인지 확인합니다.
        if (currentStep != TutorialStep.WaitPlacementComplete)
        {
            return;
        }

        // [변경] 경고 메시지 표시를 UITutorialHandler에 위임
        uiHandler?.ShowInvalidPlacementNotification("유효한 던전 타일 영역에 조각을 배치해야 합니다!", 2.0f);
    }

    /// <summary>
    /// [신규] UITutorialHandler의 모든 이벤트에서 AdvanceStep 리스너를 해제합니다.
    /// 단계 진행 시 중복 호출을 방지합니다.
    /// </summary>
    private void UnsubscribeFromAllEvents()
    {
        if (uiHandler == null) return;

        uiHandler.OnInventoryOpened.RemoveListener(AdvanceStep);
        uiHandler.OnGearEquipped.RemoveListener(AdvanceStep);
        uiHandler.OnFrameUIOpened.RemoveListener(AdvanceStep);
        uiHandler.OnPieceRetrieved.RemoveListener(AdvanceStep);
        uiHandler.OnPlacementComplete.RemoveListener(AdvanceStep);
    }

    /// <summary>
    /// 현재 단계에 따라 필요한 내부 처리를 수행하고, UI 뷰에 표시를 요청합니다.
    /// </summary>
    /// <param name="step">현재 진행된 튜토리얼 단계</param>
    private void ProcessStep(TutorialStep step)
    {
        // [핵심 변경] 모든 UI 및 트리거 SetActive 로직이 제거되었습니다.

        switch (step)
        {
            case TutorialStep.IntroFade:
                // 별도 페이드 아웃 로직 처리
                break;

            case TutorialStep.StorySequence:
                storyPanelController?.gameObject.SetActive(true);
                break;

            // ----------------- 1번 패널 활성화 단계 (메인 캔버스) -----------------
            case TutorialStep.GuideOpenInventory: // 2
                storyPanelController?.gameObject.SetActive(false); // 스토리 패널 클린업
                uiHandler?.ShowPrimaryInstruction("G 키를 눌러서 \n장비창을 열어 보세요.");
                // 이벤트 구독: 인벤토리 열림을 기다립니다.
                uiHandler.OnInventoryOpened.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideEquipGear: // 3
                uiHandler?.ShowPrimaryInstruction("동검 아이콘을 우클릭하여 장비를 장착해보세요.");
                // 이벤트 구독: 장비 장착 완료를 기다립니다.
                uiHandler.OnGearEquipped.AddListener(AdvanceStep);
                break;

            case TutorialStep.Init: // 4
                uiHandler?.ShowPrimaryInstruction("액자 앞으로 이동하여 상호작용 E 키를 누르세요.");
                // 이벤트 구독: 액자 UI 열림을 기다립니다.
                uiHandler.OnFrameUIOpened.AddListener(AdvanceStep);
                break;

            // ----------------- 2번 패널 활성화 단계 (보조 캔버스) -----------------
            case TutorialStep.GuideRetrievePiece: // 5 (꺼내기 안내)
                uiHandler?.ShowSecondInstruction("좌측에 보이는 던전조각을 드래그 해서 내려놓으세요.");
                // 이벤트 구독: 던전 조각 꺼내기를 기다립니다.
                uiHandler.OnPieceRetrieved.AddListener(AdvanceStep);
                break;

            case TutorialStep.WaitPlacementComplete: // 6 (배치 대기)
                uiHandler?.ShowSecondInstruction("조각을 던전 문앞에 배치해보세요.");
                // 이벤트 구독: 던전 배치 완료를 기다립니다.
                uiHandler.OnPlacementComplete.AddListener(AdvanceStep);
                break;

            case TutorialStep.Complete: // 7 (최종 완료)
                // [변경] 완료 처리를 UITutorialHandler에 위임
                uiHandler?.ShowCompletionUI();
                break;
        }
    }

    // 💡 ActivateTriggerOrSkip 메서드는 제거되었습니다.
}