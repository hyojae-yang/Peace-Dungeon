using UnityEngine;
using TMPro;

/// <summary>
/// 퀘스트 목록 UI에 표시되는 개별 항목을 관리하는 스크립트입니다.
/// 퀘스트 이름과 현재 상태를 표시하고, 버튼 클릭 이벤트를 처리합니다.
/// </summary>
public class QuestListItem : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("퀘스트 이름을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI questTitleText;

    [Tooltip("퀘스트 상태를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI questStatusText;

    // 이 항목이 나타내는 퀘스트의 데이터를 저장합니다.
    private QuestData questData;
    // 이 항목이 나타내는 퀘스트의 현재 상태를 저장합니다.
    private QuestState questState;

    /// <summary>
    /// 퀘스트 항목의 데이터를 설정하고 UI를 업데이트합니다.
    /// </summary>
    /// <param name="data">표시할 퀘스트 데이터.</param>
    /// <param name="state">퀘스트의 현재 상태.</param>
    public void SetQuestData(QuestData data, QuestState state)
    {
        // 퀘스트 데이터를 저장합니다.
        this.questData = data;
        this.questState = state;

        // UI를 퀘스트 데이터와 상태에 맞게 업데이트합니다.
        if (questTitleText != null)
        {
            questTitleText.text = data.questTitle;
        }

        if (questStatusText != null)
        {
            // 퀘스트 상태에 따라 다른 텍스트와 색상을 적용합니다.
            string statusText = GetStatusText(state);
            questStatusText.text = statusText;
            questStatusText.color = GetStatusColor(state);
        }
    }

    /// <summary>
    /// 퀘스트 상태에 따른 텍스트를 반환합니다.
    /// </summary>
    /// <param name="state">퀘스트 상태.</param>
    /// <returns>상태에 맞는 문자열.</returns>
    private string GetStatusText(QuestState state)
    {
        switch (state)
        {
            case QuestState.Available:
                return "진행 가능";
            case QuestState.Accepted:
                return "진행 중";
            case QuestState.Completed:
                return "완료";
            default:
                return "상태 알 수 없음";
        }
    }

    /// <summary>
    /// 퀘스트 상태에 따른 색상을 반환합니다.
    /// </summary>
    /// <param name="state">퀘스트 상태.</param>
    /// <returns>상태에 맞는 색상.</returns>
    private Color GetStatusColor(QuestState state)
    {
        switch (state)
        {
            case QuestState.Available:
                return Color.green; // 진행 가능
            case QuestState.Accepted:
                return Color.yellow; // 진행 중
            case QuestState.Completed:
                return Color.cyan; // 완료
            default:
                return Color.white;
        }
    }
}