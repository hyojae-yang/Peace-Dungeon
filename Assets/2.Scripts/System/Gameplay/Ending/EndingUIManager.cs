using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// EndingUIManager 클래스는 엔딩 크레딧 화면의 UI 요소를 관리하는 싱글톤입니다.
/// </summary>
public class EndingUIManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    private static EndingUIManager _instance;

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
    [Tooltip("엔딩 크레딧 전체를 감싸는 부모 패널 (GameObject)을 할롱하세요.")]
    [SerializeField]
    private GameObject endingPanel;

    [Tooltip("엔딩 통계를 닫고 시퀀스를 종료하는 버튼을 할당하세요.")]
    [SerializeField]
    private Button endingConfirmButton;

    [Tooltip("크레딧 스크롤을 제어할 ScrollRect 컴포넌트를 할당하세요.")]
    [SerializeField]
    private ScrollRect creditScrollRect;

    // [수정] 속도 관련 필드의 주석 수정 및 값 변경
    [Tooltip("스크롤이 시작되기 전 대기 시간 (초)을 설정하세요. (3초 ~ 5초 권장)")]
    [SerializeField]
    private float scrollDelayTime = 4f;

    [Tooltip("크레딧 자동 스크롤 속도 (일정 속도 유지)입니다. (0.05f ~ 0.1f 사이 권장)")]
    [SerializeField]
    private float scrollSpeed = 0.05f;

    // 통계 텍스트 필드 (Content 자식으로 배치)
    [Tooltip("총 플레이 시간을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    [SerializeField]
    private TextMeshProUGUI playtimeText;

    [Tooltip("총 사망 횟수를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    [SerializeField]
    private TextMeshProUGUI deathCountText;

    [Tooltip("총 몬스터 처치 마릿수를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    [SerializeField]
    private TextMeshProUGUI killCountText;

    // 자동 스크롤 코루틴 제어 변수
    private Coroutine autoScrollCoroutine;

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

        if (endingConfirmButton != null)
        {
            endingConfirmButton.onClick.RemoveAllListeners();
            endingConfirmButton.onClick.AddListener(HideEndingScreen);
        }
        else
        {
            Debug.LogError("[EndingUIManager] 엔딩 확인 버튼이 할당되지 않았습니다.");
        }

        if (creditScrollRect == null)
        {
            Debug.LogError("[EndingUIManager] Scroll Rect가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
        }
    }

    public void ShowEndingScreen()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[EndingUIManager] Ending Panel이 할당되지 않았습니다.");
            return;
        }

        DisplayPlayTime();
        DisplayDeathCount();
        DisplayKillCount(); // [수정] 확장된 버전 호출

        InitializeScrollPositionAndStartSequence();

        // 엔딩 패널이 켜지는 순간 버튼 비활성화 (스크롤 연출 전까지)
        if (endingConfirmButton != null)
        {
            endingConfirmButton.interactable = false;
        }
    }

    /// <summary>
    /// 스크롤 뷰를 초기 위치 (가장 위)로 설정하고 자동 스크롤 시퀀스를 시작합니다.
    /// </summary>
    private void InitializeScrollPositionAndStartSequence()
    {
        if (creditScrollRect == null) return;

        // Content를 가장 위로 이동 (verticalNormalizedPosition 1.0)하여 위에서 시작하도록 설정
        creditScrollRect.verticalNormalizedPosition = 1.0f;

        // 자동 스크롤이 진행되는 동안 플레이어의 수동 스크롤 조작을 막습니다.
        creditScrollRect.enabled = false;

        // 기존 코루틴을 정지하고 새로운 코루틴 시작
        if (autoScrollCoroutine != null) StopCoroutine(autoScrollCoroutine);
        autoScrollCoroutine = StartCoroutine(StartDelayedAutoScroll());
    }

    /// <summary>
    /// 지정된 지연 시간 후 자동 스크롤을 시작하는 코루틴입니다.
    /// </summary>
    private IEnumerator StartDelayedAutoScroll()
    {
        // 통계 데이터를 읽을 수 있도록 설정된 시간만큼 대기합니다.
        yield return new WaitForSeconds(scrollDelayTime);

        // 지연 후 자동 스크롤을 실행합니다.
        yield return StartCoroutine(AutoScrollCoroutine());
    }

    /// <summary>
    /// ScrollRect의 verticalNormalizedPosition을 1.0f에서 0.0f (가장 아래)까지 일정한 속도로 내리는 코루틴입니다.
    /// </summary>
    private IEnumerator AutoScrollCoroutine()
    {
        // 스크롤 목표 위치 (0.0f, 콘텐츠의 마지막 부분이 뷰포트에 걸리는 지점)
        const float targetPos = 0.0f;

        // 스크롤이 목표 위치에 도달할 때까지 반복
        while (creditScrollRect.verticalNormalizedPosition > targetPos)
        {
            // 일정한 속도를 위해 직접 값을 뺍니다. (위에서 아래 방향)
            creditScrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;

            // 목표치보다 값이 작아지면 (즉, 목표를 지나치면) 루프 종료
            if (creditScrollRect.verticalNormalizedPosition <= targetPos)
            {
                creditScrollRect.verticalNormalizedPosition = targetPos;
                break;
            }

            yield return null;
        }

        Debug.Log("자동 크레딧 스크롤이 완료되었습니다.");

        // 스크롤이 끝났으므로 버튼과 플레이어 스크롤 조작을 활성화합니다.
        if (endingConfirmButton != null)
        {
            endingConfirmButton.interactable = true;
        }
        creditScrollRect.enabled = true;
    }

    public void HideEndingScreen()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = null;
        }

        // 비활성화된 경우를 대비하여 버튼과 스크롤 조작을 확실히 활성화합니다.
        if (endingConfirmButton != null)
        {
            endingConfirmButton.interactable = true;
        }
        if (creditScrollRect != null)
        {
            creditScrollRect.enabled = true;
        }
    }

    // === 데이터 바인딩 메서드 ===

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
    /// [수정] KillCountManager로부터 총 처치 횟수와 종류별 처치 기록을 가져와 표시합니다.
    /// MonsterDataManager(가정)을 통해 ID 대신 이름을 조회하여 표시합니다.
    /// </summary>
    private void DisplayKillCount()
    {
        if (killCountText == null) return;

        if (KillCountManager.Instance != null)
        {
            var killManager = KillCountManager.Instance;
            int totalKills = killManager.TotalKills;

            // StringBuilder를 사용하여 효율적으로 문자열을 구성합니다.
            StringBuilder sb = new StringBuilder();

            // 1. 총합 표시
            sb.AppendLine($"어둠을 몰아낸 총 횟수: {totalKills} 마리\n");

            // 2. 종류별 기록 표시
            IReadOnlyDictionary<int, int> typeKills = killManager.TypeKills;

            // 딕셔너리가 비어있지 않은 경우에만 상세 정보를 추가합니다.
            if (typeKills.Count > 0)
            {
                sb.AppendLine("--- 종류별 처치 상세 ---");

                // 처치 횟수가 높은 순으로 정렬하여 표시하면 보기 좋습니다.
                foreach (var pair in typeKills)
                {
                    string monsterName;

                    // [핵심 수정] MonsterDataManager가 존재하면 이름을 조회합니다.
                    if (MonsterDataManager.Instance != null)
                    {
                        monsterName = MonsterDataManager.Instance.GetMonsterName(pair.Key);
                    }
                    else
                    {
                        // MonsterDataManager가 없으면 임시로 ID를 표시합니다.
                        monsterName = $"[ID:{pair.Key:D3} - 이름 정보 없음]";
                    }

                    // 이름과 횟수를 표시합니다.
                    sb.AppendLine($"- {monsterName}: {pair.Value} 마리");
                }
            }

            killCountText.text = sb.ToString();
        }
        else
        {
            killCountText.text = "어둠을 몰아낸 횟수: 오류 (매니저 없음)";
        }
    }
}