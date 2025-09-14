using UnityEngine;

// �÷��̾��� ����ġ�� �������� �����ϴ� ��ũ��Ʈ�Դϴ�.
public class PlayerLevelUp : MonoBehaviour
{
    // PlayerStats ��ũ��Ʈ ���� ����
    private PlayerStats playerStats;
    // PlayerStatSystem ��ũ��Ʈ ���� ����
    private PlayerStatSystem playerStatSystem;

    // ���� ������ �ʿ��� ����ġ���� ����ϴ� �� ���Ǵ� ����
    [Header("������ ���� ����")]
    [Tooltip("���� ������ �ʿ��� �⺻ ����ġ���Դϴ�.")]
    public float baseExp = 10f;
    [Tooltip("������ �������� ����ġ�� �����ϴ� �����Դϴ�.")]
    public float expGrowthFactor = 1.2f;

    void Start()
    {
        // ������ ���� ������Ʈ�� �ִ� PlayerStats ������Ʈ�� ã���ϴ�.
        playerStats = GetComponent<PlayerStats>();
        // ������ ���� ������Ʈ�� �ִ� PlayerStatSystem ������Ʈ�� ã���ϴ�.
        playerStatSystem = GetComponent<PlayerStatSystem>();

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats ������Ʈ�� PlayerLevelUp ��ũ��Ʈ�� ���� ���� ������Ʈ�� �����ϴ�.");
            return;
        }

        if (playerStatSystem == null)
        {
            Debug.LogError("PlayerStatSystem ������Ʈ�� PlayerLevelUp ��ũ��Ʈ�� ���� ���� ������Ʈ�� �����ϴ�.");
            return;
        }

        // ���� ���� �� �ʱ� requiredExperience�� �����մϴ�.
        CalculateRequiredExperience();
    }

    // �ܺο��� ȣ���Ͽ� �÷��̾�� ����ġ�� �߰��ϴ� �޼���
    public void AddExperience(float amount)
    {
        if (playerStats == null) return;

        playerStats.experience += (int)amount;

        CheckForLevelUp();
    }

    // ���� ������ �ʿ��� ����ġ���� ����Ͽ� PlayerStats�� �����մϴ�.
    private void CalculateRequiredExperience()
    {
        // ������ ����: �ʿ��� ����ġ = baseExp * (expGrowthFactor ^ (level - 1))
        playerStats.requiredExperience = baseExp * Mathf.Pow(expGrowthFactor, playerStats.level - 1);
    }

    // ����ġ�� Ȯ���ϰ� �������� �������� üũ�ϴ� �޼���
    private void CheckForLevelUp()
    {
        while (playerStats.experience >= playerStats.requiredExperience)
        {
            LevelUp();
            CalculateRequiredExperience();
            Debug.Log($"���� �������� ���� ����ġ: {playerStats.experience} / {playerStats.requiredExperience}");
        }
    }

    // �÷��̾ ��������Ű�� �޼���
    private void LevelUp()
    {
        float remainingExp = playerStats.experience - playerStats.requiredExperience;

        // ������ ����ġ�� ������Ʈ�մϴ�.
        playerStats.level++;
        playerStats.experience = (int)remainingExp;

        // ������ �� ���� ����Ʈ�� 5�� �����մϴ�.
        if (playerStatSystem != null)
        {
            playerStatSystem.statPoints += 5;
        }
        // ������ �� ��ų ����Ʈ�� 2�� �����մϴ�.
        if (playerStats != null)
        {
            playerStats.skillPoints += 2;
        }

        // �������� ���� ���� ���� ������ PlayerStatSystem�� �����մϴ�.
        playerStatSystem.UpdateFinalStats();
        playerStatSystem.StoreTempStats();

        Debug.Log($"�����մϴ�! ���� {playerStats.level}�� �������߽��ϴ�!");
    }
}