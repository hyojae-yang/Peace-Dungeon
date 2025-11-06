using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 플레이어의 퀘스트 진행 상태를 관리하는 싱글턴 클래스입니다.
/// QuestManager는 퀘스트 수락, 완료, 그리고 진행 상황 추적을 담당합니다.
/// SOLID: 단일 책임 원칙 (퀘스트 진행 상태 관리).
/// </summary>
public class QuestManager : MonoBehaviour, ISavable
{
    // SOLID 원칙: 정적 인스턴스에 대한 접근 제어 및 클래스의 유일성 보장.
    public static QuestManager Instance { get; private set; }

    // 퀘스트 목록의 구조적 변경(수락/취소/완료)을 알리는 이벤트.
    public event Action OnQuestListChanged;

    // 특정 퀘스트의 진행 상황 업데이트를 알리는 이벤트. (UI 갱신용)
    public event Action<int> OnQuestProgressUpdated; // int: 업데이트된 퀘스트의 ID

    // 플레이어가 수락한 퀘스트 목록
    private List<int> acceptedQuests = new List<int>();
    // 플레이어가 완료한 퀘스트 목록
    private List<int> completedQuests = new List<int>();
    // 퀘스트 진행 상황을 추적하는 딕셔너리.
    private Dictionary<int, QuestProgress> questProgress = new Dictionary<int, QuestProgress>();

    // 퀘스트 데이터들을 캐싱하는 딕셔너리.
    private Dictionary<int, QuestData> questDataCache = new Dictionary<int, QuestData>();

    /// <summary>
    /// 퀘스트의 진행 상황을 저장하기 위한 내부 클래스.
    /// </summary>
    [System.Serializable]
    public class QuestProgress
    {
        // 퀘스트 완료 조건을 추적하는 딕셔너리. (키: 타겟 ID, 값: 현재 달성 횟수)
        public Dictionary<int, int> progress = new Dictionary<int, int>();
    }

    private void Awake()
    {
        // 싱글턴 인스턴스 초기화 및 중복 방지.
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

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }
        // 몬스터 처치 이벤트 구독
        MonsterBase.OnAnyMonsterKilled += HandleMonsterKilled;

