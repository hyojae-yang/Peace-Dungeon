using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// 퀘스트 관련 UI(퀘스트 목록 패널)를 관리하는 싱글턴 클래스입니다.
/// NPCUIManager의 '퀘스트' 버튼과 연동됩니다.
/// SOLID: 단일 책임 원칙 (UI 항목 표시 및 정렬).
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance { get; private set; }

    [Header("Quest List UI")]
    [Tooltip("퀘스트 목록을 표시하는 패널입니다.")]
    [SerializeField]
    private GameObject questListPanel;
    [Tooltip("퀘스트 항목의 프리팹입니다.")]
    [SerializeField]
    private GameObject questListItemPrefab;
    [Tooltip("동적으로 생성된 퀘스트 항목이 배치될 부모 오브젝트입니다.")]
    [SerializeField]
    private Transform questListContent;

    // 현재 상호작용 중인 NPC의 QuestGiver 컴포넌트 참조
    private QuestGiver currentQuestGiver;
    // 퀘스트 선택 후 실행될 콜백 함수
    private Action<QuestData, QuestState> onQuestSelectedCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 퀘스트 목록 패널을 활성화하고 퀘스트 목록을 표시합니다.
    /// 이 메서드는 NPCUIManager의 '퀘스트' 버튼과 연동됩니다.
    /// </summary>
    /// <param name="questGiver">목록을 제공할 QuestGiver 컴포넌트.</param>
    /// <param name="callback">퀘스트 선택 후 실행될 콜백 함수.</param>
    public void ShowQuestList(QuestGiver questGiver, Action<QuestData, QuestState> callback)
    {
        if (questGiver == null)
        {
            Debug.LogError("QuestGiver 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        currentQuestGiver = questGiver;
        onQuestSelectedCallback = callback;

        // 기존 퀘스트 항목 모두 삭제
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // 💡 수정 사항: NPC의 현재 호감도를 가져와 GetQuestState에 전달
        int currentAffection = currentQuestGiver.GetComponent<NPC>().GetAffection();

        // 퀘스트 데이터를 가져와 상태에 따라 정렬합니다.
        List<QuestData> questDatas = currentQuestGiver.GetQuestDatas();

        // 퀘스트 상태에 따라 오름차순으로 정렬합니다.
        var sortedQuests = questDatas
            // 💡 수정: GetQuestState 메서드에 currentAffection 인자 추가
            .Select(data => new { Data = data, State = QuestManager.Instance.GetQuestState(data.questID, currentAffection) })
            .Where(q => q.State != QuestState.Unavailable && q.State != QuestState.None) // Unavailable, None 상태는 제외
            .OrderBy(q => q.State) // 상태에 따라 정렬
            .ToList();

        // 정렬된 퀘스트 목록을 기반으로 UI를 생성합니다.
        foreach (var sortedQuest in sortedQuests)
        {
            GameObject listItem = Instantiate(questListItemPrefab, questListContent);
            QuestListItem listItemScript = listItem.GetComponent<QuestListItem>();

            listItemScript.SetQuestData(sortedQuest.Data, sortedQuest.State);

            listItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnQuestSelected(sortedQuest.Data, sortedQuest.State));
        }

        questListPanel.SetActive(true);
    }

    /// <summary>
    /// 퀘스트 목록 패널을 비활성화하고 콜백을 초기화합니다.
    /// </summary>
    public void HideQuestList()
    {
        questListPanel.SetActive(false);
        currentQuestGiver = null;
        onQuestSelectedCallback = null;
    }

    /// <summary>
    /// 퀘스트 목록에서 특정 퀘스트 항목을 선택했을 때 호출되는 메서드입니다.
    /// </summary>
    private void OnQuestSelected(QuestData questData, QuestState state)
    {
        // 1. 콜백 함수를 먼저 실행하여 퀘스트 핸들러에 제어를 넘깁니다.
        if (onQuestSelectedCallback != null)
        {
            onQuestSelectedCallback.Invoke(questData, state);
        }

        // 2. 그 다음 퀘스트 목록 패널을 숨깁니다.
        HideQuestList();
    }
}