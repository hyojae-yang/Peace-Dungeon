using UnityEngine;
using System.Collections;
using NUnit.Framework; // 코루틴 사용을 위해 필요

/// <summary>
/// 플레이어 캐릭터를 따라다니는 망치(MangChi) 펫의 행동을 제어하는 스크립트입니다.
/// CharacterController를 사용하여 물리 시스템에 안전하게 이동하며, 자유 활동 반경 내에서 무작위로 움직입니다.
/// SOLID 원칙: SRP (단일 책임 원칙)에 따라 이동, 애니메이션 제어, 휴식 로직을 분리된 영역에서 처리합니다.
/// </summary>
public class MangChi : MonoBehaviour
{
    // [유일성 및 초기화]
    public static MangChi Instance { get; private set; }
    [Header("사망 설정")]
    [Tooltip("사망 애니메이션을 재생할 Animator Trigger 매개변수 이름입니다.")]
    [SerializeField]
    private string deathAnimTrigger = "DoDie"; // 사망 트리거 이름 (Animator에 설정 필요)

    [Tooltip("현재 펫이 사망 상태인지 여부입니다.")]
    private bool isDead = false; // 사망 상태 플래그
    // [이동 설정]
    [Header("펫 이동 설정")]
    [Tooltip("플레이어와 망치가 자유롭게 움직일 수 있는 활동 반경입니다. 이 거리를 벗어나면 복귀합니다.")]
    [SerializeField] private float followDistance = 4f;

    [Tooltip("이 거리를 초과하면 망치가 플레이어에게 즉시 순간이동(텔레포트)합니다.")]
    [SerializeField] private float teleportDistance = 15f;

    [Tooltip("이동 시 목표 위치에 도달하는 부드러움 정도입니다. (값이 작을수록 빠르게 반응)")]
    [SerializeField] private float smoothTime = 0.5f;

    [Tooltip("망치가 이동하는 최대 속도입니다. (Initialize에서 플레이어 스탯 기반으로 설정됨)")]
    [SerializeField] private float moveSpeed = 6f; // 초기값

    // [자유 이동 및 휴식 제어 상수]
    private const float WANDER_TIME = 2.5f;
    private const float REACH_THRESHOLD = 0.5f;
    private const float WANDER_SPEED_MULTIPLIER = 0.6f; // 자유 배회 시 이동 속도 배율 (0.6f)
    private const int REST_INTERVAL = 5; // 4~6 목표마다 휴식 (평균 5회로 설정)
    private const float MIN_REST_TIME = 3f; // 최소 휴식 시간(초)
    private const float MAX_REST_TIME = 10f; // 최대 휴식 시간(초)

    // [물리 및 중력 상수]
    private const float GRAVITY = -9.81f; // 유니티 표준 중력 값

    // [애니메이션 제어 상수]
    [Header("애니메이션 전환 설정")]
    [Tooltip("애니메이터 매개변수가 목표 값으로 전환되는 속도입니다. (값이 클수록 빠르게 전환)")]
    [SerializeField] private float animFlowSpeed = 5.0f; // 전환 속도 제어 상수 (CreatureMover 참고)

    [Tooltip("이동 속도 Magnitude가 이 값 이하일 때 펫은 멈췄다고 간주하고 Vert를 0으로 설정합니다.")]
    private const float ANIMATION_MOVE_THRESHOLD = 0.1f; // 제자리 걸음 방지 임계값 (매우 중요)

    // [내부 변수]
    private PlayerCharacter owner;
    private Vector3 currentVelocity;                 // SmoothDamp 함수가 사용하는 내부 속도 변수
    private Vector3 currentTargetPosition;           // 망치가 현재 가야 할 목표 지점 (랜덤 또는 플레이어 복귀 지점)
    private float wanderTimer;                       // 다음 랜덤 목표를 설정할 때까지의 남은 시간

    private Animator animator;                       // 애니메이터 컴포넌트 참조
    private CharacterController characterController; // CharacterController 컴포넌트 참조
    private Vector3 verticalVelocity;                // 중력 및 수직 이동 속도

    private Vector3 smoothHorizontalVelocity;        // 캐릭터의 실제 수평 이동 속도 (애니메이션 제어용)

    // [애니메이션 플로우 변수]
    private float currentFlowVert = 0.0f;           // Vert 매개변수의 현재 흐름 값 (움직임 유무)
    private float currentFlowState = 0.0f;          // State 매개변수의 현재 흐름 값 (걷기/뛰기 모드)

