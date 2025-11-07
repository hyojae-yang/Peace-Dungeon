using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 던전 위험도 레벨, 게이지 및 시간 기반 상승을 관리하는 싱글톤 매니저입니다.
/// 주요 책임: 1. 던전 입장 횟수 누적 (가속 인자). 2. 시간 기반 게이지 상승 및 이중 팩터(가속/감속) 적용. 3. 위험도 상태 저장/로드. (SRP 준수)
/// </summary>
public class DungeonRiskManager : MonoBehaviour, IDungeonRiskSystem, ISavable
{
    // 싱글톤 인스턴스
    public static DungeonRiskManager Instance { get; private set; }

    // =======================================================
    // [팩트 1: 레벨/게이지 관련 상수 정의]
    // =======================================================
    /// <summary>
    /// 위험도 레벨을 1 올리는 데 필요한 누적 입장 횟수입니다. (가정: 5회당 Lv 1)
    /// </summary>
    private const int EXPLORATION_PER_LEVEL = 5;

    /// <summary>
    /// UI 게이지가 목표 게이지 값을 따라가는 속도 (부드러움 조절).
    /// </summary>
    private const float GAUGE_SMOOTH_SPEED = 2.0f;

    /// <summary>
    /// 게이지 100%를 채우는 데 필요한 **가장 낮은 레벨의 기본 시간**(초)입니다.
    /// </summary>
    private const float BASE_TIME_TO_FILL_GAUGE = 1200.0f;

    /// <summary>
    /// 레벨 1 증가 시 **Max Time**이 추가로 늘어나는 시간(초)입니다. (레벨 기반 **감속** 인자)
    /// </summary>
    private const float TIME_INCREASE_PER_LEVEL = 300.0f;

    /// <summary>
    /// 던전 입장 횟수 1회당 게이지 상승 속도에 추가되는 가속 계수입니다 (0.02f = 2% 가속).
    /// </summary>
    private const float EXPLORATION_ACCEL_FACTOR = 0.02f;

    /// <summary>
    /// [신규 상수] 플레이어가 점유한 타일 1개당 게이지 상승 속도에 추가되는 가속 계수입니다 (0.01f = 1% 가속).
    /// 이 값은 DungeonMap에서 가져온 Occupied Tile Count 팩트 기반 **가속** 인자로 사용됩니다.
    /// </summary>
    private const float TILE_OCCUPY_ACCEL_FACTOR = 0.01f; // 타일 기반 가속 계수 추가

    [Header("위험도 상태 (팩트)")]
    [Tooltip("플레이어가 던전에 진입한 누적 횟수입니다. 이 횟수는 레벨 계산 및 게이지 가속에 사용됩니다.")]
    [SerializeField]
    private int totalExplorationCount = 0;

    /// <summary>
    /// 현재 위험도 레벨입니다. (계산된 값, 난이도 감속 인자로 사용됨)
    /// </summary>
    private int currentRiskLevel = 0;

    /// <summary>
    /// 현재 던전 탐험 중인지 나타내는 플래그입니다. (게이지 상승 모드 활성화 여부)
    /// </summary>
    private bool isExploring = false;

    /// <summary>
    /// 현재 레벨에서 누적된 게이지 진행도 값입니다. (0.0f ~ 1.0f)
    /// 시간에 따라 증가하는 실질적인 게이지 값입니다.
    /// </summary>
    private float currentLevelProgress = 0f;

    /// <summary>
    /// UI에 표시되는 시각적 게이지 비율 (Lerp 애니메이션 값)
    /// </summary>
    private float currentVisualGaugeRatio = 0f;
    // =======================================================


    private void Awake()
    {
        // 싱글톤 패턴 초기화
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 세이브 시스템에 자신을 등록 (데이터 로드/저장을 위함)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }

