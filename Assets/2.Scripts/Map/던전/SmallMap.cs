using System.Collections.Generic;
using UnityEngine;

public class SmallMap : MonoBehaviour
{
    private Camera mainCam;
    private bool isDragging = false;

    // ����Ŭ�� ������ ���� ����
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;

    // SmallMapItem ��ũ��Ʈ ����
    private SmallMapItem smallMapItem;

    [Header("Map Tile Data")]
    [SerializeField]
    private List<Vector3> mapTiles = new List<Vector3>();

    [SerializeField]
    private Vector3 originTile = Vector3.zero;

    [SerializeField] private Vector3 tileSize = new Vector3(100f, 1f, 100f);

    private Color validGizmoColor = Color.cyan;
    private Color invalidGizmoColor = Color.red;

    private void Awake()
    {
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("���� ī�޶� ã�� �� �����ϴ�. 'MainCamera' �±װ� �ùٸ��� �����Ǿ����� Ȯ���ϼ���.");
        }

        // SmallMapItem ������Ʈ ����
        smallMapItem = GetComponent<SmallMapItem>();
    }

    private void Update()
    {
        if(MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return; // ���� ĵ������ Ȱ��ȭ�� ��� �巡�� �� ȸ�� ��� Ȱ��ȭ
        }
        if (isDragging)
        {
            if (Input.GetMouseButtonDown(1))
            {
                transform.Rotate(0, 90, 0, Space.Self);
            }

            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition;
        }
    }

    private void OnMouseDown()
    {
        if (MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return; // ���� ĵ������ Ȱ��ȭ�� ��� Ŭ�� ��� Ȱ��ȭ
        }
        // ����Ŭ���� ���� ����
        if (Time.time - lastClickTime < doubleClickTime)
        {
            if (smallMapItem != null)
            {
                smallMapItem.HandleDoubleClick();
            }
            // ����Ŭ�� �� �巡�� ������ �������� ����
            return;
        }
        TestSenser.tt = false; //�׽�Ʈ���;
        // ����Ŭ���� �ƴϸ� ���� �巡�� ���� ���� ����
        isDragging = true;
        transform.position += new Vector3(0, 1f, 0);
        lastClickTime = Time.time;
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // ���콺�� ���� DungeonMap�� ���� �� ��ȿ�� �˻� ��û
        if (DungeonMap.Instance != null)
        {
            TestSenser.tt = true; //�׽�Ʈ���;
            DungeonMap.Instance.SnapAndPlace(this);
        }
    }

    // DungeonMap���� ����� �� �ֵ��� �� Ÿ�� ��� ��ȯ
    public List<Vector3> GetRotatedMapTiles()
    {
        List<Vector3> rotatedTiles = new List<Vector3>();
        foreach (Vector3 tile in mapTiles)
        {
            rotatedTiles.Add(transform.rotation * tile);
        }
        return rotatedTiles;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCam == null)
        {
            return transform.position;
        }

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1200f, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }

        return transform.position;
    }

    private void OnDrawGizmos()
    {
        // OnDrawGizmos()�� �����Ϳ����� ȣ��
        if (!isDragging || DungeonMap.Instance == null)
        {
            // �巡�� ���� �ƴϸ� ��Ҵ�� ť�� �׸���
            Gizmos.color = validGizmoColor;
            foreach (Vector3 tile in mapTiles)
            {
                Vector3 rotatedTile = transform.rotation * tile;
                Vector3 gizmoPosition = transform.position + rotatedTile;
                Gizmos.DrawWireCube(gizmoPosition, tileSize);
            }
        }
        else
        {
            // ȸ���� �� Ÿ�� �׸���
            foreach (Vector3 tile in GetRotatedMapTiles())
            {
                Vector3 gizmoPosition = transform.position + tile;
                Gizmos.DrawWireCube(gizmoPosition, tileSize);
            }
        }

        // �߽� Ÿ���� ��ġ�� ������ ť��� ǥ��
        Gizmos.color = Color.red;
        Vector3 rotatedOriginTile = transform.rotation * originTile;
        Gizmos.DrawSphere(transform.position + rotatedOriginTile, 10f);
    }
}