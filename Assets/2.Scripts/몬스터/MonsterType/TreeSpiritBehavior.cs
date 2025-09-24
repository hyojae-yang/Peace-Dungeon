using UnityEngine;
using System.Collections;

/// <summary>
/// 나무 정령 몬스터의 특화된 행동 로직을 관리하는 스크립트입니다.
/// 제자리에서 대기하다가 플레이어 감지 시 '뿌리 묶기' 공격을 준비합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
public class TreeSpiritBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;

    // === 플레이어 감지 및 공격 범위 설정 ===
    [Header("행동 설정")]
    [Tooltip("플레이어 감지 시 공격을 시작할 최소 거리입니다.")]
    [SerializeField] private float detectionRange = 5f;

    // === 특수 공격 설정 변수 ===
    [Header("특수 공격 설정")]
    [Tooltip("특수 공격의 쿨타임입니다.")]
    [SerializeField] private float aoeAttackCooldown = 10f;
    [Tooltip("특수 공격 준비 시간입니다. (애니메이션 길이에 맞추어 조절)")]
    [SerializeField] private float aoeChargeTime = 1.5f;
    [Tooltip("특수 공격 효과 프리팹입니다. RootTrap 스크립트가 포함되어야 합니다.")]
    [SerializeField] private GameObject rootTrapPrefab;

    // === 내부 상태 관리 변수 ===
    private float lastAoeAttackTime;
    private bool isCharging = false;
    private Coroutine chargeRoutine; // 중복 실행을 막기 위한 코루틴 변수

    /// <summary>
    /// 컴포넌트 초기화 및 종속성 확보를 담당합니다.
    /// </summary>
    private void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        if (monster == null) Debug.LogError("TreeSpiritBehavior 스크립트는 Monster 컴포넌트를 필요로 합니다!", this);
        if (monsterCombat == null) Debug.LogError("TreeSpiritBehavior 스크립트는 MonsterCombat 컴포넌트를 필요로 합니다!", this);

        lastAoeAttackTime = -aoeAttackCooldown;
    }

    /// <summary>
    /// 매 프레임 업데이트 로직을 처리합니다.
    /// 플레이어의 존재 여부와 거리에 따라 몬스터의 상태를 전환하고 행동을 수행합니다.
    /// </summary>
    private void Update()
    {
        // 몬스터가 죽은 상태이거나 이미 공격을 준비 중이면 아무것도 하지 않습니다.
        if (monster.currentState == MonsterBase.MonsterState.Dead || isCharging)
        {
            return;
        }

        // 플레이어가 감지 범위 내에 있고, 특수 공격 쿨타임이 지났는지 확인합니다.
        if (monster.detectableTarget != null && Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            // 공격 준비 상태로 전환하고 코루틴을 시작합니다.
            isCharging = true;
            chargeRoutine = StartCoroutine(ChargeAttackRoutine());
        }
    }

    /// <summary>
    /// 특수 공격을 준비하고 실행하는 코루틴입니다.
    /// 이 루틴이 실행되는 동안 몬스터는 다른 행동을 하지 않습니다.
    /// </summary>
    private IEnumerator ChargeAttackRoutine()
    {
        // 몬스터의 상태를 Charge로 변경합니다.
        monster.ChangeState(MonsterBase.MonsterState.Charge);

        // aoeChargeTime 만큼 기다립니다. 이 시간 동안 몬스터는 차징 상태를 유지합니다.
        yield return new WaitForSeconds(aoeChargeTime);

        // 플레이어가 아직 감지 범위 내에 있을 경우에만 공격을 실행합니다.
        if (monster.detectableTarget != null)
        {
            // RootTrap 프리팹을 플레이어 위치에 생성하여 공격 효과를 발생시킵니다.
            Vector3 playerPos = monster.detectableTarget.GetTransform().position;
            Instantiate(rootTrapPrefab, playerPos, Quaternion.identity);

            Debug.Log("나무 정령이 뿌리 묶기 공격을 시작합니다!");
        }

        // 공격이 완료되었으므로 상태를 초기화합니다.
        monster.ChangeState(MonsterBase.MonsterState.Idle);
        lastAoeAttackTime = Time.time; // 쿨타임 시작
        isCharging = false;
    }

    /// <summary>
    /// 디버깅 및 시각화를 위해 기즈모를 그립니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}