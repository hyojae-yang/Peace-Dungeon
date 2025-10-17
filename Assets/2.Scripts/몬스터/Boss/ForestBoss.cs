using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 코루틴 사용을 위해 추가

/// <summary>
/// 숲 보스 몬스터의 행동 로직 및 던전 매니저 알림 처리를 담당합니다.
/// 움직이지 않는 원거리 공격형 거대 나무 컨셉에 맞게 행동합니다.
/// </summary>
public class ForestBoss : MonoBehaviour, IBossInitializer
{
    // === 종속성 ===
    private IBossNotifier _bossNotifier;
    private Monster _monster;            // 몬스터 상태 접근 및 변경을 위한 컴포넌트
    private MonsterCombat _monsterCombat;
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
    // [추가] 로직 1: 특수 공격 전용 감지 범위 설정 변수
    [Header("특수 공격 시스템 설정")]
    [Tooltip("특수 공격을 발동시키는 유효 사거리입니다. 일반 공격 감지 범위와 별개로 운영됩니다.")]
    public float specialActivationRange = 60f;
    // === 내부 시간 변수 ===
    private float _lastAttackTime;
    private float _lastChargeTime;
    private Coroutine _chargeRoutine; // 특수 공격 시전 코루틴 참조

    // [로직 1] 뿌리 소환 공격에 필요한 프리팹 및 범위 설정
    [Header("특수 공격 1: 뿌리 소환 (Root Summon)")]
    [Tooltip("소환될 뿌리(데미지 판정을 가진 오브젝트) 프리팹입니다.")]
    public GameObject summonRootPrefab;

    [Tooltip("뿌리를 소환할 평면 영역(Planar Area)을 나타내는 **Collider**입니다. 이 콜라이더의 월드 바운드를 사용하여 무작위 위치를 계산합니다.")]
    private Collider rootSummonAreaCollider; // Collider를 사용해 정확한 영역을 얻습니다.


    // [로직 2] 공격 세부 조건 설정
    [Tooltip("한 번에 소환할 뿌리의 개수입니다.")]
    public int numberOfRootsToSummon = 7;
    // [새로 추가된 변수] 플레이어 주변 집중 설정
    [Tooltip("소환 위치가 플레이어로부터 얼마나 멀리 떨어질 수 있는지를 결정하는 최대 반경입니다. (0으로 설정하면 기존 영역 무시, 이 반경 안에서만 소환됨)")]
    public float maxDistanceFromPlayer = 15f;

    [Tooltip("플레이어 주변에 소환될 뿌리의 **비율(%)**입니다. (예: 0.75 = 75%) 나머지 뿌리는 기존 영역에 무작위로 소환됩니다.")]
    [Range(0f, 1f)]
    public float playerFocusedRootRatio = 0.6f;

    [Header("특수 공격 2: 몬스터 소환 (Monster Summon)")]
    [Tooltip("소환할 일반 몬스터 프리팹 목록입니다. (Monster 컴포넌트 및 자체 AI 로직 필수)")]
    public GameObject[] minionPrefabs; // 소환할 몬스터 배열

    [Tooltip("한 번에 소환할 몬스터의 최소 개수입니다.")]
    public int minNumberOfMinions = 2; // 최소 마릿수

    [Tooltip("한 번에 소환할 몬스터의 최대 개수입니다.")]
    public int maxNumberOfMinions = 5; // 최대 마릿수

    [Tooltip("몬스터를 소환할 위치를 결정하는 최대 반경입니다. (플레이어 위치 기준)")]
    public float minionSpawnRadius = 20f; // 소환 반경

