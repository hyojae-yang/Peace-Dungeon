using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾ ������ �нú� ��ų�� ȿ���� �����ϰ� ����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� �� �̻� �̱����� �ƴϸ�, PlayerCharacter�� ����� �����˴ϴ�.
/// </summary>
public class PassiveSkillManager : MonoBehaviour
{
    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    [Header("���� ��ũ��Ʈ")]
    [Tooltip("��� ��ų �����͸� ��� �ִ� ScriptableObject �迭�� �Ҵ��ϼ���.")]
    public SkillData[] allSkills;

    // ���� ȿ�� ��ũ��Ʈ�� �ν��Ͻ��� �����ϱ� ���� ��ųʸ��Դϴ�.
    // Ű: ��ų ID, ��: ������ IPassiveEffect ������Ʈ �ν��Ͻ�
    private Dictionary<int, IPassiveEffect> activeDynamicEffects = new Dictionary<int, IPassiveEffect>();

    void Start()
    {
        // PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;

        // ���� ���� �� �ν��Ͻ��� �����ϴ��� Ȯ���մϴ�.
        if (playerCharacter == null || playerCharacter.playerStatSystem == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� ���� ������Ʈ�� �����Ǿ����ϴ�. ���� ���� �� �ش� ������Ʈ�� ���� ���� ������Ʈ�� ���� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // ���� ���� �� �нú� ��ų�� ȿ���� ó������ �����մϴ�.
        UpdatePassiveBonuses();
    }

    /// <summary>
    /// �÷��̾ ��ų �гο��� '����' ��ư�� ������ �� ȣ��Ǿ�
    /// ���� ������ ��� �нú� ��ų�� ���� ���ʽ��� �ջ��ϰ� �����ϴ� �ٽ� �޼����Դϴ�.
    /// </summary>
    public void UpdatePassiveBonuses()
    {
        // �ջ�� ���� ���ʽ� ���� PlayerStatSystem�� �����ϱ� ���� �ʱ�ȭ�մϴ�.
        Dictionary<StatType, float> totalPassiveFlatBonuses = new Dictionary<StatType, float>();
        Dictionary<StatType, float> totalPassivePercentageBonuses = new Dictionary<StatType, float>();

        // ������ ��� ���� ȿ���� �����մϴ�.
        // ���ο� ȿ���� �����ϱ� �� ���·� �ǵ����ݴϴ�.
        foreach (var effect in activeDynamicEffects.Values)
        {
            effect.RemoveEffect(); // ���� ȿ���� ������ ���� ����ϴ�.
            // IPassiveEffect�� MonoBehaviour�� �ƴϹǷ� ���� Destroy�� �� �����ϴ�.
            // MonoBehaviour�� �� ��ȯ�Ͽ� gameObject�� ������ �ı��մϴ�.
            if (effect is MonoBehaviour monoBehaviourEffect)
            {
                Destroy(monoBehaviourEffect.gameObject);
            }
        }
        activeDynamicEffects.Clear();

        // PlayerStats�� ���� ��ų ���� �����͸� ��ȸ�ϸ� ���ʽ� ���� �ջ��մϴ�.
        // PlayerCharacter�� ���� PlayerStats �����Ϳ� �����մϴ�.
        foreach (var skillLevelPair in playerCharacter.playerStats.skillLevels)
        {
            int skillID = skillLevelPair.Key;
            int currentLevel = skillLevelPair.Value;

            // ��ų ID�� ����Ͽ� SkillData�� ã���ϴ�.
            SkillData skillData = System.Array.Find(allSkills, s => s.skillId == skillID);

            // �ش� ��ų�� �нú� ��ų�̰�, ������ 0���� ū�� Ȯ���մϴ�.
            if (skillData != null && skillData.skillType == SkillType.Passive && currentLevel > 0)
            {
                // ��ų�� ���� ���� ������ �����ɴϴ�.
                int skillLevelIndex = currentLevel - 1;

                if (skillLevelIndex >= 0 && skillLevelIndex < skillData.levelInfo.Length)
                {
                    SkillLevelInfo levelInfo = skillData.levelInfo[skillLevelIndex];

                    if (levelInfo.stats != null)
                    {
                        foreach (SkillStat stat in levelInfo.stats)
                        {
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

                    // ���� ȿ���� �����ϴ� ���, �ش� ȿ���� Ȱ��ȭ�մϴ�.
                    if (skillData is PassiveSkillData passiveSkillData && passiveSkillData.dynamicEffectPrefab != null)
                    {
                        IPassiveEffect effectInstance = Instantiate(passiveSkillData.dynamicEffectPrefab, transform).GetComponent<IPassiveEffect>();
                        effectInstance.ApplyEffect(currentLevel);
                        activeDynamicEffects.Add(skillID, effectInstance);
                    }
                }
            }
        }
        // �ջ�� ���� ���ʽ� ���� PlayerStatSystem�� �����մϴ�.
        // PlayerCharacter�� ���� PlayerStatSystem�� �޼��带 ȣ���մϴ�.
        playerCharacter.playerStatSystem.ApplyPassiveBonuses(totalPassiveFlatBonuses, totalPassivePercentageBonuses);
    }
}