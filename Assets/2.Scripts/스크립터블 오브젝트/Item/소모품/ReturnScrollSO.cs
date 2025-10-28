// ReturnScrollSO.cs
using UnityEngine;

/// <summary>
/// 던전 내부에서 플레이어를 안전하게 탈출시키는 기능을 담당하는 소모품 아이템입니다.
/// ConsumableItemSO를 상속받아 고유한 사용 로직(던전 탈출)을 가집니다.
/// CanUse()를 오버라이드하여 던전 상태에 따른 사용 유효성을 검사합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Return Scroll", menuName = "Item/Consumable/Return Scroll")]
public class ReturnScrollSO : ConsumableItemSO
{
    // 이 아이템은 던전 탈출 기능 외에 별도의 데이터(스탯, 위치)를 요구하지 않으므로 추가 필드는 없습니다.

    /// <summary>
    /// 귀환서 아이템이 현재 상황에서 유효하게 사용 가능한지 확인합니다.
    /// 던전 내부이고 보스룸이 아닐 때만 true를 반환합니다.
    /// 던전 상태 확인이라는 단일 책임을 가집니다. (SRP)
    /// </summary>
    /// <param name="player">아이템을 사용할 플레이어 캐릭터</param>
    /// <returns>사용 가능 여부</returns>
    public override bool CanUse(PlayerCharacter player)
    {
        // 유효성 검사 (싱글톤 인스턴스 존재 확인)
        if (DungeonManager.Instance == null || PlayerCharacter.Instance == null)
        {
            Debug.LogError("DungeonManager 또는 PlayerCharacter 싱글톤 인스턴스를 찾을 수 없습니다.");
            return false;
        }

        // 1. 핵심 유효성 조건 확인 (던전 내부 & 비보스룸)
        if (DungeonManager.Instance.IsInDungeon && !DungeonManager.Instance._isBossRoomActive)
        {
            return true; // 사용 가능
        }

        // 2. 사용 불가 시 경고 및 false 반환
        if (DungeonManager.Instance._isBossRoomActive)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    $"[귀환서 사용 불가] 보스 방에서는 {itemName}을 사용할 수 없습니다.",
                    NotificationType.General
                );
            }
            //Debug.LogWarning($"[귀환서 사용 불가] 보스 방에서는 {itemName}을 사용할 수 없습니다.");
        }
        else // 던전 밖
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    $"[귀환서 사용 불가] 던전 밖에서는 {itemName}을 사용할 필요가 없습니다.",
                    NotificationType.General
                );
            }
           // Debug.LogWarning($"[귀환서 사용 불가] 던전 밖에서는 {itemName}을 사용할 필요가 없습니다.");
        }

        return false; // 사용 불가
    }

    /// <summary>
    /// 아이템을 사용하는 로직을 정의합니다. (CanUse()가 true일 때만 인벤토리에서 호출됨을 가정)
    /// 이 메서드는 ConsumableItemSO의 가상 메서드를 오버라이드하며, 던전 탈출 기능을 실행합니다.
    /// </summary>
    /// <param name="player">아이템을 사용할 플레이어 캐릭터</param>
    public override void Use(PlayerCharacter player)
    {
        // Use()가 호출되었다는 것은 CanUse()가 true였음을 의미하므로, 던전 탈출 로직만 실행합니다.

        // 싱글톤 인스턴스 유효성 재검사 (안전성 확보)
        if (DungeonManager.Instance == null || PlayerCharacter.Instance == null)
        {
            Debug.LogError("핵심 인스턴스 부재로 Use 실행 불가.");
            return;
        }


        // 1. 플레이어를 던전 밖으로 이동시킵니다.
        PlayerCharacter.Instance.playerController.outDungeon();

        // 2. 몬스터 정리 및 보상 지급 로직을 담당하는 ExitDungeon()을 호출합니다.
        DungeonManager.Instance.ExitDungeon();

        // 3. 마지막으로 DungeonManager의 상태를 '던전 밖'으로 변경합니다.
        DungeonManager.Instance.IsInDungeon = false;
    }
}