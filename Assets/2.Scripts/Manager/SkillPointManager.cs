using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// �� ��ũ��Ʈ�� ��ų ����Ʈ �ý����� �ٽ� ������ �����մϴ�.
// ��ų ����Ʈ�� �ø��� ������ ���, ���� ������ Ȯ���ϰų� ����ϴ� ����� ����մϴ�.
public class SkillPointManager : MonoBehaviour
{
    // === UI �� ��ũ��Ʈ ���� ===
    [Header("UI �� ��ũ��Ʈ ����")]
    [Tooltip("��ų ����Ʈ�� ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI skillPointText;
    [Tooltip("�÷��̾��� PlayerStats ������Ʈ�� �Ҵ��ϼ���.")]
    public PlayerStats playerStats;
    [Tooltip("�нú� ��ų ȿ���� ������Ʈ�� PassiveSkillManager ������Ʈ�� �Ҵ��ϼ���.")]
    public PassiveSkillManager passiveSkillManager;

    // === ��ų ����Ʈ �ý��� ������ ===
    // 'currentSkillPoints'�� ���� �÷��̾ ������ ��ų ����Ʈ�Դϴ�.
    private int currentSkillPoints;

    // 'tempSkillLevels'�� �гο��� �ӽ÷� �����ϴ� ��ų ������ �����մϴ�.
    private Dictionary<int, int> tempSkillLevels;

    // ��ų ����Ʈ ������ �ܺο� �˸��� �̺�Ʈ
    public event System.Action<int> OnSkillPointsChanged;

    // ��ų ������ ����Ǿ����� �ܺο� �˸��� ���ο� �̺�Ʈ
    public event System.Action<int> OnSkillLeveledUp; // <-- ���ο� �̺�Ʈ �߰�

    void Awake()
    {
        // PlayerStats�� �Ҵ�Ǿ����� Ȯ��
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats�� �Ҵ���� �ʾҽ��ϴ�. �ν����Ϳ��� �Ҵ��� �ּ���.");
            return;
        }

        // ��ũ��Ʈ�� ���۵� �� �ӽ� ��ų ������ �ʱ�ȭ
        InitializePoints();
    }

    // === �ܺο��� ȣ��Ǵ� �޼��� ===

    /// <summary>
    /// ��ų �г��� ���� ������ ȣ��Ǿ�, ��ų ����Ʈ�� �ʱ�ȭ�ϰ� �ӽ� �����͸� �����մϴ�.
    /// </summary>
    public void InitializePoints()
    {
        // ���� ��ų ����Ʈ�� ��ų ������ �ӽ� �����ͷ� ������ �ʱ�ȭ�մϴ�.
        currentSkillPoints = playerStats.skillPoints;
        // ���� ����(Deep Copy)�� ���� ���� ��ųʸ��� ��ȣ�մϴ�.
        tempSkillLevels = new Dictionary<int, int>(playerStats.skillLevels);

        // UI ������Ʈ
        UpdateSkillPointUI();
    }

    /// <summary>
    /// �ӽ� ��ų ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public int GetTempSkillPoints()
    {
        return currentSkillPoints;
    }

    /// <summary>
    /// Ư�� ��ų�� ���� �ӽ� ������ �����ɴϴ�.
    /// </summary>
    /// <param name="skillId">Ȯ���� ��ų�� ID</param>
    /// <returns>�ӽ� ����, ��ų�� ������ 0�� ��ȯ</returns>
    public int GetTempSkillLevel(int skillId)
    {
        // tempSkillLevels�� null�� ��� �ʱ�ȭ
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

        if (tempSkillLevels.ContainsKey(skillId))
        {
            return tempSkillLevels[skillId];
        }
        return 0; // ��ų�� ����� �ʾ��� ���
    }

    /// <summary>
    /// �ӽ� ��ų ����Ʈ�� 1 ���ҽ�ŵ�ϴ�.
    /// </summary>
    public void SpendPoint()
    {
        currentSkillPoints--;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// �ӽ� ��ų ����Ʈ�� 1 ������ŵ�ϴ�.
    /// </summary>
    public void RefundPoint()
    {
        currentSkillPoints++;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// �ӽ� ��ų ������ ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="skillId">������Ʈ�� ��ų ID</param>
    /// <param name="tempLevel">������Ʈ�� �ӽ� ����</param>
    public void UpdateTempSkillLevel(int skillId, int tempLevel)
    {
        // tempSkillLevels�� null�� ��� �ʱ�ȭ
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

        // �ӽ� ��ųʸ��� ��ų ������ ������Ʈ�մϴ�.
        if (tempLevel > 0)
        {
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels[skillId] = tempLevel;
            }
            else
            {
                tempSkillLevels.Add(skillId, tempLevel);
            }
        }
        else
        {
            // ������ 0�� �Ǹ� ��ųʸ����� ����
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels.Remove(skillId);
            }
        }
    }

    /// <summary>
    /// ��ų �����ٿ��� �������� Ȯ���մϴ�.
    /// </summary>
    /// <param name="skillId">Ȯ���� ��ų�� ID</param>
    /// <returns>���� �ٿ� ���� �� true, �ƴϸ� false</returns>
    public bool CanLevelDown(int skillId)
    {
        int tempLevel = GetTempSkillLevel(skillId);
        int permanentLevel = playerStats.skillLevels.ContainsKey(skillId) ? playerStats.skillLevels[skillId] : 0;

        // �ӽ� ������ ���� �������� ���� ���� ���� �ٿ� ����
        return tempLevel > permanentLevel;
    }

    /// <summary>
    /// Ư�� ��ų�� ��� �� �ִ� ���� ������ �����Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="skillData">Ȯ���� ��ų ������</param>
    /// <returns>���� ������ �����Ǹ� true, �ƴϸ� false</returns>
    public bool CanLearnSkill(SkillData skillData)
    {
        // PlayerStats�� skillData�� ��ȿ���� Ȯ��
        if (playerStats == null || skillData == null)
        {
            Debug.LogError("PlayerStats �Ǵ� SkillData�� ��ȿ���� �ʽ��ϴ�.");
            return false;
        }

        // ��ų�� �䱸�ϴ� ����(requiredLevel)�� �÷��̾��� ���� ����(level)�� ��
        return skillData.requiredLevel <= playerStats.level;
    }

    // === ������� ���� ���� �� ��� �޼��� ===

    /// <summary>
    /// ����� ��ų ������ ���������� �����մϴ�.
    /// </summary>
    public void ApplyChanges()
    {
        // === ���� �ݿ� ���� ===

        // 1. �������� ��ų���� �ӽ÷� �����մϴ�. (������ ���� ���� �����ؾ� ��)
        List<int> leveledUpSkillIds = new List<int>();
        foreach (var tempLevelPair in tempSkillLevels)
        {
            int skillId = tempLevelPair.Key;
            int tempLevel = tempLevelPair.Value;
            int permanentLevel = playerStats.skillLevels.ContainsKey(skillId) ? playerStats.skillLevels[skillId] : 0;

            // �ӽ� ������ ���� �������� ������ �������� ��ų�� �����մϴ�.
            if (tempLevel > permanentLevel)
            {
                leveledUpSkillIds.Add(skillId);
            }
        }

        // 2. �ӽ� �����͸� ���� �����Ϳ� �ݿ��մϴ�.
        playerStats.skillLevels = new Dictionary<int, int>(tempSkillLevels);
        playerStats.skillPoints = currentSkillPoints;

        // 3. �������� ��ų�鿡 ���� UI ������Ʈ �̺�Ʈ�� ȣ���մϴ�.
        // �����Ͱ� ���� �ݿ��� �Ŀ� �̺�Ʈ�� ȣ���ؾ� PlayerSkillController�� �ֽ� �����͸� ������ �� �ֽ��ϴ�.
        foreach (int skillId in leveledUpSkillIds)
        {
            OnSkillLeveledUp?.Invoke(skillId);
        }

        // ���� �ݿ� �� ��ų ȿ���� ������Ʈ�մϴ�.
        if (passiveSkillManager != null)
        {
            passiveSkillManager.UpdatePassiveBonuses();
        }

        Debug.Log("��ų ���� ��������� ����Ǿ����ϴ�.");
    }

    /// <summary>
    /// ���� ������ ����ϰ� ���� ���·� �ǵ����ϴ�.
    /// </summary>
    public void DiscardChanges()
    {
        // ������ ���� �����͸� �ٽ� �ӽ� �����ͷ� �����մϴ�.
        InitializePoints();
        Debug.Log("��ų ��������� ��ҵǾ����ϴ�.");
    }

    /// <summary>
    /// ��ų ����Ʈ UI�� ������Ʈ�ϰ� �̺�Ʈ�� �߻���ŵ�ϴ�.
    /// </summary>
    private void UpdateSkillPointUI()
    {
        // UI�� �Ҵ�Ǿ� �ִٸ� �ؽ�Ʈ�� ������Ʈ�մϴ�.
        if (skillPointText != null)
        {
            skillPointText.text = $"��ų����Ʈ: \n{currentSkillPoints}";
        }
        // �ܺ� ������(SkillPanel)���� ���� ������ �˸��ϴ�.
        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }
}