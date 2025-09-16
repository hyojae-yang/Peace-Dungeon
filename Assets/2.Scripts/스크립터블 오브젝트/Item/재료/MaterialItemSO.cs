using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 재료 아이템의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// 모든 재료 아이템은 이 스크립트를 기반으로 생성됩니다.
/// BaseItemSO를 상속받아 아이템의 기본 정보를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Material Item", menuName = "Item/Material Item")]
public class MaterialItemSO : BaseItemSO
{
    // === 재료 아이템 고유 속성 ===
    [Header("재료 아이템 고유 속성")]
    [Tooltip("이 재료가 사용되는 조합식의 고유 ID 리스트입니다.")]
    [SerializeField]
    private List<int> recipeIDs = new List<int>();

    /// <summary>
    /// 이 재료가 사용되는 모든 조합식의 고유 ID를 가져옵니다.
    /// 외부 시스템(예: 조합 시스템)에서 이 재료로 만들 수 있는 아이템을 찾을 때 사용됩니다.
    /// </summary>
    public List<int> GetRecipeIDs()
    {
        return recipeIDs;
    }
}