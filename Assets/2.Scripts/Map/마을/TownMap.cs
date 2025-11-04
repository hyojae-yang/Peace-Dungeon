using System.Collections.Generic;
using UnityEngine;

public class TownMap : MonoBehaviour
{
    private Camera mainCam;
    private bool isDragging = false;

    // 더블클릭 감지를 위한 변수
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;

    // SmallMapItem 스크립트 참조
    private SmallMapItem smallMapItem;

    // **[추가]** 드래그 제한 영역 설정
    [Header("드래그 제한 설정 (고정된 월드맵 영역)")]
    [Tooltip("드래그 제한 영역의 월드 좌표 중심점입니다. 이 위치를 기준으로 경계가 생성됩니다.")]
    // 고정된 제한 영역의 중심 좌표 (사용자 조정)
    [SerializeField] private Vector3 dragLimitCenter = Vector3.zero;

    [Tooltip("중심점에서 X, Y, Z축으로 각각 허용되는 최대 이동 거리입니다. (맵 경계 크기)")]
    // 중심점으로부터의 범위 (사용자 조정)
    [SerializeField] private Vector3 dragLimitExtent = new Vector3(450f, 0f, 350f);

    [Header("Map Tile Data")]
    [SerializeField]
    private List<Vector3> mapTiles = new List<Vector3>();

    [SerializeField]
    private Vector3 originTile = Vector3.zero;

    [SerializeField] private Vector3 tileSize = new Vector3(100f, 1f, 100f);

    private Color validGizmoColor = Color.cyan;
    // 기즈모를 그릴 때 DungeonMap 대신 TownMap 또는 ViligeMap을 사용합니다.

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

    private void Start()
    {
        // Y축 제한은 3D 지면 Raycast로 인해 필요 없는 경우가 많으므로 안전장치
        if (dragLimitExtent.y < 0) dragLimitExtent.y = 0;
    }

    private void Update()
    {
        if (MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
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

            Vector3 targetPosition = GetMouseWorldPosition();

            // **[핵심 수정]** 고정된 중심점(dragLimitCenter)을 기준으로 직사각형 드래그 제한 로직 적용

            // 1. 목표 위치와 제한 중심점(dragLimitCenter) 간의 상대적 이동 거리 계산
            Vector3 offsetFromCenter = targetPosition - dragLimitCenter;

            // 2. 각 축(X, Y, Z)별로 제한 범위 내에 있는지 확인하고 보정 (Clamp)
            // 즉, TownMap의 중앙이 dragLimitCenter를 기준으로 dragLimitExtent를 벗어나지 않도록 강제합니다.
            offsetFromCenter.x = Mathf.Clamp(offsetFromCenter.x, -dragLimitExtent.x, dragLimitExtent.x);
            offsetFromCenter.y = Mathf.Clamp(offsetFromCenter.y, -dragLimitExtent.y, dragLimitExtent.y);
            offsetFromCenter.z = Mathf.Clamp(offsetFromCenter.z, -dragLimitExtent.z, dragLimitExtent.z);

            // 3. 제한된 오프셋을 제한 중심점에 더하여 최종 위치를 설정
            targetPosition = dragLimitCenter + offsetFromCenter;

            transform.position = targetPosition;
        }
    }
    // --- [추가된 생명 주기 연동 로직] ---

    /// <summary>
    /// 오브젝트가 활성화될 때 호출됩니다.
    /// WorldStateSaver.LoadData에 의해 새 오브젝트가 Instantiate 될 때 호출되어
    /// ViligeMap에 자신의 점유 상태를 등록하도록 요청합니다.
    /// </summary>
    private void OnEnable()
    {
        // OCP: 기존 OnMouseUp 로직을 침해하지 않고 로드 시나리오만 처리합니다.
        if (ViligeMap.Instance != null)
        {
            // 이 시점에 transform.position은 이미 로드 데이터에 의해 설정된 상태입니다.
            ViligeMap.Instance.RegisterOccupiedTiles(this);
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화되거나 파괴되기 직전 호출됩니다.
    /// WorldStateSaver.LoadData에 의해 기존 오브젝트가 Destroy 될 때 호출되어
    /// ViligeMap에서 자신의 점유 상태를 해제하도록 요청합니다.
    /// </summary>
    private void OnDisable()
    {
        // WorldStateSaver가 기존 맵을 Destroy할 때 호출됩니다.
        if (ViligeMap.Instance != null)
        {
            ViligeMap.Instance.DeregisterOccupiedTiles(this);
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
        // 드래그 시작 시 위치를 잠시 올립니다.
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
        // 마우스를 떼면 ViligeMap에 스냅 및 유효성 검사 요청
        if (ViligeMap.Instance != null)
        {
            TestSenser.tt = true; //테스트모드;
            ViligeMap.Instance.SnapAndPlace(this);
        }
    }

    // ViligeMap에서 사용할 수 있도록 맵 타일 목록 반환
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

        if (Physics.Raycast(ray, out hit, 1300f, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }

        return transform.position;
    }

    /// <summary>
    /// 기즈모를 통해 맵 타일과 드래그 제한 영역을 시각화합니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        // 맵 타일 그리기
        DrawGizmoTiles(transform.position);

        // **[핵심 적용]** 고정된 중심점(dragLimitCenter)을 기준으로 직사각형 드래그 제한 기즈모 표시
        // 에디터/게임 플레이 상태와 무관하게 고정된 위치에 경계를 그립니다.
        DrawDragLimitBox(dragLimitCenter);

        // 중심 타일의 위치를 빨간색 큐브로 표시
        // TownMap에 originTile이 정의되어 있다면 표시합니다.
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
        // 기즈모 색상 및 행렬 설정
        Gizmos.color = Color.yellow;

        // Size는 Extent의 2배입니다. (Extents는 중심에서 각 면까지의 거리)
        Vector3 size = dragLimitExtent * 2f;

        // Gizmos.DrawWireCube를 사용하여 직사각형(큐브) 형태로 경계를 표시합니다.
        Gizmos.DrawWireCube(center, size);
    }

    /// <summary>
    /// 현재 TownMap이 점유하는 타일 영역을 와이어 큐브로 그립니다.
    /// </summary>
    /// <param name="currentPosition">TownMap 오브젝트의 현재 위치</param>
    private void DrawGizmoTiles(Vector3 currentPosition)
    {
        Gizmos.color = validGizmoColor;
        // ViligeMap.Instance가 null일 경우 안전하게 처리
        bool isMapInstanceAvailable = ViligeMap.Instance != null;

        foreach (Vector3 tile in mapTiles)
        {
            // 현재 TownMap의 회전을 고려한 타일 위치
            Vector3 rotatedTile = transform.rotation * tile;
            Vector3 gizmoPosition = currentPosition + rotatedTile;

            // 드래그 중이 아니거나, ViligeMap이 없으면 기본 색상 사용
            Gizmos.DrawWireCube(gizmoPosition, tileSize);
        }
    }
}