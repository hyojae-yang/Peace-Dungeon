using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List를 사용하기 위해 추가

// [SOLID - SRP/OCP 준수]: 이 스크립트(추적기 관리자)는 추적 대상 목록을 관리하고, 모든 대상에 대해 LateUpdate 로직을 반복 수행합니다.
public class MinimapTargetArrowTracker : MonoBehaviour
{
    // [Inspector 할당] 화살표가 가리켜야 할 모든 목표와 아이콘을 묶어서 관리합니다.
    [System.Serializable] // 인스펙터에 표시되도록 설정
    public struct TargetArrowInfo
    {
        [Tooltip("추적할 3D 오브젝트의 Transform을 할당하세요.")]
        public Transform target;

        [Tooltip("해당 목표를 가리킬 2D 화살표 아이콘의 RectTransform을 할당하세요.")]
        public RectTransform arrowIconRectTransform;
    }

    [Header(" 목표 추적 설정")]
    [Tooltip("추적할 목표의 수만큼 요소를 추가하고 Transform과 아이콘을 할당하세요.")]
    public List<TargetArrowInfo> targetList = new List<TargetArrowInfo>();

    // *설정: 가장자리에서 5~10만큼의 여백을 주기 위한 패딩 값 (8.0f 사용)
    private const float EDGE_PADDING = 8.0f;

    // 씬에서 자동으로 찾아 할당할 변수들
    private Camera minimapCamera;
    private RawImage minimapDisplay;

    private bool dependencyError = false;

    // =======================================================
    // [핵심 추가] 던전 이벤트 구독/해제 로직
    // =======================================================
    private void OnEnable()
    {
        // OnEnable: 스크립트가 활성화될 때 (Start 전에 호출) 이벤트를 구독하여 연결합니다.
        // 스크립트의 enabled 상태와 관계없이 이벤트는 작동합니다.
        DungeonManager.OnDungeonEnter += SetAllArrowsVisible;
        DungeonManager.OnDungeonExit += SetAllArrowsInvisible;
    }

    private void OnDisable()
    {
        // OnDisable: 스크립트가 비활성화될 때 (종료 포함) 이벤트를 해제합니다.
        DungeonManager.OnDungeonEnter -= SetAllArrowsVisible;
        DungeonManager.OnDungeonExit -= SetAllArrowsInvisible;

        // 스크립트 비활성화 시 잔상을 방지하기 위해 모두 숨깁니다.
        SetAllArrowsInvisible();
    }

    /// <summary> 던전 진입 시 호출되며, 모든 화살표 GameObject를 활성화합니다. </summary>
    private void SetAllArrowsVisible()
    {
        // 화살표 아이콘이 켜지면, LateUpdate가 위치 계산을 시작합니다.
        SetAllArrowsActive(true);
    }

    /// <summary> 던전 퇴장 시 호출되며, 모든 화살표 GameObject를 비활성화합니다. </summary>
    private void SetAllArrowsInvisible()
    {
        // 화살표 아이콘이 꺼지면, LateUpdate 계산이 return 문에 의해 스킵됩니다.
        SetAllArrowsActive(false);
    }

    /// <summary> 모든 화살표 아이콘의 GameObject 활성화 상태를 일괄적으로 설정합니다. </summary>
    private void SetAllArrowsActive(bool isActive)
    {
        foreach (var info in targetList)
        {
            // ArrowIconRectTransform이 할당되지 않은 경우를 대비하여 Null 체크
            if (info.arrowIconRectTransform != null)
            {
                info.arrowIconRectTransform.gameObject.SetActive(isActive);
            }
        }
    }
    // =======================================================

