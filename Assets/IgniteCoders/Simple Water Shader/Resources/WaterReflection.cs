using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 카메라의 위치와 방향을 기준으로 물 평면에 반사된 지점의 카메라 위치/방향을 계산하고
/// 반사 카메라(Reflection Camera)를 설정하여 반사 텍스처를 렌더링하는 스크립트입니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WaterReflection : MonoBehaviour
{
    // ====================================================================================================
    // 1. 참조 (References) - 다른 객체에 대한 연결
    // ====================================================================================================

    /// <summary>
    /// 현재 씬을 렌더링하는 메인 카메라입니다. (Camera.main으로 할당)
    /// </summary>
    private Camera mainCamera;

    /// <summary>
    /// 반사 이미지를 렌더링할 카메라입니다. 이 스크립트가 붙어있는 GameObject의 Camera 컴포넌트입니다.
    /// </summary>
    private Camera reflectionCamera;

    /// <summary>
    /// 반사가 일어날 평면(예: 물 표면)의 Transform. 이 평면을 기준으로 대칭 변환을 수행합니다.
    /// [Tooltip] 이 툴팁은 에디터에서 변수의 역할을 명확히 설명해 줍니다.
    /// </summary>
    [Tooltip("카메라가 반사될 평면의 Transform (물 표면 또는 동일 위치/회전을 가진 오브젝트).")]
    public Transform reflectionPlane;

    /// <summary>
    /// 반사 카메라가 렌더링한 결과가 저장될 텍스처(RenderTexture)입니다.
    /// 이 텍스처는 보통 물 셰이더의 메인 텍스처로 사용됩니다.
    /// </summary>
    [Tooltip("물 셰이더에서 반사를 표시하기 위해 사용되는 출력 텍스처.")]
    public RenderTexture outputTexture;

    // ====================================================================================================
    // 2. 설정 변수 (Parameters) - 외부에서 조절 가능한 설정 값
    // ====================================================================================================

    /// <summary>
    /// 메인 카메라의 설정(FOV, 클리핑 평면 등)을 반사 카메라에 복사할지 여부입니다.
    /// </summary>
    public bool copyCameraParamerers;

    /// <summary>
    /// 물 평면으로부터 반사 카메라를 수직으로 얼마나 오프셋(이동)할지 결정하는 값입니다.
    /// 이는 종종 물 표면의 약간의 흔들림이나 시각적 보정을 위해 사용됩니다.
    /// </summary>
    public float verticalOffset;

    // ====================================================================================================
    // 3. 내부 상태 및 캐시 (Cache & Internal State)
    // ====================================================================================================

    /// <summary>
    /// 스크립트가 초기 설정을 완료하고 렌더링 준비가 되었는지 나타내는 상태 플래그.
    /// </summary>
    private bool isReady;

    /// <summary>
    /// 메인 카메라의 Transform 컴포넌트 캐시. 매 프레임 GetComponent를 피하기 위한 성능 최적화입니다.
    /// </summary>
    private Transform mainCamTransform;

    /// <summary>
    /// 반사 카메라의 Transform 컴포넌트 캐시. 매 프레임 GetComponent를 피하기 위한 성능 최적화입니다.
    /// </summary>
    private Transform reflectionCamTransform;

    // ====================================================================================================
    // 4. 유니티 라이프사이클 메서드
    // ====================================================================================================

    /// <summary>
    /// 초기화 시 호출되며, 필요한 카메라 참조를 가져오고 초기 유효성 검사를 수행합니다.
    /// </summary>
    public void Awake()
    {
        // 메인 카메라 참조 할당
        mainCamera = Camera.main;
        // 반사 카메라 참조 할당 (이 스크립트는 반드시 Camera 컴포넌트와 함께 있어야 함)
        reflectionCamera = GetComponent<Camera>();

        // 초기 설정을 검증하고 필요한 컴포넌트 캐싱 및 카메라 파라미터 복사를 수행합니다.
        ValidateInitialization();
    }

    /// <summary>
    /// 매 프레임 호출됩니다. 렌더링 준비가 된 경우에만 반사 위치를 계산하고 렌더링을 요청합니다.
    /// </summary>
    private void Update()
    {
        // isReady 상태 확인을 통해 불필요한 계산을 방지합니다. (성능 및 안정성)
        if (isReady)
            RenderReflection();
    }

    // ====================================================================================================
    // 5. 핵심 로직 메서드 (Core Logic Methods)
    // ====================================================================================================

    /// <summary>
    /// 반사 카메라의 위치와 회전을 계산하고 적용하여 반사 이미지를 렌더링합니다.
    /// 이 함수는 메인 카메라의 위치/방향을 물 평면에 대해 대칭 변환하는 핵심 로직을 포함합니다.
    /// </summary>
    private void RenderReflection()
    {
        // 1. 월드 공간(World Space)에서 메인 카메라의 기본 정보를 가져옵니다.
        Vector3 cameraDirectionWorldSpace = mainCamTransform.forward;
        Vector3 cameraUpWorldSpace = mainCamTransform.up;
        Vector3 cameraPositionWorldSpace = mainCamTransform.position;

        // 2. 수직 오프셋을 적용합니다. (물 표면의 높이 조절 등)
        cameraPositionWorldSpace.y += verticalOffset;

        // 3. 월드 공간의 정보를 반사 평면의 로컬 공간(Local Space)으로 변환합니다.
        // TransformDirection/TransformPoint를 사용해 좌표계를 통일합니다.
        Vector3 cameraDirectionPlaneSpace = reflectionPlane.InverseTransformDirection(cameraDirectionWorldSpace);
        Vector3 cameraUpPlaneSpace = reflectionPlane.InverseTransformDirection(cameraUpWorldSpace);
        Vector3 cameraPositionPlaneSpace = reflectionPlane.InverseTransformPoint(cameraPositionWorldSpace);

        // 4. 로컬 공간에서 Y축(평면에 수직인 축)을 뒤집어 반사를 구현합니다.
        cameraDirectionPlaneSpace.y *= -1;
        cameraUpPlaneSpace.y *= -1; // Up 벡터도 뒤집혀야 정확한 LookAt이 가능합니다.
        cameraPositionPlaneSpace.y *= -1;

        // 5. 뒤집힌 로컬 공간 정보를 다시 월드 공간으로 변환합니다.
        cameraDirectionWorldSpace = reflectionPlane.TransformDirection(cameraDirectionPlaneSpace);
        cameraUpWorldSpace = reflectionPlane.TransformDirection(cameraUpPlaneSpace);
        cameraPositionWorldSpace = reflectionPlane.TransformPoint(cameraPositionPlaneSpace);

        // 6. 계산된 위치와 방향을 반사 카메라에 적용합니다.
        reflectionCamTransform.position = cameraPositionWorldSpace;
        // LookAt을 사용하여 위치와 방향 벡터(forward, up)를 적용합니다.
        reflectionCamTransform.LookAt(cameraPositionWorldSpace + cameraDirectionWorldSpace, cameraUpWorldSpace);

        // NOTE: 이 시점에서 Unity는 reflectionCamera의 설정(targetTexture)에 따라 자동으로 렌더링을 수행합니다.
    }

    /// <summary>
    /// 스크립트 초기화 시 참조들이 유효한지 검사하고, 유효하다면 캐싱 및 카메라 설정을 복사합니다.
    /// SRP 관점에서 초기화/설정 책임을 분리했습니다.
    /// </summary>
    private void ValidateInitialization()
    {
        // 메인 카메라의 유효성 검사 및 Transform 캐싱
        if (mainCamera != null)
        {
            mainCamTransform = mainCamera.transform;
            isReady = true;
        }
        else
        {
            // 메인 카메라가 없으면 렌더링을 진행할 수 없습니다.
            Debug.LogError("Main Camera를 찾을 수 없습니다. Water Reflection 스크립트를 비활성화합니다.");
            isReady = false;
        }

        // 반사 카메라의 유효성 검사 및 Transform 캐싱
        if (reflectionCamera != null)
        {
            reflectionCamTransform = reflectionCamera.transform;
            // 앞서 mainCamera가 null이 아닐 경우 isReady가 true가 되므로, 여기서 다시 true로 설정합니다.
            if (mainCamera != null) isReady = true;
        }
        else
        {
            // 반사 카메라 컴포넌트가 없으면 문제가 발생하므로 경고를 남깁니다. (RequireComponent로 어느 정도 방지)
            Debug.LogError("Reflection Camera 컴포넌트(스크립트가 붙은 GameObject)를 찾을 수 없습니다.");
            isReady = false;
        }

        // 모든 참조가 유효하고, 카메라 파라미터 복사 옵션이 켜져 있을 경우 설정을 적용합니다.
        if (isReady && copyCameraParamerers)
        {
            // 한 번 복사 후에는 옵션을 끄도록 하여 불필요한 매번 복사를 방지합니다. (CopyFrom의 성능 부하 최소화)
            copyCameraParamerers = false; // 복사 완료 플래그 역할

            // 메인 카메라의 FOV, 클리핑 평면 등을 반사 카메라에 복사합니다.
            reflectionCamera.CopyFrom(mainCamera);

            // 반사 카메라의 출력 타겟을 할당된 RenderTexture로 설정합니다.
            reflectionCamera.targetTexture = outputTexture;

            // 물 표면 아래의 물체를 렌더링하지 않도록 근접 클리핑 평면을 조정하는 로직이 추가될 수 있습니다. (성능 및 정확도 향상)
            // 예시: reflectionCamera.nearClipPlane = Vector3.Distance(reflectionCamera.transform.position, reflectionPlane.position);
        }

        // outputTexture가 할당되지 않은 경우 경고를 표시합니다.
        if (isReady && outputTexture == null)
        {
            Debug.LogWarning("Output Texture가 할당되지 않았습니다. 반사 이미지가 렌더링되지 않습니다.");
            isReady = false;
        }
    }
}