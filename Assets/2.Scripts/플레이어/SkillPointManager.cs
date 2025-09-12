using UnityEngine;
using System.Collections.Generic;

// SkillPointManager�� �÷��̾��� ��ų ����Ʈ�� �����ϴ� �߾� ���� ��ũ��Ʈ�Դϴ�.
// ��ų ����Ʈ�� ���, ��ȯ, ����, ��� �� ��� ������ ����մϴ�.
public class SkillPointManager : MonoBehaviour
{
    // === �ܺ� ���� ===
    [Tooltip("�÷��̾��� ���� ���� �����͸� ��� �ִ� PlayerStats ��ũ��Ʈ�� �Ҵ��ϼ���.")]
    public PlayerStats playerStats;

    // === ���� ������ ===
    // ��ų �гο��� �ӽ÷� ���Ǵ� ��ų ����Ʈ. ����/��� �� �ʱ�ȭ�˴ϴ�.
    private int tempSkillPoints;
    // �ӽ÷� ����� ��ų ������ �����ϴ� Dictionary. key: ��ųID, value: �ӽ� ����
    private Dictionary<int, int> tempSkillLevels = new Dictionary<int, int>();
    // �ʱ�ȭ ���¸� �����Ͽ� Start()���� �� ���� ����ǵ��� �մϴ�.
    private bool isInitialized = false;

    // === �̺�Ʈ ===
    // ��ų ����Ʈ�� ����� �� UI�� �˸��� ���� �̺�Ʈ�Դϴ�.
    public event System.Action<int> OnSkillPointsChanged;

    void Start()
    {
        // ���� ���� �� �ʱ�ȭ �޼��带 ȣ���Ͽ� �⺻ ��ų ����Ʈ�� �����մϴ�.
        // �� �޼���� ��ų �г��� �� ������ ȣ��ǵ��� �����ϴ� ���� �� �����ϴ�.
        InitializePoints();
    }

    /// <summary>
    /// ��ų ����Ʈ�� ��ų ���� �����͸� �ʱ�ȭ�մϴ�.
    /// �÷��̾��� ���� �����͸� �ӽ� ������ �����մϴ�.
    /// </summary>
    public void InitializePoints()
    {
        // �г��� �� ������ �ʱ�ȭ ���¸� Ȯ���ϰ�, �̹� �ʱ�ȭ�Ǿ��ٸ� �ٽ� �������� �ʽ��ϴ�.
        if (isInitialized)
        {
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats�� �Ҵ���� �ʾҽ��ϴ�. �ν����� â���� �Ҵ��� �ּ���.");
            return;
        }

        // ���� ��ų ����Ʈ�� �ӽ� ������ ����
        tempSkillPoints = playerStats.skillPoints;
        // ���� ��ų ���� �����͸� �ӽ� Dictionary�� ����
        // ���� �����͸� �������� �ʱ� ���� ���� ���縦 �����մϴ�.
        tempSkillLevels = new Dictionary<int, int>(playerStats.skillLevels);
        isInitialized = true;

        // �ʱ� ��ų ����Ʈ�� UI�� �ݿ��ϵ��� �̺�Ʈ�� �߻���ŵ�ϴ�.
        OnSkillPointsChanged?.Invoke(tempSkillPoints);

        Debug.Log("��ų ����Ʈ �ý����� �ʱ�ȭ�Ǿ����ϴ�. ���� ��ų ����Ʈ: " + tempSkillPoints);
    }

    /// <summary>
    /// Ư�� ��ų�� �ӽ� ������ ������Ʈ�մϴ�.
    /// SkillConfirmationPanel���� ������/�ٿ� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="skillID">��ų�� ���� ID</param>
    /// <param name="level">���ο� �ӽ� ����</param>
    public void UpdateTempSkillLevel(int skillID, int level)
    {
        tempSkillLevels[skillID] = level;
    }

    /// <summary>
    /// Ư�� ��ų�� ���� �ӽ� ������ �����ɴϴ�.
    /// </summary>
    /// <param name="skillID">��ų�� ���� ID</param>
    /// <returns>��ų�� ���� �ӽ� ����. �����Ͱ� ������ 0�� ��ȯ�մϴ�.</returns>
    public int GetSkillCurrentLevel(int skillID)
    {
        // Dictionary�� �ش� ��ų�� �ִ��� Ȯ���ϰ�, ������ �⺻���� 0�� ��ȯ�մϴ�.
        return tempSkillLevels.ContainsKey(skillID) ? tempSkillLevels[skillID] : 0;
    }

