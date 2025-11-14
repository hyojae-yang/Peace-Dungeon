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

    // 몬스터 처치로 점수를 누적할 변수입니다.
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
    }

    /// <summary>
    /// [수정] 이 메서드는 점수 누적과 몬스터 처치 기록 전달의 단일 책임을 가집니다. (Monster.cs에서 호출됩니다.)
    /// 현재 던전 위험도 레벨에 따라 보너스 점수를 적용합니다.
    /// </summary>
    /// <param name="baseScore">몬스터로부터 획득한 기본 점수</param>
    /// <param name="monsterID">처치된 몬스터의 고유 ID (KillCountManager로 전달)</param>
    public void AddScore(int baseScore, int monsterID)
    {
        // 1. [핵심 수정] 몬스터 처치로 점수를 얻었으므로, 몬스터 ID와 함께 처치 횟수를 증가시킵니다.
        if (KillCountManager.Instance != null)
        {
            KillCountManager.Instance.AddKillCount(monsterID);
        }

        int riskLevel = 0;

        if (DungeonRiskManager.Instance != null)
        {
            // 2. DungeonRiskManager로부터 현재 위험도 레벨을 안전하게 가져옵니다.
            riskLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();
        }

        // 3. 위험도 보너스 승수 계산
        float bonusMultiplier = 1.0f + (riskLevel * RISK_BONUS_PER_LEVEL);

        // 4. 보너스가 적용된 최종 점수 계산 (정수형으로 변환 시 소수점 버림)
        int finalScore = Mathf.FloorToInt(baseScore * bonusMultiplier);

        // 5. 점수 누적
        currentDungeonScore += finalScore;
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

        return finalScore;
    }
}