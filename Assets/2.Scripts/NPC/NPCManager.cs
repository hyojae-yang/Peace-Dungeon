using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 NPC들의 동적 데이터를 중앙에서 관리하는 싱글턴 클래스입니다.
/// 게임 로딩 및 저장 시 데이터 처리를 담당하며, NPC의 특수 기능 목록도 관리합니다.
/// SOLID: 단일 책임 원칙 (모든 NPC 데이터의 중앙 관리)
/// </summary>
public class NPCManager : MonoBehaviour
{
    // NPCManager의 싱글턴 인스턴스
    public static NPCManager Instance { get; private set; }

    // 모든 NPC의 스크립터블 오브젝트 목록
    [Tooltip("모든 NPC 데이터 ScriptableObject를 여기에 할당하세요.")]
    [SerializeField]
    private List<NPCData> allNPCsData;

    // 게임 세션 동안 변경되는 NPC들의 동적 데이터
    // Key: NPC의 고유 ID (npcName), Value: 해당 NPC의 동적 데이터 인스턴스
    private Dictionary<string, NPCSessionData> npcSessionDataMap = new Dictionary<string, NPCSessionData>();

    //----------------------------------------------------------------------------------------------------------------
    // 새로운 기능: NPC 특수 기능 관리
    //----------------------------------------------------------------------------------------------------------------

    // NPC의 이름(고유 ID)을 키로, 해당 NPC가 가진 특수 기능 목록을 값으로 저장하는 딕셔너리입니다.
    private Dictionary<string, List<INPCFunction>> specialFunctionMap = new Dictionary<string, List<INPCFunction>>();

    private void Awake()
    {
        // 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
            InitializeSessionData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 모든 NPC 데이터로부터 세션 데이터를 초기화합니다.
    /// 게임 시작 시 한 번 호출되어 모든 NPC의 동적 데이터 인스턴스를 생성합니다.
    /// </summary>
    private void InitializeSessionData()
    {
        foreach (var data in allNPCsData)
        {
            if (data != null && !npcSessionDataMap.ContainsKey(data.npcName))
            {
                // NPCData로부터 초기 호감도 값을 가져와 새 인스턴스를 생성합니다.
                NPCSessionData sessionData = new NPCSessionData(data.npcName, data.playerAffection);
                npcSessionDataMap[data.npcName] = sessionData;
            }
        }
    }

    /// <summary>
    /// 특정 NPC의 현재 호감도 값을 가져옵니다.
    /// </summary>
    /// <param name="npcID">호감도를 가져올 NPC의 고유 ID (npcName)</param>
    /// <returns>해당 NPC의 현재 호감도. 데이터가 없으면 기본값인 0을 반환합니다.</returns>
    public int GetAffection(string npcID)
    {
        if (npcSessionDataMap.TryGetValue(npcID, out NPCSessionData data))
        {
            return data.playerAffection;
        }
        return 0;
    }

    /// <summary>
    /// 특정 NPC의 호감도를 변경합니다.
    /// 이 메서드는 게임 플레이 중 호감도를 조작할 때 사용됩니다.
    /// </summary>
    /// <param name="npcID">호감도를 변경할 NPC의 고유 ID (npcName)</param>
    /// <param name="value">변경할 호감도 값 (증가: 양수, 감소: 음수)</param>
    public void ChangeAffection(string npcID, int value)
    {
        if (npcSessionDataMap.TryGetValue(npcID, out NPCSessionData data))
        {
            data.playerAffection = Mathf.Clamp(data.playerAffection + value, -100, 100);
            Debug.Log($"NPC '{npcID}'의 호감도가 {value}만큼 변경되어 현재 호감도는 {data.playerAffection}입니다.");
        }
        else
        {
            Debug.LogWarning($"NPC ID '{npcID}'를 찾을 수 없어 호감도를 변경할 수 없습니다.");
        }
    }

    /// <summary>
    /// 각 특수 기능 컴포넌트가 스스로를 NPCManager에 등록할 때 사용합니다.
    /// 이 메서드는 NPC가 활성화될 때마다 호출되어 데이터를 안전하게 저장합니다.
    /// </summary>
    /// <param name="npcName">기능을 가진 NPC의 고유 이름입니다.</param>
    /// <param name="function">등록할 INPCFunction 인터페이스 컴포넌트입니다.</param>
    public void RegisterSpecialFunction(string npcName, INPCFunction function)
    {
        // 딕셔너리에 해당 NPC 이름이 아직 등록되지 않았다면, 새로운 리스트를 만듭니다.
        if (!specialFunctionMap.ContainsKey(npcName))
        {
            specialFunctionMap[npcName] = new List<INPCFunction>();
        }

        // 이미 등록된 기능인지 확인하여 중복 등록을 방지합니다.
        if (!specialFunctionMap[npcName].Contains(function))
        {
            specialFunctionMap[npcName].Add(function);
            Debug.Log($"NPC '{npcName}'에 특수 기능 '{function.FunctionButtonName}'이(가) 등록되었습니다.");
        }
    }

    /// <summary>
    /// 특정 NPC의 모든 특수 기능 목록을 반환합니다.
    /// NPC 스크립트가 이 메서드를 호출하여 특수 기능 정보를 가져갑니다.
    /// </summary>
    /// <param name="npcName">기능 목록을 요청할 NPC의 이름입니다.</param>
    /// <returns>해당 NPC의 INPCFunction 목록을 반환합니다. 없다면 비어있는 리스트를 반환합니다.</returns>
    public List<INPCFunction> GetSpecialFunctions(string npcName)
    {
        if (specialFunctionMap.ContainsKey(npcName))
        {
            return specialFunctionMap[npcName];
        }
        // NPC에 기능이 없다면 빈 리스트를 반환하여 NullReferenceException을 방지합니다.
        return new List<INPCFunction>();
    }

    // 이 외에 저장 및 로딩 관련 메서드 추가 예정
    // public void SaveNPCData() { ... }
    // public void LoadNPCData() { ... }
}