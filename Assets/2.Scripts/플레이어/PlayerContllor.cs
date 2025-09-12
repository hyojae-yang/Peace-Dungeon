using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerStats PlayerStats;
    // 속도 관련 변수
    public float walkSpeed = 10f;
    public float runSpeedMultiplier = 2f;
    public float jumpForce = 5f;

    // 컴포넌트 변수
    private Rigidbody playerRigidbody;

    // 상태 변수
    private bool isGrounded = true;

    void Start()
    {
        PlayerStats = GetComponent<PlayerStats>();
        playerRigidbody = GetComponent<Rigidbody>();
        walkSpeed = PlayerStats.baseMoveSpeed;
        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody component not found on player object!");
        }
        playerRigidbody.freezeRotation = true; // 캐릭터가 넘어지지 않도록 회전 고정
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

    void FixedUpdate()
    {
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
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

}