    void Start()
    {
        // ----------------------------------------------------
        // --- 1. 필수 종속성 찾기 ---
        // ----------------------------------------------------

        // 미니맵 Camera 자동 찾기
        GameObject minimapCamObject = GameObject.Find("MinimapCamera");
        if (minimapCamObject != null)
        {
            minimapCamera = minimapCamObject.GetComponent<Camera>();
        }

        // MinimapDisplay RawImage 자동 찾기
        GameObject minimapDisplayObject = GameObject.Find("MinimapDisplay");
        if (minimapDisplayObject != null)
        {
            minimapDisplay = minimapDisplayObject.GetComponent<RawImage>();
        }

        // ----------------------------------------------------
        // --- 2. 오류 체크 및 초기 설정 ---
        // ----------------------------------------------------
        bool hasMissingTarget = false;

        // 추적 목록 유효성 검사
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].target == null || targetList[i].arrowIconRectTransform == null)
            {
                Debug.LogError($"[Minimap Arrow | ERROR] Target List [{i}]의 목표 또는 아이콘이 할당되지 않았습니다.");
                hasMissingTarget = true;
            }
        }

        // 종속성 오류 체크
        if (minimapCamera == null || minimapDisplay == null || hasMissingTarget)
        {
            Debug.LogError($"[Minimap Arrow | ERROR] 필수 종속성 참조 실패. 추적 로직이 비활성화됩니다.");
            dependencyError = true;
            this.enabled = false; // 오류 시에만 LateUpdate를 영구 비활성화
        }
        else
        {
            // [초기 상태 설정]: 스크립트 자체는 활성화 상태를 유지합니다.
            // 던전 밖에서는 화살표가 보이지 않도록 초기 설정합니다.
            SetAllArrowsInvisible();
        }
    }

    /// <summary>
    /// LateUpdate는 매 프레임 모든 목표에 대해 위치, 경계 고정, 회전 로직을 수행합니다.
    /// </summary>
    private void LateUpdate()
    {
        if (dependencyError)
        {
            return;
        }

        // ------------------------------------------------------
        // [핵심 로직] 던전 밖이거나 DungeonManager 인스턴스가 없을 경우, 모든 계산을 스킵합니다.
        // ------------------------------------------------------
        // 이 로직은 스크립트가 활성화(enabled=true)되어 있어도 던전 밖에서는 계산 루프를 돌지 않게 합니다.
        if (DungeonManager.Instance == null || !DungeonManager.Instance.IsInDungeon)
        {
            return;
        }

        // 아래부터는 던전 안에 있을 때만 실행됩니다.

        Rect minimapRect = minimapDisplay.rectTransform.rect;
        float uiWidth = minimapRect.width;
        float uiHeight = minimapRect.height;
        float halfWidth = uiWidth / 2f;
        float halfHeight = uiHeight / 2f;

        // MinimapDisplay의 Pivot 정보는 반복문 밖에서 한 번만 가져옵니다.
        Vector2 pivot = minimapDisplay.rectTransform.pivot;
        float offsetX = (pivot.x - 0.5f) * uiWidth;
        float offsetY = (pivot.y - 0.5f) * uiHeight;


        // ----------------------------------------------------
        // --- 모든 목표에 대한 추적 로직 반복 ---
        // ----------------------------------------------------
        for (int i = 0; i < targetList.Count; i++)
        {
            TargetArrowInfo info = targetList[i];

            // 유효성 체크
            if (info.target == null || info.arrowIconRectTransform == null)
            {
                continue;
            }

            // 1. 위치 계산 및 정규화 (중심 기준)
            Vector3 cameraPos = minimapCamera.transform.position;
            Vector3 targetPos = info.target.position;

            Vector2 currentOffset = new Vector2(targetPos.x - cameraPos.x, targetPos.z - cameraPos.z);
            float viewSize = minimapCamera.orthographicSize * 2f;

            float normalizedX = currentOffset.x / viewSize;
            float normalizedY = currentOffset.y / viewSize;

            float finalPosX = normalizedX * uiWidth;
            float finalPosY = normalizedY * uiHeight;

            // 2. 아이콘 크기 및 경계 계산
            float iconHalfSizeX = info.arrowIconRectTransform.sizeDelta.x / 2f;
            float iconHalfSizeY = info.arrowIconRectTransform.sizeDelta.y / 2f;

            float maxClampX = halfWidth - iconHalfSizeX - EDGE_PADDING;
            float minClampX = -halfWidth + iconHalfSizeX + EDGE_PADDING;

            float maxClampY = halfHeight - iconHalfSizeY - EDGE_PADDING;
            float minClampY = -halfHeight + iconHalfSizeY + EDGE_PADDING;

            // 경계 마진 유효성 검사
            if (maxClampX < minClampX || maxClampY < minClampY)
            {
                // 화살표가 너무 커서 미니맵 패딩 안으로 들어갈 공간이 없을 경우 비활성화
                if (info.arrowIconRectTransform.gameObject.activeSelf)
                {
                    info.arrowIconRectTransform.gameObject.SetActive(false);
                }
                continue;
            }


            // 3. 내부/외부 판단 및 활성화 제어
            // *던전 안에 있을 때만 이 로직이 실행됩니다.*
            bool isTargetInsideMinimap =
                Mathf.Abs(finalPosX) <= maxClampX && Mathf.Abs(finalPosY) <= maxClampY;

            if (isTargetInsideMinimap)
            {
                // [요청 로직 2]: 미니맵 영역 안에 있으면 비활성화
                if (info.arrowIconRectTransform.gameObject.activeSelf)
                {
                    info.arrowIconRectTransform.gameObject.SetActive(false);
                }
                continue; // 다음 목표로 넘어갑니다.
            }

            // 이 지점에 도달하면:
            // 1. 던전 안에 있고 (LateUpdate 초반 체크)
            // 2. 미니맵 영역 밖에 있습니다.

            // [요청 로직 1]: 던전 내에 있고 미니맵 영역 밖에 오브젝트가 있으면 활성화
            if (!info.arrowIconRectTransform.gameObject.activeSelf)
            {
                info.arrowIconRectTransform.gameObject.SetActive(true);
            }

            // 4. 경계 고정 및 위치 적용
            float clampedX = Mathf.Clamp(finalPosX, minClampX, maxClampX);
            float clampedY = Mathf.Clamp(finalPosY, minClampY, maxClampY);

            // 좌표계 강제 보정 적용
            float finalLocalX = clampedX - offsetX;
            float finalLocalY = clampedY - offsetY;

            info.arrowIconRectTransform.localPosition = new Vector3(finalLocalX, finalLocalY, 0);

            // 5. 목표 지향 회전
            Vector3 targetDirection = new Vector3(targetPos.x - cameraPos.x, 0, targetPos.z - cameraPos.z);
            float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            info.arrowIconRectTransform.localRotation = Quaternion.Euler(0, 0, -angle);
        }
    }
}