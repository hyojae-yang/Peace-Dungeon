using System.Collections;
using UnityEngine;
using static MonsterBase; // MonsterBase.MonsterState에 접근하기 위해 사용됩니다.

/// <summary>
/// 몬스터의 AI 행동(감지, 추적, 상태 관리)을 담당하는 클래스입니다.
/// MonsterBase를 상속받아 공통 기능을 구현하며, 이동 로직은 개별 Behavior 스크립트에 위임합니다.
/// </summary>
public class Monster : MonsterBase, IDetectable
{
    // =======================================================
    // [핵심 추가] MonsterData 기반 읽기 전용 스탯 속성
    // =======================================================

    /// <summary>
    /// 최대 체력(Max Health) 값을 반환하는 읽기 전용 속성입니다.
    /// MonsterData에서 값을 가져오며 외부에서 설정할 수 없습니다.
    /// </summary>
    public float MaxHealth { get; private set; }

    /// <summary>
    /// 최대 마나(Max Mana) 값을 반환하는 읽기 전용 속성입니다.
    /// MonsterData에서 값을 가져오며 외부에서 설정할 수 없습니다.
    /// </summary>
    public int MaxMana { get; private set; }

    /// <summary>
    /// 공격력(Attack Power) 값을 반환하는 읽기 전용 속성입니다.
    /// MonsterData에서 값을 가져오며 외부에서 설정할 수 없습니다.
    /// </summary>
    public int AttackPower { get; private set; }

    /// <summary>
    /// 마법 공격력(Magic Attack Power) 값을 반환하는 읽기 전성 속성입니다.
    /// MonsterData에서 값을 가져오며 외부에서 설정할 수 없습니다.
    /// </summary>
    public float MagicAttackPower { get; private set; }

    /// <summary>
    /// 방어력(Defense) 값을 반환하는 읽기 전용 속성입니다.
    /// MonsterData에서 값을 가져오며 외부에서 설정할 수 없습니다.
    /// </summary>
    public int Defense { get; private set; }

    /// <summary>
    /// 마법 방어력(Magic Defense) 값을 반환하는 읽기 전용 속성입니다.
    /// MonsterData에서 값을 가져오며 외부에서 설정할 수 없습니다.
    /// </summary>
    public int MagicDefense { get; private set; }

    // === 지형 높이 보정 설정 [핵심 추가] ===
    [Header("지형 높이 보정 설정")]
    [Tooltip("지형을 식별하고 Raycast를 수행할 레이어 마스크입니다.")]
    public LayerMask groundLayer;
    [Tooltip("바닥(hit.point)으로부터 몬스터의 중심(Pivot)을 띄울 높이 값입니다. (예: 몬스터 키의 절반)")]
    public float verticalOffset = 0.5f;

    // === 플레이어 감지 관련 변수 ===
    [Header("플레이어 감지 설정")]
    [Tooltip("플레이어를 감지하는 범위(반경)입니다.")]
    public float detectionRange = 10f;
    [Tooltip("플레이어를 감지하는 부채꼴 각도입니다. (총 각도)")]
    [Range(0, 360)]
    public float detectionAngle = 120f;
    [Tooltip("플레이어 레이어 마스크입니다.")]
    public LayerMask playerLayer;

    [Header("점수 설정")]
    [Tooltip("이 몬스터를 처치했을 때 획득할 점수입니다.")]
    private int scoreValue = 0;

    // [수정] currentMoveSpeed는 이동 로직이 Behavior로 위임되면서 더 이상 Monster 내부에서 사용되지 않지만,
    // 외부 Behavior 스크립트에서 참조용으로 사용될 수 있으므로 일단 public으로 유지합니다. (혹은 private/속성으로 변경 권장)
    [HideInInspector] // Inspector에 노출되지 않도록 처리
    public float currentMoveSpeed;

    // [수정] attackRange는 MonsterCombat 또는 Behavior 스크립트에서 관리하는 것이 SRP에 맞습니다.
    // 여기서는 공통 기능이 아니므로 [HideInInspector]로 처리하거나, 해당 Behavior 스크립트로 옮기는 것을 고려해야 합니다.
    public float attackRange;

    // === 종속성 ===
    private MonsterCombat combat;
    private MonsterLoot loot;
    [HideInInspector]
    public IDetectable detectableTarget;
    Animator animator;
    [Header("사망 설정")]
    [Tooltip("사망 애니메이션이 재생되는 시간입니다. 이 시간 후 오브젝트가 파괴됩니다.")]
    public float deathAnimationDuration = 5.0f;
    Rigidbody rb;

