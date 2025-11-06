using UnityEngine;
using System; // System.Action을 사용하기 위해 using System 추가

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
            // [추가] 이미 귀환 프로세스가 진행 중인지 확인합니다. (중복 사용 방지)
            if (PlayerCharacter.Instance.IsReturnProcessActive)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(
                        $"[귀환서 사용 불가] 이미 귀환 프로세스가 진행 중입니다. 잠시 기다려주세요.",
                        NotificationType.General
                    );
                }
                return false;
            }
            return true; // 사용 가능
        }

        // 2. 사용 불가 시 경고 및 false 반환
        if (DungeonManager.Instance._isBossRoomActive)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    $"보스 방에서는 {itemName}을 사용할 수 없습니다.",
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
                    $"던전 밖에서는 {itemName}을 사용할 수 없습니다.",
                    NotificationType.General
                );
            }
            // Debug.LogWarning($"[귀환서 사용 불가] 던전 밖에서는 {itemName}을 사용할 필요가 없습니다.");
        }

        return false; // 사용 불가
    }

    /// <summary>
    /// 아이템을 사용하는 로직을 정의합니다. 
    /// [수정] 이 메서드는 딜레이 후 실행될 최종 로직을 Action으로 정의하고 PlayerCharacter에게 실행을 요청합니다.
    /// 이를 통해 귀환 실행 로직의 책임은 ReturnScrollSO에 남습니다.
    /// </summary>
    /// <param name="player">아이템을 사용할 플레이어 캐릭터</param>
    public override void Use(PlayerCharacter player)
    {
        // 싱글톤 인스턴스 유효성 재검사 (안전성 확보)
        if (DungeonManager.Instance == null || PlayerCharacter.Instance == null)
        {
            Debug.LogError("핵심 인스턴스 부재로 Use 실행 불가.");
            return;
        }

        // [수정] 딜레이 후 실행될 최종 귀환 로직을 Action으로 정의합니다. (SRP 유지)
        System.Action finalReturnAction = () =>
        {
            // 이 블록은 딜레이 완료 후 PlayerCharacter가 호출해 줍니다.

            // 1. 플레이어를 던전 밖으로 이동시킵니다. (기존 로직 유지)
            player.playerController.outDungeon();

            // 2. 몬스터 정리 및 보상 지급 로직을 담당하는 ExitDungeon()을 호출합니다. (기존 로직 유지)
            DungeonManager.Instance.ExitDungeon();

            // 3. 마지막으로 DungeonManager의 상태를 '던전 밖'으로 변경합니다. (기존 로직 유지)
            DungeonManager.Instance.IsInDungeon = false;
        };

        // [수정] PlayerCharacter에게 딜레이 실행을 요청하고, 최종 로직(Action)을 전달합니다.
        if (PlayerCharacter.Instance.StartReturnDelay(finalReturnAction))
        {
            // 딜레이 시작 성공 알림
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                   $"[귀환 시작] {PlayerCharacter.RETURN_DELAY}초 후 마을로 귀환합니다.",
                   NotificationType.General
               );
            }
            // 아이템 소모 로직은 이 Use() 메서드를 호출한 인벤토리 측에서 처리해야 합니다.
        }
    }
}