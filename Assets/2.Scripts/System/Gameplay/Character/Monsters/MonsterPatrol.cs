using UnityEngine;
using System.Collections;
using static MonsterBase; // MonsterBase.MonsterState에 접근하기 위해 사용됩니다.

/// <summary>
/// 몬스터의 순찰 행동을 전담하는 클래스입니다.
/// 코루틴을 이용한 순찰 로직과 충돌 감지 후 경로 재설정 기능을 포함합니다.
/// 오브젝트 풀링 시스템 환경을 고려하여, 비활성화 시 코루틴 실행을 막는 안전 장치와 재활성화 시 초기화 로직을 추가했습니다.
/// SOLID 원칙: SRP(단일 책임 원칙)에 따라 순찰 로직 및 경로 관리를 전적으로 담당합니다.
/// </summary>
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

    // === [핵심 수정] 지형 보정 설정 필드를 Monster 클래스로 이동했기 때문에 이 필드들을 제거합니다. ===
    // [제거] public LayerMask groundLayer;
    // [제거] public float verticalOffset = 0.5f;

    // === 종속성 ===
    private Transform monsterTransform;
    // 현재 순찰 코루틴의 참조를 담아 중복 실행이나 강제 중지를 가능하게 합니다.
    private Coroutine patrolCoroutine;
    private MonsterBase monsterBase;
    private Monster monster; // Monster 클래스의 AdjustToGround 호출을 위해 필요

    // === 내부 변수 ===
    private Vector3 currentPatrolPoint;
    private float homePointTimer;

    private void Awake()
    {
        monsterTransform = this.transform;
        monsterBase = GetComponent<MonsterBase>();
        monster = GetComponent<Monster>(); // Monster 컴포넌트 참조

        if (monsterBase == null || monster == null)
        {
            Debug.LogError("MonsterPatrol: 필수 컴포넌트(MonsterBase 또는 Monster)를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 홈 포인트가 지정되지 않았으면 몬스터의 시작 위치를 순찰의 중심점으로 사용합니다.
        if (homePoint == Vector3.zero)
        {
            homePoint = monsterTransform.position;
        }

        // 타이머 초기화는 OnEnable에서 처리하여 풀링 재사용에 대비합니다.
    }

    /// <summary>
    /// 오브젝트가 활성화(오브젝트 풀에서 재사용)될 때 호출됩니다.
    /// 순찰 상태를 깨끗하게 초기화합니다.
    /// </summary>
    private void OnEnable()
    {
        // 이전 순찰 코루틴이 남아있다면 확실히 중지시킵니다.
        StopPatrol();

        // 타이머를 무작위로 초기화하여 모든 몬스터가 동시에 순찰 범위를 바꾸는 것을 방지합니다.
        homePointTimer = UnityEngine.Random.Range(0, homePointChangeInterval);

        // 여기서 StartPatrol을 바로 호출하기보다, MonsterBase의 State Machine에 따라 호출되도록 하는 것이
        // 더 유연하고 SOLID 원칙에 부합합니다. (예: MonsterBase.OnStateChange(Patrol) -> MonsterPatrol.StartPatrol())
        // 필요하다면 여기서 StartPatrol()을 호출할 수 있습니다.
    }

    /// <summary>
    /// 매 프레임 몬스터의 상태를 확인하여 사망 상태일 경우 순찰을 중지하고 풀로 반환합니다.
    /// 이 로직은 MonsterBase가 아닌 Patrol 컴포넌트의 Update에서 관리합니다.
    /// </summary>
    private void Update()
    {
        // 몬스터가 'Dead' 상태인 경우
        if (monsterBase != null && monsterBase.currentState == MonsterState.Dead)
        {
            StopPatrol(); // 코루틴이 실행 중이면 멈춥니다.
            return;
        }

        // homePointTimer += Time.deltaTime; // 타이머 증가 로직은 코루틴 안에서 처리합니다.
    }

    /// <summary>
    /// 외부에서 순찰 행동을 시작하는 메서드입니다.
    /// 이미 순찰 중이면 중복 실행을 방지하며, 비활성 상태에서는 코루틴 실행을 막아 에러를 방지합니다.
    /// </summary>
    public void StartPatrol()
    {
        // 오브젝트와 컴포넌트가 모두 활성화되어 있는지 확인하여 
        // "Coroutine couldn't be started because the game object is inactive!" 오류를 방지합니다.
        if (!isActiveAndEnabled)
        {
            // Debug.LogWarning($"{gameObject.name}이(가) 비활성화 상태이므로 순찰 코루틴을 시작할 수 없습니다.");
            return;
        }

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
                // 순찰 중심 변경 타이머가 만료되면 순찰 중심(HomePoint)도 함께 변경합니다.
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
                // 이동 속도 적용
                monsterTransform.position += direction * monsterBase.monsterData.moveSpeed * patrolSpeedMultiplier * Time.deltaTime;

                // 회전 (부드러운 시선 처리)
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, lookRotation, Time.deltaTime * 5f);

            }

            patrolTimer += Time.deltaTime; // 순찰 지점 타이머 업데이트
            homePointTimer += Time.deltaTime; // 홈 포인트 타이머 업데이트
            yield return null; // 다음 프레임까지 대기
        }
    }

    /// <summary>
    /// 순찰 중심(HomePoint)을 유지한 채 순찰 반경 내에 새로운 랜덤 지점을 설정합니다.
    /// </summary>
    public void SetNewPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += homePoint;

        // Y축은 현재 몬스터 위치와 동일하게 유지하여 공중으로 뜨는 것을 방지합니다.
        randomDirection.y = monsterTransform.position.y;
        currentPatrolPoint = randomDirection;
    }

    /// <summary>
    /// 순찰 중심을 현재 위치로 변경하고 새로운 랜덤 지점을 설정한 후 타이머를 리셋합니다.
    /// 몬스터가 한 구역에 너무 오래 머무르는 것을 방지합니다.
    /// </summary>
    private void UpdateHomePointAndPatrolPoint()
    {
        homePoint = monsterTransform.position;
        SetNewPatrolPoint();
        homePointTimer = 0f;
    }

    /// <summary>
    /// 충돌이 발생했을 때 호출되어 순찰 지점과 중심을 재설정하고 순찰을 재시작합니다.
    /// (벽이나 장애물 회피 로직)
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트의 태그가 'Player' 또는 'Monster'가 아닐 때만 반응합니다. 
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Monster"))
        {
            // 충돌 감지 -> 순찰 정지 -> 경로 변경 -> 순찰 재시작
            StopPatrol();
            UpdateHomePointAndPatrolPoint();
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

    // 개발/디버깅을 위한 시각화 코드
    private void OnDrawGizmosSelected()
    {
        // homePoint가 초기화되지 않은 경우 현재 위치를 중심으로 가정합니다.
        Vector3 gizmoHomePoint = (Application.isPlaying && monsterTransform != null) ? homePoint : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoHomePoint, patrolRadius);

        Gizmos.color = Color.cyan;
        if (Application.isPlaying)
        {
            // 현재 순찰 목표 지점을 시각화
            Gizmos.DrawSphere(currentPatrolPoint, 0.5f);
            // 몬스터 위치에서 목표 지점까지의 선을 시각화
            Gizmos.DrawLine(transform.position, currentPatrolPoint);
        }
    }
}