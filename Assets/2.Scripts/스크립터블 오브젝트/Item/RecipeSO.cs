using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// �� ��ũ���ͺ� ������Ʈ�� ���� �ý����� ������ ������ ������ ��� �ֽ��ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class RecipeSO : ScriptableObject
{
    // [Header("������ �⺻ ����")]
    [Tooltip("�����Ǹ� �ĺ��ϴ� ���� ID�Դϴ�. ��Ÿ�ӿ� �� ID�� �����Ǹ� ã���ϴ�.")]
    public int recipeID;

    [Tooltip("������ ���� ��������� ��� �������Դϴ�.")]
    public BaseItemSO resultItem;

    [Tooltip("�����ǿ� �ʿ��� ��� �����۰� ������ ����Ʈ�Դϴ�.")]
    public List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
}

/// <summary>
/// ���� ����� ������ ��� ����ȭ ������ ����ü�Դϴ�.
/// �� ��� �����۰� �ʿ��� ������ �����մϴ�.
/// </summary>
[System.Serializable]
public struct RecipeIngredient
{
    [Tooltip("���� ���� �������Դϴ�. BaseItemSO�� ��ӹ޴� ��� �������� �����մϴ�.")]
    public BaseItemSO item;

    [Tooltip("���� �ʿ��� �������� �����Դϴ�.")]
    public int quantity;
}