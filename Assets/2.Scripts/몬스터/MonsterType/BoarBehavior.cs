using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 산양 몬스터의 고유한 행동 로직(순찰, 돌진, 공격)을 담당하는 클래스입니다.
/// MonsterBase의 상태를 관찰하며 특별한 행동을 실행합니다.
/// MonsterPatrol 컴포넌트를 제어하여 순찰 기능을 수행합니다.
/// </summary>
public class BoarBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조
    private Transform playerTransform;
    Animator animator;
    private AudioSource audioSource;

    [Header("사운드 설정")]
    [Tooltip("돌진을 시작하기 직전, 힘을 모을 때 재생되는 효과음.")]
    public AudioClip chargePrepareClip;
    [Tooltip("플레이어에게 일반 공격을 시도할 때 재생되는 효과음.")]
    public AudioClip normalAttackClip;
    // === 돌진 및 공격 설정 ===
    [Header("돌진 및 공격 설정")]
    [Tooltip("플레이어와 이 거리보다 멀리 떨어져 있을 때 돌진을 시작합니다.")]
    public float chargeDistance = 5f;
    [Tooltip("돌진 시 이동 속도입니다.")]
    public float chargeSpeed = 20f;
    [Tooltip("일반 공격을 시작할 거리입니다. 돌진 후 이 거리에 들어오면 일반 공격을 시작합니다.")]
    public float attackRange = 3f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;
    [Tooltip("돌진 한 번에 소모되는 마나 양입니다.")]
    public float manaCostPerCharge = 5f;
    [Tooltip("초당 회복되는 마나 양입니다.")]
    public float manaRegenRate = 1f;

    // [개선 추가] 돌진 준비 시간 설정
    [Header("돌진 준비 시간")]
    [Tooltip("돌진을 시작하기 전, 제자리에서 힘을 모으는 준비 시간입니다.")]
    public float chargePreparationTime = 2.5f;

    // [개선 추가] 돌진 관통 거리 설정
    [Tooltip("돌진 목표 지점(chargeDistance)을 통과하여 추가로 더 이동할 거리입니다.")]
    public float chargeOvershootDistance = 5f; // 관통 거리 변수 추가

    // === 내부 변수 ===
    private float currentMana;
    private float lastAttackTime;
    private Vector3 chargeDestination;
    private bool hasInitiatedCharge = false;
    private bool hasDealtChargeDamage = false; // 돌진 중 데미지를 입혔는지 여부

    // [개선 추가] 돌진 준비 시간 추적 변수
    private float currentChargePreparationTime = 0f;
    private bool isPreparingCharge = false; // 돌진 준비 상태를 나타내는 플래그

    void Awake()
    {
        // 필수 컴포넌트 종속성 확보 및 유효성 검사
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (monster == null || monsterCombat == null || monsterPatrol == null || animator == null)
        {
            Debug.LogError("BoarBehavior: 필수 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 플레이어 트랜스폼 찾기
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void Start()
    {
        // 마나 초기화 및 초기 상태 설정
        currentMana = monster.monsterData.maxMana;
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
    }

    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            // 데미지 입었을 때 공격 상태로 전환하는 이벤트 구독
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            // 이벤트 구독 해제 (메모리 누수 방지)
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    /// <summary>
    /// 몬스터가 데미지를 입었을 때 호출되며, 즉시 공격 상태로 전환합니다.
    /// </summary>
    private void OnMonsterDamaged(float damage)
    {
        // 데미지를 입으면 즉시 공격 상태로 전환 및 순찰 중지
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        monsterPatrol.StopPatrol();
    }

    /// <summary>
    /// 돌진 중 플레이어와 충돌하면 데미지를 입히는 콜백 함수입니다.
    /// 오직 'Charge' 상태에서만 발동하여 스킬 데미지를 처리합니다.
    /// </summary>
    private void OnCollisionEnter(Collision other)
    {
        // 돌진 상태이고, 아직 데미지를 주지 않았으며, 충돌 대상이 플레이어일 때만 발동
        if (monster.currentState == MonsterBase.MonsterState.Charge && !hasDealtChargeDamage && other.gameObject.CompareTag("Player"))
        {
            // 플레이어의 IDamageable 컴포넌트 가져오기
            if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable playerDamageable))
            {
                float chargeDamage = monster.monsterData.attackPower * 1.5f;
                // 물리 피해로 데미지 전달 (방어력 적용을 위해 DamageType.Physical 사용)
                playerDamageable.TakeDamage(chargeDamage, DamageType.Physical);

                hasDealtChargeDamage = true; // 데미지를 입혔으므로 true로 설정
            }

            // 데미지 처리 후 즉시 일반 공격 상태로 전환하여 돌진을 멈춥니다.
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    void Update()
    {
        // 사망 또는 게임 오버 상태 체크
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead || MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            return;
        }

        // 마나 회복 로직
        if (currentMana < monster.monsterData.maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 몬스터 상태별 행동 분기
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // Patrol 상태일 때 순찰 시작
                monsterPatrol.StartPatrol();

                // 플레이어를 감지하면 행동 전환
                if (distanceToPlayer < monster.detectionRange)
                {
                    monsterPatrol.StopPatrol(); // 순찰 중지
                    if (currentMana >= manaCostPerCharge)
                    {
                        // 마나가 충분하면 Charge 상태로 전환 (준비 단계 시작)
                        monster.ChangeState(MonsterBase.MonsterState.Charge);
                    }
                    else
                    {
                        // 마나가 부족하면 일반 Attack 상태로 전환
                        monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                }
                break;

            case MonsterBase.MonsterState.Charge:
                // 순찰 중지 후 돌진 로직 실행
                monsterPatrol.StopPatrol();
                HandleCharge(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                // 순찰 중지 후 공격 로직 실행
                monsterPatrol.StopPatrol();
                HandleAttack(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                // Idle 상태에서는 순찰 중지
                monsterPatrol.StopPatrol();
                break;
        }
    }

    /// <summary>
    /// 돌진 준비 및 실제 돌진 이동을 관리합니다.
    /// 일정 시간 준비 후 돌진을 시작하며, 준비 중에는 방향을 고정합니다.
    /// </summary>
    private void HandleCharge(float distanceToPlayer)
    {
        // 1. 돌진 초기화 (Charge 상태 진입 시 한 번 실행)
        if (!hasInitiatedCharge)
        {
            animator.SetTrigger("SpecialAttack"); // 돌진 준비 애니메이션 (차징)
            currentMana -= manaCostPerCharge;
            hasInitiatedCharge = true;
            hasDealtChargeDamage = false; // 새로운 돌진 시작 시 초기화
            isPreparingCharge = true; // 준비 플래그 설정
            currentChargePreparationTime = 0f; // 준비 시간 초기화

            // [핵심 로직] 돌진 방향 벡터 계산
            Vector3 chargeDirection = (playerTransform.position - transform.position).normalized;

            // [핵심 로직] 돌진 방향으로 몬스터의 시선을 즉시 고정 (돌진 방향을 확정)
            // LookAtTarget의 float.MaxValue 인자를 통해 즉시 회전하도록 합니다.
            LookAtTarget(playerTransform, float.MaxValue);

            // 최종 돌진 목표 지점 설정: 플레이어 감지 거리 + 추가 관통 거리
            float totalChargeLength = chargeDistance + chargeOvershootDistance;
            chargeDestination = transform.position + chargeDirection * totalChargeLength;
        }

        // 2. 돌진 준비 시간 처리 (몬스터를 제자리에 멈추게 함)
        if (isPreparingCharge)
        {
            currentChargePreparationTime += Time.deltaTime;

            // 준비 중에는 회전 로직을 생략하여 방향 고정 유지

            if (currentChargePreparationTime >= chargePreparationTime)
            {
                // 준비 시간이 끝나면 실제 돌진 시작
                isPreparingCharge = false;
            }
            // 준비 중에는 여기서 리턴하여 이동 로직을 건너뜁니다.
            return;
        }

        // 3. 실제 돌진 이동 로직 (준비 시간이 끝나면 실행됨)
        if (audioSource != null && chargePrepareClip != null)
        {
            audioSource.PlayOneShot(chargePrepareClip);
        }
        // 돌진 지점까지 이동
        transform.position = Vector3.MoveTowards(transform.position, chargeDestination, chargeSpeed * Time.deltaTime);

        // 돌진 중에는 회전 로직을 생략하여 일직선 경로 유지

        // 목표 지점에 도착하면 공격 상태로 전환 (관통 목표 지점)
        if (Vector3.Distance(transform.position, chargeDestination) < 0.5f)
        {
            animator.SetTrigger("End");
            hasInitiatedCharge = false;
            // 돌진 목표를 달성하면 Attack 상태로 전환하여 다음 행동을 준비합니다.
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    /// <summary>
    /// 일반 공격 및 추격 행동을 관리합니다.
    /// </summary>
    private void HandleAttack(float distanceToPlayer)
    {
        // 공격 범위 밖이면 마나에 따라 돌진 또는 추격
        if (distanceToPlayer > attackRange)
        {
            // [핵심 수정] chargeDistance (5m) 이상일 때만 돌진을 시도합니다.
            if (distanceToPlayer >= chargeDistance && currentMana >= manaCostPerCharge)
            {
                // Charge 상태로 전환되면 LookAtTarget이 즉시 회전하여 방향을 재설정함
                monster.ChangeState(MonsterBase.MonsterState.Charge);
            }
            else
            {
                // 일반 추격 이동 (attackRange와 chargeDistance 사이)
                // MoveTowardsTarget 내부에서 플레이어를 바라보며 이동
                MoveTowardsTarget(playerTransform, monster.monsterData.moveSpeed);
            }
        }
        else // 공격 범위 안일 때 (distanceToPlayer <= attackRange)
        {
            // [핵심 수정] 공격 범위 안일 때는 공격 실행 전에 플레이어를 향해 부드럽게 회전
            LookAtTarget(playerTransform, 5f);
            PerformAttack();
        }

        // 플레이어가 감지 범위를 벗어나면 Patrol 상태로 전환
        if (distanceToPlayer > monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 목표 Transform을 향해 이동하는 공통 로직
    /// </summary>
    private void MoveTowardsTarget(Transform targetTransform, float speed)
    {
        // 추격 시에는 플레이어를 향해 회전해야 함
        LookAtTarget(targetTransform, 5f);
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 목표 Transform을 향해 몬스터의 시선을 회전하는 공통 로직
    /// rotationSpeed가 float.MaxValue인 경우 즉시 회전합니다.
    /// </summary>
    private void LookAtTarget(Transform targetTransform, float rotationSpeed)
    {
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            // rotationSpeed가 float.MaxValue인 경우 즉시 회전합니다. (돌진 준비 시 사용)
            if (rotationSpeed == float.MaxValue)
            {
                transform.rotation = lookRotation;
            }
            else
            {
                // 일반 회전 (추격/공격 시 사용)
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    /// <summary>
    /// 플레이어에게 데미지를 입히는 일반 공격 로직을 실행합니다.
    /// </summary>
    private void PerformAttack()
    {
        // 공격 쿨타임 체크
        if (Time.time > lastAttackTime + attackCooldown)
        {
            // TryGetComponent를 사용하여 안전하게 컴포넌트 접근
            if (playerTransform.TryGetComponent<IDamageable>(out IDamageable playerDamageable))
            {
                animator.SetTrigger("Attack");
                if (audioSource != null && normalAttackClip != null)
                {
                    audioSource.PlayOneShot(normalAttackClip);
                }
                // 데미지 유형을 명시하여 방어력 계산이 가능하도록 함
                playerDamageable.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
                lastAttackTime = Time.time;
            }
        }
    }
}