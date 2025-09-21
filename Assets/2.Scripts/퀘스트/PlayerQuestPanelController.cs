using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어의 퀘스트 패널 UI를 관리하는 싱글턴 클래스입니다.
/// 이 스크립트는 패널을 열고 닫는 기능과 퀘스트 목록을 동적으로 업데이트하는 책임을 가집니다.
/// SOLID: 단일 책임 원칙 (UI 총괄 제어).
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

    private void Awake()
    {
        // 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
            // 패널을 시작 시 비활성화 상태로 설정
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 퀘스트 패널을 활성화/비활성화하고 UI를 업데이트합니다.
    /// 이 메서드는 UI 버튼의 OnClick() 이벤트에 연결됩니다.
    /// </summary>
    public void ToggleQuestPanel()
    {
        bool isActive = questPanel.activeSelf;
        questPanel.SetActive(!isActive);

        // 패널이 활성화될 때만 퀘스트 목록을 업데이트합니다.
        if (!isActive)
        {
            UpdateQuestList();
        }
    }

    /// <summary>
    /// 플레이어가 수락한 퀘스트 목록을 QuestManager로부터 가져와 UI를 업데이트합니다.
    /// </summary>
    // 에러가 발생했던 UpdateQuestList() 메서드
    private void UpdateQuestList()
    {
        // 1. 기존에 생성된 모든 퀘스트 항목 삭제
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // 2. QuestManager에서 수락된 퀘스트 목록을 가져옵니다.
        // 수정된 부분: .Select(q => q.questID)를 삭제하고
        // QuestManager.GetAcceptedQuests()가 반환하는 int 리스트를 그대로 사용합니다.
        List<int> acceptedQuestIDs = QuestManager.Instance.GetAcceptedQuests();

        // 3. 각 퀘스트에 대한 UI 항목 생성
        foreach (int questID in acceptedQuestIDs)
        {
            QuestData questData = QuestManager.Instance.GetQuestData(questID); // 퀘스트 데이터 가져오기

            // 데이터가 유효한지 확인
            if (questData == null)
            {
                Debug.LogWarning($"QuestID '{questID}'에 대한 QuestData를 찾을 수 없습니다.");
                continue;
            }

            // 프리팹 인스턴스화
            GameObject questItemObj = Instantiate(playerQuestItemPrefab, questListContent);
            PlayerQuestItem questItem = questItemObj.GetComponent<PlayerQuestItem>();

            // 퀘스트 진행 상황 텍스트 가져오기
            string progressText = QuestManager.Instance.GetQuestProgressText(questID);

            // UI에 정보 설정
            if (questItem != null)
            {
                questItem.SetQuestInfo(
                    questData.questTitle,
                    questData.questGiverName,
                    progressText);
            }
        }
    }
}