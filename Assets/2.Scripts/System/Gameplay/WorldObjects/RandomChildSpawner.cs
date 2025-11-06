/*
 * RandomChildSpawner.cs
 * * 기능: 부모 오브젝트의 BoxCollider 경계를 따라 무작위 위치에 자식 프리팹을 생성하고,
 * 설정에 따라 생성된 오브젝트의 색상을 노란색으로 변경합니다.
 * * 특징: 
 * 1. 생성 축(X 또는 Z)을 인스펙터에서 설정합니다. (SOLID OCP 준수)
 * 2. 자식 오브젝트의 위치는 부모의 로컬 좌표계를 따릅니다.
 * 3. 스케일은 부모의 영향을 받지 않습니다.
 * 4. [추가] 불리언 변수로 색상 변경 기능을 켜고 끌 수 있습니다. (SOLID OCP 준수)
 */
using UnityEngine;

public enum SpawnAxisType
{
    X_Axis_Fixed,
    Z_Axis_Fixed
}

public class RandomChildSpawner : MonoBehaviour
{
    // <SOLID OCP: 외부 설정 변수>

    [Header("1. 프리팹 및 개수 설정")]
    [Tooltip("생성할 수 있는 프리팹 목록입니다.")]
    [SerializeField] private GameObject[] prefabList;
    [Tooltip("생성될 오브젝트의 최소 개수 (포함)")]
    [SerializeField] private int minSpawnCount = 50;
    [Tooltip("생성될 오브젝트의 최대 개수 (포함)")]
    [SerializeField] private int maxSpawnCount = 70;

    [Header("2. 생성 위치 설정")]
    [Tooltip("가장자리 배치를 위해 X축 또는 Z축 중 어느 축을 고정할지 선택합니다.")]
    [SerializeField] private SpawnAxisType axisType = SpawnAxisType.X_Axis_Fixed;

    // 3. [추가] 색상 변경 기능 설정 변수
    [Header("3. 추가 기능 설정")]
    [Tooltip("체크 시, 생성된 모든 프리팹의 재질 색상을 검은색으로 변경합니다.")]
    [SerializeField] private bool changeColor = false;

    // 노란색을 미리 정의해둡니다. (읽기 전용 상수)
    private static readonly Color YellowColor = Color.black;


    // <내부 사용 변수>
    // 부모 오브젝트의 BoxCollider 크기 (localScale은 미적용)
    private Vector3 spawnBoundarySize = Vector3.one;

    private void Start()
    {
        SpawnChildren();
    }

    /// <summary>
    /// [SRP 준수] 부모 오브젝트에서 BoxCollider 정보를 가져와 생성 경계를 초기화합니다.
    /// </summary>
    /// <returns>초기화 성공 여부 (Collider가 존재해야 성공)</returns>
    private bool TryInitializeSize()
    {
        BoxCollider boundaryCollider = GetComponent<BoxCollider>();

        if (boundaryCollider == null)
        {
            Debug.LogError($"[RandomChildSpawner] 오브젝트 '{gameObject.name}'에 BoxCollider가 없습니다! 스폰 중단.");
            return false;
        }

        // 부모의 로컬 BoxCollider 크기를 저장합니다.
        spawnBoundarySize = boundaryCollider.size;
        return true;
    }

    /// <summary>
    /// [SRP, OCP 준수] 외부 설정(axisType)에 따라 부모 경계의 가장자리(Edge)에 위치하는 로컬 좌표를 계산합니다.
    /// </summary>
    /// <returns>생성될 오브젝트의 로컬 위치 Vector3</returns>
    private Vector3 CalculateRandomEdgePosition()
    {
        Vector3 randomLocalPos = Vector3.zero;

        // X, Z 축의 절반 크기를 계산합니다.
        float halfX = spawnBoundarySize.x / 2.0f;
        float halfZ = spawnBoundarySize.z / 2.0f;

        if (axisType == SpawnAxisType.X_Axis_Fixed) // X축 고정 모드: (X_fixed, 0, Z_random)
        {
            // X축 고정 (두 줄 배치)
            randomLocalPos.x = (Random.value < 0.5f) ? halfX : -halfX;

            // Z축 랜덤 (고르게 채워지는 축)
            // 고객님 환경에서 정상 작동이 확인된 로직 유지
            randomLocalPos.x = Random.Range(-halfX, halfX);
        }
        else // Z_Axis_Fixed 모드: (X_random, 0, Z_fixed)
        {
            // Z축 고정 (두 줄 배치)
            randomLocalPos.z = (Random.value < 0.5f) ? halfZ : -halfZ;

            // X축 랜덤 (고르게 채워지는 축)
            // 고객님 환경에서 정상 작동이 확인된 로직 유지
            randomLocalPos.z = Random.Range(-halfZ, halfZ);
        }

        // Y 값: 부모 평면(y=0)에 생성 (고객님의 설정인 -0.5f 유지)
        randomLocalPos.y = -0.5f;

        return randomLocalPos;
    }

