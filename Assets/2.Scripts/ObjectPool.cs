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
    // �ν����Ϳ��� ������ �����յ�� �� Ǯ�� ������
    public List<PoolItem> poolItems;

    // �� �����պ��� ������Ʈ Ǯ�� �����ϱ� ���� ��ųʸ�
    private Dictionary<GameObject, List<GameObject>> objectPools = new Dictionary<GameObject, List<GameObject>>();

    void Awake()
    {
        InitializePools();
    }

    void InitializePools()
    {
        foreach (var item in poolItems)
        {
            // �ش� �����տ� ���� Ǯ�� ���� ������ ���� ����
            if (!objectPools.ContainsKey(item.prefab))
            {
                objectPools.Add(item.prefab, new List<GameObject>());
            }

            // ������ �����ŭ ������Ʈ�� �����Ͽ� Ǯ�� ����
            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform); // �θ� ObjectPool ������Ʈ�� ����
                obj.SetActive(false);
                objectPools[item.prefab].Add(obj);
            }
        }
    }

    // ������ ������ Ÿ���� ������Ʈ�� Ǯ���� �����ɴϴ�.
    public GameObject GetFromPool(GameObject prefab)
{
    Debug.Log($"GetFromPool() ȣ��: ������ '{prefab.name}' ��û");

    if (!objectPools.ContainsKey(prefab))
    {
        Debug.LogError($"Error: Ǯ�� ������ '{prefab.name}'��(��) �����ϴ�.");
        return null;
    }

    List<GameObject> pool = objectPools[prefab];
    Debug.Log($"Ǯ�� �� ������Ʈ ��: {pool.Count}");

    // Ǯ���� ��Ȱ��ȭ�� ������Ʈ�� ã�� ��ȯ
    foreach (GameObject obj in pool)
    {
        if (!obj.activeInHierarchy)
        {
            //Debug.Log(obj.name);
            pool.Remove(obj); // ��� ���� ������Ʈ�̹Ƿ� Ǯ���� ����
            obj.SetActive(true);

            Debug.Log($"��Ȱ��ȭ�� ������Ʈ �߰�: '{obj.name}'");
            return obj;
        }
    }
    
    // Ǯ�� ������ ������ ���� �����Ͽ� Ǯ�� �߰� �� ��ȯ
    GameObject newObj = Instantiate(prefab, transform);
    newObj.SetActive(true);
    pool.Add(newObj);
    return newObj;
}

    // ������Ʈ�� Ǯ�� �ݳ��մϴ�.
    public void ReturnToPool(GameObject obj)
    {
        // �ݳ��Ϸ��� ������Ʈ�� �������� ã�� Ǯ�� �ֽ��ϴ�.
        // �� �κ��� obj.tag�� �ٸ� �ĺ��ڸ� ����Ͽ� �������� ã�ƾ� �� �� �ֽ��ϴ�.
        // �� ȿ������ ������δ� �ݳ��� �� ������ ������ �Բ� ���޹޴� ���Դϴ�.

        // �����ϰԴ� obj.transform.parent�� ���� ������ ������ ��ų�,
        // obj.GetComponent<PoolObjectIdentifier>().prefab �̿� ���� ó���� �� �ֽ��ϴ�.
        // ���⼭�� obj.tag�� ����Ѵٰ� �����մϴ�.
        string prefabTag = obj.tag; // �����տ� �±װ� �����Ǿ� �־�� �մϴ�.

        foreach (var item in poolItems)
        {
            if (item.prefab.tag == prefabTag)
            {
                obj.SetActive(false);
                // obj.transform.SetParent(transform); // ObjectPool�� �ڽ����� �ٽ� ����
                return;
            }
        }

        Debug.LogWarning($"Warning: Object '{obj.name}' with tag '{prefabTag}' not found in any pool. Destroying instead.");
        Destroy(obj); // Ǯ�� �ش� ������Ʈ�� ������ �ı�
    }
}