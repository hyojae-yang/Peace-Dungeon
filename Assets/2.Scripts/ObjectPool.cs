using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoolItem
{
    public GameObject prefab;
    public int size;
}

public class ObjectPool : MonoBehaviour
{
    // 인스펙터에서 설정할 프리팹들과 각 풀의 사이즈
    public List<PoolItem> poolItems;

    // 각 프리팹별로 오브젝트 풀을 관리하기 위한 딕셔너리
    private Dictionary<GameObject, List<GameObject>> objectPools = new Dictionary<GameObject, List<GameObject>>();

    void Awake()
    {
        InitializePools();
    }

    void InitializePools()
    {
        foreach (var item in poolItems)
        {
            // 해당 프리팹에 대한 풀이 아직 없으면 새로 생성
            if (!objectPools.ContainsKey(item.prefab))
            {
                objectPools.Add(item.prefab, new List<GameObject>());
            }

            // 지정된 사이즈만큼 오브젝트를 생성하여 풀에 보관
            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform); // 부모를 ObjectPool 오브젝트로 설정
                obj.SetActive(false);
                objectPools[item.prefab].Add(obj);
            }
        }
    }

    // 지정된 프리팹 타입의 오브젝트를 풀에서 가져옵니다.
    public GameObject GetFromPool(GameObject prefab)
{
    Debug.Log($"GetFromPool() 호출: 프리팹 '{prefab.name}' 요청");

    if (!objectPools.ContainsKey(prefab))
    {
        Debug.LogError($"Error: 풀에 프리팹 '{prefab.name}'이(가) 없습니다.");
        return null;
    }

    List<GameObject> pool = objectPools[prefab];
    Debug.Log($"풀의 총 오브젝트 수: {pool.Count}");

    // 풀에서 비활성화된 오브젝트를 찾아 반환
    foreach (GameObject obj in pool)
    {
        if (!obj.activeInHierarchy)
        {
            //Debug.Log(obj.name);
            pool.Remove(obj); // 사용 중인 오브젝트이므로 풀에서 제거
            obj.SetActive(true);

            Debug.Log($"비활성화된 오브젝트 발견: '{obj.name}'");
            return obj;
        }
    }
    
    // 풀에 여유가 없으면 새로 생성하여 풀에 추가 후 반환
    GameObject newObj = Instantiate(prefab, transform);
    newObj.SetActive(true);
    pool.Add(newObj);
    return newObj;
}

    // 오브젝트를 풀에 반납합니다.
    public void ReturnToPool(GameObject obj)
    {
        // 반납하려는 오브젝트의 프리팹을 찾아 풀에 넣습니다.
        // 이 부분은 obj.tag나 다른 식별자를 사용하여 프리팹을 찾아야 할 수 있습니다.
        // 더 효율적인 방법으로는 반납할 때 프리팹 정보를 함께 전달받는 것입니다.

        // 간단하게는 obj.transform.parent를 통해 프리팹 정보를 얻거나,
        // obj.GetComponent<PoolObjectIdentifier>().prefab 이와 같이 처리할 수 있습니다.
        // 여기서는 obj.tag를 사용한다고 가정합니다.
        string prefabTag = obj.tag; // 프리팹에 태그가 설정되어 있어야 합니다.

        foreach (var item in poolItems)
        {
            if (item.prefab.tag == prefabTag)
            {
                obj.SetActive(false);
                // obj.transform.SetParent(transform); // ObjectPool의 자식으로 다시 설정
                return;
            }
        }

        Debug.LogWarning($"Warning: Object '{obj.name}' with tag '{prefabTag}' not found in any pool. Destroying instead.");
        Destroy(obj); // 풀에 해당 오브젝트가 없으면 파괴
    }
}