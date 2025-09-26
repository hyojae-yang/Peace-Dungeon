using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �� ��ũ��Ʈ�� ��ų ����Ʈ �ý����� �ٽ� ������ �����մϴ�.
/// ��ų ����Ʈ�� �ø��� ������ ���, ���� ������ Ȯ���ϰų� ����ϴ� ����� ����մϴ�.
/// �̱��� ������ �����Ͽ� ��𼭵� ���� �����ϵ��� �����մϴ�.
/// </summary>
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

    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

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

        // ��ũ��Ʈ�� ���۵� �� PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;

    }

    // === UI �� ��ũ��Ʈ ���� ===
    [Header("UI �� ��ũ��Ʈ ����")]
    [Tooltip("��ų ����Ʈ�� ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI skillPointText;

    // 'currentSkillPoints'�� ���� �÷��̾ ������ ��ų ����Ʈ�Դϴ�.
    public int currentSkillPoints;

    // 'tempSkillLevels'�� �гο��� �ӽ÷� �����ϴ� ��ų ������ �����մϴ�.
    private Dictionary<int, int> tempSkillLevels;

    // ��ų ����Ʈ ������ �ܺο� �˸��� �̺�Ʈ
    public event System.Action<int> OnSkillPointsChanged;

    // ��ų ������ ����Ǿ����� �ܺο� �˸��� ���ο� �̺�Ʈ
    public event System.Action<int> OnSkillLeveledUp;

    private void Start()
    {
        // ���� ��ų ����Ʈ�� ��ų ������ �ӽ� �����ͷ� ������ �ʱ�ȭ�մϴ�.
        currentSkillPoints = playerCharacter.playerStats.skillPoints;
        // ���� ����(Deep Copy)�� ���� ���� ��ųʸ��� ��ȣ�մϴ�.
        tempSkillLevels = new Dictionary<int, int>(playerCharacter.playerStats.skillLevels);
        // ��ũ��Ʈ�� ���۵� �� �ӽ� ��ų ������ �ʱ�ȭ
        InitializePoints();
    }
    void OnEnable()
    {
        // PlayerLevelUp ��ũ��Ʈ�� ������ �̺�Ʈ�� �����մϴ�.
        PlayerLevelUp.OnPlayerLeveledUp += OnLeveledUpHandler;
    }

    void OnDisable()
    {
        // ��ũ��Ʈ ��Ȱ��ȭ �� �̺�Ʈ ������ �����մϴ�.
        PlayerLevelUp.OnPlayerLeveledUp -= OnLeveledUpHandler;
    }
    // === �ܺο��� ȣ��Ǵ� �޼��� ===

    /// <summary>
    /// ��ų �г��� ���� ������ ȣ��Ǿ�, ��ų ����Ʈ�� �ʱ�ȭ�ϰ� �ӽ� �����͸� �����մϴ�.
    /// </summary>
    public void InitializePoints()
    {
        // PlayerCharacter�� ���� PlayerStats �����Ϳ� ����
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerStats �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // ���� ��ų ����Ʈ�� ��ų ������ �ӽ� �����ͷ� ������ �ʱ�ȭ�մϴ�.
        currentSkillPoints = playerCharacter.playerStats.skillPoints;
        // ���� ����(Deep Copy)�� ���� ���� ��ųʸ��� ��ȣ�մϴ�.
        tempSkillLevels = new Dictionary<int, int>(playerCharacter.playerStats.skillLevels);

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
        if (playerCharacter == null || playerCharacter.playerStats == null) return false;

        int tempLevel = GetTempSkillLevel(skillId);
        int permanentLevel = playerCharacter.playerStats.skillLevels.ContainsKey(skillId) ? playerCharacter.playerStats.skillLevels[skillId] : 0;

        return tempLevel > permanentLevel;
    }

    /// <summary>
    /// Ư�� ��ų�� ��� �� �ִ� ���� ������ �����Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <param name="skillData">Ȯ���� ��ų ������</param>
    /// <returns>���� ������ �����Ǹ� true, �ƴϸ� false</returns>
    public bool CanLearnSkill(SkillData skillData)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null || skillData == null)
        {
            Debug.LogError("PlayerStats �Ǵ� SkillData�� ��ȿ���� �ʽ��ϴ�.");
            return false;
        }

        return skillData.requiredLevel <= playerCharacter.playerStats.level;
    }

    /// <summary>
    /// ����� ��ų ������ ���������� �����մϴ�.
    /// </summary>
    public void ApplyChanges()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        List<int> leveledUpSkillIds = new List<int>();
        foreach (var tempLevelPair in tempSkillLevels)
        {
            int skillId = tempLevelPair.Key;
            int tempLevel = tempLevelPair.Value;
            int permanentLevel = playerCharacter.playerStats.skillLevels.ContainsKey(skillId) ? playerCharacter.playerStats.skillLevels[skillId] : 0;

            if (tempLevel > permanentLevel)
            {
                leveledUpSkillIds.Add(skillId);
            }
        }

        playerCharacter.playerStats.skillLevels = new Dictionary<int, int>(tempSkillLevels);
        playerCharacter.playerStats.skillPoints = currentSkillPoints;

        foreach (int skillId in leveledUpSkillIds)
        {
            OnSkillLeveledUp?.Invoke(skillId);
        }
    }

    /// <summary>
    /// ���� ������ ����ϰ� ���� ���·� �ǵ����ϴ�.
    /// </summary>
    public void DiscardChanges()
    {
        InitializePoints();
    }

    /// <summary>
    /// ��ų ����Ʈ UI�� ������Ʈ�ϰ� �̺�Ʈ�� �߻���ŵ�ϴ�.
    /// </summary>
    public void UpdateSkillPointUI()
    {
        if (skillPointText != null)
        {
            skillPointText.text = $"��ų����Ʈ: \n{currentSkillPoints}";
        }
        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }
    /// <summary>
    /// ������ �̺�Ʈ�� �߻����� �� ȣ��� �ڵ鷯 �޼����Դϴ�.
    /// </summary>
    private void OnLeveledUpHandler()
    {
        // �������� �߻�������, ��ų ����Ʈ�� �ٽ� �ʱ�ȭ�մϴ�.
        InitializePoints();

        // Ȥ�� �� ��Ȳ�� ����� �ӽ� ��ų ���� ��ųʸ��� �ٽ� �ʱ�ȭ
        // playerCharacter.playerStats.skillLevels = new Dictionary<int, int>(); // �� �κ��� ����(ApplyChanges) �������� ����ǹǷ� �ʿ� ����

        // UI�� �ٽ� ������Ʈ�մϴ�.
        UpdateSkillPointUI();
    }
}