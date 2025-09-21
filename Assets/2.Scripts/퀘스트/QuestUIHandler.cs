using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// ����Ʈ UI �г��� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ����Ʈ ����� �������� �����ϰ�, ���õ� ����Ʈ�� �� ������ ǥ���մϴ�.
/// SOLID: ���� å�� ��Ģ (����Ʈ UI ����).
/// </summary>
public class QuestUIHandler : MonoBehaviour
{
    // --- UI ���� ���� ---
    [Header("UI References")]
    [Tooltip("����Ʈ ��ư���� ������ �θ� Transform�Դϴ�.")]
    [SerializeField] private Transform questButtonContainer;
    [Tooltip("����Ʈ ��ư �������� �Ҵ��մϴ�.")]
    [SerializeField] private GameObject questButtonPrefab;
    [Tooltip("����Ʈ �� ������ ǥ���� �г��Դϴ�.")]
    [SerializeField] private GameObject questDetailPanel;
    [Tooltip("�� ���� �г��� ����Ʈ�� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [Tooltip("�� ���� �г��� NPC�� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [Tooltip("�� ���� �г��� ����Ʈ ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [Tooltip("�� ���� �г��� ���� ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI questProgressText;

    // --- ���� ���� ---
    // ������ ����Ʈ ��ư ������Ʈ���� ������ ����Ʈ
    private List<GameObject> activeQuestButtons = new List<GameObject>();
    // ���� �� ������ ǥ�� ���� ����Ʈ�� ID
    private int currentSelectedQuestID = -1;

    // --- MonoBehaviour �޼��� ---
    private void Awake()
    {
        // ����Ʈ �Ŵ��� �ν��Ͻ��� �����Ͽ� ����Ʈ ���� �̺�Ʈ�� �����մϴ�.
        // ����Ʈ�� �����ǰų� �Ϸ�� ������ UI�� �����ϱ� �����Դϴ�.
        if (QuestManager.Instance != null)
        {
            // TODO: QuestManager�� ����Ʈ ���� ���� �̺�Ʈ (OnQuestAccepted, OnQuestCompleted)��
            // �߰��ϰ�, �� �޼������ �ش� �̺�Ʈ�� �����Ͽ� �ڵ� ���� ������ ������ �� �ֽ��ϴ�.
        }
    }

    /// <summary>
    /// ���� ������Ʈ�� Ȱ��ȭ�� ������ ȣ��Ǵ� �޼����Դϴ�.
    /// ����Ʈ UI�� ���� ������ ����� �ֽ� ���·� �����մϴ�.
    /// </summary>
    private void OnEnable()
    {
        // UI�� Ȱ��ȭ�Ǹ� �ٷ� ����Ʈ ����� �����մϴ�.
        UpdateQuestList();
    }

    /// <summary>
    /// �÷��̾ ������ ����Ʈ ����� UI�� �����մϴ�.
    /// ���� ��ư�� ��� �ı��ϰ� ���� �����մϴ�.
    /// </summary>
    public void UpdateQuestList()
    {
        // ������ ������ ��� ����Ʈ ��ư�� ����
        foreach (var button in activeQuestButtons)
        {
            Destroy(button);
        }
        activeQuestButtons.Clear();

        // QuestManager�κ��� ���� ������ ��� ����Ʈ ID�� ������
        var acceptedQuestIDs = QuestManager.Instance.GetAcceptedQuests();

        if (acceptedQuestIDs == null || acceptedQuestIDs.Count == 0)
        {
            questDetailPanel.SetActive(false);
            return;
        }

        foreach (var questID in acceptedQuestIDs)
        {
            QuestData questData = QuestManager.Instance.GetQuestData(questID);
            if (questData != null)
            {
                // ����Ʈ ��ư �������� �������� ����
                GameObject newButtonObj = Instantiate(questButtonPrefab, questButtonContainer);
                activeQuestButtons.Add(newButtonObj);

                // ��ư�� �ؽ�Ʈ�� Ŭ�� �̺�Ʈ ����
                TextMeshProUGUI buttonText = newButtonObj.GetComponentInChildren<TextMeshProUGUI>();
                Button button = newButtonObj.GetComponent<Button>();

                if (buttonText != null)
                {
                    buttonText.text = questData.questTitle;
                }

                if (button != null)
                {
                    // ���ٽ��� ����Ͽ� ��ư Ŭ�� �� �ش� ����Ʈ ID�� ����
                    button.onClick.AddListener(() => OnQuestButtonClick(questID));
                }
            }
        }

        // ��ư�� �ϳ��� ������ ù ��° ��ư�� �� ���� ǥ��
        OnQuestButtonClick(acceptedQuestIDs[0]);
    }

    /// <summary>
    /// ����Ʈ ��ư Ŭ�� �� ȣ��˴ϴ�. �� ���� �г��� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="questID">���õ� ����Ʈ�� ID.</param>
    public void OnQuestButtonClick(int questID)
    {
        // ���� ���õ� ����Ʈ ID�� ������Ʈ
        currentSelectedQuestID = questID;

        // �� �г� Ȱ��ȭ
        questDetailPanel.SetActive(true);

        // ����Ʈ ������ ��������
        QuestData questData = QuestManager.Instance.GetQuestData(questID);
        if (questData == null)
        {
            Debug.LogError($"����Ʈ ID {questID}�� �����͸� ã�� �� �����ϴ�.");
            return;
        }

        // UI �ؽ�Ʈ ������Ʈ
        questNameText.text = questData.questTitle;
        npcNameText.text = $"�Ƿ���: {questData.questGiverName}";
        questDescriptionText.text = questData.questDescription;

        // QuestManager�� GetQuestProgressText() �޼��带 ȣ���Ͽ� ���� ���¸� �����ɴϴ�.
        questProgressText.text = QuestManager.Instance.GetQuestProgressText(questID);
    }
}