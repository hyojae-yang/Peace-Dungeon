using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 곰 몬스터의 특화된 행동 로직을 관리하는 스크립트입니다.
/// 플레이어 감지, 추적, 공격(근접/특수), 순찰 복귀 로직을 담당하며,
/// 특수 공격 시에는 미리 배치된 자식 오브젝트를 활성화하여 시각 효과를 표시합니다. (성능 개선)
/// </summary>
public class BearBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol;
    private Animator animator; // 애니메이터 참조 변수
    private AudioSource audioSource;
    private Coroutine meleeAttackCoroutine; // **[추가]** 일반 공격 시퀀스 코루틴 참조

    // === 사운드 설정 추가 ===
    [Header("사운드 설정")]
    [Tooltip("플레이어에게 근접 일반 공격을 할 때 재생되는 효과음입니다.")]
    public AudioClip normalAttackClip;
    [Tooltip("특수 공격을 준비(차징)하는 동안 재생되는 소리입니다. (점점 커지는 소리 등)")]
    public AudioClip aoeChargeClip;
    [Tooltip("특수 공격(AOE)을 실제로 실행할 때 재생되는 폭발 또는 충격 소리입니다.")]
    public AudioClip aoeExecutionClip;
    // === 플레이어 감지 및 공격 범위 설정 ===
    [Header("행동 설정")]
    [Tooltip("플레이어 감지 시 몬스터가 멈춰서 공격을 시작할 최소 거리입니다. (공격 시작 경계)")]
    [SerializeField] private float attackRange = 7.0f;

    /// <summary>[추가] 유예 범위 (Hysteresis): 몬스터가 공격 상태일 때, 다시 추격(Chase) 상태로 복귀하는 거리입니다. attackRange보다 약간 넓게 설정하여 상태 버벅임을 방지합니다.</summary>
    [Tooltip("몬스터가 공격 상태일 때, 다시 추격 상태로 복귀하는 거리입니다. attackRange보다 약간 넓게 설정하여 상태 버벅임을 방지합니다.")]
    [SerializeField] private float chaseRange = 8.0f;

    // ⭐ [추가] 몬스터의 회전 속도 (Chase 상태에서 플레이어를 바라보는 속도)
    [Tooltip("Chase 상태에서 플레이어를 향해 회전하는 속도입니다.")]
    [SerializeField] private float rotationSpeed = 10.0f;

    // === 일반 공격 설정 변수 ===
    [Header("일반 공격 설정")]
    [Tooltip("일반 공격의 쿨타임입니다.")]
    [SerializeField] private float attackCooldown = 2f;
    [Tooltip("일반 공격 애니메이션 시작 후 데미지가 적용되기까지의 시간(선딜레이)입니다.")]
    [SerializeField] private float meleeAttackPreDelay = 0.5f; // **[추가]** 일반 공격 선딜레이
    private float lastAttackTime;

    // === 특수 공격 설정 변수 ===
    [Header("특수 공격 설정")]
    [Tooltip("특수 공격의 범위(반지름)입니다.")]
    [SerializeField] private float aoeAttackRadius = 5f;
    [Tooltip("특수 공격의 쿨타임입니다.")]
    [SerializeField] private float aoeAttackCooldown = 10f;
    [Tooltip("특수 공격 준비 시간입니다. (차징 애니메이션 길이에 맞추어 조절)")]
    [SerializeField] private float aoeChargeTime = 1.5f;

    [Tooltip("특수 공격 모션이 끝날 때까지 기다릴 시간입니다. 이 시간 동안 몬스터는 순찰로 복귀하지 않습니다.")]
    [SerializeField] private float aoeAttackDelayTime = 1.0f;

    // === 범위 시각화 설정 변수 (프리팹 인스턴스화 제거) ===
    [Header("시각 효과")]
    [Tooltip("특수 공격 범위를 보여줄 시각 효과를 담은 자식 오브젝트입니다. 인스턴스화 대신 활성화/비활성화됩니다.")]
    [SerializeField] private GameObject aoeVisualObject; // GameObject로 변경

    // === 내부 상태 관리 변수 ===
    private float lastAoeAttackTime;
    private float currentChargeTime;

    /// <summary>특수 공격 모션 재생 중인지 나타냅니다. 모션 완료 시간까지 순찰 복귀를 막습니다.</summary>
    private bool isAttackingSpecial = false; // 특수 공격 모션 재생 플래그
    /// <summary>특수 공격 모션이 끝나는 실제 게임 시간(Time.time)을 저장합니다.</summary>
    private float specialAttackEndTime; // 특수 공격 모션 종료 시간
    private bool isAttackingMelee = false; // **[추가]** 일반 공격 코루틴 진행 중 플래그

    /// <summary>
    /// 컴포넌트 초기화 및 종속성 확보를 담당합니다.
    /// </summary>
    private void Awake()
    {
        // 종속성 확보 (컴포넌트 누락 시 오류 보고)
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        animator = GetComponent<Animator>(); // Animator 할당
        audioSource = GetComponent<AudioSource>();

        if (monster == null) Debug.LogError("BearBehavior 스크립트는 Monster 컴포넌트를 필요로 합니다!", this);
        if (monsterCombat == null) Debug.LogError("MonsterCombat 컴포넌트를 필요로 합니다!", this);
        if (monsterPatrol == null) Debug.LogError("MonsterPatrol 컴포넌트를 필요로 합니다!", this);

        lastAttackTime = -attackCooldown;
        lastAoeAttackTime = -aoeAttackCooldown;
    }

    /// <summary>
    /// 게임 시작 시 초기 설정을 수행합니다.
    /// </summary>
    private void Start()
    {
        // 애니메이터가 null이 아닐 때만 애니메이션 설정
        if (animator != null)
        {
            animator.SetFloat("Vert", 1f); // Patrol 기본값 (걷기 베이스)
        }

        // 시각 효과 오브젝트가 할당되었다면, 시작 시 비활성화하여 안전하게 초기화합니다.
        if (aoeVisualObject != null && aoeVisualObject.activeSelf)
        {
            aoeVisualObject.SetActive(false);
        }

        //유예 거리 설정 안전 장치
        if (chaseRange < attackRange)
        {
            chaseRange = attackRange + 0.5f; // attackRange보다 최소 0.5m 크게 설정
            Debug.LogWarning("chaseRange가 attackRange보다 작아 버벅임이 발생할 수 있습니다. chaseRange를 " + chaseRange + "로 조정합니다.");
        }
    }

    /// <summary>
    /// 스크립트 비활성화 시 모든 코루틴을 중지합니다.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        isAttackingMelee = false;
    }


    /// <summary>
    /// 매 프레임 업데이트 로직을 처리합니다. 상태 전환 로직에 유예 범위를 적용하고 Charge 상태를 잠급니다.
    /// </summary>
    private void Update()
    {
        // 사망 및 게임 오버 상태 체크
        if (monster.currentState == MonsterBase.MonsterState.Dead || MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            return;
        }

        // **[수정]** Charge 또는 특수/일반 공격 모션 재생 중에는 거리 검사를 통한 상태 전환을 건너뛰어 안정성을 확보합니다.
        if (monster.currentState == MonsterBase.MonsterState.Charge || isAttackingSpecial || isAttackingMelee)
        {
            // 이 상태에서는 HandleChargeState, HandleAttackState (내부 로직), 또는 MeleeAttackSequenceCoroutine에서 로직 수행
        }
        else if (monster.detectableTarget != null) // 플레이어가 감지 범위 내에 있는 경우
        {
            float distanceToTarget = Vector3.Distance(transform.position, monster.detectableTarget.GetTransform().position);

            // 1. [유예 범위 적용] Attack 상태에서 벗어날 때 (Chase로 전환)
            if (monster.currentState == MonsterBase.MonsterState.Attack && distanceToTarget > chaseRange)
            {
                // Attack -> Chase 전환: 애니메이션 및 상태 변경
                if (animator != null)
                {
                    animator.SetFloat("Vert", 1f); // 걷기 베이스
                    animator.SetFloat("State", 1f); // 뛰기 모션
                }
                monsterPatrol.StopPatrol(); // 추격 시작 전 순찰 에이전트 정지
                monster.ChangeState(MonsterBase.MonsterState.Chase);
            }
            // 2. [유예 범위 적용] Chase 상태로 전환/유지 (Attack Range 밖)
            else if (monster.currentState != MonsterBase.MonsterState.Attack && distanceToTarget > attackRange)
            {
                if (monster.currentState != MonsterBase.MonsterState.Chase)
                {
                    // Patrol/Idle -> Chase 전환: 애니메이션 및 상태 변경
                    if (animator != null)
                    {
                        animator.SetFloat("Vert", 1f); // 걷기 베이스
                        animator.SetFloat("State", 1f); // 뛰기 모션
                    }
                    monsterPatrol.StopPatrol(); // 순찰 정지
                    monster.ChangeState(MonsterBase.MonsterState.Chase);
                }
                // Chase 상태일 경우, HandleChaseState에서 이동 로직 수행
            }
            // 3. Attack 상태로 전환 (Attack Range 안)
            else if (distanceToTarget <= attackRange)
            {
                if (monster.currentState != MonsterBase.MonsterState.Attack)
                {
                    // Attack 상태 진입 시, 즉시 Idle 모션으로 전환 (달리다가 멈춤)
                    if (animator != null)
                    {
                        animator.SetFloat("Vert", 0f);
                        animator.SetFloat("State", 0f);
                    }
                    monster.ChangeState(MonsterBase.MonsterState.Attack);
                }
            }
        }
        else // 플레이어를 놓쳤거나 감지 범위 내에 없는 경우 (detectableTarget == null)
        {
            // isAttackingSpecial 또는 isAttackingMelee가 false일 때만 순찰 복귀를 허용합니다. (모션 완료 후 복귀)
            if (monster.currentState != MonsterBase.MonsterState.Patrol && !isAttackingSpecial && !isAttackingMelee)
            {
                DeactivateAoeVisual(); // 특수 공격 시각 효과 비활성화 (순찰 복귀 전 안전 장치)

                if (animator != null)
                {
                    animator.SetFloat("Vert", 1f); // 걷기 모션의 Vert 값
                    animator.SetFloat("State", 0f); // 걷기 모션의 State 값
                }

                monster.ChangeState(MonsterBase.MonsterState.Patrol);
                monsterPatrol.StartPatrol();
            }
        }

        // 상태별 행동 실행
        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Chase:
                HandleChaseState();
                break;
            case MonsterBase.MonsterState.Attack:
                HandleAttackState();
                break;
            case MonsterBase.MonsterState.Charge:
                HandleChargeState();
                break;
        }
    }

    /// <summary>
    /// 플레이어를 추격하는 상태 로직을 처리합니다.
    /// 몬스터의 기본 이동 속도(moveSpeed)로 플레이어를 향해 이동합니다.
    /// (단일 책임 원칙: 이동 로직만 담당)
    /// </summary>
    private void HandleChaseState()
    {
        if (monster.detectableTarget == null) return;

        Transform targetTransform = monster.detectableTarget.GetTransform();

        // 1. 플레이어를 향해 회전
        RotateTowardsTarget(targetTransform);

        // 2. 플레이어를 향해 이동 (attackRange 직전까지)
        // 멈출 거리를 attackRange보다 약간 작게 설정하여 공격 범위에 정확히 진입하도록 합니다.
        MoveTowardsTarget(targetTransform, monster.currentMoveSpeed * 1.5f, attackRange - 0.1f);
    }

    /// <summary>
    /// 몬스터를 지정된 속도로 목표 지점(target)을 향해 이동시킵니다.
    /// (단일 책임 원칙: 이동 로직 추상화)
    /// </summary>
    /// <param name="target">이동할 목표 지점의 Transform.</param>
    /// <param name="speed">이동 속도.</param>
    /// <param name="stoppingDistance">멈출 거리.</param>
    private void MoveTowardsTarget(Transform target, float speed, float stoppingDistance)
    {
        // 1. 목표와의 거리를 계산
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 2. 멈출 거리보다 멀리 있다면 이동
        if (distanceToTarget > stoppingDistance)
        {
            // 목표 방향 벡터 (XZ 평면만 고려하여 y축 제외)
            Vector3 direction = target.position - transform.position;
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

            // 이동 (Rigidbody를 사용하지 않는 직접적인 Transform 조작)
            transform.position += flatDirection.normalized * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 몬스터를 지정된 회전 속도로 목표를 향해 부드럽게 회전시킵니다.
    /// (단일 책임 원칙: 회전 로직 추상화)
    /// </summary>
    /// <param name="target">바라볼 목표 지점의 Transform.</param>
    private void RotateTowardsTarget(Transform target)
    {
        // 목표 방향 벡터 (XZ 평면만 고려)
        Vector3 direction = target.position - transform.position;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        if (flatDirection != Vector3.zero)
        {
            // 목표 회전값 계산
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);

            // 현재 회전값에서 목표 회전값까지 부드럽게 회전 (Slerp 사용)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 공격 상태에서 일반 공격과 특수 공격 로직을 관리합니다.
    /// 특수 공격 모션 대기 시간 동안 순찰 복귀를 통제합니다.
    /// </summary>
    private void HandleAttackState()
    {
        if (monster.detectableTarget == null) return;

        // 1. 특수 공격 모션 대기 중인 경우 (우선 처리)
        if (isAttackingSpecial)
        {
            // 특수 공격 모션 종료 시간 도달 시 실제 데미지 적용
            if (Time.time >= specialAttackEndTime)
            {
                DeactivateAoeVisual();
                PerformAOEAttack();
                isAttackingSpecial = false; // 플래그 해제
            }
            return; // 모션 중에는 다른 공격 시도나 상태 전환 금지
        }

        // 2. 일반 공격 모션 대기 중인 경우
        if (isAttackingMelee)
        {
            return; // 일반 공격 코루틴이 완료될 때까지 대기
        }

        // 3. 특수 공격 쿨타임 체크 -> Charge 상태로 전환 (준비 단계)
        if (Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            monster.ChangeState(MonsterBase.MonsterState.Charge);
            currentChargeTime = 0;

            ActivateAoeVisual();
            if (animator != null) animator.SetTrigger("Chase"); // [수정 없음] 준비 모션 시작 (단 한 번)

            return;
        }

        // 4. 일반 공격 쿨타임 체크 -> 일반 공격 실행 (PerformMeleeAttack에서 코루틴 시작)
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
            // lastAttackTime 갱신은 코루틴 완료 시점에 이루어집니다.
        }
    }

    /// <summary>
    /// 특수 공격을 준비하는 차징 상태를 처리합니다.
    /// </summary>
    private void HandleChargeState()
    {
        if (currentChargeTime == 0 && audioSource != null && aoeChargeClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = aoeChargeClip;
            audioSource.loop = true; // 차징 시간 동안 반복 재생 (선택 사항: 단일 긴 클립이면 loop=false)
            audioSource.Play();
        }
        currentChargeTime += Time.deltaTime;
        monsterPatrol.StopPatrol(); // 움직임 멈춤 (Chase/Attack에서 이미 멈추지만 안전 장치)

        if (currentChargeTime >= aoeChargeTime)
        {
            // 1. 애니메이션 실행 (공격 실행 모션)
            if (animator != null) animator.SetTrigger("SpecialAttack");
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
            // 2. 상태 전환 및 플래그 설정
            monster.ChangeState(MonsterBase.MonsterState.Attack); // Attack 상태로 복귀
            lastAoeAttackTime = Time.time;

            // 3. 특수 공격 모션 플래그 설정 및 종료 시간 설정
            isAttackingSpecial = true;
            specialAttackEndTime = Time.time + aoeAttackDelayTime;
        }
    }

    /// <summary>
    /// 플레이어에게 근접 공격을 실행합니다. 쿨타임 및 진행 여부를 체크하여 코루틴을 시작합니다. **[수정]**
    /// </summary>
    private void PerformMeleeAttack()
    {
        if (!isAttackingMelee)
        {
            // 코루틴 시작 및 참조 저장
            meleeAttackCoroutine = StartCoroutine(MeleeAttackSequenceCoroutine());
        }
    }

    /// <summary>
    /// 일반 공격 애니메이션 재생, 선딜레이, 데미지 적용, 쿨타임 설정을 담당하는 코루틴입니다. **[추가]**
    /// </summary>
    private IEnumerator MeleeAttackSequenceCoroutine()
    {
        // 1. 공격 시작 - 플래그 설정 및 애니메이션 트리거
        isAttackingMelee = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack"); // 공격 애니메이션 재생
        }

        // 2. 선딜레이 대기 (데미지 적용 전 대기 시간)
        yield return new WaitForSeconds(meleeAttackPreDelay);

        // 3. 실제 데미지 적용 시점: 플레이어가 여전히 공격 범위 내에 있는지 확인
        if (monster.detectableTarget != null)
        {
            Transform playerTransform = monster.detectableTarget.GetTransform();
            float currentDistance = Vector3.Distance(transform.position, playerTransform.position);

            if (currentDistance <= attackRange + 0.1f) // 약간의 허용 오차 추가
            {
                if (playerTransform.TryGetComponent(out IDamageable damageable))
                {
                    // 일반 공격 사운드 재생
                    if (audioSource != null && normalAttackClip != null)
                    {
                        audioSource.PlayOneShot(normalAttackClip);
                    }
                    // 데미지 입히기
                    damageable.TakeDamage(monster.AttackPower, DamageType.Physical);
                }
            }
        }

        // 4. 공격 후딜레이 및 쿨타임 설정
        lastAttackTime = Time.time;
        // 공격 애니메이션이 끝날 때까지 남은 쿨타임 대기 (선딜레이를 제외한 잔여 시간)
        float postDelay = attackCooldown - meleeAttackPreDelay;
        if (postDelay > 0)
        {
            yield return new WaitForSeconds(postDelay);
        }

        // 5. 공격 종료
        isAttackingMelee = false; // 이동 로직 재개 허용
        meleeAttackCoroutine = null; // 코루틴 참조 해제
    }

    /// <summary>
    /// 마법 피해를 입히는 특수 공격 메서드입니다.
    /// </summary>
    private void PerformAOEAttack()
    {
        if (audioSource != null && aoeExecutionClip != null)
        {
            audioSource.PlayOneShot(aoeExecutionClip);
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeAttackRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject) continue;

            if (hitCollider.CompareTag("Player"))
            {
                if (hitCollider.TryGetComponent(out IDamageable damageable))
                {
                    float magicDamage = monster.MagicAttackPower;
                    damageable.TakeDamage(magicDamage, DamageType.Magic);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 특수 공격 범위를 시각적으로 보여주는 효과 오브젝트를 활성화하고, 준비 모션을 시작합니다.
    /// </summary>
    private void ActivateAoeVisual()
    {
        if (aoeVisualObject != null && !aoeVisualObject.activeSelf)
        {
            // 특수 공격 범위 시각화 오브젝트의 반지름을 aoeAttackRadius에 맞게 크기 조절하는 로직이 필요할 수 있습니다.
            // (예: aoeVisualObject.transform.localScale = Vector3.one * aoeAttackRadius * 2f;)
            aoeVisualObject.SetActive(true);
        }
    }

    /// <summary>
    /// 특수 공격 시각 효과 오브젝트를 비활성화합니다.
    /// </summary>
    private void DeactivateAoeVisual()
    {
        if (aoeVisualObject != null && aoeVisualObject.activeSelf)
        {
            aoeVisualObject.SetActive(false);
        }
    }

    /// <summary>
    /// 디버깅 및 시각화를 위해 기즈모를 그립니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeAttackRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // [추가] 유예 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}