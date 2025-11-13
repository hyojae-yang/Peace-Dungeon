using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // 조명을 부드럽게 전환하기 위해 DOTween 라이브러리 사용을 가정합니다.

/// <summary>
/// 위험도 레벨에 따른 환경 조명(Directional Light)과 안개(Fog) 설정을 정의하는 구조체입니다.
/// 이 구조체의 배열이 인스펙터에 '설정 테이블' 형태로 표시됩니다.
/// </summary>
[System.Serializable]
public struct RiskLevelSetting
{
    [Header("주광원 설정")]
    [Tooltip("씬 전체를 비추는 주광원의 색상입니다. (예: 평화로울 땐 푸른색, 위험할 땐 붉은색)")]
    public Color directionalLightColor;

    [Tooltip("주광원의 밝기입니다. 값이 낮을수록 어두워집니다.")]
    [Range(0.1f, 2.0f)]
    public float directionalLightIntensity;

    [Header("안개 설정")]
    [Tooltip("안개의 색상입니다. 안개 색상을 변경하면 분위기가 크게 달라집니다.")]
    public Color fogColor;

    [Tooltip("안개의 밀도입니다. 값이 높을수록 안개가 짙어지며 시야가 좁아집니다.")]
    [Range(0.0f, 0.2f)]
    public float fogDensity;

    [Header("전환 속도")]
    [Tooltip("이전 레벨에서 현재 레벨 설정으로 전환되는 데 걸리는 시간(초)입니다. (DOTween 사용)")]
    [Range(0.5f, 5.0f)]
    public float transitionDuration;
}

/// <summary>
/// DungeonRiskManager의 위험도 레벨에 따라 환경 조명 및 안개 설정을 동적으로 관리합니다.
/// 주요 책임: 1. 위험도 레벨 변화 감지. 2. 설정 테이블 기반 조명/안개 값 부드러운 전환. 
/// 3. 던전 진입/퇴장 상태에 따라 동적 조명 시스템 활성화/비활성화 및 씬 기본 조명 복구. (SRP 준수)
/// </summary>
public class DynamicLightingManager : MonoBehaviour
{
    // DungeonRiskManager의 상수에 접근하기 위해 임시 상수를 정의합니다.
    private const int RISK_CYCLE_LEVEL = 5;

    // 안개 전환 DOTween 애니메이션에 부여할 고유 ID (상수로 정의)
    private const string FOG_COLOR_TWEEN_ID = "FogColorTween";
    private const string FOG_DENSITY_TWEEN_ID = "FogDensityTween";

    // =======================================================
    // [추가] 던전 진입 전의 원래 씬 조명 상태를 저장할 필드
    // =======================================================
    private Color defaultLightColor;
    private float defaultLightIntensity;
    private Color defaultFogColor;
    private float defaultFogDensity;
    // =======================================================

    [Header("연결된 컴포넌트")]
    [Tooltip("씬에 있는 주광원(Directional Light) 컴포넌트를 직접 연결해 주세요.")]
    public Light directionalLight;

    [Header("위험도별 조명 설정 (인덱스 0부터 순서대로)")]
    [Tooltip("총 6단계 설정을 입력합니다. 인덱스 0: Lv 0 전용. 인덱스 1~5: Lv 5, 10, 15, 20, 25+에 적용됩니다.")]
    public List<RiskLevelSetting> lightingSettings;

    // 마지막으로 처리된 조명 설정 인덱스를 추적하여 중복 전환을 방지합니다.
    private int lastProcessedRiskLevel = -1;

    // 이 상태가 true일 때만 Update에서 위험도 기반 조명 전환 로직이 실행됩니다.
    private bool isDungeonLightingActive = false;

    private void Start()
    {
        if (!DOTween.instance)
        {
            DOTween.Init();
        }

        // Directional Light 연결 확인 로직 (기존 유지)
        if (directionalLight == null)
        {
            Debug.LogError("Directional Light가 DynamicLightingManager에 연결되지 않았습니다...");
            if (RenderSettings.sun != null)
            {
                directionalLight = RenderSettings.sun;
                Debug.LogWarning("Directional Light가 자동으로 RenderSettings.sun으로 설정되었습니다.");
            }
            else
            {
                this.enabled = false;
                return;
            }
        }

        // =======================================================
        // [수정] DungeonManager 이벤트 구독만 수행합니다.
        // Start() 시점에 ApplyImmediateLightingState(0) 호출을 제거하여 씬 조명을 건드리지 않습니다.
        // =======================================================
        if (DungeonManager.Instance != null)
        {
            DungeonManager.OnDungeonEnter += HandleDungeonEnter;
            DungeonManager.OnDungeonExit += HandleDungeonExit;
        }
        else
        {
            Debug.LogWarning("[DynamicLightingManager] DungeonManager 인스턴스를 찾을 수 없습니다. 던전 진입/퇴장 감지 기능이 비활성화됩니다.");
        }
        // =======================================================
    }

