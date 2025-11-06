using UnityEngine;

/// <summary>
/// 월드 스페이스에 배치된 UI 또는 GameObject가 항상 메인 카메라를 바라보도록 만드는 빌보드 스크립트입니다.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    // === 필드 ===

    /// <summary>
    /// 목표 카메라의 Transform입니다.
    /// Start()에서 자동으로 메인 카메라를 찾아 할당합니다.
    /// </summary>
    private Transform _mainCameraTransform;


    // === MonoBehaviour 메서드 ===

    private void Start()
    {
        // 메인 카메라의 Transform을 한 번만 찾아서 캐싱합니다.
        // 매번 찾지 않고 변수에 저장하여 성능을 최적화합니다.
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("[LookAtCamera] 씬에서 'MainCamera' 태그를 가진 카메라를 찾을 수 없습니다.");
            enabled = false; // 카메라가 없으면 스크립트 비활성화
        }
    }

    /// <summary>
    /// Update() 대신 LateUpdate()를 사용하여 카메라 움직임 후에 위치를 조정함으로써
    /// 텍스트가 떨리는 현상(Jittering)을 최소화하고 부드러움을 유지합니다.
    /// SOLID 원칙: 단일 책임 원칙 (카메라 움직임 이후 빌보드 회전 처리)
    /// </summary>
    private void LateUpdate()
    {
        if (_mainCameraTransform == null)
        {
            return; // 카메라가 없으면 아무것도 하지 않습니다.
        }

        // 1. 카메라를 바라보도록 회전합니다. (수정된 로직)

        // A. LookAt() 대신 Rotation 계산 사용 (권장)
        // 오브젝트의 위치에서 카메라의 위치를 뺀 벡터(카메라로 향하는 방향)를 구합니다.
        Vector3 lookDirection = _mainCameraTransform.position - transform.position;

        // 회전 값을 계산합니다. LookRotation은 Z축이 lookDirection을 향하도록 회전시킵니다.
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        // B. [핵심 수정] UI 요소의 정면(Z축)이 보통 오브젝트의 뒷면을 향하고 있으므로,
        // 계산된 회전 값에 Y축을 기준으로 180도를 추가로 회전시켜 뒤집힌 것을 보정합니다.
        // 이로 인해 UI는 항상 카메라를 향하되, 좌우 반전 없이 '정면'을 보여주게 됩니다.
        transform.rotation = targetRotation * Quaternion.Euler(0, 180, 0);


        // [선택 로직] 만약 LookAt()을 고수하고 싶다면:
        // transform.LookAt(_mainCameraTransform.position);
        // transform.Rotate(0, 180, 0); // 회전 후 180도 더 돌리기

        // 2. 선택 사항: X축과 Z축 회전을 고정하여 텍스트가 수평/수직을 유지하게 합니다.
        // 이는 HP바처럼 완전히 평평하게 유지되어야 할 때 유용하며, 카메라가 위아래로 움직여도 항상 똑바로 서있게 만듭니다.
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}