using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 플레이어의 퀘스트 진행 상태를 관리하는 싱글턴 클래스입니다.
/// QuestManager는 퀘스트 수락, 완료, 그리고 진행 상황 추적을 담당합니다.
/// SOLID: 단일 책임 원칙 (퀘스트 진행 상태 관리).
/// </summary>

// QuestManager.Instance.UpdateQuestProgress(acceptedQuestID, monsterID); // 몬스터 처치 시 호출 예시

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
            LoadAllQuestData();
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
            acceptedQuests.Remove(questID);
            questProgress.Remove(questID);

            // 💡 추가된 로직: 아이템 수집 퀘스트의 아이템 차감
            // 완료 조건 중 아이템 수집 조건이 있다면 인벤토리에서 아이템을 제거합니다.
            foreach (var condition in questData.conditions)
            {
                if (condition.conditionType == QuestCondition.ConditionType.CollectItems)
                {
                    if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
                    {
                        // InventoryManager의 RemoveItem 메서드를 호출하여 아이템을 차감합니다.
                        PlayerCharacter.Instance.inventoryManager.RemoveItem(condition.targetID, condition.requiredAmount);
                    }
                    else
                    {
                        Debug.LogError("플레이어 또는 인벤토리 매니저가 없어 퀘스트 아이템을 차감할 수 없습니다.");
                    }
                }
            }

            // 반복 퀘스트 기능 추가:
            // 퀘스트가 반복 가능하지 않다면 completedQuests에 추가하여 다시 수락할 수 없게 만듭니다.
            if (!questData.isRepeatable)
            {
                completedQuests.Add(questID);
            }

            // 💡 호감도 보상 지급 로직 추가
            // 퀘스트 데이터에 호감도 보상 값이 설정되어 있고, NPC 매니저가 존재할 경우
            if (questData.affectionReward > 0)
            {
                if (NPCManager.Instance != null)
                {
                    // 퀘스트를 준 NPC에게 호감도 보상을 지급하도록 요청합니다.
                    // SOLID: 단일 책임 원칙에 따라 호감도 변경 로직은 NPCManager에 위임합니다.
                    NPCManager.Instance.ChangeAffection(questData.questGiverName, questData.affectionReward);
                }
                else
                {
                    Debug.LogWarning("NPCManager 인스턴스를 찾을 수 없어 호감도 보상을 지급할 수 없습니다.");
                }
            }

            // 보상 지급
            GiveQuestRewards(questData);
        }
    }

    /// <summary>
    /// 퀘스트 보상을 플레이어에게 지급하는 메서드입니다.
    /// 장비 아이템의 경우 등급을 부여하여 생성합니다.
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
        }

        // 골드 지급
        if (questData.goldReward > 0)
        {
            player.playerStats.gold += questData.goldReward;
        }

        // 아이템 지급 (장비 아이템은 등급을 부여하여 생성)
        if (questData.rewardItems != null && questData.rewardItems.Count > 0)
        {
            foreach (var reward in questData.rewardItems)
            {
                // 보상 아이템이 장비 아이템인지 확인합니다.
                EquipmentItemSO equipItem = reward.itemSO as EquipmentItemSO;
                if (equipItem != null)
                {
                    // 장비 아이템인 경우, ItemGenerator를 통해 퀘스트 데이터에 명시된 등급으로 생성합니다.
                    if (ItemGenerator.Instance != null)
                    {
                        EquipmentItemSO newEquipItem = ItemGenerator.Instance.GenerateItem(equipItem, questData.rewardEquipmentGrade);
                        if (newEquipItem != null)
                        {
                            player.inventoryManager.AddItem(newEquipItem, reward.itemCount);
                        }
                    }
                    else
                    {
                        Debug.LogError("ItemGenerator 인스턴스가 없어 보상 장비를 생성할 수 없습니다.");
                    }
                }
                else
                {
                    // 장비 아이템이 아닌 경우, 기존 로직대로 아이템을 복제하여 추가합니다.
                    BaseItemSO newItem = Instantiate(reward.itemSO);
                    player.inventoryManager.AddItem(newItem, reward.itemCount);
                }
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
    // 신규 추가된 메서드 및 수정
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 주어진 퀘스트 ID의 현재 상태를 반환합니다.
    /// </summary>
    /// <param name="questID">확인할 퀘스트의 ID.</param>
    /// <param name="currentAffection">현재 NPC의 호감도.</param> // 💡추가된 매개변수
    /// <returns>
    /// QuestState 열거형 값 (Available, Accepted, ReadyToComplete, Completed, None 등).
    /// </returns>
    public QuestState GetQuestState(int questID, int currentAffection) // 💡수정: 매개변수 추가
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
            // 반복 퀘스트 기능 추가:
            // 퀘스트가 반복 가능하다면 'Available' 상태로 반환하여 다시 수락할 수 있게 합니다.
            if (data.isRepeatable)
            {
                return QuestState.Available;
            }
            return QuestState.Completed; // 일반 퀘스트는 완료 상태 유지
        }

        // 2. 수락한 퀘스트인지 확인합니다.
        if (acceptedQuests.Contains(questID))
        {
            // 퀘스트 완료 조건을 모두 충족했는지 확인합니다.
            if (CheckQuestCompletion(data))
            {
                return QuestState.ReadyToComplete;
            }
            else
            {
                return QuestState.Accepted;
            }
        }

        // 3. 수락하지 않은 퀘스트라면, 퀘스트 수락 조건(선행 퀘스트, 호감도)이 충족되었는지 확인합니다.
        // 선행 퀘스트가 모두 완료 상태라면 호감도 조건을 확인합니다.
        if (data.prerequisiteQuests.All(prereqID => completedQuests.Contains(prereqID)))
        {
            // 추가된 로직: 호감도 조건 확인
            if (currentAffection >= data.requiredAffection)
            {
                return QuestState.Available;
            }
            else
            {
                return QuestState.Unavailable; // 💡 변경: 호감도 부족 시 'Unavailable' 반환
            }
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
    // 추가된 메서드
    /// <summary>
    /// 특정 퀘스트의 현재 진행 상황을 요약하여 문자열로 반환합니다.
    /// 몬스터 처치, 아이템 수집 등 모든 유형의 조건을 통합 처리합니다.
    /// SOLID: 단일 책임 원칙 (진행 상황 데이터 제공).
    /// </summary>
    /// <param name="questID">확인할 퀘스트의 ID.</param>
    /// <returns>퀘스트 진행 상황을 담은 문자열 (예: "몬스터 처치 (5/10)").</returns>
    public string GetQuestProgressText(int questID)
    {
        // 퀘스트 데이터가 캐시에 없으면 null 반환
        if (!questDataCache.ContainsKey(questID))
        {
            return "퀘스트 데이터 없음";
        }

        QuestData data = questDataCache[questID];
        string progressSummary = "";

        // 퀘스트 완료 조건을 모두 만족했는지 확인
        bool isCompleted = CheckQuestCompletion(data);

        // 완료 상태에 따라 접두사 추가
        progressSummary += isCompleted ? "[완료 가능] " : "[진행 중] ";

        // 퀘스트 진행 상황 텍스트 생성
        if (questProgress.TryGetValue(questID, out var currentProgress))
        {
            // 퀘스트의 모든 조건을 순회하며 진행 상황을 조합
            foreach (var condition in data.conditions)
            {
                int currentAmount = 0;
                if (currentProgress.progress.ContainsKey(condition.targetID))
                {
                    currentAmount = currentProgress.progress[condition.targetID];
                }

                // 조건 유형에 따라 텍스트를 조합합니다.
                switch (condition.conditionType)
                {
                    case QuestCondition.ConditionType.DefeatMonsters:
                        progressSummary += $"몬스터 처치 ({currentAmount}/{condition.requiredAmount})";
                        break;
                    case QuestCondition.ConditionType.CollectItems:
                        // 아이템 퀘스트는 인벤토리에서 직접 수량을 확인해야 합니다.
                        int itemCount = PlayerCharacter.Instance.inventoryManager.GetItemCount(condition.targetID);
                        progressSummary += $"아이템 수집 ({itemCount}/{condition.requiredAmount})";
                        break;
                    case QuestCondition.ConditionType.TalkToNPC:
                        // 대화 퀘스트는 보통 한 번만 수행하므로 완료/미완료만 표시
                        progressSummary += $"NPC와 대화 ({currentAmount}/{condition.requiredAmount})";
                        break;
                }
                // 여러 조건이 있을 경우 줄바꿈
                if (data.conditions.Count > 1 && data.conditions.Last() != condition)
                {
                    progressSummary += "\n";
                }
            }
        }
        else
        {
            // 진행 데이터가 없을 경우
            progressSummary += "진행 데이터가 없습니다.";
        }

        return progressSummary;
    }
    // 추가된 메서드
    /// <summary>
    /// 특정 ID의 QuestData를 반환합니다.
    /// </summary>
    /// <param name="questID">찾을 퀘스트의 ID.</param>
    /// <returns>해당 QuestData 객체, 없으면 null.</returns>
    public QuestData GetQuestData(int questID)
    {
        if (questDataCache.ContainsKey(questID))
        {
            return questDataCache[questID];
        }
        return null;
    }
    // 추가될 메서드
    /// <summary>
    /// 플레이어가 현재 수락한 모든 퀘스트의 ID 목록을 반환합니다.
    /// </summary>
    public List<int> GetAcceptedQuests()
    {
        return acceptedQuests;
    }
}