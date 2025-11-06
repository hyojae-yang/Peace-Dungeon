using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro 사용을 위한 네임스페이스

/// <summary>
/// LoadingScene에서 다음 씬을 비동기로 로드하고 로딩 진행률 및 게임 팁을 표시합니다.
/// 로드 완료 후에는 사용자 입력(아무 키)을 기다린 후 다음 씬으로 전환하는 역할을 담당합니다.
/// SRP: 씬 로드 및 로딩 UI 관리라는 단일 책임을 가집니다.
/// </summary>
public class LoadingManager : MonoBehaviour
{
    // 로드할 최종 씬 이름을 MainSceneManager의 정적 변수에서 가져옵니다.
    private const string DefaultTargetScene = "MainScene";

    [Header("UI 요소")]
    [Tooltip("로딩 진행률을 표시할 슬라이더 컴포넌트입니다.")]
    [SerializeField] private Slider loadingProgressBar;

    [Tooltip("게임 팁을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI tipText;

    [Tooltip("로딩 완료 후 사용자 입력을 요청할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI loadingCompleteText; // [추가] 완료 텍스트

    [Header("게임 팁 목록")]
    [Tooltip("로딩 화면에 표시할 게임 팁 문구들입니다.")]
    [SerializeField]
    private List<string> gameTips = new List<string>
    {
        "보스가 사망하면 던전 클리어 상태가 됩니다.",
        "보스룸에서 플레이어가 사망하면 보상은 지급되지 않습니다."
    };

    [Header("로딩 시간 제약")]
    [Tooltip("로딩 씬이 최소한 이 시간(초)만큼 화면에 표시되도록 강제합니다.")]
    [SerializeField] private float minDisplayTime = 3.0f;

    [Header("깜빡임 설정")]
    [Tooltip("완료 텍스트의 페이드 인/아웃 속도(초당)입니다. 값이 낮을수록 느립니다.")]
    [SerializeField] private float blinkSpeed = 1.0f; // 깜빡임 속도 (1초에 한 번 페이드 인/아웃)

    // 깜빡임 코루틴을 제어하기 위한 변수
    private Coroutine _blinkCoroutine;

    private void Start()
    {
        // 사운드 매니저가 있다면 BGM을 재생합니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.Loading, 1.0f);
        }

        // UI 유효성 검사
        if (loadingProgressBar == null || tipText == null || loadingCompleteText == null)
        {
            Debug.LogError("로딩 UI 요소(Slider, Tip Text, Complete Text 중 하나)가 할당되지 않았습니다! 로딩이 정상 작동하지 않을 수 있습니다.");
            return;
        }

        // 초기 상태 설정: 완료 텍스트는 숨깁니다.
        loadingCompleteText.gameObject.SetActive(false);

        // 팁 목록 중 하나를 무작위로 선택하여 표시합니다.
        ShowRandomTip();

        // [핵심 로직] 로드할 최종 씬 이름을 동적으로 결정합니다.
        string targetSceneName = MainSceneManager.NextSceneToLoad;

        // 로드할 씬 이름이 없다면 기본 씬으로 설정합니다.
        if (string.IsNullOrEmpty(targetSceneName))
        {
            targetSceneName = DefaultTargetScene;
        }

        // 다음 씬 정보를 초기화합니다.
        MainSceneManager.NextSceneToLoad = "";

        // 비동기 씬 로드를 시작합니다.
        StartCoroutine(LoadNextSceneAsync(targetSceneName));
    }

    /// <summary>
    /// 로딩 바에 표시할 무작위 게임 팁을 선택하여 표시합니다.
    /// </summary>
    private void ShowRandomTip()
    {
        if (gameTips.Count > 0)
        {
            int randomIndex = Random.Range(0, gameTips.Count);
            // TextMeshProUGUI의 텍스트를 설정합니다.
            tipText.text = "팁: " + gameTips[randomIndex];
        }
        else
        {
            tipText.text = "잠시만 기다려주세요...";
        }
    }

    /// <summary>
    /// 다음 씬을 백그라운드에서 비동기로 로드하는 코루틴입니다.
    /// 로딩 완료 후 사용자 입력을 기다립니다.
    /// </summary>
    /// <param name="targetSceneName">로드할 씬의 이름입니다.</param>
    private IEnumerator LoadNextSceneAsync(string targetSceneName)
    {
        // 씬 로드 요청을 시작하고 씬 활성화를 수동으로 제어합니다.
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
        asyncOperation.allowSceneActivation = false;

        float currentDisplayTime = 0f; // 최소 표시 시간 체크용 타이머
        float loadingTimer = 0f; // 로딩 게이지 보간을 위한 타이머

        // 로딩이 완료될 때까지 반복합니다.
        while (!asyncOperation.isDone)
        {
            // 실제 로딩 진행률 (유니티는 0.9에서 멈춥니다)
            float realProgress = asyncOperation.progress;

            // 유니티 0.9 멈춤 현상을 1.0처럼 보이게 보정합니다.
            if (realProgress >= 0.9f)
            {
                realProgress = 1.0f;
            }

            // 최소 표시 시간 타이머 업데이트
            currentDisplayTime += Time.deltaTime;

            // 부드러운 로딩 게이지 업데이트
            loadingTimer += Time.deltaTime;
            // Lerp를 사용하여 부드럽게 증가시키되, 실제 로딩 진행률을 넘어가지 않도록 Min으로 제한합니다.
            loadingProgressBar.value = Mathf.Min(Mathf.Lerp(loadingProgressBar.value, realProgress, loadingTimer), realProgress);

            // 로딩이 완료되었고 (0.9 이상), UI 게이지도 거의 다 채워졌으며,
            // 최소 표시 시간도 경과했을 때 다음 단계로 넘어갑니다.
            if (asyncOperation.progress >= 0.9f &&
                loadingProgressBar.value >= 0.99f &&
                currentDisplayTime >= minDisplayTime)
            {
                // [추가된 로직] 씬 활성화 대신 사용자 입력을 기다리는 상태로 전환합니다.

                // 완료 텍스트를 활성화하고 깜빡이기 시작합니다.
                loadingCompleteText.gameObject.SetActive(true);
                StartBlinkingText();

                // 사용자 입력을 기다립니다. AnyKey는 마우스 클릭, 터치, 키보드 입력 등 모든 입력을 포함합니다.
                yield return new WaitUntil(() => Input.anyKey);

                // 깜빡임 코루틴을 중지합니다.
                if (_blinkCoroutine != null)
                {
                    StopCoroutine(_blinkCoroutine);
                    _blinkCoroutine = null;
                }
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Button_Click, 0.5f);
                }
                // 사용자 입력을 받았으니 씬 활성화를 허용하고 코루틴을 종료합니다.
                asyncOperation.allowSceneActivation = true;
                break; // while 루프를 빠져나갑니다.
            }

            yield return null; // 한 프레임을 기다립니다.
        }
    }

    /// <summary>
    /// 로딩 완료 텍스트의 깜빡임 코루틴을 시작합니다.
    /// </summary>
    private void StartBlinkingText()
    {
        // 기존 코루틴이 있다면 중복 실행을 막기 위해 중지합니다.
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        // 새로운 깜빡임 코루틴을 시작하고 변수에 저장합니다.
        _blinkCoroutine = StartCoroutine(BlinkTextCoroutine());
    }

    /// <summary>
    /// 로딩 완료 텍스트의 투명도를 주기적으로 페이드 인/아웃하여 깜빡이는 효과를 줍니다.
    /// DIP: 구체적인 UI 컴포넌트(TextMeshProUGUI)에 의존하는 대신, 
    /// 추상화된 UI 제어에 집중하는 코루틴을 통해 의존성 역전 원칙(DIP)에 가깝게 구현을 분리합니다.
    /// </summary>
    private IEnumerator BlinkTextCoroutine()
    {
        // 텍스트의 투명도를 제어하기 위한 변수
        Color currentColor = loadingCompleteText.color;
        // 깜빡이는 방향 (true: 투명해짐(1->0), false: 불투명해짐(0->1))
        bool isFadingOut = true;

        while (true)
        {
            // 시간에 따라 알파값을 조절할 보간 값 계산
            float targetAlpha = isFadingOut ? 0f : 1f;

            // Lerp를 사용하여 현재 알파값에서 목표 알파값으로 부드럽게 이동합니다.
            currentColor.a = Mathf.MoveTowards(currentColor.a, targetAlpha, Time.deltaTime * blinkSpeed);
            loadingCompleteText.color = currentColor;

            // 목표 알파값에 도달했다면 방향을 전환합니다.
            if (currentColor.a == targetAlpha)
            {
                isFadingOut = !isFadingOut; // 방향 반전
            }

            yield return null; // 한 프레임 대기
        }
    }
}