using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 코루틴 사용을 위해 추가

/// <summary>
/// 숲 보스 몬스터의 행동 로직 및 던전 매니저 알림 처리를 담당합니다.
/// 움직이지 않는 원거리 공격형 거대 나무 컨셉에 맞게 행동합니다.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
[RequireComponent(typeof(MonsterLoot))]
public class ForestBoss : MonoBehaviour, IBossInitializer
{
    // === 종속성 ===
    private IBossNotifier _bossNotifier;
    private Monster _monster;            // 몬스터 상태 접근 및 변경을 위한 컴포넌트
    private Transform _playerTransform;  // 플레이어 위치 추적을 위한 Transform

    // === 보스 행동 설정 변수 ===
    [Header("보스 공격 설정")]
    [Tooltip("플레이어를 감지하는 유효 사거리입니다. 이 거리 안에 들어와야 행동을 시작합니다.")]
    public float activationRange = 45f;
    [Tooltip("일반 공격 쿨타임입니다.")]
    public float attackCooldown = 2.5f;
    [Tooltip("특수 공격(Charge) 쿨타임입니다.")]
    public float chargeCooldown = 10.0f;
    [Tooltip("특수 공격(Charge) 시전 시간입니다. (애니메이션, 기 모으는 시간)")]
    public float chargeCastTime = 2.0f;
    [Tooltip("투사체 프리팹 (Resource 없으므로 Debug.Log로 대체 예정)")]
    public GameObject projectilePrefab;
    // [추가] 로직 1: 특수 공격 전용 감지 범위 설정 변수
    [Header("특수 공격 시스템 설정")]
    [Tooltip("특수 공격을 발동시키는 유효 사거리입니다. 일반 공격 감지 범위와 별개로 운영됩니다.")]
    public float specialActivationRange = 60f;
    // === 내부 시간 변수 ===
    private float _lastAttackTime;
    private float _lastChargeTime;
    private Coroutine _chargeRoutine; // 특수 공격 시전 코루틴 참조

    // [추가] 로직 2: 모든 특수 공격 코루틴 메서드를 저장하고 랜덤 선택에 사용할 델리게이트 리스트
    /// <summary>
    /// 특수 공격 그룹에 속한 모든 공격 코루틴 메서드(반환형: IEnumerator)를 담는 리스트입니다.
    /// 쿨타임이 찼을 때 이 리스트에서 무작위로 하나를 선택하여 실행하는 데 사용됩니다. (SOLID OCP 확장)
    /// </summary>
    private List<System.Func<IEnumerator>> _specialAttackRoutines = new List<System.Func<IEnumerator>>();


    // --- 일반 공격 (뿌리 내려치기) 설정 추가 ---
    [Header("일반 공격 디테일 (뿌리 내려치기)")]
    [Tooltip("일반 공격에 사용할 뿌리 시각 오브젝트 목록 (보스의 자식으로 별도 위치)")]
    public List<Transform> rootVisuals = new List<Transform>();

    [Tooltip("각 뿌리의 회전 중심점 역할을 할 빈 오브젝트 목록 (보스의 자식으로 별도 위치)")]
    public List<Transform> rootPivots = new List<Transform>();

    [Tooltip("뿌리 내려찍기 동작 시간 (Lerp/Slerp 속도)")]
    public float attackDownStrokeDuration = 0.6f;
    [Tooltip("뿌리 복귀 동작 시간")]
    public float attackReturnDuration = 1.0f;

    [Tooltip("뿌리 들어 올리는 초기 각도 (로컬 X축 기준)")]
    public float liftAngle = 45f;
    [Tooltip("뿌리가 내려찍는 최종 각도 (로컬 X축 기준)")]
    public float strikeAngle = -30f;
    // [주의] MonsterData.attackPower를 사용하므로 이 변수는 제거합니다.
    // --- 일반 공격 상태 관리 및 데이터 저장 변수 추가 ---
    /// <summary>
    /// 일반 공격(뿌리 내려치기) 코루틴의 참조를 저장하여 중복 실행을 방지합니다.
    /// 코루틴이 진행 중일 때는 null이 아닙니다.
    /// </summary>
    private Coroutine _basicAttackRoutine;

