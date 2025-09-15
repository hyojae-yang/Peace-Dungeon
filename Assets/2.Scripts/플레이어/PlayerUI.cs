using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 플레이어의 스탯을 UI로 표시하는 스크립트입니다.
public class PlayerUI : MonoBehaviour
{
    [Header("참조 스크립트")]
    // 이 스크립트는 이제 싱글턴으로 접근하므로 인스펙터 할당이 필요 없습니다.
    // [Tooltip("UI에 표시할 플레이어 스탯 스크립트를 할당하세요.")]
    // public PlayerStats playerStats;

    // === 슬라이더 UI 요소 ===
    [Header("슬라이더 UI")]
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider expSlider;

    // === 텍스트 UI 요소 ===
    [Header("텍스트 UI")]
    public TextMeshProUGUI levelText;

    void Update()
    {
        // PlayerStats 인스턴스가 존재하는지 확인합니다.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats 인스턴스가 존재하지 않습니다. 게임 시작 시 PlayerStats를 가진 게임 오브젝트가 씬에 있는지 확인해 주세요.");
            return;
        }

        // 매 프레임마다 UI를 업데이트합니다.
        UpdateUI();
    }

    /// <summary>
    /// PlayerStats의 데이터를 기반으로 UI를 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        // PlayerStats.Instance를 통해 데이터에 접근합니다.
        // === 슬라이더 업데이트 ===
        healthSlider.maxValue = PlayerStats.Instance.MaxHealth;
        healthSlider.value = PlayerStats.Instance.health;

        manaSlider.maxValue = PlayerStats.Instance.MaxMana;
        manaSlider.value = PlayerStats.Instance.mana;

        expSlider.maxValue = PlayerStats.Instance.requiredExperience;
        expSlider.value = PlayerStats.Instance.experience;

        // === 텍스트 업데이트 ===
        levelText.text = "Lv. " + PlayerStats.Instance.level.ToString();
    }
}