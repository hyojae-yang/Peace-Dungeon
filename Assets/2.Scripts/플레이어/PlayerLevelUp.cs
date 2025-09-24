using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// �÷��̾��� ����ġ�� �������� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� �� �̻� �̱����� �ƴϸ�, PlayerCharacter�� ����� �����˴ϴ�.
/// </summary>
public class PlayerLevelUp : MonoBehaviour
{
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    // ���� ������ �ʿ��� ����ġ���� ����ϴ� �� ���Ǵ� ����
    [Header("������ ���� ����")]
    [Tooltip("���� ������ �ʿ��� �⺻ ����ġ���Դϴ�.")]
    public float baseExp = 10f;
    [Tooltip("������ �������� ����ġ�� �����ϴ� �����Դϴ�.")]
    public float expGrowthFactor = 1.3f;
    // === �̺�Ʈ ���� ===
    /// <summary>
    /// �÷��̾ ���������� �� �ܺο� �˸��� �̺�Ʈ�Դϴ�.
    /// </summary>
    public static event System.Action OnPlayerLeveledUp;

    void Start()
    {
        // PlayerCharacter�� �ν��Ͻ��� �����ͼ� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerStats�� �ʱ�ȭ���� �ʾҽ��ϴ�. PlayerLevelUp ��ũ��Ʈ�� ����� �������� ���� �� �ֽ��ϴ�.");
            return;
        }

        // ���� ���� �� �ʱ� requiredExperience�� �����մϴ�.
        CalculateRequiredExperience();
    }

    /// <summary>
    /// �ܺο��� ȣ���Ͽ� �÷��̾�� ����ġ�� �߰��ϴ� �޼���
    /// </summary>
    /// <param name="amount">�߰��� ����ġ��</param>
    public void AddExperience(float amount)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("�÷��̾� ���ȿ� ������ �� �����ϴ�. ����ġ �߰� ����.");
            return;
        }

        playerCharacter.playerStats.experience += (int)amount;

        CheckForLevelUp();
    }

    /// <summary>
    /// ���� ������ �ʿ��� ����ġ���� ����Ͽ� PlayerStats�� �����մϴ�.
    /// </summary>
    public void CalculateRequiredExperience()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        // PlayerStats�� ���� �����Ϳ� �����մϴ�.
        // ������ ����: �ʿ��� ����ġ = baseExp * (expGrowthFactor ^ (level - 1))
        playerCharacter.playerStats.requiredExperience = baseExp * Mathf.Pow(expGrowthFactor, playerCharacter.playerStats.level - 1);
    }

    /// <summary>
    /// ����ġ�� Ȯ���ϰ� �������� �������� üũ�ϴ� �޼���
    /// </summary>
    private void CheckForLevelUp()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        while (playerCharacter.playerStats.experience >= playerCharacter.playerStats.requiredExperience)
        {
            LevelUp();
            CalculateRequiredExperience();
        }
    }

    /// <summary>
    /// �÷��̾ ��������Ű�� �޼���
    /// </summary>
    private void LevelUp()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("�÷��̾� ���ȿ� ������ �� �����ϴ�. ������ ����.");
            return;
        }

        float remainingExp = playerCharacter.playerStats.experience - playerCharacter.playerStats.requiredExperience;

        // ������ ����ġ�� ������Ʈ�մϴ�.
        playerCharacter.playerStats.level++;
        playerCharacter.playerStats.experience = (int)remainingExp;

        // ������ �� ���� ����Ʈ�� �����մϴ�. (���� ���� ����)
        if (playerCharacter.playerStatSystem != null)
        {
            playerCharacter.playerStatSystem.statPoints += 5;

            // �������� ���� ���� ���� ������ PlayerStatSystem�� �����մϴ�.
            playerCharacter.playerStatSystem.UpdateFinalStats();
            playerCharacter.playerStatSystem.StoreTempStats();
        }

        // ������ �� ��ų ����Ʈ�� �����մϴ�. (���� ���� ����)
        if (playerCharacter.playerStats != null)
        {
            playerCharacter.playerStats.skillPoints += 2;
        }

        // ü�� �� ���� ȸ�� (���� ���� ����)
        playerCharacter.playerStats.health = playerCharacter.playerStats.MaxHealth;
        playerCharacter.playerStats.mana = playerCharacter.playerStats.MaxMana;
        // �������� �Ϸ�Ǿ����� �ܺο� �˸��� �̺�Ʈ�� �߻���ŵ�ϴ�.
        OnPlayerLeveledUp?.Invoke();
    }
}