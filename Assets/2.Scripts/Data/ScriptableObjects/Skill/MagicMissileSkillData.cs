using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 매직 미사일 스킬의 데이터를 담고 발동 로직을 정의하는 스크립터블 오브젝트입니다.
/// SRP (단일 책임 원칙): 오직 스킬의 데이터와 투사체 생성 로직(위치, 개수)만을 담당합니다.
/// 타겟팅 및 추적 로직은 MagicMissileProjectile에 위임합니다.
/// </summary>
[CreateAssetMenu(fileName = "MagicMissileSkill", menuName = "Skill/Magic Missile Skill")]
public class MagicMissileSkillData : ActiveSkillData
{
    // === 필드 정의 ===

    [Header("매직 미사일 설정")]
    [Tooltip("발사할 매직 미사일 투사체 프리팹입니다.")]
    [SerializeField]
    private GameObject missilePrefab;

    [Tooltip("투사체가 타겟을 찾을 수 있는 최대 반경입니다. 이 값은 투사체 초기화 시 전달됩니다.")]
    [SerializeField]
    private float maxTargetingRange = 20f;

    [Tooltip("타겟으로 인식할 몬스터들이 속한 레이어 마스크입니다. 이 값은 투사체 초기화 시 전달됩니다.")]
    [SerializeField]
    private LayerMask monsterLayer;

    [Tooltip("플레이어 주변에 투사체가 생성될 때 분산되는 원형 반경입니다. (겹치지 않는 생성 위치 결정)")]
    [SerializeField]
    private float spawnRadius = 3.0f;

    /// <summary>
    /// 스킬을 발동하는 메인 메서드입니다.
    /// 스킬 실행의 논리적 성공 여부를 반환합니다. 타겟이 없을 경우 false를 반환하여 비용 소모를 막습니다.
    /// </summary>
    /// <param name="caster">스킬을 시전하는 주체의 Transform (투사체의 생성 위치 기준)</param>
    /// <param name="playerStats">시전자의 PlayerStats 정보</param>
    /// <param name="skillLevel">현재 스킬의 레벨 (스탯 계산에 사용)</param>
    /// <returns>스킬 발동에 성공하면 true, 타겟이 없거나 치명적인 오류 발생 시 false 반환.</returns>
    public override bool Execute(Transform caster, PlayerStats playerStats, int skillLevel) // <--- [핵심 수정] bool 반환
    {
        // 1. 투사체 생성에 필요한 스탯을 추출합니다.
        // GetStatValueForLevel 보조 메서드를 사용하여 정확한 스탯 값을 가져옵니다.
        float damage = GetStatValueForLevel(StatType.BaseDamage, skillLevel);
        int projectileCount = (int)GetStatValueForLevel(StatType.ProjectileCount, skillLevel);

        // 2. 발사 전에 타겟이 있는지 확인하여 스킬 발동을 무효화할지 결정합니다.
        if (!IsTargetAvailable(caster.position))
        {
            // 타겟이 없으면 논리적으로 스킬 발동 실패로 간주합니다.
            return false; // <--- [핵심 추가] 논리적 실패 시 false 반환
        }

        // 3. 모든 조건이 충족되면 투사체 생성 로직을 시작합니다.
        // 투사체 생성에 실패하면 false를 반환합니다.
        if (!SpawnProjectiles(caster, damage, projectileCount))
        {
            return false;
        }

        // 4. 성공적으로 발동되었으므로 true를 반환하여 비용을 소모하게 합니다.
        return true; // <--- 논리적 성공 시 true 반환
    }

