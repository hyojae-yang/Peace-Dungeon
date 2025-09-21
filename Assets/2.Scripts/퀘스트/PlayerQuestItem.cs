using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어 퀘스트 패널에 표시되는 개별 퀘스트 항목을 관리하는 스크립트.
/// QuestManager로부터 받은 정보를 UI에 표시하는 단일 책임을 가집니다.
/// </summary>
public class PlayerQuestItem : MonoBehaviour
{
    [Tooltip("퀘스트 이름을 표시할 텍스트 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI questNameText;

    [Tooltip("퀘스트를 준 NPC의 이름을 표시할 텍스트 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI giverNameText;

    [Tooltip("퀘스트 진행 상황을 표시할 텍스트 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI progressText;

    /// <summary>
    /// 퀘스트 정보를 설정하고 UI를 업데이트하는 메서드입니다.
    /// 이 메서드는 QuestManager로부터 가공된 문자열을 받아 즉시 UI에 적용합니다.
    /// SOLID: 단일 책임 원칙 (UI 표시).
    /// </summary>
    /// <param name="questName">표시할 퀘스트 이름.</param>
    /// <param name="questGiverName">퀘스트를 준 NPC의 이름.</param>
    /// <param name="progressTextContent">퀘스트 진행 상황을 나타내는 가공된 문자열.</param>
    public void SetQuestInfo(string questName, string questGiverName, string progressTextContent)
    {
        // null 체크를 통해 안정성을 확보합니다.
        if (questNameText != null)
        {
            questNameText.text = questName;
        }

        if (giverNameText != null)
        {
            giverNameText.text = questGiverName;
        }

        if (progressText != null)
        {
            progressText.text = progressTextContent;
        }
    }
}