using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Coroutine 및 이벤트 사용

/// <summary>
/// 산양 몬스터의 고유한 행동 로직(순찰, 돌진, 공격)을 담당하는 클래스입니다.
/// MonsterBase의 상태를 관찰하며 특별한 행동을 실행합니다.
/// MonsterPatrol 컴포넌트를 제어하여 순찰 기능을 수행합니다.
/// SOLID 원칙: SRP에 따라 산양의 고유 행동(돌진, 일반 공격, 추격)만 담당합니다.
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
    private Coroutine stunCoroutine;
    private Coroutine attackCoroutine; // [추가] 공격 코루틴 참조

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

    // **[핵심 추가]** 일반 공격 딜레이 설정
    [Header("일반 공격 딜레이 설정")]
    [Tooltip("공격 애니메이션이 시작된 후, 실제 데미지가 적용되기까지의 시간(선딜레이)입니다.")]
    public float attackPreDelay = 0.5f; // 예시 값
    [Tooltip("데미지 적용 후, 쿨타임까지 남은 시간(후딜레이)입니다.")]
    public float attackPostDelay = 0.5f; // 예시 값 (attackCooldown - attackPreDelay - @HitFrameTime)

    // [개선 추가] 돌진 준비 시간 설정
    [Header("돌진 준비 시간")]
    [Tooltip("돌진을 시작하기 전, 제자리에서 힘을 모으는 준비 시간입니다.")]
    public float chargePreparationTime = 2.5f;

    // [개선 추가] 돌진 관통 거리 설정
    [Tooltip("돌진 목표 지점(chargeDistance)을 통과하여 추가로 더 이동할 거리입니다.")]
    public float chargeOvershootDistance = 5f;

    // === 경직 설정 === 
    [Header("경직 설정")]
    [Tooltip("경직 효과가 지속될 최소 시간입니다. (랜덤 범위)")]
    [SerializeField] private float minStunDuration = 0.5f;
    [Tooltip("경직 효과가 지속될 최대 시간입니다. (랜덤 범위)")]
    [SerializeField] private float maxStunDuration = 1.0f;

    // === 내부 변수 ===
    private float currentMana;
    private float lastAttackTime;
    private Vector3 chargeDestination;
    private bool hasInitiatedCharge = false;
    private bool hasDealtChargeDamage = false;

    // [개선 추가] 돌진 준비 시간 추적 변수
    private float currentChargePreparationTime = 0f;
    private bool isPreparingCharge = false;

    private bool isAttacking = false; // [추가] 현재 공격 코루틴이 진행 중인지 여부

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
            // 경직 이벤트 구독
            monsterCombat.OnStunApplied += ApplyHitStun;
        }
    }

    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            // 이벤트 구독 해제 (메모리 누수 방지)
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
            // 경직 이벤트 구독 해제
            monsterCombat.OnStunApplied -= ApplyHitStun;
        }
    }

    /// <summary>
    /// 몬스터가 데미지를 입었을 때 호출되며, 즉시 공격 상태로 전환합니다.
    /// </summary>
    private void OnMonsterDamaged(float damage)
    {
        // 돌진 상태일 때는 피격으로 인한 상태 전환을 무시합니다.
        if (monster.currentState == MonsterBase.MonsterState.Charge)
        {
            return;
        }

        // **[수정]** 공격 코루틴이 실행 중일 때 피격 시 코루틴을 중지하고 상태 전환을 시도합니다.
        if (isAttacking)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            isAttacking = false;
            // 공격 중 상태 전환은 경직(Stun) 로직에서 처리됨
        }

        // 데미지를 입으면 즉시 공격 상태로 전환 및 순찰 중지 (Charge 상태가 아닐 때만 실행)
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        monsterPatrol.StopPatrol();
    }

    /// <summary>
    /// MonsterCombat.OnStunApplied 이벤트 발생 시 호출되며 경직 코루틴을 시작합니다.
    /// </summary>
    private void ApplyHitStun()
    {
        if (monster.currentState == MonsterBase.MonsterState.Dead) return;

        // **[추가]** 공격 중 경직이 들어오면 공격 코루틴을 중지합니다.
        if (isAttacking)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            isAttacking = false;
            // 몬스터의 상태는 StunRoutine에서 Stun으로 전환됨
        }

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        // Stun 상태로 진입 시, 현재 돌진 중인지 확인하여 상태 전환 여부를 결정합니다.
        // 현재 로직대로 Attack 상태에서는 경직이 작동합니다.
        if (monster.currentState != MonsterBase.MonsterState.Charge)
        {
            // 돌진 중이 아니라면 (일반 상태라면) StunRoutine을 시작합니다.
            hasInitiatedCharge = false;
            isPreparingCharge = false;
            // 이전 상태가 Attack이었다면, 경직 해제 후 Attack 상태로 복귀하여 HandleAttack 로직을 다시 타게 됩니다.
            stunCoroutine = StartCoroutine(StunRoutine());
        }
        else
        {
            // Charge 상태에서 피격 시, 경직을 완전히 무시하고 아무것도 하지 않습니다. (맷집 강화)
            return;
        }
    }

    /// <summary>
    /// 일반적인 경직 상태를 관리하고 타이머가 끝나면 이전 상태로 복귀시키는 코루틴입니다. (Charge 상태가 아닐 때만 호출됨)
    /// </summary>
    private IEnumerator StunRoutine()
    {
        // 1. 경직 직전 상태를 저장합니다.
        MonsterBase.MonsterState previousState = monster.currentState;

        // 2. 상태를 Stun으로 전환
        monster.ChangeState(MonsterBase.MonsterState.Stun);

        // 3. 순찰 이동을 즉시 중지!
        monsterPatrol.StopPatrol();

        // 4. 애니메이션/이펙트 (필요 시 추가)

        // 5. 경직 시간 대기
        float duration = UnityEngine.Random.Range(minStunDuration, maxStunDuration);
        yield return new WaitForSeconds(duration);

        // 6. 경직 해제 및 상태 복귀
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            // 이전 상태로 복귀
            monster.ChangeState(previousState);

            // 복귀 상태에 따라 애니메이션 및 행동 재개
            if (previousState == MonsterBase.MonsterState.Patrol)
            {
                // Patrol 상태로 복귀 시 순찰을 다시 시작
                monsterPatrol.StartPatrol();
            }
        }
        // 코루틴 종료
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
                // 물리 피해로 데미지 전달
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

        // Stun 상태일 때 모든 로직 건너뛰기 
        if (monster.currentState == MonsterBase.MonsterState.Stun)
        {
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
                monsterPatrol.StartPatrol();

                if (distanceToPlayer < monster.detectionRange)
                {
                    monsterPatrol.StopPatrol();
                    if (currentMana >= manaCostPerCharge)
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Charge);
                    }
                    else
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                }
                break;

            case MonsterBase.MonsterState.Charge:
                // 순찰 중지 후 돌진 로직 실행 (경직 및 피격으로 인한 상태 전환 무시)
                monsterPatrol.StopPatrol();
                HandleCharge(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                monsterPatrol.StopPatrol();
                HandleAttack(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StopPatrol();
                break;

            case MonsterBase.MonsterState.Stun:
                // Stun 상태일 때 (위에서 return 처리됨)
                break;
        }
    }

    /// <summary>
    /// 돌진 준비 및 실제 돌진 이동을 관리합니다.
    /// </summary>
    private void HandleCharge(float distanceToPlayer)
    {
        // 1. 돌진 초기화 (Charge 상태 진입 시 한 번 실행)
        if (!hasInitiatedCharge)
        {
            animator.SetTrigger("SpecialAttack");
            currentMana -= manaCostPerCharge;
            hasInitiatedCharge = true;
            hasDealtChargeDamage = false;
            isPreparingCharge = true;
            currentChargePreparationTime = 0f;

            // 돌진 초기화 시점에 사운드를 한 번만 재생
            if (audioSource != null && chargePrepareClip != null)
            {
                audioSource.PlayOneShot(chargePrepareClip);
            }

            // 돌진 방향 확정 및 즉시 회전
            Vector3 chargeDirection = (playerTransform.position - transform.position).normalized;
            LookAtTarget(playerTransform, float.MaxValue);

            float totalChargeLength = chargeDistance + chargeOvershootDistance;
            chargeDestination = transform.position + chargeDirection * totalChargeLength;
        }

        // 2. 돌진 준비 시간 처리 (몬스터를 제자리에 멈추게 함)
        if (isPreparingCharge)
        {
            currentChargePreparationTime += Time.deltaTime;

            if (currentChargePreparationTime >= chargePreparationTime)
            {
                isPreparingCharge = false;
            }
            return;
        }

        // 3. 실제 돌진 이동 로직 
        // Charge 상태는 경직/피격에 영향을 받지 않고 무조건 이동 실행
        transform.position = Vector3.MoveTowards(transform.position, chargeDestination, chargeSpeed * Time.deltaTime);

        // 목표 지점에 도착하면 공격 상태로 전환
        if (Vector3.Distance(transform.position, chargeDestination) < 0.5f)
        {
            animator.SetTrigger("End");
            hasInitiatedCharge = false;
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    /// <summary>
    /// 일반 공격 및 추격 행동을 관리합니다.
    /// </summary>
    private void HandleAttack(float distanceToPlayer)
    {
        // **[핵심 수정 1]** 공격 코루틴이 진행 중이라면, 추격/공격 로직을 모두 무시하고 제자리에 멈춥니다.
        if (isAttacking)
        {
            // 공격 애니메이션 중에는 멈춤
            return;
        }

        // **[핵심 수정 2]** 공격 범위 안일 때 (distanceToPlayer <= attackRange)
        if (distanceToPlayer <= attackRange)
        {
            // 공격 쿨타임 체크 후 공격 시작
            if (Time.time > lastAttackTime + attackCooldown)
            {
                // 공격 코루틴이 이미 실행 중이 아닐 때만 시작
                if (attackCoroutine == null)
                {
                    // 공격 시작 전 즉시 플레이어를 바라보게 합니다.
                    LookAtTarget(playerTransform, float.MaxValue);
                    attackCoroutine = StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                // 쿨타임 중: 플레이어를 향해 부드럽게 회전하면서 대기
                LookAtTarget(playerTransform, 5f);
            }
        }
        // 공격 범위 밖이면 마나에 따라 돌진 또는 추격
        else
        {
            if (distanceToPlayer >= chargeDistance && currentMana >= manaCostPerCharge)
            {
                monster.ChangeState(MonsterBase.MonsterState.Charge);
            }
            else
            {
                // 일반 추격 이동
                MoveTowardsTarget(playerTransform, monster.monsterData.moveSpeed);
            }
        }

        // 플레이어가 감지 범위를 벗어나면 Patrol 상태로 전환 (공격 코루틴 진행 중에는 이 로직을 타지 않음)
        if (distanceToPlayer > monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 일반 공격의 선딜레이, 데미지 적용, 후딜레이를 관리하는 코루틴입니다.
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        isAttacking = true; // 공격 시작 플래그 설정
        lastAttackTime = Time.time; // 공격 시작 시간 기록 (쿨타임 카운트 시작)

        // 1. 공격 애니메이션 시작
        animator.SetTrigger("Attack");

        // 2. 선딜레이 대기 (데미지 적용 전 대기 시간)
        yield return new WaitForSeconds(attackPreDelay);

        // 3. 데미지 적용 시점 (애니메이션 중간)
        float currentDistance = Vector3.Distance(transform.position, playerTransform.position);

        // **[핵심]** 데미지 적용 시점에서 플레이어가 여전히 공격 범위 내에 있는지 확인
        if (currentDistance <= attackRange)
        {
            if (playerTransform.TryGetComponent<IDamageable>(out IDamageable playerDamageable))
            {
                if (audioSource != null && normalAttackClip != null)
                {
                    audioSource.PlayOneShot(normalAttackClip); // 사운드 재생
                }
                // 데미지 입히기
                playerDamageable.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
            }
        }

        // 4. 후딜레이 대기 (쿨타임의 남은 시간 - 선딜레이)
        // 안전을 위해 전체 쿨타임에서 선딜레이 시간을 빼고 대기
        float remainingCooldown = attackCooldown - attackPreDelay;
        if (remainingCooldown > 0)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        // 5. 공격 완료 및 플래그 초기화
        isAttacking = false;
        attackCoroutine = null;

        // 공격 완료 후 HandleAttack이 다음 행동을 결정 (추격, 재공격 또는 Patrol 복귀)
    }

    /// <summary>
    /// 목표 Transform을 향해 이동하는 공통 로직
    /// </summary>
    private void MoveTowardsTarget(Transform targetTransform, float speed)
    {
        // Stun 상태이거나 공격 중일 때는 이동 로직을 실행하지 않습니다.
        if (monster.currentState == MonsterBase.MonsterState.Stun || isAttacking)
        {
            return;
        }

        LookAtTarget(targetTransform, 5f);
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 목표 Transform을 향해 몬스터의 시선을 회전하는 공통 로직
    /// </summary>
    private void LookAtTarget(Transform targetTransform, float rotationSpeed)
    {
        // Stun 상태이거나 공격 중일 때는 회전 로직을 실행하지 않습니다. (단, 즉시 회전 옵션은 제외)
        if ((monster.currentState == MonsterBase.MonsterState.Stun || isAttacking) && rotationSpeed != float.MaxValue)
        {
            return;
        }

        Vector3 direction = (targetTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            if (rotationSpeed == float.MaxValue)
            {
                transform.rotation = lookRotation;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    /// <summary>
    /// [사용 안 함] PerformAttack()은 HandleAttack 내의 AttackRoutine()으로 대체되었습니다.
    /// </summary>
    private void PerformAttack() { }
}