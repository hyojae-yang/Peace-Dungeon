using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// ������ ��Ʈ ���ʽ� �ܰ躰 ������ ��� ����ü�Դϴ�.
/// </summary>
[Serializable]
public struct SetBonusStep
{
    [Tooltip("ȿ���� �ޱ� ���� �ʿ��� ��Ʈ �������� �����Դϴ�.")]
    public int requiredCount;
    [Tooltip("�� �ܰ迡�� �߰��� �ο��� �ɷ�ġ�Դϴ�.")]
    public List<StatModifier> bonusStats;
}

/// <summary>
/// ��Ʈ �������� ���ʽ� ȿ�� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Set Bonus Data", menuName = "Item/Set Bonus Data")]
public class SetBonusDataSO : ScriptableObject
{
    [Tooltip("�ش� ��Ʈ�� �ĺ��ϴ� ���� ID�Դϴ�. EquipmentItemSO�� setID�� �����ؾ� �մϴ�.")]
    public string setID;

    [Tooltip("UI�� ǥ�õ� ��Ʈ�� �̸��Դϴ�.")]
    public string setName;

    [Tooltip("��Ʈ ������ ������ ���� ���ʽ� ȿ�� ����Դϴ�.")]
    public List<SetBonusStep> bonusSteps;
}