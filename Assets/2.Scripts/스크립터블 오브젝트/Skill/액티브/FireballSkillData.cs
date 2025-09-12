using UnityEngine;

// 이 스크립트는 파이어볼 스킬에 특화된 스크립터블 오브젝트입니다.
// SkillData 스크립트를 상속받아 파이어볼 스킬의 구체적인 데이터를 정의합니다.
[CreateAssetMenu(fileName = "FireballSkillData", menuName = "Skill/Fireball SkillData", order = 2)]
public class FireballSkillData : SkillData
{
    [Header("파이어볼 전용 정보")]
    [Tooltip("발사할 파이어볼 투사체 프리팹을 할당하세요.")]
    public GameObject fireballPrefab;

    /// <summary>
    /// 파이어볼을 발사하는 메서드입니다.
    /// SkillData 부모 클래스의 Execute 메서드를 재정의합니다.
    /// </summary>
    /// <param name="spawnPoint">투사체가 발사될 위치</param>
    /// <param name="playerStats">스킬 발동 시 필요한 플레이어의 현재 능력치</param>
    /// <param name="skillLevel">현재 스킬의 레벨</param>
    public override void Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel)
    {
        // 스킬 레벨이 유효한 범위인지 확인
        if (skillLevel > levelInfo.Length || skillLevel < 1)
        {
            Debug.LogError("스킬 레벨이 유효한 범위를 벗어났습니다.");
            return;
        }

        SkillLevelInfo currentLevelInfo = levelInfo[skillLevel - 1];

        // 기본 데미지 스탯을 찾아 최종 데미지를 계산합니다.
        float baseDamage = 0f;
        foreach (SkillStat stat in currentLevelInfo.stats)
        {
            if (stat.statType == StatType.BaseDamage)
            {
                baseDamage = stat.value;
                break;
            }
        }

        // 플레이어의 마법 공격력과 스킬의 기본 데미지를 합산하여 최종 데미지를 계산합니다.
        float finalDamage = playerStats.magicAttackPower + baseDamage;

        // 프리팹이 할당되었는지 확인합니다.
        if (fireballPrefab == null)
        {
            Debug.LogError("파이어볼 프리팹이 FireballSkillData에 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
            return;
        }

        // 프리팹을 인스턴스화하고 스킬 발사 지점의 위치와 회전을 설정합니다.
        GameObject newFireball = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);

        // 생성된 파이어볼 투사체 스크립트를 가져와 최종 데미지와 데미지 타입을 전달합니다.
        FireballProjectile projectile = newFireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.SetDamage(finalDamage, damageType);
        }
        else
        {
            Debug.LogError("파이어볼 프리팹에 FireballProjectile 스크립트가 없습니다!");
        }
    }
}