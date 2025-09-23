using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 몬스터의 고유한 행동 로직(순찰, 돌진, 공격)을 담당하는 클래스입니다.
/// MonsterBase의 상태를 관찰하며 특별한 행동을 실행합니다.
/// MonsterPatrol 컴포넌트를 제어하여 순찰 기능을 수행합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterPatrol))] // MonsterPatrol 컴포넌트가 필요함을 명시
public class BoarBehavior : MonoBehaviour
{
    // === 종속성 ===
    private Monster monster;
    private MonsterCombat monsterCombat;
    private MonsterPatrol monsterPatrol; // MonsterPatrol 컴포넌트 참조
    private Transform playerTransform;

    // === 돌진 및 공격 설정 ===
    [Header("돌진 및 공격 설정")]
    [Tooltip("플레이어와 이 거리보다 멀리 떨어져 있을 때 돌진을 시작합니다.")]
    public float chargeDistance = 8f;
    [Tooltip("돌진 시 이동 속도입니다.")]
    public float chargeSpeed = 15f;
    [Tooltip("일반 공격을 시작할 거리입니다. 돌진 후 이 거리에 들어오면 일반 공격을 시작합니다.")]
    public float attackRange = 3f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 1.5f;
    [Tooltip("돌진 한 번에 소모되는 마나 양입니다.")]
    public float manaCostPerCharge = 5f;
    [Tooltip("초당 회복되는 마나 양입니다.")]
    public float manaRegenRate = 1f;

    // === 내부 변수 ===
    private float currentMana;
    private float lastAttackTime;
    private Vector3 chargeDestination;
    private bool hasInitiatedCharge = false;
    private bool hasDealtChargeDamage = false; // 추가: 돌진 중 데미지를 입혔는지 여부

    void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        monsterPatrol = GetComponent<MonsterPatrol>(); // MonsterPatrol 컴포넌트 참조
        if (monster == null || monsterCombat == null || monsterPatrol == null)
        {
            Debug.LogError("BoarBehavior: 필수 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void Start()
    {
        currentMana = monster.monsterData.maxMana;
        // 멧돼지 몬스터는 시작부터 순찰 상태로 설정
        monster.ChangeState(MonsterBase.MonsterState.Patrol);
    }

    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken += OnMonsterDamaged;
        }
    }

    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            monsterCombat.OnDamageTaken -= OnMonsterDamaged;
        }
    }

    private void OnMonsterDamaged(float damage)
    {
        // 데미지를 입으면 즉시 공격 상태로 전환
        monster.ChangeState(MonsterBase.MonsterState.Attack);
        // 순찰 중이었다면 순찰 중지
        monsterPatrol.StopPatrol();
    }

    // 돌진 중 플레이어와 충돌하면 데미지를 입히는 콜백 함수
    private void OnCollisionEnter(Collision other)
    {
        // 돌진 상태이고, 아직 데미지를 주지 않았으며, 충돌 대상이 플레이어 태그를 가지고 있을 때만 발동
        if (monster.currentState == MonsterBase.MonsterState.Charge && !hasDealtChargeDamage && other.gameObject.CompareTag("Player"))
        {
            IDamageable playerDamageable = other.gameObject.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                float chargeDamage = monster.monsterData.attackPower * 1.5f;
                playerDamageable.TakeDamage(chargeDamage);
                hasDealtChargeDamage = true; // 데미지를 입혔으므로 true로 설정
                Debug.Log($"멧돼지 돌진 공격 성공! 플레이어에게 {chargeDamage} 데미지!");
            }
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    void Update()
    {
        if (playerTransform == null || monster.currentState == MonsterBase.MonsterState.Dead)
        {
            // 몬스터가 죽으면 모든 행동 중지
            monsterPatrol.StopPatrol();
            return;
        }

        // 마나 회복 로직
        if (currentMana < monster.monsterData.maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (monster.currentState)
        {
            case MonsterBase.MonsterState.Patrol:
                // Patrol 상태일 때 순찰 시작
                monsterPatrol.StartPatrol();

                // 플레이어를 감지하면 마나에 따라 공격 또는 돌진 상태로 전환
                if (distanceToPlayer < monster.detectionRange)
                {
                    monsterPatrol.StopPatrol(); // 순찰 중지
                    if (currentMana >= manaCostPerCharge)
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Charge);
                    }
                    else
                    {
                        monster.ChangeState(MonsterBase.MonsterState.Attack);
                    }
                }
                break;

            case MonsterBase.MonsterState.Charge:
                // 순찰 중지 후 돌진 로직 실행
                monsterPatrol.StopPatrol();
                HandleCharge(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Attack:
                // 순찰 중지 후 공격 로직 실행
                monsterPatrol.StopPatrol();
                HandleAttack(distanceToPlayer);
                break;

            case MonsterBase.MonsterState.Idle:
                // Idle 상태에서는 순찰 중지
                monsterPatrol.StopPatrol();
                break;
        }
    }

    private void HandleCharge(float distanceToPlayer)
    {
        if (!hasInitiatedCharge)
        {
            // 돌진 목표 지점 설정
            chargeDestination = transform.position + (playerTransform.position - transform.position).normalized * chargeDistance;
            currentMana -= manaCostPerCharge;
            hasInitiatedCharge = true;
            hasDealtChargeDamage = false; // 새로운 돌진 시작 시 초기화
        }

        // 돌진 지점까지 이동
        transform.position = Vector3.MoveTowards(transform.position, chargeDestination, chargeSpeed * Time.deltaTime);

        // 플레이어 방향으로 시선 변경
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // 목표 지점에 도착했거나 플레이어에게 근접하면 공격 상태로 전환
        if (Vector3.Distance(transform.position, chargeDestination) < 0.5f || distanceToPlayer <= attackRange)
        {
            hasInitiatedCharge = false;
            monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    private void HandleAttack(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            // 공격 범위 밖이면 마나에 따라 돌진 또는 추격
            if (currentMana >= manaCostPerCharge)
            {
                monster.ChangeState(MonsterBase.MonsterState.Charge);
            }
            else
            {
                MoveTowardsTarget(playerTransform, monster.monsterData.moveSpeed);
            }
        }
        else
        {
            // 공격 범위 안이면 공격 실행
            PerformAttack();
        }

        // 플레이어가 감지 범위를 벗어나면 Idle 상태로 전환
        if (distanceToPlayer > monster.detectionRange)
        {
            monster.ChangeState(MonsterBase.MonsterState.Patrol); // Idle 대신 Patrol 상태로 변경
        }
    }

    /// <summary>
    /// 목표 Transform을 향해 이동하고 회전하는 공통 로직
    /// </summary>
    private void MoveTowardsTarget(Transform targetTransform, float speed)
    {
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
                Debug.Log($"멧돼지가 플레이어에게 {monster.monsterData.attackPower}의 데미지를 입혔습니다!");
            }
        }
    }
}