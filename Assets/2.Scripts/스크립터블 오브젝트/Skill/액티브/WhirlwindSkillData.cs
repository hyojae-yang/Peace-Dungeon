using UnityEngine;
using System.Collections.Generic;

// 이 스크립트는 소용돌이 베기 스킬에 특화된 스크립터블 오브젝트입니다.
[CreateAssetMenu(fileName = "WhirlwindSkillData", menuName = "Skill/Whirlwind SkillData", order = 3)]
public class WhirlwindSkillData : SkillData
{
    [Header("소용돌이 베기 전용 정보")]
    [Tooltip("피해를 입힐 주변 반경(Collider Radius)을 설정합니다.")]
    public float damageRadius = 8.0f;

    [Tooltip("피격 시 시각적/청각적 효과를 위한 프리팹 (선택 사항)")]
    public GameObject impactEffectPrefab;

    /// <summary>
    /// 소용돌이 베기 스킬을 실행합니다. (플레이어 주변 광역 피해)
    /// </summary>
    public override void Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel)
    {
        // === 1. 유효성 검사 및 기본 데이터 준비 ===
        if (skillLevel > levelInfo.Length || skillLevel < 1)
        {
            Debug.LogError($"[Whirlwind] 스킬 레벨이 유효하지 않습니다. ID: {skillId}");
            return;
        }

        SkillLevelInfo currentLevelInfo = levelInfo[skillLevel - 1];

        // 기본 데미지 스탯을 찾아 최종 데미지를 계산합니다.
        float baseDamage = 0f;
        foreach (SkillStat stat in currentLevelInfo.stats)
        {
            // 물리 공격력 기반 스킬이라고 가정 (StatType 확인 필요)
            if (stat.statType == StatType.BaseDamage)
            {
                baseDamage = stat.value;
                break;
            }
        }

        // 플레이어의 물리 공격력과 스킬의 기본 데미지를 합산하여 최종 데미지를 계산합니다.
        // 현재는 playerStats.magicAttackPower 대신 playerStats.attackPower를 사용해야 할 수 있습니다.
        // 임시로 기본 공격력을 사용한다고 가정합니다.
        float finalDamage = playerStats.attackPower + baseDamage;

        // === 2. 주변 Collider 감지 (OverlapSphere) ===
        // 플레이어의 위치를 중심으로 damageRadius 반경 내의 모든 Collider를 감지합니다.
        // 몬스터 레이어만 타겟팅하도록 LayerMask를 사용하는 것이 이상적입니다.
        // 여기서는 예시로 모든 Collider를 가져온 후 필터링하겠습니다.
        Collider[] hitColliders = Physics.OverlapSphere(spawnPoint.position, damageRadius);

        // 이미 데미지를 입힌 대상을 기록하여 중복 피해를 방지합니다.
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        // === 3. 감지된 Collider 처리 및 데미지 적용 ===
        foreach (var hitCollider in hitColliders)
        {
            // 플레이어 자신은 건너뜁니다. (Collider 태그나 레이어 등으로 구분하는 것이 좋습니다)
            if (hitCollider.CompareTag("Player")) continue;

            // 데미지를 입힐 수 있는 IDamageable 인터페이스를 가진 객체를 찾습니다.
            IDamageable damageableObject = hitCollider.GetComponent<IDamageable>();

            if (damageableObject != null && !damagedTargets.Contains(damageableObject))
            {
                // 데미지 적용
                damageableObject.TakeDamage(finalDamage, damageType);
                damagedTargets.Add(damageableObject);

                // (선택 사항) 피격 시 이펙트 생성
                // Instantiate(impactEffectPrefab, hitCollider.transform.position, Quaternion.identity);
            }
        }

        // === 4. 시각적/청각적 피드백 ===
        // 스킬 발동 시 중앙 이펙트 생성 (선택 사항)
        if (impactEffectPrefab != null)
        {
            // 플레이어 위치에 이펙트 생성 후 일정 시간 뒤 파괴
            Destroy(Instantiate(impactEffectPrefab, spawnPoint.position, Quaternion.identity), 1.0f);
        }

        Debug.Log($"[액티브 스킬 발동] 소용돌이 베기(Lv.{skillLevel})! 주변 {damagedTargets.Count}명의 적에게 {finalDamage} 데미지 적용.");
    }

    // === 디버깅용: 에디터에서 반경 시각화 ===
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // 이 스크립터블 오브젝트를 가진 게임 오브젝트가 씬에 있어야 시각화됩니다.
        // Runtime 시각화는 다른 방법이 필요합니다.
        // Gizmos.DrawWireSphere(Vector3.zero, damageRadius); 
    }
}