        // 로드 완료 후 초기 상태를 계산하여 UI에 반영합니다.
        CalculateAndRefreshState(false);
    }

    /// <summary>
    /// [핵심 기능] 매 프레임마다 게이지를 시간에 따라 업데이트하고 UI에 반영합니다.
    /// 레벨 기반 감속과 두 가지 가속(입장 횟수, 점유 타일 수)이 이 로직에서 통합됩니다.
    /// </summary>
    private void Update()
    {
        // 1. 던전 탐험 중일 때만 게이지를 시간에 따라 증가시킵니다.
        if (isExploring)
        {
            // --- [핵심 로직: 시간 기반 게이지 상승 및 난이도 이중 팩터 적용] ---

            // a. 현재 레벨에 필요한 Max Time 계산 (레벨이 오를수록 필요 시간이 늘어나 난이도가 **감속**됩니다.)
            float requiredTimeForCurrentLevel = CalculateRequiredMaxTime(currentRiskLevel);

            // ⭐️ [신규] 플레이어가 점유한 타일 갯수를 DungeonMap에서 가져옵니다.
            int occupiedTileCount = 0;
            if (DungeonMap.Instance != null)
            {
                occupiedTileCount = DungeonMap.Instance.GetPlayerOccupiedTileCount();
            }

            // 1. 입장 횟수 기반 가속 계수
            float explorationAccel = totalExplorationCount * EXPLORATION_ACCEL_FACTOR;

            // 2. 타일 점유 기반 가속 계수
            float tileAccel = occupiedTileCount * TILE_OCCUPY_ACCEL_FACTOR;

            // 3. 최종 가속 계수 (기본 1.0f + 두 가지 가속 인자 합산)
            // 입장 횟수와 점유 타일 수가 늘어날수록 finalAccelerationMultiplier가 증가하여 게이지가 가속됩니다.
            float finalAccelerationMultiplier = 1.0f + explorationAccel + tileAccel;

            // b. 최종 초당 게이지 증가량 계산: (기본 증가량) * (최종 가속 계수)
            // (1.0f / requiredTimeForCurrentLevel) : 레벨이 높으면 느려짐 (감속)
            // * finalAccelerationMultiplier : 입장 횟수/타일 수가 많으면 빨라짐 (가속)
            float gaugeIncreasePerSecond = (1.0f / requiredTimeForCurrentLevel) * finalAccelerationMultiplier;

            // c. 현재 레벨 진행도 업데이트
            currentLevelProgress += gaugeIncreasePerSecond * Time.deltaTime;

            // d. 레벨 업 체크 (1.0f 초과 여부 확인)
            while (currentLevelProgress >= 1.0f)
            {
                // 레벨업이 발생했으므로 팩트(totalExplorationCount)를 EXPLORATION_PER_LEVEL 만큼 증가시켜 레벨을 올립니다.
                totalExplorationCount += EXPLORATION_PER_LEVEL;

                // 초과된 게이지는 다음 레벨로 이월됩니다.
                currentLevelProgress -= 1.0f;

                // 레벨업을 트리거했음을 알리기 위해 true를 전달
                CalculateAndRefreshState(true);

                //Debug.Log($" 위험도 레벨 업! 현재 레벨: {currentRiskLevel}. 게이지 초기화 후 이월. (필요 시간: {requiredTimeForCurrentLevel:F1}초, 최종 가속: {finalAccelerationMultiplier:F2}배)");
            }
        }

        // 2. UI 시각적 업데이트 (부드러운 애니메이션)
        // 시각 게이지를 현재 목표 진행도(currentLevelProgress)까지 부드럽게 따라가도록 Lerp합니다.
        currentVisualGaugeRatio = Mathf.Lerp(
            currentVisualGaugeRatio,
            currentLevelProgress,
            Time.deltaTime * GAUGE_SMOOTH_SPEED
        );

        // UI 매니저에 현재 상태 전달
        if (DungeonUIManager.Instance != null)
        {
            // UpdateRiskDisplay가 매 프레임 호출되므로 게이지가 실시간으로 움직입니다.
            DungeonUIManager.Instance.UpdateRiskDisplay(currentRiskLevel, currentVisualGaugeRatio);
        }
    }

    // =======================================================
    // 상태 계산 및 갱신 로직 (Helper)
    // =======================================================

    /// <summary>
    /// 현재 위험도 레벨에 도달하는 데 필요한 총 시간(초)을 계산합니다.
    /// 레벨이 오를수록 필요한 시간이 늘어나 난이도가 증가하여 게이지 상승 속도가 감속됩니다. (감속 팩터)
    /// </summary>
    /// <param name="level">계산할 위험도 레벨</param>
    /// <returns>레벨업에 필요한 총 시간(초)</returns>
    private float CalculateRequiredMaxTime(int level)
    {
        // 레벨 0일 때 BASE_TIME_TO_FILL_GAUGE를 반환하고, 레벨 1 증가 시 TIME_INCREASE_PER_LEVEL 만큼 시간이 늘어납니다.
        float requiredTime = BASE_TIME_TO_FILL_GAUGE + (level * TIME_INCREASE_PER_LEVEL);

        // 최소 1초는 확보 (혹시 모를 나눗셈 오류 방지)
        return Mathf.Max(1.0f, requiredTime);
    }


    /// <summary>
    /// 현재 누적된 입장 횟수를 기반으로 위험도 레벨을 계산하고 상태를 업데이트합니다.
    /// </summary>
    /// <param name="triggerLevelUp">레벨업 이벤트에 의해 호출되었는지 여부</param>
    private void CalculateAndRefreshState(bool triggerLevelUp)
    {
        // 1. 현재 위험도 레벨 계산 (totalExplorationCount 팩트 기반)
        // totalExplorationCount가 5가 되면 Lv 1, 10이 되면 Lv 2
        currentRiskLevel = totalExplorationCount / EXPLORATION_PER_LEVEL;

        // 2. UI 시각적 게이지는 현재 진행도(currentLevelProgress)로 즉시 초기화
        // currentLevelProgress는 Update()에서 시간에 의해 누적된 실제 게이지 값입니다.
        currentVisualGaugeRatio = currentLevelProgress;

        // Debug.Log($"[RiskManager:Refresh] Lv: {currentRiskLevel}, 게이지 시작 비율: {currentLevelProgress:F2}, IsExploring: {isExploring}, TriggeredByLevelUp: {triggerLevelUp}");
    }

    // =======================================================
    // IDungeonRiskSystem 구현: 입장 횟수 증가 및 상승 모드 활성화
    // =======================================================

    /// <summary>
    /// [IDungeonRiskSystem 인터페이스 구현]
    /// 던전 입장 횟수를 1 증가시키고, **시간 기반 게이지 상승 모드를 활성화**합니다.
    /// </summary>
    public void IncreaseExplorationCount(List<DungeonSpawnManager.MonsterSpawnData> activeDungeonSpawnData)
    {
        // 팩트 1: 던전 입장 횟수 증가 (가속 인자로 사용)
        totalExplorationCount++;

        // 팩트 2: 게이지 상승 모드 활성화
        isExploring = true;

        // 3. 증가된 횟수를 기반으로 레벨을 갱신하고 게이지 초기값을 설정합니다.
        // 이 때 CalculateAndRefreshState는 totalExplorationCount에 기반해 레벨을 계산합니다.
        CalculateAndRefreshState(false);

        //Debug.Log($" 던전 입장 완료! [Lv: {currentRiskLevel}] 시간 기반 게이지 상승 시작. (총 입장 횟수: {totalExplorationCount})");

    }
    /// <summary>
    /// 현재 던전의 계산된 위험도 레벨을 반환합니다.
    /// 이 레벨은 몬스터 스탯, 스폰 수 등 난이도 보정에 사용됩니다.
    /// </summary>
    /// <returns>현재 위험도 레벨 (int)</returns>
    public int GetCurrentRiskLevel()
    {
        // 팩트에 기반한 계산된 레벨을 반환합니다.
        return currentRiskLevel;
    }

    // =======================================================
    // [추가] 던전 퇴장 시 게이지 상승 모드를 종료하기 위한 단일 기능
    // =======================================================

    /// <summary>
    /// 던전 탐험이 종료되었을 때 호출되어 게이지 상승을 멈춥니다.
    /// </summary>
    public void StopExploration()
    {
        isExploring = false;
        //Debug.Log(" 던전 퇴장! 시간 기반 게이지 상승 종료.");
    }
    /// <summary>
    /// 위험도 시스템을 강제로 **레벨 0, 게이지 0**으로 초기화합니다.
    /// 게임 내 특정 이벤트(예: 난이도 초기화 아이템 사용 또는 게임 오버) 시 외부에서 호출됩니다.
    /// </summary>
    public void ResetRiskSystem()
    {
        // 1. 모든 팩트 및 상태 변수를 기본값(0 또는 false)으로 초기화합니다.
        this.totalExplorationCount = 0;      // 누적 입장 횟수 팩트 초기화
        this.currentRiskLevel = 0;           // 계산된 위험도 레벨 초기화
        this.isExploring = false;            // 탐험 중 플래그 해제
        this.currentLevelProgress = 0f;      // 시간 기반 게이지 진행도 초기화
        this.currentVisualGaugeRatio = 0f;   // 시각적 게이지 비율 초기화

        // 2. 초기화된 상태를 UI에 즉시 반영합니다.
        CalculateAndRefreshState(false);
    }

    // =======================================================
    // ISavable 인터페이스 구현 (저장/로드 활성화)
    // =======================================================

    /// <summary>
    /// 현재 DungeonRiskManager의 저장 가능한 상태를 반환합니다.
    /// </summary>
    public object SaveData()
    {
        // totalExplorationCount와 currentLevelProgress를 모두 저장합니다.
        DungeonRiskManagerSaveData data = new DungeonRiskManagerSaveData
        {
            totalExplorationCount = this.totalExplorationCount,
            currentLevelProgress = this.currentLevelProgress
        };
        return data;
    }

    /// <summary>
    /// 로드된 저장 데이터를 DungeonRiskManager의 상태에 적용합니다.
    /// </summary>
    public void LoadData(object data)
    {
        if (data is DungeonRiskManagerSaveData loadedData)
        {
            this.totalExplorationCount = loadedData.totalExplorationCount;
            this.currentLevelProgress = loadedData.currentLevelProgress;

            // 로드 완료 직후, UI를 즉시 업데이트하도록 상태를 계산합니다.
            CalculateAndRefreshState(false);
        }
    }
}