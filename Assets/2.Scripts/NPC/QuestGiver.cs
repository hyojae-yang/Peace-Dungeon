using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 특정 퀘스트를 플레이어에게 제공하는 NPC에 부착되는 컴포넌트입니다.
/// QuestGiver는 퀘스트 데이터 에셋들을 참조하고, 플레이어의 퀘스트 상태에 따라
/// 어떤 퀘스트를 제공할지 판단하는 로직을 담당합니다.
/// </summary>
public class QuestGiver : MonoBehaviour
{
    // === 퀘스트 데이터 ===
    [Tooltip("이 NPC가 제공하는 퀘스트 데이터 목록입니다. 에디터에서 할당합니다.")]
    [SerializeField]
    private List<QuestData> questDatas = new List<QuestData>();

    // === NPC 관련 참조 ===
    private NPC npc;

    /// <summary>
    /// MonoBehaviour의 Awake 메서드.
    /// </summary>
    private void Awake()
    {
        // 동일 게임 오브젝트에 부착된 NPC 컴포넌트 참조
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("QuestGiver 스크립트는 같은 오브젝트에 NPC 스크립트가 필요합니다!");
        }
    }

    /// <summary>
    /// 이 NPC가 제공하는 모든 퀘스트 데이터를 반환합니다.
    /// </summary>
    /// <returns>NPC가 제공하는 퀘스트 데이터 목록.</returns>
    public List<QuestData> GetQuestDatas()
    {
        return questDatas;
    }

    /// <summary>
    /// 플레이어가 현재 이 NPC와 상호작용했을 때 보여줄 퀘스트의 상태를 결정합니다.
    /// 퀘스트 상태 우선순위: Complete > Accepted > Available
    /// </summary>
    /// <returns>가장 높은 우선순위의 퀘스트 상태 (없으면 QuestState.None)</returns>
    public QuestState GetHighestPriorityQuestState()
    {
        QuestData currentQuest = null;

        // 1. 완료 가능한 퀘스트가 있는지 확인
        currentQuest = questDatas.FirstOrDefault(q =>
            QuestManager.Instance.IsQuestAccepted(q.questID) && QuestManager.Instance.CheckQuestCompletion(q));
        if (currentQuest != null)
        {
            return QuestState.Complete;
        }

        // 2. 이미 수락된 퀘스트가 있는지 확인
        currentQuest = questDatas.FirstOrDefault(q => QuestManager.Instance.IsQuestAccepted(q.questID));
        if (currentQuest != null)
        {
            return QuestState.Accepted;
        }

        // 3. 수락 가능한 퀘스트가 있는지 확인
        int currentAffection = (npc != null) ? npc.GetAffection() : 0;
        currentQuest = questDatas.FirstOrDefault(q =>
            !QuestManager.Instance.IsQuestAccepted(q.questID) && !QuestManager.Instance.IsQuestCompleted(q.questID) && currentAffection >= q.requiredAffection);
        if (currentQuest != null)
        {
            return QuestState.Available;
        }

        // 위의 모든 조건에 해당하지 않으면 기본 상태를 반환
        return QuestState.None;
    }
}