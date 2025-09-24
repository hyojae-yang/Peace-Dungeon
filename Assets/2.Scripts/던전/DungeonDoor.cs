using UnityEngine;
using System;

public class DungeonDoor : MonoBehaviour
{
    // SOLID ��Ģ �� ���� å�� ��Ģ(SRP)�� �ִ��� �ؼ��Ϸ� ����߽��ϴ�.
    // �� Ŭ������ ���� '�÷��̾� �浹 ����'�� '��ġ �̵�'�� �� ���� ���ҿ� �����մϴ�.

    [Header("���� ����Ʈ ����")]
    [Tooltip("�÷��̾ ó�� ������ �� �� ������ ��ġ�Դϴ�.")]
    [SerializeField] private Transform dungeonSpawnPoint;
    [Tooltip("�÷��̾ �������� ���� �� ������ ��ġ�Դϴ�.")]
    [SerializeField] private Transform exitSpawnPoint;


    /// <summary>
    /// �浹�� ���۵Ǿ��� �� �� �� ȣ��˴ϴ�.
    /// �÷��̾ "Player" �±׸� ������ �ִٸ� DungeonUIManager�� ȣ���Ͽ� �˸�â�� ���ϴ�.
    /// </summary>
    /// <param name="collision">�浹�� Collider�� ����.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // DungeonManager�� isInDungeon ���¸� Ȯ���Ͽ� �˸�â �ؽ�Ʈ�� �����մϴ�.
            if (DungeonManager.Instance != null)
            {
                string alertMessage = DungeonManager.Instance.IsInDungeon ? "�������� �����ðڽ��ϱ�?" : "������ �����Ͻðڽ��ϱ�?";

                // DungeonUIManager�� �ν��Ͻ��� ã�� �˸�â�� ���ϴ�.
                // Ȯ�� ��ư�� ������ HandleDungeonEntry �޼��尡 ����ǵ��� Action�� �Ѱ��ݴϴ�.
                if (DungeonUIManager.Instance != null)
                {
                    DungeonUIManager.Instance.ShowDungeonAlert(alertMessage, () => HandleDungeonEntry(collision.gameObject));
                }
                else
                {
                    Debug.LogWarning("DungeonUIManager �ν��Ͻ��� ã�� �� �����ϴ�!");
                }
            }
            else
            {
                Debug.LogWarning("DungeonManager �ν��Ͻ��� ã�� �� �����ϴ�!");
            }
        }
    }

    /// <summary>
    /// �÷��̾��� ���� ��ġ �̵��� ���� ���¸� �����ϴ� �޼����Դϴ�.
    /// �� �޼���� DungeonUIManager�� Ȯ�� ��ư�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="player">�̵���ų �÷��̾� GameObject.</param>
    private void HandleDungeonEntry(GameObject player)
    {
        // DungeonManager�� isInDungeon ���¸� Ȯ���մϴ�.
        if (DungeonManager.Instance != null)
        {
            if (DungeonManager.Instance.IsInDungeon == false)
            {
                // ���� ���� ���� �ƴ϶��, �������� ���Խ�ŵ�ϴ�.
                if (dungeonSpawnPoint != null)
                {
                    player.transform.position = dungeonSpawnPoint.position;
                    DungeonManager.Instance.IsInDungeon = true; // DungeonManager�� ���¸� '���� ��'���� �����մϴ�.
                }
                else
                {
                    Debug.LogWarning("���� ���� ����Ʈ�� �������� �ʾҽ��ϴ�. �÷��̾ �̵����� �ʽ��ϴ�.");
                }
            }
            else
            {
                // �̹� ���� �ȿ� �ִٸ�, ���� ������ �������ϴ�.
                if (exitSpawnPoint != null)
                {
                    player.transform.position = exitSpawnPoint.position;
                    Debug.Log("�÷��̾ ���� ������ �̵��߽��ϴ�.");
                    // DungeonManager�� ���¸� '���� ��'���� �����ϰ�, ��ġ �̵��� �Ϸ�� �� ExitDungeon() �޼��带 ȣ���մϴ�.
                    DungeonManager.Instance.IsInDungeon = false;
                    DungeonManager.Instance.ExitDungeon();
                }
                else
                {
                    Debug.LogWarning("���� �ⱸ ���� ����Ʈ�� �������� �ʾҽ��ϴ�. �÷��̾ �̵����� �ʽ��ϴ�.");
                }
            }
        }
    }

}