using UnityEngine;
using System;

public class DungeonDoor : MonoBehaviour
{
    /// <summary>
    /// 충돌이 시작되었을 때 한 번 호출됩니다.
    /// 플레이어가 "Player" 태그를 가지고 있다면 DungeonUIManager를 호출하여 알림창을 띄웁니다.
    /// </summary>
    /// <param name="collision">충돌한 Collider의 정보.</param>
    private void OnCollisionEnter(Collision collision)
    {

        // 단일 책임 원칙 (SRP): 충돌 감지 및 UI 호출 역할만 수행합니다.
        // OCP: 태그 비교는 확장 가능성이 낮은 부분이므로 그대로 유지합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            // DungeonManager의 인스턴스 유효성 검사 (의존성 관리)
            if (DungeonManager.Instance != null)
            {
                // 현재 던전 상태에 따라 알림 메시지를 결정합니다.
                string alertMessage = DungeonManager.Instance.IsInDungeon ? "던전에서 나가시겠습니까?" : "던전에 입장하시겠습니까?";

                // DungeonUIManager의 인스턴스를 찾아 알림창을 띄웁니다.
                if (DungeonUIManager.Instance != null)
                {
                    // 확인 버튼을 누르면 HandleDungeonEntry 메서드가 실행되도록 Action을 넘겨줍니다.
                    // 'collision.gameObject' 대신 'collision.gameObject'를 전달하여 플레이어 객체를 처리합니다.
                    DungeonUIManager.Instance.ShowDungeonAlert(alertMessage, () => HandleDungeonEntry(collision.gameObject));
                }
                else
                {
                    Debug.LogWarning("DungeonUIManager 인스턴스를 찾을 수 없습니다! UI를 띄울 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("DungeonManager 인스턴스를 찾을 수 없습니다! 던전 상태를 확인할 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 플레이어의 실제 위치 이동과 던전 상태를 변경하는 메서드입니다.
    /// 이 메서드는 DungeonUIManager의 확인 버튼에 의해 호출됩니다.
    /// </summary>
    /// <param name="player">이동시킬 플레이어 GameObject.</param>
    private void HandleDungeonEntry(GameObject player)
    {
        // DungeonManager가 유효한지 확인합니다.
        if (DungeonManager.Instance == null)
        {
            Debug.LogWarning("DungeonManager 인스턴스가 없어 던전 진입/퇴장 처리를 할 수 없습니다.");
            return;
        }

        // 플레이어 캐릭터 컨트롤러 인스턴스가 유효한지 확인합니다.
        if (PlayerCharacter.Instance == null || PlayerCharacter.Instance.playerController == null)
        {
            Debug.LogError("PlayerCharacter 또는 playerController 인스턴스를 찾을 수 없습니다. 플레이어 이동 처리가 불가능합니다.");
            return;
        }

        // 던전 진입 로직
        if (DungeonManager.Instance.IsInDungeon == false)
        {
            SaveManager.Instance.SaveGame(); // 던전 입장 전에 게임을 저장합니다.
            // 플레이어를 던전 안으로 이동시킵니다.
            PlayerCharacter.Instance.playerController.inDungeon();

            // DungeonManager의 상태를 '던전 안'으로 변경합니다.
            // (DungeonManager 내부 IsInDungeon Setter에서 HandleDungeonEntry()가 호출됨)
            DungeonManager.Instance.IsInDungeon = true;
        }
        // 던전 퇴장 로직 (여기가 수정되었습니다!)
        else // DungeonManager.Instance.IsInDungeon == true
        {
            // 1. 플레이어를 던전 밖으로 이동시킵니다.
            PlayerCharacter.Instance.playerController.outDungeon();

            // 2. 몬스터 정리 및 보상 지급 로직을 담당하는 ExitDungeon()을 호출합니다.
            // (ExitDungeon() 내부 로직이 IsInDungeon 상태에 의존하지 않지만, 호출 순서의 명확성을 위해 이 위치를 유지합니다.)
            DungeonManager.Instance.ExitDungeon();

            // 3. 마지막으로 DungeonManager의 상태를 '던전 밖'으로 변경합니다.
            //    이 변경으로 DungeonManager의 IsInDungeon Setter 내부 로직(HandleDungeonEntry)은 실행되지 않습니다.
            DungeonManager.Instance.IsInDungeon = false;
        }
    }
}