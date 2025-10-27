using UnityEngine;

/// <summary>
/// 튜토리얼의 시각적 안내(UI) 기능을 정의하는 인터페이스입니다.
/// TutorialManager는 이 계약에만 의존하여 DIP(의존성 역전 원칙)를 준수합니다.
/// </summary>
public interface ITutorialView
{
    /// <summary>
    /// 특정 단계의 시각적 안내(팝업, 하이라이트, 화살표 등)를 활성화합니다.
    /// </summary>
    /// <param name="step">현재 튜토리얼 단계 (어떤 안내를 보여줄지 결정)</param>
    void ShowInstruction(TutorialStep step);

    /// <summary>
    /// 화면에 표시된 모든 튜토리얼 안내를 즉시 숨깁니다.
    /// (예: 단계 완료 또는 튜토리얼 스킵 시 호출)
    /// </summary>
    void HideAllInstruction();

    /// <summary>
    /// 튜토리얼 완료 메시지를 보여주고, 보상 지급 등 완료 연출을 담당합니다.
    /// </summary>
    void ShowCompletionMessage();
}
// TutorialStep.cs (Enum 정의)

/// <summary>
/// 튜토리얼의 각 단계를 정의합니다.
/// 새로운 튜토리얼 내용이 추가될 경우 이 Enum을 확장합니다. (OCP 준수)
/// <summary>
public enum TutorialStep
{
    // 0: 유저가 액자 UI를 열기 전까지 대기하는 초기 상태입니다.
    // 플레이어가 알아서 액자 근처로 이동하여 E키를 누르도록 유도합니다.
    Init = 0,

    // 1: 액자 UI가 열렸을 때, 던전 조각 배치를 안내하는 단계입니다.
    GuidePlace = 1,

    // 2: 튜토리얼이 성공적으로 완료되고, 자유 플레이로 넘어가는 단계입니다.
    Complete = 2
}

// TutorialManager.cs (핵심 관리자)

/// <summary>
/// 튜토리얼의 전체 진행 상태와 로직을 관리하는 싱글톤 컴포넌트입니다.
/// - 튜토리얼 단계 진행(AdvanceStep)의 유일한 책임을 가집니다. (SRP 준수)
/// - UI와의 의존성을 인터페이스(ITutorialView)로 분리하여 DIP를 준수합니다.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    // [Serialized Fields]

    [Header("Dependencies")]
    [Tooltip("UI 출력을 담당하는 ITutorialView 인터페이스 구현체입니다.")]
    [SerializeField] private MonoBehaviour viewImplementation;

    // [Private Fields]

    private TutorialStep currentStep = TutorialStep.Init;
    private ITutorialView tutorialView;

    // [Public Properties]

    /// <summary>
    /// 현재 튜토리얼 단계입니다.
    /// 외부에서 읽기 전용으로 접근하여 현재 상태를 파악할 수 있습니다.
    /// </summary>
    public TutorialStep CurrentStep => currentStep;

    // [Unity Lifecycle Methods]

    private void Awake()
    {
        if (viewImplementation is ITutorialView view)
        {
            this.tutorialView = view;
        }
        else
        {
            Debug.LogError("TutorialManager: View Implementation does not implement ITutorialView interface!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        StartTutorial();
    }

    /// <summary>
    /// 튜토리얼을 시작합니다. (수정됨)
    /// - SaveManager의 IsNewGame 상태를 확인하여 튜토리얼을 시작할지 스킵할지 결정합니다.
    /// </summary>
    public void StartTutorial()
    {
        // 1. 저장 시스템을 확인하여 '이어하기'인지 판단합니다.
        bool shouldSkip = false;

        // SaveManager가 씬에 존재하고, '새로하기' 상태가 아닌 경우 스킵합니다.
        // Singleton 패턴이므로 Instance에 직접 접근합니다.
        if (SaveManager.Instance != null && !SaveManager.Instance.IsNewGame)
        {
            shouldSkip = true;
        }

        if (shouldSkip)
        {
            // '이어하기' 세션인 경우, Complete 단계로 바로 전환하고 종료 처리합니다.
            currentStep = TutorialStep.Complete;
            // 튜토리얼 UI를 띄울 필요 없이, 시스템을 즉시 종료합니다.
            FinalizeSystemShutdown();
            return;
        }

        // 2. '새로하기'이거나 SaveManager가 없을 경우, 튜토리얼을 Init 단계부터 시작합니다.
        currentStep = TutorialStep.Init;


        // UI에 현재 단계를 알려줍니다.
        tutorialView.ShowInstruction(currentStep);
    }

    /// <summary>
    /// 외부 게임 이벤트(트리거)에 의해 호출되어 다음 단계로 진행합니다.
    /// 이 메서드가 이 클래스의 유일한 핵심 로직입니다. (SRP 준수)
    /// </summary>
    public void AdvanceStep()
    {
        if (currentStep == TutorialStep.Complete)
        {
            Debug.LogWarning("[TutorialManager] 이미 완료된 튜토리얼입니다. 추가 진행 무시.");
            return;
        }

        currentStep = (TutorialStep)((int)currentStep + 1);

        ProcessStep(currentStep);
    }

    /// <summary>
    /// 튜토리얼을 강제로 스킵하고 종료합니다.
    /// </summary>
    public void SkipTutorial()
    {
        currentStep = TutorialStep.Complete;

        tutorialView.HideAllInstruction();
        // TODO: (추후 추가) 튜토리얼 스킵 보상 지급 로직

        FinalizeSystemShutdown(); // 즉시 시스템 종료 호출
    }

    /// <summary>
    /// UI Handler가 완료 메시지를 숨긴 후, 최종적으로 시스템을 종료하기 위해 호출하는 메서드입니다. (추가된 메서드)
    /// </summary>
    public void FinalizeSystemShutdown()
    {
        // Manager 컴포넌트를 비활성화하여 더 이상 AdvanceStep이 작동하지 않게 합니다.
        this.enabled = false;
        // TODO: (추후 추가) 게임의 모든 기능 활성화 (예: 원래 막아뒀던 메뉴 버튼 등)
    }

    // [Private Methods]

    /// <summary>
    /// 현재 단계에 따라 필요한 내부 처리를 수행합니다.
    /// </summary>
    /// <param name="step">현재 진행된 튜토리얼 단계</param>
    private void ProcessStep(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.Init:
                break;

            case TutorialStep.GuidePlace:
                tutorialView.ShowInstruction(step);
                // TODO: (추후 추가) 배치 시스템의 트리거 컴포넌트를 활성화하여 이벤트를 기다립니다.
                break;

            case TutorialStep.Complete:
                // 완료 메시지를 출력하고, UI Handler가 타이머를 시작하여 최종 종료를 요청합니다.
                tutorialView.HideAllInstruction();
                tutorialView.ShowCompletionMessage();

                // FinalizeSystemShutdown()은 UI Handler의 타이머가 끝난 후 호출됩니다.
                break;
        }

        // TODO: (추후 추가) 현재 단계를 저장소에 영구 저장하는 로직을 호출합니다.
    }
}