    // [휴식/파밍 상태 변수]
    private bool isResting = false;                  // 현재 망치가 휴식 중인지 여부
    private int targetWaypointCount = 0;             // 목표 지점에 도달한 횟수 (휴식 트리거용)
    // private Coroutine _restCoroutine;              // 휴식 코루틴 참조 (필요 시 사용)

    // [아이템 파밍 로직 주석 처리 (논의 보류)]
    [Header("아이템 파밍 설정")]
    [Tooltip("파밍 시도 간 최소 대기 시간 (초)")]
    [SerializeField] private float minLootInterval = 30f;
    [Tooltip("파밍 시도 간 최대 대기 시간 (초)")]
    [SerializeField] private float maxLootInterval = 15 * 60f;
    private Coroutine _lootCoroutine; // 파밍 코루틴 참조 변수
    [SerializeField] BaseItemSO[] baseItemSO; // 파밍할 아이템 리스트

    private void Awake()
    {
        // A. 싱글톤 인스턴스 관리
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되도록 DontDestroyOnLoad 제거
        }
        else
        {
            // 씬 로드 시 펫이 두 번 생성되면 새 인스턴스를 파괴
            Destroy(gameObject);
            return;
        }

        // B. MainSceneManager 이벤트 구독 (Die 메서드는 A에서만 실행됨)
        if (MainSceneManager.Instance != null)
        {
            MainSceneManager.OnGameOver += Die;
        }
    }

    /// <summary>
    /// 소환 직후 호출되어 주인 정보와 초기 설정을 완료합니다.
    /// </summary>
    public void Initialize(PlayerCharacter player)
    {
        owner = player;

        // CharacterController 참조 및 안전성 확보
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("[MangChi] CharacterController 컴포넌트를 찾을 수 없습니다. 이동 로직이 정상 작동하지 않습니다!");
            return;
        }

        // Animator 참조 및 설정
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[MangChi] Animator 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            // 애니메이션 플로우 변수 초기 설정
            currentFlowVert = 1.0f;
            currentFlowState = 0.0f;

            animator.SetFloat("Vert", currentFlowVert);
            animator.SetFloat("State", currentFlowState);
        }

        // 이동 속도 설정
        if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerStats != null)
        {
            // 플레이어 스탯 기반으로 펫의 최대 이동 속도 설정
            moveSpeed = PlayerCharacter.Instance.playerStats.moveSpeed * 1.2f;
        }

        wanderTimer = WANDER_TIME;
        targetWaypointCount = 0; // 휴식 카운터 초기화
        TeleportToOwner();
        // =======================================================
        // [핵심 추가] 던전 이벤트 구독 (펫의 파밍 기능을 던전 상태와 연동)
        // =======================================================
        if (DungeonManager.Instance != null)
        {
            // 실제 파밍 로직 연결 (다음 단계에서 StartLooting/StopLooting으로 변경될 예정)
            DungeonManager.OnDungeonEnter += StartLooting;
            DungeonManager.OnDungeonExit += StopLooting;
        }
        else
        {
            Debug.LogError("[MangChi] DungeonManager 인스턴스를 찾을 수 없습니다. 이벤트 구독 실패!");
        }
    }

    private void Update()
    {
        if (isDead)
        {
            // 사망 시에는 이동 및 애니메이션 업데이트를 수행하지 않습니다.
            return;
        }
        // 필수 컴포넌트가 없으면 로직을 건너뜁니다.
        if (owner == null || characterController == null) return;

        // **[1] 이동 목표 및 애니메이션 목표 State 설정**
        float targetState = 0.0f; // 기본: 걷기

        // 펫이 쉬는 중이 아닐 때만 이동 로직 실행
        if (!isResting)
        {
            Vector3 currentPosition = transform.position;
            Vector3 playerCenter = owner.transform.position;
            float currentDistanceToPlayer = Vector3.Distance(currentPosition, playerCenter);

            // 1. 비상 체크 (순간이동)
            if (currentDistanceToPlayer > teleportDistance)
            {
                TeleportToOwner();
                return;
            }

            // 2. 목표 위치 계산 및 속도/State 설정 (복귀/자유 배회 모드)
            float currentTargetMoveSpeed = moveSpeed;

            if (currentDistanceToPlayer > followDistance)
            {
                // 복귀 모드: 빠르고 멀리 이동
                targetState = 1.0f; // 뛰기 애니메이션 (State=1)

                currentTargetPosition = playerCenter;
                wanderTimer = WANDER_TIME;
                targetWaypointCount = 0; // 복귀 시 카운터 초기화
            }
            // 3. 자유/배회 모드: 목표 도달 또는 시간 초과 시
            else
            {
                // 배회 모드: 걷기 애니메이션 (State=0)
                targetState = 0.0f;
                currentTargetMoveSpeed *= WANDER_SPEED_MULTIPLIER; // 속도 감속 (0.6배율)

                if (Vector3.Distance(currentPosition, currentTargetPosition) < REACH_THRESHOLD || wanderTimer <= 0f)
                {
                    // 휴식 로직 체크:
                    targetWaypointCount++;
                    if (targetWaypointCount >= REST_INTERVAL)
                    {
                        StartResting();
                        targetWaypointCount = 0;
                        return; // 휴식 코루틴이 시작되었으므로 이번 프레임의 이동 로직을 건너뜁니다.
                    }

                    // 새로운 목표 위치 설정
                    Vector2 randomCircle = Random.insideUnitCircle * followDistance;
                    currentTargetPosition = playerCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);
                    wanderTimer = WANDER_TIME;
                }

                // 타이머 감소
                wanderTimer -= Time.deltaTime;
            }

            // **[2] 이동 및 회전 로직**

            // 4. 수평 이동 속도 계산 (SmoothDamp)
            Vector3 directionToTarget = (currentTargetPosition - currentPosition).normalized;
            directionToTarget.y = 0;

            // CharacterController의 현재 수평 속도를 목표 방향으로 부드럽게 전환
            Vector3 targetHorizontalVelocity = directionToTarget * currentTargetMoveSpeed;

            // Vector3.SmoothDamp를 사용하여 현재 속도를 목표 속도로 부드럽게 조정
            smoothHorizontalVelocity = Vector3.SmoothDamp( // 클래스 변수에 할당하여 애니메이션에 사용
                current: new Vector3(characterController.velocity.x, 0, characterController.velocity.z),
                target: targetHorizontalVelocity,
                currentVelocity: ref currentVelocity,
                smoothTime: smoothTime
            );

            // 5. 중력 적용
            if (characterController.isGrounded)
            {
                verticalVelocity.y = -0.5f;
            }
            else
            {
                verticalVelocity.y += GRAVITY * Time.deltaTime;
            }

            // 6. 최종 이동 실행 (CharacterController.Move)
            Vector3 finalMotion = (smoothHorizontalVelocity + verticalVelocity) * Time.deltaTime;
            characterController.Move(finalMotion);

            // 7. 부드러운 회전
            if (smoothHorizontalVelocity.magnitude > ANIMATION_MOVE_THRESHOLD)
            {
                Quaternion targetRotation = Quaternion.LookRotation(smoothHorizontalVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            // 휴식 중일 때 State 목표는 0.0f입니다.
            targetState = 0.0f;
            // 휴식 중에는 움직임이 없으므로 smoothHorizontalVelocity의 magnitude는 0에 가깝습니다.
        }

        // **[3] 애니메이션 Vert 목표 값 설정 (제자리 걸음 방지)**
        float targetVert = 0.0f;

        // 펫이 움직이는 중이거나, 움직임을 멈추고 휴식 상태가 아닐 때만 움직임 애니메이션을 목표합니다.
        // 움직이는 중 : smoothHorizontalVelocity.magnitude > 임계값
        // 회전만 하는 중 : smoothHorizontalVelocity.magnitude <= 임계값 (이때는 Vert=0이 되어 멈춤)
        if (!isResting && smoothHorizontalVelocity.magnitude > ANIMATION_MOVE_THRESHOLD)
        {
            targetVert = 1.0f;
        }

        // **[4] 애니메이션 플로우 적용**
        if (animator != null)
        {
            // Vert 매개변수 플로우: 현재 값에서 목표 값(0 또는 1)으로 부드럽게 전환
            currentFlowVert = Mathf.MoveTowards(currentFlowVert, targetVert, Time.deltaTime * animFlowSpeed);
            animator.SetFloat("Vert", currentFlowVert);

            // State 매개변수 플로우: 현재 값에서 목표 값(0 또는 1)으로 부드럽게 전환
            currentFlowState = Mathf.MoveTowards(currentFlowState, targetState, Time.deltaTime * animFlowSpeed);
            animator.SetFloat("State", currentFlowState);
        }
    }

    /// <summary>
    /// 펫을 주인 주변의 목표 위치로 즉시 순간이동시키는 유틸리티 메서드입니다.
    /// </summary>
    private void TeleportToOwner()
    {
        if (characterController != null) characterController.enabled = false;

        // 텔레포트 위치 계산 및 할당
        Vector3 teleportOffset = -owner.transform.forward * followDistance;
        transform.position = owner.transform.position + teleportOffset;

        if (characterController != null) characterController.enabled = true;

        // 이동 및 목표 관련 변수 초기화
        currentVelocity = Vector3.zero;
        verticalVelocity = Vector3.zero;
        currentTargetPosition = transform.position;
        wanderTimer = WANDER_TIME;
        isResting = false;

        // 애니메이션 플로우 변수 초기화 (즉시 걷기 모드 시작을 위해)
        currentFlowVert = 1.0f;
        currentFlowState = 0.0f;
        if (animator != null)
        {
            animator.SetFloat("Vert", currentFlowVert);
            animator.SetFloat("State", currentFlowState);
        }
    }

    /// <summary>
    /// 망치가 휴식을 시작하도록 설정하고 코루틴을 시작합니다.
    /// </summary>
    private void StartResting()
    {
        isResting = true;
        // 휴식 애니메이션 목표는 Update()의 플로우 로직에서 처리됩니다. (targetVert=0, targetState=0)

        StartCoroutine(RestingRoutine());
    }

    /// <summary>
    /// 무작위 시간 동안 망치의 이동을 멈추게 하는 코루틴입니다.
    /// </summary>
    private IEnumerator RestingRoutine()
    {
        float restTime = Random.Range(MIN_REST_TIME, MAX_REST_TIME);

        yield return new WaitForSeconds(restTime);

        // 대기 후 휴식 종료
        isResting = false;

        // 휴식 후 다음 움직임 목표는 Update()의 플로우 로직에서 걷기(Vert=1, State=0)로 자동 처리됩니다.
    }

    private void OnDestroy()
    {
        // A. 싱글톤 인스턴스 해제 로직 (Awake에서 Instance = this를 했다면 필요)
        if (Instance == this)
        {
            // 핵심 수정: 이 인스턴스가 파괴될 때만 전역 변수 Instance를 null로 설정합니다.
            Instance = null;
        }

        // B. 이벤트 구독 해제 (기존 로직 유지)
        if (MainSceneManager.Instance != null)
        {
            MainSceneManager.OnGameOver -= Die;
        }

        // C. 던전 이벤트 구독 해제 (기존 로직 유지)
        if (DungeonManager.Instance != null)
        {
            DungeonManager.OnDungeonEnter -= StartLooting;
            DungeonManager.OnDungeonExit -= StopLooting;
        }
    }
    /// <summary>
    /// MainSceneManager.OnGameOver 이벤트 발생 시 호출됩니다.
    /// 펫의 모든 행동을 멈추고 사망 모션을 실행합니다.
    /// </summary>
    private void Die()
    {
        if (this == null || gameObject == null || isDead) return; // 이 한 줄 추가를 강력히 권장합니다.

        isDead = true;

        // 1. 모든 코루틴 중지 (특히 휴식 및 파밍 루틴)
        StopAllCoroutines();
        _lootCoroutine = null; // 파밍 코루틴 참조 해제
        isResting = false; // 휴식 상태 해제

        // 2. 물리 제어 비활성화 (더 이상 움직이지 않도록)
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // 3. 사망 애니메이션 트리거
        if (animator != null && !string.IsNullOrEmpty(deathAnimTrigger))
        {
            animator.SetTrigger(deathAnimTrigger);
        }

        // 4. 디버그 및 추가 정리 로직
        // TODO: (선택 사항) 사망 후 일정 시간 뒤에 펫 오브젝트 비활성화/파괴 로직 추가 가능
    }
    /// <summary>
    /// 던전 진입 시 호출되어 파밍 루틴을 시작합니다.
    /// </summary>
    private void StartLooting()
    {
        // [핵심 수정]: MissingReferenceException 방지 가드
        // 오브젝트가 파괴되었거나, 비활성화된 상태에서 StartCoroutine 호출을 시도하는 것을 막습니다.
        // isDead 상태일 때도 코루틴을 시작하지 않도록 방어합니다.
        if (!this.isActiveAndEnabled || isDead)
        {
            // Debug.LogWarning("MangChi 인스턴스가 유효하지 않거나 사망 상태라 StartLooting을 무시했습니다.");
            return;
        }

        if (_lootCoroutine != null) return; // 이미 실행 중이면 중복 방지

        // _lootCoroutine = StartCoroutine(LootingRoutine()); // 기존 코드
        _lootCoroutine = StartCoroutine(LootingRoutine());
    }
    /// <summary>
    /// 던전 퇴장 시 호출되어 파밍 루틴을 중지합니다.
    /// </summary>
    private void StopLooting()
    {
        if (_lootCoroutine != null)
        {
            StopCoroutine(_lootCoroutine); // 다음 단계에서 활성화
            _lootCoroutine = null;
        }
    }
    // =======================================================
    // [핵심 추가] 랜덤 파밍 코루틴 및 로직
    // =======================================================

    /// <summary>
    /// 던전 내에서 무작위 시간 간격으로 아이템을 파밍하는 무한 루프 코루틴입니다.
    /// 휴식 상태(isResting)와 관계없이 독립적으로 작동합니다. (요청 사항 반영)
    /// SRP: 무작위 시간 간격으로 파밍 행동을 트리거하는 시간 제어 책임.
    /// </summary>
    private IEnumerator LootingRoutine()
    {
        // S: 무한 루프는 이 코루틴이 DungeonManager.OnDungeonExit 이벤트가 발생할 때까지 계속 실행되게 합니다.
        while (true)
        {
            // 1. 무작위 대기 시간 계산
            float waitTime = UnityEngine.Random.Range(minLootInterval, maxLootInterval);

            // 2. 대기
            yield return new WaitForSeconds(waitTime);

            // 3. 아이템 획득 로직 실행
            LootItemRandomly();
        }
    }

    /// <summary>
    /// 미리 정의된 아이템 목록에서 랜덤으로 아이템을 선택하고 디버그 로그를 출력합니다.
    /// SRP: 실제 아이템 선택 로직 및 피드백 처리 책임.
    /// </summary>
    private void LootItemRandomly()
    {
        // A. 아이템 목록 유효성 검사
        if (baseItemSO == null || baseItemSO.Length == 0)
        {
            Debug.LogWarning("[MangChi] 파밍할 아이템 목록 (baseItemSO)이 비어있거나 null입니다! 아이템 파밍 실패.");
            return;
        }

        // B. 랜덤 아이템 선택
        int randomIndex = UnityEngine.Random.Range(0, baseItemSO.Length);
        BaseItemSO lootedItem = baseItemSO[randomIndex];

        // C. 디버그 로그 출력 (선택된 아이템 정보 출력)
        // ItemDatabase나 InventoryManager 연동은 다음 단계에서 진행합니다.
        if (lootedItem != null)
        {
            // BaseItemSO 클래스에 'itemName' 필드 또는 프로퍼티가 있다고 가정합니다.
            // 인벤토리 매니저가 유효한지 추가 검사
            if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
            {
                string itemName = lootedItem.itemName;
                PlayerCharacter.Instance.inventoryManager.AddItem(lootedItem); // 인벤토리에 아이템 추가

                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(
                        $"망치가 {itemName} 아이템을 획득했습니다!",
                        NotificationType.Success // Success 타입으로 호출
                    );
                }
            }
            else
            {
                Debug.LogError("[MangChi] PlayerCharacter.Instance 또는 inventoryManager가 null입니다. 아이템 획득 알림 및 인벤토리 추가 실패.");
            }

            // TODO: (다음 단계) DungeonInventoryManager.Instance.AddPlayerItem(lootedItem.itemID); 로직 추가 예정
        }
        else
        {
            Debug.LogWarning("[MangChi] 아이템 목록에서 Null 객체가 선택되었습니다. 파밍 실패.");
        }
    }
    // === [핵심: 저장/로드 메서드 추가] ===

    /// <summary>
    /// 현재 펫의 소환 상태를 PetSystemSaveData 객체로 추출하여 반환합니다.
    /// SOLID: SRP (데이터 추출 책임) - 오직 소환 여부만 보고합니다.
    /// </summary>
    /// <returns>현재 펫의 저장 데이터 객체</returns>
    public PetSystemSaveData GetSaveData()
    {
        // 펫이 현재 씬에 살아있고(isDead=false), 활성화되어 있다면 소환된 것으로 간주합니다.
        bool isCurrentlySummoned = !isDead && gameObject.activeInHierarchy;

        return new PetSystemSaveData
        {
            isMangChiSummoned = isCurrentlySummoned
        };
    }

    /// <summary>
    /// 로드된 PetSystemSaveData를 바탕으로 펫의 상태를 복원합니다.
    /// 로드 시 위치나 상태를 복원할 필요 없이, PetManager가 펫을 생성한 후 초기화만 수행합니다.
    /// SOLID: SRP (데이터 복원 책임)
    /// </summary>
    /// <param name="data">로드된 펫 데이터</param>
    public void LoadData(PetSystemSaveData data)
    {
        // 현재는 소환 여부 외에 복원할 상태 데이터가 없으므로, 로드된 상태에 대한 특별한 로직은 없습니다.
        isDead = false; // 로드 시에는 죽은 상태 해제
    }
}