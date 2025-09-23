using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 몬스터의 순찰 행동을 전담하는 클래스입니다.
/// 코루틴을 이용한 순찰 로직과 충돌 감지 후 경로 재설정 기능을 포함합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MonsterPatrol : MonoBehaviour
{
    // === 순찰 행동 설정 ===
    [Header("순찰 행동 설정")]
    [Tooltip("순찰의 중심이 되는 지점입니다. (월드 좌표)")]
    public Vector3 homePoint;
    [Tooltip("중심 지점을 기준으로 순찰할 반경입니다.")]
    public float patrolRadius = 10f;
    [Tooltip("순찰 시 이동 속도입니다. 몬스터의 기본 이동 속도에 곱하여 사용됩니다.")]
    public float patrolSpeedMultiplier = 1f;
    [Tooltip("새로운 순찰 지점을 설정하기 전 대기 시간입니다.")]
    public float waitTimeBetweenPatrols = 1f;
    [Tooltip("순찰 지점으로 이동하기 전까지 기다리는 최대 시간입니다. 이 시간이 지나면 목표 지점까지 도착하지 않았어도 새 지점을 설정합니다.")]
    public float patrolPointChangeInterval = 5f;
    [Tooltip("순찰 중심(HomePoint)을 변경하는 주기입니다. 이 시간이 지나야 새로운 구역으로 순찰 범위를 옮깁니다.")]
    public float homePointChangeInterval = 50f;

    // === 종속성 ===
    private Transform monsterTransform;
    private Coroutine patrolCoroutine;
    private MonsterBase monsterBase;

    // === 내부 변수 ===
    private Vector3 currentPatrolPoint;
    private float homePointTimer;

    private void Awake()
    {
        monsterTransform = this.transform;
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterPatrol: MonsterBase 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 홈 포인트가 지정되지 않았으면 몬스터의 시작 위치를 사용합니다.
        if (homePoint == Vector3.zero)
        {
            homePoint = monsterTransform.position;
        }

        // 타이머를 무작위로 초기화하여 모든 몬스터가 동시에 순찰 범위를 바꾸는 것을 방지
        homePointTimer = UnityEngine.Random.Range(0, homePointChangeInterval);
    }

    /// <summary>
    /// 외부에서 순찰 행동을 시작하는 메서드입니다.
    /// 이미 순찰 중이면 중복 실행을 방지합니다.
    /// </summary>
    public void StartPatrol()
    {
        if (patrolCoroutine == null)
        {
            patrolCoroutine = StartCoroutine(PatrolCoroutine());
        }
    }

    /// <summary>
    /// 외부에서 순찰 행동을 멈추는 메서드입니다.
    /// </summary>
    public void StopPatrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
    }

    /// <summary>
    /// 순찰 로직을 실행하는 코루틴입니다.
    /// 목표 지점까지 이동하고, 도착 또는 일정 시간 경과 후 새 지점으로 이동합니다.
    /// </summary>
    private IEnumerator PatrolCoroutine()
    {
        SetNewPatrolPoint(); // 초기 순찰 지점 설정

        float patrolTimer = 0f;
        while (true)
        {
            // 목표 지점에 거의 도착했거나, 일정 시간이 지났으면 새로운 순찰 지점을 설정합니다.
            if (Vector3.Distance(monsterTransform.position, currentPatrolPoint) < 1.0f || patrolTimer >= patrolPointChangeInterval)
            {
                // 순찰 중심 변경 타이머가 만료되면 순찰 중심도 함께 변경
                if (homePointTimer >= homePointChangeInterval)
                {
                    UpdateHomePointAndPatrolPoint();
                }
                else
                {
                    SetNewPatrolPoint();
                }

                patrolTimer = 0f; // 순찰 지점 타이머 리셋
                yield return new WaitForSeconds(waitTimeBetweenPatrols);
            }

            // 목표 지점을 향해 이동
            Vector3 direction = (currentPatrolPoint - monsterTransform.position).normalized;
            if (direction != Vector3.zero)
            {
                monsterTransform.position += direction * monsterBase.monsterData.moveSpeed * patrolSpeedMultiplier * Time.deltaTime;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            patrolTimer += Time.deltaTime; // 순찰 지점 타이머 업데이트
            homePointTimer += Time.deltaTime; // 홈 포인트 타이머 업데이트
            yield return null; // 다음 프레임까지 대기
        }
    }

    /// <summary>
    /// 순찰 범위 내에 새로운 랜덤 지점을 설정합니다. (순찰 중심은 유지)
    /// </summary>
    public void SetNewPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint;

        randomDirection.y = monsterTransform.position.y;
        currentPatrolPoint = randomDirection;
    }

    /// <summary>
    /// 순찰 중심을 현재 위치로 변경하고 새로운 랜덤 지점을 설정합니다.
    /// </summary>
    private void UpdateHomePointAndPatrolPoint()
    {
        homePoint = monsterTransform.position;
        SetNewPatrolPoint();
        homePointTimer = 0f;
    }

    /// <summary>
    /// 충돌이 발생했을 때 호출되어 순찰 지점과 중심을 재설정합니다.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트의 태그가 'Player' 또는 'Monster'가 아닐 때만 반응합니다.
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Monster"))
        {
            StopPatrol();
            UpdateHomePointAndPatrolPoint(); // 충돌 시 순찰 중심과 목표 지점을 모두 변경합니다.
            StartPatrol();
        }
    }

    /// <summary>
    /// 현재 순찰 목표 지점의 값을 반환합니다.
    /// </summary>
    public Vector3 GetPatrolPoint()
    {
        return currentPatrolPoint;
    }

    private void OnDrawGizmosSelected()
    {
        if (homePoint != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(homePoint, patrolRadius);
        }
    }
}