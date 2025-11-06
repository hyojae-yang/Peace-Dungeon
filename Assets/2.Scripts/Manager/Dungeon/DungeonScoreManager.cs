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

    [Tooltip("던전 입장 시 활성화될 점수 패널입니다.")]
    [SerializeField] private GameObject scorePanel;
    [Tooltip("알림창에 표시될 점수 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 만약 씬 전환에도 유지되어야 한다면 이 주석을 해제합니다.
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
        if(DungeonManager.Instance.IsInDungeon)
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
    /// </summary>
    /// <param name="score">획득한 점수</param>
    public void AddScore(int score)
    {
        currentDungeonScore += score;
        // Debug.Log($"[ScoreManager:Add] 점수 {score} 획득! 누적 점수: {currentDungeonScore}");
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

        // Debug.Log($"[ScoreManager:Calculate] 🎉 최종 던전 점수: {finalScore}점 반환.");
        return finalScore;
    }
}
