// 파일명: RecipeItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 요리 UI의 개별 레시피 아이템을 관리하는 스크립트입니다.
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

    private const string EMPTY_SLOT_EMOJI = "-"; // 빈 슬롯에 표시할 이모티콘

    // === 데이터 할당 메서드 ===
    /// <summary>
    /// RecipeSO 데이터를 받아와서 UI를 업데이트하는 메서드입니다.
    /// </summary>
    /// <param name="recipeSO">표시할 레시피 데이터입니다.</param>
    public void SetData(RecipeSO recipeSO)
    {
        // 1. 완성 아이템 정보 설정
        if (recipeSO.resultItem != null)
        {
            resultImage.sprite = recipeSO.resultItem.itemIcon;
            resultText.text = recipeSO.resultItem.itemName;
        }

        // 2. 재료 아이템 정보 설정
        // 재료 텍스트 컴포넌트들을 순회하며 데이터 할당
        for (int i = 0; i < ingredientTexts.Count; i++)
        {
            // 레시피 재료가 존재하는지 확인
            if (i < recipeSO.ingredients.Count)
            {
                var ingredient = recipeSO.ingredients[i];
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