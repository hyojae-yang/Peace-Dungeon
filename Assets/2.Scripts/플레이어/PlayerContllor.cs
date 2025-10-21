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
    // 애니메이션 블렌딩을 위한 변수입니다.
    [Tooltip("걷기/달리기 애니메이션 상태 변화를 부드럽게 만들기 위한 속도입니다.")]
    private const float AnimationDamping = 0.1f;
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
        playerCharacter.animator.SetFloat("Walk", 0);
        playerCharacter.animator.SetFloat("Run", 0);
    }
    void Update()
    {
        //Debug.Log("위치 변경: " + transform.position + " by " + this.GetType().Name);
        // 땅에 닿았을 때만 점프 가능
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerCharacter.animator.SetTrigger("Jump");
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // 물리학 업데이트는 FixedUpdate에서 처리하는 것이 좋습니다.
    void FixedUpdate()
    {
        if (!canMove)
        {
            // 움직일 수 없을 때는 속도를 0으로 설정하고 애니메이션을 멈춥니다.
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);

            // 애니메이션 파라미터를 0으로 설정하여 Idle 상태로 돌아갑니다.
            // 움직임 제어 불가능 상태에서는 애니메이션을 멈춥니다.
            playerCharacter.animator.SetFloat("Walk", 0, AnimationDamping, Time.fixedDeltaTime);
            playerCharacter.animator.SetFloat("Run", 0, AnimationDamping, Time.fixedDeltaTime);
            return;
        }

        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 이동 속도를 업데이트할 수 없습니다.");
            return;
        }

        // 입력 값 받기
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // 이동 입력 벡터 (XZ 평면)
        Vector3 rawMovement = new Vector3(xInput, 0f, zInput).normalized;

        // 달리기 상태 확인
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // 현재 적용할 이동 속도를 계산합니다.
        float currentSpeed = walkSpeed;
        if (isRunning)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        // 이동 벡터 계산 및 Rigidbody 속도 적용
        Vector3 movement = rawMovement * currentSpeed;
        Vector3 newVelocity = new Vector3(movement.x, playerRigidbody.linearVelocity.y, movement.z);
        playerRigidbody.linearVelocity = newVelocity;

        // 이동 입력의 크기(magnitude)를 기반으로 애니메이션 속도(Amount)를 계산합니다.
        // 입력이 없으면 0, 있으면 1에 가까운 값이 나옵니다.
        float inputMagnitude = rawMovement.magnitude;

        // 애니메이션 파라미터의 목표 값
        float targetWalk = 0.0f;
        float targetRun = 0.0f;

        if (inputMagnitude > 0.01f)
        {
            // 1. 이동 입력이 있을 경우, "Walk" 파라미터는 항상 켜집니다.
            targetWalk = inputMagnitude;

            // 2. 달리기 중일 때만 "Run" 파라미터가 켜집니다. (Walk=1, Run=1 상태가 됨)
            if (isRunning)
            {
                targetRun = inputMagnitude;
            }
            // 3. 걷기 중일 때는 "Run" 파라미터가 0으로 유지됩니다. (Walk=1, Run=0 상태가 됨)
            else
            {
                targetRun = 0.0f;
            }
        }
        // 이동 입력이 없을 때(Idle)는 targetWalk와 targetRun 모두 0으로 유지됩니다. (Walk=0, Run=0 상태가 됨)

        // 계산된 목표 값을 애니메이터 파라미터에 부드럽게 적용합니다.
        playerCharacter.animator.SetFloat("Walk", targetWalk, AnimationDamping, Time.fixedDeltaTime);
        playerCharacter.animator.SetFloat("Run", targetRun, AnimationDamping, Time.fixedDeltaTime);

        // ====================================================================

        // 이동 입력이 있을 경우에만 회전을 처리합니다.
        if (inputMagnitude > 0.01f) // 실제로 움직이고 있을 때만 회전
        {
            // 이동 방향(movement) 벡터를 바라보는 회전(Quaternion)을 계산합니다.
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.z).normalized);

            // 현재 회전(transform.rotation)을 목표 회전(targetRotation)으로 부드럽게 보간합니다.
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