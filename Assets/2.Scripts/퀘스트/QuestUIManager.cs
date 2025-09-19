using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// 퀘스트 관련 UI(퀘스트 목록 패널)를 관리하는 싱글턴 클래스입니다.
/// NPCUIManager의 '퀘스트' 버튼과 연동됩니다.
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance { get; private set; }

    [Header("Quest List UI")]
    [Tooltip("퀘스트 목록을 표시하는 패널입니다.")]
    [SerializeField]
    private GameObject questListPanel;
    [Tooltip("퀘스트 항목의 프리팹입니다. 이 프리팹을 인스턴스화하여 목록을 만듭니다.")]
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
            DontDestroyOnLoad(gameObject);
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

        // 퀘스트 목록을 동적으로 생성
        List<QuestData> questDatas = currentQuestGiver.GetQuestDatas();
        foreach (QuestData data in questDatas)
        {
            // QuestManager를 통해 직접 퀘스트 상태를 확인합니다.
            QuestState state = QuestManager.Instance.GetQuestState(data.questID);

            // "진행 가능" 상태, "진행 중" 상태, "완료 가능" 상태의 퀘스트만 목록에 표시합니다.
            if (state == QuestState.Available || state == QuestState.Accepted || state == QuestState.Complete)
            {
                GameObject listItem = Instantiate(questListItemPrefab, questListContent);
                QuestListItem listItemScript = listItem.GetComponent<QuestListItem>();

                // 퀘스트 항목 UI 업데이트
                listItemScript.SetQuestData(data, state);

                // 버튼 클릭 이벤트 연결
                listItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnQuestSelected(data, state));
            }
        }
        // 퀘스트 목록 UI 활성화
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
    /// <param name="questData">선택된 퀘스트의 데이터.</param>
    /// <param name="state">선택된 퀘스트의 현재 상태.</param>
    private void OnQuestSelected(QuestData questData, QuestState state)
    {
        // 퀘스트 목록 패널을 숨깁니다.
        HideQuestList();

        // 선택된 퀘스트 정보와 상태를 NPCUIManager로 다시 전달합니다.
        onQuestSelectedCallback?.Invoke(questData, state);
    }
}