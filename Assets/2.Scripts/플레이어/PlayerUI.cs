using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 플레이어의 스탯을 UI로 표시하는 스크립트입니다.
public class PlayerUI : MonoBehaviour
{
    // 외부에서 할당받을 PlayerStats 스크립트 변수
    [Header("참조 스크립트")]
    [Tooltip("UI에 표시할 플레이어 스탯 스크립트를 할당하세요.")]
    public PlayerStats playerStats;

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
        // 할당되었는지 확인
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트가 할당되지 않았습니다. 인스펙터 창에서 할당해 주세요.");
            return;
        }

        // 매 프레임마다 UI를 업데이트합니다.
        UpdateUI();
    }

    private void UpdateUI()
    {
        // === 슬라이더 업데이트 ===
        healthSlider.maxValue = playerStats.MaxHealth;
        healthSlider.value = playerStats.health;

        manaSlider.maxValue = playerStats.MaxMana;
        manaSlider.value = playerStats.mana;

        expSlider.maxValue = playerStats.requiredExperience;
        expSlider.value = playerStats.experience;

        // === 텍스트 업데이트 ===
        levelText.text = "Lv. " + playerStats.level.ToString();
    }
}