    private void Awake()
    {
        combat = GetComponent<MonsterCombat>();
        if (combat == null) Debug.LogError("MonsterCombat 컴포넌트를 찾을 수 없습니다!");

        loot = GetComponent<MonsterLoot>();
        if (loot == null) Debug.LogError("MonsterLoot 컴포넌트를 찾을 수 없습니다!");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (monsterData != null)
        {
            // [MonsterData.score] 필드가 있다고 가정합니다. (없다면 MonsterData에 추가 필요)
            scoreValue = monsterData.score;
            currentMoveSpeed = monsterData.moveSpeed;
            InitializeAndApplyRiskCorrection(monsterData);
        }
    }

    /// <summary>
    /// MonsterData의 스탯 데이터를 Monster 클래스의 읽기 전용 속성에 할당합니다.
    /// </summary>
    /// <param name="data">할당할 MonsterData ScriptableObject입니다.</param>
    // =======================================================
    // [핵심 추가] 초기 스탯 설정 및 위험도 보정 로직
    // =======================================================

    /// <summary>
    /// MonsterData를 기반으로 스탯을 초기화하고, DungeonRiskManager의 레벨에 따라 스탯 보정을 적용합니다.
    /// </summary>
    /// <param name="data">기본 스탯을 포함하는 MonsterData ScriptableObject입니다.</param>
    private void InitializeAndApplyRiskCorrection(MonsterData data)
    {
        // 1. 던전 위험도 레벨 가져오기
        int riskLevel = 0;
        if (DungeonRiskManager.Instance != null)
        {
            // 던전 위험도 매니저로부터 현재 위험도 레벨을 가져옵니다.
            riskLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();
        }
        else
        {
            // Debug.LogWarning("DungeonRiskManager 인스턴스를 찾을 수 없습니다. 기본 레벨 0으로 설정됩니다.");
            // riskLevel이 0이면 보정은 1.0f가 됩니다.
        }

        // 2. 보정 승수 계산 (레벨당 20% 증가)
        // 보정 승수 = 1.0f + (위험도 레벨 * 0.2f)
        // 예시: Lv 10 -> 1.0 + (10 * 0.2) = 3.0f (300% 증가, 즉 3배)
        const float CORRECTION_PER_LEVEL = 0.20f;
        float correctionMultiplier = 1.0f + (riskLevel * CORRECTION_PER_LEVEL);

        // 3. 스탯에 보정 승수 적용하여 읽기 전용 속성에 할당

        // 체력 (float)
        MaxHealth = data.maxHealth * correctionMultiplier;

        // 마나 (int, 정수로 변환 시 소수점 버림)
        MaxMana = Mathf.FloorToInt(data.maxMana * correctionMultiplier);

        // 공격력 (int)
        AttackPower = Mathf.FloorToInt(data.attackPower * correctionMultiplier);

        // 마법 공격력 (float)
        MagicAttackPower = data.magicAttackPower * correctionMultiplier;

        // 방어력 (int)
        Defense = Mathf.FloorToInt(data.defense * correctionMultiplier);

        // 마법 방어력 (int)
        MagicDefense = Mathf.FloorToInt(data.magicDefense * correctionMultiplier);

        // 디버그 (선택 사항)
        // Debug.Log($"[Monster Stats] Lv: {riskLevel}, 보정치: {correctionMultiplier:F2}x, 최종 공격력: {AttackPower}");
    }

    private void Update()
    {
        DetectPlayer();
        switch (currentState)
        {
            case MonsterState.Patrol:
            case MonsterState.Chase:
            case MonsterState.Attack:
            case MonsterState.Flee:
            case MonsterState.Stun:
                // [핵심 수정] Patrol, Chase, Attack, Flee 상태의 모든 이동 로직을 제거합니다.
                // 모든 이동 처리는 DeerBehavior, BearBehavior 등의 개별 Behavior 스크립트가 전적으로 담당합니다.
                break;
            case MonsterState.Dead:
                break;
        }
    }