    /// <summary>
    /// 스탯에 정의된 개수만큼 매직 미사일 투사체를 생성하고 초기화합니다.
    /// SRP: 오직 투사체 생성과 위치 계산 책임만을 집니다.
    /// </summary>
    /// <param name="caster">시전자의 Transform</param>
    /// <param name="damage">각 투사체가 가할 기본 데미지</param>
    /// <param name="count">생성할 투사체의 총 개수</param>
    /// <returns>투사체 생성 및 초기화 성공 시 true, 실패 시 false를 반환합니다.</returns>
    private bool SpawnProjectiles(Transform caster, float damage, int count) // <--- [추가 수정] bool 반환
    {
        if (missilePrefab == null)
        {
            Debug.LogError("MagicMissileSkillData: 투사체 프리팹(missilePrefab)이 할당되지 않았습니다!");
            return false;
        }

        float startAngle = caster.eulerAngles.y;

        for (int i = 0; i < count; i++)
        {
            // 1. 투사체 분산 각도 계산 및 위치 결정
            float angle = startAngle + (i * (360f / count));
            Vector3 offsetDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 spawnPosition = caster.position + offsetDirection * spawnRadius + Vector3.up * 0.5f;

            // 2. 투사체 생성 및 컴포넌트 획득
            GameObject missileGO = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
            MagicMissileProjectile missile = missileGO.GetComponent<MagicMissileProjectile>();

            if (missile != null)
            {
                // 3. 투사체 초기화 (데이터 주입)
                missile.Initialize(damage, maxTargetingRange, monsterLayer, this.damageType);
            }
            else
            {
                Debug.LogError($"MagicMissileSkillData: 할당된 프리팹 '{missilePrefab.name}'에 MagicMissileProjectile 컴포넌트가 없습니다!");
                // 치명적인 오류이므로 스킬 발동 실패로 간주합니다.
                // 이미 생성된 투사체가 있어도 마나 소모 방지를 위해 false 반환
                return false;
            }
        }

        // 투사체 생성이 성공적으로 완료되었습니다.
        return true;
    }

    /// <summary>
    /// 스킬 발동 전에, 투사체가 타겟을 찾을 수 있는 범위 내에 몬스터가 있는지 확인합니다.
    /// (스킬 발동 무효화 예외 처리 1번을 위한 메서드입니다.)
    /// </summary>
    /// <param name="origin">탐색을 시작할 중심 위치 (시전자 위치)</param>
    /// <returns>탐색 범위 내에 몬스터가 있으면 true, 없으면 false</returns>
    private bool IsTargetAvailable(Vector3 origin)
    {
        // Physics.OverlapSphere를 사용하여 maxTargetingRange 내의 콜라이더를 확인합니다.
        // 이 때, 필드로 정의된 monsterLayer 마스크를 사용합니다.
        Collider[] hitColliders = Physics.OverlapSphere(origin, maxTargetingRange, monsterLayer);

        // 유효한 몬스터(Collider)가 하나라도 있으면 배열 길이가 0보다 큽니다.
        return hitColliders.Length > 0;
    }

    /// <summary>
    /// 주어진 스킬 레벨에서 특정 StatType에 해당하는 값을 추출하여 반환합니다.
    /// OCP (개방-폐쇄 원칙): 스킬 데이터 구조(levelInfo)가 바뀌지 않는 한, 이 메서드는 변경되지 않습니다.
    /// </summary>
    /// <param name="statType">찾으려는 능력치의 종류</param>
    /// <param name="skillLevel">현재 스킬 레벨 (1부터 시작)</param>
    /// <returns>해당 StatType의 값. 찾지 못하면 0f 반환.</returns>
    private float GetStatValueForLevel(StatType statType, int skillLevel)
    {
        // 1. 유효성 검사: 스킬 레벨은 최소 1이며, levelInfo 배열 인덱스를 벗어나면 안 됩니다.
        // 유니티 배열은 0부터 시작하므로, 레벨(1부터)을 인덱스(0부터)로 변환합니다.
        int index = skillLevel - 1;
        if (index < 0 || index >= levelInfo.Length)
        {
            Debug.LogError($"Skill ID {skillId}: 유효하지 않은 스킬 레벨({skillLevel}) 또는 levelInfo 배열 범위를 벗어났습니다.");
            return 0f;
        }

        // 2. 해당 레벨의 정보(SkillLevelInfo)를 가져옵니다.
        SkillLevelInfo info = levelInfo[index];

        // 3. 해당 레벨 정보의 스탯 배열(SkillStat[])을 탐색합니다.
        if (info.stats != null)
        {
            // LINQ를 사용하면 코드가 간결해지지만, 성능과 가독성을 위해 foreach를 사용합니다.
            foreach (SkillStat stat in info.stats)
            {
                if (stat.statType == statType)
                {
                    // 4. 값을 찾았으므로 반환합니다.
                    return stat.value;
                }
            }
        }

        // 5. 스탯을 찾지 못했거나 stats 배열이 null인 경우
        return 0f;
    }
}