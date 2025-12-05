using UnityEngine;

// [SOLID - SRP 준수]: 이 스크립트는 MinimapCamera의 위치를 지정된 대상과 동기화하는 책임만 가집니다.
public class MinimapTracker : MonoBehaviour
{
    // [Inspector 할당] 따라다닐 플레이어 오브젝트의 Transform을 할당받는 변수입니다.
    [Tooltip("따라다닐 플레이어 오브젝트의 Transform을 할당하세요.")]
    public Transform target;

    // [Inspector 할당] 카메라가 유지할 고정 높이입니다. (MinimapCamera의 Y값과 동일하게 설정)
    [Tooltip("카메라가 유지할 고정 높이입니다. (MinimapCamera의 Y값과 동일하게 설정)")]
    public float altitude = 100f;

    // 성능 최적화를 위해 카메라의 Transform을 미리 저장해 둡니다.
    private Transform cameraTransform;

    private void Awake()
    {
        // 스크립트가 부착된 오브젝트 (MinimapCamera)의 Transform을 가져옵니다.
        cameraTransform = this.transform;
    }

    /// <summary>
    /// 모든 Update 처리가 완료된 후, 플레이어 이동에 맞춰 위치를 업데이트하여 카메라를 추적합니다.
    /// LateUpdate를 사용하여 부드러운 추적을 보장합니다.
    /// </summary>
    private void LateUpdate()
    {
        // 타겟이 할당되지 않았다면 동작을 멈춥니다.
        if (target == null) return;

        // 플레이어의 XZ 위치를 가져오고, Y축은 고정 높이(altitude)를 사용합니다.
        cameraTransform.position = new Vector3(
            target.position.x,
            altitude, // Minimap은 항상 고정된 높이에서 찍어야 하므로 고정값 사용
            target.position.z
        );
    }
}