using UnityEngine;
using System.Linq;

/// <summary>
/// ���� NPC ���� ������Ʈ�� �����Ǵ� ���� ��ũ��Ʈ.
/// NPCData�� �����Ͽ� ������ ������ ������, �ٸ� ������Ʈ(�̵�, ��ȣ�ۿ�)�� �����մϴ�.
/// NPCManager�� ���� ���� �����Ϳ� �����ϰ�, ��� ������Ʈ ������ Ȯ���ϴ� ����� �����մϴ�.
/// </summary>
public class NPC : MonoBehaviour
{
    // �� NPC�� ���� �����͸� ��� ScriptableObject.
    [Tooltip("�� NPC�� ���� ���� ������(ScriptableObject)�� �Ҵ��մϴ�.")]
    [SerializeField]
    private NPCData npcData;

    // NPC�� �̵��� ����ϴ� ������Ʈ.
    private NPCMovement npcMovement;

    // NPC�� �÷��̾� ���� ��ȣ�ۿ��� ����ϴ� ������Ʈ.
    private NPCInteraction npcInteraction;

    // ����Ʈ ����� ����ϴ� ������Ʈ.
    private QuestGiver questGiver;

    // ���� ����� ����ϴ� ������Ʈ.
    private ShopManager shopManager;

    /// <summary>
    /// NPC�� ���� �����͸� �������� ������Ƽ. �ܺο��� �б� �������� ����մϴ�.
    /// </summary>
    public NPCData Data
    {
        get { return npcData; }
    }

    /// <summary>
    /// NPC�� ����Ʈ ������ ������Ʈ�� �������� ������Ƽ. �ܺο��� �б� �������� ����մϴ�.
    /// </summary>
    public QuestGiver QuestGiver
    {
        get { return questGiver; }
    }

    /// <summary>
    /// MonoBehaviour�� Awake �޼���. ��ũ��Ʈ�� �ε�� �� ������Ʈ���� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();
        npcInteraction = GetComponent<NPCInteraction>();
        questGiver = GetComponent<QuestGiver>();
        shopManager = GetComponent<ShopManager>();

        if (npcData == null)
        {
            Debug.LogError("NPCData�� �Ҵ���� �ʾҽ��ϴ�! NPC: " + this.gameObject.name);
        }
    }

    /// <summary>
    /// NPC�� ���� ȣ������ �����ϴ� �޼���.
    /// NPCManager�� ���� ���� �����͸� �����ɴϴ�.
    /// </summary>
    /// <returns>���� �÷��̾ ���� ȣ���� ��</returns>
    public int GetAffection()
    {
        if (NPCManager.Instance != null && Data != null)
        {
            return NPCManager.Instance.GetAffection(Data.npcName);
        }
        return 0;
    }

    /// <summary>
    /// �÷��̾��� �ൿ�� ���� ȣ������ �����ϴ� �޼���.
    /// NPCManager�� ���� ���� �����͸� �����մϴ�.
    /// </summary>
    /// <param name="value">������ ȣ���� �� (����: ���, ����: ����)</param>
    public void ChangeAffection(int value)
    {
        if (NPCManager.Instance != null && Data != null)
        {
            NPCManager.Instance.ChangeAffection(Data.npcName, value);
        }
    }

    /// <summary>
    /// NPC�� Ư�� ������Ʈ�� ������ �ִ��� Ȯ���ϴ� ���׸� �޼���.
    /// </summary>
    /// <typeparam name="T">Ȯ���� ������Ʈ Ÿ��</typeparam>
    /// <returns>������Ʈ�� �����ϸ� true, �ƴϸ� false.</returns>
    public bool HasComponent<T>() where T : Component
    {
        return GetComponent<T>() != null;
    }

    /// <summary>
    /// NPC�� ����Ʈ ����� ������ �ִ��� Ȯ���ϴ� �޼���.
    /// </summary>
    /// <returns>����Ʈ ��� ������Ʈ�� �����ϸ� true, �ƴϸ� false.</returns>
    public bool HasQuestGiver()
    {
        return questGiver != null;
    }

    /// <summary>
    /// NPC�� Ư�� ����� ������ �ִ��� Ȯ���ϴ� �޼���.
    /// </summary>
    /// <returns>�ϳ� �̻��� Ư�� ��� ������Ʈ�� �����ϸ� true, �ƴϸ� false.</returns>
    public bool HasSpecialFunction()
    {
        // ����, ���尣 �� Ư�� ��� ������Ʈ���� ���⿡ �߰�
        return shopManager != null;
    }

    /// <summary>
    /// Ư�� ��ư�� ǥ�õ� �̸��� ��ȯ�ϴ� �޼���.
    /// </summary>
    /// <returns>NPC�� ���� Ư�� ��ɿ� �ش��ϴ� �̸�. ����� ������ �� ���ڿ� ��ȯ.</returns>
    public string GetSpecialButtonName()
    {
        if (shopManager != null)
        {
            return "����";
        }
        return "";
    }
}