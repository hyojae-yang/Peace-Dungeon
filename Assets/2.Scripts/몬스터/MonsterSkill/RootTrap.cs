using UnityEngine;
using System.Collections;

/// <summary>
/// '�Ѹ� ����' ���� ȿ���� ����ϴ� ��ũ��Ʈ�Դϴ�.
/// Ʈ���� ������ ������ �÷��̾��� �̵��� ���� �ð� ���� �����ϰ�, ������ ������ϴ�.
/// </summary>
public class RootTrap : MonoBehaviour
{
    [Tooltip("�÷��̾��� �̵��� ����� �ð��Դϴ�.")]
    [SerializeField] private float immobilizeDuration = 3.0f;
    private bool isPlayerImmobilized = false; // �÷��̾�� ȿ���� �����ߴ��� Ȯ���ϴ� �÷���
    private float originalSpeed; // �÷��̾��� ���� �̵� �ӵ��� ������ ����

    private void Start()
    {
        // ���� �ð��� ������ ������ �ı��ǵ��� �ڷ�ƾ ����
        StartCoroutine(SelfDestructRoutine());
    }

    /// <summary>
    /// Ʈ���� ������ �ٸ� �ݶ��̴��� �������� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="other">������ ������ �ݶ��̴�</param>
    private void OnTriggerEnter(Collider other)
    {
        // �̹� ȿ���� �����߰ų�, ������ ������Ʈ�� �÷��̾ �ƴϸ� �Լ��� �����մϴ�.
        if (isPlayerImmobilized || !other.CompareTag("Player"))
        {
            return;
        }

        // �÷��̾� ĳ���Ϳ� ��Ʈ�ѷ� ������Ʈ�� ��� ��ȿ���� Ȯ���մϴ�.
        if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerController != null)
        {
            isPlayerImmobilized = true;

            // �÷��̾��� ���� �̵� �ӵ��� �����մϴ�.
            originalSpeed = PlayerCharacter.Instance.playerController.walkSpeed;

            // �̵� ���� �ڷ�ƾ�� �����մϴ�.
            StartCoroutine(ImmobilizePlayerRoutine(PlayerCharacter.Instance.playerController));
        }
    }

    /// <summary>
    /// �÷��̾��� �̵��� ���� �ð� ���� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="controller">�÷��̾� ��Ʈ�ѷ� ������Ʈ</param>
    private IEnumerator ImmobilizePlayerRoutine(PlayerController controller)
    {
        // �÷��̾� �̵� �ӵ��� 0���� �����Ͽ� �̵� �Ұ� ���·� ����ϴ�.
        controller.walkSpeed = 0f;


        // ������ �ð���ŭ ��ٸ��ϴ�.
        yield return new WaitForSeconds(immobilizeDuration);

        // �̵� ������ �����մϴ�.
        // �̶�, ���� �ӵ��� PlayerStats���� �ٽ� ���������� �Ͽ�, 
        // ��Ÿ�ӿ� �ӵ��� ����Ǿ����� �ֽ� ���� �����մϴ�.
        controller.walkSpeed = PlayerCharacter.Instance.playerStats.moveSpeed;

        isPlayerImmobilized = false;
    }

    /// <summary>
    /// ���� �ð� �� ������Ʈ�� �ı��ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    private IEnumerator SelfDestructRoutine()
    {
        // ����Ʈ�� ���ӵ� �ð���ŭ ��ٸ��ϴ�.
        yield return new WaitForSeconds(immobilizeDuration + 0.5f);
        Destroy(gameObject);
    }
}