    /// <summary>
    /// 각 Root Visual 오브젝트의 초기 로컬 회전값(Quaternion)을 저장하는 딕셔너리입니다.
    /// 공격 후 원래 위치로 정확히 복귀시키기 위해 Awake() 시점에 초기화됩니다.
    /// Key: Root Visual Transform, Value: 초기 localRotation
    /// </summary>
    private Dictionary<Transform, Quaternion> _initialRootRotations;

    private void Awake()
    {
        // 1. 필수 컴포넌트 종속성 확보
        _monster = GetComponent<Monster>();
        if (_monster == null)
        {
            Debug.LogError("ForestBoss: Monster 컴포넌트가 필요합니다!");
            enabled = false;
        }

        // 2. 플레이어 Transform 찾기 (보스의 핵심 목표)
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("ForestBoss: Player (Tag:'Player')를 찾을 수 없습니다! 공격 로직 비활성화.");
        }

        // --- 3. 일반 공격 (뿌리 내려치기) 초기화 로직 추가 ---
        InitializeRootAttack();
        // [추가] 로직 4: 특수 공격 초기화 메서드 호출 (Awake 메서드의 끝에 추가)
        InitializeSpecialAttacks();
    }

    void Start()
    {
        // 보스는 생성 시 대기 상태(Idle)에서 시작하며, 플레이어 감지 시 공격을 시작합니다.
        _monster.ChangeState(MonsterBase.MonsterState.Idle);
        // 초기 공격 시간을 설정하여 즉시 공격하지 않도록 합니다.
        _lastAttackTime = Time.time;
        _lastChargeTime = Time.time;
    }

    void Update()
    {
        // 사망 상태 또는 플레이어가 없으면 행동을 멈춥니다.
        if (_monster.currentState == MonsterBase.MonsterState.Dead || _playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        // [수정] 플레이어 방향으로 고정적으로 회전 (나무 보스는 몸을 돌리는 대신 시선만 돌립니다.)
        // 일반 공격(뿌리 내려치기) 코루틴이 실행 중이 아니고,
        // [수정] 현재 상태가 특수 공격(Charge) 상태가 아닐 때만 회전합니다.
        if (_basicAttackRoutine == null && _monster.currentState != MonsterBase.MonsterState.Charge)
        {
            LookAtTarget(_playerTransform);
        }

        switch (_monster.currentState)
        {
            case MonsterBase.MonsterState.Idle:
                HandleIdle(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Attack:
                HandleAttack(distanceToPlayer);
                break;
            case MonsterBase.MonsterState.Charge:
                // Charge 상태에서는 코루틴이 알아서 시전 중이므로 Update에서는 대기합니다.
                break;
        }
    }

    // --- 몬스터 행동 처리 메서드 ---

    /// <summary>
    /// Idle 상태 처리: 플레이어가 사거리 내에 들어왔는지 확인하고 공격 상태로 전환합니다.
    /// [수정] 일반 공격 범위(activationRange) 대신 특수 공격 범위(specialActivationRange)를 기준으로 판단합니다.
    /// </summary>
    private void HandleIdle(float distanceToPlayer)
    {
        // [로직 1] 가장 넓은 특수 공격 범위 내에 들어왔는지 확인
        if (distanceToPlayer <= specialActivationRange)
        {
            _monster.ChangeState(MonsterBase.MonsterState.Attack);
        }
    }

    /// <summary>
    /// Attack 상태 처리: 일반 원거리 공격과 특수 공격(Charge) 중 무엇을 사용할지 판단합니다.
    /// </summary>
    private void HandleAttack(float distanceToPlayer)
    {
        // [로직 1] Idle로 복귀하는 기준을 가장 넓은 특수 공격 범위로 변경합니다.
        // 플레이어가 specialActivationRange 밖으로 나가면 Idle로 복귀합니다.
        if (distanceToPlayer > specialActivationRange)
        {
            _monster.ChangeState(MonsterBase.MonsterState.Idle);
            return;
        }

        // [로직 2] 특수 공격 쿨타임 확인 및 전환
        // (로직 1에서 이미 specialActivationRange 내에 있다는 것이 보장됨)
        if (Time.time >= _lastChargeTime + chargeCooldown)
        {
            _monster.ChangeState(MonsterBase.MonsterState.Charge);
            _chargeRoutine = StartCoroutine(PerformChargeAttack());
            return;
        }

        // [로직 3] 일반 공격 쿨타임 및 범위 확인 및 실행
        // 일반 공격은 오직 activationRange 내에서만 실행되도록 조건을 추가합니다.
        if (distanceToPlayer <= activationRange && Time.time >= _lastAttackTime + attackCooldown)
        {
            if (_basicAttackRoutine == null)
            {
                PerformBasicAttack();
            }
        }
    }

    /// <summary>
    /// 일반 공격(뿌리 내려치기) 코루틴을 시작하고, 쿨타임을 업데이트합니다.
    /// </summary>
    private void PerformBasicAttack()
    {
        // [수정] 기존의 Debug.Log를 제거하고 코루틴을 시작하는 역할만 수행
        // 코루틴 시작 전, 보스가 공격 상태인지 다시 한번 확인하는 것이 좋습니다.
        if (_monster.currentState != MonsterBase.MonsterState.Attack) return;

        // 코루틴 시작 및 참조 저장
        _basicAttackRoutine = StartCoroutine(PerformBasicAttackRoutine());

        // 쿨타임 업데이트는 코루틴 내부 (공격 완료 시점)에서 처리할 예정이므로 여기서는 제거합니다.
        // _lastAttackTime = Time.time; 
    }
    /// <summary>
    /// 실제 일반 공격 동작을 시간 흐름에 따라 처리하는 코루틴입니다.
    /// (뿌리 선택 -> 들어 올리기 -> 내려찍기 및 피해 판정 -> 복귀)
    /// </summary>
    private IEnumerator PerformBasicAttackRoutine()
    {
        // 1. 공격에 사용할 뿌리 선택
        if (rootPivots.Count == 0)
        {
            Debug.LogError("ForestBoss: 공격할 뿌리(Root Pivots)가 없습니다. 공격 코루틴을 종료합니다.");
            _lastAttackTime = Time.time;
            _basicAttackRoutine = null;
            yield break;
        }

        int randomIndex = Random.Range(0, rootPivots.Count);
        Transform targetRootPivot = rootPivots[randomIndex];
        Transform targetRootVisual = rootVisuals[randomIndex];

        // [수정] RootHitbox 컴포넌트를 여기서 미리 가져옵니다!
        RootHitbox hitbox = targetRootVisual.GetComponent<RootHitbox>(); // <--- 이 라인 추가!

        if (hitbox == null)
        {
            Debug.LogError($"ForestBoss: {targetRootVisual.name}에 RootHitbox가 없습니다. 공격 코루틴을 종료합니다.");
            _lastAttackTime = Time.time;
            _basicAttackRoutine = null;
            yield break;
        }
        // ----------------------------------------------------------------------------------
        // 1.5. 플레이어 방향으로 Y축 회전 (공격 시점 플레이어 위치 고정) <--- 여기에 추가
        // ----------------------------------------------------------------------------------
        Vector3 directionToPlayer = (_playerTransform.position - targetRootPivot.position).normalized;
        directionToPlayer.y = 0;
        Quaternion targetYRotation = Quaternion.LookRotation(directionToPlayer);
        targetRootPivot.rotation = targetYRotation; // 월드 회전 적용
        // **참고:** 사용자님의 조치(Pivot을 Visual의 부모로 설정)로 인해 코드는 targetRootPivot에 회전을 적용합니다.

        // ----------------------------------------------------------------------------------
        // 2. 뿌리 들어 올리기 동작 (Lift)
        // ----------------------------------------------------------------------------------

        Quaternion liftStartRotation = targetRootPivot.localRotation;

        // [수정 적용] 부호를 반전시켜 들어 올리는 목표 회전을 계산합니다.
        Quaternion liftTargetRotation = liftStartRotation * Quaternion.Euler(-liftAngle, 0, 0);

        float timeElapsed = 0f;
        float liftDuration = attackDownStrokeDuration; // 들어 올리는 시간은 내려찍는 시간과 동일하게 설정

        while (timeElapsed < liftDuration)
        {
            float t = timeElapsed / liftDuration;
            targetRootPivot.localRotation = Quaternion.Slerp(
                liftStartRotation,
                liftTargetRotation,
                t
            );
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        targetRootPivot.localRotation = liftTargetRotation;


        // ----------------------------------------------------------------------------------
        // 3. 뿌리 내려찍기 동작 (Strike) 및 피해 판정
        // ----------------------------------------------------------------------------------

        // 내려찍기 시작점: liftTargetRotation (들어 올리기 완료 지점)
        // 내려찍기 목표점: liftStartRotation에서 strikeAngle(-30f)만큼 회전
        // strikeAngle 역시 내리는 동작이므로 liftAngle과 동일한 부호 반전 로직을 따릅니다.
        Quaternion strikeTargetRotation = liftStartRotation * Quaternion.Euler(-strikeAngle, 0, 0);

        timeElapsed = 0f;
        // 내려찍기 시간은 attackDownStrokeDuration을 사용합니다.

        // 피해 판정 활성화 (단, 이미 활성화된 상태일 수 있으므로 StartStrike()가 안전합니다.)
        hitbox.StartStrike(_monster.monsterData.attackPower);

        while (timeElapsed < attackDownStrokeDuration)
        {
            float t = timeElapsed / attackDownStrokeDuration;
            targetRootPivot.localRotation = Quaternion.Slerp(
                liftTargetRotation, // 들어 올린 위치에서 시작
                strikeTargetRotation, // 내려찍을 위치로 이동
                t
            );
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        targetRootPivot.localRotation = strikeTargetRotation;

        // 내려찍는 동작이 완료된 후, 히트박스를 비활성화합니다.
        hitbox.EndStrike();


        // ----------------------------------------------------------------------------------
        // 4. 뿌리 복귀 동작 (Return)
        // ----------------------------------------------------------------------------------

        // 복귀 시작점: strikeTargetRotation (내려찍기 완료 지점)
        // 복귀 목표점: _initialRootRotations에서 가져와야 하지만, 이 코루틴 내부에서는 startRotation이 가장 확실한 초기 상태입니다.
        // **수정**: InitializeRootAttack()에서 _initialRootRotations에 'pivot'의 로컬 회전을 저장했으므로, 딕셔너리를 사용합니다.

        Quaternion returnStartRotation = targetRootPivot.localRotation;
        Quaternion initialRotation;

        // 초기 회전값 딕셔너리에서 안전하게 로드합니다. (Awake에서 초기화됨)
        if (!_initialRootRotations.TryGetValue(targetRootPivot, out initialRotation))
        {
            Debug.LogWarning("ForestBoss: 초기 회전값을 찾을 수 없습니다. 현재 로컬 회전을 복귀 목표로 사용합니다.");
            initialRotation = returnStartRotation; // 비상 시 현재 위치 사용
        }

        timeElapsed = 0f;

        while (timeElapsed < attackReturnDuration)
        {
            float t = timeElapsed / attackReturnDuration;
            targetRootPivot.localRotation = Quaternion.Slerp(
                returnStartRotation, // 내려찍은 위치에서 시작
                initialRotation, // 초기 위치로 복귀
                t
            );
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        targetRootPivot.localRotation = initialRotation;

        // 5. 공격 종료
        _lastAttackTime = Time.time;
        _basicAttackRoutine = null;
    }
    /// <summary>
    /// 특수 공격(Charge)을 코루틴으로 실행합니다. (예: 거대 뿌리 폭발 -> **이제 모든 특수 공격의 진입점**)
    /// Charge 상태는 시전 시간(ChargeCastTime)이 필요합니다.
    /// </summary>
    private IEnumerator PerformChargeAttack()
    {
        Debug.Log($"ForestBoss: 특수 공격 그룹 시전 시작! ({chargeCastTime}초)");

        // 1. 시전 시간 대기 (애니메이션, 기 모으기)
        yield return new WaitForSeconds(chargeCastTime);

        // 2. [수정] 등록된 특수 공격 중 하나를 랜덤으로 선택하여 실행합니다.
        if (_specialAttackRoutines.Count == 0)
        {
            Debug.LogError("ForestBoss: 등록된 특수 공격이 없습니다. 공격을 종료합니다.");
            // 쿨타임만 업데이트하고 종료
            _lastChargeTime = Time.time;
            _monster.ChangeState(MonsterBase.MonsterState.Attack);
            _chargeRoutine = null;
            yield break;
        }

        // 델리게이트 리스트에서 랜덤으로 공격을 선택합니다.
        int randomIndex = Random.Range(0, _specialAttackRoutines.Count);
        System.Func<IEnumerator> selectedAttack = _specialAttackRoutines[randomIndex];

        // 3. [수정] 선택된 공격 코루틴을 실행하고, 해당 코루틴이 끝날 때까지 대기합니다.
        // **쿨타임 업데이트 및 상태 복귀는 이제 개별 공격 코루틴에서 담당합니다.**
        Debug.Log($"ForestBoss: 랜덤 선택된 공격 코루틴 실행. (인덱스: {randomIndex})");
        yield return StartCoroutine(selectedAttack.Invoke());

        // **참고**: 이 코루틴은 selectedAttack이 끝날 때까지 대기한 후 자동으로 종료됩니다.
        // 쿨타임 업데이트와 상태 복귀는 이미 개별 공격 코루틴에서 처리했으므로, 
        // 여기서는 별도의 종료 로직이 필요 없습니다. 
    }

    /// <summary>
    /// 보스가 움직이지 않고 플레이어 방향으로만 회전하도록 처리합니다.
    /// </summary>
    private void LookAtTarget(Transform targetTransform)
    {
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        // Y축 회전만 계산하여 몸통이 아닌 시선만 돌리게 합니다.
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
    /// <summary>
    /// 일반 공격에 사용될 뿌리 시각 오브젝트들의 초기 회전값을 저장하고,
    /// 필요한 컴포넌트(RootHitbox)가 올바르게 부착되어 있는지 검증합니다.
    /// </summary>
    private void InitializeRootAttack()
    {
        // 1. 딕셔너리 초기화
        _initialRootRotations = new Dictionary<Transform, Quaternion>();

        // 2. Pivot과 Visual 목록 길이 검사 (두 목록의 길이가 같아야 함)
        if (rootVisuals.Count != rootPivots.Count)
        {
            Debug.LogError("ForestBoss: rootVisuals 목록의 길이와 rootPivots 목록의 길이가 일치하지 않습니다! 초기화 실패.");
            return;
        }

        // 3. 각 뿌리 Visual의 초기 회전값 저장 및 RootHitbox 검증
        for (int i = 0; i < rootVisuals.Count; i++)
        {
            Transform visual = rootVisuals[i];

            // a. 초기 회전값 저장 (Pivot의 로컬 회전을 저장해야 함)
            Transform pivot = rootPivots[i];
            _initialRootRotations.Add(pivot, pivot.localRotation);

            // b. RootHitbox 컴포넌트 검증
            RootHitbox hitbox = visual.GetComponent<RootHitbox>();
            if (hitbox == null)
            {
                Debug.LogError($"ForestBoss: Root Visual [{visual.name}]에 RootHitbox 컴포넌트가 없습니다! 공격이 제대로 동작하지 않습니다.");
            }
        }

        if (rootVisuals.Count == 0)
        {
            Debug.LogWarning("ForestBoss: 일반 공격에 사용할 뿌리(rootVisuals)가 지정되지 않았습니다. 뿌리 내려치기 공격이 비활성화됩니다.");
        }
    }
    // [추가] 로직 1: 특수 공격 1: 뿌리 소환 코루틴 틀 추가
    /// <summary>
    /// 특수 공격 1: 뿌리 소환(Root Summon) 공격을 실행하는 코루틴입니다. (랜덤 테스트용 더미)
    /// 이 메서드는 PerformChargeAttack()에 의해 무작위로 호출됩니다.
    /// </summary>
    private IEnumerator PerformRootSummon()
    {
        Debug.Log("ForestBoss: (특수 공격 1) '뿌리 소환' 시전 시작! (시전 시간 1.0초 가정)");
        yield return new WaitForSeconds(1.0f); // 임시 시전 시간

        // [랜덤 테스트용]
        Debug.Log("<color=yellow>ForestBoss: (특수 공격 1) '뿌리 소환' 발동! (랜덤 실행 성공)</color>");

        // 쿨타임 업데이트 및 상태 복귀 로직
        _lastChargeTime = Time.time;
        _monster.ChangeState(MonsterBase.MonsterState.Attack);
        _chargeRoutine = null; // 진입점 코루틴 해제
    }
    // [추가] 로직 2: 특수 공격 2: 몬스터 소환 코루틴 틀 추가
    /// <summary>
    /// 특수 공격 2: 몬스터 소환(Monster Summon) 공격을 실행하는 코루틴입니다. (랜덤 테스트용 더미)
    /// 이 메서드는 PerformChargeAttack()에 의해 무작위로 호출됩니다.
    /// </summary>
    private IEnumerator PerformMonsterSummon()
    {
        Debug.Log("ForestBoss: (특수 공격 2) '몬스터 소환' 시전 시작! (시전 시간 2.0초 가정)");
        yield return new WaitForSeconds(2.0f); // 임시 시전 시간

        // [랜덤 테스트용]
        Debug.Log("<color=red>ForestBoss: (특수 공격 2) '몬스터 소환' 발동! (랜덤 실행 성공)</color>");

        // 쿨타임 업데이트 및 상태 복귀 로직 (나중에 몬스터 사망 대기 로직으로 대체됨)
        _lastChargeTime = Time.time;
        _monster.ChangeState(MonsterBase.MonsterState.Attack);
        _chargeRoutine = null; // 진입점 코루틴 해제
    }
    // 메서드 역할: 특수 공격 3: 낙뢰 공격 (Lightning Strike)
    /// <summary>
    /// 특수 공격 3: 플레이어 위치에 낙뢰 공격을 실행하는 코루틴입니다. (새로운 공격 추가 예시)
    /// </summary>
    private IEnumerator PerformLightningStrike()
    {
        // [로직 1] 시전 시간 대기 (애니메이션, 기 모으기)
        Debug.Log("ForestBoss: (특수 공격 3) '낙뢰 공격' 시전 시작! (시전 시간 1.5초 가정)");
        yield return new WaitForSeconds(1.5f);

        // [로직 2] 실제 공격 로직 실행 (지금은 더미)
        Debug.Log("<color=lime>ForestBoss: (특수 공격 3) '낙뢰 공격' 발동! (플레이어 위치에 이펙트 표시)</color>");

        // [로직 3] 쿨타임 업데이트 및 상태 복귀 (필수 마무리 로직)
        _lastChargeTime = Time.time;
        _monster.ChangeState(MonsterBase.MonsterState.Attack);
        _chargeRoutine = null; // 진입점 코루틴 해제 (PerformChargeAttack 코루틴 종료 유도)
    }
    // [추가] 로직 3: 특수 공격 목록을 초기화하고 등록하는 메서드 추가
    /// <summary>
    /// 모든 특수 공격 코루틴 메서드를 리스트에 등록하여, 랜덤 실행을 위한 준비를 합니다.
    /// OCP를 준수하여 새로운 특수 공격이 추가될 때마다 이 메서드만 수정됩니다.
    /// </summary>
    // 메서드 역할: 모든 특수 공격 목록을 초기화하고 등록하는 메서드
    private void InitializeSpecialAttacks()
    {
        _specialAttackRoutines.Clear();

        // 기존 공격 등록
        _specialAttackRoutines.Add(PerformRootSummon);
        _specialAttackRoutines.Add(PerformMonsterSummon);

        // [로직 1] 새로운 특수 공격을 리스트에 추가합니다.
        //_specialAttackRoutines.Add(PerformLightningStrike);

        Debug.Log($"ForestBoss: 특수 공격 {_specialAttackRoutines.Count}개가 리스트에 등록되었습니다.");
    }
    // --- 던전 매니저 알림 로직 (기존 유지) ---

    /// <summary>
    /// 이 보스 몬스터에게 처치 알림을 받을 객체(Notifier)를 설정합니다. (의존성 주입)
    /// </summary>
    /// <param name="notifier">IBossNotifier 인터페이스를 구현한 객체</param>
    public void SetNotifier(IBossNotifier notifier)
    {
        _bossNotifier = notifier;
    }

    /// <summary>
    /// 몬스터가 파괴되기 직전에 호출됩니다. (보스 사망 감지 시점)
    /// 이 시점을 활용하여 DungeonManager에 보스가 처치되었음을 알립니다.
    /// </summary>
    private void OnDestroy()
    {
        // Charge 코루틴이 진행 중이라면 정지합니다. (안전 장치)
        if (_chargeRoutine != null)
        {
            StopCoroutine(_chargeRoutine);
        }

        if (_bossNotifier != null)
        {
            _bossNotifier.NotifyBossDefeated();
            _bossNotifier = null;
        }
        else
        {
            Debug.LogWarning("ForestBoss: Notifier가 설정되지 않아 DungeonManager에 알림을 보낼 수 없습니다. (테스트 또는 초기화 오류)");
        }
    }
}