using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 퀘스트 목록의 각 버튼을 관리하는 스크립트입니다.
/// 버튼에 퀘스트 ID를 저장하고, 클릭 이벤트를 QuestUIHandler에 전달합니다.
/// SOLID: 단일 책임 원칙 (개별 퀘스트 버튼 관리).
/// </summary>
public class QuestButton : MonoBehaviour
{
    // --- UI 참조 변수 ---
    [Tooltip("퀘스트 이름을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [Tooltip("버튼 컴포넌트입니다.")]
    [SerializeField] private Button button;

    // --- 내부 변수 ---
    // 이 버튼이 나타내는 퀘스트의 고유 ID
    private int questID;

    /// <summary>
    /// 버튼을 초기화하는 메서드입니다.
    /// QuestUIHandler에서 버튼을 동적으로 생성할 때 호출됩니다.
    /// </summary>
    /// <param name="id">버튼이 나타낼 퀘스트의 ID.</param>
    /// <param name="name">버튼에 표시될 퀘스트의 이름.</param>
    /// <param name="onClickAction">버튼 클릭 시 실행될 액션.</param>
    public void Initialize(int id, string name, Action<int> onClickAction)
    {
        this.questID = id;

        // 버튼 텍스트 설정
        if (questNameText != null)
        {
            questNameText.text = name;
        }

        // 버튼 클릭 이벤트에 액션 추가
        if (button != null)
        {
            button.onClick.AddListener(() => onClickAction.Invoke(this.questID));
        }
    }
}