        // [수정/핵심 추가] 인벤토리 변경 이벤트 구독: 아이템 수집 퀘스트의 실시간 갱신을 위함.
        if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
        {
            // InventoryManager에 OnInventoryChanged 이벤트가 있다고 가정하고 구독합니다.
            PlayerCharacter.Instance.inventoryManager.onInventoryChanged += UpdateQuestsOnInventoryChange;
        }
    }

    /// <summary>
    /// 퀘스트 매니저가 파괴될 때, 구독했던 정적 및 인스턴스 이벤트에서 반드시 해제합니다.
    /// 메모리 누수를 방지하기 위함입니다.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            MonsterBase.OnAnyMonsterKilled -= HandleMonsterKilled;

            // [수정/핵심 추가] 인벤토리 변경 이벤트 구독 해제.
            if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
            {
                PlayerCharacter.Instance.inventoryManager.onInventoryChanged -= UpdateQuestsOnInventoryChange;
            }
        }
    }

    /// <summary>
    /// MonsterBase.OnAnyMonsterKilled 이벤트 발생 시 호출되는 핸들러 메서드입니다.
    /// </summary>
    /// <param name="monsterID">사망한 몬스터의 고유 ID.</param>
    private void HandleMonsterKilled(int monsterID)
    {
        // 이벤트 핸들러는 중계만 하고, 실제 업데이트 책임은 분리된 메서드에 위임합니다.
        UpdateQuestsOnMonsterDefeat(monsterID);
    }

    /// <summary>
    /// **[신규]** 인벤토리 변경 이벤트 발생 시 호출되어 아이템 수집 퀘스트를 업데이트합니다.
    /// 아이템 퀘스트는 InventoryManager에서 실시간으로 수량을 체크하므로, 여기서는 UI 갱신 신호만 보냅니다.
    /// SOLID 원칙: 개방-폐쇄 원칙 (InventoryManager 변경 없이 퀘스트 기능 확장).
    /// </summary>
    public void UpdateQuestsOnInventoryChange()
    {
        // 수락된 퀘스트 목록을 순회합니다.
        foreach (int questID in acceptedQuests.ToList())
        {
            if (questDataCache.TryGetValue(questID, out QuestData data))
            {
                // 아이템 수집 퀘스트가 있는지 빠르게 확인합니다.
                bool hasCollectItemsCondition = data.conditions
                    .Any(condition => condition.conditionType == QuestCondition.ConditionType.CollectItems);

                if (hasCollectItemsCondition)
                {
                    // 진행 상황 데이터는 QuestManager에 저장되지 않지만,
                    // UI(PlayerQuestPanelController)가 CheckQuestCompletion() 및 GetQuestProgressText()를 호출하여
                    // InventoryManager의 최신 데이터를 읽어가도록 이벤트를 발생시킵니다.
                    OnQuestProgressUpdated?.Invoke(questID);
                }
            }
        }
    }

    /// <summary>
    /// **핵심 로직:** 몬스터 처치 시 호출되어 모든 수락된 퀘스트를 순회하며 진행 상황을 업데이트합니다.
    /// </summary>
    /// <param name="monsterID">사망한 몬스터의 고유 ID입니다. 이 ID로 퀘스트 조건을 확인합니다.</param>
    public void UpdateQuestsOnMonsterDefeat(int monsterID)
    {
        // 1. 플레이어가 현재 수락한 모든 퀘스트를 순회합니다.
        foreach (int questID in acceptedQuests.ToList())
        {
            // 2. 퀘스트 데이터 가져오기.
            if (questDataCache.TryGetValue(questID, out QuestData data))
            {
                // 3. 퀘스트의 모든 조건을 순회하며 몬스터 처치 조건이 있는지 확인합니다.
                foreach (var condition in data.conditions)
                {
                    if (condition.conditionType == QuestCondition.ConditionType.DefeatMonsters &&
                        condition.targetID == monsterID)
                    {
                        // 4. 조건이 충족되면, 카운트를 1 증가시키고 이벤트를 호출합니다.
                        UpdateQuestProgress(questID, monsterID, 1);
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"QuestID {questID}에 대한 QuestData를 캐시에서 찾을 수 없습니다.");
            }
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

            // 퀘스트 목록 변경 UI 갱신 신호를 보냅니다.
            OnQuestListChanged?.Invoke();
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

            // 퀘스트 목록 변경 UI 갱신 신호를 보냅니다.
            OnQuestListChanged?.Invoke();
        }
    }

    /// <summary>
    /// 퀘스트를 완료 처리하고 보상을 지급합니다.
    /// </summary>
    /// <param name="questID">완료할 퀘스트의 ID.</param>
    /// <param name="questData">완료할 퀘스트의 데이터.</param>
    public void CompleteQuest(int questID, QuestData questData)
    {
        // 유효성 검사
        if (IsQuestAccepted(questID) && !IsQuestCompleted(questID))
        {
            // 아이템 수집 퀘스트의 아이템 차감 로직
            foreach (var condition in questData.conditions)
            {
                if (condition.conditionType == QuestCondition.ConditionType.CollectItems)
                {
                    if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
                    {
                        PlayerCharacter.Instance.inventoryManager.RemoveItem(condition.targetID, condition.requiredAmount);
                    }
                    else
                    {
                        Debug.LogError("플레이어 또는 인벤토리 매니저가 없어 퀘스트 아이템을 차감할 수 없습니다.");
                    }
                }
            }

            // 퀘스트 완료 처리
            acceptedQuests.Remove(questID);
            questProgress.Remove(questID);

            // 반복 퀘스트가 아니라면 완료 목록에 추가
            if (!questData.isRepeatable)
            {
                completedQuests.Add(questID);
            }

            // 호감도 보상 지급 로직 (책임 위임)
            if (questData.affectionReward > 0)
            {
                if (NPCManager.Instance != null)
                {
                    NPCManager.Instance.ChangeAffection(questData.questGiverName, questData.affectionReward);
                }
                else
                {
                    Debug.LogWarning("NPCManager 인스턴스를 찾을 수 없어 호감도 보상을 지급할 수 없습니다.");
                }
            }

            // 보상 지급
            GiveQuestRewards(questData);

            // 퀘스트 목록 UI 갱신 신호를 보냅니다.
            OnQuestListChanged?.Invoke();
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

        var player = PlayerCharacter.Instance;
        if (player == null)
        {
            Debug.LogError("플레이어 캐릭터 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 경험치 및 골드 지급
        if (questData.experienceReward > 0)
        {
            player.playerLevelUp.AddExperience(questData.experienceReward);
        }

        if (questData.goldReward > 0)
        {
            player.playerStats.gold += questData.goldReward;
        }

        // 아이템 지급 로직
        if (questData.rewardItems != null && questData.rewardItems.Count > 0)
        {
            foreach (var reward in questData.rewardItems)
            {
                EquipmentItemSO equipItem = reward.itemSO as EquipmentItemSO;
                if (equipItem != null)
                {
                    // 장비 아이템: ItemGenerator를 통해 등급 부여하여 생성
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
                    // 일반 아이템: 복제하여 인벤토리에 추가
                    BaseItemSO newItem = Instantiate(reward.itemSO);
                    player.inventoryManager.AddItem(newItem, reward.itemCount);
                }
            }
        }
    }

    /// <summary>
    /// 퀘스트의 진행 상황을 업데이트합니다.
    /// 몬스터 처치, NPC 대화 등 퀘스트 진행 데이터에 영향을 주는 이벤트에서 호출됩니다.
    /// </summary>
    public void UpdateQuestProgress(int questID, int targetID, int amount = 1)
    {
        bool progressChanged = false; // 퀘스트 진행이 실제로 바뀌었는지 추적

        if (questProgress.ContainsKey(questID))
        {
            QuestProgress progress = questProgress[questID];
            if (!progress.progress.ContainsKey(targetID))
            {
                progress.progress[targetID] = 0;
            }

            // 진행 상황이 실제로 증가했는지 확인
            int oldValue = progress.progress[targetID];
            progress.progress[targetID] += amount;

            if (oldValue != progress.progress[targetID])
            {
                progressChanged = true;
            }
        }

        // 진행 상황이 변경된 경우에만 이벤트를 호출합니다.
        if (progressChanged)
        {
            OnQuestProgressUpdated?.Invoke(questID);
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
            // 진행 데이터가 없으면 미완료
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
                    // 아이템 수집 퀘스트는 InventoryManager에서 실시간으로 확인합니다.
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
                    // 누적형 퀘스트는 QuestManager의 내부 진행 데이터로 확인합니다.
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
    // 기타 유틸리티 및 데이터 관련 메서드
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 주어진 퀘스트 ID의 현재 상태를 반환합니다. (NPC 대화/UI 표시용)
    /// </summary>
    /// <param name="questID">확인할 퀘스트의 ID.</param>
    /// <param name="currentAffection">현재 NPC의 호감도.</param>
    public QuestState GetQuestState(int questID, int currentAffection)
    {
        // 퀘스트 데이터 로드 확인
        if (questDataCache.Count == 0)
        {
            LoadAllQuestData();
        }

        if (!questDataCache.ContainsKey(questID))
        {
            Debug.LogWarning($"QuestID '{questID}'에 해당하는 퀘스트 데이터가 없습니다.");
            return QuestState.None;
        }

        QuestData data = questDataCache[questID];

        // 1. 완료 여부 확인
        if (completedQuests.Contains(questID))
        {
            // 반복 퀘스트라면 다시 수락 가능 상태
            if (data.isRepeatable)
            {
                return QuestState.Available;
            }
            return QuestState.Completed; // 일반 퀘스트는 완료 상태 유지
        }

        // 2. 수락 여부 확인
        if (acceptedQuests.Contains(questID))
        {
            // 완료 조건을 모두 충족했는지 확인
            if (CheckQuestCompletion(data))
            {
                return QuestState.ReadyToComplete;
            }
            else
            {
                return QuestState.Accepted;
            }
        }

        // 3. 수락 가능 여부 확인
        // 선행 퀘스트와 호감도 조건 확인
        if (data.prerequisiteQuests.All(prereqID => completedQuests.Contains(prereqID)))
        {
            if (currentAffection >= data.requiredAffection)
            {
                return QuestState.Available;
            }
            else
            {
                return QuestState.Unavailable; // 호감도 부족
            }
        }

        // 아직 선행 퀘스트를 완료하지 못한 경우
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

    /// <summary>
    /// 특정 퀘스트의 현재 진행 상황을 요약하여 문자열로 반환합니다.
    /// </summary>
    /// <param name="questID">확인할 퀘스트의 ID.</param>
    public string GetQuestProgressText(int questID)
    {
        if (!questDataCache.ContainsKey(questID))
        {
            return "퀘스트 데이터 없음";
        }

        QuestData data = questDataCache[questID];
        string progressSummary = "";

        bool isCompleted = CheckQuestCompletion(data);
        progressSummary += isCompleted ? "[완료 가능] " : "[진행 중] ";

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

                // 조건 유형에 따라 텍스트 조합
                switch (condition.conditionType)
                {
                    case QuestCondition.ConditionType.DefeatMonsters:
                        // 몬스터 처치: 누적 데이터 사용
                        progressSummary += $"몬스터 처치 ({currentAmount}/{condition.requiredAmount})";
                        break;
                    case QuestCondition.ConditionType.CollectItems:
                        // 아이템 수집: 인벤토리의 실시간 수량 사용
                        int itemCount = 0;
                        if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
                        {
                            itemCount = PlayerCharacter.Instance.inventoryManager.GetItemCount(condition.targetID);
                        }
                        progressSummary += $"아이템 수집 ({itemCount}/{condition.requiredAmount})";
                        break;
                    case QuestCondition.ConditionType.TalkToNPC:
                        // NPC 대화: 누적 데이터 사용
                        progressSummary += $"NPC와 대화 ({currentAmount}/{condition.requiredAmount})";
                        break;
                }
                // 여러 조건이 있을 경우 줄바꿈 처리
                if (data.conditions.Count > 1 && data.conditions.Last() != condition)
                {
                    progressSummary += "\n";
                }
            }
        }
        else
        {
            progressSummary += "진행 데이터가 없습니다.";
        }

        return progressSummary;
    }

    /// <summary>
    /// 특정 ID의 QuestData를 반환합니다.
    /// </summary>
    public QuestData GetQuestData(int questID)
    {
        if (questDataCache.ContainsKey(questID))
        {
            return questDataCache[questID];
        }
        return null;
    }

    /// <summary>
    /// 플레이어가 현재 수락한 모든 퀘스트의 ID 목록을 반환합니다.
    /// </summary>
    public List<int> GetAcceptedQuests()
    {
        return acceptedQuests;
    }

    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 퀘스트의 동적 데이터를 SaveData 객체로 변환하여 반환합니다. (직렬화)
    /// </summary>
    public object SaveData()
    {
        QuestsSaveData data = new QuestsSaveData();
        data.acceptedQuests = new List<int>(acceptedQuests);
        data.completedQuests = new List<int>(completedQuests);

        // Dictionary를 직렬화 가능한 List로 변환합니다.
        foreach (var quest in questProgress)
        {
            QuestProgressSaveData progressData = new QuestProgressSaveData();
            progressData.questID = quest.Key;

            // 중첩된 Dictionary도 List로 변환합니다.
            foreach (var target in quest.Value.progress)
            {
                progressData.progress.Add(new TargetProgress { targetID = target.Key, currentAmount = target.Value });
            }
            data.questProgressList.Add(progressData);
        }

        return data;
    }

    /// <summary>
    /// SaveData 객체의 데이터를 현재 QuestManager에 적용합니다. (역직렬화)
    /// </summary>
    /// <param name="data">로드할 데이터가 담긴 QuestsSaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is QuestsSaveData loadedData)
        {
            // 리스트 데이터 로드
            acceptedQuests = new List<int>(loadedData.acceptedQuests);
            completedQuests = new List<int>(loadedData.completedQuests);

            // 딕셔너리 데이터 로드
            questProgress.Clear();
            foreach (var progressData in loadedData.questProgressList)
            {
                QuestProgress newProgress = new QuestProgress();
                foreach (var target in progressData.progress)
                {
                    newProgress.progress[target.targetID] = target.currentAmount;
                }
                questProgress[progressData.questID] = newProgress;
            }
        }
    }
}