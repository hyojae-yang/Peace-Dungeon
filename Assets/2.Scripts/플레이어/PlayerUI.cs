using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �÷��̾��� ������ UI�� ǥ���ϴ� ��ũ��Ʈ�Դϴ�.
public class PlayerUI : MonoBehaviour
{
    // �ܺο��� �Ҵ���� PlayerStats ��ũ��Ʈ ����
    [Header("���� ��ũ��Ʈ")]
    [Tooltip("UI�� ǥ���� �÷��̾� ���� ��ũ��Ʈ�� �Ҵ��ϼ���.")]
    public PlayerStats playerStats;

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
        // �Ҵ�Ǿ����� Ȯ��
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�. �ν����� â���� �Ҵ��� �ּ���.");
            return;
        }

        // �� �����Ӹ��� UI�� ������Ʈ�մϴ�.
        UpdateUI();
    }

    private void UpdateUI()
    {
        // === �����̴� ������Ʈ ===
        healthSlider.maxValue = playerStats.MaxHealth;
        healthSlider.value = playerStats.health;

        manaSlider.maxValue = playerStats.MaxMana;
        manaSlider.value = playerStats.mana;

        expSlider.maxValue = playerStats.requiredExperience;
        expSlider.value = playerStats.experience;

        // === �ؽ�Ʈ ������Ʈ ===
        levelText.text = "Lv. " + playerStats.level.ToString();
    }
}