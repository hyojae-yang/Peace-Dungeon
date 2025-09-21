using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �÷��̾��� ����Ʈ �г� UI�� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// �� ��ũ��Ʈ�� �г��� ���� �ݴ� ��ɰ� ����Ʈ ����� �������� ������Ʈ�ϴ� å���� �����ϴ�.
/// SOLID: ���� å�� ��Ģ (UI �Ѱ� ����).
/// </summary>
public class PlayerQuestPanelController : MonoBehaviour
{
    public static PlayerQuestPanelController Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("����Ʈ �г� ��ü�� ��� �θ� GameObject�Դϴ�.")]
    [SerializeField]
    private GameObject questPanel;
    [Tooltip("����Ʈ �׸���� ��ġ�� ��ũ�Ѻ��� Content Transform�Դϴ�.")]
    [SerializeField]
    private Transform questListContent;
    [Tooltip("���� ����Ʈ �׸��� �����ϴ� �� ����� �������Դϴ�.")]
    [SerializeField]
    private GameObject playerQuestItemPrefab;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
            // �г��� ���� �� ��Ȱ��ȭ ���·� ����
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ����Ʈ �г��� Ȱ��ȭ/��Ȱ��ȭ�ϰ� UI�� ������Ʈ�մϴ�.
    /// �� �޼���� UI ��ư�� OnClick() �̺�Ʈ�� ����˴ϴ�.
    /// </summary>
    public void ToggleQuestPanel()
    {
        bool isActive = questPanel.activeSelf;
        questPanel.SetActive(!isActive);

        // �г��� Ȱ��ȭ�� ���� ����Ʈ ����� ������Ʈ�մϴ�.
        if (!isActive)
        {
            UpdateQuestList();
        }
    }

    /// <summary>
    /// �÷��̾ ������ ����Ʈ ����� QuestManager�κ��� ������ UI�� ������Ʈ�մϴ�.
    /// </summary>
    // ������ �߻��ߴ� UpdateQuestList() �޼���
    private void UpdateQuestList()
    {
        // 1. ������ ������ ��� ����Ʈ �׸� ����
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // 2. QuestManager���� ������ ����Ʈ ����� �����ɴϴ�.
        // ������ �κ�: .Select(q => q.questID)�� �����ϰ�
        // QuestManager.GetAcceptedQuests()�� ��ȯ�ϴ� int ����Ʈ�� �״�� ����մϴ�.
        List<int> acceptedQuestIDs = QuestManager.Instance.GetAcceptedQuests();

        // 3. �� ����Ʈ�� ���� UI �׸� ����
        foreach (int questID in acceptedQuestIDs)
        {
            QuestData questData = QuestManager.Instance.GetQuestData(questID); // ����Ʈ ������ ��������

            // �����Ͱ� ��ȿ���� Ȯ��
            if (questData == null)
            {
                Debug.LogWarning($"QuestID '{questID}'�� ���� QuestData�� ã�� �� �����ϴ�.");
                continue;
            }

            // ������ �ν��Ͻ�ȭ
            GameObject questItemObj = Instantiate(playerQuestItemPrefab, questListContent);
            PlayerQuestItem questItem = questItemObj.GetComponent<PlayerQuestItem>();

            // ����Ʈ ���� ��Ȳ �ؽ�Ʈ ��������
            string progressText = QuestManager.Instance.GetQuestProgressText(questID);

            // UI�� ���� ����
            if (questItem != null)
            {
                questItem.SetQuestInfo(
                    questData.questTitle,
                    questData.questGiverName,
                    progressText);
            }
        }
    }
}