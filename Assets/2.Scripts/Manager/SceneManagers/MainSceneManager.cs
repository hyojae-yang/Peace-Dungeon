using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro; // Coroutine 사용을 위해 추가

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

    [Header("튜토리얼 패널")]
    [SerializeField]
    private GameObject TutorialPanel;

    // [수정] GameObject 대신 연출 로직을 담은 컴포넌트를 직접 참조합니다. (책임 분리)
    [Header("게임 오버 패널 컨트롤러")]
    [SerializeField]
    private GameOverPanelController gameOverPanelController;

    public bool isGameOver = false;

    public static event Action OnGameOver; // <-- 게임 오버 이벤트 추가

    [SerializeField] GameObject player;

    /// <summary>
    /// LoadingManager가 다음에 로드해야 할 최종 목적지 씬의 이름입니다.
    /// 정적 변수로 설정하여 어떤 씬에서도 접근할 수 있도록 합니다.
    /// </summary>
    public static string NextSceneToLoad = ""; // <-- 이 변수를 추가합니다.

    // === [추가된 요소: 페이드 인/아웃을 위한 변수] ===
    [Header("씬 전환 페이드 효과")]
    [Tooltip("씬 전환 시 화면을 덮을 검은색 Image 컴포넌트를 할당하세요. (LoadingScene의 Fade Panel과 동일한 것을 사용하거나 이 씬에 별도로 준비)")]
    [SerializeField] private Image fadePanel;

    [Tooltip("씬에서 다른 씬으로 넘어갈 때 (페이드 아웃) 걸리는 시간(초)입니다.")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Tooltip("이 씬에 진입했을 때 페이드 인이 필요한 경우 걸리는 시간(초)입니다. (현재 MainScene에서는 Start()에서 사용하지 않음)")]
    [SerializeField] private float fadeInDuration = 1.0f;
    // ============================================
    [Tooltip("게임 설정 패널을 활성화할 버튼입니다.")]
    public Button settingsButton;
    [Tooltip("게임 설정 내용을 담고 있는 패널입니다. (비활성화/활성화 토글용)")]
    public GameObject settingsPanel;
    [Tooltip("저장 경로를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    public TextMeshProUGUI saveText;
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

        TutorialPanel.SetActive(true);
    }

    private void Start()
    {
        // [추가] MainScene 진입 시 만약 LoadingScene에서 페이드 인이 처리되지 않았다면
        // (즉시 MainScene을 로드한 경우) 여기서 FadeFromBlack을 호출하여 화면을 드러냅니다.
        if (fadePanel != null)
        {
            // MainScene이 로드될 때, fadePanel이 씬에 있다면 초기 상태를 검은색으로 설정하고
            // FadeFromBlack을 호출할 수 있습니다. (LoadingManager의 Start()와 유사)
            Color initialColor = fadePanel.color;
            initialColor.a = 0f; // 기본적으로는 투명하게 시작
            fadePanel.color = initialColor;
            fadePanel.gameObject.SetActive(true);

            // 만약 MainScene이 LoadingScene을 거쳤다면 이미 투명할 것입니다.
            // 필요하다면, LoadingScene에서 넘어왔는지 확인하는 별도의 로직을 추가하여
            // 여기서는 FadeFromBlack을 호출하지 않도록 할 수 있습니다.
            // 하지만 지금은 LoadSceneWithFade만 사용하므로 이 부분은 씬 전환 시 부적절할 수 있어 주석처리합니다.
            // StartCoroutine(FadeFromBlack(fadeInDuration)); 
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.Main_A, 2.0f);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            settingsButton.onClick.AddListener(PlayButtonSFXSafely);
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }
        else
        {
            Debug.LogWarning("경고: '게임 설정' 버튼이 할당되지 않았습니다. 해당 기능은 작동하지 않습니다.");
        }
        if (saveText != null && SaveManager.Instance != null)
        {
            saveText.text = "저장 위치 \n" + SaveManager.Instance.saveFilePath;
        }
    }

    /// <summary>
    /// SoundManager 인스턴스 존재 여부를 확인하고 SFX를 안전하게 재생하는 헬퍼 메서드입니다.
    /// 버튼 OnClick() 이벤트에 연결되어 단일 책임 원칙을 보조합니다.
    /// </summary>
    public void PlayButtonSFXSafely()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonSFX();
        }
    }
    public void OnSettingsButtonClick()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("'게임 설정' 패널이 할당되지 않아 기능을 수행할 수 없습니다.");
        }
    }
    public void DeactivatePanel(GameObject targetPanel)
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }
    // (HandlePanelActivation, HandlePanelDeactivation 메서드는 변경 없이 유지)

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

    // === [추가 및 수정된 씬 전환 로직] ===

    /// <summary>
    /// 씬 전환 전에 페이드 아웃 효과를 실행하고, 완료되면 LoadingScene으로 전환합니다.
    /// </summary>
    /// <param name="targetSceneName">다음 로드할 씬의 이름입니다.</param>
    public void LoadSceneWithFade(string targetSceneName)
    {
        // 1. 최종 목적지 씬 이름 설정
        MainSceneManager.NextSceneToLoad = targetSceneName;

        // 2. 씬 전환 코루틴 시작
        StartCoroutine(CoLoadSceneWithFade());
    }

    /// <summary>
    /// Exit 버튼 클릭 시 TitleScene으로 전환합니다.
    /// </summary>
    public void Exit()
    {
        LoadSceneWithFade("TitleScene");
    }

    /// <summary>
    /// 게임 오버 후 Restart 버튼 클릭 시 MainScene으로 전환합니다.
    /// </summary>
    public void Restart()
    {
        // 1. **가장 먼저** isGameOver 상태를 재시작 상태(false)로 변경하여 
        //    DungeonManager가 보상 로직을 실행하지 못하게 막습니다.
        isGameOver = false;

        // 2. 저장 불러오기 (위치, 스탯 등 모든 게임 데이터 복구)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }

        // 3. 페이드와 함께 MainScene으로 전환
        LoadSceneWithFade("MainScene");
    }

    /// <summary>
    /// 화면을 검게 만든 후 LoadingScene으로 이동시키는 코루틴입니다.
    /// </summary>
    private IEnumerator CoLoadSceneWithFade()
    {
        // 1. 페이드 아웃 (화면을 검게 가림)
        yield return StartCoroutine(FadeToBlack(fadeOutDuration));

        // 2. 페이드가 완료된 후 LoadingScene으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
    }

    /// <summary>
    /// 씬 로드가 아닌, 순수하게 화면만 페이드 인/아웃시키는 공용 메서드입니다.
    /// (예: 던전 진입/퇴장 시 연출)
    /// </summary>
    /// <param name="fadeInDuration">페이드 인(검은색->투명) 시간입니다.</param>
    /// <param name="fadeOutDuration">페이드 아웃(투명->검은색) 시간입니다.</param>
    public void PerformScreenFade(float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
    {
        StartCoroutine(CoPerformScreenFade(fadeOutDuration, fadeInDuration));
    }

    /// <summary>
    /// 화면을 검게 가렸다가 다시 드러내는 연출 코루틴입니다.
    /// </summary>
    private IEnumerator CoPerformScreenFade(float fadeOutDuration, float fadeInDuration)
    {
        // 1. 화면 검게 가리기 (Fade Out)
        yield return StartCoroutine(FadeToBlack(fadeOutDuration));

        // 2. 화면이 검은 상태에서 잠시 대기 (원하는 경우 추가 가능)
        // yield return new WaitForSeconds(0.2f); 

        // 3. 화면 다시 드러내기 (Fade In)
        yield return StartCoroutine(FadeFromBlack(fadeInDuration));
    }


    /// <summary>
    /// 화면을 서서히 검은색에서 투명하게(알파값 0) 페이드 인시키는 코루틴입니다.
    /// (알파 1.0 -> 0.0)
    /// </summary>
    private IEnumerator FadeFromBlack(float duration)
    {
        if (fadePanel == null) yield break;

        float timer = 0f;
        Color color = fadePanel.color;

        // 현재 알파값부터 0.0까지 진행합니다.
        float startAlpha = color.a;
        float targetAlpha = 0.0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            color.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadePanel.color = color;

            yield return null;
        }

        // 정확히 0.0으로 설정하여 완료를 보장합니다.
        color.a = 0.0f;
        fadePanel.color = color;
    }


    /// <summary>
    /// 화면을 서서히 검은색으로(알파값 1) 페이드 아웃시키는 코루틴입니다.
    /// (알파 0.0 -> 1.0)
    /// </summary>
    private IEnumerator FadeToBlack(float duration)
    {
        if (fadePanel == null) yield break;

        float timer = 0f;
        Color color = fadePanel.color;

        // 현재 알파값부터 1.0까지 진행합니다.
        float startAlpha = color.a;
        float targetAlpha = 1.0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            color.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadePanel.color = color;

            yield return null;
        }

        // 정확히 1.0으로 설정하여 완료를 보장합니다.
        color.a = 1.0f;
        fadePanel.color = color;
    }
    public void PerformScreenFade()
    {
        StartCoroutine(CoPerformScreenFade(this.fadeOutDuration, this.fadeInDuration));
    }
    // === (기존 로직 유지) ===

    public void save()
    {
        // DungeonManager.Instance가 유효한지 확인하고 던전 상태를 체크합니다.
        if (DungeonManager.Instance != null && DungeonManager.Instance.IsInDungeon)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    "던전 내부에서는 저장할 수 없습니다.",
                    NotificationType.General
                );
            }
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogError("SaveManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// [추가된 기능] 외부(예: PlayerHealth)에서 게임 오버를 선언할 때 호출되는 메서드입니다.
    /// 씬 상태를 게임 오버로 전환하고, UI 연출을 해당 컨트롤러에 위임합니다.
    /// </summary>
    public void SetGameOver()
    {
        isGameOver = true;
        OnGameOver?.Invoke();
        // BGM 변경 및 게임 오버 처리
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.Main_D);
        }
        // 게임 오버 상태에 진입하면 PlayerCanvas를 비활성화하여
        // 플레이어의 상호작용을 막습니다.
        if (playerCanvas != null && playerCanvas.activeInHierarchy)
        {
            playerCanvas.SetActive(false);
        }

        // [수정] GameObject.SetActive(true) 대신, 컨트롤러의 연출 메서드를 호출하여
        // 패널 활성화와 시각적 효과 시작을 모두 컨트롤러에게 위임합니다. (책임 분리)
        if (gameOverPanelController != null)
        {
            gameOverPanelController.ShowPanelWithEffect();
        }
        else
        {
            Debug.LogError("GameOverPanelController가 MainSceneManager에 할당되지 않았습니다. UI 연출을 할 수 없습니다.");
        }

        // TODO: Time.timeScale = 0; 또는 게임 오버 패널 활성화 등의 추가 로직을 여기에 구현합니다.
    }
}