    /// <summary>
    /// 스크립트가 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.OnDungeonEnter -= HandleDungeonEnter;
            DungeonManager.OnDungeonExit -= HandleDungeonExit;
        }
    }

    // =======================================================
    // 던전 진입/퇴장 이벤트 핸들러
    // =======================================================

    /// <summary>
    /// DungeonManager.OnDungeonEnter 이벤트가 호출될 때 실행됩니다.
    /// 1. 현재 씬의 기본 조명 상태를 저장합니다.
    /// 2. 동적 조명 시스템을 활성화하고 현재 위험도에 맞는 조명을 즉시 적용합니다.
    /// </summary>
    private void HandleDungeonEnter()
    {
        // [핵심 추가] 던전 진입 직전의 씬 환경 설정을 저장합니다. (마을 조명 복구를 위해)
        SaveDefaultLightingState();

        this.isDungeonLightingActive = true; // 동적 조명 활성화

        // 던전에 진입하자마자 현재 위험도에 맞는 조명을 부드러운 전환 없이 즉시 적용합니다.
        ApplyInitialLightingState();
    }

    /// <summary>
    /// DungeonManager.OnDungeonExit 이벤트가 호출될 때 실행됩니다.
    /// 1. 동적 조명 시스템을 비활성화합니다.
    /// 2. 저장해 둔 원래 씬 조명 상태로 즉시 복구합니다.
    /// </summary>
    private void HandleDungeonExit()
    {
        this.isDungeonLightingActive = false; // 동적 조명 비활성화

        // 기존에 진행 중이던 모든 DOTween 애니메이션을 즉시 종료합니다.
        DOTween.Kill(directionalLight);
        DOTween.Kill(FOG_COLOR_TWEEN_ID);
        DOTween.Kill(FOG_DENSITY_TWEEN_ID);

        // [핵심 수정] Index 0 대신, 던전 진입 전에 저장해 둔 원래 씬 조명 상태로 복구합니다.
        RestoreDefaultLightingState();
    }

    // =======================================================
    // 상태 저장 및 복구 메서드 (새로 추가)
    // =======================================================

    /// <summary>
    /// 현재 씬의 Directional Light 및 Render Settings (안개) 값을 저장합니다.
    /// </summary>
    private void SaveDefaultLightingState()
    {
        if (directionalLight != null)
        {
            defaultLightColor = directionalLight.color;
            defaultLightIntensity = directionalLight.intensity;
        }

        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
    }

    /// <summary>
    /// 저장된 값으로 Directional Light 및 Render Settings (안개)를 즉시 복구합니다.
    /// </summary>
    private void RestoreDefaultLightingState()
    {
        if (directionalLight != null)
        {
            directionalLight.color = defaultLightColor;
            directionalLight.intensity = defaultLightIntensity;
        }

        RenderSettings.fogColor = defaultFogColor;
        RenderSettings.fogDensity = defaultFogDensity;

        // 조명 복구 후 lastProcessedRiskLevel을 리셋하여 다음 던전 진입 시 0단계부터 다시 적용되도록 합니다.
        lastProcessedRiskLevel = -1;
    }

    // =======================================================
    // 기존 조명 전환 로직 (Update, ChangeLighting, ApplyInitialLightingState, ApplyImmediateLightingState)
    // - 던전 내에서만 작동하도록 isDungeonLightingActive 체크 로직 유지
    // - ApplyImmediateLightingState는 내부적으로만 사용됩니다.
    // =======================================================

    /// <summary>
    /// 매 프레임마다 DungeonRiskManager의 상태를 확인하여 조명 전환이 필요한지 확인합니다.
    /// **isDungeonLightingActive가 true일 때만** 실행됩니다.
    /// </summary>
    private void Update()
    {
        // 던전 조명 활성화 상태가 아니거나 필수 인스턴스/설정이 없으면 로직을 실행하지 않습니다.
        if (!isDungeonLightingActive || DungeonRiskManager.Instance == null || lightingSettings == null || lightingSettings.Count == 0)
        {
            return;
        }

        // 1. 현재 던전 위험도 레벨을 DungeonRiskManager로부터 가져옵니다.
        int currentRiskLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();

        // 2. 위험도 레벨에 따른 설정 인덱스를 계산합니다.
        int targetStageIndex;
        if (currentRiskLevel == 0)
        {
            // 위험도 0은 인덱스 0 사용
            targetStageIndex = 0;
        }
        else
        {
            // DungeonRiskManager의 상수에 접근하여 주기적으로 인덱스 증가
            int cycleLevel = (DungeonRiskManager.PITCH_CHANGE_CYCLE_LEVEL > 0) ? DungeonRiskManager.PITCH_CHANGE_CYCLE_LEVEL : RISK_CYCLE_LEVEL;
            targetStageIndex = (currentRiskLevel / cycleLevel) + 1;
        }

        // 조명 변화의 최대 단계(lightingSettings.Count - 1)를 초과하지 않도록 클램프합니다.
        int clampedLevel = Mathf.Min(targetStageIndex, lightingSettings.Count - 1);

        // 이전 레벨과 현재 클램프된 레벨이 다를 때만 조명 전환을 시작합니다.
        if (clampedLevel != lastProcessedRiskLevel)
        {
            ChangeLighting(clampedLevel);
            lastProcessedRiskLevel = clampedLevel;
        }
    }

    /// <summary>
    /// 현재 위험도 레벨에 맞는 설정 데이터를 찾고 DOTween을 사용하여 조명과 안개를 부드럽게 전환합니다.
    /// </summary>
    /// <param name="targetLevelIndex">적용할 조명 설정 테이블의 인덱스</param>
    private void ChangeLighting(int targetLevelIndex)
    {
        if (targetLevelIndex < 0 || targetLevelIndex >= lightingSettings.Count)
        {
            Debug.LogError($"조명 설정 리스트 인덱스 ({targetLevelIndex})가 범위를 벗어났습니다.");
            return;
        }

        RiskLevelSetting targetSetting = lightingSettings[targetLevelIndex];

        // --- 1. Directional Light (주광원) 전환 로직 ---
        if (directionalLight != null)
        {
            DOTween.Kill(directionalLight);
            directionalLight.DOColor(targetSetting.directionalLightColor, targetSetting.transitionDuration)
                .SetId(directionalLight);
            directionalLight.DOIntensity(targetSetting.directionalLightIntensity, targetSetting.transitionDuration)
                .SetId(directionalLight);
        }

        // --- 2. Render Settings (안개) 전환 로직 ---
        DOTween.Kill(FOG_COLOR_TWEEN_ID);
        DOTween.Kill(FOG_DENSITY_TWEEN_ID);

        DOTween.To(() => RenderSettings.fogColor,
                   x => RenderSettings.fogColor = x,
                   targetSetting.fogColor,
                   targetSetting.transitionDuration)
                   .SetId(FOG_COLOR_TWEEN_ID);

        DOTween.To(() => RenderSettings.fogDensity,
                   x => RenderSettings.fogDensity = x,
                   targetSetting.fogDensity,
                   targetSetting.transitionDuration)
                   .SetId(FOG_DENSITY_TWEEN_ID);
    }

    /// <summary>
    /// 던전 진입 시 (HandleDungeonEnter) 호출되어 현재 위험도에 맞는 조명 상태를 즉시 설정합니다.
    /// </summary>
    private void ApplyInitialLightingState()
    {
        if (DungeonRiskManager.Instance == null || lightingSettings.Count == 0) return;

        int currentRiskLevel = DungeonRiskManager.Instance.GetCurrentRiskLevel();
        int targetStageIndex;

        // 조명 설정 인덱스 계산 (기존 로직 유지)
        if (currentRiskLevel == 0)
        {
            targetStageIndex = 0;
        }
        else
        {
            int cycleLevel = (DungeonRiskManager.PITCH_CHANGE_CYCLE_LEVEL > 0) ? DungeonRiskManager.PITCH_CHANGE_CYCLE_LEVEL : RISK_CYCLE_LEVEL;
            targetStageIndex = (currentRiskLevel / cycleLevel) + 1;
        }

        int clampedLevel = Mathf.Min(targetStageIndex, lightingSettings.Count - 1);

        // 즉시 적용 메서드 호출
        ApplyImmediateLightingState(clampedLevel);

        lastProcessedRiskLevel = clampedLevel;
    }

    /// <summary>
    /// 지정된 인덱스의 조명 설정을 부드러운 전환 없이 즉시 적용합니다.
    /// (던전 진입 시 초기 설정에 사용)
    /// </summary>
    /// <param name="targetLevelIndex">적용할 조명 설정 테이블의 인덱스</param>
    private void ApplyImmediateLightingState(int targetLevelIndex)
    {
        if (lightingSettings.Count == 0 || targetLevelIndex < 0 || targetLevelIndex >= lightingSettings.Count)
        {
            return;
        }

        RiskLevelSetting targetSetting = lightingSettings[targetLevelIndex];

        // --- 1. Directional Light (주광원) 즉시 적용 ---
        if (directionalLight != null)
        {
            directionalLight.color = targetSetting.directionalLightColor;
            directionalLight.intensity = targetSetting.directionalLightIntensity;
        }

        // --- 2. Render Settings (안개) 즉시 적용 ---
        RenderSettings.fogColor = targetSetting.fogColor;
        RenderSettings.fogDensity = targetSetting.fogDensity;
    }
}