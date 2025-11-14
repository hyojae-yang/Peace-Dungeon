using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MonsterDataManager 클래스는 게임 내 모든 몬스터 데이터(MonsterData SO)를 로드하고
/// 몬스터 ID를 이름으로 변환해주는 룩업 테이블을 관리하는 싱글톤입니다.
/// </summary>
public class MonsterDataManager : MonoBehaviour
{
    // === 싱글톤 인스턴스 ===
    private static MonsterDataManager _instance;

    public static MonsterDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MonsterDataManager>();
                if (_instance == null)
                {
                    Debug.LogError("MonsterDataManager 인스턴스가 씬에 없습니다. 게임 오브젝트에 컴포넌트를 추가해야 합니다.");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 몬스터 ID(int)를 몬스터 이름(string)에 매핑하는 룩업 테이블입니다.
    /// </summary>
    private readonly Dictionary<int, string> _monsterNameMap = new Dictionary<int, string>();

    /// <summary>
    /// 유니티의 Resources 폴더 내 몬스터 데이터 Asset이 있는 경로입니다.
    /// (예: "Data/Monsters/")
    /// </summary>
    private const string MONSTER_DATA_PATH = "Monster";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        LoadAllMonsterData();
    }

    /// <summary>
    /// Resources 폴더에서 모든 MonsterData 스크립터블 오브젝트를 로드하고 
    /// ID-이름 매핑 룩업 테이블(_monsterNameMap)을 구성합니다.
    /// </summary>
    private void LoadAllMonsterData()
    {
        _monsterNameMap.Clear(); // 맵 초기화

        MonsterData[] allMonsters = Resources.LoadAll<MonsterData>(MONSTER_DATA_PATH);

        if (allMonsters.Length == 0)
        {
            Debug.LogWarning($"[MonsterDataManager] 경로 '{MONSTER_DATA_PATH}'에서 로드된 몬스터 데이터가 없습니다. Asset 경로를 확인해주세요.");
            return;
        }

        foreach (var data in allMonsters)
        {
            if (data == null || data.monsterID == 0)
            {
                Debug.LogWarning($"[MonsterDataManager] 유효하지 않은 몬스터 데이터(이름:{data.name}, ID:{data.monsterID})가 로드되었습니다.");
                continue;
            }

            int currentID = data.monsterID;
            string currentName = data.monsterName;

            // [수정] 중복된 ID가 있다면 에러 대신 처리를 시도합니다.
            if (_monsterNameMap.ContainsKey(currentID))
            {
                string existingName = _monsterNameMap[currentID];

                if (existingName == currentName)
                {
                    // 이름이 같으면 단순히 경고 후 덮어쓰기 (실제론 동일 Asset의 복사본일 가능성)
                    // 이 경우 처치 기록은 ID를 기준으로 통합됩니다.
                   // Debug.LogWarning($"[MonsterDataManager] 중복된 몬스터 ID 발견! ID: {currentID}, 이름: {currentName}. 동일 이름이므로 덮어씁니다. 처치 기록은 이 ID로 통합됩니다.");
                    _monsterNameMap[currentID] = currentName; // 덮어쓰기
                }
                else
                {
                    // ID는 같으나 이름이 다르면, 이 ID의 대표 이름을 무엇으로 할지 결정해야 합니다.
                    // (여기서는 나중에 로드된 이름으로 덮어쓰고 경고를 남깁니다.)
                   // Debug.LogWarning($"[MonsterDataManager] ID는 같으나 이름이 다른 몬스터 발견! ID: {currentID}. 기존 이름: {existingName}, 새 이름: {currentName}. 새 이름으로 덮어씁니다.");
                    _monsterNameMap[currentID] = currentName; // 덮어쓰기 (대표 이름 설정)
                }
            }
            else
            {
                // 중복되지 않은 경우, 맵에 새 ID와 이름을 추가합니다.
                _monsterNameMap.Add(currentID, currentName);
            }
        }
        //Debug.Log($"[MonsterDataManager] 총 {_monsterNameMap.Count}개의 유효한 몬스터 ID-이름 맵 로드 완료.");
    }

    /// <summary>
    /// 몬스터 ID를 받아 해당 몬스터의 이름을 반환합니다.
    /// EndingUIManager의 DisplayKillCount 메서드에서 사용됩니다.
    /// </summary>
    /// <param name="id">조회할 몬스터의 고유 ID입니다.</param>
    /// <returns>몬스터 이름. ID를 찾지 못하면 대체 문자열을 반환합니다.</returns>
    public string GetMonsterName(int id)
    {
        // 룩업 테이블에서 ID를 검색합니다.
        if (_monsterNameMap.TryGetValue(id, out string name))
        {
            // 이름을 찾았으면 반환합니다.
            return name;
        }

        // ID가 맵에 없는 경우 안전한 대체 문자열을 반환합니다.
        return $"[알 수 없는 몬스터 ID:{id}]";
    }
}