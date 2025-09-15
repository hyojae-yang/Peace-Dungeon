using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �÷��̾��� ������ UI�� ǥ���ϴ� ��ũ��Ʈ�Դϴ�.
public class PlayerUI : MonoBehaviour
{
    [Header("���� ��ũ��Ʈ")]
    // �� ��ũ��Ʈ�� ���� �̱������� �����ϹǷ� �ν����� �Ҵ��� �ʿ� �����ϴ�.
    // [Tooltip("UI�� ǥ���� �÷��̾� ���� ��ũ��Ʈ�� �Ҵ��ϼ���.")]
    // public PlayerStats playerStats;

    // === �����̴� UI ��� ===
    [Header("�����̴� UI")]
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider expSlider;

    // === �ؽ�Ʈ UI ��� ===
    [Header("�ؽ�Ʈ UI")]
    public TextMeshProUGUI levelText;

    void Update()
    {
        // PlayerStats �ν��Ͻ��� �����ϴ��� Ȯ���մϴ�.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� �������� �ʽ��ϴ�. ���� ���� �� PlayerStats�� ���� ���� ������Ʈ�� ���� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // �� �����Ӹ��� UI�� ������Ʈ�մϴ�.
        UpdateUI();
    }

    /// <summary>
    /// PlayerStats�� �����͸� ������� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateUI()
    {
        // PlayerStats.Instance�� ���� �����Ϳ� �����մϴ�.
        // === �����̴� ������Ʈ ===
        healthSlider.maxValue = PlayerStats.Instance.MaxHealth;
        healthSlider.value = PlayerStats.Instance.health;

        manaSlider.maxValue = PlayerStats.Instance.MaxMana;
        manaSlider.value = PlayerStats.Instance.mana;

        expSlider.maxValue = PlayerStats.Instance.requiredExperience;
        expSlider.value = PlayerStats.Instance.experience;

        // === �ؽ�Ʈ ������Ʈ ===
        levelText.text = "Lv. " + PlayerStats.Instance.level.ToString();
    }
}