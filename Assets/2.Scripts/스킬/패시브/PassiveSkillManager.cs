using UnityEngine;
using System.Collections.Generic;

// �÷��̾ ������ �нú� ��ų�� ȿ���� �����ϰ� ����ϴ� ��ũ��Ʈ�Դϴ�.
public class PassiveSkillManager : MonoBehaviour
{
    [Header("���� ��ũ��Ʈ")]
    [Tooltip("�÷��̾��� PlayerStatSystem ������Ʈ�� �Ҵ��ϼ���.")]
    public PlayerStatSystem playerStatSystem;

    // ���� ȿ�� ��ũ��Ʈ�� �ν��Ͻ��� �����ϱ� ���� ��ųʸ��Դϴ�.
    // Ű: ��ų ������, ��: ������ IPassiveEffect ������Ʈ �ν��Ͻ�
    private Dictionary<PassiveSkillData, IPassiveEffect> activeDynamicEffects = new Dictionary<PassiveSkillData, IPassiveEffect>();

    // �÷��̾ ������ �нú� ��ų�� ���� ������ �Բ� �����ϴ� ����ü�Դϴ�.
    [System.Serializable]
    public struct LearnedPassiveSkill
    {
        public PassiveSkillData skillData;
        public int currentLevel;
    }

    [Header("������ �нú� ��ų")]
    [Tooltip("���� �÷��̾ ������ �нú� ��ų ��ϰ� �����Դϴ�.")]
    public List<LearnedPassiveSkill> learnedSkills = new List<LearnedPassiveSkill>();

    void Start()
    {
        // PlayerStatSystem ������Ʈ�� �Ҵ�Ǿ����� Ȯ���մϴ�.
        if (playerStatSystem == null)
        {
            Debug.LogError("PlayerStatSystem�� �Ҵ���� �ʾҽ��ϴ�. �ν����� â���� �Ҵ��� �ּ���.");
            return;
        }

        // ���� ���� �� �нú� ��ų�� ȿ���� ó������ �����մϴ�.
        UpdatePassiveBonuses();
    }

    /// <summary>
    /// �÷��̾ ���ο� �нú� ��ų�� �������� �� ȣ��Ǵ� �޼����Դϴ�.
    /// ��ų ��Ͽ� �߰��ϰ�, ���� ���ʽ��� ���� ȿ���� �����մϴ�.
    /// </summary>
    /// <param name="newSkill">���� ������ �нú� ��ų ������</param>
    public void LearnPassiveSkill(PassiveSkillData newSkill)
    {
        // �̹� ��� ��ų���� Ȯ��
        bool alreadyLearned = false;
        for (int i = 0; i < learnedSkills.Count; i++)
        {
            if (learnedSkills[i].skillData == newSkill)
            {
                alreadyLearned = true;
                // ��ų ������ Max �������� Ȯ��
                if (learnedSkills[i].currentLevel < newSkill.levelInfo.Length)
                {
                    // ������ ������ LevelUpPassiveSkill �޼���� ����
                    LevelUpPassiveSkill(newSkill);
                }
                else
                {
                    Debug.Log($"'{newSkill.skillName}'��(��) �̹� �ִ� �����Դϴ�.");
                }
                break;
            }
        }

        // ���ο� ��ų�̸� ��Ͽ� �߰� (���� 1�� ����)
        if (!alreadyLearned)
        {
            if (newSkill.levelInfo.Length == 0)
            {
                Debug.LogWarning($"'{newSkill.skillName}'�� ���� ������ �����ϴ�.");
                return;
            }

            LearnedPassiveSkill newLearnedSkill = new LearnedPassiveSkill
            {
                skillData = newSkill,
                currentLevel = 1
            };
            learnedSkills.Add(newLearnedSkill);

            // ȿ���� �����Ͽ� ��� ���ȿ� �ݿ��մϴ�.
            UpdatePassiveBonuses();

            Debug.Log($"'{newSkill.skillName}' �нú� ��ų�� �����߽��ϴ�!");
        }
    }

