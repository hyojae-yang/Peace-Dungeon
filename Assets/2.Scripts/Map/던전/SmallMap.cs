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
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Map_Rotate, 0.5f);
                }
                transform.Rotate(0, 90, 0, Space.Self);
            }

            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition;
        }
    }
    // --- [추가된 생명 주기 연동 로직] ---

    /// <summary>
    /// 오브젝트가 활성화될 때 호출됩니다.
    /// WorldStateSaver.LoadData에 의해 새 오브젝트가 Instantiate 될 때 호출되어
    /// DungeonMap에 자신의 점유 상태를 등록하도록 요청합니다.
    /// </summary>
    private void OnEnable()
    {
        // 로드 직후 isDragging 상태가 아닐 때만 실행됩니다.
        // OCP: 기존 OnMouseUp 로직을 침해하지 않고 로드 시나리오만 처리합니다.
        if (DungeonMap.Instance != null)
        {
            // 이 시점에 transform.position은 이미 로드 데이터에 의해 설정된 상태입니다.
            DungeonMap.Instance.RegisterOccupiedTiles(this);
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화되거나 파괴되기 직전 호출됩니다.
    /// WorldStateSaver.LoadData에 의해 기존 오브젝트가 Destroy 될 때 호출되어
    /// DungeonMap에서 자신의 점유 상태를 해제하도록 요청합니다.
    /// </summary>
    private void OnDisable()
    {
        // WorldStateSaver가 기존 맵을 Destroy할 때 호출됩니다.
        if (DungeonMap.Instance != null)
        {
            DungeonMap.Instance.DeregisterOccupiedTiles(this);
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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Map_Grab, 0.5f);
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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Map_Place, 0.5f);
        }
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