    /// <summary>
    /// [SRP 준수] 오브젝트의 렌더러를 찾아 재질의 기본 색상을 노란색으로 변경합니다.
    /// </summary>
    /// <param name="target">색상을 변경할 GameObject</param>
    private void ChangeColorIfRequired(GameObject target)
    {
        // 불리언 변수가 체크되지 않았다면 아무 작업 없이 종료합니다. (OCP)
        if (!changeColor)
        {
            return;
        }

        // 오브젝트의 Renderer 컴포넌트를 가져옵니다.
        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer != null)
        {
            // [참고] material 대신 sharedMaterial을 사용하면 씬의 다른 오브젝트에도 영향을 줄 수 있으므로, 
            // 여기서는 안전하게 material을 사용하여 인스턴스화된 재질의 색상만 변경합니다.

            // Renderer의 재질에서 메인 색상 속성을 찾아 색상을 변경합니다.
            // 대부분의 Standard 셰이더는 "_Color" 속성을 사용합니다.
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = YellowColor;
            }
            else
            {
                Debug.LogWarning($"[RandomChildSpawner] '{target.name}'의 셰이더에 '_Color' 속성이 없어 색상 변경을 건너뜁니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[RandomChildSpawner] '{target.name}'에 Renderer 컴포넌트가 없어 색상 변경을 건너뜁니다.");
        }
    }


    /// <summary>
    /// [SRP, OCP 준수] 메인 생성 함수: 자식의 스케일만 부모의 영향을 받지 않도록 역 스케일을 수동 적용합니다.
    /// </summary>
    public void SpawnChildren()
    {
        if (!TryInitializeSize() || prefabList == null || prefabList.Length == 0)
        {
            if (prefabList == null || prefabList.Length == 0)
            {
                Debug.LogWarning("[RandomChildSpawner] 생성할 프리팹 목록이 비어있습니다.");
            }
            return;
        }

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        // 부모 스케일의 역수를 미리 계산합니다.
        Vector3 parentScale = transform.localScale;

        Vector3 inverseParentScale = new Vector3(
            1f / (Mathf.Abs(parentScale.x) < Mathf.Epsilon ? 1f : parentScale.x),
            1f / (Mathf.Abs(parentScale.y) < Mathf.Epsilon ? 1f : parentScale.y),
            1f / (Mathf.Abs(parentScale.z) < Mathf.Epsilon ? 1f : parentScale.z)
        );

        // 자식 오브젝트의 월드 스케일이 (1, 1, 1)이 되도록 보정하는 localScale 값
        Vector3 desiredLocalScale = Vector3.Scale(Vector3.one, inverseParentScale);


        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefabToSpawn = prefabList[Random.Range(0, prefabList.Length)];

            // 랜덤 위치 계산 (가장자리 로직)
            Vector3 spawnLocalPosition = CalculateRandomEdgePosition();

            // 1. 부모에게 바로 붙여 생성합니다.
            GameObject newChild = Instantiate(prefabToSpawn, transform);

            // 2. 계산된 로컬 위치 및 회전 적용
            newChild.transform.localPosition = spawnLocalPosition;
            newChild.transform.localRotation = Quaternion.identity;

            // 3. [핵심 로직]: localScale에 역 스케일을 적용하여 World Scale이 (1, 1, 1)이 되도록 보정합니다.
            newChild.transform.localScale = desiredLocalScale;

            // 4. [추가된 기능 호출]: 설정에 따라 색상 변경을 시도합니다.
            ChangeColorIfRequired(newChild);

            newChild.name = $"Spawned_{prefabToSpawn.name}_{i}";
        }

        string colorMessage = changeColor ? " (색 변경됨) " : "";
    }
}