    [Tooltip("플레이어 주변에 집중적으로 소환될 몬스터의 비율(%)입니다. (예: 0.6 = 60%)")]
    [Range(0f, 1f)]
    public float playerFocusedMinionRatio = 0.6f; // 플레이어 집중 비율

    
    // [추가] 로직 4: 분노(Enrage) 상태 관리 변수
    [Header("보스 분노(Enrage) 상태 설정")]
    [Tooltip("보스가 분노 상태로 전환되는 체력 임계값 비율입니다. (예: 0.3 = 30%)")]
    public float enrageHealthThreshold = 0.3f; // [사용자 요청] 전역 변수 최소화

    /// <summary>
    /// 분노 상태 진입 로직이 이미 한 번 실행되었는지 추적하는 플래그입니다. 
    /// 이 플래그가 참이면 이벤트 핸들러에서 더 이상 검사하지 않습니다.
    /// [사용자 요청] 단 한 번의 호출을 보장하는 최소한의 상태 변수입니다.
    /// </summary>
    private bool _hasEnraged = false;
    // =========================================================================================

    // [추가] 로직 2: 모든 특수 공격 코루틴 메서드를 저장하고 랜덤 선택에 사용할 델리게이트 리스트
    /// <summary>
    /// 특수 공격 그룹에 속한 모든 공격 코루틴 메서드(반환형: IEnumerator)를 담는 리스트입니다.
    /// 쿨타임이 찼을 때 이 리스트에서 무작위로 하나를 선택하여 실행하는 데 사용됩니다. (SOLID OCP 확장)
    /// </summary>
    private List<System.Func<IEnumerator>> _specialAttackRoutines = new List<System.Func<IEnumerator>>();
    /// <summary>
    /// 특수 공격(PerformMonsterSummon)으로 소환된 모든 미니언의 GameObject 참조를 저장하는 리스트입니다.
    /// 보스 사망 시 남아있는 미니언을 모두 정리(Destroy)하는 데 사용됩니다.
    /// </summary>
    private List<GameObject> _activeMinions = new List<GameObject>();

    // --- 일반 공격 (뿌리 내려치기) 설정 추가 ---
    [Header("일반 공격 디테일 (뿌리 내려치기)")]
    [Tooltip("일반 공격에 사용할 뿌리 시각 오브젝트 목록 (보스의 자식으로 별도 위치)")]
    public List<Transform> rootVisuals = new List<Transform>();

    [Tooltip("각 뿌리의 회전 중심점 역할을 할 빈 오브젝트 목록 (보스의 자식으로 별도 위치)")]
    public List<Transform> rootPivots = new List<Transform>();

    [Tooltip("뿌리 내려찍기 동작 시간 (Lerp/Slerp 속도)")]
    public float attackDownStrokeDuration = 0.5f;
    [Tooltip("뿌리 복귀 동작 시간")]
    public float attackReturnDuration = 0.8f;

