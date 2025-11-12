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

    /// <summary>
    /// 스크립트가 로드될 때 한 번 호출됩니다.
    /// 메인 카메라 및 SmallMapItem 컴포넌트를 참조합니다.
    /// </summary>
    private void Awake()
    {
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다. 'MainCamera' 태그가 올바르게 설정되었는지 확인하세요.");
        }

        smallMapItem = GetComponent<SmallMapItem>();
    }

    /// <summary>
    /// 오브젝트가 활성화된 프레임에 한 번 호출됩니다.
    /// 초기 드래그 제한 설정과 함께, **맵이 생성되자마자 유효성 검사를 수행**합니다.
    /// </summary>
    private void Start()
    {
        // Y축 제한은 필요 없는 경우가 많으므로 안전장치
        if (dragLimitExtent.y < 0) dragLimitExtent.y = 0;

        // ==========================================================
        // [핵심 수정] 초기 위치 유효성 검사 및 정리 (SOLID: Liskov Substitution Principle 준수)
        // 맵이 생성되는 즉시 SnapAndPlace를 호출하여, 유효하지 않은 위치(occupiedTiles 미등록 상태)라면
        // offGridPosition으로 이동시키고 occupiedTiles에서 미등록 상태를 유지시켜 회수 대상으로 만듭니다.
        // TownMap과 동일한 초기화 행위를 강제합니다.
        // ==========================================================
        if (DungeonMap.Instance != null)
        {
            // 이 로직이 없으면 인벤토리에서 꺼낸 후 드래그하지 않은 맵은 회수되지 않을 수 있습니다.
            DungeonMap.Instance.SnapAndPlace(this);
        }
    }

    /// <summary>
    /// 매 프레임 호출됩니다.
    /// 드래그 상태일 경우 마우스 위치를 추적하며 드래그 제한을 적용하고, 마우스 우클릭 시 맵을 90도 회전시킵니다.
    /// </summary>
    private void Update()
    {
        // 던전 캔버스가 활성화된 경우에만 맵 기능을 사용 가능하도록 제한
        if (MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return;
        }
        if (isDragging)
        {
            // 마우스 우클릭 시 회전
            if (Input.GetMouseButtonDown(1))
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Map_Rotate, 0.5f);
                }
                // 맵 자신을 기준으로 90도 회전
                transform.Rotate(0, 90, 0, Space.Self);
            }

            Vector3 targetPosition = GetMouseWorldPosition();

            // 고정된 중심점을 기준으로 직사각형 드래그 제한 로직 적용

            // 1. 목표 위치와 제한 중심점(dragLimitCenter) 간의 상대적 이동 거리 계산
            Vector3 offsetFromCenter = targetPosition - dragLimitCenter;

            // 2. 각 축(X, Y, Z)별로 제한 범위 내에 있도록 Clamp (보정)
            offsetFromCenter.x = Mathf.Clamp(offsetFromCenter.x, -dragLimitExtent.x, dragLimitExtent.x);
            offsetFromCenter.y = Mathf.Clamp(offsetFromCenter.y, -dragLimitExtent.y, dragLimitExtent.y);
            offsetFromCenter.z = Mathf.Clamp(offsetFromCenter.z, -dragLimitExtent.z, dragLimitExtent.z);

            // 3. 제한된 오프셋을 제한 중심점에 더하여 최종 위치를 설정
            targetPosition = dragLimitCenter + offsetFromCenter;

            transform.position = targetPosition;
        }
    }

    /// <summary>
    /// 오브젝트가 활성화될 때 호출됩니다.
    /// 맵이 그리드에 배치될 수 있도록 DungeonMap에 등록을 시도합니다.
    /// </summary>
    private void OnEnable()
    {
        if (DungeonMap.Instance != null)
        {
            // DungeonMap에 현재 맵의 타일 점유 상태 등록을 요청
            DungeonMap.Instance.RegisterOccupiedTiles(this);
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화되거나 파괴되기 직전 호출됩니다.
    /// DungeonMap에 등록된 점유 상태를 해제하도록 요청합니다.
    /// </summary>
    private void OnDisable()
    {
        if (DungeonMap.Instance != null)
        {
            // DungeonMap에 현재 맵의 타일 점유 상태 해제를 요청
            DungeonMap.Instance.DeregisterOccupiedTiles(this);
        }
    }

    /// <summary>
    /// 마우스 버튼을 누르는 순간 호출됩니다.
    /// 더블클릭 및 드래그 시작 로직을 처리합니다.
    /// </summary>
    private void OnMouseDown()
    {
        if (MainSceneManager.Instance != null && !MainSceneManager.Instance.isDungeonCanvasActive)
        {
            return;
        }

        // 더블클릭 감지
        if (Time.time - lastClickTime < doubleClickTime)
        {
            if (smallMapItem != null)
            {
                smallMapItem.HandleDoubleClick();
            }
            return;
        }

        // 싱글클릭 시 드래그 시작
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Map_Grab, 0.5f);
        }
        TestSenser.tt = false;

        // 드래그 시작 준비
        dragStartPosition = transform.position;

        isDragging = true;
        // 드래그 중임을 시각적으로 표시하기 위해 살짝 위로 이동
        transform.position += new Vector3(0, 1f, 0);
        lastClickTime = Time.time;
    }

    /// <summary>
    /// 마우스 버튼을 떼는 순간 호출됩니다.
    /// 드래그를 종료하고 DungeonMap에 맵의 최종 스냅 및 배치를 요청합니다.
    /// </summary>
    private void OnMouseUp()
    {
        // 드래그 상태가 아니었다면 (예: 더블클릭만 발생한 경우) 종료
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        // 배치 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Map_Place, 0.5f);
        }

        // DungeonMap에 스냅 및 배치 요청 (유효성 검사 및 위치 조정 포함)
        if (DungeonMap.Instance != null)
        {
            TestSenser.tt = true;
            DungeonMap.Instance.SnapAndPlace(this);
        }
    }

    /// <summary>
    /// 현재 맵의 회전을 고려한 타일 위치 목록을 반환합니다.
    /// DungeonMap의 배치 유효성 검사에 사용됩니다.
    /// </summary>
    /// <returns>회전된 로컬 타일 위치의 월드 좌표 목록</returns>
    public List<Vector3> GetRotatedMapTiles()
    {
        List<Vector3> rotatedTiles = new List<Vector3>();
        foreach (Vector3 tile in mapTiles)
        {
            // 현재 맵의 회전을 로컬 타일에 적용
            rotatedTiles.Add(transform.rotation * tile);
        }
        return rotatedTiles;
    }

    /// <summary>
    /// 마우스 커서의 현재 위치를 Raycast를 통해 월드 좌표로 변환하여 반환합니다.
    /// </summary>
    /// <returns>마우스가 바라보는 월드 좌표</returns>
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCam == null)
        {
            return transform.position;
        }

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // "Ground" 레이어 마스크를 가진 오브젝트에 대해서만 Raycast 수행
        if (Physics.Raycast(ray, out hit, 1200f, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }

        return transform.position;
    }

    /// <summary>
    /// 에디터 씬 뷰에서 맵 타일 영역과 드래그 제한 영역을 시각적으로 표시합니다.
    /// </summary>
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

    /// <summary>
    /// 맵이 점유하는 각 타일의 위치를 와이어 큐브로 그립니다.
    /// </summary>
    /// <param name="currentPosition">SmallMap 오브젝트의 현재 월드 위치</param>
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