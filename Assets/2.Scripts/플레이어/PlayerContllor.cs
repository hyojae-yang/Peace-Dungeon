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

    [Header("회전 설정")]
    [Tooltip("플레이어가 이동 방향으로 회전하는 속도입니다. 값이 높을수록 더 빠르게 회전합니다.")]
    public float rotationSpeed = 10f; // 부드러운 회전을 위한 변수
    // 컴포넌트 변수
    private Rigidbody playerRigidbody;

    [Header("스폰 포인트 설정")]
    [Tooltip("플레이어가 처음 던전에 들어갈 때 스폰될 위치입니다.")]
    [SerializeField] private Transform dungeonSpawnPoint;
    [Tooltip("플레이어가 던전에서 나갈 때 스폰될 위치입니다.")]
    [SerializeField] private Transform exitSpawnPoint;
    [Tooltip("보스룸 입장 시 플레이어가 이동할 위치입니다.")]
    [SerializeField] private Transform bossRoomSpawnPoint;
    // 상태 변수
    [Tooltip("플레이어가 땅에 닿았는지 여부를 나타냅니다.")]
    private bool isGrounded = true;
    public bool canMove = true;

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
        // 이동 입력이 있을 경우에만 회전을 처리합니다. (movement.magnitude > 0.1f로 공중에 있을 때의 미세한 움직임 방지)
        if (movement.magnitude > 0.01f) // 실제로 움직이고 있을 때만 회전
        {
            // 이동 방향(movement) 벡터를 바라보는 회전(Quaternion)을 계산합니다.
            // Quaternion.LookRotation은 Z축이 movement 방향을 바라보게 회전 값을 만들어 줍니다.
            // movement.normalized를 사용하여 방향 정보만 가져옵니다.
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.z).normalized);

            // 현재 회전(transform.rotation)을 목표 회전(targetRotation)으로 부드럽게 보간합니다.
            // Time.fixedDeltaTime은 FixedUpdate 주기와 동기화되어 프레임 드롭에 관계없이 일정한 회전 속도를 보장합니다.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
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
    public void inDungeon()
    {
        canMove = false;
        playerCharacter.transform.position = dungeonSpawnPoint.position;
        canMove = true;
    }
    public void outDungeon()
    {
        canMove = false;
        playerCharacter.transform.position = exitSpawnPoint.position;
        canMove = true;
    }
    public void enterBossRoom()
    {
        canMove = false;
        playerCharacter.transform.position = bossRoomSpawnPoint.position;
        canMove = true;
    }
}