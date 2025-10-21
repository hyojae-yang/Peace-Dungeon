using UnityEngine;

/// <summary>
/// 모든 쥐 몬스터의 공통 행동을 정의하는 추상 기반 클래스입니다.
/// 이 클래스는 직접 게임 오브젝트에 부착할 수 없으며,
/// 자식 클래스(RatLeader, RatFollower)에서 상속받아 사용해야 합니다.
/// SOLID 원칙 중 단일 책임 원칙(SRP)과 개방-폐쇄 원칙(OCP)을 준수합니다.
/// </summary>
public abstract class RatBehavior : MonoBehaviour
{
    // === 종속성 ===
    protected Monster monster;
    protected MonsterCombat monsterCombat;
    protected Transform playerTransform;
    Animator animator;
    Rigidbody rb;

    // ⭐️ [수정] 이동 명령을 저장하고 FixedUpdate에서 처리하기 위한 변수
    private Vector3 _movementDirection = Vector3.zero;

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
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        // Rigidbody 누락 시 경고 및 비활성화
        if (rb == null)
        {
            Debug.LogError(gameObject.name + ": Rigidbody 컴포넌트가 없어 이동이 불가능합니다.");
            enabled = false;
        }

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
        animator.SetFloat("Vert", 1f);
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
        if (MainSceneManager.Instance.isGameOver)
        {
            // 게임 오버 시 몬스터 행동 중지
            monster.ChangeState(MonsterBase.MonsterState.Patrol);
        }
    }

    /// <summary>
    /// ⭐️ [수정] 물리 업데이트: Rigidbody의 실제 이동을 FixedUpdate에서 안전하게 처리합니다.
    /// </summary>
    private void FixedUpdate()
    {
        // Rigidbody가 없거나, 이동 명령이 없거나, 죽은 상태면 이동하지 않습니다.
        if (rb == null || _movementDirection == Vector3.zero || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            return;
        }

        // FixedUpdate에서는 고정된 시간인 Time.fixedDeltaTime을 사용합니다.
        // MonsterData.moveSpeed는 자식 클래스에서 사용 가능하도록 Monster 클래스에 정의되어 있다고 가정합니다.
        Vector3 moveVector = _movementDirection.normalized * monster.monsterData.moveSpeed * Time.fixedDeltaTime;

        // Rigidbody.MovePosition을 사용하여 물리 엔진에 안전하게 이동 의사를 전달
        rb.MovePosition(rb.position + moveVector);

        // 이동 처리가 끝났으므로 다음 Update 명령이 들어올 때까지 방향 초기화
        _movementDirection = Vector3.zero;
    }

    /// <summary>
    /// 몬스터의 이동 방향을 설정하는 공통 메서드입니다. 
    /// [수정] Update 사이클에서 호출되며, 이동 명령(_movementDirection)을 저장하고 회전만 처리합니다.
    /// </summary>
    protected void Move(Vector3 direction, float speed)
    {
        if (direction != Vector3.zero)
        {
            // ⭐️ [핵심 수정]: 이동 명령을 FixedUpdate에서 사용할 변수에 저장
            _movementDirection = direction;

            // 회전 로직 (Slerp는 Update에서 사용 가능)
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            _movementDirection = Vector3.zero;
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
    /// 플레이어에게 공격을 수행하는 공통 메서드입니다.
    /// </summary>
    protected void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                if (animator != null)
                { animator.SetTrigger("Attack"); }
                playerDamageable.TakeDamage(monster.monsterData.attackPower,DamageType.Physical);
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