    [Tooltip("뿌리 들어 올리는 초기 각도 (로컬 X축 기준)")]
    public float liftAngle = 45f;
    [Tooltip("뿌리가 내려찍는 최종 각도 (로컬 X축 기준)")]
    public float strikeAngle = -30f;

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
    // ========================= 필드 추가 (1개) ===========================
    /// <summary>
    /// 이 플래그가 false일 경우, OnDestroy에서 DungeonManager로의 처치 알림을 생략합니다.
    /// DungeonManager.DeadDungeon() 메서드에서 강제 파괴를 시작하기 전에 false로 설정합니다.
    /// 기본값은 true로, MonsterCombat/Monster.Die()를 통한 정상 처치 시에는 알림이 정상적으로 전송됩니다.
    /// </summary>
    private bool _shouldNotifyDefeat = true;
    // ===================================================================
    private void Awake()
    {
        // 1. 필수 컴포넌트 종속성 확보
        _monster = GetComponent<Monster>();
        // [수정/추가] MonsterCombat 컴포넌트 확보
        _monsterCombat = GetComponent<MonsterCombat>();

        if (_monster == null || _monsterCombat == null) // MonsterCombat 유효성 검사 추가
        {
            Debug.LogError("ForestBoss: Monster 또는 MonsterCombat 컴포넌트가 필요합니다!");
            enabled = false;
            return;
        }
        if (_monster == null)
        {
            Debug.LogError("ForestBoss: Monster 컴포넌트가 필요합니다!");
            enabled = false;
        }
        // [추가] 2. MonsterCombat 이벤트 구독
        /// <summary>
        /// MonsterCombat에서 데미지를 입었음을 알리는 이벤트를 구독하여, 
        /// 체력 변화 시마다 분노 상태 임계점을 검사하도록 합니다. (SRP 준수)
        /// </summary>
        _monsterCombat.OnDamageTaken += OnMonsterDamaged;
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
        if (_monster.currentState == MonsterBase.MonsterState.Dead || MainSceneManager.Instance.isGameOver)
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

        yield return StartCoroutine(selectedAttack.Invoke());

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
    /// 특수 공격 1: 뿌리 소환(Root Summon) 공격을 실행하는 코루틴입니다. 
    /// 지정된 영역 내 무작위 위치에 경고 마커를 소환하며, 플레이어 주변 집중 비율에 따라 위치를 보정합니다.
    /// </summary>
    private IEnumerator PerformRootSummon()
    {
        // [로직 1] 유효성 검사 (Collider와 프리팹 할당 확인)
        // 소환 영역이 설정되지 않았거나 프리팹이 없다면 즉시 종료
        if (rootSummonAreaCollider == null || summonRootPrefab == null)
        {
            Debug.LogError("ForestBoss: RootSummonAreaCollider 또는 SummonRootPrefab이 할당되지 않았습니다. 뿌리 소환 공격을 종료합니다.");
            _lastChargeTime = Time.time;
            _monster.ChangeState(MonsterBase.MonsterState.Attack);
            _chargeRoutine = null;
            yield break;
        }

        // =========================================================================================
        // [수정된 로직] Collider.bounds 기반 무작위 소환 위치 계산 및 핸들러 초기화
        // =========================================================================================
        Bounds areaBounds = rootSummonAreaCollider.bounds; // Collider의 월드 공간 Bounds 정보를 가져옵니다.
        Vector3 playerPos = _playerTransform.position; // 플레이어 월드 위치를 한 번 저장합니다.
        float rootMagicDamage = _monster.monsterData.magicAttackPower; // 공격력을 한 번만 계산

        // [추가] 플레이어 집중 소환 개수 계산
        /// <summary>
        /// 플레이어 주변에 집중적으로 소환될 뿌리의 개수입니다.
        /// </summary>
        int focusedRootCount = Mathf.RoundToInt(numberOfRootsToSummon * playerFocusedRootRatio);
        /// <summary>
        /// 소환 영역 내 무작위 위치에 소환될 뿌리의 개수입니다.
        /// </summary>
        int randomRootCount = numberOfRootsToSummon - focusedRootCount;

        List<GameObject> warningMarkers = new List<GameObject>(); // 다음 단계에서 제거 예정

        // -----------------------------------------------------------------------------------------
        // A. [새로운 로직] 플레이어 주변 집중 소환
        // -----------------------------------------------------------------------------------------
        for (int i = 0; i < focusedRootCount; i++)
        {
            // 1. 플레이어 위치를 기준으로 원형 범위 내 랜덤 위치를 구합니다.
            Vector2 randomCircle = Random.insideUnitCircle * maxDistanceFromPlayer;
            float randomWorldX = playerPos.x + randomCircle.x;
            float randomWorldZ = playerPos.z + randomCircle.y;

            // 2. 소환 위치를 소환 영역 내로 클램프(Clamp)합니다.
            randomWorldX = Mathf.Clamp(randomWorldX, areaBounds.min.x, areaBounds.max.x);
            randomWorldZ = Mathf.Clamp(randomWorldZ, areaBounds.min.z, areaBounds.max.z);

            // 3. Y축은 Area의 바닥(월드 최소 Y)으로 고정합니다. (땅에 소환)
            float spawnWorldY = areaBounds.min.y;
            Vector3 spawnWorldPosition = new Vector3(randomWorldX, spawnWorldY, randomWorldZ);

            // 4. 경고 이펙트 인스턴스화
            GameObject warningRoot = Instantiate(summonRootPrefab, spawnWorldPosition, Quaternion.identity);

            // 5. [핵심 로직 추가] RootSummonHandler를 가져와 초기화합니다.
            RootSummonHandler handler = warningRoot.GetComponent<RootSummonHandler>();

            if (handler != null)
            {
                // 공격력(Damage)을 주입하고, 핸들러 내부 코루틴을 시작시킵니다.
                handler.InitializeAndStartAttack(rootMagicDamage);
            }
            else
            {
                Debug.LogError($"ForestBoss: 생성된 뿌리 '{warningRoot.name}'에서 RootSummonHandler 컴포넌트를 찾을 수 없습니다. (Instantiate 오류)");
                Destroy(warningRoot);
            }

            warningMarkers.Add(warningRoot); // 임시 리스트에 추가 (다음 단계에서 제거 예정)
        }

        // -----------------------------------------------------------------------------------------
        // B. [기존 로직 유지] 넓은 영역 무작위 소환 (나머지 개수)
        // -----------------------------------------------------------------------------------------
        Vector3 worldCenter = areaBounds.center;
        Vector3 worldExtents = areaBounds.extents;

        for (int i = 0; i < randomRootCount; i++)
        {
            // 1. 월드 좌표계에서 X와 Z의 랜덤 값을 계산 (월드 바운드 내 무작위 위치)
            float randomWorldX = Random.Range(worldCenter.x - worldExtents.x, worldCenter.x + worldExtents.x);
            float randomWorldZ = Random.Range(worldCenter.z - worldExtents.z, worldCenter.z + worldExtents.z);

            // 2. Y축은 Area의 바닥(월드 최소 Y)으로 고정하여 땅에 소환되도록 합니다.
            float spawnWorldY = areaBounds.min.y;

            Vector3 spawnWorldPosition = new Vector3(randomWorldX, spawnWorldY, randomWorldZ);

            // 3. 경고 이펙트(summonRootPrefab) 인스턴스화
            GameObject warningRoot = Instantiate(summonRootPrefab, spawnWorldPosition, Quaternion.identity);

            // 4. [핵심 로직 추가] RootSummonHandler를 가져와 초기화합니다.
            RootSummonHandler handler = warningRoot.GetComponent<RootSummonHandler>();

            if (handler != null)
            {
                // 공격력(Damage)을 주입하고, 핸들러 내부 코루틴을 시작시킵니다.
                handler.InitializeAndStartAttack(rootMagicDamage);
            }
            else
            {
                Debug.LogError($"ForestBoss: 생성된 뿌리 '{warningRoot.name}'에서 RootSummonHandler 컴포넌트를 찾을 수 없습니다. (Instantiate 오류)");
                Destroy(warningRoot);
            }

            warningMarkers.Add(warningRoot); // 임시 리스트에 추가
        }

        // 5. 쿨타임 업데이트 및 상태 복귀 로직
        _lastChargeTime = Time.time;
        _monster.ChangeState(MonsterBase.MonsterState.Attack);
        _chargeRoutine = null;

        // **참고**: RootSummonHandler가 즉시 공격 코루틴을 시작하므로 이 코루틴은 즉시 종료됩니다.
        yield break;
    }
    // [수정] 로직 2: 특수 공격 2: 몬스터 소환 코루틴
    /// <summary>
    /// 특수 공격 2: 몬스터 소환(Monster Summon) 공격을 실행하는 코루틴입니다.
    /// 지정된 수의 몬스터를 플레이어 주변에 소환한 후, 즉시 Attack 상태로 복귀합니다.
    /// </summary>
    private IEnumerator PerformMonsterSummon()
    {
        // 1. 유효성 검사: 필수 변수 할당 확인 (Instantiate만 수행하므로 이 검사가 중요합니다.)
        if (minionPrefabs == null || minionPrefabs.Length == 0 || rootSummonAreaCollider == null)
        {
            Debug.LogError("ForestBoss: MinionPrefabs 리스트가 비어 있거나, RootSummonAreaCollider가 할당되지 않았습니다. 몬스터 소환 공격을 종료하고 복귀합니다.");
            // 실패 시에도 쿨타임 및 상태 복귀 로직은 실행되어야 합니다.
            _lastChargeTime = Time.time;
            _monster.ChangeState(MonsterBase.MonsterState.Attack);
            _chargeRoutine = null;
            yield break;
        }

        // 2. 소환할 최종 마릿수 랜덤 결정
        // [로직] 최소/최대 마릿수 사이에서 랜덤으로 개수를 결정합니다.
        int totalMinionsToSummon = Random.Range(minNumberOfMinions, maxNumberOfMinions + 1);

        // 3. 시전 시간 대기 (애니메이션, 시각적 효과 등)
        yield return new WaitForSeconds(chargeCastTime); // PerformChargeAttack에서 이미 대기했으므로, 여기서는 추가 시전 시간만 적용 (예시: 0.5초)
                                                         // 참고: PerformChargeAttack에서 이미 chargeCastTime만큼 대기했으므로, 여기서는 짧은 시간만 대기하거나 아예 0으로 설정 가능
        yield return new WaitForSeconds(0.1f);

        // 4. 소환 로직 준비
        Bounds areaBounds = rootSummonAreaCollider.bounds;
        Vector3 playerPos = _playerTransform.position;

        // 플레이어 집중 소환 개수 계산 (뿌리 소환과 동일한 로직)
        int focusedMinionCount = Mathf.RoundToInt(totalMinionsToSummon * playerFocusedMinionRatio);
        int randomMinionCount = totalMinionsToSummon - focusedMinionCount;

        // --- 소환 루틴 A: 플레이어 주변 집중 소환 ---
        for (int i = 0; i < focusedMinionCount; i++)
        {
            // 1. 플레이어 주변 랜덤 위치 계산
            Vector2 randomCircle = Random.insideUnitCircle * minionSpawnRadius;
            float randomWorldX = playerPos.x + randomCircle.x;
            float randomWorldZ = playerPos.z + randomCircle.y;

            // 2. 위치를 소환 영역 내로 클램프(Clamp)
            randomWorldX = Mathf.Clamp(randomWorldX, areaBounds.min.x, areaBounds.max.x);
            randomWorldZ = Mathf.Clamp(randomWorldZ, areaBounds.min.z, areaBounds.max.z);
            float spawnWorldY = areaBounds.min.y; // 땅바닥 Y축
            Vector3 spawnWorldPosition = new Vector3(randomWorldX, spawnWorldY, randomWorldZ);

            // 3. [로직] 몬스터 프리팹 배열에서 랜덤으로 하나를 선택
            GameObject selectedPrefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];

            // 4. 몬스터 인스턴스화 (Instantiate만 수행하며, 미니언의 초기화는 자체 스크립트에 맡깁니다.)
            GameObject newMinion = Instantiate(selectedPrefab, spawnWorldPosition, Quaternion.identity);
            // [추가] 소환된 몬스터를 리스트에 등록합니다.
            _activeMinions.Add(newMinion); // <--- 이 라인 추가!
        }

        // --- 소환 루틴 B: 넓은 영역 무작위 소환 ---
        Vector3 worldCenter = areaBounds.center;
        Vector3 worldExtents = areaBounds.extents;

        for (int i = 0; i < randomMinionCount; i++)
        {
            // 1. 월드 바운드 내 무작위 위치 계산
            float randomWorldX = Random.Range(worldCenter.x - worldExtents.x, worldCenter.x + worldExtents.x);
            float randomWorldZ = Random.Range(worldCenter.z - worldExtents.z, worldCenter.z + worldExtents.z);
            float spawnWorldY = areaBounds.min.y;
            Vector3 spawnWorldPosition = new Vector3(randomWorldX, spawnWorldY, randomWorldZ);

            // 2. [로직] 몬스터 프리팹 배열에서 랜덤으로 하나를 선택
            GameObject selectedPrefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];

            // 3. 몬스터 인스턴스화 (Instantiate만 수행)
            GameObject newMinion = Instantiate(selectedPrefab, spawnWorldPosition, Quaternion.identity);
            // [추가] 소환된 몬스터를 리스트에 등록합니다.
            _activeMinions.Add(newMinion); // <--- 이 라인 추가!
        }

