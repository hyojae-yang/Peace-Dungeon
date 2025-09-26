using UnityEngine;
using System.Collections.Generic;
using System;

public class DungeonSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class MonsterSpawnData
    {
        [Tooltip("생성할 몬스터의 프리팹입니다.")]
        public GameObject monsterPrefab;
        [Tooltip("생성할 몬스터의 최소 개수입니다.")]
        [Range(0, 300)]
        public int minSpawnCount;
        [Tooltip("생성할 몬스터의 최대 개수입니다.")]
        [Range(0, 300)]
        public int maxSpawnCount;
        [Tooltip("해당 몬스터 처치 시 획득할 점수입니다.")]
        [Range(0, 1000)]
        public int score;
    }

    [Header("몬스터 스폰 설정")]
    [Tooltip("몬스터들이 스폰될 평면(Plan) 오브젝트들의 배열입니다. 각 플랜에는 Renderer가 있어야 합니다.")]
    [SerializeField] private GameObject[] spawnPlans;
    [Tooltip("생성할 몬스터들의 종류와 개수를 설정합니다.")]
    [SerializeField] private List<MonsterSpawnData> monsterSpawnList;

    // 몬스터의 종류별로 생성된 몬스터 리스트를 저장합니다.
    private Dictionary<string, List<GameObject>> spawnedMonsters = new Dictionary<string, List<GameObject>>();
    // 생성된 몬스터 객체와 점수를 매핑합니다.
    private Dictionary<GameObject, int> monsterScores = new Dictionary<GameObject, int>();

    /// <summary>
    /// Awake 메서드는 스크립트 인스턴스가 로드될 때 호출됩니다.
    /// 던전 매니저에 자신을 등록합니다.
    /// </summary>
    private void Awake()
    {
        if (spawnPlans == null || spawnPlans.Length == 0)
        {
            Debug.LogError("스폰 평면(Plan) 배열이 설정되지 않았습니다. 몬스터를 생성할 수 없습니다.");
            return;
        }

        // DungeonManager의 인스턴스가 존재하면 자신을 등록합니다.
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.RegisterSpawnManager(this);
        }
    }

    /// <summary>
    /// 스크립트가 파괴될 때 던전 매니저에 등록 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.UnregisterSpawnManager(this);
        }
    }

    /// <summary>
    /// DungeonManager에 의해 호출되어 몬스터 생성을 시작하는 메서드입니다.
    /// </summary>
    public void SpawnAllMonsters()
    {
        if (spawnPlans.Length == 0 || monsterSpawnList == null || monsterSpawnList.Count == 0)
        {
            Debug.LogWarning("스폰할 몬스터나 스폰 평면이 없습니다.");
            return;
        }

        // **중요: 스폰 전에 모든 딕셔너리를 초기화하여 이전 데이터를 제거합니다.**
        spawnedMonsters.Clear();
        monsterScores.Clear();

        foreach (var spawnData in monsterSpawnList)
        {
            if (spawnData.monsterPrefab == null)
            {
                Debug.LogWarning("몬스터 프리팹이 할당되지 않았습니다. 다음 몬스터로 넘어갑니다.");
                continue;
            }

            int numberOfMonstersToSpawn = UnityEngine.Random.Range(spawnData.minSpawnCount, spawnData.maxSpawnCount + 1);

            if (!spawnedMonsters.ContainsKey(spawnData.monsterPrefab.name))
            {
                spawnedMonsters.Add(spawnData.monsterPrefab.name, new List<GameObject>());
            }

            for (int i = 0; i < numberOfMonstersToSpawn; i++)
            {
                GameObject selectedPlan = spawnPlans[UnityEngine.Random.Range(0, spawnPlans.Length)];
                Renderer planRenderer = selectedPlan.GetComponent<Renderer>();

                if (planRenderer == null)
                {
                    Debug.LogError($"'{selectedPlan.name}'에 Renderer 컴포넌트가 없습니다. 몬스터를 생성할 수 없습니다.");
                    continue;
                }

                Bounds selectedBounds = planRenderer.bounds;

                float randomX = UnityEngine.Random.Range(selectedBounds.min.x, selectedBounds.max.x);
                float randomZ = UnityEngine.Random.Range(selectedBounds.min.z, selectedBounds.max.z);

                Vector3 spawnPosition = new Vector3(randomX, selectedBounds.max.y, randomZ);

                GameObject spawnedMonster = Instantiate(spawnData.monsterPrefab, spawnPosition, Quaternion.identity);

                spawnedMonsters[spawnData.monsterPrefab.name].Add(spawnedMonster);
                monsterScores.Add(spawnedMonster, spawnData.score);

            }
        }

        if (DungeonScoreManager.Instance != null)
        {
            DungeonScoreManager.Instance.InitializeScores(monsterScores);
        }

    }

    /// <summary>
    /// 던전에서 나갈 때 호출되어 생성된 모든 몬스터를 파괴하고 관련 딕셔너리를 비웁니다.
    /// </summary>
    public void DestroyAllMonsters()
    {
        try
        {

            // 생성된 몬스터들을 모두 찾아 파괴합니다.
            foreach (var monsterList in spawnedMonsters.Values)
            {
                foreach (var monster in monsterList)
                {
                    if (monster != null)
                    {
                        Destroy(monster);
                    }
                }
            }

            // 딕셔너리 내부의 리스트들을 모두 비웁니다.
            foreach (var monsterList in spawnedMonsters.Values)
            {
                monsterList.Clear();
            }

            // 모든 몬스터 객체 참조를 초기화합니다.
            spawnedMonsters.Clear();
            monsterScores.Clear();

        }
        catch (Exception ex)
        {
            Debug.LogError("DestroyAllMonsters 예외: " + ex);
        }
    }
}