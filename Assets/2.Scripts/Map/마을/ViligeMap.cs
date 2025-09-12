using System.Collections.Generic;
using UnityEngine;

public class ViligeMap : MonoBehaviour
{
    // 싱글턴 패턴을 위한 인스턴스
    public static ViligeMap Instance { get; private set; }

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
    public bool IsPlacementValid(TownMap map, Vector3 targetWorldPos)
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

                // 핵심 수정: 스몰맵의 모든 타일이 던전 범위 안에 있는지 확인
                if (!validDungeonTileCoords.Contains(gridCoords))
                {
                    return false; // 하나라도 던전 범위를 벗어나면 유효하지 않음
                }
            }
            return true; // 모든 검사 통과
        }
    }

    // 최종 위치에 맵을 스냅하고 점유 상태 업데이트
    public void SnapAndPlace(TownMap map)
    {
        Vector3 currentMouseWorldPos = map.transform.position; // 마우스에서 최종적으로 놓인 위치
        Vector2Int originCoords = GetGridCoordinates(currentMouseWorldPos);
        Vector3 snappedPos = GetWorldPosition(originCoords); // 그리드에 맞춰 스냅될 잠정적 위치

        // 유효성 검사를 먼저 수행합니다.
        bool isValidPlacement = IsPlacementValid(map, currentMouseWorldPos);

        // Debug.Log($"SnapAndPlace: 맵 {map.name} 위치 {currentMouseWorldPos} 에서 유효성 검사. 결과: {isValidPlacement}");

        // 1. 제거 로직: 일단 현재 맵이 점유하고 있던 모든 타일 정보를 제거합니다.
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
                AddOccupiedTiles(map); // 새로운 위치 정보를 occupiedTiles에 추가
            }
            else // 던전 타일 외부에 놓으려는 경우
            {
                // 맵의 transform.position은 이미 currentMouseWorldPos에 있습니다.
                // 던전 외부는 occupiedTiles에 등록하지 않습니다.
            }
        }
        else // IsPlacementValid가 false인 경우 (유효하지 않은 배치: 겹침, 범위를 벗어남 등)
        {
            map.transform.position = offGridPosition; // OffGridPosition으로 이동
        }
    }

    private void RemoveOccupiedTiles(TownMap map)
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var pair in occupiedTiles)
        {
            if (pair.Value == map.transform)
            {
                toRemove.Add(pair.Key);
            }
        }
        foreach (var key in toRemove)
        {
            occupiedTiles.Remove(key);
        }
    }

    private void AddOccupiedTiles(TownMap map)
    {
        foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
        {
            Vector3 worldTilePos = map.transform.position + tileOffset;
            Vector2Int gridCoords = GetGridCoordinates(worldTilePos);
            occupiedTiles[gridCoords] = map.transform;
        }
    }

    private Vector3 GetWorldPosition(Vector2Int gridCoords)
    {
        float x = gridCoords.x * gridSize.x + gridOrigin.x;
        float z = gridCoords.y * gridSize.z + gridOrigin.z;
        return new Vector3(x, 0, z);
    }
}