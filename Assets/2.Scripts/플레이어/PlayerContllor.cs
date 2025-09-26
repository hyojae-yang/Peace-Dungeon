using UnityEngine;
using System;
using System.Collections.Generic;

// 플레이어의 이동 및 점프를 제어하는 스크립트입니다.
// 이 스크립트는 PlayerCharacter의 멤버로 관리됩니다.
public class PlayerController : MonoBehaviour
{
    // PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

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
    private bool canMove = true;

    void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        // Rigidbody 컴포넌트를 가져옵니다.
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody 컴포넌트를 찾을 수 없습니다. 플레이어 오브젝트에 Rigidbody를 부착해 주세요.");
            return;
        }

        // 캐릭터가 넘어지지 않도록 회전을 고정합니다.
        playerRigidbody.freezeRotation = true;

        // PlayerCharacter를 통해 PlayerStats의 이동 속도 값을 가져와 초기화합니다.
        if (playerCharacter.playerStats != null)
        {
            walkSpeed = playerCharacter.playerStats.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats가 PlayerCharacter에 할당되지 않았습니다. 기본 walkSpeed를 사용합니다.");
        }
    }

    void Update()
    {
        //Debug.Log("위치 변경: " + transform.position + " by " + this.GetType().Name);
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
        if (!canMove) return;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 이동 속도를 업데이트할 수 없습니다.");
           
            return;
        }

        // PlayerStats의 moveSpeed 값이 런타임에 변경될 수 있으므로 매 프레임 업데이트합니다.
        //walkSpeed = playerCharacter.playerStats.moveSpeed;

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
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
        }
    }
}