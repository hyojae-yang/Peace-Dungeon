using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// �� ��ũ��Ʈ�� ��ų ����Ʈ �ý����� �ٽ� ������ �����մϴ�.
// ��ų ����Ʈ�� �ø��� ������ ���, ���� ������ Ȯ���ϰų� ����ϴ� ����� ����մϴ�.
// �̱��� ������ �����Ͽ� ��𼭵� ���� �����ϵ��� �����մϴ�.
public class SkillPointManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    private static SkillPointManager _instance;
    public static SkillPointManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SkillPointManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SkillPointManagerSingleton");
                    _instance = singletonObject.AddComponent<SkillPointManager>();
                    Debug.Log("���ο� 'SkillPointManagerSingleton' ���� ������Ʈ�� �����߽��ϴ�.");
                }
            }
            return _instance;
        }
    }

    // === ��ũ��Ʈ �ʱ�ȭ ===
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        // ��ũ��Ʈ�� ���۵� �� �ӽ� ��ų ������ �ʱ�ȭ
        InitializePoints();
    }

    // === UI �� ��ũ��Ʈ ���� ===
    [Header("UI �� ��ũ��Ʈ ����")]
    [Tooltip("��ų ����Ʈ�� ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI skillPointText;

    // �� ��ũ��Ʈ���� ���� �̱������� �����ϹǷ� �ν����� �Ҵ��� �ʿ� �����ϴ�.
    // [Tooltip("�÷��̾��� PlayerStats ������Ʈ�� �Ҵ��ϼ���.")]
    // public PlayerStats playerStats;
    // [Tooltip("�нú� ��ų ȿ���� ������Ʈ�� PassiveSkillManager ������Ʈ�� �Ҵ��ϼ���.")]
    // public PassiveSkillManager passiveSkillManager;

    // 'currentSkillPoints'�� ���� �÷��̾ ������ ��ų ����Ʈ�Դϴ�.
    private int currentSkillPoints;

    // 'tempSkillLevels'�� �гο��� �ӽ÷� �����ϴ� ��ų ������ �����մϴ�.
    private Dictionary<int, int> tempSkillLevels;

    // ��ų ����Ʈ ������ �ܺο� �˸��� �̺�Ʈ
    public event System.Action<int> OnSkillPointsChanged;

    // ��ų ������ ����Ǿ����� �ܺο� �˸��� ���ο� �̺�Ʈ
    public event System.Action<int> OnSkillLeveledUp;

    // === �ܺο��� ȣ��Ǵ� �޼��� ===

    /// <summary>
    /// ��ų �г��� ���� ������ ȣ��Ǿ�, ��ų ����Ʈ�� �ʱ�ȭ�ϰ� �ӽ� �����͸� �����մϴ�.
    /// </summary>
    public void InitializePoints()
    {
        // PlayerStats.Instance�� ���� �����Ϳ� ����
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // ���� ��ų ����Ʈ�� ��ų ������ �ӽ� �����ͷ� ������ �ʱ�ȭ�մϴ�.
        currentSkillPoints = PlayerStats.Instance.skillPoints;
        // ���� ����(Deep Copy)�� ���� ���� ��ųʸ��� ��ȣ�մϴ�.
        tempSkillLevels = new Dictionary<int, int>(PlayerStats.Instance.skillLevels);

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
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

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
        int permanentLevel = PlayerStats.Instance.skillLevels.ContainsKey(skillId) ? PlayerStats.Instance.skillLevels[skillId] : 0;

        return tempLevel > permanentLevel;
    }

    /// <summary>
    /// Ư�� ��ų�� ��� �� �ִ� ���� ������ �����Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="skillData">Ȯ���� ��ų ������</param>
    /// <returns>���� ������ �����Ǹ� true, �ƴϸ� false</returns>
    public bool CanLearnSkill(SkillData skillData)
    {
        if (PlayerStats.Instance == null || skillData == null)
        {
            Debug.LogError("PlayerStats �Ǵ� SkillData�� ��ȿ���� �ʽ��ϴ�.");
            return false;
        }

        return skillData.requiredLevel <= PlayerStats.Instance.level;
    }

    /// <summary>
    /// ����� ��ų ������ ���������� �����մϴ�.
    /// </summary>
    public void ApplyChanges()
    {
        List<int> leveledUpSkillIds = new List<int>();
        foreach (var tempLevelPair in tempSkillLevels)
        {
            int skillId = tempLevelPair.Key;
            int tempLevel = tempLevelPair.Value;
            int permanentLevel = PlayerStats.Instance.skillLevels.ContainsKey(skillId) ? PlayerStats.Instance.skillLevels[skillId] : 0;

            if (tempLevel > permanentLevel)
            {
                leveledUpSkillIds.Add(skillId);
            }
        }

        PlayerStats.Instance.skillLevels = new Dictionary<int, int>(tempSkillLevels);
        PlayerStats.Instance.skillPoints = currentSkillPoints;

        foreach (int skillId in leveledUpSkillIds)
        {
            OnSkillLeveledUp?.Invoke(skillId);
        }

        // PassiveSkillManager�� �̱������� ����Ǹ� �Ʒ� �ּ��� �����ϼ���.
        // if (PassiveSkillManager.Instance != null)
        // {
        //     PassiveSkillManager.Instance.UpdatePassiveBonuses();
        // }

        Debug.Log("��ų ���� ��������� ����Ǿ����ϴ�.");
    }

    /// <summary>
    /// ���� ������ ����ϰ� ���� ���·� �ǵ����ϴ�.
    /// </summary>
    public void DiscardChanges()
    {
        InitializePoints();
        Debug.Log("��ų ��������� ��ҵǾ����ϴ�.");
    }

    /// <summary>
    /// ��ų ����Ʈ UI�� ������Ʈ�ϰ� �̺�Ʈ�� �߻���ŵ�ϴ�.
    /// </summary>
    private void UpdateSkillPointUI()
    {
        if (skillPointText != null)
        {
            skillPointText.text = $"��ų����Ʈ: \n{currentSkillPoints}";
        }
        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }
}