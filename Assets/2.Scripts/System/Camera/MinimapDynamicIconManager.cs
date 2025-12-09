using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// [SOLID - OCP/SRP 준수]: 씬 내에서 다양한 타입의 추적 대상(몬스터, NPC 등)의 생성 및 소멸에 맞춰,
// 해당 타입에 맞는 미니맵 아이콘을 동적으로 생성/제거하는 책임만 가집니다.
public class MinimapDynamicIconManager : MonoBehaviour
{
    // ==========================================================
    // [1. Enum 정의] 추적할 대상의 타입을 구분합니다.
    // ==========================================================

    /// <summary>
    /// 미니맵에 동적으로 표시할 대상의 타입 목록입니다.
    /// 새로운 추적 대상이 필요할 때 여기에 추가합니다.
    /// </summary>
    public enum MinimapTargetType
    {
        None = 0,       // 기본/오류 방지
        Monster = 1,    // 적대적 대상
        NPC = 2,        // 상인, 퀘스트 제공자 등 비적대적 대상
        Resource = 3,   // 채집 가능한 자원
        Exit = 4        // 던전 출구/입구 등
    }

    // ==========================================================
    // [2. 구조체 정의] 인스펙터에서 타입별 프리팹을 설정합니다.
    // ==========================================================

    /// <summary>
    /// MinimapTargetType과 그에 대응하는 아이콘 프리팹을 묶는 구조체입니다.
    /// </summary>
    [System.Serializable]
    public struct TargetIconConfig
    {
        [Tooltip("아이콘이 표시될 대상의 타입입니다.")]
        public MinimapTargetType targetType;

        [Tooltip("해당 타입의 대상이 등록될 때 인스턴스화할 아이콘 UI 프리팹입니다. MinimapIconTracker 컴포넌트가 부착되어 있어야 합니다.")]
        public GameObject iconPrefab;
    }

    // ==========================================================
    // [3. 멤버 변수 및 Static Instance]
    // ==========================================================

    // [설계 패턴: Static Instance] 씬 어디서든 이 Manager에 쉽게 접근하도록 합니다.
    public static MinimapDynamicIconManager Instance { get; private set; }

    [Header("Dependencies")]
    [Tooltip("미니맵 이미지가 출력되는 RawImage (MinimapDisplay)")]
    public RawImage minimapDisplay;

    [Header("Target Icon Configuration")]
    [Tooltip("추적 대상 타입별로 사용할 아이콘 프리팹을 할당합니다.")]
    public List<TargetIconConfig> iconConfigs = new List<TargetIconConfig>();

    // 런타임 최적화를 위한 딕셔너리: TargetType을 키로 사용하여 O(1) 속도로 프리팹을 찾습니다.
    private Dictionary<MinimapTargetType, GameObject> prefabMap = new Dictionary<MinimapTargetType, GameObject>();

    // 현재 활성화된 대상 Transform(Key)과 해당 아이콘 오브젝트(Value)를 저장하는 딕셔너리
    // 팩트: Transform을 사용하여 대상을 찾을 때 O(1)의 속도를 보장합니다.
    private Dictionary<Transform, GameObject> activeTargetIcons = new Dictionary<Transform, GameObject>();

    // ==========================================================
    // [생성 및 초기화]
    // ==========================================================
    void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            InitializePrefabMap();
        }
    }

    /// <summary>
    /// 인스펙터에서 설정된 iconConfigs 리스트를 딕셔너리로 변환하여 런타임 성능을 최적화합니다.
    /// </summary>
    private void InitializePrefabMap()
    {
        prefabMap.Clear();
        foreach (var config in iconConfigs)
        {
            // 중복된 타입이 있거나 프리팹이 누락된 경우를 방지합니다.
            if (config.iconPrefab == null)
            {
                Debug.LogError($"[MinimapDynamicIconManager] {config.targetType} 타입에 할당된 아이콘 프리팹이 누락되었습니다.");
                continue;
            }
            if (prefabMap.ContainsKey(config.targetType))
            {
                Debug.LogWarning($"[MinimapDynamicIconManager] {config.targetType} 타입이 중복되어, 첫 번째 프리팹만 사용됩니다.");
                continue;
            }

            prefabMap.Add(config.targetType, config.iconPrefab);
        }

        // 미니맵 디스플레이 필수 체크
        if (minimapDisplay == null)
        {
            Debug.LogError("[MinimapDynamicIconManager] MinimapDisplay RawImage가 할당되지 않았습니다. 아이콘 등록이 불가능합니다.");
        }
    }


    // ==========================================================
    // [외부 호출 함수]
    // ==========================================================

    /// <summary>
    /// 추적 대상(몬스터, NPC 등)이 생성되었을 때 호출되어 해당 대상의 아이콘을 등록합니다.
    /// </summary>
    /// <param name="targetTransform">추적할 대상의 Transform입니다.</param>
    /// <param name="targetType">추적할 대상의 타입(MinimapTargetType)입니다. 이 타입에 따라 아이콘 프리팹이 결정됩니다.</param>
    public void RegisterTarget(Transform targetTransform, MinimapTargetType targetType)
    {
        // 1. 유효성 검사 및 중복 등록 방지
        if (minimapDisplay == null || targetTransform == null || activeTargetIcons.ContainsKey(targetTransform))
        {
            return;
        }

        // 2. 타입에 맞는 프리팹 찾기
        if (!prefabMap.TryGetValue(targetType, out GameObject prefab))
        {
            Debug.LogWarning($"[MinimapDynamicIconManager] 알 수 없는 타입 [{targetType}]입니다. 아이콘 등록을 건너뜁니다.");
            return;
        }

        // 3. 아이콘 UI 오브젝트 동적 생성
        // 부모를 minimapDisplay의 Transform으로 설정하여 미니맵 영역 내에 배치합니다.
        GameObject iconObject = Instantiate(prefab, minimapDisplay.transform);

        // 4. 아이콘의 추적 스크립트 (MinimapIconTracker) 설정
        MinimapIconTracker tracker = iconObject.GetComponent<MinimapIconTracker>();
        if (tracker != null)
        {
            // MinimapIconTracker의 Target 변수에 현재 대상의 Transform을 할당합니다.
            tracker.target = targetTransform;
            // 팩트: 이 시점에서 MinimapIconTracker의 Start() 함수가 실행되어 추적을 시작합니다.
        }
        else
        {
            Debug.LogError($"[MinimapDynamicIconManager] {targetType} 프리팹에 MinimapIconTracker 컴포넌트가 없습니다.");
        }

        // 5. 딕셔너리에 등록
        activeTargetIcons.Add(targetTransform, iconObject);
    }

    /// <summary>
    /// 추적 대상이 파괴되었을 때 호출되어 해당 대상의 아이콘을 제거합니다.
    /// </summary>
    /// <param name="targetTransform">제거할 대상의 Transform입니다.</param>
    public void DeregisterTarget(Transform targetTransform)
    {
        if (targetTransform == null) return;

        // 딕셔너리에서 아이콘을 찾습니다.
        if (activeTargetIcons.TryGetValue(targetTransform, out GameObject iconObject))
        {
            // 딕셔너리에서 제거
            activeTargetIcons.Remove(targetTransform);

            // 아이콘 UI 오브젝트 파괴
            Destroy(iconObject);
        }
    }
}