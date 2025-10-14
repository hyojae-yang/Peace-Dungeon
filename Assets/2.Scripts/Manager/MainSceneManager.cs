using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System;

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

    [Header("게임 오버 패널")]
    [SerializeField]
    private GameObject gameOverPanel;

    public bool isGameOver = false;

    public static event Action OnGameOver; // <-- 게임 오버 이벤트 추가

    [SerializeField] GameObject player;

    /// <summary>
    /// LoadingManager가 다음에 로드해야 할 최종 목적지 씬의 이름입니다.
    /// 정적 변수로 설정하여 어떤 씬에서도 접근할 수 있도록 합니다.
    /// </summary>
    public static string NextSceneToLoad = ""; // <-- 이 변수를 추가합니다.
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

        gameOverPanel.SetActive(false);
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
    public void Exit()
    {
        // [수정] 1. 최종 목적지(TitleScene)를 정적 변수에 설정
        MainSceneManager.NextSceneToLoad = "TitleScene";

        // [수정] 2. LoadingScene으로 전환하여 비동기 로드를 시작
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
    }
    public void save()
    {
        if (DungeonManager.Instance.IsInDungeon)
        {
            Debug.Log("던전 내부에서는 저장할 수 없습니다.");
            return;
        }
        SaveManager.Instance.SaveGame();
    }
    /// <summary>
    /// [추가된 기능] 외부(예: PlayerHealth)에서 게임 오버를 선언할 때 호출되는 메서드입니다.
    /// isGameOver 상태를 true로 설정하고, 게임 오버 시 필요한 초기 UI 로직을 실행합니다.
    /// </summary>
    public void SetGameOver()
    {
        OnGameOver?.Invoke();
        // 게임 오버 상태에 진입하면 PlayerCanvas를 비활성화하여
        // 플레이어의 상호작용을 막습니다.
        if (playerCanvas != null && playerCanvas.activeInHierarchy)
        {
            playerCanvas.SetActive(false);
        }
        gameOverPanel.SetActive(true);
        // TODO: Time.timeScale = 0; 또는 게임 오버 패널 활성화 등의 추가 로직을 여기에 구현합니다.
    }
    public void Restart()
    {
        // 1. **가장 먼저** isGameOver 상태를 재시작 상태(false)로 변경하여 
        //    DungeonManager가 보상 로직을 실행하지 못하게 막습니다.
        isGameOver = false; // 위치 변경
        // 2. 저장 불러오기 (위치, 스탯 등 모든 게임 데이터 복구)
        //    이것이 먼저 실행되어야 던전 상태를 리셋할 때 충돌이 적습니다.
        SaveManager.Instance.LoadGame();

        // [수정] 3. MainScene으로 즉시 로드하는 대신, 목표를 설정하고 LoadingScene으로 전환
        MainSceneManager.NextSceneToLoad = "MainScene";
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene"); // <-- 이 부분 수정

        // [중요] 씬 전환이 일어난 후에는 이 아래의 코드는 실행되지 않거나,
        // 새 씬의 오브젝트 인스턴스에 접근하므로 오류가 발생할 수 있습니다.
        // **따라서 아래의 3, 4번 로직은 새 MainScene 인스턴스가 로드된 후에 실행되어야 합니다.**

        // 3. UI 및 플레이어 리셋
        /*if (playerCanvas != null)
        {
            playerCanvas.SetActive(true);
        }
        gameOverPanel.SetActive(false);
        player.SetActive(true);
        PlayerCharacter.Instance.playerStats.health = PlayerCharacter.Instance.playerStats.MaxHealth;
        PlayerCharacter.Instance.playerStats.mana = PlayerCharacter.Instance.playerStats.MaxMana;
        PlayerCharacter.Instance.playerController.outDungeon(); // 플레이어 컨트롤러 상태 리셋

        // 4. 던전 상태 및 보스 파괴 (보스 파괴는 로드 후 잔여 오브젝트 정리 목적으로 실행)
        //    이 시점에서는 isGameOver가 이미 false이므로 DeadDungeon이 클리어 로직을 실행하지 못합니다.
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.IsInDungeon = false;
            DungeonManager.Instance._isBossRoomActive = false;

            if (DungeonManager.Instance.currentBossInstance != null)
            {
                Destroy(DungeonManager.Instance.currentBossInstance.gameObject);
                DungeonManager.Instance.currentBossInstance = null; // 인스턴스 참조도 확실히 제거
            }
            DungeonManager.Instance.DeadDungeon();
        }*/
    }
}