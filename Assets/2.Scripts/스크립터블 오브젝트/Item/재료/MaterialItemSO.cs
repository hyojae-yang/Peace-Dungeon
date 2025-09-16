using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��� �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// ��� ��� �������� �� ��ũ��Ʈ�� ������� �����˴ϴ�.
/// BaseItemSO�� ��ӹ޾� �������� �⺻ ������ �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Material Item", menuName = "Item/Material Item")]
public class MaterialItemSO : BaseItemSO
{
    // === ��� ������ ���� �Ӽ� ===
    [Header("��� ������ ���� �Ӽ�")]
    [Tooltip("�� ��ᰡ ���Ǵ� ���ս��� ���� ID ����Ʈ�Դϴ�.")]
    [SerializeField]
    private List<int> recipeIDs = new List<int>();

    /// <summary>
    /// �� ��ᰡ ���Ǵ� ��� ���ս��� ���� ID�� �����ɴϴ�.
    /// �ܺ� �ý���(��: ���� �ý���)���� �� ���� ���� �� �ִ� �������� ã�� �� ���˴ϴ�.
    /// </summary>
    public List<int> GetRecipeIDs()
    {
        return recipeIDs;
    }
}