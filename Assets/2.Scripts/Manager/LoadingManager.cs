using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // [수정 1] TextMeshPro 사용을 위한 네임스페이스 추가

/// <summary>
/// LoadingScene에서 다음 씬을 비동기로 로드하고 로딩 진행률 및 게임 팁을 표시하는 역할을 담당합니다.
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
    [SerializeField] private TextMeshProUGUI tipText; // [수정 2] Text 대신 TextMeshProUGUI 사용

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
    [SerializeField] private float minDisplayTime = 3.0f; // [추가] 3초를 기본값으로 설정
    private void Start()
    {
        // UI 유효성 검사 (TextMeshProUGUI로 변경)
        if (loadingProgressBar == null || tipText == null)
        {
            Debug.LogError("로딩 UI 요소(Slider 또는 TextMeshProUGUI)가 할당되지 않았습니다! 로딩이 정상 작동하지 않을 수 있습니다.");
            return;
        }

        // 팁 목록 중 하나를 무작위로 선택하여 표시합니다.
        ShowRandomTip();

        // [핵심 로직] 로드할 최종 씬 이름을 동적으로 결정합니다.
        string targetSceneName = MainSceneManager.NextSceneToLoad;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            targetSceneName = DefaultTargetScene;
        }

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
            // [수정 3] Text 컴포넌트와 동일하게 .text 프로퍼티를 사용합니다.
            tipText.text = "팁: " + gameTips[randomIndex];
        }
        else
        {
            tipText.text = "잠시만 기다려주세요...";
        }
    }

    /// <summary>
    /// 다음 씬을 백그라운드에서 비동기로 로드하는 코루틴입니다.
    /// 로딩 진행률에 따라 UI를 업데이트하고, 로드가 완료되면 씬을 전환합니다.
    /// </summary>
    /// <param name="targetSceneName">로드할 씬의 이름입니다.</param>
    private IEnumerator LoadNextSceneAsync(string targetSceneName)
    {
        // 씬 로드 요청을 시작합니다.
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
        asyncOperation.allowSceneActivation = false; // 씬 활성화를 수동으로 제어하여 버벅거림을 분산합니다.

        float timer = 0f; // 로딩 게이지 보간을 위한 타이머

        // 로딩이 완료될 때까지 반복합니다.
        while (!asyncOperation.isDone)
        {
            // 실제 로딩 진행률 (0.9에서 멈춤)
            float realProgress = asyncOperation.progress;

            // 유니티가 90%에서 멈추는 것을 100%처럼 보이게 보정합니다.
            if (realProgress >= 0.9f)
            {
                realProgress = 1.0f;
            }

            // 부드러운 로딩 게이지 업데이트
            timer += Time.deltaTime;
            loadingProgressBar.value = Mathf.Min(Mathf.Lerp(loadingProgressBar.value, realProgress, timer), realProgress);

            // 로딩이 완료되었고 (0.9 이상), UI 게이지도 거의 다 채워졌으며,
            // [수정] 최소 표시 시간도 경과했을 때 씬을 활성화합니다.
            if (asyncOperation.progress >= 0.9f &&
                loadingProgressBar.value >= 0.99f &&
                timer >= minDisplayTime) // [추가된 조건]
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null; // 한 프레임을 기다려 메인 스레드의 부하를 분산합니다.
        }
    }
}