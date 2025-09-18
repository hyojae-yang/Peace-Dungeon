using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.WebRequestMethods;

/// <summary>
/// 플레이어의 스탯을 UI로 표시하는 스크립트입니다.
/// PlayerCharacter를 통해 PlayerStats 데이터에 접근하여 UI를 업데이트합니다.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    // === 참조 스크립트 ===
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // === 슬라이더 UI 요소 ===
    [Header("슬라이더 UI")]
    [Tooltip("체력을 표시할 Slider 컴포넌트를 할당하세요.")]
    public Slider healthSlider;
    [Tooltip("마나를 표시할 Slider 컴포넌트를 할당하세요.")]
    public Slider manaSlider;
    [Tooltip("경험치를 표시할 Slider 컴포넌트를 할당하세요.")]
    public Slider expSlider;

    // === 텍스트 UI 요소 ===
    [Header("텍스트 UI")]
    [Tooltip("레벨을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI levelText;

    private void Start()
    {
        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;

        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스가 존재하지 않습니다. 씬에 PlayerCharacter를 가진 게임 오브젝트가 있는지 확인해 주세요.");
            // UI 업데이트를 멈추기 위해 이 스크립트를 비활성화합니다.
            this.enabled = false;
        }
    }

    void Update()
    {
        // 매 프레임마다 UI를 업데이트합니다.
        UpdateUI();
    }

    /// <summary>
    /// PlayerCharacter를 통해 PlayerStats의 데이터를 기반으로 UI를 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        // PlayerCharacter 참조가 유효한지 확인합니다.
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            // 이 경우 Start()에서 이미 에러를 로그했으므로 추가적인 로그는 필요 없습니다.
            return;
        }

        // === 슬라이더 업데이트 ===
        healthSlider.maxValue = playerCharacter.playerStats.MaxHealth;
        healthSlider.value = playerCharacter.playerStats.health;

        manaSlider.maxValue = playerCharacter.playerStats.MaxMana;
        manaSlider.value = playerCharacter.playerStats.mana;

        expSlider.maxValue = playerCharacter.playerStats.requiredExperience;
        expSlider.value = playerCharacter.playerStats.experience;

        // === 텍스트 업데이트 ===
        levelText.text = "Lv. " + playerCharacter.playerStats.level.ToString();
    }
}