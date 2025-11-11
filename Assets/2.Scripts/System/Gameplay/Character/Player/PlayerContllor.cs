using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

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
    //  (수정된 부분) jumpForce를 속도에 곱해지는 최종 배율로 변경하고 기본값을 0.5f로 변경했습니다.
    [Tooltip("점프 시 적용될 힘의 크기입니다. (이 값이 현재 이동 속도에 곱해져 최종 점프력이 결정됩니다. 1.0f는 속도 그대로 반영을 의미합니다.)")]
    public float jumpForce = 0.5f; 
    
    [Header("회전 설정")]
    [Tooltip("플레이어가 이동 방향으로 회전하는 속도입니다. 값이 높을수록 더 빠르게 회전합니다.")]
    public float rotationSpeed = 10f;
    [Header("점프 설정")] // 점프 딜레이 변수 추가
    [Tooltip("점프 애니메이션 시작 후 물리적인 힘이 적용되기까지의 지연 시간입니다. (애니메이션 싱크용)")]
    public float jumpDelayTime = 0.3f; // 0.3초 정도가 일반적인 준비 동작에 해당합니다.

    // 컴포넌트 변수
    private Rigidbody playerRigidbody;

    // === 발소리 관련 변수 ===
    private AudioSource playerAudioSource; //발소리 재생용 AudioSource
    [Header("발소리 설정")] // 오디오 클립 및 설정
    [Tooltip("걷기 시 재생할 발소리 클립 목록입니다. 여러 개를 등록하여 랜덤 재생할 수 있습니다.")]
    public List<AudioClip> walkFootstepClips; // 걷기 소리
    [Tooltip("달리기 시 재생할 발소리 클립 목록입니다. 걷기보다 더 빠르게 재생됩니다.")]
    public List<AudioClip> runFootstepClips; // 뛰기 소리
    [Tooltip("점프 시 재생할 효과음 클립입니다.")]
    public AudioClip jumpSoundClip; // 점프 효과음
    // 현재 발소리 재생 간격 타이머
    private float footstepTimer;
    [Tooltip("걷기 시 발소리가 재생되는 최소 간격(초)입니다.")]
    public float walkFootstepInterval = 0.5f; // 걷기 재생 주기 (느리게)
    [Tooltip("달리기 시 발소리가 재생되는 최소 간격(초)입니다.")]
    public float runFootstepInterval = 0.3f; // 뛰기 재생 주기 (빠르게)
    // =============================

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

        //AudioSource 컴포넌트 가져오기
        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
        {
            // 발소리는 3D 사운드가 적합하므로 AudioSource가 필수입니다.
            Debug.LogError("AudioSource 컴포넌트를 찾을 수 없습니다. 발소리 재생이 불가능합니다!");
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
            if (UITutorialHandler.Instance != null)
            { UITutorialHandler.Instance.OnAimingPerformed.Invoke(); }
        }
        else
        {
            // 마우스 버튼을 떼거나 움직일 수 없으면 시각화 숨김
            if (playerCharacter.playerAttack.visualizerContainer != null && playerCharacter.playerAttack.visualizerContainer.activeSelf)
            {
                playerCharacter.playerAttack.visualizerContainer.SetActive(false);
            }
        }

        // 땅에 닿았을 때만 점프 가능하며, canMove이 true일 때만 점프를 허용합니다.
        if (canMove && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 기존의 즉시 점프 로직을 코루틴 실행으로 대체합니다.
            StartCoroutine(PerformJumpWithDelay());
        }
    }

    // 물리학 업데이트는 FixedUpdate에서 처리하는 것이 좋습니다.
    void FixedUpdate()
    {
        if (!canMove)
        {
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
            playerCharacter.animator.SetFloat("Walk", 0, AnimationDamping, Time.fixedDeltaTime);
            playerCharacter.animator.SetFloat("Run", 0, AnimationDamping, Time.fixedDeltaTime);

            // 움직이지 않을 때는 타이머 리셋
            footstepTimer = 0f;
            return;
        }

        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 이동 속도를 업데이트할 수 없습니다.");
            return;
        }

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

        if (inputMagnitude > 0.01f) // 이동 중인 경우
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

            //발소리 재생 로직 호출
            HandleFootsteps(isRunning);
        }
        else // 정지 상태
        {
            //정지 상태일 때는 타이머 리셋
            footstepTimer = 0f;
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
    /// 이동 상태에 따라 발소리 재생을 관리합니다.
    /// </summary>
    /// <param name="isRunning">현재 달리기 상태인지 여부</param>
    private void HandleFootsteps(bool isRunning) //발소리 관리 메서드
    {
        // 땅에 닿지 않았거나 오디오 소스가 없으면 재생하지 않습니다.
        if (!isGrounded || playerAudioSource == null) return;

        // 걷기 또는 뛰기 간격 설정
        float currentInterval = isRunning ? runFootstepInterval : walkFootstepInterval;

        // 타이머 업데이트
        footstepTimer += Time.fixedDeltaTime;

        // 설정된 간격이 지났다면 발소리 재생
        if (footstepTimer >= currentInterval)
        {
            PlayFootstepSound(isRunning);
            footstepTimer = 0f; // 타이머 리셋
        }
    }

    /// <summary>
    /// 걷기 또는 뛰기 클립 목록에서 랜덤으로 클립을 선택하여 재생합니다.
    /// 볼륨 배율을 2.0f로 설정하여 기본 볼륨보다 2배 크게 재생합니다.
    /// </summary>
    /// <param name="isRunning">현재 달리기 상태인지 여부</param>
    private void PlayFootstepSound(bool isRunning) // 실제 사운드 재생 메서드
    {
        List<AudioClip> clips = isRunning ? runFootstepClips : walkFootstepClips;

        // 클립 목록이 비어있거나 오디오 소스가 없으면 재생하지 않습니다.
        if (clips.Count == 0 || playerAudioSource == null) return;

        // 클립 목록에서 랜덤으로 하나 선택
        int randomIndex = UnityEngine.Random.Range(0, clips.Count);
        AudioClip clipToPlay = clips[randomIndex];

        // PlayOneShot의 두 번째 인수로 2.0f를 전달하여 볼륨을 2배로 높입니다.
        const float volumeMultiplier = 2.0f;

        // PlayOneShot을 사용하여 다른 사운드와 겹치지 않게 재생
        playerAudioSource.PlayOneShot(clipToPlay, volumeMultiplier);
    }

    /// <summary>
    /// 마우스 포인터가 월드 공간에서 가리키는 지점을 바라보도록 플레이어를 회전시키는 메서드입니다.
    /// </summary>
    public void RotateTowardsMouseCursor()
    {
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
    
    // (최종 수정된 부분) 점프력을 현재 수평 속도에 비례하도록 정확히 반영합니다.
    /// <summary>
    /// 점프 애니메이션 시작 후, 지정된 딜레이 후에 물리적인 점프를 실행합니다.
    /// 최종 점프력은 '현재 수평 속도'에 'jumpForce' 변수(배율 역할)를 곱하여 결정됩니다.
    /// SOLID 원칙: 단일 책임 원칙 (점프 실행의 순차적 책임을 가짐)
    /// </summary>
    private IEnumerator PerformJumpWithDelay()
    {
        // 1. **점프 힘 계산**: 현재 Rigidbody의 수평 속도의 크기(magnitude)를 점프력에 반영합니다.
        
        // Rigidbody의 현재 수평 속도(x, z)를 가져와 크기를 계산합니다. 
        // 이 값이 걷기 속도(예: 10) 또는 달리기 속도(예: 20)가 됩니다.
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        float currentMovementSpeed = horizontalVelocity.magnitude;
        
        // 최종 점프력 = (현재 이동 속도) * (jumpForce 변수 값: 배율 역할)
        float actualJumpForce = currentMovementSpeed * jumpForce;
        
        // 정지 상태일 때 (속도가 0에 가까울 때) 점프력도 0에 가까워집니다. 
        // 이는 요청하신 '이동 속도 그대로 반영'을 충족합니다.

        // 점프 효과음 재생
        if (playerAudioSource != null && jumpSoundClip != null)
        {
            playerAudioSource.PlayOneShot(jumpSoundClip);
        }

        // 2. 애니메이터에게 점프 시작을 알립니다. (점프 준비 동작 시작)
        playerCharacter.animator.SetTrigger("Jump");

        // 3. jumpDelayTime 만큼 대기 (애니메이션 싱크를 맞추는 핵심)
        yield return new WaitForSeconds(jumpDelayTime);

        // 4. 딜레이 후, 계산된 실제 점프 힘을 적용하여 실제 점프를 실행합니다.
        playerRigidbody.AddForce(Vector3.up * actualJumpForce, ForceMode.Impulse);

        // 5. 땅에 닿지 않은 상태로 변경
        isGrounded = false;
    }

    // ... (기존 스폰 관련 public 메서드 유지)
    public void inDungeon()
    {
        canMove = false;
        playerCharacter.transform.position = dungeonSpawnPoint.position;
        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Dungeon_Enter);
        }
        canMove = true;
    }
    public void outDungeon()
    {
        canMove = false;
        playerCharacter.transform.position = exitSpawnPoint.position;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Dungeon_Exit);
        }
        canMove = true;
    }
    public void enterBossRoom()
    {
        canMove = false;
        playerCharacter.transform.position = bossRoomSpawnPoint.position;
        canMove = true;
    }
}