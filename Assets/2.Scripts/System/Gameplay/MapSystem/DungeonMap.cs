using System.Collections.Generic;
using UnityEngine;
using System.Linq; // HashSet 및 Linq 사용을 위해 유지

public class DungeonMap : MonoBehaviour
{
    // 싱글턴 패턴을 위한 인스턴스 (SRP, Singleton)
    public static DungeonMap Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField]
    private Vector3 gridSize = new Vector3(100f, 1f, 100f);
    [SerializeField]
    private Transform gridOriginTile; // 그리드 원점 타일을 직접 할당

    private Vector3 gridOrigin; // 스크립트가 내부적으로 사용할 그리드 원점
    [SerializeField]
    private Vector3 offGridPosition = new Vector3(-9999f, 0f, 0f); // 회수 대상 맵들이 놓이는 위치

    // 1단계: 인스펙터에서 할당할 던전 타일들을 저장할 배열
    [Header("던전 타일 정보")]
    [SerializeField]
    private Transform[] dungeonTiles; // 미리 배치된 던전 타일들을 담을 배열

    // ==========================================================
    // [추가된 기능] 핵심 목표 타일 및 상태 변수
    // ==========================================================
    [Header("핵심 목표 타일")]
    [Tooltip("이 타일이 SmallMap에 의해 점유되면 CanDungeon이 True가 됩니다. (dungeonTiles 중 하나여야 함)")]
    [SerializeField]
    private Transform coreObjectiveTile; // 인스펙터에서 할당할 핵심 타일 (특정 조건 충족용)

    private Vector2Int coreObjectiveCoords; // 핵심 타일의 그리드 좌표 (빠른 검색용)
    [SerializeField]
    private Transform[] denialTiles;// 빌리지맵 범위
    // occupiedTiles에 값으로 등록되어 겹침을 유도할 더미 Transform
    private Transform DUMMY_DENIAL_MAP; //
    // 내부 상태를 저장하는 백킹 필드
    private bool _canDungeon = false;

    /// <summary>
    /// 핵심 목표 타일이 SmallMap에 의해 점유되었는지 나타내는 상태 플래그입니다. (Encapsulation)
    /// 이 값이 True이면 던전의 주요 조건이 충족되었음을 의미합니다.
    /// 외부에서는 읽기(Get)만 가능하며, 쓰기(Set)는 DungeonMap 내부에서만 가능합니다.
    /// </summary>
    public bool CanDungeon
    {
        get => _canDungeon;
        private set => _canDungeon = value;
    }
    // ==========================================================

    // 배치된 다른 스몰맵 타일들의 위치와 오브젝트를 저장하는 딕셔너리
    // Key: 그리드 좌표, Value: 해당 타일을 점유한 SmallMap의 Transform (또는 DUMMY_DENIAL_MAP)
    private Dictionary<Vector2Int, Transform> occupiedTiles = new Dictionary<Vector2Int, Transform>();

    // 던전 타일들의 그리드 좌표를 저장할 HashSet (빠른 검색용)
    private HashSet<Vector2Int> validDungeonTileCoords = new HashSet<Vector2Int>();

    private void Awake()
    {
        // 싱글턴 인스턴스 초기화 (SRP, Singleton)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // 이미 다른 인스턴스가 있다면 현재 게임 오브젝트 파괴 (싱글턴 규칙)
            Destroy(gameObject);
            return; // 이후 코드는 실행하지 않음
        }

        // 그리드 원점 설정
        if (gridOriginTile != null)
        {
            gridOrigin = gridOriginTile.position;
        }
        else
        {
            Debug.LogError("Grid Origin Tile이 할당되지 않았습니다. Grid Origin Tile을 Inspector에서 설정해주세요.");
        }

        // 1단계: 던전 타일 그리드 좌표 등록
        if (dungeonTiles == null || dungeonTiles.Length == 0)
        {
            Debug.LogError("Dungeon Tiles 배열이 비어있습니다. Inspector에서 던전 타일들을 할당해주세요.");
            return;
        }

        foreach (Transform tile in dungeonTiles)
        {
            if (tile != null)
            {
                Vector2Int gridCoords = GetGridCoordinates(tile.position);
                validDungeonTileCoords.Add(gridCoords);
            }
        }

        // ==========================================================
        // [고객님 요청] Denial Tile 영구 점유 로직 (OCP: 기존 로직 확장)
        // ==========================================================
        if (denialTiles != null && denialTiles.Length > 0)
        {
            // DUMMY_DENIAL_MAP을 현재 DungeonMap의 Transform으로 설정하여 SmallMap과 구분
            DUMMY_DENIAL_MAP = this.transform;

            foreach (Transform denialTile in denialTiles) // 배열 순회
            {
                if (denialTile != null)
                {
                    Vector2Int denialCoords = GetGridCoordinates(denialTile.position);
                    // Denial Tile 위치를 DUMMY_DENIAL_MAP으로 점유합니다.
                    occupiedTiles[denialCoords] = DUMMY_DENIAL_MAP;
                }
            }
        }

        // ==========================================================
        // [추가된 기능] 핵심 타일 그리드 좌표 초기화
        // ==========================================================
        if (coreObjectiveTile != null)
        {
            coreObjectiveCoords = GetGridCoordinates(coreObjectiveTile.position);

            // 핵심 타일이 던전 영역 내에 있는지 안전성 검사
            if (!validDungeonTileCoords.Contains(coreObjectiveCoords))
            {
                Debug.LogWarning("Core Objective Tile이 Dungeon Tiles 영역 밖에 있습니다. 의도한 동작이 아닐 수 있습니다.");
            }
        }
        else
        {
            Debug.LogWarning("핵심 목표 타일(Core Objective Tile)이 할당되지 않았습니다. CanDungeon 기능이 작동하지 않습니다.");
        }
    }
    // --- [추가된 공개 API 영역] ---

    /// <summary>
    /// **[수정됨]** DungeonMap 그리드 영역에 유효하게 배치되지 않아 occupiedTiles에 등록되지 않은
    /// **순수한 SmallMap** 인스턴스만 찾아 리스트로 반환합니다. (TownMap 등의 파생 클래스는 제외)
    /// </summary>
    /// <returns>회수 대상 SmallMap 리스트</returns>
    public List<SmallMap> GetInvalidlyPlacedMaps()
    {
        // 씬에 활성화되어 있는 모든 SmallMap 인스턴스를 찾습니다. (TownMap 포함)
        SmallMap[] allMaps = FindObjectsByType<SmallMap>(FindObjectsSortMode.None);

        List<SmallMap> invalidMaps = new List<SmallMap>();

        // occupiedTiles 딕셔너리의 Value (SmallMap.transform)들만 추출하여 HashSet을 생성합니다.
        HashSet<Transform> occupiedTransforms = new HashSet<Transform>();

        // Denial Tile의 더미 Transform은 회수 대상 SmallMap이 아니므로 제외하고 추가합니다.
        foreach (Transform t in occupiedTiles.Values)
        {
            if (t != DUMMY_DENIAL_MAP)
            {
                occupiedTransforms.Add(t);
            }
        }

        // 전체 SmallMap(파생 클래스 포함)을 순회하며 검사합니다.
        foreach (SmallMap map in allMaps)
        {
            // OCP/LSP: 순수 SmallMap 타입인 인스턴스만 던전 회수 대상으로 간주합니다.
            // TownMap은 ViligeMap에서 관리되어야 하므로 제외합니다.
            if (map.GetType() != typeof(SmallMap))
            {
                continue;
            }

            // 맵의 Transform이 occupiedTiles에 등록되지 않았다면 회수 대상으로 간주합니다.
            if (!occupiedTransforms.Contains(map.transform))
            {
                invalidMaps.Add(map);
                // Debug.Log($"<color=orange>[DungeonMap Reclaim Target]</color> '{map.name}'이 occupiedTiles에 등록되지 않아 회수 대상으로 판정됨.");
            }
        }

        // Debug.Log($"DungeonMap: 최종 회수 대상 SmallMap {invalidMaps.Count}개 반환.");
        return invalidMaps;
    }


    /// <summary>
    /// 로드(Load) 시스템에 의해 SmallMap 오브젝트가 파괴될 때,
    /// occupiedTiles에서 해당 맵이 점유했던 모든 정보를 해제합니다.
    /// (WorldStateSaver.LoadData -> Destroy -> SmallMap.OnDisable 시 호출)
    /// </summary>
    /// <param name="map">점유를 해제할 SmallMap 인스턴스</param>
    public void DeregisterOccupiedTiles(SmallMap map)
    {
        // 기존의 내부 해제 로직을 그대로 재사용합니다. (DRY)
        RemoveOccupiedTiles(map);
        // Debug.Log($"Deregister: 맵 {map.name}의 점유 정보 해제 완료.");
    }

    /// <summary>
    /// 로드(Load) 시스템에 의해 SmallMap 오브젝트가 새로 생성될 때,
    /// 해당 맵의 현재 위치를 기반으로 occupiedTiles에 점유 상태를 등록합니다.
    /// (WorldStateSaver.LoadData -> Instantiate -> SmallMap.OnEnable 시 호출)
    /// </summary>
    /// <param name="map">점유를 등록할 SmallMap 인스턴스</param>
    public void RegisterOccupiedTiles(SmallMap map)
    {
        // 1. 등록 전에 혹시 모를 잔여 정보를 제거하여 중복 등록을 방지합니다. (SRP)
        RemoveOccupiedTiles(map);

        // 2. 현재 맵이 던전 영역과 접촉하는지 확인합니다. (던전 외부는 등록하지 않음)
        bool hasContact = false;
        Vector3 currentMapPosition = map.transform.position;

        foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
        {
            Vector3 worldTilePos = currentMapPosition + tileOffset;
            Vector2Int gridCoords = GetGridCoordinates(worldTilePos);

            if (validDungeonTileCoords.Contains(gridCoords))
            {
                hasContact = true;
                break;
            }
        }

        // 3. 던전 영역과 접촉하는 경우에만 occupiedTiles에 등록합니다.
        if (hasContact)
        {
            // 기존의 내부 등록 로직을 그대로 재사용합니다. (DRY)
            AddOccupiedTiles(map);
            // Debug.Log($"Register: 맵 {map.name}의 점유 정보 등록 완료.");
        }
    }

    /// <summary>
    /// 월드 위치(Vector3)를 그리드 좌표(Vector2Int)로 변환하여 반환합니다. (SRP)
    /// </summary>
    /// <param name="worldPos">변환할 월드 위치</param>
    /// <returns>해당 위치의 그리드 좌표</returns>
    public Vector2Int GetGridCoordinates(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - gridOrigin;
        int x = Mathf.RoundToInt(relativePos.x / gridSize.x);
        int z = Mathf.RoundToInt(relativePos.z / gridSize.z);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 맵 배치의 유효성을 검사합니다. (겹침, 던전 범위 이탈 여부) (SRP)
    /// </summary>
    /// <param name="map">배치할 SmallMap 인스턴스</param>
    /// <param name="targetWorldPos">배치할 월드 위치</param>
    /// <returns>배치 가능 여부 (bool)</returns>
    public bool IsPlacementValid(SmallMap map, Vector3 targetWorldPos)
    {
        // 스몰맵의 타일 중 하나라도 던전 타일과 겹치는지 확인하는 플래그
        bool hasContactWithDungeon = false;

        // 스몰맵의 각 타일에 대해 검사
        foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
        {
            Vector3 worldTilePos = targetWorldPos + tileOffset;
            Vector2Int gridCoords = GetGridCoordinates(worldTilePos);

            // 1차 검사: 스몰맵의 타일 중 하나라도 던전 타일과 겹치는지 확인
            if (validDungeonTileCoords.Contains(gridCoords))
            {
                hasContactWithDungeon = true;
            }

            // 2차 검사: 이미 다른 스몰맵이 점유된 위치인지 확인
            if (occupiedTiles.ContainsKey(gridCoords))
            {
                // 현재 검사 중인 맵 자신이 아닌 다른 맵이나 Denial Map이 점유하고 있다면 유효하지 않음
                if (occupiedTiles[gridCoords] != map.transform)
                {
                    return false; // 다른 맵/Denial Map과 겹치므로 유효하지 않음
                }
            }
        }

        // 최종 판정: 던전 타일과 전혀 겹치지 않았다면
        if (!hasContactWithDungeon)
        {
            return true; // 유효성 검사 통과 (자유 배치 허용)
        }
        else
        {
            // 던전 타일과 겹침이 있었다면, 모든 타일이 유효한 던전 범위 안에 있는지 재확인 (LSP: 던전 규칙)
            foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
            {
                Vector3 worldTilePos = targetWorldPos + tileOffset;
                Vector2Int gridCoords = GetGridCoordinates(worldTilePos);

                // 스몰맵의 모든 타일이 던전 범위 안에 있는지 확인
                if (!validDungeonTileCoords.Contains(gridCoords))
                {
                    return false; // 하나라도 던전 범위를 벗어나면 유효하지 않음
                }
            }
            return true; // 모든 검사 통과 (던전 내부에 완벽하게 배치됨)
        }
    }

    /// <summary>
    /// 마우스에서 놓인 위치를 기반으로 맵을 스냅하고, 유효성 검사 후 점유 상태를 업데이트합니다. (SRP)
    /// </summary>
    /// <param name="map">배치할 SmallMap 인스턴스</param>
    public void SnapAndPlace(SmallMap map)
    {
        Vector3 currentMouseWorldPos = map.transform.position; // 최종적으로 놓인 위치
        Vector2Int originCoords = GetGridCoordinates(currentMouseWorldPos);
        Vector3 snappedPos = GetWorldPosition(originCoords); // 그리드에 맞춰 스냅될 잠정적 위치

        // 유효성 검사를 먼저 수행합니다.
        bool isValidPlacement = IsPlacementValid(map, currentMouseWorldPos);

        // 1. 제거 로직: 일단 현재 맵이 점유하고 있던 모든 타일 정보를 제거합니다.
        // 이 과정에서 CanDungeon 상태가 업데이트됩니다.
        RemoveOccupiedTiles(map);

        if (isValidPlacement)
        {
            // 2. 배치 로직: 유효한 위치일 경우 (던전 내부 또는 외부)
            // hasContact를 다시 계산하여 던전 타일과의 접촉 여부를 확인합니다.
            bool hasContact = false;
            foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
            {
                Vector3 worldTilePos = currentMouseWorldPos + tileOffset;
                Vector2Int gridCoords = GetGridCoordinates(worldTilePos);
                if (validDungeonTileCoords.Contains(gridCoords))
                {
                    hasContact = true;
                    break;
                }
            }

            if (hasContact) // 던전 타일 위에 놓으려는 경우 (유효한 배치)
            {
                map.transform.position = snappedPos; // 그리드에 맞춰 스냅된 위치로 배치
                // 새로운 위치 정보를 occupiedTiles에 추가하고, 이 내부에서 CanDungeon을 업데이트합니다.
                AddOccupiedTiles(map);
            }
            // else: 던전 외부에 놓으려는 경우. occupiedTiles에 등록하지 않습니다.
        }
        else // IsPlacementValid가 false인 경우 (유효하지 않은 배치)
        {
            map.transform.position = offGridPosition; // OffGridPosition으로 이동
            // 유효하지 않은 배치는 occupiedTiles에 등록되지 않습니다.
        }
    }

    /// <summary>
    /// SmallMap이 점유했던 모든 그리드 타일 정보를 Dictionary에서 제거하고, 상태를 업데이트합니다. (SRP)
    /// </summary>
    /// <param name="map">점유를 해제할 SmallMap 인스턴스</param>
    private void RemoveOccupiedTiles(SmallMap map)
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();

        // Dictionary를 순회하며 해당 맵이 점유했던 모든 키를 찾습니다.
        foreach (var pair in occupiedTiles)
        {
            if (pair.Value == map.transform)
            {
                toRemove.Add(pair.Key);
            }
        }

        // 찾은 키들을 Dictionary에서 제거합니다.
        foreach (var key in toRemove)
        {
            occupiedTiles.Remove(key);
        }

        // 점유 해제 후, 핵심 타일의 상태를 즉시 업데이트합니다. (OCP)
        UpdateCanDungeonState();
    }

    /// <summary>
    /// SmallMap이 현재 위치에서 점유하는 모든 그리드 타일 정보를 Dictionary에 등록하고, 상태를 업데이트합니다. (SRP)
    /// </summary>
    /// <param name="map">점유를 등록할 SmallMap 인스턴스</param>
    private void AddOccupiedTiles(SmallMap map)
    {
        // SmallMap을 구성하는 모든 타일 오프셋을 순회하며 점유 상태를 등록합니다.
        foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
        {
            Vector3 worldTilePos = map.transform.position + tileOffset;
            Vector2Int gridCoords = GetGridCoordinates(worldTilePos);
            // 딕셔너리에 추가하거나 기존 값을 덮어씁니다.
            occupiedTiles[gridCoords] = map.transform;
        }

        // 점유 등록 후, 핵심 타일의 상태를 즉시 업데이트합니다. (OCP)
        UpdateCanDungeonState();
    }

    /// <summary>
    /// 핵심 목표 타일(coreObjectiveCoords)이 현재 SmallMap에 의해 점유되었는지 확인하고
    /// CanDungeon 변수의 상태를 업데이트합니다. (SRP)
    /// </summary>
    private void UpdateCanDungeonState()
    {
        // 핵심 타일이 할당되지 않았다면 상태를 False로 유지하고 종료
        if (coreObjectiveTile == null)
        {
            CanDungeon = false;
            return;
        }

        // occupiedTiles 딕셔너리에 핵심 타일의 좌표(Key)가 존재하고, 
        // 그 값이 Denial Map이 아닌 SmallMap의 Transform인지 확인합니다.
        bool isOccupiedBySmallMap = occupiedTiles.ContainsKey(coreObjectiveCoords) &&
                                    occupiedTiles[coreObjectiveCoords] != DUMMY_DENIAL_MAP;

        // CanDungeon 속성(Property)을 통해 값을 안전하게 설정합니다. (Encapsulation)
        CanDungeon = isOccupiedBySmallMap;

        // 추가적인 튜토리얼/이벤트 호출 로직
        if (CanDungeon && UITutorialHandler.Instance != null)
        {
            UITutorialHandler.Instance.OnPlacementComplete.Invoke();
        }
        else
        {
            // 튜토리얼 단계일 경우, 유효하지 않은 배치 알림을 띄웁니다.
            if (TutorialManager.Instance != null && TutorialManager.Instance.CurrentStep == TutorialStep.WaitPlacementComplete)
            {
                TutorialManager.Instance.ShowInvalidPlacementNotification();
            }
        }
    }

    /// <summary>
    /// Denial Tile을 제외한, 플레이어(SmallMap)가 현재 점유하고 있는 타일의 총 개수를 반환합니다. (SRP)
    /// </summary>
    /// <returns>순수하게 플레이어가 배치한 타일의 갯수 (int)</returns>
    public int GetPlayerOccupiedTileCount()
    {
        int count = 0;
        // occupiedTiles 딕셔너리의 '값' (Transform)을 순회합니다.
        foreach (var tileTransform in occupiedTiles.Values)
        {
            // Denial Tile (빌리지 맵)을 제외하고, SmallMap에 의해 점유된 타일만 카운트합니다.
            if (tileTransform != DUMMY_DENIAL_MAP)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 그리드 좌표(Vector2Int)를 월드 위치(Vector3)로 변환하여 반환합니다. (SRP)
    /// </summary>
    /// <param name="gridCoords">변환할 그리드 좌표</param>
    /// <returns>그리드 중심의 월드 위치</returns>
    private Vector3 GetWorldPosition(Vector2Int gridCoords)
    {
        float x = gridCoords.x * gridSize.x + gridOrigin.x;
        float z = gridCoords.y * gridSize.z + gridOrigin.z;
        // 높이는 0으로 고정합니다.
        return new Vector3(x, 0, z);
    }
}