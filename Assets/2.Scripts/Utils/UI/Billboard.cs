using UnityEngine;

/// <summary>
/// UI 오브젝트가 항상 메인 카메라를 바라보도록(Billboard) 회전을 관리하는 스크립트입니다.
/// 단일 책임 원칙(SRP)에 따라 회전 로직만을 담당합니다.
/// </summary>
public class Billboard : MonoBehaviour
{
    // === 변수 선언 및 주석 ===

    // [최적화] 매번 Camera.main을 호출하지 않도록 Transform 컴포넌트를 캐싱합니다.
    private Transform targetCameraTransform;

    [Header("빌보드 설정")]
    [Tooltip("UI의 수직(Y) 축 회전을 고정하여 기울어지지 않게 합니다. (일반적으로 true)")]
    public bool lockYAxis = true;

    private void Awake()
    {
        // 1. 팩트: 메인 카메라의 Transform을 찾아서 캐싱합니다.
        if (Camera.main != null)
        {
            targetCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("[Billboard] 씬에서 'Main Camera' 태그를 가진 카메라를 찾을 수 없습니다. 스크립트 비활성화.");
            enabled = false;
        }
    }

    /// <summary>
    /// 모든 오브젝트 이동과 카메라 움직임이 끝난 후(LateUpdate) 회전을 적용하여 부드러움을 유지합니다.
    /// </summary>
    private void LateUpdate()
    {
        if (targetCameraTransform == null) return;

        // 1. 카메라를 바라보는 방향 벡터를 계산합니다. (UI 위치 -> 카메라 위치)
        // 몬스터 위치에서 카메라 위치를 빼면, UI 오브젝트가 카메라를 향해 '뒤로' 보는 방향이 됩니다.
        Vector3 directionToCamera = targetCameraTransform.position - transform.position;

        // 2. 해당 방향을 바라보는 회전을 계산합니다.
        // Quaternion.LookRotation을 사용하여, UI의 Z축(앞)이 카메라 쪽으로 향하도록 회전을 생성합니다.
        Quaternion rotationToCamera = Quaternion.LookRotation(-directionToCamera);

        // 3. Y축 고정 로직 (2D UI 평면 유지 최적화)
        if (lockYAxis)
        {
            // X, Z축 회전(기울임) 정보를 0으로 고정하여 UI가 항상 수직으로 서 있도록 합니다.
            // Y축만 회전 정보(좌우 방향)를 유지합니다.
            rotationToCamera.x = 0f;
            rotationToCamera.z = 0f;

            // 정규화 후 최종 회전 적용
            transform.rotation = Quaternion.Normalize(rotationToCamera);
        }
        else
        {
            // Y축 고정이 필요 없는 경우 (완벽한 3D 빌보딩)
            transform.rotation = rotationToCamera;
        }
    }
}