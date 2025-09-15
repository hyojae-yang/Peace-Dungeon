// BaseItemSO.cs
using UnityEngine;

/// <summary>
/// ��� �������� ���� �Ӽ��� �����ϴ� �߻� Ŭ�����Դϴ�.
/// ��� ������ ��ũ���ͺ� ������Ʈ�� �� Ŭ������ ��ӹ޾ƾ� �մϴ�.
/// </summary>
public abstract class BaseItemSO : ScriptableObject
{
    // [Header("�⺻ ������ ����")]
    [Tooltip("�������� �ĺ��ϴ� ���� ID�Դϴ�. ��Ÿ�ӿ� �� ID�� �������� �˻��մϴ�.")]
    public int itemID;

    [Tooltip("���� ������ ǥ�õ� �������� �̸��Դϴ�.")]
    public string itemName;

    [Tooltip("�������� �� �����Դϴ�. �κ��丮 ������ ���˴ϴ�.")]
    [TextArea(3, 5)]
    public string itemDescription;

    [Tooltip("�κ��丮���� �������� ��Ÿ�� �������Դϴ�.")]
    public Sprite itemIcon;

    [Tooltip("�������� ������ �Ǹ��� ���� �����Դϴ�.")]
    public int itemPrice;

    [Tooltip("�������� ������ ��Ÿ���ϴ�.")]
    public ItemType itemType;

    [Tooltip("�κ��丮 �� ĭ�� �ִ�� ������ �� �ִ� �������� �����Դϴ�. 1�� �����ϸ� ��ġ�� �ʽ��ϴ�.")]
    public int maxStack = 1;
}