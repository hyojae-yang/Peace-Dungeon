using UnityEngine;
using System.Collections;

/// <summary>
/// 곰 몬스터의 특화된 행동 로직을 관리하는 스크립트입니다.
/// 플레이어 감지, 추적, 공격(근접/특수), 순찰 복귀 로직을 담당하며,
/// 특수 공격 시에는 미리 배치된 자식 오브젝트를 활성화하여 시각 효과를 표시합니다. (성능 개선)
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
[RequireComponent(typeof(MonsterPatrol))]
public class BearBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol;
    private Animator animator; // 💡 애니메이터 참조 변수

    // === 플레이어 감지 및 공격 범위 설정 ===
    [Header("행동 설정")]
    [Tooltip("플레이어 감지 시 몬스터가 멈춰서 공격을 시작할 최소 거리입니다.")]
    [SerializeField] private float attackRange = 2.5f;

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

    // 💡 [추가] 특수 공격 애니메이션 모션이 끝날 때까지 대기할 시간입니다. (시간 기반 모션 완료 기능)
    [Tooltip("특수 공격 모션이 끝날 때까지 기다릴 시간입니다. 이 시간 동안 몬스터는 순찰로 복귀하지 않습니다.")]
    [SerializeField] private float aoeAttackDelayTime = 1.0f;

    // === 범위 시각화 설정 변수 (프리팹 인스턴스화 제거) ===
    [Header("시각 효과")]
    [Tooltip("특수 공격 범위를 보여줄 시각 효과를 담은 자식 오브젝트입니다. 인스턴스화 대신 활성화/비활성화됩니다.")]
    [SerializeField] private GameObject aoeVisualObject; // GameObject로 변경

    // === 내부 상태 관리 변수 ===
    private float lastAoeAttackTime;
    private float currentChargeTime;
    private bool isCharging = false;

    /// <summary>특수 공격 모션 재생 중인지 나타냅니다. 모션 완료 시간까지 순찰 복귀를 막습니다.</summary>
    private bool isAttackingSpecial = false; // 💡 [추가] 특수 공격 모션 재생 플래그
    /// <summary>특수 공격 모션이 끝나는 실제 게임 시간(Time.time)을 저장합니다.</summary>
    private float specialAttackEndTime; // 💡 [추가] 특수 공격 모션 종료 시간

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
            animator.SetFloat("Vert", 1f);
        }

        // 시각 효과 오브젝트가 할당되었다면, 시작 시 비활성화하여 안전하게 초기화합니다.
        if (aoeVisualObject != null && aoeVisualObject.activeSelf)
        {
            aoeVisualObject.SetActive(false);
        }
    }

    /// <summary>
    /// 매 프레임 업데이트 로직을 처리합니다.
    /// 플레이어의 존재 여부와 거리에 따라 몬스터의 상태를 전환하고 행동을 수행합니다.
    /// </summary>
    private void Update()
    {
        // 사망 및 게임 오버 상태 체크
        if (monster.currentState == MonsterBase.MonsterState.Dead || MainSceneManager.Instance.isGameOver)
        {
            monsterPatrol.StopPatrol();
            return;
        }

        // --- 플레이어 감지 및 상태 전환 로직 ---
        // 💡 [수정] Charge 또는 SpecialAttack 재생 중에는 상태 전환 로직을 건너뜁니다.
        if (isCharging || isAttackingSpecial)
        {
            // Charge/SpecialAttack 상태일 때는 상태 전환 로직을 건너뛰고 HandleChargeState() 또는 HandleAttackState()에 제어 위임
        }
        else if (monster.detectableTarget != null) // 플레이어가 감지 범위 내에 있는 경우
        {
            float distanceToTarget = Vector3.Distance(transform.position, monster.detectableTarget.GetTransform().position);

            if (distanceToTarget > attackRange) // 플레이어와 멀리 떨어져 있으면 추적
            {
                if (monster.currentState != MonsterBase.MonsterState.Chase)
                {
                    if (animator != null) animator.SetFloat("State", 1f);

                    monsterPatrol.StopPatrol();
                    monster.ChangeState(MonsterBase.MonsterState.Chase);
                }
            }
            else // 플레이어와 충분히 가까우면 공격
            {
                if (monster.currentState != MonsterBase.MonsterState.Attack && monster.currentState != MonsterBase.MonsterState.Charge)
                {
                    monster.ChangeState(MonsterBase.MonsterState.Attack);
                }
            }
        }
        else // 플레이어를 놓쳤거나 감지 범위 내에 없는 경우 (detectableTarget == null)
        {
            // 💡 [핵심 수정] isAttackingSpecial이 false일 때만 순찰 복귀를 허용합니다. (모션 완료 후 복귀)
            if (monster.currentState != MonsterBase.MonsterState.Patrol && !isAttackingSpecial)
            {
                // Patrol로 돌아갈 때 Charge 상태를 명확히 초기화 (안전 장치)
                isCharging = false;
                DeactivateAoeVisual();

                if (animator != null) animator.SetFloat("State", 0f);

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
    /// 또한, 특수 공격 모션 대기 시간을 처리하여 순찰 복귀를 통제합니다.
    /// </summary>
    private void HandleAttackState()
    {
        // [핵심 추가] 특수 공격 모션 대기 시간 처리 (순찰 복귀 통제 로직)
        if (isAttackingSpecial)
        {
            // 특수 공격 모션 재생 시간만큼 대기합니다.
            if (Time.time >= specialAttackEndTime)
            {
                DeactivateAoeVisual();
                // [추가] 모션 시간이 끝났다면 이제 데미지를 처리합니다.
                PerformAOEAttack();
                // 모션 시간이 끝났다면 플래그를 해제하여 다음 Update()에서 상태 전환(순찰 복귀 포함)이 가능하게 합니다.
                isAttackingSpecial = false;
                // 플레이어가 아직 감지된다면 Attack 상태를 유지하고, 놓쳤다면 다음 Update에서 Patrol로 전환될 것입니다.
            }
            // 모션 재생 중에는 일반 공격을 시도하지 않습니다.
            return;
        }

        if (monster.detectableTarget == null) return;

        // 1. 특수 공격 쿨타임 체크 -> Charge 상태로 전환
        if (Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            monster.ChangeState(MonsterBase.MonsterState.Charge);
            currentChargeTime = 0;
            isCharging = true;

            // if (animator != null) animator.SetTrigger("Chase"); // 필요하다면 다시 추가

            ActivateAoeVisual();
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

        if (currentChargeTime >= aoeChargeTime)
        {
            // 1. 애니메이션 실행
            if (animator != null) animator.SetTrigger("SpecialAttack");

            // 2. 상태 전환 및 플래그 설정
            monster.ChangeState(MonsterBase.MonsterState.Attack); // Attack 상태로 복귀
            lastAoeAttackTime = Time.time;
            isCharging = false;

            // [핵심 추가] 특수 공격 모션 플래그 설정 및 종료 시간 설정
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
            Debug.Log($"곰이 {monster.detectableTarget.GetTransform().name}에게 {monster.monsterData.attackPower}의 물리 피해를 입혔습니다!");
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

            if (hitCollider.TryGetComponent(out IDamageable damageable))
            {
                float magicDamage = monster.monsterData.magicAttackPower;
                damageable.TakeDamage(magicDamage, DamageType.Magic);

                // 디버그 로그 수정
                Debug.Log($"곰이 {hitCollider.name}에게 {magicDamage}의 마법 피해를 입혔습니다!");
            }
        }
    }

    /// <summary>
    /// 특수 공격 범위를 시각적으로 보여주는 효과 오브젝트를 활성화합니다.
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
    }
}