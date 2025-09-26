using UnityEngine;
using System.Collections.Generic;
using System;

public class DungeonSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class MonsterSpawnData
    {
        [Tooltip("������ ������ �������Դϴ�.")]
        public GameObject monsterPrefab;
        [Tooltip("������ ������ �ּ� �����Դϴ�.")]
        [Range(0, 300)]
        public int minSpawnCount;
        [Tooltip("������ ������ �ִ� �����Դϴ�.")]
        [Range(0, 300)]
        public int maxSpawnCount;
        [Tooltip("�ش� ���� óġ �� ȹ���� �����Դϴ�.")]
        [Range(0, 1000)]
        public int score;
    }

    [Header("���� ���� ����")]
    [Tooltip("���͵��� ������ ���(Plan) ������Ʈ���� �迭�Դϴ�. �� �÷����� Renderer�� �־�� �մϴ�.")]
    [SerializeField] private GameObject[] spawnPlans;
    [Tooltip("������ ���͵��� ������ ������ �����մϴ�.")]
    [SerializeField] private List<MonsterSpawnData> monsterSpawnList;

    // ������ �������� ������ ���� ����Ʈ�� �����մϴ�.
    private Dictionary<string, List<GameObject>> spawnedMonsters = new Dictionary<string, List<GameObject>>();
    // ������ ���� ��ü�� ������ �����մϴ�.
    private Dictionary<GameObject, int> monsterScores = new Dictionary<GameObject, int>();

    /// <summary>
    /// Awake �޼���� ��ũ��Ʈ �ν��Ͻ��� �ε�� �� ȣ��˴ϴ�.
    /// ���� �Ŵ����� �ڽ��� ����մϴ�.
    /// </summary>
    private void Awake()
    {
        if (spawnPlans == null || spawnPlans.Length == 0)
        {
            Debug.LogError("���� ���(Plan) �迭�� �������� �ʾҽ��ϴ�. ���͸� ������ �� �����ϴ�.");
            return;
        }

        // DungeonManager�� �ν��Ͻ��� �����ϸ� �ڽ��� ����մϴ�.
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.RegisterSpawnManager(this);
        }
    }

    /// <summary>
    /// ��ũ��Ʈ�� �ı��� �� ���� �Ŵ����� ��� �����մϴ�.
    /// </summary>
    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.UnregisterSpawnManager(this);
        }
    }

    /// <summary>
    /// DungeonManager�� ���� ȣ��Ǿ� ���� ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    public void SpawnAllMonsters()
    {
        if (spawnPlans.Length == 0 || monsterSpawnList == null || monsterSpawnList.Count == 0)
        {
            Debug.LogWarning("������ ���ͳ� ���� ����� �����ϴ�.");
            return;
        }

        // **�߿�: ���� ���� ��� ��ųʸ��� �ʱ�ȭ�Ͽ� ���� �����͸� �����մϴ�.**
        spawnedMonsters.Clear();
        monsterScores.Clear();

        foreach (var spawnData in monsterSpawnList)
        {
            if (spawnData.monsterPrefab == null)
            {
                Debug.LogWarning("���� �������� �Ҵ���� �ʾҽ��ϴ�. ���� ���ͷ� �Ѿ�ϴ�.");
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
                    Debug.LogError($"'{selectedPlan.name}'�� Renderer ������Ʈ�� �����ϴ�. ���͸� ������ �� �����ϴ�.");
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
    /// �������� ���� �� ȣ��Ǿ� ������ ��� ���͸� �ı��ϰ� ���� ��ųʸ��� ���ϴ�.
    /// </summary>
    public void DestroyAllMonsters()
    {
        try
        {

            // ������ ���͵��� ��� ã�� �ı��մϴ�.
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

            // ��ųʸ� ������ ����Ʈ���� ��� ���ϴ�.
            foreach (var monsterList in spawnedMonsters.Values)
            {
                monsterList.Clear();
            }

            // ��� ���� ��ü ������ �ʱ�ȭ�մϴ�.
            spawnedMonsters.Clear();
            monsterScores.Clear();

        }
        catch (Exception ex)
        {
            Debug.LogError("DestroyAllMonsters ����: " + ex);
        }
    }
}