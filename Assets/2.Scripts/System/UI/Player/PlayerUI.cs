using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어의 스탯을 UI로 표시하는 스크립트입니다.
/// PlayerCharacter를 통해 PlayerStats 데이터에 접근하여 UI를 업데이트합니다.
/// **[SOLID 원칙 개선]** 슬라이더를 비율 기반(0~1)으로 업데이트하여 시각화 로직을 통일합니다.
/// **[경험치 개선]** PlayerStats의 experience와 requiredExperience가 long 타입임을 가정하고 수정되었습니다.
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
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI HpText;
    public TextMeshProUGUI MpText;
    public TextMeshProUGUI ExpText;

    private void Start()
    {
        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;

        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스가 존재하지 않습니다. 씬에 PlayerCharacter를 가진 게임 오브젝트가 있는지 확인해 주세요.");
            // UI 업데이트를 멈추기 위해 이 스크립트를 비활성화합니다.
            this.enabled = false;
            return;
        }

        // 모든 슬라이더의 최대값을 1.0f로 통일합니다.
        if (healthSlider)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
        }
        if (manaSlider)
        {
            manaSlider.minValue = 0f;
            manaSlider.maxValue = 1f;
        }
        if (expSlider)
        {
            expSlider.minValue = 0f;
            expSlider.maxValue = 1f;
        }

        // **[개선]** Start에서 초기 UI 상태를 한 번 업데이트합니다. (이벤트 시스템이 없을 경우)
        UpdateUI();
    }

    void Update()
    {
        // 매 프레임마다 UI를 업데이트합니다.
        // **[참고]** 성능 최적화를 위해 실시간 데이터 변경 이벤트(OnExperienceChanged 등)를 구독하여 
        // 필요한 시점에만 UI를 업데이트하는 것을 권장합니다.
        UpdateUI();
    }

    /// <summary>
    /// PlayerCharacter를 통해 PlayerStats의 데이터를 기반으로 UI를 업데이트합니다.
    /// 슬라이더를 비율 기반(0~1)으로 업데이트하며, 경험치 long 타입을 안전하게 처리합니다.
    /// </summary>
    private void UpdateUI()
    {
        // PlayerCharacter 참조가 유효한지 확인합니다.
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            return;
        }

        // PlayerStats의 데이터를 더 자주 참조하여 코드의 가독성을 높입니다.
        var stats = playerCharacter.playerStats;

        // === 슬라이더 업데이트 (비율 기반) ===
        // 현재값 / 최대값 비율을 계산하여 슬라이더 value에 대입합니다.

        // 체력 슬라이더: (현재 체력 / 최대 체력) - float
        if (healthSlider)
        {
            healthSlider.value = stats.MaxHealth > 0 ? stats.health / stats.MaxHealth : 0f;
        }

        // 마나 슬라이더: (현재 마나 / 최대 마나) - float
        if (manaSlider)
        {
            manaSlider.value = stats.MaxMana > 0 ? stats.mana / stats.MaxMana : 0f;
        }

        // 경험치 슬라이더: (현재 경험치 / 필요 경험치) - long을 float으로 변환하여 정밀한 비율 계산
        if (expSlider)
        {
            // requiredExperience가 0이 아니며, long을 float으로 명시적 형변환하여 나눗셈을 수행합니다.
            if (stats.requiredExperience > 0)
            {
                // long을 double로 변환 후 float으로 변환하는 것이 안전하지만, Unity 환경에서는 float으로 바로 변환해도 
                // 수십억 단위까지는 충분한 정밀도를 제공합니다.
                float currentExp = (float)stats.experience;
                float requiredExp = (float)stats.requiredExperience;
                expSlider.value = currentExp / requiredExp;
            }
            else
            {
                expSlider.value = 0f;
            }
        }


        // === 텍스트 업데이트 ===
        if (levelText)
        {
            levelText.text = "Lv. " + stats.level.ToString();
        }
        if (GoldText)
        {
            GoldText.text = stats.Gold.ToString() + "원";
        }

        // 체력 텍스트: 현재 값 / 최대 값 - float
        if (HpText)
        {
            // float 값은 소수점 처리를 위해 여전히 Mathf.FloorToInt를 사용합니다.
            HpText.text = Mathf.FloorToInt(stats.health).ToString("F0") + " / " + Mathf.FloorToInt(stats.MaxHealth).ToString("F0");
        }

        // 마나 텍스트: 현재 값 / 최대 값 - float
        if (MpText)
        {
            MpText.text = Mathf.FloorToInt(stats.mana).ToString("F0") + " / " + Mathf.FloorToInt(stats.MaxMana).ToString("F0");
        }

        // 경험치 텍스트: 현재 값 / 필요 경험치 - long을 안전하게 문자열로 변환
        if (ExpText)
        {
            // long 타입의 정수 경험치를 바로 문자열로 변환하여 정밀도 손실 없이 표시합니다.
            ExpText.text = stats.experience.ToString() + " / " + stats.requiredExperience.ToString();
        }
    }
}