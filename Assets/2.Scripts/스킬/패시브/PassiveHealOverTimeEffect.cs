using UnityEngine;
using System.Collections;

// �÷��̾��� ü���� �ִ� ü���� ������ŭ �ֱ������� ȸ����Ű�� ���� �нú� ��ų�Դϴ�.
// IPassiveEffect �������̽��� �����մϴ�.
public class PassiveHealOverTimeEffect : MonoBehaviour, IPassiveEffect
{
    // === ���� ���� �� ���� ===
    [Header("����")]
    // �� ��ũ��Ʈ�� ������ �÷��̾� ���ӿ�����Ʈ�� PlayerStats ������Ʈ�� �����մϴ�.
    private PlayerStats playerStats;

    [Header("����")]
    [Tooltip("��ų ������ ���� �ʴ� ü�� ȸ�� ����(%). �� ������ ������ �迭�� �����մϴ�.")]
    // ��: 1����(1%), 2����(2%), 3����(3%), 4����(4%), 5����(5%)
    public float[] healPercentagePerLevel;

    [Tooltip("ȸ�� ȿ���� ƽ �ֱ�(��). ��: 1�̸� 1�ʸ��� ü�� ȸ��")]
    [SerializeField] private float tickRate = 1f;

    private Coroutine healCoroutine; // �ڷ�ƾ�� �����ϱ� ���� ����
    private int currentSkillLevel; // ���� ��ų ������ �����ϴ� ����

    private void Awake()
    {
        // �θ� ���ӿ�����Ʈ(�÷��̾�)���� PlayerStats ������Ʈ�� ã���ϴ�.
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats ������Ʈ�� ã�� �� �����ϴ�. �� ��ũ��Ʈ�� PlayerStats�� �ִ� ������Ʈ�� �����Ǿ�� �մϴ�.");
        }
    }

    // IPassiveEffect �������̽��� �����ϴ� �ٽ� �޼����Դϴ�.
    // PassiveSkillData.cs���� �� ��ũ��Ʈ�� ȣ���� �� ���˴ϴ�.
    // ������ �ڷ�ƾ ������ �����ϵ��� �����߽��ϴ�.
    public void ExecuteEffect(SkillLevelInfo skillLevelInfo, PlayerStats playerStats)
    {
        // �� �ý��ۿ����� skillLevelInfo�� ������� �����Ƿ�,
        // ���� ��ų ������ �ӽ÷� 1�� �����մϴ�.
        // �� ���� ���� SkillPointManager�� ���� ����� �� �ֽ��ϴ�.
        currentSkillLevel = 1;

        // ApplyEffect�� ȣ���Ͽ� ���� ü�� ȸ�� �ڷ�ƾ�� �����մϴ�.
        ApplyEffect(currentSkillLevel);
    }

    /// <summary>
    /// ��ų ȿ���� Ȱ��ȭ�ϰ�, ���� ������ ���� �ڷ�ƾ�� �����մϴ�.
    /// </summary>
    /// <param name="skillLevel">���� ��ų ���� (1���� ����)</param>
    public void ApplyEffect(int skillLevel)
    {
        // ù ���� ��, ������ �����ϰ� �ڷ�ƾ�� �����մϴ�.
        currentSkillLevel = skillLevel;
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
        }
        healCoroutine = StartCoroutine(HealOverTime());
        Debug.Log($"ü�� ȸ�� �нú� ��ų �ߵ�! ����: {skillLevel}, ȸ�� ����: {GetHealPercentage()}%");
    }

    /// <summary>
    /// ��ų ȿ���� �����Ͽ� ü�� ȸ�� �ڷ�ƾ�� �����մϴ�.
    /// </summary>
    public void RemoveEffect()
    {
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null; // �ڷ�ƾ ������ null�� ����
        }
    }

    /// <summary>
    /// ��ų�� ������ ����� �� ȿ���� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="newSkillLevel">����� ��ų ����</param>
    public void UpdateLevel(int newSkillLevel)
    {
        currentSkillLevel = newSkillLevel;
        Debug.Log($"ü�� ȸ�� �нú� ��ų ������! ���ο� ����: {currentSkillLevel}, ȸ�� ����: {GetHealPercentage()}%");
    }

    /// <summary>
    /// ���� ��ų ������ �ش��ϴ� ü�� ȸ�� ������ �������� ���� �޼����Դϴ�.
    /// </summary>
    /// <returns>���� ��ų ������ ü�� ȸ�� ����</returns>
    private float GetHealPercentage()
    {
        if (currentSkillLevel > 0 && currentSkillLevel <= healPercentagePerLevel.Length)
        {
            return healPercentagePerLevel[currentSkillLevel - 1];
        }
        Debug.LogError($"��ȿ���� ���� ��ų �����Դϴ�: {currentSkillLevel}");
        return 0f;
    }

    /// <summary>
    /// ���� �ð����� �÷��̾��� ü���� ȸ����Ű�� �ڷ�ƾ�Դϴ�.
    /// </summary>
    private IEnumerator HealOverTime()
    {
        while (true)
        {
            // playerStats�� null�� �ƴ��� �׻� Ȯ���Ͽ� �����ϰ� �ڵ带 �����մϴ�.
            if (playerStats != null)
            {
                // ���� ������ �ش��ϴ� ȸ�� ������ �����ͼ� ����մϴ�.
                float healPercentage = GetHealPercentage();
                // ����ڴ��� PlayerStats ��ũ��Ʈ�� �������� ����մϴ�.
                float healAmount = playerStats.MaxHealth * (healPercentage / 100f);

                // ���� ü�¿� ȸ������ ���մϴ�.
                // ����ڴ��� PlayerStats ��ũ��Ʈ�� �������� ����մϴ�.
                playerStats.health += healAmount;

                // ü���� �ִ� ü���� �ʰ����� �ʵ��� �����մϴ�.
                // ����ڴ��� PlayerStats ��ũ��Ʈ�� �������� ����մϴ�.
                playerStats.health = Mathf.Min(playerStats.health, playerStats.MaxHealth);

                // ����׿� �α�
                Debug.Log($"ü�� {healAmount:F2} ȸ��. ���� ü��: {playerStats.health:F2}");
            }
            // ���� ƽ���� ����մϴ�.
            yield return new WaitForSeconds(tickRate);
        }
    }
}