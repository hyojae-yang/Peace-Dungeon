using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어의 공격 로직을 제어하는 스크립트입니다.
/// 장착된 무기 데이터에 따라 공격 방식이 동적으로 변경됩니다.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    // === 무기 데이터 ===
    private WeaponItemSO equippedWeapon; // PlayerEquipment로부터 전달받을 무기 데이터
    private float lastAttackTime;        // 마지막 공격 시간을 기록하는 변수

    // === 플레이어 스탯 및 레이어 마스크 ===
    [Header("설정")]
    [Tooltip("몬스터에게 데미지를 입히는 데 사용할 레이어 마스크입니다.")]
    public LayerMask monsterLayer;

    // TODO: PlayerStats를 싱글턴으로 관리하고 있으므로, Start()에서 참조를 가져올 필요 없음.

    /// <summary>
    /// PlayerEquipment 스크립트로부터 현재 장착된 무기 데이터를 전달받는 메서드입니다.
    /// </summary>
    public void UpdateEquippedWeapon(WeaponItemSO weapon)
    {
        equippedWeapon = weapon;
        if (equippedWeapon != null)
        {
            lastAttackTime = -equippedWeapon.attackSpeed;
        }
        else
        {
            // 무기가 해제될 경우, 마지막 공격 시간을 초기화하여 즉시 공격 불가 상태로 만듭니다.
            lastAttackTime = 0;
        }
    }

    void Update()
    {
        // 1. 공격 조건 확인
        if (equippedWeapon != null && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + equippedWeapon.attackSpeed)
        {
            Attack();
            lastAttackTime = Time.time; // 공격 시간 업데이트
        }
    }

    /// <summary>
    /// 플레이어의 기본 공격 로직을 실행합니다.
    /// 장착된 무기 데이터에 따라 공격 방식이 동적으로 변경됩니다.
    /// </summary>
    void Attack()
    {
        // 2. 무기 타입에 따라 다른 공격 로직 실행
        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Spear:
                PerformMeleeAttack();
                break;
            case WeaponType.Staff:
            case WeaponType.Bow:
                PerformRangedAttack();
                break;
            default:
                Debug.LogWarning("알 수 없는 무기 타입입니다.");
                break;
        }
    }

    /// <summary>
    /// 근접 공격 로직을 실행합니다. (기존 로직 유지)
    /// </summary>
    private void PerformMeleeAttack()
    {
        float currentAttackRange = equippedWeapon.attackRange;
        float currentAttackAngle = equippedWeapon.attackAngle;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentAttackRange, monsterLayer);

        foreach (Collider monsterCollider in hitColliders)
        {
            Vector3 directionToMonster = (monsterCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToMonster);

            if (angle < currentAttackAngle * 0.5f)
            {
                Monster monster = monsterCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    float finalDamage = PlayerStats.Instance.attackPower;
                    monster.TakeDamage(finalDamage, equippedWeapon.damageType);

                    if (equippedWeapon.knockbackForce > 0)
                    {
                        Rigidbody monsterRb = monster.GetComponent<Rigidbody>();
                        if (monsterRb != null)
                        {
                            Vector3 knockbackDirection = (monsterCollider.transform.position - transform.position).normalized;
                            monsterRb.AddForce(knockbackDirection * equippedWeapon.knockbackForce, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 원거리 공격 로직을 실행합니다. (신규 추가)
    /// 무기 프리팹을 생성하여 발사체로 만듭니다.
    /// </summary>
    private void PerformRangedAttack()
    {
        // 원거리 무기 프리팹(발사체)이 설정되어 있을 경우에만 실행
        if (equippedWeapon.projectilePrefab != null)
        {
            // 발사체(투사체)를 플레이어의 위치에서 생성합니다.
            GameObject projectile = Instantiate(equippedWeapon.projectilePrefab, transform.position, transform.rotation);

            // 발사체에 데이터를 전달합니다.
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                // 수정된 부분: 이제 속도 매개변수를 전달할 필요가 없습니다.
                projectileComponent.SetProjectileData(equippedWeapon, monsterLayer);
            }
            else
            {
                Debug.LogError("발사체 프리팹에 Projectile 스크립트가 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("원거리 공격을 위한 발사체 프리팹이 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 공격 범위를 유니티 에디터에서 시각적으로 확인하기 위한 함수입니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (equippedWeapon != null)
        {
            Gizmos.color = Color.red;

            switch (equippedWeapon.weaponType)
            {
                case WeaponType.Sword:
                case WeaponType.Axe:
                case WeaponType.Spear:
                    // 근접 무기일 경우 부채꼴 영역을 그립니다.
                    Vector3 forwardLimit = transform.position + transform.forward * equippedWeapon.attackRange;
                    Gizmos.DrawLine(transform.position, forwardLimit);

                    Vector3 leftLimit = Quaternion.Euler(0, -equippedWeapon.attackAngle * 0.5f, 0) * transform.forward * equippedWeapon.attackRange;
                    Gizmos.DrawLine(transform.position, transform.position + leftLimit);

                    Vector3 rightLimit = Quaternion.Euler(0, equippedWeapon.attackAngle * 0.5f, 0) * transform.forward * equippedWeapon.attackRange;
                    Gizmos.DrawLine(transform.position, transform.position + rightLimit);
                    break;
                case WeaponType.Staff:
                case WeaponType.Bow:
                    // 원거리 무기일 경우 공격 범위를 원으로 그립니다.
                    Gizmos.DrawWireSphere(transform.position, equippedWeapon.attackRange);
                    break;
            }
        }
    }
}