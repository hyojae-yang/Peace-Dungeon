// FrameUITrigger.cs

using UnityEngine;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 필요합니다.

/// <summary>
/// 튜토리얼의 첫 단계를 진행하기 위한 트리거 컴포넌트입니다.
/// - 던전 액자 상호작용 시스템에 부착되어, 액자 UI가 열리는 이벤트를 감지합니다.
/// - TutorialManager.AdvanceStep()을 호출하는 책임을 가집니다. (SRP/ISP 준수)
/// </summary>
public class FrameUITrigger : MonoBehaviour
{
    // [Dependencies]

    // UnityEvent를 사용하여 코드 참조 없이 인스펙터에서 메서드를 연결할 수 있게 합니다.
    [Header("Dependencies")]
    [Tooltip("액자 UI가 열리는 순간 호출되어야 할 TutorialManager의 AdvanceStep()을 연결하세요.")]
    // 인스펙터에서 TutorialManager.AdvanceStep()을 연결할 것입니다.
    public UnityEvent OnFrameUIOpened = new UnityEvent();

    // [Private Fields]

    private bool isTriggered = false; // 이벤트 중복 호출 방지용 플래그

    // [Private Properties]

    /// <summary>
    /// 씬에서 TutorialManager 인스턴스를 찾아옵니다. (경고 해결 및 안전성 확보)
    /// </summary>
    private TutorialManager TutorialManagerInstance
    {
        // FindObjectOfType 대신 최신 API를 사용하여 씬에서 Manager를 찾습니다.
        // 이 속성은 호출될 때마다 Manager를 찾으므로, 성능 최적화를 위해
        // Start/Awake에서 한 번만 찾아 필드에 저장하는 방식으로 추후 개선될 수 있습니다.
        get { return FindFirstObjectByType<TutorialManager>(); }
    }

    // [Public Methods]

    /// <summary>
    /// 액자 상호작용을 담당하는 기존 컴포넌트에서 이 메서드를 호출해야 합니다.
    /// (예: 기존 DungeonFrameInteraction.cs의 OpenUI() 메서드 마지막 줄에 호출)
    /// </summary>
    public void NotifyFrameUIOpened()
    {
        // 튜토리얼이 이미 진행되었거나 완료되었다면 무시합니다.
        if (isTriggered)
        {
            return;
        }

        TutorialManager manager = TutorialManagerInstance;

        // 1. TutorialManager가 씬에 존재하는지 확인하고 (안전성)
        // 2. 현재 단계가 Init 단계에 있을 때만 이벤트를 발생시킵니다.
        if (manager != null && manager.CurrentStep == TutorialStep.Init)
        {
            isTriggered = true;
            // 인스펙터에 연결된 TutorialManager.AdvanceStep()을 호출합니다.
            OnFrameUIOpened.Invoke();
        }
        else if (manager == null)
        {
            Debug.LogWarning("[FrameUITrigger] 경고: 튜토리얼 매니저를 씬에서 찾을 수 없습니다. 연결을 확인하세요.");
        }
    }

    /// <summary>
    /// 액자 UI가 닫힐 때 호출되어 isTriggered를 리셋할 수도 있습니다.
    /// (현재는 Init 단계에서만 사용되므로 필수는 아닙니다.)
    /// </summary>
    public void ResetTrigger()
    {
        isTriggered = false;
    }
}