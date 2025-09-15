using UnityEngine;
using System.Collections.Generic;

// �÷��̾��� ����ġ�� �������� �����ϴ� ��ũ��Ʈ�Դϴ�.
public class PlayerLevelUp : MonoBehaviour
{
    // PlayerStats�� PlayerStatSystem ��ũ��Ʈ�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // private PlayerStats playerStats;
    // private PlayerStatSystem playerStatSystem;

    // ���� ������ �ʿ��� ����ġ���� ����ϴ� �� ���Ǵ� ����
    [Header("������ ���� ����")]
    [Tooltip("���� ������ �ʿ��� �⺻ ����ġ���Դϴ�.")]
    public float baseExp = 10f;
    [Tooltip("������ �������� ����ġ�� �����ϴ� �����Դϴ�.")]
    public float expGrowthFactor = 1.2f;

    void Start()
    {
        // ���� ���� �� PlayerStats�� PlayerStatSystem �̱��� �ν��Ͻ��� �����ϴ��� Ȯ���մϴ�.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� �������� �ʽ��ϴ�. ���� ���� �� �ش� ������Ʈ�� ���� ���� ������Ʈ�� ���� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        if (PlayerStatSystem.Instance == null)
        {
            Debug.LogError("PlayerStatSystem �ν��Ͻ��� �������� �ʽ��ϴ�. ���� ���� �� �ش� ������Ʈ�� ���� ���� ������Ʈ�� ���� �ִ��� Ȯ���� �ּ���.");
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
        if (PlayerStats.Instance == null) return;

        PlayerStats.Instance.experience += (int)amount;

        CheckForLevelUp();
    }

    /// <summary>
    /// ���� ������ �ʿ��� ����ġ���� ����Ͽ� PlayerStats�� �����մϴ�.
    /// </summary>
    private void CalculateRequiredExperience()
    {
        // PlayerStats.Instance�� ���� �����Ϳ� �����մϴ�.
        // ������ ����: �ʿ��� ����ġ = baseExp * (expGrowthFactor ^ (level - 1))
        PlayerStats.Instance.requiredExperience = baseExp * Mathf.Pow(expGrowthFactor, PlayerStats.Instance.level - 1);
    }

    /// <summary>
    /// ����ġ�� Ȯ���ϰ� �������� �������� üũ�ϴ� �޼���
    /// </summary>
    private void CheckForLevelUp()
    {
        if (PlayerStats.Instance == null) return;

        while (PlayerStats.Instance.experience >= PlayerStats.Instance.requiredExperience)
        {
            LevelUp();
            CalculateRequiredExperience();
            Debug.Log($"���� �������� ���� ����ġ: {PlayerStats.Instance.experience} / {PlayerStats.Instance.requiredExperience}");
        }
    }

    /// <summary>
    /// �÷��̾ ��������Ű�� �޼���
    /// </summary>
    private void LevelUp()
    {
        if (PlayerStats.Instance == null || PlayerStatSystem.Instance == null) return;

        float remainingExp = PlayerStats.Instance.experience - PlayerStats.Instance.requiredExperience;

        // ������ ����ġ�� ������Ʈ�մϴ�.
        PlayerStats.Instance.level++;
        PlayerStats.Instance.experience = (int)remainingExp;

        // ������ �� ���� ����Ʈ�� �����մϴ�.
        PlayerStatSystem.Instance.statPoints += 5;

        // ������ �� ��ų ����Ʈ�� �����մϴ�.
        PlayerStats.Instance.skillPoints += 2;

        // �������� ���� ���� ���� ������ PlayerStatSystem�� �����մϴ�.
        PlayerStatSystem.Instance.UpdateFinalStats();
        PlayerStatSystem.Instance.StoreTempStats();

        Debug.Log($"�����մϴ�! ���� {PlayerStats.Instance.level}�� �������߽��ϴ�!");
    }
}