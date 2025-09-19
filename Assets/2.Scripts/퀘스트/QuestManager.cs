using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어의 퀘스트 진행 상태를 관리하는 싱글턴 클래스입니다.
/// QuestManager는 퀘스트 수락, 완료, 그리고 진행 상황 추적을 담당합니다.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // 플레이어가 수락한 퀘스트 목록
    private List<int> acceptedQuests = new List<int>();
    // 플레이어가 완료한 퀘스트 목록
    private List<int> completedQuests = new List<int>();
    // 퀘스트 진행 상황을 추적하는 딕셔너리.
    // 키: 퀘스트 ID, 값: 퀘스트 목표 달성 상황
    private Dictionary<int, QuestProgress> questProgress = new Dictionary<int, QuestProgress>();

    // 퀘스트 데이터들을 캐싱하는 딕셔너리.
    // 키: 퀘스트 ID, 값: QuestData ScriptableObject
    private Dictionary<int, QuestData> questDataCache = new Dictionary<int, QuestData>();

    /// <summary>
    /// 퀘스트의 진행 상황을 저장하기 위한 내부 클래스.
    /// </summary>
    [System.Serializable]
    public class QuestProgress
    {
        // 퀘스트 완료 조건을 추적하는 딕셔너리.
        // 키: 조건의 타겟 ID (몬스터 ID, 아이템 ID 등), 값: 현재 달성 횟수
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
    /// 특정 퀘스트가 현재 수락된 상태인지 확인합니다.
    /// </summary>
    public bool IsQuestAccepted(int questID)
    {
        return acceptedQuests.Contains(questID);
    }

    /// <summary>
    /// 특정 퀘스트가 이미 완료된 상태인지 확인합니다.
    /// </summary>
    public bool IsQuestCompleted(int questID)
    {
        return completedQuests.Contains(questID);
    }

    /// <summary>
    /// 퀘스트를 수락합니다. 퀘스트 진행 상황을 초기화합니다.
    /// </summary>
    public void AcceptQuest(int questID)
    {
        if (!IsQuestAccepted(questID) && !IsQuestCompleted(questID))
        {
            acceptedQuests.Add(questID);
            questProgress[questID] = new QuestProgress();
            Debug.Log($"퀘스트 '{questID}'가 수락되었습니다.");
        }
    }

    /// <summary>
    /// 퀘스트를 취소합니다. 퀘스트 진행 상황을 제거합니다.
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
            Debug.Log($"퀘스트 '{questID}'가 취소되었습니다.");
        }
    }

    /// <summary>
    /// 퀘스트를 완료 처리하고 보상을 지급합니다.
    /// </summary>
    /// <param name="questID">완료할 퀘스트의 ID.</param>
    /// <param name="questData">완료할 퀘스트의 데이터.</param>
    public void CompleteQuest(int questID, QuestData questData)
    {
        // 유효성 검사: 퀘스트가 수락된 상태이고, 아직 완료되지 않았는지 확인
        if (IsQuestAccepted(questID) && !IsQuestCompleted(questID))
        {
            // 퀘스트 완료 처리
            completedQuests.Add(questID);
            acceptedQuests.Remove(questID);
            questProgress.Remove(questID);

            Debug.Log($"퀘스트 '{questID}'가 완료되었습니다!");

            // 보상 지급
            GiveQuestRewards(questData);
        }
    }

    /// <summary>
    /// 퀘스트 보상을 플레이어에게 지급하는 메서드입니다.
    /// </summary>
    /// <param name="questData">보상 정보가 담긴 퀘스트 데이터.</param>
    private void GiveQuestRewards(QuestData questData)
    {
        if (questData == null)
        {
            Debug.LogError("퀘스트 데이터가 없어 보상을 지급할 수 없습니다.");
            return;
        }

        // 플레이어 캐릭터 인스턴스에 접근
        var player = PlayerCharacter.Instance;
        if (player == null)
        {
            Debug.LogError("플레이어 캐릭터 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 경험치 지급
        if (questData.experienceReward > 0)
        {
            player.playerLevelUp.AddExperience(questData.experienceReward);
            Debug.Log($"{questData.experienceReward} 경험치를 얻었습니다.");
        }

        // 골드 지급
        if (questData.goldReward > 0)
        {
            player.playerStats.gold += questData.goldReward;
            Debug.Log($"{questData.goldReward} 골드를 얻었습니다.");
        }

        // 아이템 지급 (TODO: InventoryManager에 아이템 추가하는 로직 구현 필요)
        if (questData.rewardItems != null && questData.rewardItems.Count > 0)
        {
            foreach (var reward in questData.rewardItems)
            {
                // player.inventoryManager.AddItem(reward.itemID, reward.itemCount);
                Debug.Log($"{reward.itemCount}개의 아이템 ID '{reward.itemID}'를 얻었습니다.");
            }
        }
    }

    /// <summary>
    /// 퀘스트의 진행 상황을 업데이트합니다.
    /// 몬스터 처치, 아이템 획득 등의 이벤트에서 호출됩니다.
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
            Debug.Log($"퀘스트 '{questID}'의 '{targetID}' 목표 진행 상황: {progress.progress[targetID]}");
        }
    }

    /// <summary>
    /// 플레이어가 퀘스트의 모든 완료 조건을 충족했는지 확인합니다.
    /// </summary>
    /// <param name="questData">확인할 퀘스트 데이터.</param>
    /// <returns>모든 조건이 충족되면 true, 아니면 false.</returns>
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
                    // PlayerCharacter를 통해 InventoryManager에 접근
                    if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
                    {
                        if (!PlayerCharacter.Instance.inventoryManager.HasItem(condition.targetID, condition.requiredAmount))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogError("PlayerCharacter 또는 InventoryManager를 찾을 수 없습니다.");
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
    // 신규 추가된 메서드
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 주어진 퀘스트 ID의 현재 상태를 반환합니다.
    /// </summary>
    /// <param name="questID">확인할 퀘스트의 ID.</param>
    /// <returns>
    /// QuestState 열거형 값 (Available, Accepted, Complete, Completed, None 등).
    /// </returns>
    public QuestState GetQuestState(int questID)
    {
        // 퀘스트 데이터 캐시가 비어있으면 초기화합니다.
        if (questDataCache.Count == 0)
        {
            LoadAllQuestData();
        }

        // 해당 ID의 퀘스트 데이터가 있는지 확인합니다.
        if (!questDataCache.ContainsKey(questID))
        {
            Debug.LogWarning($"QuestID '{questID}'에 해당하는 퀘스트 데이터가 없습니다.");
            return QuestState.None;
        }

        // 퀘스트 데이터 가져오기
        QuestData data = questDataCache[questID];

        // 1. 이미 완료한 퀘스트인지 확인합니다.
        if (completedQuests.Contains(questID))
        {
            return QuestState.Completed; // 이미 완료된 퀘스트
        }

        // 2. 수락한 퀘스트인지 확인합니다.
        if (acceptedQuests.Contains(questID))
        {
            // 퀘스트 완료 조건을 모두 충족했는지 확인합니다.
            if (CheckQuestCompletion(data))
            {
                return QuestState.Complete; // 완료 가능한 퀘스트
            }
            else
            {
                return QuestState.Accepted; // 진행 중인 퀘스트
            }
        }

        // 3. 수락하지 않은 퀘스트라면, 퀘스트 수락 조건(선행 퀘스트)이 충족되었는지 확인합니다.
        // 선행 퀘스트가 없거나, 선행 퀘스트가 모두 완료 상태라면 'Available'입니다.
        if (data.prerequisiteQuests.All(prereqID => completedQuests.Contains(prereqID)))
        {
            return QuestState.Available; // 수락 가능한 퀘스트
        }

        // 모든 조건에 해당하지 않으면 퀘스트는 존재하지 않거나, 아직 진행 불가능한 상태입니다.
        return QuestState.None;
    }

    /// <summary>
    /// 모든 QuestData ScriptableObject를 리소스 폴더에서 찾아 캐시합니다.
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