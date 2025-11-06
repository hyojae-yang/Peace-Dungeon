using System;

/// <summary>
/// 게임 세션 동안 변경되는 NPC의 동적 데이터(예: 호감도)를 담는 클래스입니다.
/// 이 클래스는 게임 저장 및 불러오기 시스템에서 사용됩니다.
/// </summary>
[Serializable]
public class NPCSessionData
{
    // 이 데이터가 속한 NPC의 고유 ID(이름)입니다.
    public string npcID;

    // 플레이어에 대한 NPC의 현재 호감도입니다.
    public int playerAffection;

    /// <summary>
    /// NPCSessionData의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="id">NPC의 고유 ID.</param>
    /// <param name="initialAffection">초기 호감도 값.</param>
    public NPCSessionData(string id, int initialAffection)
    {
        npcID = id;
        playerAffection = initialAffection;
    }
}