using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// 퀘스트 UI 패널을 관리하는 스크립트입니다.
/// 퀘스트 목록을 동적으로 생성하고, 선택된 퀘스트의 상세 정보를 표시합니다.
/// SOLID: 단일 책임 원칙 (퀘스트 UI 관리).
/// </summary>
public class QuestUIHandler : MonoBehaviour
{
    // --- UI 참조 변수 ---
    [Header("UI References")]
    [Tooltip("퀘스트 버튼들이 생성될 부모 Transform입니다.")]
    [SerializeField] private Transform questButtonContainer;
    [Tooltip("퀘스트 버튼 프리팹을 할당합니다.")]
    [SerializeField] private GameObject questButtonPrefab;
    [Tooltip("퀘스트 상세 정보를 표시할 패널입니다.")]
    [SerializeField] private GameObject questDetailPanel;
    [Tooltip("상세 정보 패널의 퀘스트명 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [Tooltip("상세 정보 패널의 NPC명 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [Tooltip("상세 정보 패널의 퀘스트 설명 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [Tooltip("상세 정보 패널의 진행 상태 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI questProgressText;

    // --- 내부 변수 ---
    // 생성된 퀘스트 버튼 오브젝트들을 관리할 리스트
    private List<GameObject> activeQuestButtons = new List<GameObject>();
    // 현재 상세 정보를 표시 중인 퀘스트의 ID
    private int currentSelectedQuestID = -1;

    // --- MonoBehaviour 메서드 ---
    private void Awake()
    {
        // 퀘스트 매니저 인스턴스에 접근하여 퀘스트 관련 이벤트에 구독합니다.
        // 퀘스트가 수락되거나 완료될 때마다 UI를 갱신하기 위함입니다.
        if (QuestManager.Instance != null)
        {
            // TODO: QuestManager에 퀘스트 상태 변경 이벤트 (OnQuestAccepted, OnQuestCompleted)를
            // 추가하고, 이 메서드들을 해당 이벤트에 구독하여 자동 갱신 로직을 구현할 수 있습니다.
        }
    }

    /// <summary>
    /// 게임 오브젝트가 활성화될 때마다 호출되는 메서드입니다.
    /// 퀘스트 UI가 열릴 때마다 목록을 최신 상태로 갱신합니다.
    /// </summary>
    private void OnEnable()
    {
        // UI가 활성화되면 바로 퀘스트 목록을 갱신합니다.
        UpdateQuestList();
    }

    /// <summary>
    /// 플레이어가 수락한 퀘스트 목록을 UI에 갱신합니다.
    /// 기존 버튼을 모두 파괴하고 새로 생성합니다.
    /// </summary>
    public void UpdateQuestList()
    {
        // 기존에 생성된 모든 퀘스트 버튼을 제거
        foreach (var button in activeQuestButtons)
        {
            Destroy(button);
        }
        activeQuestButtons.Clear();

        // QuestManager로부터 현재 수락된 모든 퀘스트 ID를 가져옴
        var acceptedQuestIDs = QuestManager.Instance.GetAcceptedQuests();

        if (acceptedQuestIDs == null || acceptedQuestIDs.Count == 0)
        {
            questDetailPanel.SetActive(false);
            return;
        }

        foreach (var questID in acceptedQuestIDs)
        {
            QuestData questData = QuestManager.Instance.GetQuestData(questID);
            if (questData != null)
            {
                // 퀘스트 버튼 프리팹을 동적으로 생성
                GameObject newButtonObj = Instantiate(questButtonPrefab, questButtonContainer);
                activeQuestButtons.Add(newButtonObj);

                // 버튼의 텍스트와 클릭 이벤트 설정
                TextMeshProUGUI buttonText = newButtonObj.GetComponentInChildren<TextMeshProUGUI>();
                Button button = newButtonObj.GetComponent<Button>();

                if (buttonText != null)
                {
                    buttonText.text = questData.questTitle;
                }

                if (button != null)
                {
                    // 람다식을 사용하여 버튼 클릭 시 해당 퀘스트 ID를 전달
                    button.onClick.AddListener(() => OnQuestButtonClick(questID));
                }
            }
        }

        // 버튼이 하나라도 있으면 첫 번째 버튼의 상세 정보 표시
        OnQuestButtonClick(acceptedQuestIDs[0]);
    }

    /// <summary>
    /// 퀘스트 버튼 클릭 시 호출됩니다. 상세 정보 패널을 업데이트합니다.
    /// </summary>
    /// <param name="questID">선택된 퀘스트의 ID.</param>
    public void OnQuestButtonClick(int questID)
    {
        // 현재 선택된 퀘스트 ID를 업데이트
        currentSelectedQuestID = questID;

        // 상세 패널 활성화
        questDetailPanel.SetActive(true);

        // 퀘스트 데이터 가져오기
        QuestData questData = QuestManager.Instance.GetQuestData(questID);
        if (questData == null)
        {
            Debug.LogError($"퀘스트 ID {questID}의 데이터를 찾을 수 없습니다.");
            return;
        }

        // UI 텍스트 업데이트
        questNameText.text = questData.questTitle;
        npcNameText.text = $"의뢰인: {questData.questGiverName}";
        questDescriptionText.text = questData.questDescription;

        // QuestManager의 GetQuestProgressText() 메서드를 호출하여 진행 상태를 가져옵니다.
        questProgressText.text = QuestManager.Instance.GetQuestProgressText(questID);
    }
}