using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC�� �Ǹ��ϴ� ������ ��� �����͸� ��� ScriptableObject�Դϴ�.
/// ����Ƽ �����Ϳ��� Assets -> Create -> NPC -> Shop Data�� ���� �������� ������ �� �ֽ��ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Shop Data", menuName = "NPC/Shop Data")]
public class ShopData : ScriptableObject
{
    [Tooltip("�� �������� �Ǹ��� ������ ����Դϴ�.")]
    public List<BaseItemSO> itemsToSell;
}