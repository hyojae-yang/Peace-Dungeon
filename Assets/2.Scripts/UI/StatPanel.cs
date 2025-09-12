using UnityEngine;
using TMPro;

// 플레이어의 스탯과 능력치를 UI에 표시하는 스크립트입니다.
public class StatPanel : MonoBehaviour
{
    // === 참조 스크립트 ===
    [Header("참조 스크립트")]
    [Tooltip("플레이어의 PlayerStats 스크립트를 할당하세요.")]
    public PlayerStats playerStats;
    [Tooltip("플레이어의 PlayerStatSystem 스크립트를 할당하세요.")]
    public PlayerStatSystem playerStatSystem;

    // === 플레이어 기본 정보 텍스트 ===
    [Header("플레이어 기본 정보")]
    public TMP_Text characterNameText;
    public TMP_Text levelText;
    public TMP_Text statPointsText;

    // === 능력치 텍스트 ===
    [Header("능력치 텍스트")]
    public TMP_Text healthText;
    public TMP_Text manaText;
    public TMP_Text attackPowerText;
    public TMP_Text magicAttackPowerText;
    public TMP_Text defenseText;
    public TMP_Text magicDefenseText;
    public TMP_Text criticalChanceText;
    public TMP_Text criticalDamageMultiplierText;
    public TMP_Text moveSpeedText;
    public TMP_Text evasionChanceText;

    // === 스탯 텍스트 ===
    [Header("투자 스탯 텍스트")]
    public TMP_Text strengthText;
    public TMP_Text intelligenceText;
    public TMP_Text constitutionText;
    public TMP_Text agilityText;
    public TMP_Text focusText;
    public TMP_Text enduranceText;
    public TMP_Text vitalityText;

    // UI 업데이트 간격 (성능 최적화를 위해 매 프레임 업데이트하지 않습니다.)
    private float updateInterval = 0.2f;
    private float lastUpdateTime;

    void Start()
    {
        if (playerStats == null || playerStatSystem == null)
        {
            Debug.LogError("PlayerStats 또는 PlayerStatSystem 스크립트가 할당되지 않았습니다. 인스펙터 창에서 할당해 주세요.");
            return;
        }

        // 스탯 패널이 활성화될 때 임시 스탯을 저장하는 로직
        // 이 부분은 StatPanel이 활성화될 때마다 호출되어야 합니다.
        // 예를 들어, PanelManager의 TogglePanel() 또는 ActivatePanel() 메서드 내에서 호출할 수 있습니다.
        playerStatSystem.StoreTempStats();

        UpdateUI();
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 모든 UI 텍스트를 업데이트하는 메인 메서드
    /// 임시 스탯을 기반으로 미리보기 값을 표시합니다.
    /// </summary>
    public void UpdateUI()
    {
        // === 기본 정보 업데이트 ===
        characterNameText.text = "이름: " + playerStats.characterName;
        levelText.text = "레벨: " + playerStats.level.ToString();
        statPointsText.text = "포인트: " + playerStatSystem.tempStatPoints.ToString();

        // === 능력치 텍스트 업데이트 ===
        // 최대 체력과 최대 마나를 기존 값과 스탯 투자로 인한 증가량을 분리해서 표시합니다.
        healthText.text = $"최대 체력: {playerStats.MaxHealth.ToString("F0")} (+{((playerStatSystem.tempConstitution - playerStatSystem.constitution) * 10f + (playerStatSystem.tempVitality - playerStatSystem.vitality) * 5f).ToString("F0")})";
        manaText.text = $"최대 마나: {playerStats.MaxMana.ToString("F0")} (+{((playerStatSystem.tempEndurance - playerStatSystem.endurance) * 5f).ToString("F0")})";

        // 나머지 능력치는 기존 값과 스탯 투자로 인한 증가량을 분리해서 표시합니다.
        attackPowerText.text = $"공격력: {playerStats.attackPower.ToString("F1")} (+{((playerStatSystem.tempStrength - playerStatSystem.strength) * 2f).ToString("F1")})";
        magicAttackPowerText.text = $"마법 공격력: {playerStats.magicAttackPower.ToString("F1")} (+{((playerStatSystem.tempIntelligence - playerStatSystem.intelligence) * 2.5f + (playerStatSystem.tempFocus - playerStatSystem.focus) * 0.5f).ToString("F1")})";
        defenseText.text = $"방어력: {playerStats.defense.ToString("F0")} (+{((playerStatSystem.tempConstitution - playerStatSystem.constitution) * 1f).ToString("F0")})";
        magicDefenseText.text = $"마법 방어력: {playerStats.magicDefense.ToString("F0")} (+{((playerStatSystem.tempEndurance - playerStatSystem.endurance) * 1f).ToString("F0")})";
        criticalChanceText.text = $"치명타 확률: {(playerStats.criticalChance * 100).ToString("F1")}% (+{((playerStatSystem.tempFocus - playerStatSystem.focus) * 0.001f * 100).ToString("F1")}%)";
        criticalDamageMultiplierText.text = $"치명타 데미지: {(playerStats.criticalDamageMultiplier * 100).ToString("F0")}% (+{((playerStatSystem.tempStrength - playerStatSystem.strength) * 0.01f * 100).ToString("F0")}%)";
        moveSpeedText.text = $"이동 속도: {playerStats.moveSpeed.ToString("F1")} (+{((playerStatSystem.tempAgility - playerStatSystem.agility) * 0.2f + (playerStatSystem.tempVitality - playerStatSystem.vitality) * 0.1f).ToString("F1")})";
        evasionChanceText.text = $"회피율: {(playerStats.evasionChance * 100).ToString("F1")}% (+{((playerStatSystem.tempAgility - playerStatSystem.agility) * 0.002f * 100).ToString("F1")}%)";

        // === 스탯 텍스트 업데이트 ===
        strengthText.text = "힘: " + playerStatSystem.tempStrength.ToString();
        intelligenceText.text = "지능: " + playerStatSystem.tempIntelligence.ToString();
        constitutionText.text = "체질: " + playerStatSystem.tempConstitution.ToString();
        agilityText.text = "민첩: " + playerStatSystem.tempAgility.ToString();
        focusText.text = "집중: " + playerStatSystem.tempFocus.ToString();
        enduranceText.text = "인내: " + playerStatSystem.tempEndurance.ToString();
        vitalityText.text = "활력: " + playerStatSystem.tempVitality.ToString();
    }

    /// <summary>
    /// UI에서 스탯을 적용하기 위해 호출하는 메서드 (저장 버튼에 연결)
    /// </summary>
    public void OnClickApplyButton()
    {
        playerStatSystem.ApplyStats();
        UpdateUI(); // UI 최종 업데이트
    }

    /// <summary>
    /// UI에서 스탯 변경을 취소하기 위해 호출하는 메서드 (취소 버튼에 연결)
    /// </summary>
    public void OnClickResetButton()
    {
        playerStatSystem.ResetTempStats();
        UpdateUI(); // UI 초기 상태로 되돌리기
    }
}