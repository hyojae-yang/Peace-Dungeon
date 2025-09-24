using UnityEngine;
using System;

public class DungeonDoor : MonoBehaviour
{
    // SOLID 원칙 중 단일 책임 원칙(SRP)을 최대한 준수하려 노력했습니다.
    // 이 클래스는 이제 '플레이어 충돌 감지'와 '위치 이동'의 두 가지 역할에 집중합니다.

    [Header("스폰 포인트 설정")]
    [Tooltip("플레이어가 처음 던전에 들어갈 때 스폰될 위치입니다.")]
    [SerializeField] private Transform dungeonSpawnPoint;
    [Tooltip("플레이어가 던전에서 나갈 때 스폰될 위치입니다.")]
    [SerializeField] private Transform exitSpawnPoint;


    /// <summary>
    /// 충돌이 시작되었을 때 한 번 호출됩니다.
    /// 플레이어가 "Player" 태그를 가지고 있다면 DungeonUIManager를 호출하여 알림창을 띄웁니다.
    /// </summary>
    /// <param name="collision">충돌한 Collider의 정보.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // DungeonManager의 isInDungeon 상태를 확인하여 알림창 텍스트를 결정합니다.
            if (DungeonManager.Instance != null)
            {
                string alertMessage = DungeonManager.Instance.IsInDungeon ? "던전에서 나가시겠습니까?" : "던전에 입장하시겠습니까?";

                // DungeonUIManager의 인스턴스를 찾아 알림창을 띄웁니다.
                // 확인 버튼을 누르면 HandleDungeonEntry 메서드가 실행되도록 Action을 넘겨줍니다.
                if (DungeonUIManager.Instance != null)
                {
                    DungeonUIManager.Instance.ShowDungeonAlert(alertMessage, () => HandleDungeonEntry(collision.gameObject));
                }
                else
                {
                    Debug.LogWarning("DungeonUIManager 인스턴스를 찾을 수 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("DungeonManager 인스턴스를 찾을 수 없습니다!");
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
        // DungeonManager의 isInDungeon 상태를 확인합니다.
        if (DungeonManager.Instance != null)
        {
            if (DungeonManager.Instance.IsInDungeon == false)
            {
                // 아직 던전 안이 아니라면, 던전으로 진입시킵니다.
                if (dungeonSpawnPoint != null)
                {
                    player.transform.position = dungeonSpawnPoint.position;
                    DungeonManager.Instance.IsInDungeon = true; // DungeonManager의 상태를 '던전 안'으로 변경합니다.
                }
                else
                {
                    Debug.LogWarning("던전 스폰 포인트가 설정되지 않았습니다. 플레이어가 이동하지 않습니다.");
                }
            }
            else
            {
                // 이미 던전 안에 있다면, 던전 밖으로 내보냅니다.
                if (exitSpawnPoint != null)
                {
                    player.transform.position = exitSpawnPoint.position;
                    Debug.Log("플레이어가 던전 밖으로 이동했습니다.");
                    // DungeonManager의 상태를 '던전 밖'으로 변경하고, 위치 이동이 완료된 후 ExitDungeon() 메서드를 호출합니다.
                    DungeonManager.Instance.IsInDungeon = false;
                    DungeonManager.Instance.ExitDungeon();
                }
                else
                {
                    Debug.LogWarning("던전 출구 스폰 포인트가 설정되지 않았습니다. 플레이어가 이동하지 않습니다.");
                }
            }
        }
    }

}