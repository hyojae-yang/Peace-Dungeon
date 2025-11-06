using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어의 퀘스트 패널 UI를 관리하는 싱글턴 클래스입니다.
/// 이 스크립트는 패널을 열고 닫는 기능, 퀘스트 목록의 동적 업데이트 및 실시간 갱신을 담당합니다.
/// SOLID: 단일 책임 원칙 (UI 총괄 제어 및 이벤트 처리).
/// </summary>
public class PlayerQuestPanelController : MonoBehaviour
{
    public static PlayerQuestPanelController Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("퀘스트 패널 전체를 담는 부모 GameObject입니다.")]
    [SerializeField]
    private GameObject questPanel;
    [Tooltip("퀘스트 항목들이 배치될 스크롤뷰의 Content Transform입니다.")]
    [SerializeField]
    private Transform questListContent;
    [Tooltip("개별 퀘스트 항목을 생성하는 데 사용할 프리팹입니다.")]
    [SerializeField]
    private GameObject playerQuestItemPrefab;

    /// <summary>
    /// 현재 패널에 표시 중인 퀘스트 항목들을 관리하는 딕셔너리.
    /// 키: 퀘스트 ID, 값: 해당 ID를 표시하는 PlayerQuestItem 컴포넌트
    /// UI와 데이터를 동기화하고 실시간 업데이트에 사용됩니다.
    /// </summary>
    private Dictionary<int, PlayerQuestItem> activeQuestItems = new Dictionary<int, PlayerQuestItem>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 퀘스트 관리자 인스턴스가 생성된 후에 이벤트를 구독합니다.
        if (QuestManager.Instance != null)
        {
            // 진행 상황 업데이트 이벤트 구독 (텍스트 갱신용)
            QuestManager.Instance.OnQuestProgressUpdated += OnQuestProgressUpdatedHandler;

            // [핵심 추가] 목록 구조 변경 이벤트 구독 (항목 생성/제거용)
            // QuestManager에 OnQuestListChanged 이벤트가 있다고 가정합니다.
            QuestManager.Instance.OnQuestListChanged += OnQuestListChangedHandler;
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독을 해제합니다.
        if (Instance == this && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestProgressUpdated -= OnQuestProgressUpdatedHandler;
            QuestManager.Instance.OnQuestListChanged -= OnQuestListChangedHandler;
        }
    }

    /// <summary>
    /// **[추가된 핸들러]** QuestManager에서 퀘스트 목록(수락/완료/취소)이 변경될 때 호출됩니다.
    /// 패널이 활성화되어 있을 때만 UI 목록 구조를 즉시 동기화합니다.
    /// </summary>
    private void OnQuestListChangedHandler()
    {
        if (questPanel != null && questPanel.activeSelf)
        {
            // 퀘스트 항목의 추가/제거가 발생했으므로 목록 구조를 동기화합니다.
            SynchronizeQuestList();
        }
    }

    /// <summary>
    /// QuestManager에서 퀘스트 진행 상황이 업데이트될 때 호출되는 핸들러입니다.
    /// 패널이 활성화 상태일 때, 해당 퀘스트 항목만 **선택적으로** 갱신합니다.
    /// </summary>
    private void OnQuestProgressUpdatedHandler(int updatedQuestID)
    {
        if (questPanel != null && questPanel.activeSelf)
        {
            UpdateSingleQuestItem(updatedQuestID);
        }
    }

    /// <summary>
    /// 단일 퀘스트 항목의 텍스트 내용만 업데이트합니다. (실시간 업데이트 로직)
    /// </summary>
    /// <param name="questID">갱신할 퀘스트의 ID.</param>
    private void UpdateSingleQuestItem(int questID)
    {
        // 딕셔너리에서 해당 퀘스트 ID를 가진 UI 항목을 찾습니다.
        if (activeQuestItems.TryGetValue(questID, out PlayerQuestItem questItem))
        {
            QuestData questData = QuestManager.Instance.GetQuestData(questID);

            if (questData != null)
            {
                string progressText = QuestManager.Instance.GetQuestProgressText(questID);

                // UI 항목의 텍스트만 갱신합니다. (가장 효율적인 갱신)
                questItem.SetQuestInfo(
                    questData.questTitle,
                    questData.questGiverName,
                    progressText);
            }
        }
    }

    /// <summary>
    /// 퀘스트 패널을 활성화/비활성화하고 UI를 업데이트합니다.
    /// </summary>
    public void ToggleQuestPanel()
    {
        bool isActive = questPanel.activeSelf;
        questPanel.SetActive(!isActive);

        // 패널이 활성화될 때 (열릴 때) 목록을 데이터와 동기화합니다.
        if (!isActive)
        {
            SynchronizeQuestList();
        }
    }

    /// <summary>
    /// [핵심 수정] 플레이어가 수락한 퀘스트 목록과 UI 항목을 동기화합니다 (DIFFing).
    /// 이 메서드는 전체 파괴 후 재생성 대신, 추가/제거가 필요한 항목만 처리합니다.
    /// </summary>
    private void SynchronizeQuestList()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 1. 현재 데이터 목록 (팩트)
        List<int> acceptedQuestIDs = QuestManager.Instance.GetAcceptedQuests();
        HashSet<int> acceptedQuestIDsSet = new HashSet<int>(acceptedQuestIDs);

        // 2. [REMOVE] UI에는 있지만 데이터에는 없는 항목 제거 (완료 또는 취소된 퀘스트)
        // 딕셔너리 키 목록을 가져와야 순회 중 제거해도 오류가 나지 않습니다.
        List<int> keysToRemove = activeQuestItems.Keys.Except(acceptedQuestIDsSet).ToList();

        foreach (int questID in keysToRemove)
        {
            if (activeQuestItems.TryGetValue(questID, out PlayerQuestItem itemToRemove))
            {
                Destroy(itemToRemove.gameObject);
                activeQuestItems.Remove(questID);
            }
        }

        // 3. [ADD & UPDATE] 데이터에는 있지만 UI에는 없는 항목 추가 (새로 수락된 퀘스트)
        foreach (int questID in acceptedQuestIDs)
        {
            // 퀘스트 데이터 유효성 확인
            QuestData questData = QuestManager.Instance.GetQuestData(questID);
            if (questData == null)
            {
                Debug.LogWarning($"QuestID '{questID}'에 대한 QuestData를 찾을 수 없습니다.");
                continue;
            }

            if (!activeQuestItems.ContainsKey(questID))
            {
                // UI에 없는 경우: 항목 새로 생성 및 딕셔너리에 추가
                GameObject questItemObj = Instantiate(playerQuestItemPrefab, questListContent);
                PlayerQuestItem questItem = questItemObj.GetComponent<PlayerQuestItem>();

                if (questItem != null)
                {
                    string progressText = QuestManager.Instance.GetQuestProgressText(questID);
                    questItem.SetQuestInfo(
                        questData.questTitle,
                        questData.questGiverName,
                        progressText);

                    activeQuestItems.Add(questID, questItem);
                }
            }
            // 4. [UPDATE] 이미 존재하는 항목이라도 진행 상황 텍스트를 최신화 (안정성 보강)
            else
            {
                // 패널이 열릴 때 모든 텍스트를 한 번 갱신합니다.
                UpdateSingleQuestItem(questID);
            }
        }
    }
}