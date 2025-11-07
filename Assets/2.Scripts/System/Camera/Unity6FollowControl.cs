using UnityEngine;
using Unity.Cinemachine;
// TutorialManager와 UITutorialHandler를 사용하기 위해 필요
// 만약 이 스크립트가 TutorialManager보다 먼저 로드된다면 문제가 발생할 수 있습니다.
// 하지만 씬 구조상 TutorialManager가 먼저 Awake/Start를 수행한다고 가정합니다. 

/// <summary>
/// VCam의 회전 기능을 모두 제거하고, 자연스러운 Y축 연동 확대/축소(Zoom) 기능만 남깁니다.
/// VCam은 Cinemachine Follow 컴포넌트와 Aim 설정을 사용하여 타겟을 추적하고 바라봅니다.
/// </summary>
public class Unity6ZoomOnlyControl : MonoBehaviour // 스크립트 이름 변경
{
    // ======================================================================
    // 변수: 설정 필드 (SOLID - 설정 분리)
    // ======================================================================

    [Header("Cinemachine & Target")]
    [Tooltip("이 스크립트가 부착된 VCam 컴포넌트입니다.")]
    [SerializeField] private CinemachineCamera _targetVCam;
    [Tooltip("카메라가 따라가고, 바라볼 대상(플레이어)의 Transform을 연결해주세요.")]
    [SerializeField] private Transform _target;

    // Zoom
    [Header("Zoom Settings")]
    [Tooltip("스크롤 휠 이동에 따른 확대/축소 속도입니다.")]
    [SerializeField] private float _zoomSpeed = 1f;
    [Tooltip("최대 확대 (Z축이 0에 가까움)")]
    [SerializeField] private float _minFollowOffsetZ = -2.0f;
    [Tooltip("최소 확대 (Z축이 0에서 가장 멈)")]
    [SerializeField] private float _maxFollowOffsetZ = -50.0f;

    [Header("Natural Zoom Height (Y축 연동)")]
    [Tooltip("가장 가까울 때(min Z)의 Y축 높이입니다.")]
    [SerializeField] private float _minFollowOffsetY = 2.0f;
    [Tooltip("가장 멀 때(max Z)의 Y축 높이입니다.")]
    [SerializeField] private float _maxFollowOffsetY = 50.0f;

    // ======================================================================
    // 내부 상태 변수
    // ======================================================================
    private CinemachineFollow _followComponent;
    private float _currentFollowOffsetZ;
    private float _currentFollowOffsetY;

    // 회전 관련 변수는 모두 제거했습니다.

    // ======================================================================
    // 유니티 생명 주기 메서드
    // ======================================================================

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        // 회전 입력 처리 메서드(HandleRotationInput) 제거
        HandleZoomInput();
    }

    private void LateUpdate()
    {
        // 회전/궤도 적용 메서드(ApplyOrbitPositionAndLookAt) 제거
        // Cinemachine의 기본 Follow/Aim 기능이 위치/회전을 담당합니다.
    }

    // ======================================================================
    // 초기화 및 입력 처리
    // ======================================================================

    private void InitializeComponents()
    {
        _followComponent = _targetVCam.GetComponent<CinemachineFollow>();
        if (_followComponent == null)
        {
            Debug.LogError("Cinemachine Follow 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 초기 Offset 값 설정
        _currentFollowOffsetZ = _followComponent.FollowOffset.z;
        _currentFollowOffsetY = _followComponent.FollowOffset.y;
    }

    /// <summary>
    /// 마우스 스크롤 휠 입력을 감지하여 카메라의 확대/축소를 처리합니다. (Z, Y축 연동)
    /// </summary>
    private void HandleZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0 && _followComponent != null)
        {
            // 1. Z 값 업데이트 (줌 거리)
            _currentFollowOffsetZ = Mathf.Clamp(
                _currentFollowOffsetZ + scrollInput * _zoomSpeed,
                _maxFollowOffsetZ, // -50.0 (멀리)
                _minFollowOffsetZ  // -2.0 (가까이)
            );

            // 2. Y 값 업데이트 (줌 높이) - Z 값에 비례하여 높이도 변경됩니다.
            // Z 값은 음수이므로, Lerp를 위해 ZMin(-2.0)을 t=0, ZMax(-50.0)을 t=1로 매핑합니다.
            float t = Mathf.InverseLerp(_minFollowOffsetZ, _maxFollowOffsetZ, _currentFollowOffsetZ);
            _currentFollowOffsetY = Mathf.Lerp(_minFollowOffsetY, _maxFollowOffsetY, t);

            // 3. Follow Offset 적용
            Vector3 newOffset = _followComponent.FollowOffset;

            // Z축과 Y축 모두 부드럽게 보간
            newOffset.z = Mathf.Lerp(newOffset.z, _currentFollowOffsetZ, Time.deltaTime * 10f);
            newOffset.y = Mathf.Lerp(newOffset.y, _currentFollowOffsetY, Time.deltaTime * 10f);

            _followComponent.FollowOffset = newOffset;
        }

        // **[핵심 수정]** 줌 조작이 있었고, 현재 튜토리얼 단계가 GuideZoomControl일 때만 Invoke를 호출합니다.
        if (scrollInput != 0 && UITutorialHandler.Instance != null && TutorialManager.Instance != null &&
            TutorialManager.Instance.CurrentStep == TutorialStep.GuideZoomControl)
        {
            // 줌 조작이 감지되었으므로, 다음 단계로 진행을 요청합니다.
            UITutorialHandler.Instance.OnZoomChanged.Invoke();
        }
    }
}