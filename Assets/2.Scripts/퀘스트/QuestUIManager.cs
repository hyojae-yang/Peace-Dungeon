using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// ����Ʈ ���� UI(����Ʈ ��� �г�)�� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// NPCUIManager�� '����Ʈ' ��ư�� �����˴ϴ�.
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance { get; private set; }

    [Header("Quest List UI")]
    [Tooltip("����Ʈ ����� ǥ���ϴ� �г��Դϴ�.")]
    [SerializeField]
    private GameObject questListPanel;
    [Tooltip("����Ʈ �׸��� �������Դϴ�. �� �������� �ν��Ͻ�ȭ�Ͽ� ����� ����ϴ�.")]
    [SerializeField]
    private GameObject questListItemPrefab;
    [Tooltip("�������� ������ ����Ʈ �׸��� ��ġ�� �θ� ������Ʈ�Դϴ�.")]
    [SerializeField]
    private Transform questListContent;

    // ���� ��ȣ�ۿ� ���� NPC�� QuestGiver ������Ʈ ����
    private QuestGiver currentQuestGiver;
    // ����Ʈ ���� �� ����� �ݹ� �Լ�
    private Action<QuestData, QuestState> onQuestSelectedCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ����Ʈ ��� �г��� Ȱ��ȭ�ϰ� ����Ʈ ����� ǥ���մϴ�.
    /// �� �޼���� NPCUIManager�� '����Ʈ' ��ư�� �����˴ϴ�.
    /// </summary>
    /// <param name="questGiver">����� ������ QuestGiver ������Ʈ.</param>
    /// <param name="callback">����Ʈ ���� �� ����� �ݹ� �Լ�.</param>
    public void ShowQuestList(QuestGiver questGiver, Action<QuestData, QuestState> callback)
    {
        if (questGiver == null)
        {
            Debug.LogError("QuestGiver ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        currentQuestGiver = questGiver;
        onQuestSelectedCallback = callback;

        // ���� ����Ʈ �׸� ��� ����
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // ����Ʈ ����� �������� ����
        List<QuestData> questDatas = currentQuestGiver.GetQuestDatas();
        foreach (QuestData data in questDatas)
        {
            // QuestManager�� ���� ���� ����Ʈ ���¸� Ȯ���մϴ�.
            QuestState state = QuestManager.Instance.GetQuestState(data.questID);

            // "���� ����" ����, "���� ��" ����, "�Ϸ� ����" ������ ����Ʈ�� ��Ͽ� ǥ���մϴ�.
            if (state == QuestState.Available || state == QuestState.Accepted || state == QuestState.Complete)
            {
                GameObject listItem = Instantiate(questListItemPrefab, questListContent);
                QuestListItem listItemScript = listItem.GetComponent<QuestListItem>();

                // ����Ʈ �׸� UI ������Ʈ
                listItemScript.SetQuestData(data, state);

                // ��ư Ŭ�� �̺�Ʈ ����
                listItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnQuestSelected(data, state));
            }
        }
        // ����Ʈ ��� UI Ȱ��ȭ
        questListPanel.SetActive(true);
    }

    /// <summary>
    /// ����Ʈ ��� �г��� ��Ȱ��ȭ�ϰ� �ݹ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void HideQuestList()
    {
        questListPanel.SetActive(false);
        currentQuestGiver = null;
        onQuestSelectedCallback = null;
    }

    /// <summary>
    /// ����Ʈ ��Ͽ��� Ư�� ����Ʈ �׸��� �������� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="questData">���õ� ����Ʈ�� ������.</param>
    /// <param name="state">���õ� ����Ʈ�� ���� ����.</param>
    private void OnQuestSelected(QuestData questData, QuestState state)
    {
        // ����Ʈ ��� �г��� ����ϴ�.
        HideQuestList();

        // ���õ� ����Ʈ ������ ���¸� NPCUIManager�� �ٽ� �����մϴ�.
        onQuestSelectedCallback?.Invoke(questData, state);
    }
}