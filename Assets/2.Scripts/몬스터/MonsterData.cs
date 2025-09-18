using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������ ��� �⺻ ���Ȱ� ���� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// �� ���� ������ ���� �ڵ带 �������� �ʰ� �پ��� ���͸� ���� �� �ֽ��ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Monster Data", menuName = "Monster/Monster Data", order = 1)]
public class MonsterData : ScriptableObject
{
    [Header("���� �⺻ ����")]
    [Tooltip("���͸� �ĺ��ϴ� ���� ID�Դϴ�.")]
    public int monsterID;
    [Tooltip("������ �̸��Դϴ�.")]
    public string monsterName;

    [Tooltip("������ ���� �����Դϴ�.")]
    public int monsterLevel;

    [Header("���� �⺻ ����")]
    public float maxHealth; // �ִ� ü��
    public int maxMana; // �ִ� ����
    public int attackPower; // ���ݷ�
    public float magicAttackPower; // ���� ���ݷ�
    public int defense; // ����
    public int magicDefense; // ���� ����
    public float moveSpeed; // �̵� �ӵ�
    public float criticalChance; // ġ��Ÿ Ȯ�� (0.0 ~ 1.0)
    public float criticalDamageMultiplier; // ġ��Ÿ ������ ����

    [Header("���� ���� Ÿ��")]
    [Tooltip("������ �⺻ ���� Ÿ���� �����մϴ�.")]
    public DamageType attackDamageType = DamageType.Physical;

    [Header("���� ����")]
    [Tooltip("óġ �� ��� ����ġ ���� �����Դϴ�.")]
    public int minExpReward;
    public int maxExpReward;

    [Tooltip("óġ �� ��� ��� ���� �����Դϴ�.")]
    public int minGoldReward;
    public int maxGoldReward;

    [Header("������ ��� ����")]
    [Tooltip("��� ������ ������ ��ϰ� ��� Ȯ���Դϴ�.")]
    public List<LootItem> lootTable = new List<LootItem>();

    [Tooltip("���Ͱ� �׾��� �� ����� �������� �ּ� �����Դϴ�.")]
    public int minItemDropCount = 0;

    [Tooltip("���Ͱ� �׾��� �� ����� �������� �ִ� �����Դϴ�.")]
    public int maxItemDropCount = 1;

    /// <summary>
    /// ������ ��� ���̺��� �� �׸��� �����ϴ� ����ü�Դϴ�.
    /// �� ����ü�� MonsterData ��ũ���ͺ� ������Ʈ���� ���˴ϴ�.
    /// </summary>
    [System.Serializable]
    public struct LootItem
    {
        [Tooltip("����� ������ ��ũ���ͺ� ������Ʈ�Դϴ�. (����ڴ��� ItemData Ŭ������ �������ּ���.)")]
        public BaseItemSO itemData;

        [Tooltip("�������� ��ӵ� Ȯ���Դϴ�. (0.0f ~ 1.0f ������ ��)")]
        [Range(0.0f, 1.0f)]
        public float dropChance;
    }
}
