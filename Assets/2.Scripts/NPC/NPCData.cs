using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC의 고유한 정보를 담는 ScriptableObject.
/// 이름, 성격, 관계, 플레이어에 대한 호감도 등 변경되지 않는 데이터를 에셋으로 관리합니다.
/// 이 데이터는 게임 시작 시 NPCSessionData로 복사되어 사용됩니다.
/// </summary>
[CreateAssetMenu(fileName = "New NPC Data", menuName = "NPC/NPC Data", order = 1)]
public class NPCData : ScriptableObject
{
    // NPC의 고유 ID입니다.
    [Header("Basic Information")]
    [Tooltip("NPC의 고유한 식별자 ID입니다.")]
    public int npcID;

    // NPC의 이름을 저장합니다.
    [Tooltip("NPC의 고유한 이름입니다.")]
    public string npcName;

    // NPC의 성격을 나타내는 문자열입니다.
    [Tooltip("NPC의 성격을 한 문장으로 설명합니다.")]
    [TextArea(3, 5)]
    public string npcPersonality;

    // 이 NPC가 다른 NPC와 맺고 있는 관계를 나타냅니다.
    [Header("Relationships")]
    [Tooltip("다른 NPC와의 관계를 정의합니다. Key는 상대방 NPC의 이름, Value는 관계의 설명입니다.")]
    public List<Relationship> relationships = new List<Relationship>();

    // 관계를 직렬화하기 위한 내부 클래스
    [System.Serializable]
    public class Relationship
    {
        public string targetNPCName;
        public string relationshipType;
    }

    // 플레이어에 대한 NPC의 초기 호감도를 저장합니다.
    [Header("Player Affection")]
    [Tooltip("플레이어에 대한 NPC의 초기 호감도입니다. -100에서 100 사이의 값을 가질 수 있습니다.")]
    [Range(-100, 100)]
    public int playerAffection = 0;

    // 퀘스트 상태에 따라 달라지는 대화 목록을 저장합니다.
    [Header("Dialogue Based on Quest & Affection")]
    [Tooltip("퀘스트 상태에 따라 NPC의 대화 내용을 정의합니다.")]
    public List<DialogueGroup> dialogueGroups = new List<DialogueGroup>();
}

/// <summary>
/// 퀘스트 진행 상태를 나타내는 열거형.
/// </summary>
public enum QuestState
{
    /// <summary>
    /// NPC에게 퀘스트가 없는 상태.
    /// 플레이어가 '대화하기' 버튼을 눌렀을 때 평상시 대화로 사용됩니다.
    /// </summary>
    None,

    /// <summary>
    /// 퀘스트가 있지만, 플레이어의 호감도 부족 등의 이유로 수락할 수 없는 상태.
    /// 플레이어가 '대화하기' 버튼을 눌렀을 때, 퀘스트를 줄 수 없는 이유를 설명하는 대화로 사용됩니다.
    /// </summary>
    Unavailable,

    /// <summary>
    /// 퀘스트를 줄 수 있는 상태.
    /// 퀘스트 목록에서 해당 퀘스트를 선택했을 때, 퀘스트의 배경 스토리를 설명하는 대화로 사용됩니다.
    /// </summary>
    Available,

    /// <summary>
    /// 퀘스트를 수락한 상태.
    /// 진행 중인 퀘스트를 취소하려 할 때, NPC가 아쉬움을 표현하는 대화로 사용됩니다.
    /// </summary>
    Accepted,

    /// <summary>
    /// 퀘스트 목표를 모두 달성하고 보상을 받을 수 있는 상태.
    /// 완료된 퀘스트를 선택했을 때, NPC가 플레이어를 칭찬하고 보상을 주는 대화로 사용됩니다.
    /// </summary>
    ReadyToComplete,

    /// <summary>
    /// 퀘스트를 완료하고 보상까지 모두 받은 상태.
    /// 퀘스트 완료 후 NPC에게 다시 말을 걸었을 때, 반복적으로 출력되는 대화로 사용됩니다.
    /// </summary>
    Completed
}

/// <summary>
/// 특정 퀘스트 상태에 대한 대화 데이터를 정의하는 클래스.
/// </summary>
[System.Serializable]
public class DialogueGroup
{
    [Tooltip("이 대화 그룹이 활성화될 퀘스트 상태입니다. (None, Available, Accepted, ReadyToComplete, Completed)")]
    public QuestState questState;

    // --- 이 부분이 새로 추가됩니다.
    [Tooltip("이 대화 그룹이 적용될 퀘스트의 고유 ID입니다. 'None' 상태일 경우 비워둡니다.")]
    public int questID;
    // ---

    [Tooltip("호감도 범위에 따른 상호작용 시 초기 대화 목록입니다.")]
    public List<AffectionDialogue> interactionDialogue = new List<AffectionDialogue>();

    [Tooltip("해당 퀘스트 상태와 호감도 범위에 따른 대화 목록입니다.")]
    public List<AffectionDialogue> generalDialogues = new List<AffectionDialogue>();
}

/// <summary>
/// 특정 호감도 범위에 속할 때 사용할 대화 데이터를 정의하는 클래스.
/// 이제 여러 개의 대사 메시지를 포함할 수 있습니다.
/// </summary>
[System.Serializable]
public class AffectionDialogue
{
    [Tooltip("이 대화가 활성화되는 최소 호감도입니다. (포함)")]
    public int minAffection;

    [Tooltip("이 대화가 활성화되는 최대 호감도입니다. (미포함)")]
    public int maxAffection;

    [Tooltip("해당 호감도 범위에서 출력될 여러 개의 대화 메시지입니다.")]
    [TextArea(3, 5)]
    public string[] dialogueTexts;
}