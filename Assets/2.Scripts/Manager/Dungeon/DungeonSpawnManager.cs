using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 던전 내 몬스터의 생성(Spawn)과 파괴(Destroy)를 전담하는 클래스입니다.
/// Object Pooling 시스템 없이 기본 Instantiate/Destroy 방식으로 동작합니다.
/// SOLID 원칙 중 Single Responsibility Principle을 지키기 위해,
/// 몬스터 자체의 점수 관리나 풀링에 대한 책임은 지지 않습니다.
/// </summary>
public class DungeonSpawnManager : MonoBehaviour
{
    /// <summary>
    /// 유니티 인스펙터에서 몬스터 종류별 스폰 정보를 설정하기 위한 Serializable 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class MonsterSpawnData
    {
        [Tooltip("생성할 몬스터의 원본 프리팹입니다.")]
        public GameObject monsterPrefab;
        [Tooltip("생성할 몬스터의 최소 개수입니다. (포함)")]
        [Range(0, 300)]
        public int minSpawnCount;
        [Tooltip("생성할 몬스터의 최대 개수입니다. (포함)")]
        [Range(0, 1000)]
        public int maxSpawnCount;
    }

    [Header("몬스터 스폰 설정")]
    [Tooltip("몬스터들이 스폰될 평면(Plan) 오브젝트들의 배열입니다. 각 플랜에는 Renderer가 필수적입니다.")]
    [SerializeField] private GameObject[] spawnPlans;
    [Tooltip("던전에서 생성할 몬스터들의 종류와 개수를 설정합니다.")]
    [SerializeField] private List<MonsterSpawnData> monsterSpawnList;

    /// <summary>
    /// 생성된 몬스터 객체들을 저장하는 딕셔너리입니다.
    /// Key: 몬스터의 원본 프리팹 (GameObject)
    /// Value: 해당 프리팹으로 생성된 활성화된 몬스터 인스턴스 리스트
    /// DestroyAllMonsters 메서드에서 원본 프리팹을 키로 사용하여 몬스터들을 일괄 파괴할 수 있게 합니다.
    /// </summary>
    private Dictionary<GameObject, List<GameObject>> spawnedMonsters = new Dictionary<GameObject, List<GameObject>>();

