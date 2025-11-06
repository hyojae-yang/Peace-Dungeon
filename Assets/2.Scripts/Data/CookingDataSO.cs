// 파일명: CookingDataSO.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 요리 NPC가 제공하는 레시피 목록을 저장하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Cooking Data", menuName = "Custom/Cooking Data")]
public class CookingDataSO : ScriptableObject
{
    // 요리 NPC가 제공할 모든 레시피 목록을 담는 리스트입니다.
    [Tooltip("이 NPC가 제공할 모든 요리 레시피 목록을 여기에 할당합니다.")]
    public List<RecipeSO> recipes;
}