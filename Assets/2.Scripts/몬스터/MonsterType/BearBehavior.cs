using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 곰 몬스터의 특수 행동 로직을 관리하는 스크립트입니다.
/// 일반 공격과 특수 공격을 담당하며, 몬스터의 기본 상태 로직(감지, 추적)과는 분리되어 있습니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
[RequireComponent(typeof(Rigidbody))]
public class BearBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private Rigidbody rb;

    // === 일반 공격 설정 변수 ===
    [Header("일반 공격 설정")]
    [Tooltip("일반 공격을 시작할 최소 거리입니다.")]
    [SerializeField] private float attackRange = 2f;
    [Tooltip("일반 공격의 쿨타임입니다.")]
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime; // 마지막 일반 공격이 실행된 시간

    // === 특수 공격 설정 변수 ===
    [Header("특수 공격 설정")]
    [Tooltip("특수 공격의 범위(반지름)입니다.")]
    [SerializeField] private float aoeAttackRadius = 5f;
    [Tooltip("특수 공격의 쿨타임입니다.")]
    [SerializeField] private float aoeAttackCooldown = 10f;
    [Tooltip("특수 공격 준비 시간입니다. (차징 애니메이션 길이에 맞추어 조절)")]
    [SerializeField] private float aoeChargeTime = 1.5f;

    // === 상태 관리 변수 ===
    private float lastAoeAttackTime; // 마지막 특수 공격이 실행된 시간
    private float currentChargeTime; // 현재 차징이 진행된 시간

    private void Awake()
    {
        // 필요한 컴포넌트들을 가져옵니다.
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        rb = GetComponent<Rigidbody>();

        // 특수 공격 쿨타임을 초기화하여 게임 시작 즉시 특수 공격이 가능하게 합니다.
        lastAoeAttackTime = -aoeAttackCooldown;
    }

    private void Update()
    {
        // 몬스터가 죽었다면 로직을 실행하지 않습니다.
        if (monster.currentState == Monster.MonsterState.Dead)
        {
            return;
        }

        // 몬스터의 현재 상태에 따라 적절한 행동을 수행합니다.
        switch (monster.currentState)
        {
            case Monster.MonsterState.Attack:
                HandleAttackState();
                break;
            case Monster.MonsterState.Charge:
                HandleChargeState();
                break;
        }
    }

    /// <summary>
    /// 공격 상태에서 일반 공격과 특수 공격 로직을 관리합니다.
    /// Monster 스크립트가 이미 플레이어 감지 및 상태 전환을 처리하므로,
    /// 이 메서드는 오직 공격 로직만 담당합니다.
    /// </summary>
    private void HandleAttackState()
    {
        // 플레이어 트랜스폼이 없을 경우, 추적 상태로 복귀합니다.
        // 이는 Monster 스크립트에서 타겟을 잃었을 때 일어납니다.
        if (monster.detectableTarget == null)
        {
            monster.ChangeState(Monster.MonsterState.Chase);
            return;
        }

        // 특수 공격 쿨타임 체크.
        if (Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            // 쿨타임이 지나면 특수 공격을 위해 Charge 상태로 전환합니다.
            monster.ChangeState(Monster.MonsterState.Charge);
            currentChargeTime = 0;
            // Charge 상태 진입 시, Rigidbody의 속도를 0으로 만들어 몬스터의 움직임을 멈춥니다.
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // 일반 공격 쿨타임 체크.
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
            lastAttackTime = Time.time;
        }
    }

    /// <summary>
    /// 특수 공격을 준비하는 차징 상태를 처리합니다.
    /// 일정 시간이 지나면 특수 공격을 실행하고 다시 Attack 상태로 돌아갑니다.
    /// </summary>
    private void HandleChargeState()
    {
        // 차징 시간을 업데이트합니다.
        currentChargeTime += Time.deltaTime;

        // 차징 시간이 설정값을 초과하면 특수 공격을 실행합니다.
        if (currentChargeTime >= aoeChargeTime)
        {
            PerformAOEAttack(); // 특수 공격 실행
            monster.ChangeState(Monster.MonsterState.Attack); // Attack 상태로 복귀
            lastAoeAttackTime = Time.time; // 마지막 특수 공격 시간 갱신
        }
    }

    /// <summary>
    /// 주변의 모든 생명체에게 마법 피해를 입히는 특수 공격 메서드입니다.
    /// </summary>
    private void PerformAOEAttack()
    {
        // 곰 주변에 있는 모든 콜라이더를 감지합니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeAttackRadius);

        // 감지된 콜라이더들을 순회하며 데미지를 입힙니다.
        foreach (var hitCollider in hitColliders)
        {
            // IDamageable 인터페이스를 가진 컴포넌트를 찾고, 자기 자신은 공격 대상에서 제외합니다.
            if (hitCollider.TryGetComponent(out IDamageable damageable) && hitCollider.gameObject != this.gameObject)
            {
                // MonsterData에서 설정된 마법 공격력을 가져옵니다.
                float magicDamage = monster.monsterData.magicAttackPower;

                // TakeDamage 메서드를 호출하여 마법 데미지를 입힙니다.
                // MonsterCombat 스크립트가 알아서 마법 방어력을 적용합니다.
                damageable.TakeDamage(magicDamage, DamageType.Magic);
            }
        }
    }

    /// <summary>
    /// 플레이어에게 근접 공격을 실행하고 데미지를 입히는 메서드입니다.
    /// 이 메서드는 Attack 상태에서만 호출됩니다.
    /// </summary>
    private void PerformMeleeAttack()
    {
        // Monster 스크립트에서 타겟을 이미 감지했는지 확인합니다.
        if (monster.detectableTarget == null) return;

        // 플레이어에게만 데미지를 입히도록 명확히 지정합니다.
        if (monster.detectableTarget.GetTransform().TryGetComponent(out IDamageable damageable))
        {
            monsterCombat.TakeDamage(monster.monsterData.attackPower, DamageType.Physical);
        }
    }

    /// <summary>
    /// 디버깅 및 시각화를 위해 기즈모를 그립니다.
    /// 에디터에서 특수 공격 범위를 빨간색 구체로 표시합니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeAttackRadius);
    }
}