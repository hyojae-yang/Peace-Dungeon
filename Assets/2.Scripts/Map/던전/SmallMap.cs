using System.Collections.Generic;
using UnityEngine;

public class SmallMap : MonoBehaviour
{
    private Camera mainCam;
    private bool isDragging = false;

    // 더블클릭 감지를 위한 변수
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;

    // SmallMapItem 스크립트 참조
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
            Debug.LogError("메인 카메라를 찾을 수 없습니다. 'MainCamera' 태그가 올바르게 설정되었는지 확인하세요.");
        }

        // SmallMapItem 컴포넌트 참조
        smallMapItem = GetComponent<SmallMapItem>();
    }

    private void Update()
    {
        if(MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return; // 던전 캔버스가 활성화된 경우 드래그 및 회전 기능 활성화
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
            return; // 던전 캔버스가 활성화된 경우 클릭 기능 활성화
        }
        // 더블클릭을 먼저 감지
        if (Time.time - lastClickTime < doubleClickTime)
        {
            if (smallMapItem != null)
            {
                smallMapItem.HandleDoubleClick();
            }
            // 더블클릭 시 드래그 로직은 실행하지 않음
            return;
        }
        TestSenser.tt = false; //테스트모드;
        // 더블클릭이 아니면 기존 드래그 시작 로직 실행
        isDragging = true;
        transform.position += new Vector3(0, 1f, 0);
        lastClickTime = Time.time;
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // 마우스를 떼면 DungeonMap에 스냅 및 유효성 검사 요청
        if (DungeonMap.Instance != null)
        {
            TestSenser.tt = true; //테스트모드;
            DungeonMap.Instance.SnapAndPlace(this);
        }
    }

    // DungeonMap에서 사용할 수 있도록 맵 타일 목록 반환
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
        // OnDrawGizmos()는 에디터에서만 호출
        if (!isDragging || DungeonMap.Instance == null)
        {
            // 드래그 중이 아니면 평소대로 큐브 그리기
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
            // 회전된 맵 타일 그리기
            foreach (Vector3 tile in GetRotatedMapTiles())
            {
                Vector3 gizmoPosition = transform.position + tile;
                Gizmos.DrawWireCube(gizmoPosition, tileSize);
            }
        }

        // 중심 타일의 위치를 빨간색 큐브로 표시
        Gizmos.color = Color.red;
        Vector3 rotatedOriginTile = transform.rotation * originTile;
        Gizmos.DrawSphere(transform.position + rotatedOriginTile, 10f);
    }
}