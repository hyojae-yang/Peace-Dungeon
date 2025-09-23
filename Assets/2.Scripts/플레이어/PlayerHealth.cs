using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// IDetectable�� IDamageable �������̽��� �����ϸ�, �÷��̾��� ü�� �� ��� ������ �����մϴ�.
/// �� ��ũ��Ʈ�� �� �̻� �̱����� �ƴϸ�, PlayerCharacter�� ����� �����˴ϴ�.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    void Start()
    {
        // PlayerCharacter�� �ν��Ͻ��� �����ͼ� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerStats�� �ʱ�ȭ���� �ʾҽ��ϴ�. PlayerHealth ��ũ��Ʈ�� ����� �������� ���� �� �ֽ��ϴ�.");
        }
    }

    // IDetectable �������̽��� �޼��� ����

    /// <summary>
    /// �÷��̾ ���� ������ �������� Ȯ���մϴ�.
    /// </summary>
    public bool IsDetectable()
    {
        // PlayerCharacter �� playerStats�� ��ȿ���� ���� Ȯ���մϴ�.
        if (playerCharacter != null && playerCharacter.playerStats != null)
        {
            // �÷��̾ ����ִٸ� ���� �����ϵ��� true�� ��ȯ�մϴ�.
            return playerCharacter.playerStats.health > 0;
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
    /// ���� ������ ���� �޴� �޼����Դϴ�.
    /// </summary>
    /// <param name="amount">���� ��������</param>
    public void TakeDamage(float amount)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("�÷��̾� ���ȿ� ������ �� �����ϴ�. TakeDamage(float amount) ����.");
            return;
        }

        // PlayerStats�� health ������ ���� �����Ͽ� �������� �����մϴ�.
        playerCharacter.playerStats.health -= amount;


        // ü���� 0���� �۰ų� �������� ���� ó��
        if (playerCharacter.playerStats.health <= 0)
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
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("�÷��̾� ���ȿ� ������ �� �����ϴ�. TakeDamage(float amount, DamageType type) ����.");
            return;
        }

        float finalDamage = amount;

        // ������ Ÿ�Կ� ���� ���� ����
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(amount - playerCharacter.playerStats.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(amount - playerCharacter.playerStats.magicDefense, 0);
                break;
            case DamageType.True:
                // ���� ���ش� ������ �����մϴ�.
                break;
        }

        playerCharacter.playerStats.health -= finalDamage;

        if (playerCharacter.playerStats.health <= 0)
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
        gameObject.SetActive(false);
    }
}