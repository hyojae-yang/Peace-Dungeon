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

    // 드래그 시작 위치를 저장할 변수 (로직은 사용하지만 제한 기준점은 아님)
    private Vector3 dragStartPosition;

    [Header("드래그 제한 설정 (고정된 월드맵 영역)")]
    [Tooltip("드래그 제한 영역의 월드 좌표 중심점입니다. 이 위치를 기준으로 경계가 생성됩니다.")]
    // 드래그 제한의 고정된 중심점 월드 좌표
    [SerializeField] private Vector3 dragLimitCenter = Vector3.zero;

    [Tooltip("중심점에서 X, Y, Z축으로 각각 허용되는 최대 이동 거리입니다. (맵 경계 크기)")]
    // 중심점으로부터의 범위 (Extent)
    [SerializeField] private Vector3 dragLimitExtent = new Vector3(500f, 0f, 350f);

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

        smallMapItem = GetComponent<SmallMapItem>();
    }

    private void Start()
    {
        // Y축 제한은 필요 없는 경우가 많으므로 안전장치
        if (dragLimitExtent.y < 0) dragLimitExtent.y = 0;
    }

    private void Update()
    {
        if (MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return;
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

            Vector3 targetPosition = GetMouseWorldPosition();

            // 고정된 중심점을 기준으로 직사각형 드래그 제한 로직 적용

            // 1. 목표 위치와 제한 중심점(dragLimitCenter) 간의 상대적 이동 거리 계산
            Vector3 offsetFromCenter = targetPosition - dragLimitCenter;

            // 2. 각 축(X, Y, Z)별로 제한 범위 내에 있는지 확인하고 보정 (Clamp)
            // 즉, offsetFromCenter의 각 요소가 -Extent와 +Extent 사이에 있도록 제한합니다.
            offsetFromCenter.x = Mathf.Clamp(offsetFromCenter.x, -dragLimitExtent.x, dragLimitExtent.x);
            offsetFromCenter.y = Mathf.Clamp(offsetFromCenter.y, -dragLimitExtent.y, dragLimitExtent.y);
            offsetFromCenter.z = Mathf.Clamp(offsetFromCenter.z, -dragLimitExtent.z, dragLimitExtent.z);

            // 3. 제한된 오프셋을 제한 중심점에 더하여 최종 위치를 설정
            targetPosition = dragLimitCenter + offsetFromCenter;

            transform.position = targetPosition;
        }
    }

    // (OnEnable, OnDisable 메서드는 변경 없이 유지)
    private void OnEnable()
    {
        if (DungeonMap.Instance != null)
        {
            DungeonMap.Instance.RegisterOccupiedTiles(this);
        }
    }

    private void OnDisable()
    {
        if (DungeonMap.Instance != null)
        {
            DungeonMap.Instance.DeregisterOccupiedTiles(this);
        }
    }

    private void OnMouseDown()
    {
        if (MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return;
        }

        if (Time.time - lastClickTime < doubleClickTime)
        {
            if (smallMapItem != null)
            {
                smallMapItem.HandleDoubleClick();
            }
            return;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Map_Grab, 0.5f);
        }
        TestSenser.tt = false;

        // 드래그 시작 위치 저장 (나중에 스냅 시 필요할 수 있으나, 제한 중심점으로는 사용하지 않음)
        dragStartPosition = transform.position;

        isDragging = true;
        transform.position += new Vector3(0, 1f, 0);
        lastClickTime = Time.time;
    }

    private void OnMouseUp()
    {
        // 드래그 시작 시 isDragging을 true로 설정했으므로,
        // 드래그가 유효하게 시작됐는지 확인합니다.
        // 그리고 드래그가 실제로 시작된 경우에만 배치 시도 및 사운드를 재생합니다.
        if (!isDragging)
        {
            return; // 드래그 상태가 아니었다면 아무것도 하지 않고 종료
        }

        // isDragging을 false로 바꾸는 것은 유효한 드래그 후 배치 시도 직전에 위치하는 것이 적절합니다.
        isDragging = false;

        // [수정된 로직] 드래그가 끝났을 때만 배치 사운드를 재생합니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Map_Place, 0.5f);
        }

        if (DungeonMap.Instance != null)
        {
            TestSenser.tt = true;
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
        if (!Application.isPlaying)
        {
            DrawGizmoTiles(transform.position);

            // 고정된 중심점을 기준으로 직사각형 드래그 제한 기즈모 표시
            DrawDragLimitBox(dragLimitCenter);
        }
        else
        {
            // 게임 중
            DrawGizmoTiles(transform.position);

            // 고정된 중심점을 기준으로 직사각형 드래그 제한 기즈모 표시
            if (isDragging)
            {
                // 드래그 중이더라도 고정된 dragLimitCenter를 기준으로 박스를 그립니다.
                DrawDragLimitBox(dragLimitCenter);
            }
        }

        // 중심 타일의 위치를 빨간색 큐브로 표시
        Gizmos.color = Color.red;
        Vector3 rotatedOriginTile = transform.rotation * originTile;
        Gizmos.DrawSphere(transform.position + rotatedOriginTile, 10f);
    }

    /// <summary>
    /// 인스펙터에 설정된 dragLimitExtent를 기준으로 직사각형 제한 영역을 기즈모로 그립니다.
    /// </summary>
    /// <param name="center">제한 영역의 중심 위치 (고정된 dragLimitCenter)</param>
    private void DrawDragLimitBox(Vector3 center)
    {
        Gizmos.color = Color.yellow;

        // Size는 Extent의 2배입니다. (Extents는 중심에서 각 면까지의 거리)
        Vector3 size = dragLimitExtent * 2f;

        // Gizmos.DrawWireCube를 사용하여 직사각형(큐브) 형태로 경계를 표시합니다.
        Gizmos.DrawWireCube(center, size);
    }

    // 타일 그리기 로직을 분리하여 OnDrawGizmos에서 재활용합니다.
    private void DrawGizmoTiles(Vector3 currentPosition)
    {
        Gizmos.color = validGizmoColor;
        foreach (Vector3 tile in mapTiles)
        {
            Vector3 rotatedTile = transform.rotation * tile;
            Vector3 gizmoPosition = currentPosition + rotatedTile;
            Gizmos.DrawWireCube(gizmoPosition, tileSize);
        }
    }
}