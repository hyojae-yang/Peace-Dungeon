using UnityEngine;
using System.Collections.Generic;

// 플레이어가 습득한 패시브 스킬의 효과를 관리하고 계산하는 스크립트입니다.
public class PassiveSkillManager : MonoBehaviour
{
    [Header("참조 스크립트")]
    [Tooltip("플레이어의 PlayerStatSystem 컴포넌트를 할당하세요.")]
    public PlayerStatSystem playerStatSystem;

    // 동적 효과 스크립트의 인스턴스를 관리하기 위한 딕셔너리입니다.
    // 키: 스킬 데이터, 값: 생성된 IPassiveEffect 컴포넌트 인스턴스
    private Dictionary<PassiveSkillData, IPassiveEffect> activeDynamicEffects = new Dictionary<PassiveSkillData, IPassiveEffect>();

    // 플레이어가 습득한 패시브 스킬과 현재 레벨을 함께 저장하는 구조체입니다.
    [System.Serializable]
    public struct LearnedPassiveSkill
    {
        public PassiveSkillData skillData;
        public int currentLevel;
    }

    [Header("습득한 패시브 스킬")]
    [Tooltip("현재 플레이어가 습득한 패시브 스킬 목록과 레벨입니다.")]
    public List<LearnedPassiveSkill> learnedSkills = new List<LearnedPassiveSkill>();

    void Start()
    {
        // PlayerStatSystem 컴포넌트가 할당되었는지 확인합니다.
        if (playerStatSystem == null)
        {
            Debug.LogError("PlayerStatSystem이 할당되지 않았습니다. 인스펙터 창에서 할당해 주세요.");
            return;
        }

        // 게임 시작 시 패시브 스킬의 효과를 처음으로 적용합니다.
        UpdatePassiveBonuses();
    }

    /// <summary>
    /// 플레이어가 새로운 패시브 스킬을 습득했을 때 호출되는 메서드입니다.
    /// 스킬 목록에 추가하고, 스탯 보너스와 동적 효과를 재계산합니다.
    /// </summary>
    /// <param name="newSkill">새로 습득한 패시브 스킬 데이터</param>
    public void LearnPassiveSkill(PassiveSkillData newSkill)
    {
        // 이미 배운 스킬인지 확인
        bool alreadyLearned = false;
        for (int i = 0; i < learnedSkills.Count; i++)
        {
            if (learnedSkills[i].skillData == newSkill)
            {
                alreadyLearned = true;
                // 스킬 레벨이 Max 레벨인지 확인
                if (learnedSkills[i].currentLevel < newSkill.levelInfo.Length)
                {
                    // 레벨업 로직을 LevelUpPassiveSkill 메서드로 위임
                    LevelUpPassiveSkill(newSkill);
                }
                else
                {
                    Debug.Log($"'{newSkill.skillName}'은(는) 이미 최대 레벨입니다.");
                }
                break;
            }
        }

        // 새로운 스킬이면 목록에 추가 (레벨 1로 시작)
        if (!alreadyLearned)
        {
            if (newSkill.levelInfo.Length == 0)
            {
                Debug.LogWarning($"'{newSkill.skillName}'에 레벨 정보가 없습니다.");
                return;
            }

            LearnedPassiveSkill newLearnedSkill = new LearnedPassiveSkill
            {
                skillData = newSkill,
                currentLevel = 1
            };
            learnedSkills.Add(newLearnedSkill);

            // 효과를 재계산하여 즉시 스탯에 반영합니다.
            UpdatePassiveBonuses();

            Debug.Log($"'{newSkill.skillName}' 패시브 스킬을 습득했습니다!");
        }
    }

