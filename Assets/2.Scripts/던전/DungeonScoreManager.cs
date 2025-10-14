using UnityEngine;
using System.Collections.Generic;

public class DungeonScoreManager : MonoBehaviour
{
    public static DungeonScoreManager Instance { get; private set; }

    // 💡 변경 1: totalScore 변수를 제거하거나, 아니면 최소한 여기서 사용하지 않도록 합니다. 
    // 현재 구조에서는 매 던전마다 점수를 새로 계산하므로 이 전역 변수가 필요 없습니다.
    // 만약 전체 게임의 누적 점수가 필요하다면, 이 변수를 유지하되, CalculateFinalScore()에서 이 변수를 건드리지 않도록 해야 합니다.
    // 여기서는 '던전 최종 점수' 계산 역할에 맞게 totalScore를 제거하거나 사용하지 않겠습니다.
    //private int totalScore = 0; // 이 줄을 제거하거나 주석 처리합니다.

    // 만약 totalScore 변수를 살려야 한다면 (예: 전체 게임 점수 누적용), 아래 CalculateFinalScore에서 이 변수를 사용하지 않도록 수정합니다.
    // 안전을 위해, 아래 코드에서는 totalScore는 그대로 두고, CalculateFinalScore 내부에서만 지역 변수를 사용하도록 하겠습니다.
    // (다만, 이 경우 totalScore는 초기화되지 않아 계속 누적되는 잠재적 위험이 남아있습니다. 이 변수를 제거하는 것을 더 강력히 권장합니다.)

    private Dictionary<GameObject, int> monsterScores;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 💡 추가: totalScore 변수를 사용한다면, Awake에서 명시적으로 0으로 초기화
           // totalScore = 0; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// DungeonSpawnManager로부터 몬스터-점수 데이터를 **누적하여** 초기화하는 메서드입니다.
    /// (여러 스폰 매니저의 데이터를 안전하게 합치기 위해 로직을 변경합니다.)
    /// </summary>
    /// <param name="scores">몬스터 게임오브젝트와 점수를 담은 딕셔너리</param>
    public void InitializeScores(Dictionary<GameObject, int> scores)
    {
        // 변경: 기존 monsterScores가 null이면 새로 생성 (이전에 Clear() 되었다면 이미 초기화되었을 것임)
        if (monsterScores == null)
        {
            monsterScores = new Dictionary<GameObject, int>();
        }

        // 변경: 전달받은 모든 몬스터-점수 쌍을 기존 딕셔너리에 추가합니다.
        foreach (var pair in scores)
        {
            if (pair.Key != null)
            {
                // 키 중복 체크: Add는 이미 키가 존재하면 예외를 발생시키므로 TryAdd로 안전하게 변경하거나,
                // 스폰 매니저가 유니크한 몬스터 객체만 넘긴다는 가정을 유지합니다. (현재 로직 유지)
                monsterScores.Add(pair.Key, pair.Value);
            }
        }

        // totalScore = 0; // 이 위치에서 초기화하면 InitializeScores를 여러 번 호출할 때마다 누적된 점수가 0으로 초기화됩니다. (잘못된 위치)
    }

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

        // 핵심 수정 1: 던전별 최종 점수를 계산할 지역 변수 (currentDungeonScore)를 선언하고 0으로 초기화합니다.
        int currentDungeonScore = 0;

        // 파괴된 몬스터들을 점수 계산하고 딕셔너리에서 제거합니다.
        // 참고: Dictionary를 순회하면서 원소를 제거하는 것은 일반적으로 안전하지 않습니다.
        // 여기서는 제거를 하는 대신, 파괴된 몬스터의 점수만 계산합니다. (Clear()가 있으므로)
        foreach (var pair in monsterScores)
        {
            // bool isUnityNull = pair.Key == null; // 진단용 로직이므로 생략 가능
            // bool isStrictlyNull = pair.Key is null; // 진단용 로직이므로 생략 가능

            if (pair.Key == null)
            {
                // 💡 핵심 수정 2: totalScore 대신 지역 변수 currentDungeonScore에 합산합니다.
                currentDungeonScore += pair.Value;
            }
        }

        // 핵심 수정 3: 클래스 멤버 변수인 totalScore를 여기서 초기화하여 다음 던전에 점수가 누적되는 것을 막습니다.
        // 만약 totalScore가 전역(게임 전체) 점수라면 이 줄을 제거하고,
        // 이 함수가 반환하는 currentDungeonScore를 사용하여 전역 점수를 갱신해야 합니다.
        // 현재는 '던전 최종 점수' 계산에 초점을 맞추므로, 다음 던전을 위해 totalScore를 0으로 초기화합니다.
        //totalScore = 0; // 또는 totalScore = currentDungeonScore; (만약 totalScore가 마지막 던전 점수 저장용이라면)

        // 다음 던전을 위해 딕셔너리를 비웁니다. (이것이 던전 점수 관리의 핵심 초기화 로직입니다.)
        monsterScores.Clear();

        // 💡 핵심 수정 4: 계산된 지역 변수 점수를 반환합니다.
        return currentDungeonScore;
    }

    // 💡 참고: totalScore 변수를 사용하지 않거나, 제거하는 것을 고려하여 코드를 더 깔끔하게 만들 수 있습니다.
    // 만약 totalScore가 전체 게임 점수였다면, InitializeScores와 CalculateFinalScore 사이에 ResetScores() 메서드를 추가하는 것이 SOLID 원칙에 더 적합합니다.
}