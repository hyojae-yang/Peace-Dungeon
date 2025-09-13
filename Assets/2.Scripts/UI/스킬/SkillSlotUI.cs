using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용한다면 필요합니다.

// 이 스크립트는 액티브 스킬 슬롯 패널에 있는 개별 슬롯의 UI를 담당합니다.
// 스킬 등록/해제 시 이미지와 텍스트를 업데이트하며, 추후 쿨타임 슬라이더 등을 관리합니다.
public class SkillSlotUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [Tooltip("스킬 이미지를 표시할 Image 컴포넌트를 할당하세요.")]
    public Image skillImage;
    [Tooltip("스킬이 등록되지 않았을 때 표시할 기본 슬롯 스프라이트를 할당하세요.")]
    public Sprite defaultSlotSprite; // <-- 기본 스프라이트 변수 추가

    // 이 스크립트는 데이터 자체를 저장하지 않고, 받은 데이터로 UI만 업데이트합니다.
    private SkillData currentSkillData;

    /// <summary>
    /// 외부(SlotSelectionPanel)에서 호출되어 슬롯의 UI를 업데이트합니다.
    /// </summary>
    /// <param name="data">슬롯에 등록할 스킬 데이터. 해제 시에는 null을 전달합니다.</param>
    public void UpdateUI(SkillData data)
    {
        // 현재 슬롯에 등록된 스킬 데이터를 업데이트합니다.
        currentSkillData = data;

        // 데이터가 유효한지 확인하고 UI를 갱신합니다.
        if (currentSkillData != null)
        {
            // 스킬이 등록된 경우: 이미지를 활성화하고, 스프라이트를 설정합니다.
            skillImage.enabled = true;
            skillImage.sprite = currentSkillData.skillImage;
        }
        else
        {
            // 스킬이 해제된 경우 (data가 null):
            // 이미지를 활성화 상태로 유지하되, 기본 스프라이트로 변경합니다.
            skillImage.enabled = true; // 이미지는 보이도록 유지
            skillImage.sprite = defaultSlotSprite; // <-- null 대신 기본 스프라이트 할당

            // 만약 기본 스프라이트도 없다면 이미지를 비활성화할 수 있습니다.
            // if (defaultSlotSprite == null)
            // {
            //     skillImage.enabled = false;
            // }
        }
    }
}