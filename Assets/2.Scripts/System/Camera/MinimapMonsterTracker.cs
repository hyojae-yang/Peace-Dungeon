using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// [SOLID - SRP 준수]: 씬 내에서 몬스터의 생성 및 소멸에 맞춰 미니맵 아이콘을 동적으로 생성/제거하는 책임만 가집니다.
public class MinimapMonsterTracker : MonoBehaviour
{
    // [설계 패턴: Static Instance] 씬 어디서든 이 Manager에 쉽게 접근하도록 합니다.
    // 팩트: FindObjectOfType()보다 훨씬 빠르고 효율적인 접근 방식입니다.
    public static MinimapMonsterTracker Instance { get; private set; }

    // ==========================================================
    // [Inspector 할당 필요 항목]
    // ==========================================================
    [Header("Dependencies")]
    [Tooltip("MinimapIconTracker 스크립트가 부착된 몬스터 아이콘 UI Prefab")]
    public GameObject monsterIconPrefab;

    [Tooltip("미니맵 이미지가 출력되는 RawImage (MinimapDisplay)")]
    public RawImage minimapDisplay;

    // 현재 활성화된 몬스터 Transform(Key)과 해당 아이콘 오브젝트(Value)를 저장하는 딕셔너리
    // 팩트: 딕셔너리를 사용하여 몬스터가 사라졌을 때 O(1)의 속도로 해당 아이콘을 찾을 수 있습니다.
    private Dictionary<Transform, GameObject> activeMonsterIcons = new Dictionary<Transform, GameObject>();

    // ==========================================================
    // [생성 및 초기화]
    // ==========================================================
    void Awake()
    {
        // 싱글톤 초기화: 씬에 여러 개가 생성되는 것을 방지합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // ==========================================================
    // [외부 호출 함수]
    // ==========================================================

    /// <summary>
    /// 몬스터가 생성되었을 때, 몬스터 오브젝트에 의해 호출되어 아이콘을 등록합니다.
    /// </summary>
    /// <param name="monsterTransform">추적할 몬스터의 Transform</param>
    public void RegisterMonster(Transform monsterTransform)
    {
        // 유효성 검사 및 중복 등록 방지
        if (monsterTransform == null || activeMonsterIcons.ContainsKey(monsterTransform))
        {
            return;
        }

        // 1. 아이콘 UI 오브젝트 동적 생성
        //    부모를 minimapDisplay의 Transform으로 설정하여 미니맵 영역 내에 배치합니다.
        GameObject iconObject = Instantiate(monsterIconPrefab, minimapDisplay.transform);

        // 2. 아이콘의 추적 스크립트 (MinimapIconTracker) 설정
        MinimapIconTracker tracker = iconObject.GetComponent<MinimapIconTracker>();
        if (tracker != null)
        {
            // MinimapIconTracker의 Target 변수에 현재 몬스터의 Transform을 할당합니다.
            tracker.target = monsterTransform;

            // 팩트: 이 시점에서 MinimapIconTracker의 Start() 함수가 실행되며,
            //      minimapCamera와 minimapDisplay를 스스로 찾아 할당합니다.
        }

        // 3. 딕셔너리에 등록
        activeMonsterIcons.Add(monsterTransform, iconObject);
    }

    /// <summary>
    /// 몬스터가 파괴되었을 때, 몬스터 오브젝트에 의해 호출되어 아이콘을 제거합니다.
    /// </summary>
    /// <param name="monsterTransform">제거할 몬스터의 Transform</param>
    public void DeregisterMonster(Transform monsterTransform)
    {
        if (monsterTransform == null) return;

        // 딕셔너리에서 아이콘을 찾습니다.
        if (activeMonsterIcons.TryGetValue(monsterTransform, out GameObject iconObject))
        {
            // 딕셔너리에서 제거
            activeMonsterIcons.Remove(monsterTransform);

            // 아이콘 UI 오브젝트 파괴
            Destroy(iconObject);
        }
    }
}