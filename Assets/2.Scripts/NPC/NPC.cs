using UnityEngine;
using System.Linq;

/// <summary>
/// 실제 NPC 게임 오브젝트에 부착되는 메인 스크립트.
/// NPCData를 참조하여 고유한 정보를 가지고, 다른 컴포넌트(이동, 상호작용)와 연동합니다.
/// NPCManager를 통해 동적 데이터에 접근하고, 기능 컴포넌트 유무를 확인하는 기능을 제공합니다.
/// </summary>
public class NPC : MonoBehaviour
{
    // 이 NPC의 고유 데이터를 담는 ScriptableObject.
    [Tooltip("이 NPC에 대한 고유 데이터(ScriptableObject)를 할당합니다.")]
    [SerializeField]
    private NPCData npcData;

    // NPC의 이동을 담당하는 컴포넌트.
    private NPCMovement npcMovement;

    // NPC와 플레이어 간의 상호작용을 담당하는 컴포넌트.
    private NPCInteraction npcInteraction;

    // 퀘스트 기능을 담당하는 컴포넌트.
    private QuestGiver questGiver;

    // 상점 기능을 담당하는 컴포넌트.
    private ShopManager shopManager;

    /// <summary>
    /// NPC의 고유 데이터를 가져오는 프로퍼티. 외부에서 읽기 전용으로 사용합니다.
    /// </summary>
    public NPCData Data
    {
        get { return npcData; }
    }

    /// <summary>
    /// NPC의 퀘스트 제공자 컴포넌트를 가져오는 프로퍼티. 외부에서 읽기 전용으로 사용합니다.
    /// </summary>
    public QuestGiver QuestGiver
    {
        get { return questGiver; }
    }

    /// <summary>
    /// MonoBehaviour의 Awake 메서드. 스크립트가 로드될 때 컴포넌트들을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();
        npcInteraction = GetComponent<NPCInteraction>();
        questGiver = GetComponent<QuestGiver>();
        shopManager = GetComponent<ShopManager>();

        if (npcData == null)
        {
            Debug.LogError("NPCData가 할당되지 않았습니다! NPC: " + this.gameObject.name);
        }
    }

    /// <summary>
    /// NPC의 현재 호감도에 접근하는 메서드.
    /// NPCManager를 통해 동적 데이터를 가져옵니다.
    /// </summary>
    /// <returns>현재 플레이어에 대한 호감도 값</returns>
    public int GetAffection()
    {
        if (NPCManager.Instance != null && Data != null)
        {
            return NPCManager.Instance.GetAffection(Data.npcName);
        }
        return 0;
    }

    /// <summary>
    /// 플레이어의 행동에 따라 호감도를 변경하는 메서드.
    /// NPCManager를 통해 동적 데이터를 변경합니다.
    /// </summary>
    /// <param name="value">변경할 호감도 값 (증가: 양수, 감소: 음수)</param>
    public void ChangeAffection(int value)
    {
        if (NPCManager.Instance != null && Data != null)
        {
            NPCManager.Instance.ChangeAffection(Data.npcName, value);
        }
    }

    /// <summary>
    /// NPC가 특정 컴포넌트를 가지고 있는지 확인하는 제네릭 메서드.
    /// </summary>
    /// <typeparam name="T">확인할 컴포넌트 타입</typeparam>
    /// <returns>컴포넌트가 존재하면 true, 아니면 false.</returns>
    public bool HasComponent<T>() where T : Component
    {
        return GetComponent<T>() != null;
    }

    /// <summary>
    /// NPC가 퀘스트 기능을 가지고 있는지 확인하는 메서드.
    /// </summary>
    /// <returns>퀘스트 기능 컴포넌트가 존재하면 true, 아니면 false.</returns>
    public bool HasQuestGiver()
    {
        return questGiver != null;
    }

    /// <summary>
    /// NPC가 특수 기능을 가지고 있는지 확인하는 메서드.
    /// </summary>
    /// <returns>하나 이상의 특수 기능 컴포넌트가 존재하면 true, 아니면 false.</returns>
    public bool HasSpecialFunction()
    {
        // 상점, 대장간 등 특수 기능 컴포넌트들을 여기에 추가
        return shopManager != null;
    }

    /// <summary>
    /// 특수 버튼에 표시될 이름을 반환하는 메서드.
    /// </summary>
    /// <returns>NPC가 가진 특수 기능에 해당하는 이름. 기능이 없으면 빈 문자열 반환.</returns>
    public string GetSpecialButtonName()
    {
        if (shopManager != null)
        {
            return "상점";
        }
        return "";
    }
}