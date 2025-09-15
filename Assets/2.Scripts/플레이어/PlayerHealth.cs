using UnityEngine;
using System.Collections.Generic;

// IDetectable�� IDamageable �������̽��� �����մϴ�.
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // PlayerStats ��ũ��Ʈ�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // private PlayerStats playerStats;

    void Start()
    {
        // ���� ���� ��, PlayerStats �̱��� �ν��Ͻ��� �����ϴ��� Ȯ���մϴ�.
        // GetComponent�� ���� ������ �ʿ䰡 �����ϴ�.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� �������� �ʽ��ϴ�. ���� ���� �� PlayerStats�� ���� ���� ������Ʈ�� ���� �ִ��� Ȯ���� �ּ���.");
        }
    }

    // IDetectable �������̽��� �޼��� ����

    /// <summary>
    /// �÷��̾ ���� ������ �������� Ȯ���մϴ�.
    /// </summary>
    public bool IsDetectable()
    {
        // �÷��̾ ����ִٸ� ���� �����ϵ��� true�� ��ȯ�մϴ�.
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.health > 0;
        }
        return false;
    }

    /// <summary>
    /// �� ������Ʈ�� Ʈ�������� ��ȯ�մϴ�.
    /// </summary>
    public Transform GetTransform()
    {
        return this.transform;
    }

    // IDamageable �������̽��� �޼��� ���� (�����ε�)

    /// <summary>
    /// ���� ������ ���� �޴� �޼����Դϴ�. (���� ��� ����)
    /// </summary>
    /// <param name="amount">���� ��������</param>
    public void TakeDamage(float amount)
    {
        // PlayerStats �ν��Ͻ��� ��ȿ���� �ٽ� �ѹ� Ȯ���մϴ�.
        if (PlayerStats.Instance == null) return;

        // PlayerStats�� health ������ ���� �����Ͽ� �������� �����մϴ�.
        PlayerStats.Instance.health -= amount;

        Debug.Log("�÷��̾ �������� �Ծ����ϴ�! ���� ü��: " + PlayerStats.Instance.health);

        // ü���� 0���� �۰ų� �������� ���� ó��
        if (PlayerStats.Instance.health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ������ Ÿ�Կ� ���� �÷��̾��� ������ �����Ͽ� ���ظ� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="amount">���� ��������</param>
    /// <param name="type">������ Ÿ�� (����, ����, ���� ���� ��)</param>
    public void TakeDamage(float amount, DamageType type)
    {
        // PlayerStats �ν��Ͻ��� ��ȿ���� �ٽ� �ѹ� Ȯ���մϴ�.
        if (PlayerStats.Instance == null) return;

        float finalDamage = amount;

        // ������ Ÿ�Կ� ���� ���� ����
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(amount - PlayerStats.Instance.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(amount - PlayerStats.Instance.magicDefense, 0);
                break;
            case DamageType.True:
                // ���� ���ش� ������ �����մϴ�.
                break;
        }

        PlayerStats.Instance.health -= finalDamage;

        Debug.Log($"�÷��̾ {finalDamage}�� {type} ���ظ� �Ծ����ϴ�! ���� ü��: {PlayerStats.Instance.health}");

        if (PlayerStats.Instance.health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// �÷��̾ �׾��� �� ȣ��� �޼���
    /// </summary>
    private void Die()
    {
        Debug.Log("�÷��̾ ����߽��ϴ�!");

        // ���⿡ ���� ����, �÷��̾� ������Ʈ �ı� �� �߰� ������ �����մϴ�.
        Destroy(gameObject);
    }
}