    /// <summary>
    /// ���� ������ �ӽ� ��ų ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ӽ� ��ų ����Ʈ</returns>
    public int GetTempSkillPoints()
    {
        return tempSkillPoints;
    }

    /// <summary>
    /// ��ų ����Ʈ�� 1 ����մϴ�. (�ӽ÷� ����)
    /// </summary>
    public void SpendPoint()
    {
        tempSkillPoints--;
        // UI ������Ʈ�� ���� �̺�Ʈ�� �߻���ŵ�ϴ�.
        OnSkillPointsChanged?.Invoke(tempSkillPoints);
        Debug.Log("��ų ����Ʈ 1 ���! ���� ����Ʈ: " + tempSkillPoints);
    }

    /// <summary>
    /// ��ų ����Ʈ�� 1 �ǵ����ϴ�. (�ӽ÷� ����)
    /// </summary>
    public void RefundPoint()
    {
        tempSkillPoints++;
        // UI ������Ʈ�� ���� �̺�Ʈ�� �߻���ŵ�ϴ�.
        OnSkillPointsChanged?.Invoke(tempSkillPoints);
        Debug.Log("��ų ����Ʈ 1 ��ȯ! ���� ����Ʈ: " + tempSkillPoints);
    }

    /// <summary>
    /// ����� ��ų ����Ʈ�� ��ų ������ ���� ���� �����ϰ� �ʱ�ȭ�մϴ�.
    /// �� �޼���� '����' ��ư�� ����˴ϴ�.
    /// </summary>
    public void ApplyChanges()
    {
        // �ӽ� ����Ʈ�� ���� ���ȿ� �����մϴ�.
        playerStats.skillPoints = tempSkillPoints;
        // �ӽ� ��ų ������ ���� ���ȿ� �����մϴ�.
        playerStats.skillLevels = new Dictionary<int, int>(tempSkillLevels);

        Debug.Log("��ų ����Ʈ�� ��ų ������ ����Ǿ����ϴ�.");

        // �� ��ũ��Ʈ�� �ʱ�ȭ ���¸� �缳���Ͽ� ���� �г� Ȱ��ȭ �� �ٽ� �ʱ�ȭ�ǵ��� �մϴ�.
        isInitialized = false;
        // UI�� ���� ������ ������Ʈ
        OnSkillPointsChanged?.Invoke(playerStats.skillPoints);
    }

    /// <summary>
    /// ����� ��ų ����Ʈ�� ��� ����ϰ� ���� ������ �ǵ����ϴ�.
    /// �� �޼���� '���' ��ư�� ����˴ϴ�.
    /// </summary>
    public void DiscardChanges()
    {
        // �ӽ� ����Ʈ�� ���� ���� ������ �ǵ����ϴ�.
        tempSkillPoints = playerStats.skillPoints;
        // �ӽ� ��ų ������ ���� ���� ������ �ǵ����ϴ�.
        tempSkillLevels = new Dictionary<int, int>(playerStats.skillLevels);

        Debug.Log("��ų ����Ʈ ������ ��ҵǾ����ϴ�. ���� ������ ����.");

        // �� ��ũ��Ʈ�� �ʱ�ȭ ���¸� �缳���Ͽ� ���� �г� Ȱ��ȭ �� �ٽ� �ʱ�ȭ�ǵ��� �մϴ�.
        isInitialized = false;
        // UI�� ���� ������ ������Ʈ
        OnSkillPointsChanged?.Invoke(tempSkillPoints);
    }

    /// <summary>
    /// ������ ���� ������ �÷��̾�� ���ο� ��ų ����Ʈ�� �߰��մϴ�.
    /// �� �޼���� �÷��̾� ������ �ý��ۿ��� ȣ���ϴ� ���� �����ϴ�.
    /// </summary>
    /// <param name="amount">�߰��� ��ų ����Ʈ ��</param>
    public void AddPoints(int amount)
    {
        playerStats.skillPoints += amount;
        // tempSkillPoints�� Ȱ��ȭ ������ ��� �Բ� ������ŵ�ϴ�.
        if (isInitialized)
        {
            tempSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(tempSkillPoints);
        }
        Debug.Log($"��ų ����Ʈ {amount}���� �߰��Ǿ����ϴ�. ���� �� ����Ʈ: {playerStats.skillPoints}");
    }
}