using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �÷��̾��� ����Ʈ ���� ���¸� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// QuestManager�� ����Ʈ ����, �Ϸ�, �׸��� ���� ��Ȳ ������ ����մϴ�.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // �÷��̾ ������ ����Ʈ ���
    private List<int> acceptedQuests = new List<int>();
    // �÷��̾ �Ϸ��� ����Ʈ ���
    private List<int> completedQuests = new List<int>();
    // ����Ʈ ���� ��Ȳ�� �����ϴ� ��ųʸ�.
    // Ű: ����Ʈ ID, ��: ����Ʈ ��ǥ �޼� ��Ȳ
    private Dictionary<int, QuestProgress> questProgress = new Dictionary<int, QuestProgress>();

    // ����Ʈ �����͵��� ĳ���ϴ� ��ųʸ�.
    // Ű: ����Ʈ ID, ��: QuestData ScriptableObject
    private Dictionary<int, QuestData> questDataCache = new Dictionary<int, QuestData>();

    /// <summary>
    /// ����Ʈ�� ���� ��Ȳ�� �����ϱ� ���� ���� Ŭ����.
    /// </summary>
    [System.Serializable]
    public class QuestProgress
    {
        // ����Ʈ �Ϸ� ������ �����ϴ� ��ųʸ�.
        // Ű: ������ Ÿ�� ID (���� ID, ������ ID ��), ��: ���� �޼� Ƚ��
        public Dictionary<int, int> progress = new Dictionary<int, int>();
    }

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
    /// Ư�� ����Ʈ�� ���� ������ �������� Ȯ���մϴ�.
    /// </summary>
    public bool IsQuestAccepted(int questID)
    {
        return acceptedQuests.Contains(questID);
    }

    /// <summary>
    /// Ư�� ����Ʈ�� �̹� �Ϸ�� �������� Ȯ���մϴ�.
    /// </summary>
    public bool IsQuestCompleted(int questID)
    {
        return completedQuests.Contains(questID);
    }

    /// <summary>
    /// ����Ʈ�� �����մϴ�. ����Ʈ ���� ��Ȳ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void AcceptQuest(int questID)
    {
        if (!IsQuestAccepted(questID) && !IsQuestCompleted(questID))
        {
            acceptedQuests.Add(questID);
            questProgress[questID] = new QuestProgress();
            Debug.Log($"����Ʈ '{questID}'�� �����Ǿ����ϴ�.");
        }
    }

    /// <summary>
    /// ����Ʈ�� ����մϴ�. ����Ʈ ���� ��Ȳ�� �����մϴ�.
    /// </summary>
    public void CancelQuest(int questID)
    {
        if (IsQuestAccepted(questID))
        {
            acceptedQuests.Remove(questID);
            if (questProgress.ContainsKey(questID))
            {
                questProgress.Remove(questID);
            }
            Debug.Log($"����Ʈ '{questID}'�� ��ҵǾ����ϴ�.");
        }
    }

    /// <summary>
    /// ����Ʈ�� �Ϸ� ó���ϰ� ������ �����մϴ�.
    /// </summary>
    /// <param name="questID">�Ϸ��� ����Ʈ�� ID.</param>
    /// <param name="questData">�Ϸ��� ����Ʈ�� ������.</param>
    public void CompleteQuest(int questID, QuestData questData)
    {
        // ��ȿ�� �˻�: ����Ʈ�� ������ �����̰�, ���� �Ϸ���� �ʾҴ��� Ȯ��
        if (IsQuestAccepted(questID) && !IsQuestCompleted(questID))
        {
            // ����Ʈ �Ϸ� ó��
            completedQuests.Add(questID);
            acceptedQuests.Remove(questID);
            questProgress.Remove(questID);

            Debug.Log($"����Ʈ '{questID}'�� �Ϸ�Ǿ����ϴ�!");

            // ���� ����
            GiveQuestRewards(questData);
        }
    }

    /// <summary>
    /// ����Ʈ ������ �÷��̾�� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="questData">���� ������ ��� ����Ʈ ������.</param>
    private void GiveQuestRewards(QuestData questData)
    {
        if (questData == null)
        {
            Debug.LogError("����Ʈ �����Ͱ� ���� ������ ������ �� �����ϴ�.");
            return;
        }

        // �÷��̾� ĳ���� �ν��Ͻ��� ����
        var player = PlayerCharacter.Instance;
        if (player == null)
        {
            Debug.LogError("�÷��̾� ĳ���� �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // ����ġ ����
        if (questData.experienceReward > 0)
        {
            player.playerLevelUp.AddExperience(questData.experienceReward);
            Debug.Log($"{questData.experienceReward} ����ġ�� ������ϴ�.");
        }

        // ��� ����
        if (questData.goldReward > 0)
        {
            player.playerStats.gold += questData.goldReward;
            Debug.Log($"{questData.goldReward} ��带 ������ϴ�.");
        }

        // ������ ���� (TODO: InventoryManager�� ������ �߰��ϴ� ���� ���� �ʿ�)
        if (questData.rewardItems != null && questData.rewardItems.Count > 0)
        {
            foreach (var reward in questData.rewardItems)
            {
                // player.inventoryManager.AddItem(reward.itemID, reward.itemCount);
                Debug.Log($"{reward.itemCount}���� ������ ID '{reward.itemID}'�� ������ϴ�.");
            }
        }
    }

    /// <summary>
    /// ����Ʈ�� ���� ��Ȳ�� ������Ʈ�մϴ�.
    /// ���� óġ, ������ ȹ�� ���� �̺�Ʈ���� ȣ��˴ϴ�.
    /// </summary>
    public void UpdateQuestProgress(int questID, int targetID, int amount = 1)
    {
        if (questProgress.ContainsKey(questID))
        {
            QuestProgress progress = questProgress[questID];
            if (!progress.progress.ContainsKey(targetID))
            {
                progress.progress[targetID] = 0;
            }
            progress.progress[targetID] += amount;
            Debug.Log($"����Ʈ '{questID}'�� '{targetID}' ��ǥ ���� ��Ȳ: {progress.progress[targetID]}");
        }
    }

    /// <summary>
    /// �÷��̾ ����Ʈ�� ��� �Ϸ� ������ �����ߴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="questData">Ȯ���� ����Ʈ ������.</param>
    /// <returns>��� ������ �����Ǹ� true, �ƴϸ� false.</returns>
    public bool CheckQuestCompletion(QuestData questData)
    {
        if (questData == null) return false;
        if (!IsQuestAccepted(questData.questID)) return false;

        if (!questProgress.TryGetValue(questData.questID, out var currentProgress))
        {
            return false;
        }

        foreach (var condition in questData.conditions)
        {
            int currentAmount = 0;
            if (currentProgress.progress.ContainsKey(condition.targetID))
            {
                currentAmount = currentProgress.progress[condition.targetID];
            }

            switch (condition.conditionType)
            {
                case QuestCondition.ConditionType.CollectItems:
                    // PlayerCharacter�� ���� InventoryManager�� ����
                    if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
                    {
                        if (!PlayerCharacter.Instance.inventoryManager.HasItem(condition.targetID, condition.requiredAmount))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogError("PlayerCharacter �Ǵ� InventoryManager�� ã�� �� �����ϴ�.");
                        return false;
                    }
                    break;
                case QuestCondition.ConditionType.TalkToNPC:
                case QuestCondition.ConditionType.DefeatMonsters:
                    if (currentAmount < condition.requiredAmount)
                    {
                        return false;
                    }
                    break;
            }
        }
        return true;
    }

    //----------------------------------------------------------------------------------------------------------------
    // �ű� �߰��� �޼���
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// �־��� ����Ʈ ID�� ���� ���¸� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="questID">Ȯ���� ����Ʈ�� ID.</param>
    /// <returns>
    /// QuestState ������ �� (Available, Accepted, Complete, Completed, None ��).
    /// </returns>
    public QuestState GetQuestState(int questID)
    {
        // ����Ʈ ������ ĳ�ð� ��������� �ʱ�ȭ�մϴ�.
        if (questDataCache.Count == 0)
        {
            LoadAllQuestData();
        }

        // �ش� ID�� ����Ʈ �����Ͱ� �ִ��� Ȯ���մϴ�.
        if (!questDataCache.ContainsKey(questID))
        {
            Debug.LogWarning($"QuestID '{questID}'�� �ش��ϴ� ����Ʈ �����Ͱ� �����ϴ�.");
            return QuestState.None;
        }

        // ����Ʈ ������ ��������
        QuestData data = questDataCache[questID];

        // 1. �̹� �Ϸ��� ����Ʈ���� Ȯ���մϴ�.
        if (completedQuests.Contains(questID))
        {
            return QuestState.Completed; // �̹� �Ϸ�� ����Ʈ
        }

        // 2. ������ ����Ʈ���� Ȯ���մϴ�.
        if (acceptedQuests.Contains(questID))
        {
            // ����Ʈ �Ϸ� ������ ��� �����ߴ��� Ȯ���մϴ�.
            if (CheckQuestCompletion(data))
            {
                return QuestState.Complete; // �Ϸ� ������ ����Ʈ
            }
            else
            {
                return QuestState.Accepted; // ���� ���� ����Ʈ
            }
        }

        // 3. �������� ���� ����Ʈ���, ����Ʈ ���� ����(���� ����Ʈ)�� �����Ǿ����� Ȯ���մϴ�.
        // ���� ����Ʈ�� ���ų�, ���� ����Ʈ�� ��� �Ϸ� ���¶�� 'Available'�Դϴ�.
        if (data.prerequisiteQuests.All(prereqID => completedQuests.Contains(prereqID)))
        {
            return QuestState.Available; // ���� ������ ����Ʈ
        }

        // ��� ���ǿ� �ش����� ������ ����Ʈ�� �������� �ʰų�, ���� ���� �Ұ����� �����Դϴ�.
        return QuestState.None;
    }

    /// <summary>
    /// ��� QuestData ScriptableObject�� ���ҽ� �������� ã�� ĳ���մϴ�.
    /// </summary>
    private void LoadAllQuestData()
    {
        QuestData[] allQuestData = Resources.LoadAll<QuestData>("Quests");
        foreach (var data in allQuestData)
        {
            if (!questDataCache.ContainsKey(data.questID))
            {
                questDataCache.Add(data.questID, data);
            }
        }
    }
}