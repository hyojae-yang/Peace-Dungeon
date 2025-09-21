using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 씬의 주요 UI 패널들을 중앙에서 관리하는 매니저 클래스입니다.
/// 특정 팝업 패널이 활성화되면 PlayerCanvas를 비활성화하고,
/// 모든 팝업 패널이 비활성화되면 PlayerCanvas를 다시 활성화합니다.
/// SOLID: 개방-폐쇄 원칙 (새로운 팝업 패널 추가 시 이 스크립트의 코드 수정 필요 없음)
/// </summary>
public class MainSceneManager : MonoBehaviour
{
    // MainSceneManager의 싱글턴 인스턴스
    public static MainSceneManager Instance { get; private set; }

    [Header("UI Group References")]
    [Tooltip("게임 플레이 중 항상 활성화되어야 하는 메인 UI 캔버스입니다.")]
    [SerializeField]
    private GameObject playerCanvas;

    [Tooltip("특정 이벤트로 인해 활성화되어 PlayerCanvas를 덮는 팝업 패널들입니다.")]
    [SerializeField]
    private List<GameObject> popUpPanels = new List<GameObject>();

    [Tooltip("던전 캔버스를 직접 할당합니다. 던전 상태를 추적하는 데 사용됩니다.")]
    [SerializeField]
    private GameObject dungeonCanvas;

    [Header("UI 상태 추적 변수")]
    [Tooltip("던전 캔버스가 현재 활성화되어 있는지 여부를 나타냅니다.")]
    public bool isDungeonCanvasActive = false;

    /// <summary>
    /// 스크립트 인스턴스가 로드될 때 호출되어 싱글턴을 설정하고 이벤트 리스너를 등록합니다.
    /// </summary>
    private void Awake()
    {
        // 1. 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 이미 다른 MainSceneManager 인스턴스가 존재합니다. 새로운 인스턴스를 파괴합니다.");
            Destroy(gameObject);
        }

        // 2. UIEventHandler의 두 이벤트에 모두 구독
        UIEventHandler.OnPanelActivated += HandlePanelActivation;
        UIEventHandler.OnPanelDeactivated += HandlePanelDeactivation;
    }

    /// <summary>
    /// 이벤트를 통해 패널 활성화 신호를 받으면 호출되는 메서드입니다.
    /// 활성화된 패널이 팝업 패널이면 PlayerCanvas를 비활성화하고, 던전 캔버스라면 상태 변수를 업데이트합니다.
    /// </summary>
    /// <param name="activatedPanel">활성화된 패널의 게임 오브젝트입니다.</param>
    private void HandlePanelActivation(GameObject activatedPanel)
    {
        // 활성화된 패널이 팝업 패널 리스트에 포함되어 있는지 확인합니다.
        if (popUpPanels.Contains(activatedPanel))
        {
            // PlayerCanvas가 이미 비활성화 상태가 아닐 경우에만 비활성화합니다.
            if (playerCanvas.activeInHierarchy)
            {
                playerCanvas.SetActive(false);
            }
        }

        // 활성화된 패널이 할당된 던전 캔버스인지 확인하고 변수를 업데이트합니다.
        if (activatedPanel == dungeonCanvas)
        {
            isDungeonCanvasActive = true;
        }
    }

    /// <summary>
    /// 이벤트를 통해 패널 비활성화 신호를 받으면 호출되는 메서드입니다.
    /// 모든 팝업 패널이 꺼졌을 때만 PlayerCanvas를 다시 활성화하고, 던전 캔버스라면 상태 변수를 업데이트합니다.
    /// </summary>
    /// <param name="deactivatedPanel">비활성화된 패널의 게임 오브젝트입니다.</param>
    private void HandlePanelDeactivation(GameObject deactivatedPanel)
    {
        // 비활성화된 패널이 팝업 패널 리스트에 포함되어 있는지 확인합니다.
        if (popUpPanels.Contains(deactivatedPanel))
        {
            // LINQ를 사용하여 현재 활성화된 팝업 패널이 있는지 확인합니다.
            bool anyPopUpPanelIsActive = popUpPanels.Any(panel => panel.activeInHierarchy);

            // 활성화된 팝업 패널이 더 이상 없을 경우에만 PlayerCanvas를 활성화합니다.
            if (!anyPopUpPanelIsActive)
            {
                playerCanvas.SetActive(true);
            }
        }

        // 비활성화된 패널이 할당된 던전 캔버스인지 확인하고 변수를 업데이트합니다.
        if (deactivatedPanel == dungeonCanvas)
        {
            isDungeonCanvasActive = false;
        }
    }

    /// <summary>
    /// 게임 오브젝트가 파괴될 때 호출되어 이벤트 리스너를 해제합니다.
    /// 메모리 누수를 방지하기 위한 필수 작업입니다.
    /// </summary>
    private void OnDestroy()
    {
        UIEventHandler.OnPanelActivated -= HandlePanelActivation;
        UIEventHandler.OnPanelDeactivated -= HandlePanelDeactivation;
    }
}