    /// <summary>
    /// 현재 습득한 모든 패시브 스킬의 스탯 보너스를 합산하고, PlayerStatSystem에 전달하는 핵심 메서드입니다.
    /// </summary>
    public void UpdatePassiveBonuses()
    {
        // 고정값과 백분율 스탯 보너스를 저장할 딕셔너리를 초기화합니다.
        Dictionary<StatType, float> totalPassiveFlatBonuses = new Dictionary<StatType, float>();
        Dictionary<StatType, float> totalPassivePercentageBonuses = new Dictionary<StatType, float>();

        // 습득한 모든 패시브 스킬 목록을 순회하며 보너스 값을 합산합니다.
        foreach (var learnedSkill in learnedSkills)
        {
            // 스킬의 현재 레벨 정보를 가져옵니다.
            int skillLevelIndex = learnedSkill.currentLevel - 1;

            if (skillLevelIndex >= 0 && skillLevelIndex < learnedSkill.skillData.levelInfo.Length)
            {
                SkillLevelInfo levelInfo = learnedSkill.skillData.levelInfo[skillLevelIndex];

                // 마나 소모량과 쿨타임은 패시브 스킬에 필요 없으므로 스탯 목록만 순회합니다.
                if (levelInfo.stats != null)
                {
                    foreach (SkillStat stat in levelInfo.stats)
                    {
                        // isPercentage 변수를 사용하여 고정값과 백분율을 구분하여 합산합니다.
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

            // 동적 효과를 포함하는 경우, 해당 효과를 활성화하거나 업데이트합니다.
            // 패시브 스킬을 획득/레벨업하는 시점에 한 번만 호출되도록 변경합니다.
            if (learnedSkill.skillData.dynamicEffectPrefab != null && !activeDynamicEffects.ContainsKey(learnedSkill.skillData))
            {
                IPassiveEffect effectInstance = Instantiate(learnedSkill.skillData.dynamicEffectPrefab, transform).GetComponent<IPassiveEffect>();
                effectInstance.ApplyEffect(learnedSkill.currentLevel);
                activeDynamicEffects.Add(learnedSkill.skillData, effectInstance);
            }
        }

        // 합산된 최종 보너스 값을 PlayerStatSystem에 전달합니다.
        playerStatSystem.ApplyPassiveBonuses(totalPassiveFlatBonuses, totalPassivePercentageBonuses);
    }

    /// <summary>
    /// 패시브 스킬의 레벨을 올리는 메서드입니다.
    /// </summary>
    /// <param name="skillToLevelUp">레벨업할 패시브 스킬 데이터</param>
    public void LevelUpPassiveSkill(PassiveSkillData skillToLevelUp)
    {
        for (int i = 0; i < learnedSkills.Count; i++)
        {
            if (learnedSkills[i].skillData == skillToLevelUp)
            {
                // 스킬 레벨을 올립니다.
                LearnedPassiveSkill upgradedSkill = learnedSkills[i];
                upgradedSkill.currentLevel++;
                learnedSkills[i] = upgradedSkill;

                // 스탯과 동적 효과를 재계산하여 적용합니다.
                UpdatePassiveBonuses();

                // 동적 효과가 있으면 레벨을 업데이트합니다.
                if (activeDynamicEffects.TryGetValue(skillToLevelUp, out IPassiveEffect effect))
                {
                    effect.UpdateLevel(upgradedSkill.currentLevel);
                }

                Debug.Log($"'{skillToLevelUp.skillName}' 스킬이 레벨 {upgradedSkill.currentLevel}로 레벨업했습니다!");
                return;
            }
        }
    }

    /// <summary>
    /// 패시브 스킬을 목록에서 제거하고 동적 효과를 비활성화하는 메서드입니다.
    /// </summary>
    /// <param name="skillToRemove">제거할 패시브 스킬 데이터</param>
    public void RemovePassiveSkill(PassiveSkillData skillToRemove)
    {
        for (int i = 0; i < learnedSkills.Count; i++)
        {
            if (learnedSkills[i].skillData == skillToRemove)
            {
                // 동적 효과가 있으면 제거합니다.
                if (activeDynamicEffects.TryGetValue(skillToRemove, out IPassiveEffect effect))
                {
                    effect.RemoveEffect();
                    activeDynamicEffects.Remove(skillToRemove);
                }

                learnedSkills.RemoveAt(i);

                // 스탯 보너스를 재계산합니다.
                UpdatePassiveBonuses();

                Debug.Log($"'{skillToRemove.skillName}' 패시브 스킬을 제거했습니다.");
                return;
            }
        }
    }
}