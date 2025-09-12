using System.Collections.Generic;
using UnityEngine;

public class ViligeMap : MonoBehaviour
{
    // �̱��� ������ ���� �ν��Ͻ�
    public static ViligeMap Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField]
    private Vector3 gridSize = new Vector3(100f, 1f, 100f);
    [SerializeField]
    private Transform gridOriginTile; // �׸��� ���� Ÿ���� ���� �Ҵ�

    private Vector3 gridOrigin; // ��ũ��Ʈ�� ���������� ����� �׸��� ����
    [SerializeField]
    private Vector3 offGridPosition = new Vector3(-9999f, 0f, 0f);

    // 1�ܰ�: �ν����Ϳ��� �Ҵ��� ���� Ÿ�ϵ��� ������ �迭
    [Header("���� Ÿ�� ����")]
    [SerializeField]
    private Transform[] dungeonTiles; // �̸� ��ġ�� ���� Ÿ�ϵ��� ���� �迭

    // ��ġ�� �ٸ� ������ Ÿ�ϵ��� ��ġ�� ������Ʈ�� �����ϴ� ��ųʸ�
    // (���� Ÿ�� ��ü�� ���⿡ ���Ե��� �ʽ��ϴ�. ��ȿ�� '����'�� ǥ���ϱ� ���� ���)
    private Dictionary<Vector2Int, Transform> occupiedTiles = new Dictionary<Vector2Int, Transform>();

    // ���� Ÿ�ϵ��� �׸��� ��ǥ�� ������ HashSet (���� �˻���)
    private HashSet<Vector2Int> validDungeonTileCoords = new HashSet<Vector2Int>();

    private void Awake()
    {
        // �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // �̹� �ٸ� �ν��Ͻ��� �ִٸ� ���� ���� ������Ʈ �ı� (�̱��� ��Ģ)
            Destroy(gameObject);
            return; // ���� �ڵ�� �������� ����
        }
        // �׸��� ���� Ÿ���� ��ġ�� ���� �׸��� �������� ����
        if (gridOriginTile != null)
        {
            gridOrigin = gridOriginTile.position;
        }
        else
        {
            Debug.LogError("Grid Origin Tile�� �Ҵ���� �ʾҽ��ϴ�. Grid Origin Tile�� Inspector���� �������ּ���.");
        }

        // 1�ܰ�: �ν����Ϳ��� �Ҵ�� ���� Ÿ�ϵ��� �׸��� ��ǥ�� validDungeonTileCoords�� ����
        if (dungeonTiles == null || dungeonTiles.Length == 0)
        {
            Debug.LogError("Dungeon Tiles �迭�� ����ֽ��ϴ�. Inspector���� ���� Ÿ�ϵ��� �Ҵ����ּ���.");
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

    // ���콺 ��ġ�� �׸��� ��ǥ�� ��ȯ�Ͽ� ��ȯ
    public Vector2Int GetGridCoordinates(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - gridOrigin;
        int x = Mathf.RoundToInt(relativePos.x / gridSize.x);
        int z = Mathf.RoundToInt(relativePos.z / gridSize.z);
        return new Vector2Int(x, z);
    }

    // ��ȿ�� �˻�: ��ġ�ų� �׸��� ������ ������� Ȯ��
    public bool IsPlacementValid(TownMap map, Vector3 targetWorldPos)
    {
        // �������� Ÿ�� �� �ϳ��� ���� Ÿ�ϰ� ��ġ���� Ȯ���ϴ� �÷���
        bool hasContactWithDungeon = false;

        // �������� �� Ÿ�Ͽ� ���� �˻�
        foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
        {
            Vector3 worldTilePos = targetWorldPos + tileOffset;
            Vector2Int gridCoords = GetGridCoordinates(worldTilePos);

            // 1�� �˻�: �������� Ÿ�� �� �ϳ��� ���� Ÿ�ϰ� ��ġ���� Ȯ��
            if (validDungeonTileCoords.Contains(gridCoords))
            {
                hasContactWithDungeon = true;
            }

            // 2�� �˻�: �̹� �ٸ� �������� ������ ��ġ���� Ȯ��
            if (occupiedTiles.ContainsKey(gridCoords))
            {
                if (occupiedTiles[gridCoords] != map.transform)
                {
                    return false; // �ٸ� �����ʰ� ��ġ�Ƿ� ��ȿ���� ����
                }
            }
        }

        // ���� ����: ���� Ÿ�ϰ� ���� ��ġ�� �ʾҴٸ�
        if (!hasContactWithDungeon)
        {
            return true; // ��ȿ�� �˻� ��� (���� ��ġ ���)
        }
        else
        {
            // ���� Ÿ�ϰ� ��ħ�� �־��ٸ�, ��� Ÿ���� ��ȿ�� ��ġ�� �ִ��� ��Ȯ��
            foreach (Vector3 tileOffset in map.GetRotatedMapTiles())
            {
                Vector3 worldTilePos = targetWorldPos + tileOffset;
                Vector2Int gridCoords = GetGridCoordinates(worldTilePos);

                // �ٽ� ����: �������� ��� Ÿ���� ���� ���� �ȿ� �ִ��� Ȯ��
                if (!validDungeonTileCoords.Contains(gridCoords))
                {
                    return false; // �ϳ��� ���� ������ ����� ��ȿ���� ����
                }
            }
            return true; // ��� �˻� ���
        }
    }

    // ���� ��ġ�� ���� �����ϰ� ���� ���� ������Ʈ
    public void SnapAndPlace(TownMap map)
    {
        Vector3 currentMouseWorldPos = map.transform.position; // ���콺���� ���������� ���� ��ġ
        Vector2Int originCoords = GetGridCoordinates(currentMouseWorldPos);
        Vector3 snappedPos = GetWorldPosition(originCoords); // �׸��忡 ���� ������ ������ ��ġ

        // ��ȿ�� �˻縦 ���� �����մϴ�.
        bool isValidPlacement = IsPlacementValid(map, currentMouseWorldPos);

        // Debug.Log($"SnapAndPlace: �� {map.name} ��ġ {currentMouseWorldPos} ���� ��ȿ�� �˻�. ���: {isValidPlacement}");

        // 1. ���� ����: �ϴ� ���� ���� �����ϰ� �ִ� ��� Ÿ�� ������ �����մϴ�.
        RemoveOccupiedTiles(map);

        if (isValidPlacement)
        {
            // 2. ��ġ ����: ��ȿ�� ��ġ�� ��� (���� ���� �Ǵ� �ܺ�)
            // hasContact�� �ٽ� ����Ͽ� ���� Ÿ�ϰ��� ���� ���θ� Ȯ���մϴ�.
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

            if (hasContact) // ���� Ÿ�� ���� �������� ���
            {
                map.transform.position = snappedPos; // �׸��忡 ���� ������ ��ġ�� ��ġ
                AddOccupiedTiles(map); // ���ο� ��ġ ������ occupiedTiles�� �߰�
            }
            else // ���� Ÿ�� �ܺο� �������� ���
            {
                // ���� transform.position�� �̹� currentMouseWorldPos�� �ֽ��ϴ�.
                // ���� �ܺδ� occupiedTiles�� ������� �ʽ��ϴ�.
            }
        }
        else // IsPlacementValid�� false�� ��� (��ȿ���� ���� ��ġ: ��ħ, ������ ��� ��)
        {
            map.transform.position = offGridPosition; // OffGridPosition���� �̵�
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