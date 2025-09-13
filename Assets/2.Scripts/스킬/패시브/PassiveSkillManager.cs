using UnityEngine;
using System.Collections.Generic;

// 플레이어가 습득한 패시브 스킬의 효과를 관리하고 계산하는 스크립트입니다.
public class PassiveSkillManager : MonoBehaviour
{
    [Header("참조 스크립트")]
    [Tooltip("플레이어의 PlayerStatSystem 컴포넌트를 할당하세요.")]
    public PlayerStatSystem playerStatSystem;
    [Tooltip("플레이어의 PlayerStats 컴포넌트를 할당하세요.")]
    public PlayerStats playerStats;
    [Tooltip("모든 스킬 데이터를 담고 있는 ScriptableObject 배열을 할당하세요.")]
    public SkillData[] allSkills;

    // 동적 효과 스크립트의 인스턴스를 관리하기 위한 딕셔너리입니다.
    // 키: 스킬 ID, 값: 생성된 IPassiveEffect 컴포넌트 인스턴스
    private Dictionary<int, IPassiveEffect> activeDynamicEffects = new Dictionary<int, IPassiveEffect>();

    void Start()
    {
        // PlayerStatSystem과 PlayerStats 컴포넌트가 할당되었는지 확인합니다.
        if (playerStatSystem == null || playerStats == null)
        {
            Debug.LogError("PlayerStatSystem 또는 PlayerStats가 할당되지 않았습니다. 인스펙터 창에서 할당해 주세요.");
            return;
        }

        // 게임 시작 시 패시브 스킬의 효과를 처음으로 적용합니다.
        UpdatePassiveBonuses();
    }

    /// <summary>
    /// 플레이어가 스킬 패널에서 '저장' 버튼을 눌렀을 때 호출되어
    /// 현재 습득한 모든 패시브 스킬의 스탯 보너스를 합산하고 적용하는 핵심 메서드입니다.
    /// </summary>
    public void UpdatePassiveBonuses()
    {
        // 합산된 최종 보너스 값을 PlayerStatSystem에 전달하기 위해 초기화합니다.
        Dictionary<StatType, float> totalPassiveFlatBonuses = new Dictionary<StatType, float>();
        Dictionary<StatType, float> totalPassivePercentageBonuses = new Dictionary<StatType, float>();

        // 기존의 모든 동적 효과를 제거합니다.
        // 새로운 효과를 적용하기 전 상태로 되돌려줍니다.
        foreach (var effect in activeDynamicEffects.Values)
        {
            effect.RemoveEffect(); // 동적 효과의 로직을 먼저 멈춥니다.
            // IPassiveEffect는 MonoBehaviour가 아니므로 직접 Destroy할 수 없습니다.
            // MonoBehaviour로 형 변환하여 gameObject를 가져와 파괴합니다.
            if (effect is MonoBehaviour monoBehaviourEffect)
            {
                Destroy(monoBehaviourEffect.gameObject);
            }
        }
        activeDynamicEffects.Clear();

        // PlayerStats의 최종 스킬 레벨 데이터를 순회하며 보너스 값을 합산합니다.
        foreach (var skillLevelPair in playerStats.skillLevels)
        {
            int skillID = skillLevelPair.Key;
            int currentLevel = skillLevelPair.Value;

            // 스킬 ID를 사용하여 SkillData를 찾습니다.
            SkillData skillData = System.Array.Find(allSkills, s => s.skillId == skillID);

            // 해당 스킬이 패시브 스킬이고, 레벨이 0보다 큰지 확인합니다.
            if (skillData != null && skillData.skillType == SkillType.Passive && currentLevel > 0)
            {
                // 스킬의 현재 레벨 정보를 가져옵니다.
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

                    // 동적 효과를 포함하는 경우, 해당 효과를 활성화합니다.
                    if (skillData is PassiveSkillData passiveSkillData && passiveSkillData.dynamicEffectPrefab != null)
                    {
                        IPassiveEffect effectInstance = Instantiate(passiveSkillData.dynamicEffectPrefab, transform).GetComponent<IPassiveEffect>();
                        effectInstance.ApplyEffect(currentLevel);
                        activeDynamicEffects.Add(skillID, effectInstance);
                    }
                }
            }
        }
        // 합산된 최종 보너스 값을 PlayerStatSystem에 전달합니다.
        playerStatSystem.ApplyPassiveBonuses(totalPassiveFlatBonuses, totalPassivePercentageBonuses);
    }
}