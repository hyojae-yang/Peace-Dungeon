using UnityEngine;
using System.Collections.Generic;

public class DungeonScoreManager : MonoBehaviour
{
    public static DungeonScoreManager Instance { get; private set; }

    private int totalScore = 0;

    private Dictionary<GameObject, int> monsterScores;

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
    }

    /// <summary>
    /// DungeonSpawnManager로부터 몬스터-점수 데이터를 초기화하는 메서드입니다.
    /// </summary>
    /// <param name="scores">몬스터 게임오브젝트와 점수를 담은 딕셔너리</param>
    public void InitializeScores(Dictionary<GameObject, int> scores)
    {
        monsterScores = scores;
        totalScore = 0;
        Debug.Log("DungeonScoreManager가 몬스터 점수 데이터를 초기화했습니다.");
    }

    // DungeonScoreManager.cs (CalculateFinalScore 메서드만 수정)
    // ... (Awake, InitializeScores 메서드 생략)

    /// <summary>
    /// 던전에서 나갈 때 호출되어 최종 점수를 계산하고 반환합니다. (진단용 로직 포함)
    /// </summary>
    /// <returns>최종 점수</returns>
    public int CalculateFinalScore()
    {
        if (monsterScores == null)
        {
            Debug.LogWarning("점수 계산을 위한 몬스터 딕셔너리가 초기화되지 않았습니다.");
            return 0;
        }
        // 파괴된 몬스터들을 점수 계산하고 딕셔너리에서 제거합니다.
        foreach (var pair in monsterScores)
        {
            bool isUnityNull = pair.Key == null;
            // C#의 엄격한 null 비교 (is를 사용하여 딕셔너리 내부의 엄격한 null 참조 검사)
            bool isStrictlyNull = pair.Key is null;
            // [기존 로직]
            if (pair.Key == null)
            {
                totalScore += pair.Value;
            }
            else
            {
                Debug.Log($"-> **점수 합산 실패!** (객체가 Null로 인식되지 않음)");
            }
        }
        // 다음 던전을 위해 딕셔너리를 비웁니다.
        monsterScores.Clear();

        return totalScore;
    }
}