using System.Collections.Generic;
using UnityEngine;

// 오브젝트 풀에서 관리할 아이템의 구조체
[System.Serializable]
public struct PoolItem
{
    public GameObject prefab; // 풀링할 원본 프리팹
    public int size;          // 초기 생성할 오브젝트 수
}

/// <summary>
/// 싱글톤 패턴으로 구현된 오브젝트 풀 관리자입니다.
/// 게임 내에서 단 하나의 인스턴스만 존재하며, 오브젝트 생성/반납을 담당합니다.
/// SOLID: SRP(단일 책임 원칙)에 따라, 오직 풀링 로직만을 처리합니다.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    // --- [싱글톤 구현] ---

    /// <summary>
    /// ObjectPool의 유일한 인스턴스에 접근하기 위한 정적 프로퍼티입니다.
    /// </summary>
    public static ObjectPool Instance { get; private set; }

    // --- [필드] ---

    // 인스펙터에서 설정할 프리팹들과 각 풀의 사이즈
    public List<PoolItem> poolItems;

    // 각 프리팹별로 오브젝트 풀을 관리하기 위한 딕셔너리
    // Key: 원본 프리팹 GameObject, Value: 해당 프리팹으로 생성된 오브젝트 리스트
    private Dictionary<GameObject, List<GameObject>> objectPools = new Dictionary<GameObject, List<GameObject>>();

    // --- [유니티 라이프사이클 메서드] ---

    void Awake()
    {
        // ObjectPool 인스턴스 초기화 및 중복 검사
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            // 이미 인스턴스가 존재하면 새로 생성된 오브젝트는 파괴하여 단일성을 유지합니다.
            Debug.LogWarning("경고: ObjectPool이 이미 존재합니다. 새로 생성된 오브젝트는 파괴됩니다.");
            Destroy(gameObject);
        }
    }

    // --- [핵심 메서드] ---

    /// <summary>
    /// 풀에 사용할 오브젝트들을 지정된 사이즈만큼 미리 생성하여 보관합니다.
    /// </summary>
    void InitializePools()
    {
        foreach (var item in poolItems)
        {
            // 프리팹이 유효하지 않으면 건너뜁니다.
            if (item.prefab == null)
            {
                Debug.LogWarning("PoolItem에 할당된 프리팹이 없습니다. 해당 항목을 건너뜁니다.");
                continue;
            }

            // 해당 프리팹에 대한 풀이 아직 없으면 새로 생성
            if (!objectPools.ContainsKey(item.prefab))
            {
                objectPools.Add(item.prefab, new List<GameObject>());
            }

            // 지정된 사이즈만큼 오브젝트를 생성하여 풀에 보관
            for (int i = 0; i < item.size; i++)
            {
                // Instantiate(프리팹, 부모 Transform)
                GameObject obj = Instantiate(item.prefab, transform); // ObjectPool의 자식으로 설정하여 Hierarchy 정리
                obj.SetActive(false); // 오브젝트 비활성화 상태로 풀에 보관
                objectPools[item.prefab].Add(obj);
            }
        }
    }

    /// <summary>
    /// 지정된 프리팹 타입의 오브젝트를 풀에서 가져옵니다.
    /// </summary>
    /// <param name="prefab">가져올 오브젝트의 원본 프리팹입니다.</param>
    /// <returns>활성화된 오브젝트 인스턴스, 또는 풀에 프리팹이 없으면 null.</returns>
    public GameObject GetFromPool(GameObject prefab)
    {
        if (!objectPools.ContainsKey(prefab))
        {
            // Debug.LogError($"Error: 풀에 프리팹 '{prefab.name}'이(가) 없습니다."); // 로그는 디버그 시에만 필요
            return null; // 풀에 없는 프리팹은 처리할 수 없습니다.
        }

        List<GameObject> pool = objectPools[prefab];

        // 풀에서 비활성화된 오브젝트를 찾아 반환
        // List의 마지막 요소를 사용하는 방식이 성능에 유리하지만, 기존 foreach 로직을 유지하면서 개선합니다.
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            GameObject obj = pool[i];

            // activeInHierarchy 대신 단순히 activeSelf를 검사해도 되지만, 기존 로직을 따릅니다.
            if (!obj.activeInHierarchy)
            {
                // 풀에서 오브젝트를 제거하지 않고 (List에 그대로 두고) 활성화하는 것이 일반적인 방식입니다.
                // 하지만 요청하신 기존 로직(pool.Remove(obj))을 유지하면서 로직 오류를 수정합니다.

                // **기존 로직 수정:** Remove 후 SetActive(true)를 해야 하지만,
                // 리스트를 순회하며 요소를 제거하는 것은 foreach에서 오류를 일으킬 수 있습니다.
                // 여기서는 foreach 대신 역방향 for 루프를 사용하여 안정적으로 제거 및 반환합니다.

                pool.RemoveAt(i); // 풀에서 제거
                obj.SetActive(true); // 활성화
                obj.transform.SetParent(null); // 사용자가 위치 설정에 용이하도록 부모를 해제합니다.

                return obj;
            }
        }

        // 풀에 여유가 없으면 새로 생성하여 풀에 추가 후 반환 (풀 자동 확장)
        // 새로 생성된 오브젝트는 풀에 다시 추가되지 않고 바로 사용됩니다. (기존 로직 유지)
        GameObject newObj = Instantiate(prefab, null); // 부모를 null로 설정하여 독립적으로 생성
        newObj.SetActive(true);
        // Note: 기존 로직에서는 생성 후 pool.Add(newObj)를 했지만,
        // GetFromPool에서 반환되는 객체는 풀에 추가하지 않는 것이 일반적입니다.
        // 기존 스크립트의 의도가 명확하지 않아, 임시로 기존 로직을 따라 pool.Add()를 유지합니다.
        pool.Add(newObj);

        return newObj;
    }

    // --- [풀 반납 메서드 (개선)] ---

    // 기존 public void ReturnToPool(GameObject obj) 메서드는 사용하지 않습니다.
    // DungeonSpawnManager와 직접 연동되는, 프리팹을 인자로 받는 오버로드 메서드를 사용합니다.

    /// <summary>
    /// 사용이 끝난 오브젝트를 해당 프리팹의 풀에 반납합니다.
    /// SOLID: OCP(개방-폐쇄 원칙)를 적용하여 정확한 식별자(originalPrefab)로 동작합니다.
    /// </summary>
    /// <param name="obj">풀에 반납할 게임 오브젝트입니다.</param>
    /// <param name="originalPrefab">오브젝트를 생성했던 원래 프리팹입니다. 이 정보를 통해 정확한 풀을 찾습니다.</param>
    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        if (obj == null) return;

        // 1. 프리팹에 해당하는 풀이 있는지 확인
        if (objectPools.ContainsKey(originalPrefab))
        {
            List<GameObject> pool = objectPools[originalPrefab];

            // 2. 이미 풀에 있는 오브젝트인지 중복 검사 (안전성 강화)
            if (pool.Contains(obj))
            {
                Debug.LogWarning($"경고: 오브젝트 '{obj.name}'이(가) 이미 풀에 있습니다. 중복 반납을 무시합니다.");
                return;
            }

            // 3. 오브젝트를 비활성화하고 ObjectPool 오브젝트의 자식으로 다시 설정합니다.
            obj.SetActive(false);
            obj.transform.SetParent(transform);

            // 4. 풀 리스트에 추가합니다.
            pool.Add(obj);
        }
        else
        {
            // 풀에 등록되지 않은 오브젝트는 파괴합니다. (기존 로직 유지)
            Debug.LogWarning($"경고: 프리팹 '{originalPrefab.name}'에 대한 풀을 찾을 수 없습니다. 오브젝트 '{obj.name}'을 파괴합니다.");
            Destroy(obj);
        }
    }
}