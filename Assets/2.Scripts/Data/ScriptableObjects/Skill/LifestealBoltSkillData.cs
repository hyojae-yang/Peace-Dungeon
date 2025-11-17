using UnityEngine;

// 이 스크립트는 흡혈 화살 스킬에 특화된 스크립터블 오브젝트입니다.
// StatType에 LifestealRate가 정의되어 있다고 가정하고 진행합니다.
[CreateAssetMenu(fileName = "LifestealBoltSkillData", menuName = "Skill/Lifesteal Bolt SkillData", order = 4)]
public class LifestealBoltSkillData : ActiveSkillData
{
    [Header("흡혈 화살 전용 정보")]
    [Tooltip("발사할 흡혈 투사체 프리팹을 할당하세요. 이 프리팹에는 LifestealBoltProjectile 컴포넌트가 있어야 합니다.")]
    public GameObject lifestealBoltPrefab;

    /// <summary>
    /// 흡혈 화살 스킬을 발동하고, 계산된 데미지와 흡혈률을 투사체에 전달합니다.
    /// SkillData 부모 클래스의 Execute 메서드를 재정의하고 성공 여부를 반환합니다.
    /// </summary>
    /// <param name="spawnPoint">투사체가 발사될 위치</param>
    /// <param name="playerStats">스킬 발동 시 필요한 플레이어의 현재 능력치</param>
    /// <param name="skillLevel">현재 스킬의 레벨</param>
    /// <returns>스킬의 효과가 논리적으로 성공적으로 발동되었으면 true, 실패했으면 false를 반환합니다.</returns>
    public override bool Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel) // <--- [핵심 수정] bool 반환
    {
        // === 1. 유효성 검사 및 레벨 정보 추출 ===
        if (skillLevel > levelInfo.Length || skillLevel < 1)
        {
            Debug.LogError($"[LifestealBolt] 스킬 레벨 ({skillLevel})이 유효한 범위를 벗어났습니다. ID: {skillId}");
            return false; // <--- 실패 시 false 반환
        }
        base.Execute(spawnPoint, playerStats, skillLevel); // 기본 사운드 재생 호출
        SkillLevelInfo currentLevelInfo = levelInfo[skillLevel - 1];

        // === 2. 스킬 스탯 추출 ===
        float baseDamage = 0f;
        float lifestealRate = 0f; // 스킬 레벨에 따른 기본 흡혈률 (예: 0.1f = 10%)

        foreach (SkillStat stat in currentLevelInfo.stats)
        {
            if (stat.statType == StatType.BaseDamage)
            {
                baseDamage = stat.value;
            }
            // 스킬 레벨에 정의된 흡혈률 스탯을 찾습니다.
            else if (stat.statType == StatType.LifestealRate)
            {
                lifestealRate = stat.value;
            }
        }

        // === 3. 최종 데미지 계산 ===
        float finalDamage = playerStats.magicAttackPower + baseDamage;

        // *참고: 플레이어의 장비/패시브에서 오는 추가 흡혈률이 있다면 여기서 lifestealRate에 합산해야 합니다.*

        // === 4. 투사체 생성 및 초기화 ===
        if (lifestealBoltPrefab == null)
        {
            Debug.LogError("[LifestealBolt] 투사체 프리팹이 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
            return false; // <--- 실패 시 false 반환
        }

        // 프리팹을 인스턴스화하고 스킬 발사 지점의 위치와 회전을 설정합니다.
        GameObject newBolt = Instantiate(lifestealBoltPrefab, spawnPoint.position, spawnPoint.rotation);

        // 투사체 스크립트를 가져와 계산된 값을 전달합니다.
        LifestealBoltProjectile projectile = newBolt.GetComponent<LifestealBoltProjectile>();
        if (projectile != null)
        {
            // 투사체에 데미지, 흡혈률, 그리고 플레이어의 스탯 참조를 전달합니다.
            projectile.Initialize(finalDamage, damageType, lifestealRate, playerStats);
        }
        else
        {
            Debug.LogError($"할당된 프리팹 '{lifestealBoltPrefab.name}'에 LifestealBoltProjectile 스크립트가 없습니다!");
            return false; // <--- 컴포넌트 누락 시 false 반환
        }

        // 5. 성공적으로 투사체가 발사되었으므로 true를 반환합니다.
        return true; // <--- 성공 시 true 반환
    }
}