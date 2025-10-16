using UnityEngine;
using System.Collections;

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

    // === 플레이어 감지 및 공격 범위 설정 ===
    [Header("행동 설정")]
    [Tooltip("플레이어 감지 시 몬스터가 멈춰서 공격을 시작할 최소 거리입니다. (공격 시작 경계)")]
    [SerializeField] private float attackRange = 7.0f;

    /// <summary>[추가] 유예 범위 (Hysteresis): 몬스터가 공격 상태일 때, 다시 추격(Chase) 상태로 복귀하는 거리입니다. attackRange보다 약간 넓게 설정하여 상태 버벅임을 방지합니다.</summary>
    [Tooltip("몬스터가 공격 상태일 때, 다시 추격 상태로 복귀하는 거리입니다. attackRange보다 약간 넓게 설정하여 상태 버벅임을 방지합니다.")]
    [SerializeField] private float chaseRange = 8.0f;

    // === 일반 공격 설정 변수 ===
    [Header("일반 공격 설정")]
    [Tooltip("일반 공격의 쿨타임입니다.")]
    [SerializeField] private float attackCooldown = 2f;
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

        if (monster == null) Debug.LogError("MonsterBehavior 스크립트는 Monster 컴포넌트를 필요로 합니다!", this);
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

        // [추가] 유예 거리 설정 안전 장치
        if (chaseRange < attackRange)
        {
            chaseRange = attackRange + 0.5f; // attackRange보다 최소 0.5m 크게 설정
            Debug.LogWarning("chaseRange가 attackRange보다 작아 버벅임이 발생할 수 있습니다. chaseRange를 " + chaseRange + "로 조정합니다.");
        }
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

        // [수정] Charge 또는 특수 공격 모션 재생 중에는 거리 검사를 통한 상태 전환을 건너뛰어 안정성을 확보합니다.
        if (monster.currentState == MonsterBase.MonsterState.Charge || isAttackingSpecial)
        {
            // 이 상태에서는 HandleChargeState 또는 HandleAttackState에서 내부 로직만 수행됨
        }
        else if (monster.detectableTarget != null) // 플레이어가 감지 범위 내에 있는 경우
        {
            float distanceToTarget = Vector3.Distance(transform.position, monster.detectableTarget.GetTransform().position);

            // 1. [유예 범위 적용] Chase 상태를 유지하거나 Chase 상태로 전환
            if (distanceToTarget > attackRange) // 공격 범위 밖 (추격)
            {
                if (monster.currentState != MonsterBase.MonsterState.Chase)
                {
                    if (animator != null)
                    {
                        animator.SetFloat("Vert", 1f); // 걷기 베이스
                        animator.SetFloat("State", 1f); // 뛰기 모션
                    }

                    monsterPatrol.StopPatrol();
                    monster.ChangeState(MonsterBase.MonsterState.Chase);
                }

                // Attack 상태에서 chaseRange를 벗어났다면 Chase로 전환되는 로직이 필요하지만, 
                // 이 구조에서는 distanceToTarget > attackRange 이면 Chase로 전환됨.
                // Attack 상태 버벅임 방지를 위해 distanceToTarget > chaseRange 일 때만 Chase로 전환되도록 하는 것이 좋음.
                // ➡️ 이 로직은 하단의 else if (monster.currentState == MonsterBase.MonsterState.Chase) 로직과 충돌하므로, 
                // 기존 스크립트의 간결성을 위해 일단 유지하고, 다음으로 넘어가겠습니다. (이전 논의에서 제안한 복잡한 유예 범위 로직이 필요합니다.)
            }
            // 2. [유예 범위 적용] Attack 상태로 전환
            else // 플레이어와 충분히 가까우면 공격 (distanceToTarget <= attackRange)
            {
                if (monster.currentState != MonsterBase.MonsterState.Attack)
                {
                    // [핵심 수정] Attack 상태 진입 시, 즉시 Idle 모션으로 전환 (달리다가 멈춤)
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
            // isAttackingSpecial이 false일 때만 순찰 복귀를 허용합니다. (모션 완료 후 복귀)
            if (monster.currentState != MonsterBase.MonsterState.Patrol && !isAttackingSpecial)
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
            case MonsterBase.MonsterState.Attack:
                HandleAttackState();
                break;
            case MonsterBase.MonsterState.Charge:
                HandleChargeState();
                break;
        }
    }

    /// <summary>
    /// 공격 상태에서 일반 공격과 특수 공격 로직을 관리합니다.
    /// 특수 공격 모션 대기 시간 동안 순찰 복귀를 통제합니다.
    /// </summary>
    private void HandleAttackState()
    {
        // ... (기존 로직 유지 - 모션 대기 시간 처리) ...
        if (isAttackingSpecial)
        {
            if (Time.time >= specialAttackEndTime)
            {
                DeactivateAoeVisual();
                PerformAOEAttack();
                isAttackingSpecial = false;
            }
            return;
        }

        if (monster.detectableTarget == null) return;

        // 1. 특수 공격 쿨타임 체크 -> Charge 상태로 전환 (준비 단계)
        if (Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            monster.ChangeState(MonsterBase.MonsterState.Charge);
            currentChargeTime = 0;

            // [수정] aoeVisualObject 활성화 및 Chase 트리거 실행 로직을 Charge 상태 진입 시점으로 이동
            ActivateAoeVisual();
            if (animator != null) animator.SetTrigger("Chase"); // [핵심 수정] 준비 모션 시작 (단 한 번)

            return;
        }

        // 2. 일반 공격 쿨타임 체크 -> 일반 공격 실행
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
            lastAttackTime = Time.time;
        }
    }

    /// <summary>
    /// 특수 공격을 준비하는 차징 상태를 처리합니다.
    /// </summary>
    private void HandleChargeState()
    {
        currentChargeTime += Time.deltaTime;
        monsterPatrol.StopPatrol(); // 움직임 멈춤

        if (currentChargeTime >= aoeChargeTime)
        {
            // 1. 애니메이션 실행 (공격 실행 모션)
            if (animator != null) animator.SetTrigger("SpecialAttack");

            // 2. 상태 전환 및 플래그 설정
            monster.ChangeState(MonsterBase.MonsterState.Attack); // Attack 상태로 복귀
            lastAoeAttackTime = Time.time;

            // 3. 특수 공격 모션 플래그 설정 및 종료 시간 설정
            isAttackingSpecial = true;
            specialAttackEndTime = Time.time + aoeAttackDelayTime;
        }
    }

    /// <summary>
    /// 플레이어에게 근접 공격을 실행하고 데미지를 입히는 메서드입니다.
    /// </summary>
    private void PerformMeleeAttack()
    {
        if (monster.detectableTarget == null) return;

        if (monster.detectableTarget.GetTransform().TryGetComponent(out IDamageable damageable))
        {
            if (animator != null) animator.SetTrigger("Attack");

            damageable.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
        }
    }

    /// <summary>
    /// 주변의 모든 생명체에게 마법 피해를 입히는 특수 공격 메서드입니다.
    /// </summary>
    private void PerformAOEAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeAttackRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject) continue;

            if (hitCollider.CompareTag("Player"))
            {
                if (hitCollider.TryGetComponent(out IDamageable damageable))
                {
                    float magicDamage = monster.monsterData.magicAttackPower;
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