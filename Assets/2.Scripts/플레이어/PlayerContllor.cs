using UnityEngine;

// �÷��̾��� �̵� �� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
public class PlayerController : MonoBehaviour
{
    // PlayerStats ��ũ��Ʈ�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // PlayerStats PlayerStats;

    // �ӵ� ���� ����
    [Header("�ӵ� ����")]
    [Tooltip("�ȱ� �ӵ��Դϴ�. PlayerStats.Instance.moveSpeed�� �����Ͽ� �ǽð����� ������Ʈ�˴ϴ�.")]
    public float walkSpeed = 10f; // �ʱ� ���� �ν����Ϳ��� ����������, Start()���� PlayerStats�� ������ ����ϴ�.
    [Tooltip("�޸��� �� ����� �ӵ� �����Դϴ�.")]
    public float runSpeedMultiplier = 2f;
    [Tooltip("���� �� ����� ���� ũ���Դϴ�.")]
    public float jumpForce = 5f;

    // ������Ʈ ����
    private Rigidbody playerRigidbody;

    // ���� ����
    [Tooltip("�÷��̾ ���� ��Ҵ��� ���θ� ��Ÿ���ϴ�.")]
    private bool isGrounded = true;

    void Start()
    {
        // Rigidbody ������Ʈ�� �����ɴϴ�.
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody ������Ʈ�� ã�� �� �����ϴ�. �÷��̾� ������Ʈ�� Rigidbody�� ������ �ּ���.");
            return;
        }

        // ĳ���Ͱ� �Ѿ����� �ʵ��� ȸ���� �����մϴ�.
        playerRigidbody.freezeRotation = true;

        // PlayerStats.Instance�� ���� �⺻ �̵� �ӵ� ���� ������ �ʱ�ȭ�մϴ�.
        if (PlayerStats.Instance != null)
        {
            walkSpeed = PlayerStats.Instance.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� ã�� �� �����ϴ�. �⺻ walkSpeed�� ����մϴ�.");
        }
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

    // ������ ������Ʈ�� FixedUpdate���� ó���ϴ� ���� �����ϴ�.
    void FixedUpdate()
    {
        // PlayerStats.Instance�� moveSpeed ���� ��Ÿ�ӿ� ����� �� �����Ƿ� �� ������ ������Ʈ�մϴ�.
        if (PlayerStats.Instance != null)
        {
            walkSpeed = PlayerStats.Instance.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� �������� �ʽ��ϴ�. �̵� �ӵ��� ������Ʈ�� �� �����ϴ�.");
            return;
        }

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
        // "Ground" �±׸� ���� ������Ʈ�� ����� ��
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}