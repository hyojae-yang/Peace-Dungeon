using UnityEngine;

/// <summary>
/// 나무 정령이 발사하는 투사체의 움직임과 충돌 처리를 담당합니다.
/// 플레이어와 충돌 시 실제 피해를 주는 RootTrap 이펙트를 생성하고 소멸합니다.
/// </summary>
public class RootProjectile : MonoBehaviour
{
    // === 외부 설정 변수 ===
    [Header("투사체 설정")]
    [Tooltip("투사체의 이동 속도입니다.")]
    [SerializeField] private float moveSpeed = 8f;
    [Tooltip("투사체가 아무것도 맞추지 못하고 자동으로 사라지는 시간입니다.")]
    [SerializeField] private float lifetime = 5f;

    // 플레이어 충돌 시 생성될 실제 공격 효과 프리팹입니다. (RootTrap 스크립트 포함)
    [Tooltip("충돌 시 생성할 RootTrap 공격 효과 프리팹입니다.")]
    [SerializeField] private GameObject rootTrapPrefab;

    // === 내부 상태 변수 ===
    private Rigidbody rb;
    private Vector3 initialTargetPosition; // 투사체가 날아갈 초기 목표 위치
    private bool isFired = false; // 발사가 시작되었는지 여부 확인

    /// <summary>
    /// 컴포넌트 초기화 및 종속성 확보를 담당합니다.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 생성 후 일정 시간 뒤 자동으로 파괴되도록 예약합니다.
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Update는 프레임마다 호출되며, 투사체의 지속적인 이동 로직을 처리합니다.
    /// </summary>
    private void Update()
    {
        // 발사가 시작되었다면, 현재 설정된 속도로 전진합니다.
        // Rigidbody를 사용하지만, 간단한 투사체 이동을 위해 직접 Transform을 제어할 수도 있습니다.
        // 여기서는 Rigidbody를 사용하여 좀 더 물리적인 제어를 시도합니다.
        if (isFired)
        {
            // Update에서는 단순 이동 방향을 유지하고, 물리 업데이트는 FixedUpdate에서 처리합니다.
        }
    }

    /// <summary>
    /// 물리 업데이트 주기에 호출됩니다. Rigidbody를 제어합니다.
    /// </summary>
    private void FixedUpdate()
    {
        if (isFired)
        {
            // 앞으로 설정된 속도로 계속 움직이도록 Rigidbody에 힘을 가합니다.
            rb.linearVelocity = transform.forward * moveSpeed;
        }
    }

    /// <summary>
    /// TreeSpiritBehavior에서 호출됩니다. 투사체의 목표와 발사 방향을 설정합니다.
    /// </summary>
    /// <param name="targetTransform">투사체가 날아갈 목표(플레이어)의 Transform입니다.</param>
    public void SetTargetAndFire(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("투사체 발사 시 목표(Target)가 null입니다. 투사체를 즉시 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        // 목표 위치(플레이어의 초기 위치)를 저장합니다.
        // 투사체는 발사 시점을 기준으로 목표 위치를 향해 직선으로 날아갑니다.
        initialTargetPosition = targetTransform.position;

        // 발사 방향 계산: 몬스터 위치에서 목표 위치를 향하는 벡터
        Vector3 direction = (initialTargetPosition - transform.position).normalized;

        // 투사체의 회전을 목표 방향으로 설정합니다. (시각적 효과)
        transform.rotation = Quaternion.LookRotation(direction);

        // Rigidbody 초기화: 혹시 모를 이전 속도를 제거
        rb.linearVelocity = Vector3.zero;

        // 발사 플래그 설정 및 FixedUpdate에서 이동 시작
        isFired = true;
    }

    /// <summary>
    /// 충돌 발생 시 호출되는 이벤트 함수입니다.
    /// 플레이어와 충돌했는지 확인하고 RootTrap을 생성합니다.
    /// </summary>
    /// <param name="other">충돌한 Collider 정보입니다.</param>
    private void OnTriggerEnter(Collider other)
    {
        // 몬스터 자신과의 충돌 방지 (필요 시 태그나 레이어로 정교하게 처리)
        // 여기서는 간단히 플레이어 태그만 확인합니다.
        if (other.CompareTag("Player"))
        {
            // 1. 공격 효과 (RootTrap) 생성
            // 충돌 지점(또는 플레이어 위치)에 실제 트랩을 생성합니다.
            // Y축을 보정하여 땅 위에 생성되도록 할 수 있습니다. (필요 시 수정)
            Vector3 trapPosition = other.transform.position;
            Instantiate(rootTrapPrefab, trapPosition, Quaternion.identity);

            // 2. 투사체 소멸
            // 투사체는 할 일을 다 했으므로 자신을 파괴합니다.
            Destroy(gameObject);
        }

        // 환경 오브젝트(벽, 땅 등)와 충돌 시에도 소멸시키고 싶다면 여기에 로직을 추가합니다.
        // 예: else if (other.CompareTag("Wall")) { Destroy(gameObject); }
    }
}