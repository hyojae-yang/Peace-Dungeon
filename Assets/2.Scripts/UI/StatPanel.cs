using UnityEngine;
using TMPro;

// �÷��̾��� ���Ȱ� �ɷ�ġ�� UI�� ǥ���ϴ� ��ũ��Ʈ�Դϴ�.
public class StatPanel : MonoBehaviour
{
    // === ���� ��ũ��Ʈ ===
    [Header("���� ��ũ��Ʈ")]
    [Tooltip("�÷��̾��� PlayerStats ��ũ��Ʈ�� �Ҵ��ϼ���.")]
    public PlayerStats playerStats;
    [Tooltip("�÷��̾��� PlayerStatSystem ��ũ��Ʈ�� �Ҵ��ϼ���.")]
    public PlayerStatSystem playerStatSystem;

    // === �÷��̾� �⺻ ���� �ؽ�Ʈ ===
    [Header("�÷��̾� �⺻ ����")]
    public TMP_Text characterNameText;
    public TMP_Text levelText;
    public TMP_Text statPointsText;

    // === �ɷ�ġ �ؽ�Ʈ ===
    [Header("�ɷ�ġ �ؽ�Ʈ")]
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

    // === ���� �ؽ�Ʈ ===
    [Header("���� ���� �ؽ�Ʈ")]
    public TMP_Text strengthText;
    public TMP_Text intelligenceText;
    public TMP_Text constitutionText;
    public TMP_Text agilityText;
    public TMP_Text focusText;
    public TMP_Text enduranceText;
    public TMP_Text vitalityText;

    // UI ������Ʈ ���� (���� ����ȭ�� ���� �� ������ ������Ʈ���� �ʽ��ϴ�.)
    private float updateInterval = 0.2f;
    private float lastUpdateTime;

    void Start()
    {
        if (playerStats == null || playerStatSystem == null)
        {
            Debug.LogError("PlayerStats �Ǵ� PlayerStatSystem ��ũ��Ʈ�� �Ҵ���� �ʾҽ��ϴ�. �ν����� â���� �Ҵ��� �ּ���.");
            return;
        }

        // ���� �г��� Ȱ��ȭ�� �� �ӽ� ������ �����ϴ� ����
        // �� �κ��� StatPanel�� Ȱ��ȭ�� ������ ȣ��Ǿ�� �մϴ�.
        // ���� ���, PanelManager�� TogglePanel() �Ǵ� ActivatePanel() �޼��� ������ ȣ���� �� �ֽ��ϴ�.
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
    /// ��� UI �ؽ�Ʈ�� ������Ʈ�ϴ� ���� �޼���
    /// �ӽ� ������ ������� �̸����� ���� ǥ���մϴ�.
    /// </summary>
    public void UpdateUI()
    {
        // === �⺻ ���� ������Ʈ ===
        characterNameText.text = "�̸�: " + playerStats.characterName;
        levelText.text = "����: " + playerStats.level.ToString();
        statPointsText.text = "����Ʈ: " + playerStatSystem.tempStatPoints.ToString();

        // === �ɷ�ġ �ؽ�Ʈ ������Ʈ ===
        // �ִ� ü�°� �ִ� ������ ���� ���� ���� ���ڷ� ���� �������� �и��ؼ� ǥ���մϴ�.
        healthText.text = $"�ִ� ü��: {playerStats.MaxHealth.ToString("F0")} (+{((playerStatSystem.tempConstitution - playerStatSystem.constitution) * 10f + (playerStatSystem.tempVitality - playerStatSystem.vitality) * 5f).ToString("F0")})";
        manaText.text = $"�ִ� ����: {playerStats.MaxMana.ToString("F0")} (+{((playerStatSystem.tempEndurance - playerStatSystem.endurance) * 5f).ToString("F0")})";

        // ������ �ɷ�ġ�� ���� ���� ���� ���ڷ� ���� �������� �и��ؼ� ǥ���մϴ�.
        attackPowerText.text = $"���ݷ�: {playerStats.attackPower.ToString("F1")} (+{((playerStatSystem.tempStrength - playerStatSystem.strength) * 2f).ToString("F1")})";
        magicAttackPowerText.text = $"���� ���ݷ�: {playerStats.magicAttackPower.ToString("F1")} (+{((playerStatSystem.tempIntelligence - playerStatSystem.intelligence) * 2.5f + (playerStatSystem.tempFocus - playerStatSystem.focus) * 0.5f).ToString("F1")})";
        defenseText.text = $"����: {playerStats.defense.ToString("F0")} (+{((playerStatSystem.tempConstitution - playerStatSystem.constitution) * 1f).ToString("F0")})";
        magicDefenseText.text = $"���� ����: {playerStats.magicDefense.ToString("F0")} (+{((playerStatSystem.tempEndurance - playerStatSystem.endurance) * 1f).ToString("F0")})";
        criticalChanceText.text = $"ġ��Ÿ Ȯ��: {(playerStats.criticalChance * 100).ToString("F1")}% (+{((playerStatSystem.tempFocus - playerStatSystem.focus) * 0.001f * 100).ToString("F1")}%)";
        criticalDamageMultiplierText.text = $"ġ��Ÿ ������: {(playerStats.criticalDamageMultiplier * 100).ToString("F0")}% (+{((playerStatSystem.tempStrength - playerStatSystem.strength) * 0.01f * 100).ToString("F0")}%)";
        moveSpeedText.text = $"�̵� �ӵ�: {playerStats.moveSpeed.ToString("F1")} (+{((playerStatSystem.tempAgility - playerStatSystem.agility) * 0.2f + (playerStatSystem.tempVitality - playerStatSystem.vitality) * 0.1f).ToString("F1")})";
        evasionChanceText.text = $"ȸ����: {(playerStats.evasionChance * 100).ToString("F1")}% (+{((playerStatSystem.tempAgility - playerStatSystem.agility) * 0.002f * 100).ToString("F1")}%)";

        // === ���� �ؽ�Ʈ ������Ʈ ===
        strengthText.text = "��: " + playerStatSystem.tempStrength.ToString();
        intelligenceText.text = "����: " + playerStatSystem.tempIntelligence.ToString();
        constitutionText.text = "ü��: " + playerStatSystem.tempConstitution.ToString();
        agilityText.text = "��ø: " + playerStatSystem.tempAgility.ToString();
        focusText.text = "����: " + playerStatSystem.tempFocus.ToString();
        enduranceText.text = "�γ�: " + playerStatSystem.tempEndurance.ToString();
        vitalityText.text = "Ȱ��: " + playerStatSystem.tempVitality.ToString();
    }

    /// <summary>
    /// UI���� ������ �����ϱ� ���� ȣ���ϴ� �޼��� (���� ��ư�� ����)
    /// </summary>
    public void OnClickApplyButton()
    {
        playerStatSystem.ApplyStats();
        UpdateUI(); // UI ���� ������Ʈ
    }

    /// <summary>
    /// UI���� ���� ������ ����ϱ� ���� ȣ���ϴ� �޼��� (��� ��ư�� ����)
    /// </summary>
    public void OnClickResetButton()
    {
        playerStatSystem.ResetTempStats();
        UpdateUI(); // UI �ʱ� ���·� �ǵ�����
    }
}