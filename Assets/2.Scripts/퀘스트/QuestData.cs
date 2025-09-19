using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����Ʈ�� ������ ������ ��� ScriptableObject.
/// ����Ʈ�� �̸�, ��ǥ, ����, ���� ���� �� ������ �ʴ� �����͸� �������� �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Quest Data", menuName = "Quest/Quest Data", order = 1)]
public class QuestData : ScriptableObject
{
    // ����Ʈ�� ���� ID. ����Ʈ�� �ĺ��ϴ� �� ���˴ϴ�.
    [Header("Quest Information")]
    [Tooltip("����Ʈ�� ������ �̸��Դϴ�. QuestManager���� ����Ʈ�� �ĺ��ϴ� �� ���˴ϴ�.")]
    public int questID;

    // ����Ʈ�� �����Դϴ�.
    [Tooltip("����Ʈ�� �����Դϴ�.")]
    public string questTitle;

    // ����Ʈ ��ǥ�� ���� �����Դϴ�.
    [Tooltip("����Ʈ ��ǥ�� ���� �����Դϴ�.")]
    [TextArea(3, 5)]
    public string questDescription;

    // �� ����Ʈ�� �ޱ� ���� �ʿ��� �ּ� ȣ�����Դϴ�.
    [Tooltip("����Ʈ�� �ޱ� ���� �ʿ��� �ּ� ȣ�����Դϴ�. �� ������ �����ؾ� NPC�� ����Ʈ ��Ͽ� ǥ�õ˴ϴ�.")]
    public int requiredAffection;

    // �� ����Ʈ�� �ޱ� ���� ����Ǿ�� �ϴ� ����Ʈ���� ID ����Դϴ�.
    [Header("Quest Prerequisites")]
    [Tooltip("�� ����Ʈ�� �����ϱ� ���� �Ϸ��ؾ� �ϴ� ���� ����Ʈ�� ID ����Դϴ�.")]
    public List<int> prerequisiteQuests = new List<int>();

    // ����Ʈ�� �Ϸ� ������ ��� ����Ʈ
    [Header("Quest Conditions")]
    [Tooltip("����Ʈ �ϷḦ ���� �����ؾ� �� ���� ����Դϴ�.")]
    public List<QuestCondition> conditions = new List<QuestCondition>();

    // ����Ʈ �Ϸ� �� ������ ���� �� �ִ��� ����
    [Tooltip("����Ʈ�� �� ���� �Ϸ��� �� �ִ���, �ݺ� �������� �����մϴ�.")]
    public bool isRepeatable = false;

    // ����Ʈ ���� ����Դϴ�.
    [Header("Quest Rewards")]
    [Tooltip("����Ʈ �Ϸ� �� �÷��̾ ���� ���� ������ ����Դϴ�.")]
    public List<RewardItem> rewardItems = new List<RewardItem>();

    // ����Ʈ ���� ����ġ�Դϴ�.
    [Tooltip("����Ʈ �Ϸ� �� �÷��̾ ���� ����ġ�Դϴ�.")]
    public int experienceReward;

    // ����Ʈ ���� ����Դϴ�.
    [Tooltip("����Ʈ �Ϸ� �� �÷��̾ ���� ����Դϴ�.")]
    public int goldReward;

    // ���� ������ ������ ����ȭ�ϱ� ���� ���� Ŭ����
    [System.Serializable]
    public class RewardItem
    {
        public int itemID;
        public int itemCount;
    }
}

/// <summary>
/// ����Ʈ �Ϸ� ������ �����ϴ� Ŭ����.
/// </summary>
[System.Serializable]
public class QuestCondition
{
    // ����Ʈ �Ϸ� ������ ���� (������ ����, ��ȭ, ���� óġ ��)
    public enum ConditionType
    {
        CollectItems,
        TalkToNPC,
        DefeatMonsters
    }

    [Tooltip("����Ʈ �Ϸ� ������ ������ �����մϴ�.")]
    public ConditionType conditionType;

    // ���ǿ� �ʿ��� ��ǥ ID (������ ID, NPC �̸�, ���� �̸� ��)
    [Tooltip("���ǿ� �ʿ��� ��ǥ�� ���� ID�Դϴ�.")]
    public int targetID;

    // ��ǥ ���� (������ ����, ���� ������ ��)
    [Tooltip("���ǿ� �ʿ��� ��ǥ �����Դϴ�.")]
    public int requiredAmount;
}