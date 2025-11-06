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
    /// (추가/수정) 플레이어에게 보여줄 수 있는 퀘스트 목록을 반환합니다.
    /// 퀘스트 매니저의 상태와 NPC의 호감도 조건을 모두 만족해야 합니다.
    /// SOLID: 개방-폐쇄 원칙. 퀘스트를 필터링하는 책임을 QuestGiver에 부여.
    /// </summary>
    /// <returns>플레이어가 수락 가능한 퀘스트 목록.</returns>
    public List<QuestData> GetAvailableQuests()
    {
        // NPC가 없으면 빈 목록 반환
        if (npc == null) return new List<QuestData>();

        int currentAffection = npc.GetAffection();

        // LINQ를 사용하여 퀘스트 목록을 필터링
        return questDatas.Where(quest =>
            // 수정된 로직: QuestManager의 GetQuestState()에 호감도 값을 넘겨주어,
            // 모든 조건을 통합적으로 판단하게 합니다.
            QuestManager.Instance.GetQuestState(quest.questID, currentAffection) == QuestState.Available
        ).ToList();
    }

    /// <summary>
    /// (추가/수정) 플레이어가 현재 진행 중인 퀘스트 목록을 반환합니다.
    /// </summary>
    public List<QuestData> GetAcceptedQuests()
    {
        return questDatas.Where(quest =>
            // 변경: GetQuestState에 호감도 인자를 추가했습니다.
            QuestManager.Instance.GetQuestState(quest.questID, npc.GetAffection()) == QuestState.Accepted ||
            QuestManager.Instance.GetQuestState(quest.questID, npc.GetAffection()) == QuestState.ReadyToComplete
        ).ToList();
    }

    /// <summary>
    /// 플레이어가 현재 이 NPC와 상호작용했을 때 보여줄 퀘스트의 상태를 결정합니다.
    /// 퀘스트 상태 우선순위: Complete > Accepted > Available
    /// </summary>
    /// <returns>가장 높은 우선순위의 퀘스트 상태 (없으면 QuestState.None)</returns>
    public QuestState GetHighestPriorityQuestState()
    {
        // 추가된 변수: 호감도를 한 번만 가져와서 여러 번 사용합니다.
        int currentAffection = npc.GetAffection();

        // 1. 완료 가능한 퀘스트가 있는지 확인
        // 변경: GetQuestState에 호감도 인자를 추가했습니다.
        if (questDatas.Any(q => QuestManager.Instance.GetQuestState(q.questID, currentAffection) == QuestState.ReadyToComplete))
        {
            return QuestState.ReadyToComplete;
        }

        // 2. 이미 수락된 퀘스트가 있는지 확인
        // 변경: GetQuestState에 호감도 인자를 추가했습니다.
        if (questDatas.Any(q => QuestManager.Instance.GetQuestState(q.questID, currentAffection) == QuestState.Accepted))
        {
            return QuestState.Accepted;
        }

        // 3. 수락 가능한 퀘스트가 있는지 확인 (호감도 조건 포함)
        if (GetAvailableQuests().Count > 0)
        {
            return QuestState.Available;
        }

        // 위의 모든 조건에 해당하지 않으면 기본 상태를 반환
        return QuestState.None;
    }
}