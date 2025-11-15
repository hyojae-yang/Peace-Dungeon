using UnityEngine;
using System; // Action 타입 사용을 위해 필요할 수 있습니다. (안전하게 추가)

public class BossRoomDoor : MonoBehaviour
{
    /// <summary>
    /// 오브젝트가 활성화될 때, DungeonManager의 이벤트에 구독하여 상태 변화를 감지합니다.
    /// DIP(의존성 역전 원칙)를 위해 이벤트 시스템을 활용합니다.
    /// </summary>
    private void OnEnable()
    {
        // DungeonManager.Instance.OnDungeonCleared += OnDungeonClearedHandler; 
        // DungeonManager에 이벤트가 있다고 가정하고 구독합니다. (없다면 직접 추가해야 합니다.)
        // 예시를 위해 DungeonManager가 클리어 이벤트를 노출한다고 가정합니다.

        // *주의*: DungeonManager가 싱글톤이고 이미 Awake에서 초기화되었다고 가정합니다.
        if (DungeonManager.Instance != null)
        {
            // *가상의 이벤트 구독*: 보스 처치 후 포털을 다시 켜기 위함.
            DungeonManager.Instance.OnBossDefeated += OnBossDefeatedHandler;
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화되거나 파괴될 때, 구독을 해제하여 메모리 누수를 방지합니다.
    /// </summary>
    private void OnDisable()
    {
        if (DungeonManager.Instance != null)
        {
            // 구독 해제: 잊지 마세요!
            DungeonManager.Instance.OnBossDefeated -= OnBossDefeatedHandler;
        }
    }

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
                if (DungeonManager.Instance.IsDungeonCleared) // 클리어 상태
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

    // --- 이벤트 핸들러 (DungeonManager로부터 호출) ---

    /// <summary>
    /// **[요청 3] 보스 처치 후 포털 재활성화:** DungeonManager에서 보스 처치 이벤트가 발생하면 호출됩니다.
    /// </summary>
    private void OnBossDefeatedHandler()
    {
    }

    // --- 내부 유틸리티 및 로직 (기존 코드 유지) ---

    /// <summary>
    /// 플레이어에게 보스룸 입장 여부를 묻는 알림창을 띄우도록 UIManager에 요청하는 메서드입니다.
    /// </summary>
    private void ShowEntryAlert(GameObject player)
    {
        if (DungeonUIManager.Instance == null)
        {
            Debug.LogWarning("DungeonUIManager 인스턴스를 찾을 수 없습니다! UI를 띄울 수 없습니다.");
            return;
        }

        string alertMessage = "보스룸에 입장하시겠습니까?";

        DungeonUIManager.Instance.ShowDungeonAlert(
            alertMessage,
            () => TriggerBossRoomEntry(player) // 확인 버튼 클릭 시 입장 처리 메서드 연결
        );
    }

    /// <summary>
    /// **[요청 2] 플레이어 입장 시 포털 비활성화:** UI 알림창의 '확인' 버튼을 눌렀을 때 호출됩니다.
    /// </summary>
    private void TriggerBossRoomEntry(GameObject player)
    {
        if (MainSceneManager.Instance != null)
        {
            // 화면을 검게 가렸다가 (Fade Out) 즉시 다시 열어주는 (Fade In) 효과를 줍니다.
            MainSceneManager.Instance.PerformScreenFade(
                fadeOutDuration: 0.3f,
                fadeInDuration: 0.5f
            );
        }
        // 2. 플레이어 이동 처리
        if (PlayerCharacter.Instance == null || PlayerCharacter.Instance.playerController == null)
        {
            Debug.LogError("PlayerCharacter 또는 playerController 인스턴스를 찾을 수 없습니다. 플레이어 이동 처리가 불가능합니다.");
            return;
        }
        PlayerCharacter.Instance.playerController.enterBossRoom();

        // 3. DungeonManager 상태 변경 및 후속 로직 위임 
        if (DungeonManager.Instance == null)
        {
            Debug.LogWarning("DungeonManager 인스턴스를 찾을 수 없어 보스룸 상태 변경 요청을 할 수 없습니다.");
            return;
        }
        DungeonManager.Instance.HandleBossRoomEntry(player);
    }

    /// <summary>
    /// 플레이어에게 던전 퇴장 여부를 묻는 알림창을 띄우도록 UIManager에 요청하는 메서드입니다.
    /// </summary>
    private void ShowExitAlert()
    {
        if (DungeonUIManager.Instance == null)
        {
            Debug.LogWarning("DungeonUIManager 인스턴스를 찾을 수 없습니다! UI를 띄울 수 없습니다.");
            return;
        }
        string alertMessage = "던전밖으로 나가시겠습니까?";

        DungeonUIManager.Instance.ShowDungeonAlert(
            alertMessage,
            () => TriggerDungeonExit()
        );
    }

    /// <summary>
    /// UI 알림창의 '확인' 버튼을 눌렀을 때 호출되어, 실제 던전 퇴장 처리(보상 지급 포함)를 수행합니다.
    /// </summary>
    private void TriggerDungeonExit()
    {
        // 1. DungeonManager 인스턴스 확인
        if (DungeonManager.Instance == null)
        {
            Debug.LogError("DungeonManager 인스턴스를 찾을 수 없습니다. 퇴장 처리가 불가능합니다.");
            return;
        }

        // 2. 던전 퇴장 및 보상 처리 (책임을 DungeonManager에 위임)
        DungeonManager.Instance.ExitDungeon();

        // 3. 던전 상태 초기화 (DungeonManager에 책임 위임)
        DungeonManager.Instance.ResetDungeonState();

        PlayerCharacter.Instance.playerController.outDungeon();
       // Debug.Log("보상 지급 및 던전 퇴장 처리 완료.");
    }
}