using UnityEngine;

/// <summary>
/// 모든 쥐 몬스터의 공통 행동을 정의하는 추상 기반 클래스입니다.
/// 이 클래스는 직접 게임 오브젝트에 부착할 수 없으며,
/// 자식 클래스(RatLeader, RatFollower)에서 상속받아 사용해야 합니다.
/// SOLID 원칙 중 단일 책임 원칙(SRP)과 개방-폐쇄 원칙(OCP)을 준수합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
public abstract class RatBehavior : MonoBehaviour
{
    // === 종속성 ===
    protected Monster monster; // public으로 변경하여 자식 클래스에서 접근 가능하게 함
    protected MonsterCombat monsterCombat;
    protected Transform playerTransform;

    // === 들쥐 떼 행동을 위한 공통 설정 ===
    [Header("공통 행동 설정")]
    [Tooltip("다른 몬스터를 감지할 수 있는 반경입니다.")]
    public float flockDetectionRadius = 10f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.0f;
    [Tooltip("일반 공격이 가능한 거리입니다.")]
    public float attackRange = 1.5f;

    protected float lastAttackTime;
    protected Vector3 currentPatrolPoint;

    protected virtual void Awake()
    {
        monster = GetComponent<Monster>();
        if (monster == null) Debug.LogError("RatBehavior: Monster 컴포넌트를 찾을 수 없습니다.");
        monsterCombat = GetComponent<MonsterCombat>();
        if (monsterCombat == null) Debug.LogError("RatBehavior: MonsterCombat 컴포넌트를 찾을 수 없습니다.");

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    protected virtual void OnEnable()
    {
        monsterCombat.OnDamageTaken += OnMonsterDamaged;
    }

    protected virtual void OnDisable()
    {
        monsterCombat.OnDamageTaken -= OnMonsterDamaged;
    }

    protected virtual void Start()
    {
        SetNewPatrolPoint();
    }

    /// <summary>
    /// Update() 대신 각 역할에 맞는 행동을 정의하는 추상 메서드입니다.
    /// 자식 클래스에서 반드시 구현해야 합니다.
    /// </summary>
    public abstract void UpdateBehavior();

    // MonoBehaviour의 Update() 메서드를 오버라이드하여 UpdateBehavior()를 호출합니다.
    private void Update()
    {
        if (monster.currentState != MonsterBase.MonsterState.Dead)
        {
            UpdateBehavior();
        }
    }

    /// <summary>
    /// 몬스터가 피해를 입었을 때 호출되는 메서드입니다.
    /// 자식 클래스에서 오버라이드하여 추가 로직을 구현할 수 있습니다.
    /// </summary>
    /// <param name="damage">받은 피해량</param>
    protected virtual void OnMonsterDamaged(float damage)
    {
        // 피격 시의 공통 로직 (예: 상태 변경)
    }

    /// <summary>
    /// 몬스터의 이동을 제어하는 공통 메서드입니다.
    /// </summary>
    protected void Move(Vector3 direction, float speed)
    {
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 플레이어에게 공격을 수행하는 공통 메서드입니다.
    /// </summary>
    protected void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(monster.monsterData.attackPower);
                lastAttackTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 몬스터의 순찰 행동을 처리하는 공통 메서드입니다.
    /// </summary>
    protected void Patrol()
    {
        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1.0f)
        {
            SetNewPatrolPoint();
        }
        Move(currentPatrolPoint - transform.position, monster.monsterData.moveSpeed);
    }

    /// <summary>
    /// 새로운 순찰 지점을 설정하는 공통 메서드입니다.
    /// </summary>
    protected void SetNewPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * monster.detectionRange;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;
        currentPatrolPoint = randomDirection;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, flockDetectionRadius);
    }
    public Monster GetMonster()
    { 
        return monster;
    }
}