    /// <summary>
    /// 스크립트 인스턴스가 로드될 때 호출됩니다.
    /// 스폰 평면이 유효한지 확인하고, DungeonManager에 자신을 등록합니다.
    /// </summary>
    private void Awake()
    {
        // 입력 유효성 검사 (Fail-fast 패턴)
        if (spawnPlans == null || spawnPlans.Length == 0)
        {
            Debug.LogError("스폰 평면(Plan) 배열이 설정되지 않았습니다. 몬스터를 생성할 수 없습니다.");
            return;
        }

        // DungeonManager가 있다면, 몬스터 스폰/파괴에 대한 책임을 위임받기 위해 자신을 등록합니다.
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.RegisterSpawnManager(this);
        }
    }

    /// <summary>
    /// 스크립트가 파괴될 때 호출됩니다.
    /// 메모리 누수 방지 및 정확한 상태 관리를 위해 DungeonManager에 등록을 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.UnregisterSpawnManager(this);
        }
    }

    /// <summary>
    /// DungeonManager에 의해 호출되어 몬스터 생성을 시작합니다.
    /// 설정된 데이터에 따라 몬스터를 생성하고 spawnPlans 영역 내에 랜덤하게 배치합니다.
    /// 오브젝트 풀링을 사용하지 않고 'Instantiate'로 몬스터를 생성합니다.
    /// </summary>
    public void SpawnAllMonsters()
    {
        // 스폰 가능 여부 체크
        if (spawnPlans.Length == 0 || monsterSpawnList == null || monsterSpawnList.Count == 0)
        {
            Debug.LogWarning("스폰할 몬스터나 스폰 평면이 없습니다. 몬스터 생성 작업을 건너뜁니다.");
            return;
        }

        // 스폰 전에 이전 라운드의 데이터 클린업
        spawnedMonsters.Clear();

        // 설정된 몬스터 목록을 순회하며 몬스터를 생성합니다.
        foreach (var spawnData in monsterSpawnList)
        {
            if (spawnData.monsterPrefab == null)
            {
                Debug.LogWarning("몬스터 프리팹이 할당되지 않은 MonsterSpawnData가 있습니다. 다음 몬스터로 넘어갑니다.");
                continue;
            }

            // min/max 범위 내에서 무작위 몬스터 개수를 결정합니다.
            int numberOfMonstersToSpawn = UnityEngine.Random.Range(spawnData.minSpawnCount, spawnData.maxSpawnCount + 1);

            // 해당 프리팹에 대한 리스트를 딕셔너리에 준비합니다.
            if (!spawnedMonsters.ContainsKey(spawnData.monsterPrefab))
            {
                spawnedMonsters.Add(spawnData.monsterPrefab, new List<GameObject>());
            }

            // 결정된 수만큼 몬스터를 생성하고 스폰 위치를 결정합니다.
            for (int i = 0; i < numberOfMonstersToSpawn; i++)
            {
                // 1. 스폰할 평면을 무작위로 선택합니다.
                GameObject selectedPlan = spawnPlans[UnityEngine.Random.Range(0, spawnPlans.Length)];
                Renderer planRenderer = selectedPlan.GetComponent<Renderer>();

                if (planRenderer == null)
                {
                    Debug.LogError($"'{selectedPlan.name}'에 Renderer 컴포넌트가 없습니다. 몬스터를 생성할 수 없습니다.");
                    continue; // 다음 스폰 시도로 넘어갑니다.
                }

                // 2. 평면의 경계(Bounds)를 가져옵니다.
                Bounds selectedBounds = planRenderer.bounds;

                // 3. 경계 내에서 무작위 x, z 좌표를 결정합니다.
                float randomX = UnityEngine.Random.Range(selectedBounds.min.x, selectedBounds.max.x);
                float randomZ = UnityEngine.Random.Range(selectedBounds.min.z, selectedBounds.max.z);

                // 4. 스폰 위치를 계산합니다. (평면의 y축 최댓값 + 몬스터의 중심이 될 수 있도록)
                Vector3 spawnPosition = new Vector3(randomX, selectedBounds.max.y, randomZ);

                // 5. 오브젝트 풀링 없이, 순수하게 'Instantiate'로 몬스터를 생성합니다.
                GameObject spawnedMonster = Instantiate(spawnData.monsterPrefab, spawnPosition, Quaternion.identity);

                // 6. 생성된 몬스터를 추적 리스트에 추가합니다.
                spawnedMonsters[spawnData.monsterPrefab].Add(spawnedMonster);
            }
        }
    }

    /// <summary>
    /// 던전에서 나갈 때 DungeonManager에 의해 호출되어 생성된 모든 몬스터를 파괴합니다.
    /// 오브젝트 풀링을 사용하지 않고 'Destroy'로 몬스터를 제거합니다.
    /// </summary>
    public void DestroyAllMonsters()
    {
        try
        {
            // 생성된 몬스터 딕셔너리를 순회하며 모든 몬스터 객체를 파괴합니다.
            foreach (var kvp in spawnedMonsters)
            {
                // 원본 프리팹은 사용하지 않지만, 구조상 키로 남아 있습니다.
                // GameObject originalPrefab = kvp.Key; 

                foreach (var monster in kvp.Value)
                {
                    // 몬스터 객체가 아직 남아있다면 (null이 아니라면) 파괴합니다.
                    if (monster != null)
                    {
                        // 오브젝트 풀링 대신 Unity의 기본 Destroy 메서드를 사용합니다.
                        Destroy(monster);
                    }
                }
            }

            // 모든 몬스터 객체 참조를 초기화하고 딕셔너리를 비웁니다.
            // 리스트 내부 요소가 모두 파괴되었더라도, 리스트 자체와 딕셔너리 구조는 메모리에서 해제해야 합니다.
            foreach (var monsterList in spawnedMonsters.Values)
            {
                monsterList.Clear();
            }
            spawnedMonsters.Clear();

        }
        catch (Exception ex)
        {
            // 예외 발생 시 로그 출력 (강건한 코드 작성을 위함)
            Debug.LogError("DestroyAllMonsters 실행 중 예외가 발생했습니다: " + ex.Message);
        }
    }
}