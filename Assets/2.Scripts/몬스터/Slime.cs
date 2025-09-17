using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

// Monster 클래스를 상속받는 Slime 클래스입니다.
public class Slime : Monster
{
    // 슬라임의 공격 쿨타임을 위한 변수
    [Header("슬라임 공격 설정")]
    public float attackCooldown = 2f; // 공격 쿨타임 (2초)
    public float attackRange = 1.5f; // 공격 범위

    private float lastAttackTime;

    // 'protected override' 키워드를 추가하여 부모 클래스의 Awake 메서드를 재정의합니다.
    protected override void Awake()
    {
        // 부모 클래스의 Awake를 호출합니다.
        base.Awake();
        lastAttackTime = -attackCooldown; // 게임 시작 시 바로 공격할 수 있도록 초기화
    }

    // 부모 클래스의 Update()를 오버라이드하여 새로운 동작을 추가합니다.
    protected override void Update()
    {
        // 부모 클래스인 Monster의 Update()를 먼저 호출하여 플레이어 감지 및 이동 로직을 실행합니다.
        base.Update();

        // 플레이어가 감지되었는지 확인합니다.
        if (isPlayerDetected && detectableTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, detectableTarget.GetTransform().position);

            // 플레이어가 공격 범위 안에 있고 쿨타임이 지났다면 공격을 시도합니다.
            if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(); // 공격 메서드 호출
            }
        }
    }

    // Attack() 메서드를 재정의(Override)하여 슬라임만의 공격 방식을 구현합니다.
    public override void Attack()
    {
        // 부모 클래스 Monster의 Attack() 메서드를 먼저 호출합니다.
        base.Attack();

        // 공격 쿨타임 업데이트
        lastAttackTime = Time.time;

        Debug.Log("슬라임이 끈적한 공격을 시도합니다!");
    }
}