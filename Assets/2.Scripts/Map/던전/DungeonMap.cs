using System.Collections.Generic;
using UnityEngine;

public class DungeonMap : MonoBehaviour
{
    // 싱글턴 패턴을 위한 인스턴스
    public static DungeonMap Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField]
    private Vector3 gridSize = new Vector3(100f, 1f, 100f);
    [SerializeField]
    private Transform gridOriginTile; // 그리드 원점 타일을 직접 할당

    private Vector3 gridOrigin; // 스크립트가 내부적으로 사용할 그리드 원점
    [SerializeField]
    private Vector3 offGridPosition = new Vector3(-9999f, 0f, 0f);

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
    // (던전 타일 자체는 여기에 포함되지 않습니다. 유효한 '구역'을 표시하기 위해 사용)
    private Dictionary<Vector2Int, Transform> occupiedTiles = new Dictionary<Vector2Int, Transform>();

    // 던전 타일들의 그리드 좌표를 저장할 HashSet (빠른 검색용)
    private HashSet<Vector2Int> validDungeonTileCoords = new HashSet<Vector2Int>();

    private void Awake()
    {
        // 싱글턴 인스턴스 초기화
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
        // 그리드 원점 타일의 위치를 실제 그리드 원점으로 설정
        if (gridOriginTile != null)
        {
            gridOrigin = gridOriginTile.position;
        }
        else
        {
            Debug.LogError("Grid Origin Tile이 할당되지 않았습니다. Grid Origin Tile을 Inspector에서 설정해주세요.");
        }

        // 1단계: 인스펙터에서 할당된 던전 타일들의 그리드 좌표를 validDungeonTileCoords에 저장
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
        // [추가된 기능] 핵심 타일 그리드 좌표 초기화 (Awake 단계에서 1회 수행)
        // ==========================================================
        if (coreObjectiveTile != null)
        {
            coreObjectiveCoords = GetGridCoordinates(coreObjectiveTile.position);

            // 핵심 타일이 던전 영역 내에 있는지 검사 (선택 사항이지만 안전성 향상)
            if (!validDungeonTileCoords.Contains(coreObjectiveCoords))
            {
                Debug.LogWarning("Core Objective Tile이 Dungeon Tiles 영역 밖에 있습니다. 의도한 동작이 아닐 수 있습니다.");
            }
        }
        else
        {
            // 핵심 타일이 없어도 다른 기능은 작동하므로 Error 대신 Warning을 표시
            Debug.LogWarning("핵심 목표 타일(Core Objective Tile)이 할당되지 않았습니다. CanDungeon 기능이 작동하지 않습니다.");
        }
    }
    // --- [추가된 공개 API 영역] ---

    /// <summary>
    /// 로드(Load) 시스템에 의해 SmallMap 오브젝트가 파괴될 때,
    /// occupiedTiles에서 해당 맵이 점유했던 모든 정보를 해제합니다.
    /// (WorldStateSaver.LoadData -> Destroy -> SmallMap.OnDisable 시 호출)
    /// </summary>
    /// <param name="map">점유를 해제할 SmallMap 인스턴스</param>
    public void DeregisterOccupiedTiles(SmallMap map)
    {
        // 기존의 내부 해제 로직을 그대로 재사용합니다.
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
        // 1. 등록 전에 혹시 모를 잔여 정보를 제거하여 중복 등록을 방지합니다.
        // OCP: 기존 로직을 보호하면서 새로운 생명주기 로직을 지원합니다.
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
            // 기존의 내부 등록 로직을 그대로 재사용합니다.
            AddOccupiedTiles(map);
            // Debug.Log($"Register: 맵 {map.name}의 점유 정보 등록 완료.");
        }
        // else: 점유 등록 시 CanDungeon 상태 변경은 AddOccupiedTiles 내부에서 처리되므로, 
        //       여기서는 별도로 호출할 필요 없음.
    }
    // 마우스 위치를 그리드 좌표로 변환하여 반환
    public Vector2Int GetGridCoordinates(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - gridOrigin;
        int x = Mathf.RoundToInt(relativePos.x / gridSize.x);
        int z = Mathf.RoundToInt(relativePos.z / gridSize.z);
        return new Vector2Int(x, z);
    }

    // 유효성 검사: 겹치거나 그리드 밖으로 벗어나는지 확인
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
                // 현재 검사 중인 맵 자신이 아니라 다른 맵이 점유하고 있다면 유효하지 않음
                if (occupiedTiles[gridCoords] != map.transform)
                {
                    return false; // 다른 스몰맵과 겹치므로 유효하지 않음
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
            // 던전 타일과 겹침이 있었다면, 모든 타일이 유효한 위치에 있는지 재확인
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
            return true; // 모든 검사 통과
        }
    }

    // 최종 위치에 맵을 스냅하고 점유 상태 업데이트
    public void SnapAndPlace(SmallMap map)
    {
        Vector3 currentMouseWorldPos = map.transform.position; // 마우스에서 최종적으로 놓인 위치
        Vector2Int originCoords = GetGridCoordinates(currentMouseWorldPos);
        Vector3 snappedPos = GetWorldPosition(originCoords); // 그리드에 맞춰 스냅될 잠정적 위치

        // 유효성 검사를 먼저 수행합니다.
        bool isValidPlacement = IsPlacementValid(map, currentMouseWorldPos);

        // Debug.Log($"SnapAndPlace: 맵 {map.name} 위치 {currentMouseWorldPos} 에서 유효성 검사. 결과: {isValidPlacement}");

        // 1. 제거 로직: 일단 현재 맵이 점유하고 있던 모든 타일 정보를 제거합니다.
        // 이 시점에서 CanDungeon 상태가 업데이트됩니다.
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

            if (hasContact) // 던전 타일 위에 놓으려는 경우
            {
                map.transform.position = snappedPos; // 그리드에 맞춰 스냅된 위치로 배치
                // 새로운 위치 정보를 occupiedTiles에 추가하고, 이 내부에서 CanDungeon을 업데이트합니다.
                AddOccupiedTiles(map);
            }
            else // 던전 타일 외부에 놓으려는 경우
            {
                // 맵의 transform.position은 이미 currentMouseWorldPos에 있습니다.
                // 던전 외부는 occupiedTiles에 등록하지 않습니다. (RemoveOccupiedTiles에서 이미 해제되었으므로 추가 작업 불필요)
            }
        }
        else // IsPlacementValid가 false인 경우 (유효하지 않은 배치: 겹침, 범위를 벗어남 등)
        {
            map.transform.position = offGridPosition; // OffGridPosition으로 이동
            // RemoveOccupiedTiles에서 이미 점유가 해제되었고, AddOccupiedTiles가 호출되지 않았으므로 
            // 현재 상태 그대로 유지됩니다.
        }
    }

    /// <summary>
    /// SmallMap이 점유했던 모든 그리드 타일 정보를 Dictionary에서 제거합니다.
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

        // ==========================================================
        // [추가된 기능] 점유 해제 후, 핵심 타일의 상태를 즉시 업데이트합니다.
        // OCP: 기존 로직을 보호하고, 새 기능을 확장합니다.
        // ==========================================================
        UpdateCanDungeonState();
    }

    /// <summary>
    /// SmallMap이 현재 위치에서 점유하는 모든 그리드 타일 정보를 Dictionary에 등록합니다.
    /// </summary>
    /// <param name="map">점유를 등록할 SmallMap 인스턴스</param>
    private void AddOccupiedTiles(SmallMap map)
    {
        // SmallMap을 구성하는 모든 타일 오프셋을 순회하며 점유 상태를 등록합니다.
        foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
        {
            Vector3 worldTilePos = map.transform.position + tileOffset;
            Vector2Int gridCoords = GetGridCoordinates(worldTilePos);
            occupiedTiles[gridCoords] = map.transform;
        }

        // ==========================================================
        // [추가된 기능] 점유 등록 후, 핵심 타일의 상태를 즉시 업데이트합니다.
        // OCP: 기존 로직을 보호하고, 새 기능을 확장합니다.
        // ==========================================================
        UpdateCanDungeonState();
    }

    // ==========================================================
    // [추가된 기능] 핵심 목표 타일 점유 상태를 업데이트하는 메서드 (SRP)
    // ==========================================================
    /// <summary>
    /// 핵심 목표 타일(coreObjectiveCoords)이 현재 SmallMap에 의해 점유되었는지 확인하고
    /// CanDungeon 변수의 상태를 업데이트합니다.
    /// </summary>
    private void UpdateCanDungeonState()
    {
        // 핵심 타일이 인스펙터에 할당되지 않았다면 상태를 False로 유지하고 종료합니다.
        if (coreObjectiveTile == null)
        {
            CanDungeon = false;
            return;
        }

        // occupiedTiles 딕셔너리에 핵심 타일의 좌표(Key)가 존재하는지 확인합니다.
        // 하나라도 점유되어 있다면 True입니다.
        bool isOccupied = occupiedTiles.ContainsKey(coreObjectiveCoords);

        // CanDungeon 속성(Property)을 통해 값을 안전하게 설정합니다.
        CanDungeon = isOccupied;

        // Debug.Log($"[Dungeon State] CanDungeon 상태 업데이트됨: {CanDungeon}");
    }
    // ==========================================================

    /// <summary>
    /// 그리드 좌표(Vector2Int)를 월드 위치(Vector3)로 변환하여 반환합니다.
    /// </summary>
    /// <param name="gridCoords">변환할 그리드 좌표</param>
    /// <returns>그리드 중심의 월드 위치</returns>
    private Vector3 GetWorldPosition(Vector2Int gridCoords)
    {
        float x = gridCoords.x * gridSize.x + gridOrigin.x;
        float z = gridCoords.y * gridSize.z + gridOrigin.z;
        return new Vector3(x, 0, z);
    }
}