// DungeonPlacementTrigger.cs

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 튜토리얼의 'GuidePlace' 단계를 완료하는 트리거 컴포넌트입니다.
/// - 던전 배치 로직(DungeonPlacer.cs 등)에 부착되어, 조각 배치가 완료되는 순간을 감지합니다.
/// - 성공적으로 튜토리얼이 진행되면 AdvanceStep()을 호출합니다. (SRP 및 DIP 준수)
/// </summary>
public class DungeonPlacementTrigger : MonoBehaviour
{
    // [Dependencies]

    // UnityEvent를 사용하여 인스펙터에서 TutorialManager.AdvanceStep()을 연결합니다.
    [Header("Dependencies")]
    [Tooltip("조각 배치가 성공적으로 완료되는 순간 호출되어야 할 TutorialManager.AdvanceStep()을 연결하세요.")]
    public UnityEvent OnPlacementCompleted = new UnityEvent();

    // [Private Fields]

    // 배치 트리거는 오직 한 번만 작동해야 합니다. (첫 번째 배치 완료 시)
    private bool isCompleted = false;

    // [Public Methods]

    /// <summary>
    /// 던전 배치 시스템의 '조각 배치 성공' 로직이 완료된 직후 이 메서드를 호출해야 합니다.
    /// (예: DungeonPlacer.cs에서 조각이 3D 맵에 최종적으로 생성된 후)
    /// </summary>
    public void NotifyPlacementCompleted()
    {
        // 튜토리얼이 이미 완료되었다면 중복 호출을 막습니다.
        if (isCompleted)
        {
            return;
        }

        // 1. 현재 튜토리얼 단계가 'GuidePlace' 단계인지 확인합니다.
        //    (Manager를 찾아야 함: 안정성을 위해 FindFirstObjectByType 사용)
        TutorialManager manager = FindFirstObjectByType<TutorialManager>();

        if (manager != null && manager.CurrentStep == TutorialStep.GuidePlace)
        {
            isCompleted = true; // 완료 상태로 설정

            // 인스펙터에 연결된 TutorialManager.AdvanceStep()을 호출합니다.
            OnPlacementCompleted.Invoke();

            // Note: 이 트리거는 이후 다시 사용되지 않으므로, 스스로 비활성화하는 것도 고려할 수 있습니다.
            // this.enabled = false;
        }
        else if (manager == null)
        {
            Debug.LogWarning("[DungeonPlacementTrigger] 경고: 튜토리얼 매니저를 씬에서 찾을 수 없습니다.");
        }
    }
}