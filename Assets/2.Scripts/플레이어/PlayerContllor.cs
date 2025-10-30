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
    public float walkSpeed = 10f;
    [Tooltip("달리기 시 적용될 속도 배율입니다.")]
    public float runSpeedMultiplier = 2f;
    [Tooltip("점프 시 적용될 힘의 크기입니다.")]
    public float jumpForce = 5f;

    [Header("회전 설정")]
    [Tooltip("플레이어가 이동 방향으로 회전하는 속도입니다. 값이 높을수록 더 빠르게 회전합니다.")]
    public float rotationSpeed = 10f;

    // 컴포넌트 변수
    private Rigidbody playerRigidbody;

    [Header("스폰 포인트 설정")]
    [SerializeField] private Transform dungeonSpawnPoint;
    [SerializeField] private Transform exitSpawnPoint;
    [SerializeField] private Transform bossRoomSpawnPoint;

    // 상태 변수
    [Tooltip("플레이어가 땅에 닿았는지 여부를 나타냅니다.")]
    private bool isGrounded = true;
    public bool canMove = true;
    [Tooltip("현재 마우스 커서 방향으로 회전 중인지 여부를 나타냅니다. 이 상태일 때는 키보드 이동 방향 회전 로직이 비활성화됩니다.")]
    private bool isRotatingByMouse = false;

    // 애니메이션 블렌딩을 위한 변수입니다.
    [Tooltip("걷기/달리기 애니메이션 상태 변화를 부드럽게 만들기 위한 속도입니다.")]
    private const float AnimationDamping = 0.1f;

    void Start()
    {
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody 컴포넌트를 찾을 수 없습니다. 플레이어 오브젝트에 Rigidbody를 부착해 주세요.");
            return;
        }

        playerRigidbody.freezeRotation = true;

        if (playerCharacter.playerStats != null)
        {
            walkSpeed = playerCharacter.playerStats.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStats가 PlayerCharacter에 할당되지 않았습니다. 기본 walkSpeed를 사용합니다.");
        }
        canMove = true;
        playerCharacter.animator.SetFloat("Walk", 0);
        playerCharacter.animator.SetFloat("Run", 0);
    }

    void Update()
    {
        // 1. 마우스 회전 상태 초기화
        isRotatingByMouse = false;

        // canMove 상태이고 마우스 오른쪽 버튼을 누르고 있을 때 마우스 방향으로 회전 및 범위 시각화 업데이트
        if (canMove && Input.GetMouseButton(1))
        {
            RotateTowardsMouseCursor();

            // [수정된 로직] 무기 장착 여부를 최우선으로 체크합니다.
            if (playerCharacter.playerAttack.equippedWeapon != null)
            {
                // 무기가 있으면 모든 무기 타입에 대해 시각화 업데이트 요청
                playerCharacter.playerAttack.UpdateVisualizerShape();
            }
            else
            {
                // 무기가 없으면 시각화 오브젝트 비활성화 (Null 에러 방지)
                if (playerCharacter.playerAttack.visualizerContainer != null && playerCharacter.playerAttack.visualizerContainer.activeSelf)
                {
                    playerCharacter.playerAttack.visualizerContainer.SetActive(false);
                }
            }
        }
        else
        {
            // 마우스 버튼을 떼거나 움직일 수 없으면 시각화 숨김
            if (playerCharacter.playerAttack.visualizerContainer != null && playerCharacter.playerAttack.visualizerContainer.activeSelf)
            {
                playerCharacter.playerAttack.visualizerContainer.SetActive(false);
            }
        }

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
            // ... (기존 이동 불가 로직 유지)
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
            playerCharacter.animator.SetFloat("Walk", 0, AnimationDamping, Time.fixedDeltaTime);
            playerCharacter.animator.SetFloat("Run", 0, AnimationDamping, Time.fixedDeltaTime);
            return;
        }

        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 이동 속도를 업데이트할 수 없습니다.");
            return;
        }

        // ... (기존 이동 및 애니메이션 로직 유지)
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        Vector3 rawMovement = new Vector3(xInput, 0f, zInput).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = walkSpeed;
        if (isRunning)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        Vector3 movement = rawMovement * currentSpeed;
        Vector3 newVelocity = new Vector3(movement.x, playerRigidbody.linearVelocity.y, movement.z);
        playerRigidbody.linearVelocity = newVelocity;

        float inputMagnitude = rawMovement.magnitude;
        float targetWalk = 0.0f;
        float targetRun = 0.0f;

        if (inputMagnitude > 0.01f)
        {
            targetWalk = inputMagnitude;
            if (isRunning)
            {
                targetRun = inputMagnitude;
            }
            else
            {
                targetRun = 0.0f;
            }
        }

        playerCharacter.animator.SetFloat("Walk", targetWalk, AnimationDamping, Time.fixedDeltaTime);
        playerCharacter.animator.SetFloat("Run", targetRun, AnimationDamping, Time.fixedDeltaTime);

        // 이동 입력이 있을 경우에만 회전을 처리합니다.
        if (!isRotatingByMouse && inputMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 마우스 포인터가 월드 공간에서 가리키는 지점을 바라보도록 플레이어를 회전시키는 메서드입니다.
    /// </summary>
    public void RotateTowardsMouseCursor()
    {
        // ... (기존 RotateTowardsMouseCursor 로직 유지)
        int layerMask = LayerMask.GetMask("Ground");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            Vector3 targetPoint = hit.point;
            Vector3 direction = targetPoint - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            isRotatingByMouse = true;
        }
    }

    // 땅에 닿았는지 확인
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // ... (기존 이동 관련 public 메서드 유지)
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