    /// <summary>
    /// ���� ������ ��� �нú� ��ų�� ���� ���ʽ��� �ջ��ϰ�, PlayerStatSystem�� �����ϴ� �ٽ� �޼����Դϴ�.
    /// </summary>
    public void UpdatePassiveBonuses()
    {
        // �������� ����� ���� ���ʽ��� ������ ��ųʸ��� �ʱ�ȭ�մϴ�.
        Dictionary<StatType, float> totalPassiveFlatBonuses = new Dictionary<StatType, float>();
        Dictionary<StatType, float> totalPassivePercentageBonuses = new Dictionary<StatType, float>();

        // ������ ��� �нú� ��ų ����� ��ȸ�ϸ� ���ʽ� ���� �ջ��մϴ�.
        foreach (var learnedSkill in learnedSkills)
        {
            // ��ų�� ���� ���� ������ �����ɴϴ�.
            int skillLevelIndex = learnedSkill.currentLevel - 1;

            if (skillLevelIndex >= 0 && skillLevelIndex < learnedSkill.skillData.levelInfo.Length)
            {
                SkillLevelInfo levelInfo = learnedSkill.skillData.levelInfo[skillLevelIndex];

                // ���� �Ҹ𷮰� ��Ÿ���� �нú� ��ų�� �ʿ� �����Ƿ� ���� ��ϸ� ��ȸ�մϴ�.
                if (levelInfo.stats != null)
                {
                    foreach (SkillStat stat in levelInfo.stats)
                    {
                        // isPercentage ������ ����Ͽ� �������� ������� �����Ͽ� �ջ��մϴ�.
                        Dictionary<StatType, float> targetDict = stat.isPercentage ? totalPassivePercentageBonuses : totalPassiveFlatBonuses;

                        if (targetDict.ContainsKey(stat.statType))
                        {
                            targetDict[stat.statType] += stat.value;
                        }
                        else
                        {
                            targetDict.Add(stat.statType, stat.value);
                        }
                    }
                }
            }

            // ���� ȿ���� �����ϴ� ���, �ش� ȿ���� Ȱ��ȭ�ϰų� ������Ʈ�մϴ�.
            // �нú� ��ų�� ȹ��/�������ϴ� ������ �� ���� ȣ��ǵ��� �����մϴ�.
            if (learnedSkill.skillData.dynamicEffectPrefab != null && !activeDynamicEffects.ContainsKey(learnedSkill.skillData))
            {
                IPassiveEffect effectInstance = Instantiate(learnedSkill.skillData.dynamicEffectPrefab, transform).GetComponent<IPassiveEffect>();
                effectInstance.ApplyEffect(learnedSkill.currentLevel);
                activeDynamicEffects.Add(learnedSkill.skillData, effectInstance);
            }
        }

        // �ջ�� ���� ���ʽ� ���� PlayerStatSystem�� �����մϴ�.
        playerStatSystem.ApplyPassiveBonuses(totalPassiveFlatBonuses, totalPassivePercentageBonuses);
    }

    /// <summary>
    /// �нú� ��ų�� ������ �ø��� �޼����Դϴ�.
    /// </summary>
    /// <param name="skillToLevelUp">�������� �нú� ��ų ������</param>
    public void LevelUpPassiveSkill(PassiveSkillData skillToLevelUp)
    {
        for (int i = 0; i < learnedSkills.Count; i++)
        {
            if (learnedSkills[i].skillData == skillToLevelUp)
            {
                // ��ų ������ �ø��ϴ�.
                LearnedPassiveSkill upgradedSkill = learnedSkills[i];
                upgradedSkill.currentLevel++;
                learnedSkills[i] = upgradedSkill;

                // ���Ȱ� ���� ȿ���� �����Ͽ� �����մϴ�.
                UpdatePassiveBonuses();

                // ���� ȿ���� ������ ������ ������Ʈ�մϴ�.
                if (activeDynamicEffects.TryGetValue(skillToLevelUp, out IPassiveEffect effect))
                {
                    effect.UpdateLevel(upgradedSkill.currentLevel);
                }

                Debug.Log($"'{skillToLevelUp.skillName}' ��ų�� ���� {upgradedSkill.currentLevel}�� �������߽��ϴ�!");
                return;
            }
        }
    }

    /// <summary>
    /// �нú� ��ų�� ��Ͽ��� �����ϰ� ���� ȿ���� ��Ȱ��ȭ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="skillToRemove">������ �нú� ��ų ������</param>
    public void RemovePassiveSkill(PassiveSkillData skillToRemove)
    {
        for (int i = 0; i < learnedSkills.Count; i++)
        {
            if (learnedSkills[i].skillData == skillToRemove)
            {
                // ���� ȿ���� ������ �����մϴ�.
                if (activeDynamicEffects.TryGetValue(skillToRemove, out IPassiveEffect effect))
                {
                    effect.RemoveEffect();
                    activeDynamicEffects.Remove(skillToRemove);
                }

                learnedSkills.RemoveAt(i);

                // ���� ���ʽ��� �����մϴ�.
                UpdatePassiveBonuses();

                Debug.Log($"'{skillToRemove.skillName}' �нú� ��ų�� �����߽��ϴ�.");
                return;
            }
        }
    }
}