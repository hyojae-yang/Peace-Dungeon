using UnityEngine;
using System.Collections.Generic;

// IDetectable�� IDamageable �������̽��� �����մϴ�.
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // PlayerStats ��ũ��Ʈ�� ���۷����� ���� ����
    private PlayerStats playerStats;

    void Start()
    {
        // ���� ���� ��, ���� ���� ������Ʈ�� ������ PlayerStats ������Ʈ�� ã���ϴ�.
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats ������Ʈ�� PlayerHealth�� ���� ���� ������Ʈ�� �����ϴ�. PlayerStats�� ���� ������ �ּ���.");
        }
    }

    // IDetectable �������̽��� �޼��� ����
    public bool IsDetectable()
    {
        // �÷��̾ ����ִٸ� ���� �����ϵ��� true�� ��ȯ�մϴ�.
        if (playerStats != null)
        {
            return playerStats.health > 0;
        }
        return false;
    }

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
        if (playerStats == null) return;

        // PlayerStats�� health ������ ���� �����Ͽ� �������� �����մϴ�.
        playerStats.health -= amount;

        Debug.Log("�÷��̾ �������� �Ծ����ϴ�! ���� ü��: " + playerStats.health);

        // ü���� 0���� �۰ų� �������� ���� ó��
        if (playerStats.health <= 0)
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
        if (playerStats == null) return;

        float finalDamage = amount;

        // ������ Ÿ�Կ� ���� ���� ����
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(amount - playerStats.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(amount - playerStats.magicDefense, 0);
                break;
            case DamageType.True:
                // ���� ���ش� ������ �����մϴ�.
                break;
        }

        playerStats.health -= finalDamage;

        Debug.Log($"�÷��̾ {finalDamage}�� {type} ���ظ� �Ծ����ϴ�! ���� ü��: {playerStats.health}");

        if (playerStats.health <= 0)
        {
            Die();
        }
    }

    // �÷��̾ �׾��� �� ȣ��� �޼���
    private void Die()
    {
        Debug.Log("�÷��̾ ����߽��ϴ�!");

        // ���⿡ ���� ����, �÷��̾� ������Ʈ �ı� �� �߰� ������ �����մϴ�.
        Destroy(gameObject);
    }
}