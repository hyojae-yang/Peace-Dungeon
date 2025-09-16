using UnityEngine;

/// <summary>
/// 퀘스트 아이템의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// 모든 퀘스트 아이템은 이 스크립트를 기반으로 생성됩니다.
/// BaseItemSO를 상속받아 아이템의 기본 정보를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Quest Item", menuName = "Item/Quest Item")]
public class QuestItemSO : BaseItemSO
{
    // === 퀘스트 아이템 고유 속성 ===
    [Header("퀘스트 아이템 고유 속성")]
    [Tooltip("이 퀘스트 아이템이 속한 퀘스트의 고유 ID입니다.")]
    [SerializeField]
    private int questID;

    /// <summary>
    /// 이 퀘스트 아이템이 속한 퀘스트의 고유 ID를 가져옵니다.
    /// 외부 시스템(예: 퀘스트 시스템)에서 플레이어의 인벤토리에 있는
    /// 퀘스트 아이템을 식별할 때 사용됩니다.
    /// </summary>
    /// <returns>퀘스트 아이템이 속한 퀘스트의 고유 ID</returns>
    public int GetQuestID()
    {
        return questID;
    }
}