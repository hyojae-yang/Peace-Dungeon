using UnityEngine;
using TMPro; // TextMeshProUGUI를 사용하기 위해 필요합니다.
using UnityEngine.UI; // Button 컴포넌트를 사용하기 위해 필요합니다.

/// <summary>
/// EndingUIManager 클래스는 엔딩 크레딧 화면의 UI 요소를 관리하는 싱글톤입니다.
/// 단일 책임 원칙(SRP)에 따라 데이터 바인딩 및 UI 표시만을 담당합니다.
/// </summary>
public class EndingUIManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    private static EndingUIManager _instance;

    /// <summary>
    /// 싱글톤 인스턴스 접근 프로퍼티입니다.
    /// </summary>
    public static EndingUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EndingUIManager>();

                if (_instance == null)
                {
                    Debug.LogError("EndingUIManager 인스턴스가 씬에 없습니다. 게임 오브젝트에 컴포넌트를 추가해야 합니다.");
                }
            }
            return _instance;
        }
    }

    // === UI 필드 ===
    [Header("UI References")]
    [Tooltip("엔딩 크레딧 전체를 감싸는 부모 패널 (GameObject)을 할당하세요.")]
    [SerializeField]
    private GameObject endingPanel;

    // 엔딩 화면 종료 버튼
    [Tooltip("엔딩 통계를 닫고 시퀀스를 종료하는 버튼을 할당하세요.")]
    [SerializeField]
    private Button endingConfirmButton;

    [Tooltip("총 플레이 시간을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    [SerializeField]
    private TextMeshProUGUI playtimeText;

    [Tooltip("총 사망 횟수를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    [SerializeField]
    private TextMeshProUGUI deathCountText;

    [Tooltip("총 몬스터 처치 마릿수를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    [SerializeField]
    private TextMeshProUGUI killCountText;

    /// <summary>
    /// 싱글톤 무결성 및 버튼 리스너 연결을 담당합니다.
    /// SOLID 규칙: OCP(개방-폐쇄 원칙)에 따라 리스너를 한 곳에 모아 관리하여 코드 변경을 최소화합니다.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        // 버튼의 onClick 이벤트에 HideEndingScreen 메서드를 자동으로 연결합니다.
        if (endingConfirmButton != null)
        {
            // 리스너가 중복으로 추가되는 것을 방지하기 위해 이전에 추가된 리스너를 제거합니다.
            endingConfirmButton.onClick.RemoveAllListeners();
            // 현재 스크립트의 HideEndingScreen() 메서드를 버튼 클릭 시 호출하도록 등록합니다.
            endingConfirmButton.onClick.AddListener(HideEndingScreen);
        }
        else
        {
            Debug.LogError("[EndingUIManager] 엔딩 확인 버튼이 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
        }
    }

    /// <summary>
    /// 엔딩 패널을 활성화하고 필요한 데이터를 UI에 바인딩합니다.
    /// EndingManager의 명령을 받아 실행됩니다.
    /// SOLID: SRP (데이터 표시 및 UI 활성화 책임)
    /// </summary>
    public void ShowEndingScreen()
    {
        // 1. 패널 활성화
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[EndingUIManager] Ending Panel이 할당되지 않았습니다.");
            return;
        }

        // 2. 통계 데이터 바인딩
        DisplayPlayTime();
        DisplayDeathCount();
        DisplayKillCount();
    }

    /// <summary>
    /// 엔딩 통계 화면을 비활성화하고, 필요하다면 EndingManager에 종료 신호를 보냅니다.
    /// 이 메서드는 UI의 버튼(확인/나가기 등) 이벤트에 자동으로 연결됩니다.
    /// </summary>
    public void HideEndingScreen()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        // 추가 작업: 시간 스케일 복원, 씬 전환 등의 로직이 EndingManager에서 실행되어야 한다면
        // 여기서 EndingManager.Instance.EndSequence() 등을 호출하여 제어를 넘길 수 있습니다.
    }

    /// <summary>
    /// PlaytimeManager로부터 데이터를 가져와 UI 텍스트에 표시합니다.
    /// </summary>
    private void DisplayPlayTime()
    {
        if (playtimeText == null) return;

        if (PlaytimeManager.Instance != null)
        {
            string formattedTime = PlaytimeManager.Instance.GetFormattedPlayTime();
            playtimeText.text = "모험에 투자한 시간: " + formattedTime;
        }
        else
        {
            playtimeText.text = "모험에 투자한 시간: 오류 (매니저 없음)";
        }
    }

    /// <summary>
    /// DeathCountManager로부터 데이터를 가져와 UI 텍스트에 표시합니다.
    /// </summary>
    private void DisplayDeathCount()
    {
        if (deathCountText == null) return;

        if (DeathCountManager.Instance != null)
        {
            int deaths = DeathCountManager.Instance.TotalDeaths;
            deathCountText.text = $"다시 일어선 횟수: {deaths} 회";
        }
        else
        {
            deathCountText.text = "다시 일어선 횟수: 오류 (매니저 없음)";
        }
    }

    /// <summary>
    /// KillCountManager로부터 데이터를 가져와 UI 텍스트에 표시합니다.
    /// </summary>
    private void DisplayKillCount()
    {
        if (killCountText == null) return;

        if (KillCountManager.Instance != null)
        {
            int kills = KillCountManager.Instance.TotalKills;
            killCountText.text = $"어둠을 몰아낸 횟수: {kills} 마리";
        }
        else
        {
            killCountText.text = "어둠을 몰아낸 횟수: 오류 (매니저 없음)";
        }
    }
}