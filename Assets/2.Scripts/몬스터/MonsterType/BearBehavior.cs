using UnityEngine;
using System.Collections;

/// <summary>
/// 곰 몬스터의 특화된 행동 로직을 관리하는 스크립트입니다.
/// 플레이어 감지, 추적, 공격(근접/특수), 순찰 복귀 로직을 담당합니다.
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

    // === 범위 시각화 설정 변수 ===
    [Header("시각 효과")]
    [Tooltip("특수 공격 범위를 보여줄 시각 효과 프리팹입니다.")]
    [SerializeField] private GameObject aoeVisualPrefab;
    private GameObject currentAoeVisual; // 생성된 시각 효과 인스턴스

    // === 내부 상태 관리 변수 ===
    private float lastAoeAttackTime;
    private float currentChargeTime;
    private bool isCharging = false;

    /// <summary>
    /// 컴포넌트 초기화 및 종속성 확보를 담당합니다.
    /// </summary>
    private void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        if (monster == null) Debug.LogError("MonsterBehavior 스크립트는 Monster 컴포넌트를 필요로 합니다!", this);
        if (monsterCombat == null) Debug.LogError("MonsterBehavior 스크립트는 MonsterCombat 컴포넌트를 필요로 합니다!", this);
        if (monsterPatrol == null) Debug.LogError("MonsterBehavior 스크립트는 MonsterPatrol 컴포넌트를 필요로 합니다!", this);

        lastAttackTime = -attackCooldown;
        lastAoeAttackTime = -aoeAttackCooldown;
    }

    /// <summary>
    /// 매 프레임 업데이트 로직을 처리합니다.
    /// 플레이어의 존재 여부와 거리에 따라 몬스터의 상태를 전환하고 행동을 수행합니다.
    /// </summary>
    private void Update()
    {
        if (monster.currentState == MonsterBase.MonsterState.Dead)
        {
            monsterPatrol.StopPatrol();
            return;
        }

        // --- 플레이어 감지 및 상태 전환 로직 ---
        // isCharging 상태일 때는 플레이어 위치와 관계없이 상태 전환 로직을 실행하지 않습니다.
        if (isCharging)
        {
            // HandleChargeState()가 Charge 상태 로직을 전담하게 합니다.
        }
        else if (monster.detectableTarget != null) // 플레이어가 감지 범위 내에 있는 경우
        {
            float distanceToTarget = Vector3.Distance(transform.position, monster.detectableTarget.GetTransform().position);

            if (distanceToTarget > attackRange) // 플레이어와 멀리 떨어져 있으면 추적
            {
                if (monster.currentState != MonsterBase.MonsterState.Chase)
                {
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
        else // 플레이어를 놓쳤거나 감지 범위 내에 없는 경우
        {
            if (monster.currentState != MonsterBase.MonsterState.Patrol)
            {
                monster.ChangeState(MonsterBase.MonsterState.Patrol);
                monsterPatrol.StartPatrol();
            }
        }

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
    /// </summary>
    private void HandleAttackState()
    {
        if (monster.detectableTarget == null) return;

        if (Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            monster.ChangeState(MonsterBase.MonsterState.Charge);
            currentChargeTime = 0;
            isCharging = true;
            // Charge 상태로 진입할 때 시각 효과 생성
            SpawnAoeVisual();
            return;
        }

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
            PerformAOEAttack();
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            lastAoeAttackTime = Time.time;
            isCharging = false;
            DestroyAoeVisual(); // 특수 공격이 완료되었을 때만 시각 효과를 제거합니다.
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
                Debug.Log($"곰이 {hitCollider.name}에게 {magicDamage}의 마법 범위 피해를 입혔습니다!");
            }
        }
    }

    /// <summary>
    /// 특수 공격 범위를 시각적으로 보여주는 효과를 생성합니다.
    /// </summary>
    private void SpawnAoeVisual()
    {
        if (aoeVisualPrefab != null && currentAoeVisual == null)
        {
            // 몬스터 위치에 시각 효과를 생성하고 부모를 곰 몬스터로 설정합니다.
            currentAoeVisual = Instantiate(aoeVisualPrefab, transform);

            // 생성된 프리팹의 로컬 포지션을 직접 설정합니다.
            // x와 z는 0으로 두고, y 포지션만 원하는 값으로 변경하세요.
            // 곰의 자식으로 들어가므로 곰의 피벗(pivot)을 기준으로 위치가 결정됩니다.
            currentAoeVisual.transform.localPosition = new Vector3(0, -0.5f, 0); // 원하는 Y값으로 변경하세요.

            // 부모(곰)의 스케일을 고려하여 자식 프리팹의 스케일을 조정합니다.
            float parentScaleX = transform.localScale.x;
            float parentScaleZ = transform.localScale.z;

            // 원통의 지름을 aoeAttackRadius * 2로 설정하고, 곰의 스케일로 나눠줍니다.
            float finalScaleX = (aoeAttackRadius * 2) / parentScaleX;
            float finalScaleZ = (aoeAttackRadius * 2) / parentScaleZ;

            // Y축 스케일은 원통을 납작하게 만들어 바닥에 붙도록 고정합니다.
            currentAoeVisual.transform.localScale = new Vector3(finalScaleX, 0.01f, finalScaleZ);
        }
    }

    /// <summary>
    /// 특수 공격 시각 효과를 제거합니다.
    /// </summary>
    private void DestroyAoeVisual()
    {
        if (currentAoeVisual != null)
        {
            Destroy(currentAoeVisual);
            currentAoeVisual = null;
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
