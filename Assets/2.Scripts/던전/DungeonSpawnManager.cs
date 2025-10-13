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

    // 💡 변경 1: 딕셔너리 키 타입을 string에서 GameObject로 변경
    /// <summary>
    /// 몬스터의 종류(원본 프리팹)별로 생성된 몬스터 리스트를 저장합니다.
    /// Key가 GameObject이므로 DestroyAllMonsters에서 원본 프리팹을 바로 알 수 있습니다.
    /// </summary>
    private Dictionary<GameObject, List<GameObject>> spawnedMonsters = new Dictionary<GameObject, List<GameObject>>();

    // 생성된 몬스터 객체와 점수를 매핑합니다. (변경 없음)
    private Dictionary<GameObject, int> monsterScores = new Dictionary<GameObject, int>();

    /// <summary>
    /// Awake 메서드는 스크립트 인스턴스가 로드될 때 호출됩니다.
    /// 던전 매니저에 자신을 등록합니다. (변경 없음)
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
    /// 스크립트가 파괴될 때 던전 매니저에 등록 해제합니다. (변경 없음)
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

        // 💡 변경 2-1: ObjectPool 인스턴스를 가져와 풀링 사용 여부를 판단
        ObjectPool pooler = ObjectPool.Instance;
        bool usePooling = pooler != null;
        if (!usePooling)
        {
            Debug.LogWarning("ObjectPool 인스턴스를 찾을 수 없습니다. 풀링을 사용하지 않고 Instantiate를 사용합니다.");
        }


        foreach (var spawnData in monsterSpawnList)
        {
            if (spawnData.monsterPrefab == null)
            {
                Debug.LogWarning("몬스터 프리팹이 할당되지 않았습니다. 다음 몬스터로 넘어갑니다.");
                continue;
            }

            int numberOfMonstersToSpawn = UnityEngine.Random.Range(spawnData.minSpawnCount, spawnData.maxSpawnCount + 1);

            // 💡 변경 2-2: 딕셔너리 키를 monsterPrefab (GameObject)으로 사용
            if (!spawnedMonsters.ContainsKey(spawnData.monsterPrefab))
            {
                spawnedMonsters.Add(spawnData.monsterPrefab, new List<GameObject>());
            }

            for (int i = 0; i < numberOfMonstersToSpawn; i++)
            {
                // **(이 부분은 기존 로직이 그대로 유지됩니다: 스폰 위치 계산)**
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
                // **(스폰 위치 계산 로직 끝)**

                GameObject spawnedMonster = null;

                if (usePooling)
                {
                    // 💡 변경 2-3: Instantiate 대신 풀에서 가져오기 시도
                    spawnedMonster = pooler.GetFromPool(spawnData.monsterPrefab);
                }

                if (spawnedMonster == null)
                {
                    // 💡 변경 2-4: 풀링 실패 또는 풀링 미사용 시 기존 Instantiate 로직
                    spawnedMonster = Instantiate(spawnData.monsterPrefab, spawnPosition, Quaternion.identity);
                }

                // 위치/회전 설정 (풀에서 가져왔든 새로 만들었든 공통으로 적용)
                spawnedMonster.transform.position = spawnPosition;
                spawnedMonster.transform.rotation = Quaternion.identity;


                // 딕셔너리에 추가 (키가 GameObject이므로 spawnData.monsterPrefab 사용)
                spawnedMonsters[spawnData.monsterPrefab].Add(spawnedMonster);
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
            // 변경 3-1: ObjectPool 인스턴스를 가져와 풀링 사용 여부를 판단
            ObjectPool pooler = ObjectPool.Instance;
            bool usePooling = pooler != null;

            if (!usePooling)
            {
                Debug.LogWarning("ObjectPool 인스턴스를 찾을 수 없습니다. 기존 Destroy 로직을 사용합니다.");
            }

            // 생성된 몬스터들을 모두 찾아 반납하거나 파괴합니다.
            foreach (var kvp in spawnedMonsters)
            {
                // Key는 이제 원본 프리팹 GameObject입니다!
                GameObject originalPrefab = kvp.Key;

                foreach (var monster in kvp.Value)
                {
                    if (monster != null)
                    {
                        if (usePooling)
                        {
                            // 변경 3-2: Destroy 대신 ReturnToPool 오버로드 메서드 호출
                            // 원본 프리팹(originalPrefab)을 함께 넘겨 풀에 정확히 반납합니다.
                            pooler.ReturnToPool(monster, originalPrefab);
                        }
                        else
                        {
                            // 풀링 시스템 미사용 시: 기존 Destroy 로직 유지
                            Destroy(monster);
                        }
                    }
                }
            }

            // 딕셔너리 내부의 리스트들을 모두 비웁니다.
            // **(이 부분은 기존 로직이 그대로 유지됩니다)**
            foreach (var monsterList in spawnedMonsters.Values)
            {
                monsterList.Clear();
            }

            // 모든 몬스터 객체 참조를 초기화합니다.
            spawnedMonsters.Clear();
            monsterScores.Clear();
            // **(딕셔너리 초기화 로직 끝)**

        }
        catch (Exception ex)
        {
            Debug.LogError("DestroyAllMonsters 예외: " + ex);
        }
    }
}