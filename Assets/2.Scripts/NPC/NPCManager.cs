using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� NPC���� ���� �����͸� �߾ӿ��� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// ���� �ε� �� ���� �� ������ ó���� ����մϴ�.
/// </summary>
public class NPCManager : MonoBehaviour
{
    // NPCManager�� �̱��� �ν��Ͻ�
    public static NPCManager Instance { get; private set; }

    // ��� NPC�� ��ũ���ͺ� ������Ʈ ���
    [Tooltip("��� NPC ������ ScriptableObject�� ���⿡ �Ҵ��ϼ���.")]
    [SerializeField]
    private List<NPCData> allNPCsData;

    // ���� ���� ���� ����Ǵ� NPC���� ���� ������
    // Key: NPC�� ���� ID (npcName), Value: �ش� NPC�� ���� ������ �ν��Ͻ�
    private Dictionary<string, NPCSessionData> npcSessionDataMap = new Dictionary<string, NPCSessionData>();

    private void Awake()
    {
        // �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
            InitializeSessionData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��� NPC �����ͷκ��� ���� �����͸� �ʱ�ȭ�մϴ�.
    /// ���� ���� �� �� �� ȣ��Ǿ� ��� NPC�� ���� ������ �ν��Ͻ��� �����մϴ�.
    /// </summary>
    private void InitializeSessionData()
    {
        foreach (var data in allNPCsData)
        {
            if (data != null && !npcSessionDataMap.ContainsKey(data.npcName))
            {
                // NPCData�κ��� �ʱ� ȣ���� ���� ������ �� �ν��Ͻ��� �����մϴ�.
                NPCSessionData sessionData = new NPCSessionData(data.npcName, data.playerAffection);
                npcSessionDataMap[data.npcName] = sessionData;
            }
        }
        Debug.Log("��� NPC ���� �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// Ư�� NPC�� ���� ȣ���� ���� �����ɴϴ�.
    /// </summary>
    /// <param name="npcID">ȣ������ ������ NPC�� ���� ID (npcName)</param>
    /// <returns>�ش� NPC�� ���� ȣ����. �����Ͱ� ������ �⺻���� 0�� ��ȯ�մϴ�.</returns>
    public int GetAffection(string npcID)
    {
        if (npcSessionDataMap.TryGetValue(npcID, out NPCSessionData data))
        {
            return data.playerAffection;
        }
        Debug.LogWarning($"NPC ID '{npcID}'�� ���� ���� �����Ͱ� �����ϴ�.");
        return 0;
    }

    /// <summary>
    /// Ư�� NPC�� ȣ������ �����մϴ�.
    /// �� �޼���� ���� �÷��� �� ȣ������ ������ �� ���˴ϴ�.
    /// </summary>
    /// <param name="npcID">ȣ������ ������ NPC�� ���� ID (npcName)</param>
    /// <param name="value">������ ȣ���� �� (����: ���, ����: ����)</param>
    public void ChangeAffection(string npcID, int value)
    {
        if (npcSessionDataMap.TryGetValue(npcID, out NPCSessionData data))
        {
            data.playerAffection = Mathf.Clamp(data.playerAffection + value, -100, 100);
            Debug.Log($"NPC '{npcID}'�� ȣ������ {value}��ŭ ����Ǿ� ���� ȣ������ {data.playerAffection}�Դϴ�.");
        }
        else
        {
            Debug.LogWarning($"NPC ID '{npcID}'�� ã�� �� ���� ȣ������ ������ �� �����ϴ�.");
        }
    }

    // �� �ܿ� ���� �� �ε� ���� �޼��� �߰� ����
    // public void SaveNPCData() { ... }
    // public void LoadNPCData() { ... }
}