        _lastChargeTime = Time.time;
        _monster.ChangeState(MonsterBase.MonsterState.Attack); // Attack 상태로 즉시 복귀!
        _chargeRoutine = null; // PerformChargeAttack 코루틴 종료 유도

        yield break; // 코루틴 즉시 종료!
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

    }
    /// <summary>
    /// MonsterCombat.OnDamageTaken 이벤트 발생 시 호출되는 핸들러입니다.
    /// (SRP: 체력 변화 감지 책임 / OCP: 분노 로직 활성화)
    /// </summary>
    /// <param name="damage">입은 데미지 양 (실제 로직에서는 사용되지 않음)</param>
    private void OnMonsterDamaged(float damage)
    {
        // 1. 이미 분노 상태라면 더 이상 검사할 필요가 없습니다. (단발성 호출 보장)
        if (_hasEnraged)
        {
            return;
        }

        // 2. 현재 체력 비율 확인
        // MonsterCombat이 현재 체력을, Monster가 최대 체력을 가지고 있으므로 둘 다 필요합니다.
        float currentHealthRatio = _monsterCombat.GetCurrentHealth() / _monster.monsterData.maxHealth;

        if (currentHealthRatio <= enrageHealthThreshold)
        {
            // 3. 분노 상태 활성화
            _hasEnraged = true; // 단발성 플래그를 참으로 설정
            ActivateEnragePhase(); // 핵심 분노 메서드 호출 (딱 한 번만 실행됨!)
        }
    }
    /// <summary>
    /// 보스가 체력 임계점에 도달했을 때 **딱 한 번** 호출되는 메서드입니다.
    /// 이 메서드 내부에 분노 상태 돌입에 따른 능력치/패턴 변경 로직을 구현합니다.
    /// (사용자 정의 로직 삽입 공간, OCP 준수)
    /// </summary>
    private void ActivateEnragePhase()
    {
        //Debug.Log("<color=red>★★★ FOREST BOSS: 분노(ENRAGE) 상태 활성화! (단발성 이벤트) ★★★</color>");
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification($"타락한 숲지기: 분노 상태 활성화!", NotificationType.Warning);
        }
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // 분노 상태 시각 효과: 붉은 색으로 변경
            renderer.material.color = Color.red;
        }
        // ====================================================================
        // [사용자 로직 삽입 공간]

        // 예시: 일반 공격 쿨타임을 1.0초로 변경 (사용자 로직)
        attackCooldown = 1.0f;
        // 예시: 특수 공격 쿨타임을 5.0초로 변경 (사용자 로직)
        chargeCooldown = 5.0f;
        chargeCastTime = 1.0f; // 시전 시간도 단축
        // 예시: 플레이어 집중 소환 비율을 1.0로 증가 (사용자 로직)
        playerFocusedRootRatio = 1.0f;
        // 예시: 몬스터 소환 개수를 증가 (사용자 로직)
        minNumberOfMinions += 2;
        maxNumberOfMinions += 5;
        attackDownStrokeDuration = 0.4f; // 내려찍기 속도 증가
        attackReturnDuration = 0.7f; // 복귀 속도 증가
        chargeCastTime = 1.0f; // 시전 시간 단축
        // ====================================================================

        // [핵심] 패턴 변화가 즉시 적용되도록 쿨타임을 현재 시간으로 업데이트합니다.
        _lastAttackTime = Time.time;
        _lastChargeTime = Time.time;
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
    // [추가] IBossInitializer 인터페이스의 SetSummonArea 메서드 구현
    /// <summary>
    /// DungeonManager로부터 뿌리 소환 영역 Collider를 주입받아 내부 필드에 저장합니다.
    /// (IBossInitializer 계약 구현 / DIP 수용)
    /// </summary>
    /// <param name="collider">DungeonManager가 씬에서 찾아 할당한 Collider 객체</param>
    public void SetSummonArea(Collider collider)
    {
        // [SOLID: DIP] 외부 의존성을 주입받아 ForestBoss의 책임을 분리합니다.
        if (collider == null)
        {
            Debug.LogError("ForestBoss: 주입받은 소환 영역 Collider가 Null입니다. 특수 공격이 비활성화됩니다.");
        }
        // 주입된 값을 안전하게 private 필드에 저장합니다.
        rootSummonAreaCollider = collider;
    }
    // ========================= 메서드 추가 (1개) =========================
    /// <summary>
    /// DungeonManager에서 DeadDungeon()이 호출되어 강제 파괴가 필요할 때 호출됩니다.
    /// OnDestroy()에서 클리어 알림을 보내지 않도록 플래그를 설정합니다.
    /// </summary>
    public void PrepareForForcedDestroy()
    {
        this._shouldNotifyDefeat = false;
    }
    // ===================================================================
    /// <summary>
    /// 몬스터가 파괴되기 직전에 호출됩니다. (보스 사망 감지 시점)
    /// 이 시점을 활용하여 DungeonManager에 보스가 처치되었음을 알립니다.
    /// </summary>
    private void OnDestroy()
    {
        // [추가] 몬스터 정리 로직 시작
        // 보스 사망 시 소환된 잔여 미니언을 모두 정리합니다.
        foreach (GameObject minion in _activeMinions)
        {
            // 몬스터가 이미 플레이어에게 처치되었을 수 있으므로 null 체크
            if (minion != null)
            {
                // [정리 로직] 해당 미니언의 GameObject를 즉시 파괴합니다.
                Destroy(minion);
            }
        }
        // 리스트도 깔끔하게 정리 (필수!)
        _activeMinions.Clear(); // <--- 이 라인 추가!
                                // [추가] 몬스터 정리 로직 끝

        // [추가] 이벤트 구독 해제 (메모리 누수 방지)
        if (_monsterCombat != null)
        {
            _monsterCombat.OnDamageTaken -= OnMonsterDamaged; // <--- 이 라인 추가!
        }
        // Charge 코루틴이 진행 중이라면 정지합니다. (안전 장치)
        if (_chargeRoutine != null)
        {
            StopCoroutine(_chargeRoutine);
        }

        if (_bossNotifier != null)
        {
            // 핵심 수정: _shouldNotifyDefeat가 true일 때만 알림을 보냅니다.
            // DeadDungeon()에 의해 PrepareForForcedDestroy()가 호출되었다면 이 조건은 false가 됩니다.
            if (_shouldNotifyDefeat)
            {
                _bossNotifier.NotifyBossDefeated(); // 기존 알림 메서드 호출
            }
            else
            {
                // 강제 파괴 시 알림을 보내지 않고 Notifier 정리만 수행
            }

            // 알림 여부와 관계없이 Notifier 참조 해제
            _bossNotifier = null;
        }
        else
        {
            Debug.LogWarning("ForestBoss: Notifier가 설정되지 않아 DungeonManager에 알림을 보낼 수 없습니다. (테스트 또는 초기화 오류)");
        }
    }
}