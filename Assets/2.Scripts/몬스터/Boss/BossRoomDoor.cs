using UnityEngine;

public class BossRoomDoor : MonoBehaviour
{

    /// <summary>
    /// 충돌이 시작되었을 때 한 번 호출됩니다.
    /// 플레이어("Player" 태그)가 문에 닿았을 때, 보스룸 상태를 확인하여 UI 알림을 띄울지 결정합니다.
    /// </summary>
    /// <param name="collision">충돌한 Collision의 정보.</param>
    private void OnCollisionEnter(Collision collision)
    {
        // 1. 플레이어 태그 체크
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. DungeonManager 인스턴스 유효성 검사 (의존성 관리)
            if (DungeonManager.Instance == null)
            {
                Debug.LogWarning("DungeonManager 인스턴스를 찾을 수 없어 보스룸 상태 확인이 불가능합니다.");
                return;
            }

            // 3. **핵심 기능**: 보스룸 상태 확인 및 분기 처리
            if (!DungeonManager.Instance.IsBossRoomActive)
            {
                // **[추가 로직]**
                // 3-1. 보스가 처치되어 클리어 상태인지 확인 (퇴장 상호작용)
                if (DungeonManager.Instance.IsDungeonCleared) // <--- 새로운 클리어 상태 확인
                {
                    // 클리어 상태일 때, 퇴장 알림을 요청합니다.
                    ShowExitAlert();
                }
                // 3-2. 일반적인 보스룸 미활성 상태 (입장 상호작용)
                else
                {
                    // 보스룸이 비활성화 상태일 때만 UI 알림 요청 메서드를 호출합니다.
                    ShowEntryAlert(collision.gameObject);
                }
            }
            else
            {
                // 보스 전투 중 (활성 상태)
                return;
            }
        }
    }
    // BossRoomDoor.cs 에 추가될 내용입니다.

    /// <summary>
    /// 플레이어에게 보스룸 입장 여부를 묻는 알림창을 띄우도록 UIManager에 요청하는 메서드입니다.
    /// 단일 책임 원칙(SRP)에 따라, 이 메서드는 UI 메시지 준비 및 요청 책임만 수행합니다.
    /// </summary>
    /// <param name="player">입장 알림을 받을 플레이어 GameObject.</param>
    private void ShowEntryAlert(GameObject player)
    {
        // DungeonUIManager 인스턴스 유효성 검사 (안전성 확보)
        if (DungeonUIManager.Instance == null)
        {
            Debug.LogWarning("DungeonUIManager 인스턴스를 찾을 수 없습니다! UI를 띄울 수 없습니다.");
            return;
        }

        // 알림 메시지 설정
        // 이전에 동의했던 메시지를 사용합니다.
        string alertMessage = "보스룸에 입장하시겠습니까?";

        // DungeonUIManager의 ShowDungeonAlert 메서드를 호출하여 UI 표시를 위임합니다.
        // 확인 버튼 클릭 시, 다음 단계인 TriggerBossRoomEntry 메서드가 실행되도록 Action을 넘겨줍니다.
        DungeonUIManager.Instance.ShowDungeonAlert(
            alertMessage,
            () => TriggerBossRoomEntry(player) // 다음 단계의 메서드를 연결합니다.
        );
    }
    /// <summary>
    /// UI 알림창의 '확인' 버튼을 눌렀을 때 호출되어,
    /// 실제 보스룸 입장 처리를 수행합니다. (DungeonDoor의 HandleDungeonEntry와 유사한 역할)
    /// 단일 책임 원칙(SRP) 관점에서는 무거울 수 있으나, 기존 시스템과의 일관성을 유지합니다.
    /// </summary>
    /// <param name="player">입장 처리를 할 플레이어 GameObject.</param>
    private void TriggerBossRoomEntry(GameObject player)
    {
        // 1. 플레이어 이동 처리 (가장 먼저 수행)
        // 플레이어 캐릭터 컨트롤러 인스턴스가 유효한지 확인합니다. (안전성 확보)
        if (PlayerCharacter.Instance == null || PlayerCharacter.Instance.playerController == null)
        {
            Debug.LogError("PlayerCharacter 또는 playerController 인스턴스를 찾을 수 없습니다. 플레이어 이동 처리가 불가능합니다.");
            return;
        }

        // 플레이어 컨트롤러의 메서드를 직접 호출하여 위치 이동을 위임합니다.
        PlayerCharacter.Instance.playerController.enterBossRoom();

        // 2. DungeonManager 상태 변경 및 후속 로직 위임 (다음 논의 주제)
        // DungeonManager가 유효한지 확인합니다. (안전성 확보)
        if (DungeonManager.Instance == null)
        {
            Debug.LogWarning("DungeonManager 인스턴스를 찾을 수 없어 보스룸 상태 변경 요청을 할 수 없습니다.");
            return;
        }

        DungeonManager.Instance.HandleBossRoomEntry(player);

    }
    /// <summary>
    /// 플레이어에게 던전 퇴장 여부를 묻는 알림창을 띄우도록 UIManager에 요청하는 메서드입니다.
    /// 단일 책임 원칙(SRP)에 따라, 이 메서드는 UI 메시지 준비 및 요청 책임만 수행합니다.
    /// </summary>
    private void ShowExitAlert()
    {
        // DungeonUIManager 인스턴스 유효성 검사 (안전성 확보)
        if (DungeonUIManager.Instance == null)
        {
            Debug.LogWarning("DungeonUIManager 인스턴스를 찾을 수 없습니다! UI를 띄울 수 없습니다.");
            return;
        }

        // 알림 메시지 설정
        string alertMessage = "던전밖으로 나가시겠습니까?";

        // DungeonUIManager의 ShowDungeonAlert 메서드를 호출하여 UI 표시를 위임합니다.
        // 확인 버튼 클릭 시, 퇴장 처리를 위한 TriggerDungeonExit 메서드가 실행되도록 Action을 넘겨줍니다.
        DungeonUIManager.Instance.ShowDungeonAlert(
            alertMessage,
            () => TriggerDungeonExit() // <--- 퇴장 처리 메서드를 연결합니다.
        );
    }

    /// <summary>
    /// UI 알림창의 '확인' 버튼을 눌렀을 때 호출되어,
    /// 실제 던전 퇴장 처리(보상 지급 포함)를 수행하고 상태를 초기화합니다.
    /// </summary>
    private void TriggerDungeonExit()
    {
        // 1. DungeonManager 인스턴스 확인 (안전성 확보)
        if (DungeonManager.Instance == null)
        {
            Debug.LogError("DungeonManager 인스턴스를 찾을 수 없습니다. 퇴장 처리가 불가능합니다.");
            return;
        }

        // 2. 던전 퇴장 및 보상 처리 (책임을 DungeonManager에 위임)
        // ExitDungeon() 내부에는 보상 지급 및 몬스터 정리 로직이 포함되어 있습니다.
        DungeonManager.Instance.ExitDungeon();

        // 3. 던전 상태 초기화 (DungeonManager에 책임 위임)
        // 클리어 상태 및 InDungeon 상태를 해제하여 다음 던전 진입을 준비합니다.
        DungeonManager.Instance.ResetDungeonState();

        PlayerCharacter.Instance.playerController.outDungeon();
        Debug.Log("보상 지급 및 던전 퇴장 처리 완료.");
    }
}
