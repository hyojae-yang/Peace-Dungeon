// FireballSkillData.cs
using UnityEngine;
/// <summary>
/// 투사체 발사 시 마우스의 목표 월드 위치(조준 정보)가 필요한 스킬 데이터가 구현해야 할 인터페이스입니다.
/// </summary>
public interface IHasAiming
{
    /// <summary>
    /// PlayerSkillController로부터 계산된 마우스 목표 월드 좌표를 주입받기 위한 메서드입니다.
    /// </summary>
    /// <param name="targetPosition">마우스 Raycast로 계산된 목표 월드 좌표</param>
    void SetTargetPosition(Vector3 targetPosition);
}
// 이 스크립트는 파이어볼 스킬에 특화된 스크립터블 오브젝트입니다.
// SkillData 스크립트를 상속받아 파이어볼 스킬의 구체적인 데이터를 정의합니다.
[CreateAssetMenu(fileName = "FireballSkillData", menuName = "Skill/Fireball SkillData", order = 2)]
public class FireballSkillData : ActiveSkillData, IHasAiming // ⭐ IHasAiming 인터페이스 구현 추가!
{
    [Header("파이어볼 전용 정보")]
    [Tooltip("발사할 파이어볼 투사체 프리펩을 할당하세요.")]
    public GameObject fireballPrefab;

    // IHasAiming 인터페이스를 통해 주입받을 마우스 목표 위치
    private Vector3 targetAimPosition = Vector3.zero;

    /// <summary>
    /// IHasAiming 인터페이스 구현: PlayerSkillController로부터 마우스 목표 위치를 받습니다.
    /// </summary>
    /// <param name="targetPosition">마우스 Raycast로 계산된 목표 월드 좌표</param>
    public void SetTargetPosition(Vector3 targetPosition)
    {
        // 받은 마우스 목표 위치를 저장하여 Execute() 시점에서 사용합니다.
        this.targetAimPosition = targetPosition;
    }

    /// <summary>
    /// 파이어볼을 발사하는 메서드입니다.
    /// 스킬이 성공적으로 발사되면 true를 반환하여 비용(마나/쿨타임)을 소모하게 합니다.
    /// </summary>
    /// <param name="spawnPoint">투사체가 발사될 위치</param>
    /// <param name="playerStats">스킬 발동 시 필요한 플레이어의 현재 능력치</param>
    /// <param name="skillLevel">현재 스킬의 레벨</param>
    /// <returns>스킬의 효과가 논리적으로 성공적으로 발동되었으면 true, 실패했으면 false를 반환합니다.</returns>
    public override bool Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel) // 시그니처 변경 없음!
    {
        // 1. 스킬 레벨이 유효한 범위인지 확인 및 프리팹 확인
        if (skillLevel > levelInfo.Length || skillLevel < 1 || fireballPrefab == null)
        {
            Debug.LogError("스킬 발동 실패: 레벨 정보 부족 또는 프리팹 미할당.");
            return false;
        }

        SkillLevelInfo currentLevelInfo = levelInfo[skillLevel - 1];

        // 2. 최종 데미지 계산 (기존과 동일)
        float baseDamage = 0f;
        foreach (SkillStat stat in currentLevelInfo.stats)
        {
            if (stat.statType == StatType.BaseDamage)
            {
                baseDamage = stat.value;
                break;
            }
        }
        float finalDamage = playerStats.magicAttackPower + baseDamage;

        // 3. 방향 벡터 계산 (추가된 핵심 로직)
        // 저장된 targetAimPosition을 사용하여 발사 방향을 계산합니다.
        Vector3 direction = (targetAimPosition - spawnPoint.position).normalized;

        // 4. 투사체 생성 및 초기화
        // 생성 시 회전은 (0, 0, 0)인 Quaternion.identity로 설정하고, 방향은 스크립트에 주입합니다.
        GameObject newFireball = Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);

        // 생성된 파이어볼 투사체 스크립트를 가져와 초기화합니다.
        FireballProjectile projectile = newFireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.SetDamage(finalDamage, damageType);

            // 투사체에 계산된 방향을 주입합니다.
            projectile.SetDirection(direction);
        }
        else
        {
            Debug.LogError("파이어볼 프리팹에 FireballProjectile 스크립트가 없거나 SetDirection 메서드를 찾을 수 없습니다!");
            Destroy(newFireball);
            return false;
        }

        // 5. 성공적으로 투사체가 발사되었으므로 true를 반환합니다.
        return true;
    }
}