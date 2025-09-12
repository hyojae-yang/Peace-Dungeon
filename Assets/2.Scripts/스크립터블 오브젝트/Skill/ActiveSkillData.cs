using UnityEngine;

// ��Ƽ�� ��ų �����͸� ��� ��ũ���ͺ� ������Ʈ Ŭ�����Դϴ�.
[CreateAssetMenu(fileName = "NewActiveSkillData", menuName = "Skill/New ActiveSkillData")]
public class ActiveSkillData : SkillData
{
    [Header("��Ƽ�� ��ų ����")]
    [Tooltip("��ų ȿ���� ������ �������Դϴ�. �� �������� ISkillEffect�� �����ؾ� �մϴ�.")]
    public GameObject skillEffectPrefab;

    /// <summary>
    /// ��ų �ߵ� �� ȣ��˴ϴ�. ���� ��ų ȿ�� �������� �����ϰ� �����մϴ�.
    /// </summary>
    public override void Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel)
    {
        if (skillEffectPrefab == null)
        {
            Debug.LogError($"'{skillName}' ��ų�� ȿ�� �������� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // 1. ��ų ȿ�� �������� �����մϴ�.
        GameObject skillEffectObject = Instantiate(skillEffectPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. �����տ� ������ ISkillEffect ������Ʈ�� �����ɴϴ�.
        ISkillEffect effectComponent = skillEffectObject.GetComponent<ISkillEffect>();

        if (effectComponent != null)
        {
            // 3. ��ų ������ �´� �����͸� �����ɴϴ�.
            SkillLevelInfo currentLevelInfo = levelInfo[skillLevel - 1];

            // 4. ��ų ȿ���� �����մϴ�.
            effectComponent.ExecuteEffect(currentLevelInfo, spawnPoint, playerStats);
        }
        else
        {
            Debug.LogError($"�Ҵ�� �����տ� ISkillEffect ������Ʈ�� �����ϴ�: {skillEffectPrefab.name}");
        }
    }
}