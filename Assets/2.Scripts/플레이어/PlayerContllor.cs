using UnityEngine;

// 플레이어의 이동 및 점프를 제어하는 스크립트입니다.
public class PlayerController : MonoBehaviour
{
    // PlayerStats 스크립트는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.
    // PlayerStats PlayerStats;

    // 속도 관련 변수
    [Header("속도 설정")]
    [Tooltip("걷기 속도입니다. PlayerStats.Instance.moveSpeed를 참조하여 실시간으로 업데이트됩니다.")]
    public float walkSpeed = 10f; // 초기 값은 인스펙터에서 설정되지만, Start()에서 PlayerStats의 값으로 덮어씁니다.
    [Tooltip("달리기 시 적용될 속도 배율입니다.")]
    public float runSpeedMultiplier = 2f;
    [Tooltip("점프 시 적용될 힘의 크기입니다.")]
    public float jumpForce = 5f;

    // 컴포넌트 변수
    private Rigidbody playerRigidbody;

    // 상태 변수
    [Tooltip("플레이어가 땅에 닿았는지 여부를 나타냅니다.")]
    private bool isGrounded = true;

    void Start()
    {
        // Rigidbody 컴포넌트를 가져옵니다.
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody 컴포넌트를 찾을 수 없습니다. 플레이어 오브젝트에 Rigidbody를 부착해 주세요.");
            return;
        }

        // 캐릭터가 넘어지지 않도록 회전을 고정합니다.
        playerRigidbody.freezeRotation = true;

        // PlayerStats.Instance를 통해 기본 이동 속도 값을 가져와 초기화합니다.
        if (PlayerStats.Instance != null)
        {
            walkSpeed = PlayerStats.Instance.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats 인스턴스를 찾을 수 없습니다. 기본 walkSpeed를 사용합니다.");
        }
    }

    void Update()
    {
        // 땅에 닿았을 때만 점프 가능
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // 물리학 업데이트는 FixedUpdate에서 처리하는 것이 좋습니다.
    void FixedUpdate()
    {
        // PlayerStats.Instance의 moveSpeed 값이 런타임에 변경될 수 있으므로 매 프레임 업데이트합니다.
        if (PlayerStats.Instance != null)
        {
            walkSpeed = PlayerStats.Instance.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats 인스턴스가 존재하지 않습니다. 이동 속도를 업데이트할 수 없습니다.");
            return;
        }

        // 입력 값 받기
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // 달리기 속도 적용
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= runSpeedMultiplier;
        }

        // 이동 벡터 계산
        Vector3 movement = new Vector3(xInput, 0f, zInput).normalized * currentSpeed;

        // Rigidbody에 속도 적용 (Y축 속도 유지)
        Vector3 newVelocity = new Vector3(movement.x, playerRigidbody.linearVelocity.y, movement.z);
        playerRigidbody.linearVelocity = newVelocity;
    }

    // 땅에 닿았는지 확인
    private void OnCollisionEnter(Collision collision)
    {
        // "Ground" 태그를 가진 오브젝트에 닿았을 때
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}