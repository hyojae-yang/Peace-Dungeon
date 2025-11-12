using TMPro;
using UnityEngine;

/// <summary>
/// 던전의 점수를 관리하는 매니저 클래스입니다.
/// 이 클래스는 오직 '점수 누적 및 반환'의 단일 책임(SRP)만을 수행하며, 
/// 몬스터 객체의 null 여부를 추측하는 불안정한 로직을 제거합니다.
/// </summary>
public class DungeonScoreManager : MonoBehaviour
{
    public static DungeonScoreManager Instance { get; private set; }

    // 대신, 몬스터가 처치될 때마다 점수를 누적할 변수를 사용합니다.
    private int currentDungeonScore = 0;

    /// <summary>
    /// 위험도 레벨 1 증가 시 점수에 추가되는 보너스 비율입니다. (0.10f = 10%)
    /// </summary>
    private const float RISK_BONUS_PER_LEVEL = 0.10f;

    [Tooltip("던전 입장 시 활성화될 점수 패널입니다.")]
    [SerializeField] private GameObject scorePanel;
    [Tooltip("알림창에 표시될 점수 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        scorePanel.SetActive(false);
        // 참고: totalScore는 던전마다 초기화되어야 하므로 Awake에서는 초기화하지 않습니다.
    }

    private void Update()
    {
        // DungeonManager가 null이 아닐 때만 IsInDungeon을 체크
        if (DungeonManager.Instance != null && DungeonManager.Instance.IsInDungeon)
        {
            scorePanel.SetActive(true);
            scoreText.text = $"{currentDungeonScore}";
        }
        else
        {
            scorePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 던전 진입 시 DungeonManager에 의해 호출되어 점수 누적 시스템을 0으로 초기화합니다.
    /// </summary>
    public void ResetScore()
    {
        currentDungeonScore = 0;
        // Debug.Log("[ScoreManager:Reset] 점수 누적 시스템 초기화 완료. 현재 점수: 0");
    }

    /// <summary>
    /// 이 메서드는 점수 누적의 단일 책임을 가집니다. (Monster.cs에서 호출됩니다.)
    /// 현재 던전 위험도 레벨에 따라 보너스 점수를 적용합니다.
    /// </summary>
    /// <param name="baseScore">몬스터로부터 획득한 기본 점수</param>
    public void AddScore(int baseScore)
    {
        // 몬스터 처치로 점수를 얻었으므로, 총 처치 횟수를 증가시킵니다.
        if (KillCountManager.Instance != null)
        {
            KillCountManager.Instance.AddKillCount();
        }

        int riskLevel = 0;

        // [핵심 수정] DungeonRiskManager에서 레벨을 가져옵니다.
        if (DungeonRiskManager.Instance != null)
        {
            // 1. DungeonRiskManager로부터 현재 위험도 레벨을 안전하게 가져옵니다.
            riskLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();
        }
        // else: 위험도 매니저가 없으면 riskLevel은 0이 됩니다. (보너스 없음)

        // 2. 위험도 보너스 승수 계산
        // 승수 = 1.0 + (레벨 * 0.1)
        float bonusMultiplier = 1.0f + (riskLevel * RISK_BONUS_PER_LEVEL);

        // 3. 보너스가 적용된 최종 점수 계산 (정수형으로 변환 시 소수점 버림)
        int finalScore = Mathf.FloorToInt(baseScore * bonusMultiplier);

        // 4. 점수 누적
        currentDungeonScore += finalScore;
        // Debug.Log($"[ScoreManager:Add] 기본 점수 {baseScore}, 위험도 {riskLevel} 적용! 최종 점수 {finalScore} 획득! 누적 점수: {currentDungeonScore}");
    }

    /// <summary>
    /// 던전에서 나갈 때 DungeonManager에 의해 호출되어 최종 점수를 계산하고 반환합니다.
    /// </summary>
    /// <returns>최종 점수</returns>
    public int CalculateFinalScore()
    {
        // 1. 현재 누적된 점수를 최종 점수로 확정합니다.
        int finalScore = currentDungeonScore;

        // 2. 다음 던전 진입을 위해 점수를 즉시 초기화합니다.
        ResetScore();

        // Debug.Log($"[ScoreManager:Calculate] 최종 던전 점수: {finalScore}점 반환.");
        return finalScore;
    }
}