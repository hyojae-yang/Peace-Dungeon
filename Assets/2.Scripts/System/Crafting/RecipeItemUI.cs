// 파일명: RecipeItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 요리 UI의 개별 레시피 아이템을 관리하는 스크립트입니다.
/// 레시피 발견 여부에 따라 정보를 숨기거나 표시하는 책임을 가집니다.
/// SOLID: 개방-폐쇄 원칙 (표시 기능 확장을 위해 SetData 메서드에 매개변수를 추가)
/// </summary>
public class RecipeItemUI : MonoBehaviour
{
    // === UI 참조 ===
    [Header("UI References")]
    [Tooltip("완성 아이템 이미지를 표시하는 UI 컴포넌트입니다.")]
    [SerializeField]
    private Image resultImage;
    [Tooltip("완성 아이템의 이름을 표시하는 UI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI resultText;

    // 재료 텍스트 컴포넌트들을 리스트로 관리하여 유연성을 높입니다.
    [Tooltip("재료 아이템의 이름을 표시하는 UI 컴포넌트들입니다.")]
    [SerializeField]
    private List<TextMeshProUGUI> ingredientTexts = new List<TextMeshProUGUI>(4);

    //private Button craftButton; // 이 줄을 삭제합니다.

    private const string EMPTY_SLOT_EMOJI = "-"; // 빈 재료 슬롯에 표시할 텍스트
    private const string UNKNOWN_RECIPE_TEXT = "???"; // 미발견 레시피에 표시할 텍스트

    [Header("Visual Config")]
    [Tooltip("미발견 레시피일 때 사용할 기본/알 수 없는 아이콘 스프라이트입니다.")]
    [SerializeField]
    private Sprite unknownSprite; // 인스펙터에서 설정 가능하도록 추가

    // === 데이터 할당 메서드 ===
    /// <summary>
    /// RecipeSO 데이터를 받아와서 UI를 업데이트하며, 레시피 발견 여부에 따라 내용을 숨깁니다.
    /// SOLID: 단일 책임 원칙 (UI를 업데이트하는 역할).
    /// </summary>
    /// <param name="recipeSO">표시할 레시피 데이터입니다.</param>
    /// <param name="isDiscovered">레시피가 플레이어에게 발견된 상태인지 여부입니다.</param>
    public void SetData(RecipeSO recipeSO, bool isDiscovered)
    {
        // 1. 레시피가 발견되지 않은 경우 (미발견 레시피 숨기기 로직)
        if (!isDiscovered)
        {
            // A. 완성 아이템 정보 숨기기
            resultImage.sprite = unknownSprite; // 알 수 없는 아이콘으로 표시
            resultText.text = UNKNOWN_RECIPE_TEXT;

            // B. 재료 아이템 정보 숨기기
            for (int i = 0; i < ingredientTexts.Count; i++)
            {
                // 재료 텍스트도 모두 '???'로 표시하거나 비웁니다.
                // 여기서는 레시피에 필요한 재료 개수만큼만 '???'를 표시하여 힌트를 줄 수 있도록 설계합니다.
                if (i < recipeSO.ingredients.Count)
                {
                    ingredientTexts[i].text = UNKNOWN_RECIPE_TEXT;
                }
                else
                {
                    ingredientTexts[i].text = EMPTY_SLOT_EMOJI;
                }
            }
            return; // 미발견 처리 후 함수 종료
        }

        // 2. 레시피가 발견된 경우 (기존 로직 유지)

        // A. 완성 아이템 정보 설정
        if (recipeSO.resultItem != null)
        {
            resultImage.sprite = recipeSO.resultItem.itemIcon;
            resultText.text = recipeSO.resultItem.itemName;
        }

        // B. 재료 아이템 정보 설정
        // 재료 텍스트 컴포넌트들을 순회하며 데이터 할당
        for (int i = 0; i < ingredientTexts.Count; i++)
        {
            // 레시피 재료가 존재하는지 확인
            if (i < recipeSO.ingredients.Count)
            {
                var ingredient = recipeSO.ingredients[i];
                // 수량은 1개로 고정되는 의도에 맞게, 재료 목록에서도 quantity가 1로 설정되어 있다고 가정합니다.
                ingredientTexts[i].text = $"{ingredient.item.itemName} x{ingredient.quantity}";
            }
            else
            {
                // 재료가 없는 슬롯은 이모티콘으로 채웁니다.
                ingredientTexts[i].text = EMPTY_SLOT_EMOJI;
            }
        }
    }
}