    /// <summary>
    /// 플레이어를 감지하는 메서드. (로직 변경 없음: 타겟만 찾습니다)
    /// </summary>
    private void DetectPlayer()
    {
        // ... (DetectPlayer 내부 로직은 변경 없이 유지합니다. 타겟을 찾거나 놓칠 뿐, 상태 전환은 Behavior 스크립트가 담당합니다.)
        // ...

        // 이미 타겟을 발견한 경우의 재확인 로직
        if (detectableTarget != null && detectableTarget.IsDetectable())
        {
            Vector3 currentDirectionToTarget = (detectableTarget.GetTransform().position - transform.position);
            float distance = currentDirectionToTarget.magnitude;

            // 1. 거리가 감지 범위 내에 있는지 확인합니다.
            if (distance <= detectionRange)
            {
                return;
            }

            // 거리를 벗어났거나 시야가 가려지면 타겟을 놓칩니다.
            detectableTarget = null;
            // ⭐️ [추가] 타겟을 놓치면 Behavior 스크립트가 상태를 Patrol로 전환하도록 유도합니다.
            // 직접 상태를 바꾸는 대신, detectableTarget = null 만으로 상태 변화를 Behavior에 위임합니다.
            return;
        }

        // 타겟이 없을 때 초기 시야각 감지를 수행합니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        foreach (Collider hit in hitColliders)
        {
            IDetectable target = hit.GetComponent<IDetectable>();
            if (target != null && target.IsDetectable())
            {
                Vector3 directionToTarget = (target.GetTransform().position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                // 몬스터가 타겟을 **처음** 감지할 때만 시야각 체크를 엄격하게 적용합니다.
                if (angle < detectionAngle * 0.5f)
                {
                    // 타겟을 발견했습니다. Behavior 스크립트가 Chase 상태로 전환하도록 유도합니다.
                    detectableTarget = target;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 외부에서 몬스터의 상태를 안전하게 변경하기 위한 메서드입니다. (로직 변경 없음)
    /// </summary>
    /// <param name="newState">변경할 몬스터의 새로운 상태</param>
    public void ChangeState(MonsterState newState)
    {
        SetState(newState);
    }

    /// <summary>
    /// 몬스터의 현재 이동 속도를 설정합니다. Behavior 스크립트가 이 메서드를 사용하여 속도를 제어합니다.
    /// </summary>
    /// <param name="newSpeed">설정할 새로운 이동 속도 값입니다.</param>
    public void SetMovementSpeed(float newSpeed)
    {
        // [추가] Behavior 스크립트에서 이동 속도를 덮어쓸 수 있도록 Setter 제공
        currentMoveSpeed = newSpeed;
    }


    // --- MonsterBase 가상 메서드 오버라이드 ---
    public override void Die()
    {
        // ... (이하 Die 로직 유지)
        ChangeState(MonsterState.Dead);
        if (DungeonScoreManager.Instance != null)
        {
            // [핵심 수정] 몬스터의 점수와 MonsterData의 고유 ID를 함께 전달하여
            // KillCountManager가 종류별 처치 횟수를 기록할 수 있도록 합니다.
            if (monsterData != null)
            {
                DungeonScoreManager.Instance.AddScore(scoreValue, monsterData.monsterID);
                // Debug.Log($"[Monster:Die] 점수 {scoreValue}와 ID {monsterData.monsterID} 보고 완료!");
            }
            else
            {
                // MonsterData가 없으면 ID 없이 점수만 보고 (이 경우 KillCountManager에 기록되지 않음)
                DungeonScoreManager.Instance.AddScore(scoreValue, -1); // 임시 ID: -1
                Debug.LogError("[Monster:Die] MonsterData가 할당되지 않아 몬스터 ID 보고에 실패했습니다. 점수만 보고합니다.");
            }
        }
        else
        {
            Debug.LogError("[Monster:Die] DungeonScoreManager 인스턴스를 찾을 수 없어 점수 보고에 실패했습니다!");
        }

        loot.GiveReward();

        if (monsterData != null)
        {
            RaiseMonsterKilledEvent(monsterData.monsterID);
        }
        else
        {
            Debug.LogError("MonsterData가 할당되지 않아 몬스터 처치 이벤트를 발생시킬 수 없습니다.");
        }

        GetComponent<Collider>().enabled = false;
        if (rb != null)
        {
            // 사망 시 isKinematic을 true로 설정하여 시체가 움직이지 않게 합니다.
            rb.isKinematic = true;
        }
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (deathAnimationDuration > 0f)
        {
            StartCoroutine(HandleDeathSequence(deathAnimationDuration));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 몬스터 사망 애니메이션 재생 시간만큼 대기한 후 오브젝트를 파괴합니다.
    /// </summary>
    private IEnumerator HandleDeathSequence(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(gameObject);
    }

    // --- IDetectable 인터페이스 구현 ---
    public bool IsDetectable()
    {
        return currentState != MonsterState.Dead;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 forwardLimit = transform.position + transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, forwardLimit);
        Vector3 leftLimit = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
        Vector3 rightLimit = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
    }
}