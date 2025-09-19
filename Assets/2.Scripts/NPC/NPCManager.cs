using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 NPC들의 동적 데이터를 중앙에서 관리하는 싱글턴 클래스입니다.
/// 게임 로딩 및 저장 시 데이터 처리를 담당합니다.
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
        Debug.Log("모든 NPC 세션 데이터가 초기화되었습니다.");
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
        Debug.LogWarning($"NPC ID '{npcID}'에 대한 세션 데이터가 없습니다.");
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

    // 이 외에 저장 및 로딩 관련 메서드 추가 예정
    // public void SaveNPCData() { ... }
    // public void LoadNPCData() { ... }
}