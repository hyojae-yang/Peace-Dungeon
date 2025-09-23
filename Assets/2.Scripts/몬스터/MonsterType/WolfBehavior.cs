using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System; // 이벤트 사용을 위해 System 네임스페이스 추가

/// <summary>
/// 늑대 몬스터의 고유한 행동 로직을 담당하는 클래스입니다.
/// 플레이어를 공격하다 체력이 절반 이하로 떨어지면 주변 동료를 소집해 무리를 형성하고 함께 공격합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))]
[RequireComponent(typeof(MonsterCombat))] // MonsterCombat 컴포넌트가 필요함을 명시
public class WolfBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterPatrol monsterPatrol;
    private MonsterCombat monsterCombat;
    private Transform playerTransform;

    // === 행동 설정 ===
    [Header("늑대 행동 설정")]
    [Tooltip("체력이 이 비율 이하로 떨어지면 무리를 소집합니다.")]
    [Range(0.1f, 0.9f)]
    public float callForHelpHealthRatio = 0.5f;
    [Tooltip("동료를 찾기 위해 주변을 탐색할 반경입니다.")]
    public float flockDetectionRadius = 15f;
    [Tooltip("플레이어에게 공격을 시작하는 최소 거리입니다.")]
    public float attackRange = 2f;
    [Tooltip("무리 공격 시 몬스터의 이동 속도입니다.")]
    public float packAttackSpeed = 5f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;

    // === 내부 상태 변수 ===
    private bool hasCalledForHelp = false;
    private bool isLeader = false;
    private WolfBehavior leader;
    private List<WolfBehavior> followers = new List<WolfBehavior>();
    private float lastAttackTime;

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterPatrol = GetComponent<MonsterPatrol>();
        monsterCombat = GetComponent<MonsterCombat>();
        if (monster == null || monsterPatrol == null || monsterCombat == null)
        {
            Debug.LogError("WolfBehavior: 필수 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void OnEnable()
    {
        // 몬스터가 데미지를 입을 때 OnMonsterDamaged 메서드를 호출하도록 구독
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    void OnDisable()
    {
        // 구독 해제
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    /// <summary>
    /// 몬스터가 데미지를 입었을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="damage">입은 피해량</param>
    private void OnMonsterDamaged(float damage)
    {
        // 체력이 절반 이하로 떨어지면 동료를 소집
        if (!hasCalledForHelp && monsterCombat.GetCurrentHealth() <= monster.monsterData.maxHealth * callForHelpHealthRatio)
        {
            CallForHelp();
        }
    }

    void Start()
    {
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Chase:
                HandleChase(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Flocking:
                HandleFlocking(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Attack:
                HandleAttack(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Idle:
                monsterPatrol.StopPatrol();
                break;
        }
    }

    /// <summary>
    /// 순찰 상태 로직을 처리합니다.
    /// 플레이어를 감지하면 추적 상태로 전환합니다.
    /// </summary>
    private void HandlePatrol(float distanceToPlayer)
    {
        monsterPatrol.StartPatrol();
        if (distanceToPlayer < monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Chase);
            monsterPatrol.StopPatrol();
        }
    }

    /// <summary>
    /// 추적 상태 로직을 처리합니다.
    /// 체력 조건에 따라 무리 소집을 시도하거나, 공격 범위에 들어오면 공격 상태로 전환합니다.
    /// </summary>
    private void HandleChase(float distanceToPlayer)
    {
        // 1. 플레이어가 공격 범위에 들어오면 공격 상태로 전환
        if (distanceToPlayer <= attackRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Attack);
            return;
        }

        // 2. 플레이어가 감지 범위를 벗어나면 순찰 상태로 돌아감
        if (distanceToPlayer > monster.detectionRange + 2f)
        {
            ExitPack();
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 일반 공격 상태 로직을 처리합니다.
    /// </summary>
    private void HandleAttack(float distanceToPlayer)
    {
        // 플레이어가 공격 범위를 벗어나면 다시 추적 상태로 전환
        if (distanceToPlayer > attackRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Chase);
        }
        else
        {
            // 직접 공격 로직을 실행
            PerformAttack();
        }
    }

    /// <summary>
    /// 무리 행동 상태 로직을 처리합니다. (Flocking 상태)
    /// 리더와 추종자 간의 움직임을 분리하여 관리하며, 공격 범위에 들어오면 공격 상태로 전환합니다.
    /// </summary>
    private void HandleFlocking(float distanceToPlayer)
    {
        if (isLeader)
        {
            // 리더는 플레이어를 직접 추격
            MoveTowardsTarget(playerTransform, packAttackSpeed);

            // 모든 추종자들이 플레이어를 공격하도록 명령
            foreach (var follower in followers)
            {
                if (follower != null)
                {
                    // 추종자도 공격 범위에 들어오면 공격 상태로 전환
                    if (Vector3.Distance(follower.transform.position, playerTransform.position) <= follower.attackRange)
                    {
                        follower.monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                    else
                    {
                        follower.MoveTowardsTarget(playerTransform, packAttackSpeed);
                    }
                }
            }
        }
        else if (leader != null)
        {
            // 추종자는 리더를 따라 이동
            MoveTowardsTarget(leader.transform, packAttackSpeed);

            // 추종자도 공격 범위에 들어오면 공격 상태로 전환
            if (distanceToPlayer <= attackRange)
            {
                monster.ChangeState(MonsterBase.MonsterState.Attack);
            }
        }

        // 플레이어가 일정 범위를 벗어나면 무리 해제 및 추적 중단
        if (distanceToPlayer > monster.detectionRange + 5f)
        {
            ExitPack();
        }
    }

    /// <summary>
    /// 동료 늑대들을 탐색하여 무리를 소집합니다.
    /// 이 메서드가 호출된 늑대가 무리의 리더가 됩니다.
    /// </summary>
    private void CallForHelp()
    {

        hasCalledForHelp = true;
        isLeader = true;
        monster.ChangeState(MonsterBase.MonsterState.Flocking);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, flockDetectionRadius);


        foreach (var hitCollider in hitColliders)
        {
            WolfBehavior otherWolf = hitCollider.GetComponent<WolfBehavior>();

            // 디버그용 로그: WolfBehavior 컴포넌트를 찾았는지 확인
            if (otherWolf != null && otherWolf != this)
            {
                if (!otherWolf.IsPartOfPack())
                {
                    otherWolf.JoinPack(this);
                    AddFollower(otherWolf);
                }
            }
        }
    }

    /// <summary>
    /// 다른 늑대가 이 늑대를 무리에 합류시키는 데 사용합니다.
    /// </summary>
    /// <param name="newLeader">무리의 리더 늑대</param>
    public void JoinPack(WolfBehavior newLeader)
    {
        if (isLeader) return; // 이미 리더라면 무시

        leader = newLeader;
        isLeader = false;
        monster.ChangeState(MonsterBase.MonsterState.Flocking); // Flocking 상태로 전환
    }

    /// <summary>
    /// 무리에서 이탈하여 다시 순찰 상태로 돌아갑니다.
    /// 리더는 모든 추종자들을 해산시킵니다.
    /// </summary>
    private void ExitPack()
    {
        hasCalledForHelp = false;
        isLeader = false;
        leader = null;
        followers.Clear();
        monster.ChangeState(MonsterBase.MonsterState.Patrol);

        // 추종자들에게 무리 해산을 알림
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                follower.ExitPack();
            }
        }
    }

    /// <summary>
    /// 무리에 속해 있는지 여부를 반환합니다.
    /// </summary>
    public bool IsPartOfPack()
    {
        return leader != null || isLeader;
    }

    /// <summary>
    /// 추종자를 무리 목록에 추가합니다.
    /// </summary>
    /// <param name="wolf">추가할 추종자 늑대</param>
    private void AddFollower(WolfBehavior wolf)
    {
        if (!followers.Contains(wolf))
        {
            followers.Add(wolf);
        }
    }

    /// <summary>
    /// 목표 지점을 향해 이동하는 공통 로직.
    /// </summary>
    public void MoveTowardsTarget(Transform targetTransform, float speed)
    {
        if (targetTransform == null) return;

        Vector3 direction = (targetTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 플레이어에게 데미지를 입히는 일반 공격 로직을 실행합니다.
    /// </summary>
    private void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(monster.monsterData.attackPower);
                lastAttackTime = Time.time;
                Debug.Log($"늑대가 플레이어에게 {monster.monsterData.attackPower}의 데미지를 입혔습니다!");
            }
        }
    }
}