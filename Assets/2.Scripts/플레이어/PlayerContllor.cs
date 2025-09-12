using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerStats PlayerStats;
    // �ӵ� ���� ����
    public float walkSpeed = 10f;
    public float runSpeedMultiplier = 2f;
    public float jumpForce = 5f;

    // ������Ʈ ����
    private Rigidbody playerRigidbody;

    // ���� ����
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
        playerRigidbody.freezeRotation = true; // ĳ���Ͱ� �Ѿ����� �ʵ��� ȸ�� ����
    }

    void Update()
    {
        // ���� ����� ���� ���� ����
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        // �Է� �� �ޱ�
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // �޸��� �ӵ� ����
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= runSpeedMultiplier;
        }

        // �̵� ���� ���
        Vector3 movement = new Vector3(xInput, 0f, zInput).normalized * currentSpeed;

        // Rigidbody�� �ӵ� ���� (Y�� �ӵ� ����)
        Vector3 newVelocity = new Vector3(movement.x, playerRigidbody.linearVelocity.y, movement.z);
        playerRigidbody.linearVelocity = newVelocity;
    }

    // ���� ��Ҵ��� Ȯ��
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

}