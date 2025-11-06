using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 조합 레시피의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// 이 스크립터블 오브젝트는 조합 시스템이 참조할 레시피 정보를 담고 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class RecipeSO : ScriptableObject
{
    // [Header("레시피 기본 정보")]
    [Tooltip("레시피를 식별하는 고유 ID입니다. 런타임에 이 ID로 레시피를 찾습니다.")]
    public int recipeID;

    [Tooltip("조합을 통해 만들어지는 결과 아이템입니다.")]
    public BaseItemSO resultItem;

    [Tooltip("레시피에 필요한 재료 아이템과 수량의 리스트입니다.")]
    public List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
}

/// <summary>
/// 조합 재료의 정보를 담는 직렬화 가능한 구조체입니다.
/// 각 재료 아이템과 필요한 수량을 저장합니다.
/// </summary>
[System.Serializable]
public struct RecipeIngredient
{
    [Tooltip("재료로 사용될 아이템입니다. BaseItemSO를 상속받는 모든 아이템이 가능합니다.")]
    public BaseItemSO item;

    [Tooltip("재료로 필요한 아이템의 수량입니다.")]
    public int quantity;
}