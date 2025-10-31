using UnityEngine;
using System.Collections;

/// <summary>
/// 다람쥐 몬스터의 고유한 행동 로직(도망치기)을 담당하는 클래스입니다.
/// MonsterPatrol 컴포넌트를 제어합니다.
/// SOLID 원칙: SRP(단일 책임 원칙)에 따라 다람쥐의 고유 행동 로직(Flee 결정 및 실행)만 담당합니다.
/// </summary>
public class SquirrelBehavior : MonoBehaviour
{
    // === 도망 행동 설정 ===
    [Header("도망 행동 설정")]
    // [제거] fleeDistance: Monster.detectionRange를 사용하여 시야 감지를 합니다.
    [Tooltip("플레이어가 시야에서 벗어났을 때 몬스터가 멈출 거리입니다. (도주 목표 거리)")]
    public float stopFleeDistance = 20f;
    [Tooltip("도망치는 방향으로의 이동 속도 배율입니다.")]
    public float fleeSpeedMultiplier = 1.5f;
    // [추가] 회전 속도 (도주 시 타겟 반대 방향으로 부드럽게 회전)
    [Tooltip("도주 상태에서 플레이어 반대 방향으로 회전하는 속도입니다.")]
    [SerializeField] private float rotationSpeed = 10.0f;
    [Header("사운드 설정")]
    [Tooltip("플레이어 감지 시 한 번 재생되는 놀라는 소리(예: '찍')")]
    public AudioClip startledClip;

    // === 종속성 ===
    private Monster monster;
    private MonsterPatrol monsterPatrol;
    private Transform playerTransform;
    private Animator animator;
    private AudioSource audioSource;

    // 몬스터의 기본 순찰 속도 참조 (도망 속도 계산에 사용)
    private float basePatrolSpeed = 3.0f;

    void Awake()
    {
        // === 필수 컴포넌트 참조 ===
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (monster == null || monsterPatrol == null || animator == null)
        {
            Debug.LogError("SquirrelBehavior: 필수 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // MonsterData에 moveSpeed가 있다면 기본 순찰 속도로 사용
        if (monster.monsterData != null)
        {
            basePatrolSpeed = monster.monsterData.moveSpeed;
        }
    }

    private void Start()
    {
        // 초기 상태를 Patrol로 설정합니다.
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
        // 기본 걷기 애니메이션 설정
        animator.SetFloat("Vert", 0.5f);
    }

    void Update()
    {
        // === 몬스터 상태 확인 및 예외 처리 (Dead, Game Over) ===
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            return;
        }
        // MainSceneManager가 싱글톤일 경우, 인스턴스가 존재하는지 확인해야 합니다.
        if (MainSceneManager.Instance != null && MainSceneManager.Instance.isGameOver)
        {
            // 게임 오버 시 모든 행동 중지
            monsterPatrol.StopPatrol();
            animator.SetFloat("Vert", 0f);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // [수정] 거리 기반 로직 제거
                HandlePatrolState();
                break;

            case MonsterBase.MonsterState.Flee:
                // Flee 상태에서는 거리 정보를 사용하여 도주를 실행합니다.
                HandleFleeState(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                // Idle 상태에서는 순찰도, 도망도 하지 않고 정지합니다.
                monsterPatrol.StopPatrol();
                animator.SetFloat("Vert", 0f);
                break;
        }
    }

    // ---------------------------------------------------------------------
    // === 상태별 전용 핸들러 메서드 ===
    // ---------------------------------------------------------------------

    /// <summary>
    /// 순찰 상태 로직을 처리합니다. 플레이어 감지 시 Flee 상태로 전환합니다.
    /// </summary>
    private void HandlePatrolState()
    {
        // 순찰 상태일 때만 순찰을 시작합니다.
        monsterPatrol.StartPatrol();
        animator.SetFloat("Vert", 0.5f); // 걷는 모션

        // Monster가 플레이어(detectableTarget)를 감지했다면 도망 상태로 전환
        if (monster.detectableTarget != null)
        {
            // [효과음 추가 로직] 감지 순간 딱 한번만 재생됩니다.
            if (audioSource != null && startledClip != null)
            {
                // PlayOneShot을 사용하여 현재 재생 중인 클립에 영향을 주지 않고 한 번만 재생합니다.
                audioSource.PlayOneShot(startledClip);
            }
            monster.ChangeState(MonsterBase.MonsterState.Flee);
            monsterPatrol.StopPatrol(); // 순찰 중지
        }
    }

    /// <summary>
    /// 도망 상태 로직을 처리합니다.
    /// </summary>
    private void HandleFleeState(float distanceToPlayer)
    {
        // 1. 도주 해제 조건 확인
        // 플레이어를 놓치고(detectableTarget == null) 충분한 거리를 확보했으면 Patrol로 복귀
        if (monster.detectableTarget == null && distanceToPlayer > stopFleeDistance)
        {
            // 충분히 멀리 도망쳤으면 다시 Patrol 상태로 돌아갑니다.
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
            animator.SetFloat("Vert", 0.5f); // 걷는 모션으로 복귀
            return;
        }

        // 2. 도주 실행 (detectableTarget이 있거나, stopFleeDistance 내에 있으면 계속 도망)
        if (distanceToPlayer < stopFleeDistance || monster.detectableTarget != null)
        {
            monsterPatrol.StopPatrol();
            //[변경] 헬퍼 메서드 사용으로 변경
            MoveAwayFromTarget(playerTransform, basePatrolSpeed * fleeSpeedMultiplier, stopFleeDistance);
            RotateAwayFromTarget(playerTransform);

            animator.SetFloat("Vert", 1f); // 뛰는 모션
        }
        else
        {
            // 도주 목표 거리에 도달했고 플레이어를 놓쳤다면 정지
            monster.ChangeState(MonsterBase.MonsterState.Idle);
            animator.SetFloat("Vert", 0f);
        }
    }

    /// <summary>
    /// 몬스터를 지정된 속도로 목표 지점(target)에게서 멀어지도록 이동시킵니다. (도주)
    /// </summary>
    /// <param name="target">멀어질 목표 지점의 Transform.</param>
    /// <param name="speed">적용할 이동 속도.</param>
    /// <param name="stoppingDistance">도주를 멈출 거리.</param>
    private void MoveAwayFromTarget(Transform target, float speed, float stoppingDistance)
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 도주 조건이 충족되면 이동
        if (distanceToTarget < stoppingDistance || monster.detectableTarget != null)
        {
            // 도주 방향 벡터 = (몬스터 위치 - 타겟 위치) (XZ 평면만 고려)
            Vector3 direction = transform.position - target.position;
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

            // 이동 (기존 FleeFromPlayer 로직과 동일)
            transform.Translate(flatDirection.normalized * speed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// 몬스터를 목표의 반대 방향으로 부드럽게 회전시킵니다. (도주)
    /// </summary>
    /// <param name="target">반대 방향을 계산할 목표 지점의 Transform.</param>
    private void RotateAwayFromTarget(Transform target)
    {
        // 도망 방향 = 타겟 반대 방향
        Vector3 direction = transform.position - target.position;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            // 기존 FleeFromPlayer의 Time.deltaTime * 5f를 rotationSpeed로 대체하여 통일
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}