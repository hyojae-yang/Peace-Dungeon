using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용한다면 필요합니다.

// 이 스크립트는 액티브 스킬 슬롯 패널에 있는 개별 슬롯의 UI를 담당합니다.
// 스킬 등록/해제 시 이미지와 텍스트를 업데이트하며, 추후 쿨타임 슬라이더 등을 관리합니다.
public class SkillSlotUI : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("스킬 이미지를 표시할 Image 컴포넌트를 할당하세요.")]
    public Image skillImage;
    [Tooltip("스킬이 등록되지 않았을 때 표시할 기본 슬롯 스프라이트를 할당하세요.")]
    public Sprite defaultSlotSprite; // <-- 기본 스프라이트 변수 추가
    [Tooltip("스킬의 마나 소모량을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI manaCostText;
    [Tooltip("스킬의 남은 쿨타임을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI cooldownText; // <-- 쿨타임 텍스트 변수 추가
    [Tooltip("쿨타임 진행 상황을 시각적으로 표시할 슬라이더 컴포넌트를 할당하세요.")]
    public Slider cooldownSlider; // <-- 쿨타임 슬라이더 추가

    // 이 스크립트는 데이터 자체를 저장하지 않고, 받은 데이터로 UI만 업데이트합니다.
    private SkillData currentSkillData;

    /// <summary>
    /// 외부(SlotSelectionPanel)에서 호출되어 슬롯의 UI를 업데이트합니다.
    /// </summary>
    /// <param name="data">슬롯에 등록할 스킬 데이터. 해제 시에는 null을 전달합니다.</param>
    /// <param name="manaCost">표시할 스킬의 마나 소모량.</param>
    public void UpdateUI(SkillData data, float manaCost)
    {
        currentSkillData = data;

        if (currentSkillData != null)
        {
            skillImage.enabled = true;
            skillImage.sprite = currentSkillData.skillImage;

            if (manaCostText != null)
            {
                manaCostText.text = manaCost.ToString();
            }

            // 스킬이 등록되면 슬라이더를 초기화합니다.
            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(false);
            }
        }
        else
        {
            skillImage.enabled = true;
            skillImage.sprite = defaultSlotSprite;

            if (manaCostText != null)
            {
                manaCostText.text = string.Empty;
            }

            // 스킬이 해제되면 쿨타임 텍스트와 슬라이더를 비웁니다.
            if (cooldownText != null)
            {
                cooldownText.text = string.Empty;
            }
            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 남은 쿨타임 값을 받아 UI를 업데이트합니다.
    /// </summary>
    /// <param name="remainingCooldown">남은 쿨타임 시간 (초)</param>
    /// <param name="maxCooldown">스킬의 최대 쿨타임 시간 (초)</param>
    public void UpdateCooldownUI(float remainingCooldown, float maxCooldown)
    {
        // 쿨타임 텍스트와 슬라이더가 모두 할당되었는지 확인합니다.
        if (cooldownText == null || cooldownSlider == null) return;

        // 쿨타임이 남았다면 텍스트와 슬라이더를 표시하고, 아니면 비웁니다.
        if (remainingCooldown > 0f)
        {
            cooldownText.text = remainingCooldown.ToString("F1"); // 소수점 첫째 자리까지 표시
            cooldownSlider.gameObject.SetActive(true);

            // 슬라이더의 값을 업데이트합니다.
            cooldownSlider.maxValue = maxCooldown;
            cooldownSlider.value = remainingCooldown;
        }
        else
        {
            cooldownText.text = string.Empty;
            cooldownSlider.gameObject.SetActive(false);
        }
    }
}