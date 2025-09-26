using UnityEngine;
using System;
using System.Collections.Generic;

// �÷��̾��� �̵� �� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
// �� ��ũ��Ʈ�� PlayerCharacter�� ����� �����˴ϴ�.
public class PlayerController : MonoBehaviour
{
    // PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

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
    private bool canMove = true;

    void Start()
    {
        // PlayerCharacter�� �ν��Ͻ��� �����ͼ� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� ã�� �� �����ϴ�. ��ũ��Ʈ�� ����� �������� ���� �� �ֽ��ϴ�.");
            return;
        }

        // Rigidbody ������Ʈ�� �����ɴϴ�.
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody ������Ʈ�� ã�� �� �����ϴ�. �÷��̾� ������Ʈ�� Rigidbody�� ������ �ּ���.");
            return;
        }

        // ĳ���Ͱ� �Ѿ����� �ʵ��� ȸ���� �����մϴ�.
        playerRigidbody.freezeRotation = true;

        // PlayerCharacter�� ���� PlayerStats�� �̵� �ӵ� ���� ������ �ʱ�ȭ�մϴ�.
        if (playerCharacter.playerStats != null)
        {
            walkSpeed = playerCharacter.playerStats.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats�� PlayerCharacter�� �Ҵ���� �ʾҽ��ϴ�. �⺻ walkSpeed�� ����մϴ�.");
        }
    }

    void Update()
    {
        //Debug.Log("��ġ ����: " + transform.position + " by " + this.GetType().Name);
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
        if (!canMove) return;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerStats�� �ʱ�ȭ���� �ʾҽ��ϴ�. �̵� �ӵ��� ������Ʈ�� �� �����ϴ�.");
           
            return;
        }

        // PlayerStats�� moveSpeed ���� ��Ÿ�ӿ� ����� �� �����Ƿ� �� ������ ������Ʈ�մϴ�.
        //walkSpeed = playerCharacter.playerStats.moveSpeed;

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
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
        }
    }
}