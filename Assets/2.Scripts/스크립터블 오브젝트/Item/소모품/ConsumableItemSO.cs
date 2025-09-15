// ConsumableItemSO.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �Ҹ�ǰ �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ Ŭ�����Դϴ�.
/// BaseItemSO�� ��ӹ޾� �Ҹ�ǰ���� ���� �Ӽ��� �����ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Item/Consumable Item")]
public class ConsumableItemSO : BaseItemSO
{
    [Header("�Ҹ�ǰ �Ӽ�")]
    [Tooltip("�Ҹ�ǰ ��� �� ����� �ɷ�ġ ���ʽ��Դϴ�. ���� ȿ���� ���� �� �ֽ��ϴ�.")]
    public List<StatModifier> consumptionEffects = new List<StatModifier>();

    [Tooltip("�� ���Կ� ���� �� �ִ� �ִ� �����Դϴ�.")]
    public int maxStackCount = 10;

    [Tooltip("�Ҹ�ǰ ��� �� �÷��̾�� ����Ǵ� ���� �Ǵ� ������� ���� �ð�(��)�Դϴ�. (0�� ��� ��� ȿ��)")]
    public float effectDuration = 0f;

    /// <summary>
    /// �Ҹ�ǰ�� ����ϴ� ������ �����ϴ� ����(virtual) �޼����Դϴ�.
    /// ���� �� Ŭ������ ��ӹ޾� �� ������ ����� ���� �Ҹ�ǰ(��: ��Ȱ ������)�� ���� �� �ֽ��ϴ�.
    /// </summary>
    public virtual void Use(PlayerStats playerStats)
    {
        Debug.Log($"{itemName}�� ����߽��ϴ�!");

        // consumptionEffects ����Ʈ�� ��� ��� ȿ���� �÷��̾�� �����ϴ� ����
        // ��: ü�� ȸ��, ���� ȸ�� ��
        foreach (var effect in consumptionEffects)
        {
            // ���⿡�� effect�� statType�� value�� playerStats�� �ݿ��ϴ� �ڵ带 �ۼ��ؾ� �մϴ�.
            // ���� ���, effect.statType�� MaxHealth���, playerStats.health�� ������Ű�� ���Դϴ�.
        }
    }
}