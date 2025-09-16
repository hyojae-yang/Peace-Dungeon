using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��� �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ Ŭ�����Դϴ�.
/// BaseItemSO�� ��ӹ޾� ����� ���� �Ӽ��� �����ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Item/Equipment Item")]
public class EquipmentItemSO : BaseItemSO
{
    [Header("��� �Ӽ�")]
    [Tooltip("��� �������� ����Դϴ�. ��޿� ���� �ɷ�ġ ���ʽ��� �޶����ϴ�.")]
    public ItemGrade itemGrade;

    [Tooltip("�� ��� �������� ����� �� �ִ� �����Դϴ�. (��: Weapon, Helmet, Accessory1 ��)")]
    public EquipSlot equipSlot; // ���� ���� ����

    [Tooltip("�� ��� �������� ���� �����Դϴ�. (��: Weapon, Helmet, Accessory ��)")]
    public EquipType equipType; // ����� ���� ����

    // �÷��̾��� PlayerStats ��ũ��Ʈ�� ������ �� �ֵ��� StatModifier�� ����մϴ�.
    [Header("�⺻ �ɷ�ġ")]
    [Tooltip("��� �⺻������ �ο��ϴ� �ɷ�ġ�Դϴ�. ��޿� ���� ���� �����˴ϴ�.")]
    public List<StatModifier> baseStats = new List<StatModifier>();

    [Header("�߰� �ɷ�ġ (�ɼ�)")]
    [Tooltip("�� �������� ���� �� �ִ� �߰� �ɷ�ġ ����Դϴ�. ItemGenerator�� ���� �������� �ο��˴ϴ�.")]
    public List<StatModifier> additionalStats = new List<StatModifier>();

    // --- ���ο� ��� �߰� ---

    [Header("��Ʈ ������ �Ӽ�")]
    [Tooltip("�� �������� ���� ��Ʈ�� ���� ID�Դϴ�. ��Ʈ �������� �ƴ� ��� ����Ӵϴ�.")]
    public string setID;
}