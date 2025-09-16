using UnityEngine;

/// <summary>
/// ����Ʈ �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// ��� ����Ʈ �������� �� ��ũ��Ʈ�� ������� �����˴ϴ�.
/// BaseItemSO�� ��ӹ޾� �������� �⺻ ������ �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Quest Item", menuName = "Item/Quest Item")]
public class QuestItemSO : BaseItemSO
{
    // === ����Ʈ ������ ���� �Ӽ� ===
    [Header("����Ʈ ������ ���� �Ӽ�")]
    [Tooltip("�� ����Ʈ �������� ���� ����Ʈ�� ���� ID�Դϴ�.")]
    [SerializeField]
    private int questID;

    /// <summary>
    /// �� ����Ʈ �������� ���� ����Ʈ�� ���� ID�� �����ɴϴ�.
    /// �ܺ� �ý���(��: ����Ʈ �ý���)���� �÷��̾��� �κ��丮�� �ִ�
    /// ����Ʈ �������� �ĺ��� �� ���˴ϴ�.
    /// </summary>
    /// <returns>����Ʈ �������� ���� ����Ʈ�� ���� ID</returns>
    public int GetQuestID()
    {
        return questID;
    }
}