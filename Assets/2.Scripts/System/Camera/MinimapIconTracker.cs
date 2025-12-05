using UnityEngine;
using UnityEngine.UI;

// [SOLID - SRP 준수]: 월드 오브젝트의 3D 위치를 미니맵 UI의 2D 위치로 변환하여 아이콘을 추적하는 책임만 가집니다.
public class MinimapIconTracker : MonoBehaviour
{
    // [Inspector 할당] 월드에서 위치를 따라다닐 대상
    [Tooltip("월드에서 위치를 따라다닐 대상 (플레이어 또는 몬스터)의 Transform")]
    public Transform target;

    // [핵심 추가] 미니맵 경계에서 아이콘이 사라지기 시작할 여유 공간 (픽셀 단위)
    [Header("Icon Display Settings")]
    [Tooltip("미니맵 경계선 안쪽으로 아이콘이 사라지기 시작할 여유 공간 (픽셀)")]
    [SerializeField] // Inspector에서 조정 가능하게 함
    private float edgeMargin = 10f; // 기본값 10픽셀 설정

    // 씬에서 자동으로 찾아 할당할 변수들입니다.
    private Camera minimapCamera;
    private RawImage minimapDisplay;

    // 아이콘 자체의 RectTransform (UI 좌표 계산에 필요)
    private RectTransform iconRectTransform;

    // 아이콘의 표시 여부를 제어할 Image 컴포넌트
    private Image iconImage;

    void Start()
    {
        // 1. 아이콘의 RectTransform 및 Image 컴포넌트를 미리 캐싱합니다.
        iconRectTransform = GetComponent<RectTransform>();
        iconImage = GetComponent<Image>();

        // 2. 미니맵 Camera 자동 찾기
        GameObject minimapCamObject = GameObject.Find("MinimapCamera");
        if (minimapCamObject != null)
        {
            minimapCamera = minimapCamObject.GetComponent<Camera>();
        }

        // 3. MinimapDisplay RawImage 자동 찾기
        GameObject minimapDisplayObject = GameObject.Find("MinimapDisplay");
        if (minimapDisplayObject != null)
        {
            minimapDisplay = minimapDisplayObject.GetComponent<RawImage>();
        }

        // 팩트 확인: 필수 컴포넌트 확인
        bool isDependencyMissing = minimapCamera == null || minimapDisplay == null || target == null || iconImage == null;

        if (isDependencyMissing)
        {
            Debug.LogError($"MinimapIconTracker: 필수 종속성 참조 실패. 추적기가 비활성화됩니다. (Cam: {minimapCamera == null}, Display: {minimapDisplay == null}, Target: {target == null}, Image: {iconImage == null})");
            this.enabled = false; // 스크립트 비활성화
            if (iconRectTransform != null) this.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 목표의 3D 위치를 계산하여 아이콘의 2D 위치에 반영하고, 미니맵 경계 내에서만 Image를 활성화합니다.
    /// </summary>
    private void LateUpdate()
    {
        // 종속성 오류로 인해 스크립트 자체가 비활성화된 경우 Early Return
        if (target == null || minimapCamera == null || minimapDisplay == null || iconImage == null)
        {
            return;
        }

        // ----------------------------------------------------
        // --- 1. 월드 위치에서 미니맵 상대 위치 계산 (중앙 기준) ---
        // ----------------------------------------------------
        Vector3 cameraPos = minimapCamera.transform.position;
        Vector3 targetPos = target.position;

        Vector2 offset = new Vector2(targetPos.x - cameraPos.x, targetPos.z - cameraPos.z);
        float viewSize = minimapCamera.orthographicSize * 2f;

        float normalizedX = offset.x / viewSize;
        float normalizedY = offset.y / viewSize;

        Rect minimapRect = minimapDisplay.rectTransform.rect;
        float uiWidth = minimapRect.width;
        float uiHeight = minimapRect.height;

        // 중앙(0,0) 기준의 UI 좌표
        float finalPosX = normalizedX * uiWidth;
        float finalPosY = normalizedY * uiHeight;

        float halfWidth = uiWidth / 2f;
        float halfHeight = uiHeight / 2f;


        // ----------------------------------------------------
        // --- 2. 미니맵 내부/외부 판단 및 Image 표시/숨김 (마진 적용) ---
        // ----------------------------------------------------

        // [핵심 변경] 경계 마진(edgeMargin)을 적용하여 실제 영역을 축소합니다.
        float boundedHalfWidth = halfWidth - edgeMargin;
        float boundedHalfHeight = halfHeight - edgeMargin;

        // 마진이 미니맵 크기보다 클 경우 오류 방지
        if (boundedHalfWidth < 0 || boundedHalfHeight < 0)
        {
            Debug.LogError("MinimapIconTracker: edgeMargin이 미니맵 크기의 절반보다 크거나 같습니다. 마진 값을 줄여주세요.");
            // 이 경우, 아이콘이 무조건 비활성화되도록 처리하거나 기본값으로 대체할 수 있습니다.
            iconImage.enabled = false;
            return;
        }

        // 축소된 경계 내에 목표가 있는지 확인
        bool isTargetInsideMinimap =
            Mathf.Abs(finalPosX) <= boundedHalfWidth && Mathf.Abs(finalPosY) <= boundedHalfHeight;

        if (!isTargetInsideMinimap)
        {
            // 목표가 외부에 있다면 Image 컴포넌트만 비활성화 
            if (iconImage.enabled)
            {
                iconImage.enabled = false;
            }
            return; // 연산 중단
        }

        // 목표가 내부에 있다면 Image 컴포넌트 활성화
        if (!iconImage.enabled)
        {
            iconImage.enabled = true;
        }

        // ----------------------------------------------------
        // --- 3. 위치 적용 및 우측 상단 앵커(1, 1) 보정 ---
        // ----------------------------------------------------

        // 1) 좌측 하단(0, 0) 기준 좌표로 변환: 중앙 좌표 + 절반 크기
        float bottomLeftX = finalPosX + halfWidth;
        float bottomLeftY = finalPosY + halfHeight;

        // 2) 우측 상단(1, 1) 앵커 기준으로 변환: (좌측 하단 기준 좌표) - (전체 크기)
        float finalLocalX = bottomLeftX - uiWidth;
        float finalLocalY = bottomLeftY - uiHeight;

        this.iconRectTransform.localPosition = new Vector3(finalLocalX, finalLocalY, 0);

        // --- 4. 아이콘 회전 ---
        float targetYRotation = target.eulerAngles.y;
        this.iconRectTransform.localRotation = Quaternion.Euler(0, 0, -targetYRotation);
    }
}