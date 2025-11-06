using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퀘스트의 고유한 정보를 담는 ScriptableObject.
/// 퀘스트의 이름, 목표, 보상, 진행 상태 등 변하지 않는 데이터를 에셋으로 관리합니다.
/// SOLID: 단일 책임 원칙 (퀘스트의 불변 데이터 관리).
/// </summary>
[CreateAssetMenu(fileName = "New Quest Data", menuName = "Quest/Quest Data", order = 1)]
public class QuestData : ScriptableObject
{
    // 퀘스트의 고유 ID. 퀘스트를 식별하는 데 사용됩니다.
    [Header("Quest Information")]
    [Tooltip("퀘스트의 고유한 이름입니다. QuestManager에서 퀘스트를 식별하는 데 사용됩니다.")]
    public int questID;

    // 퀘스트의 제목입니다.
    [Tooltip("퀘스트의 제목입니다.")]
    public string questTitle;

    // 퀘스트 목표에 대한 설명입니다.
    [Tooltip("퀘스트 목표에 대한 설명입니다.")]
    [TextArea(3, 5)]
    public string questDescription;

    // 💡 추가된 부분: 이 퀘스트를 주는 NPC의 이름입니다.
    [Tooltip("이 퀘스트를 주는 NPC의 이름입니다. 호감도 조건 확인에 사용됩니다.")]
    public string questGiverName;

    // 이 퀘스트를 받기 위해 필요한 최소 호감도입니다.
    [Tooltip("퀘스트를 받기 위해 필요한 최소 호감도입니다. 이 조건을 만족해야 NPC의 퀘스트 목록에 표시됩니다.")]
    public int requiredAffection;

    // 이 퀘스트를 받기 위해 선행되어야 하는 퀘스트들의 ID 목록입니다.
    [Header("Quest Prerequisites")]
    [Tooltip("이 퀘스트를 시작하기 전에 완료해야 하는 선행 퀘스트의 ID 목록입니다.")]
    public List<int> prerequisiteQuests = new List<int>();

    // 퀘스트의 완료 조건을 담는 리스트
    [Header("Quest Conditions")]
    [Tooltip("퀘스트 완료를 위해 충족해야 할 조건 목록입니다.")]
    public List<QuestCondition> conditions = new List<QuestCondition>();

    // 퀘스트 완료 후 보상을 받을 수 있는지 여부
    [Tooltip("퀘스트를 한 번만 완료할 수 있는지, 반복 가능한지 설정합니다.")]
    public bool isRepeatable = false;

    // 퀘스트 보상 목록입니다.
    [Header("Quest Rewards")]
    [Tooltip("퀘스트 완료 시 플레이어가 받을 보상 아이템 목록입니다.")]
    public List<RewardItem> rewardItems = new List<RewardItem>();

    // 퀘스트 보상 경험치입니다.
    [Tooltip("퀘스트 완료 시 플레이어가 받을 경험치입니다.")]
    public int experienceReward;

    // 퀘스트 보상 골드입니다.
    [Tooltip("퀘스트 완료 시 플레이어가 받을 골드입니다.")]
    public int goldReward;

    [Header("Rewards")]
    [Tooltip("퀘스트 완료 시 지급되는 호감도 보상입니다. (양수 입력)")]
    public int affectionReward;

    // ⭐ 추가된 부분: 보상 장비 아이템의 등급
    [Tooltip("퀘스트 보상 아이템에 EquipmentItemSO가 포함된 경우, 해당 장비에 적용될 등급입니다.")]
    public ItemGrade rewardEquipmentGrade;

    // 보상 아이템 정보를 직렬화하기 위한 내부 클래스
    [System.Serializable]
    public class RewardItem
    {
        /// <summary>
        /// 보상으로 제공할 아이템 ScriptableObject입니다.
        /// </summary>
        [Tooltip("보상으로 제공할 아이템 ScriptableObject입니다.")]
        public BaseItemSO itemSO;

        /// <summary>
        /// 아이템 수량입니다.
        /// </summary>
        [Tooltip("아이템 수량입니다.")]
        public int itemCount;
    }
}

/// <summary>
/// 퀘스트 완료 조건을 정의하는 클래스.
/// </summary>
[System.Serializable]
public class QuestCondition
{
    // 퀘스트 완료 조건의 종류 (아이템 수집, 대화, 몬스터 처치 등)
    public enum ConditionType
    {
        CollectItems,
        TalkToNPC,
        DefeatMonsters
    }

    [Tooltip("퀘스트 완료 조건의 종류를 설정합니다.")]
    public ConditionType conditionType;

    // 조건에 필요한 목표 ID (아이템 ID, NPC 이름, 몬스터 이름 등)
    [Tooltip("조건에 필요한 목표의 고유 ID입니다.")]
    public int targetID;

    // 목표 수량 (아이템 개수, 몬스터 마릿수 등)
    [Tooltip("조건에 필요한 목표 수량입니다.")]
    public int requiredAmount;
}