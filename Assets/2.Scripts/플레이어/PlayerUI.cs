using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.WebRequestMethods;

/// <summary>
/// �÷��̾��� ������ UI�� ǥ���ϴ� ��ũ��Ʈ�Դϴ�.
/// PlayerCharacter�� ���� PlayerStats �����Ϳ� �����Ͽ� UI�� ������Ʈ�մϴ�.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    // === ���� ��ũ��Ʈ ===
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    // === �����̴� UI ��� ===
    [Header("�����̴� UI")]
    [Tooltip("ü���� ǥ���� Slider ������Ʈ�� �Ҵ��ϼ���.")]
    public Slider healthSlider;
    [Tooltip("������ ǥ���� Slider ������Ʈ�� �Ҵ��ϼ���.")]
    public Slider manaSlider;
    [Tooltip("����ġ�� ǥ���� Slider ������Ʈ�� �Ҵ��ϼ���.")]
    public Slider expSlider;

    // === �ؽ�Ʈ UI ��� ===
    [Header("�ؽ�Ʈ UI")]
    [Tooltip("������ ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI levelText;

    private void Start()
    {
        // PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;

        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� �������� �ʽ��ϴ�. ���� PlayerCharacter�� ���� ���� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            // UI ������Ʈ�� ���߱� ���� �� ��ũ��Ʈ�� ��Ȱ��ȭ�մϴ�.
            this.enabled = false;
        }
    }

    void Update()
    {
        // �� �����Ӹ��� UI�� ������Ʈ�մϴ�.
        UpdateUI();
    }

    /// <summary>
    /// PlayerCharacter�� ���� PlayerStats�� �����͸� ������� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateUI()
    {
        // PlayerCharacter ������ ��ȿ���� Ȯ���մϴ�.
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            // �� ��� Start()���� �̹� ������ �α������Ƿ� �߰����� �α״� �ʿ� �����ϴ�.
            return;
        }

        // === �����̴� ������Ʈ ===
        healthSlider.maxValue = playerCharacter.playerStats.MaxHealth;
        healthSlider.value = playerCharacter.playerStats.health;

        manaSlider.maxValue = playerCharacter.playerStats.MaxMana;
        manaSlider.value = playerCharacter.playerStats.mana;

        expSlider.maxValue = playerCharacter.playerStats.requiredExperience;
        expSlider.value = playerCharacter.playerStats.experience;

        // === �ؽ�Ʈ ������Ʈ ===
        levelText.text = "Lv. " + playerCharacter.playerStats.level.ToString();
    }
}