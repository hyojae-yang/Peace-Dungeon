// ���ϸ�: CookingDataSO.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �丮 NPC�� �����ϴ� ������ ����� �����ϴ� ScriptableObject�Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Cooking Data", menuName = "Custom/Cooking Data")]
public class CookingDataSO : ScriptableObject
{
    // �丮 NPC�� ������ ��� ������ ����� ��� ����Ʈ�Դϴ�.
    [Tooltip("�� NPC�� ������ ��� �丮 ������ ����� ���⿡ �Ҵ��մϴ�.")]
    public List<RecipeSO> recipes;
}