using System;

/// <summary>
/// ���� ���� ���� ����Ǵ� NPC�� ���� ������(��: ȣ����)�� ��� Ŭ�����Դϴ�.
/// �� Ŭ������ ���� ���� �� �ҷ����� �ý��ۿ��� ���˴ϴ�.
/// </summary>
[Serializable]
public class NPCSessionData
{
    // �� �����Ͱ� ���� NPC�� ���� ID(�̸�)�Դϴ�.
    public string npcID;

    // �÷��̾ ���� NPC�� ���� ȣ�����Դϴ�.
    public int playerAffection;

    /// <summary>
    /// NPCSessionData�� �� �ν��Ͻ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="id">NPC�� ���� ID.</param>
    /// <param name="initialAffection">�ʱ� ȣ���� ��.</param>
    public NPCSessionData(string id, int initialAffection)
    {
        npcID = id;
        playerAffection = initialAffection;
    }
}