using UnityEngine;

// 액티브 스킬 데이터만 담는 스크립터블 오브젝트 클래스입니다.
[CreateAssetMenu(fileName = "NewActiveSkillData", menuName = "Skill/New ActiveSkillData")]
public class ActiveSkillData : SkillData
{
    [Header("액티브 스킬 전용")]
    [Tooltip("스킬 효과를 실행할 프리팹입니다. 이 프리팹은 ISkillEffect를 구현해야 합니다.")]
    public GameObject skillEffectPrefab;

    /// <summary>
    /// 스킬 발동 시 호출됩니다. 동적 스킬 효과 프리팹을 생성하고 실행합니다.
    /// </summary>
    public override void Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel)
    {
        if (skillEffectPrefab == null)
        {
            Debug.LogError($"'{skillName}' 스킬에 효과 프리팹이 할당되지 않았습니다.");
            return;
        }

        // 1. 스킬 효과 프리팹을 생성합니다.
        GameObject skillEffectObject = Instantiate(skillEffectPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. 프리팹에 부착된 ISkillEffect 컴포넌트를 가져옵니다.
        ISkillEffect effectComponent = skillEffectObject.GetComponent<ISkillEffect>();

        if (effectComponent != null)
        {
            // 3. 스킬 레벨에 맞는 데이터를 가져옵니다.
            SkillLevelInfo currentLevelInfo = levelInfo[skillLevel - 1];

            // 4. 스킬 효과를 실행합니다.
            effectComponent.ExecuteEffect(currentLevelInfo, spawnPoint, playerStats);
        }
        else
        {
            Debug.LogError($"할당된 프리팹에 ISkillEffect 컴포넌트가 없습니다: {skillEffectPrefab.name}");
        }
    }
}