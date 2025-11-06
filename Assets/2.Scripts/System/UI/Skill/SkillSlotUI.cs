using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용한다면 필요합니다.

// 이 스크립트는 액티브 스킬 슬롯 패널에 있는 개별 슬롯의 UI를 담당합니다.
// 스킬 등록/해제 시 이미지와 텍스트를 업데이트하며, 쿨타임 슬라이더 등을 관리합니다.
// [SOLID: 단일 책임 원칙 (SRP)을 준수하며 UI 표시 역할만 수행합니다.]
public class SkillSlotUI : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("스킬 이미지를 표시할 Image 컴포넌트를 할당하세요.")]
    public Image skillImage;
    [Tooltip("스킬이 등록되지 않았을 때 표시할 기본 슬롯 스프라이트를 할당하세요.")]
    public Sprite defaultSlotSprite;
    [Tooltip("스킬의 마나 소모량을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI manaCostText;
    [Tooltip("스킬의 남은 쿨타임을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI cooldownText;
    [Tooltip("쿨타임 진행 상황을 시각적으로 표시할 슬라이더 컴포넌트를 할당하세요.")]
    public Slider cooldownSlider;

    // 이 스크립트는 데이터 자체를 저장하지 않고, 받은 데이터의 참조로 UI만 업데이트합니다.
    private SkillData currentSkillData; // 현재 등록된 스킬 데이터 참조

    /// <summary>
    /// 외부(SlotSelectionPanel)에서 호출되어 슬롯의 UI를 업데이트합니다.
    /// 스킬 등록/해제, 마나 코스트 등을 표시합니다.
    /// </summary>
    /// <param name="data">슬롯에 등록할 스킬 데이터. 해제 시에는 null을 전달합니다.</param>
    /// <param name="manaCost">표시할 스킬의 마나 소모량.</param>
    public void UpdateUI(SkillData data, float manaCost)
    {
        currentSkillData = data;

        // 쿨타임 관련 UI 초기화 (스킬 등록/해제와 관계없이 초기 상태 설정)
        ResetCooldownUI();

        if (currentSkillData != null)
        {
            // 스킬 등록
            skillImage.enabled = true;
            skillImage.sprite = currentSkillData.skillImage;

            if (manaCostText != null)
            {
                // 마나 코스트는 정수로 표시하는 경우가 많으므로 "F0" 또는 그냥 ToString() 사용
                manaCostText.text = ((int)manaCost).ToString();
            }
        }
        else
        {
            // 스킬 해제
            skillImage.enabled = true;
            skillImage.sprite = defaultSlotSprite;

            if (manaCostText != null)
            {
                manaCostText.text = string.Empty;
            }
        }
    }

    /// <summary>
    /// 스킬의 최대 쿨타임 시간을 슬라이더에 설정합니다.
    /// [SOLID: 단일 책임 원칙에 따라 maxValue 설정 책임을 분리했습니다. 스킬 등록 시 1회 호출 권장]
    /// </summary>
    /// <param name="maxCooldown">스킬의 최대 쿨타임 시간 (초)</param>
    public void SetMaxCooldown(float maxCooldown)
    {
        if (cooldownSlider != null && maxCooldown > 0f)
        {
            cooldownSlider.maxValue = maxCooldown;
        }
    }

    /// <summary>
    /// 남은 쿨타임 값을 받아 UI를 업데이트합니다.
    /// 이 메서드는 쿨타임 진행 동안 매 프레임(또는 일정 주기) 호출되어야 합니다.
    /// </summary>
    /// <param name="remainingCooldown">남은 쿨타임 시간 (초)</param>
    public void UpdateCooldownUI(float remainingCooldown)
    {
        // 쿨타임 텍스트와 슬라이더가 모두 할당되었는지, 스킬이 등록되었는지 확인
        if (cooldownText == null || cooldownSlider == null || currentSkillData == null)
        {
            return;
        }

        // remainingCooldown 값이 유효한지 확인 (음수나 비정상적인 값 방지)
        if (remainingCooldown < 0f)
        {
            remainingCooldown = 0f;
        }

        // 쿨타임이 남았다면 UI 표시
        if (remainingCooldown > 0.01f) // 0에 가까운 값까지 처리
        {
            cooldownSlider.gameObject.SetActive(true);

            // 쿨타임 길이에 따라 텍스트 포맷을 동적으로 변경하여 가독성 향상
            if (remainingCooldown < 10f)
            {
                cooldownText.text = remainingCooldown.ToString("F1"); // 10초 미만: 소수점 첫째 자리까지 표시
            }
            else
            {
                cooldownText.text = Mathf.CeilToInt(remainingCooldown).ToString(); // 10초 이상: 정수로 표시
            }

            // 슬라이더 값을 업데이트 (maxValue는 SetMaxCooldown에서 설정했다고 가정)
            cooldownSlider.value = remainingCooldown;
        }
        else // 쿨타임 종료
        {
            ResetCooldownUI();
        }
    }

    /// <summary>
    /// 쿨타임 관련 UI (텍스트, 슬라이더)를 초기 상태로 리셋합니다.
    /// </summary>
    private void ResetCooldownUI()
    {
        if (cooldownText != null)
        {
            cooldownText.text = string.Empty;
        }
        if (cooldownSlider != null)
        {
            cooldownSlider.value = 0f; // 값도 0으로 초기화
            cooldownSlider.gameObject.SetActive(false);
        }
    }
}