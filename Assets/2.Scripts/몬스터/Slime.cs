using UnityEngine;

// Monster 클래스를 상속받는 Slime 클래스입니다.
public class Slime : Monster
{
    // 슬라임의 공격 쿨타임을 위한 변수
    [Header("슬라임 공격 설정")]
    public float attackCooldown = 2f; // 공격 쿨타임 (2초)
    public float attackRange = 1.5f; // 공격 범위

    private float lastAttackTime;

    void Awake()
    {
        lastAttackTime = -attackCooldown; // 게임 시작 시 바로 공격할 수 있도록 초기화
    }

    // 부모 클래스의 Update()를 오버라이드하여 새로운 동작을 추가합니다.
    protected override void Update()
    {
        // 부모 클래스인 Monster의 Update()를 먼저 호출하여 플레이어 감지 로직을 실행합니다.
        base.Update();

        // 플레이어가 감지되었는지 확인합니다.
        // isPlayerDetected와 detectableTarget 변수는 Monster 스크립트에서 관리됩니다.
        if (isPlayerDetected && detectableTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, detectableTarget.GetTransform().position);

            // 플레이어와의 거리가 공격 범위보다 멀면 플레이어를 추적
            if (distanceToTarget > attackRange)
            {
                // 플레이어를 향해 이동
                Vector3 direction = (detectableTarget.GetTransform().position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            // 플레이어가 공격 범위 안에 있고 쿨타임이 지났다면 공격
            else if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(); // 공격 메서드 호출
            }
        }
    }

    // Attack() 메서드를 재정의(Override)하여 슬라임만의 공격 방식을 구현합니다.
    public override void Attack()
    {
        // 부모 클래스 Monster의 Attack() 메서드를 먼저 호출합니다.
        // 이 메서드 내에서 IDamageable 인터페이스를 통해 데미지 로직을 처리합니다.
        base.Attack();

        // 공격 쿨타임 업데이트
        lastAttackTime = Time.time;

        Debug.Log("슬라임이 끈적한 공격을 시도합니다!");
    }
}