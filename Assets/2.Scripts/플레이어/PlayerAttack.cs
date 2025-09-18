using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어의 공격 로직을 제어하는 스크립트입니다.
/// 장착된 무기 데이터에 따라 공격 방식이 동적으로 변경됩니다.
/// SOLID 원칙 중 단일 책임 원칙(SRP)을 준수합니다.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // === 무기 데이터 ===
    /// <summary>
    /// PlayerEquipment로부터 전달받을 무기 데이터입니다.
    /// </summary>
    private WeaponItemSO equippedWeapon;

    /// <summary>
    /// 마지막 공격 시간을 기록하는 변수입니다.
    /// 공격 쿨타임을 체크하는 데 사용됩니다.
    /// </summary>
    private float lastAttackTime;

    // === 플레이어 스탯 및 레이어 마스크 ===
    [Header("설정")]
    [Tooltip("몬스터에게 데미지를 입히는 데 사용할 레이어 마스크입니다.")]
    public LayerMask monsterLayer;

    private void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        // 초기화 시, 즉시 공격 가능 상태로 만듭니다.
        //equippedWeapon이 초기화되지 않았으므로 -100f와 같은 충분히 작은 값으로 설정
        lastAttackTime = -100f;
    }

    /// <summary>
    /// PlayerEquipment 스크립트로부터 현재 장착된 무기 데이터를 전달받는 메서드입니다.
    /// </summary>
    /// <param name="weapon">새로 장착된 무기 데이터</param>
    public void UpdateEquippedWeapon(WeaponItemSO weapon)
    {
        equippedWeapon = weapon;
        if (equippedWeapon != null)
        {
            // 무기 장착 시, 마지막 공격 시간을 초기화하여 즉시 공격 가능 상태로 만듭니다.
            lastAttackTime = Time.time - equippedWeapon.attackSpeed;
        }
    }

    void Update()
    {
        // 1. 공격 조건 확인
        // 무기가 장착되었는지, 마우스 왼쪽 버튼이 눌렸는지, 공격 쿨타임이 지났는지 확인합니다.
        if (equippedWeapon != null && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + equippedWeapon.attackSpeed)
        {
            Attack();
            lastAttackTime = Time.time; // 공격 시간 업데이트
        }
    }

    /// <summary>
    /// 플레이어의 기본 공격 로직을 실행합니다.
    /// 이 메서드는 데미지 계산과 공격 로직 실행을 담당합니다.
    /// </summary>
    void Attack()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 공격을 진행할 수 없습니다.");
            return;
        }

        // 2. 무기 타입에 따라 기본 데미지 설정
        float baseDamage;
        bool isMagicAttack = (equippedWeapon.weaponType == WeaponType.Staff);

        if (isMagicAttack)
        {
            // 지팡이일 경우 마법 공격력 사용
            baseDamage = playerCharacter.playerStats.magicAttackPower;
        }
        else
        {
            // 그 외 무기일 경우 일반 공격력 사용
            baseDamage = playerCharacter.playerStats.attackPower;
        }

        // 3. 치명타 여부 판정
        // 치명타 확률(criticalChance)을 기반으로 치명타 발생 여부를 결정합니다.
        bool isCritical = Random.Range(0f, 1f) <= playerCharacter.playerStats.criticalChance;

        // 4. 최종 데미지 계산
        float finalDamage = baseDamage;
        if (isCritical)
        {
            // 치명타 발생 시, 치명타 데미지 배율(criticalDamageMultiplier)을 곱합니다.
            finalDamage *= playerCharacter.playerStats.criticalDamageMultiplier;
        }

        // 5. 무기 타입에 따라 다른 공격 로직 실행 (계산된 데미지 전달)
        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Spear:
                PerformMeleeAttack(finalDamage);
                break;
            case WeaponType.Staff:
            case WeaponType.Bow:
                PerformRangedAttack(finalDamage);
                break;
            default:
                Debug.LogWarning("알 수 없는 무기 타입입니다.");
                break;
        }
    }

    /// <summary>
    /// 근접 공격 로직을 실행합니다.
    /// </summary>
    /// <param name="damage">계산된 최종 데미지</param>
    private void PerformMeleeAttack(float damage)
    {
        float currentAttackRange = equippedWeapon.attackRange;
        float currentAttackAngle = equippedWeapon.attackAngle;

        // 공격 범위 내의 몬스터를 찾습니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentAttackRange, monsterLayer);

        foreach (Collider monsterCollider in hitColliders)
        {
            Vector3 directionToMonster = (monsterCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToMonster);

            // 몬스터가 공격 각도 범위 안에 있는지 확인합니다.
            if (angle < currentAttackAngle * 0.5f)
            {
                // 수정된 부분: Monster 컴포넌트 대신 IDamageable 인터페이스를 가져옵니다.
                IDamageable damageableTarget = monsterCollider.GetComponent<IDamageable>();

                if (damageableTarget != null)
                {
                    // 계산된 최종 데미지를 전달하여 몬스터에게 피해를 입힙니다.
                    damageableTarget.TakeDamage(damage, equippedWeapon.damageType);

                    // 넉백 로직 (기존 로직 유지)
                    if (equippedWeapon.knockbackForce > 0)
                    {
                        Rigidbody monsterRb = monsterCollider.GetComponent<Rigidbody>();
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
    /// 원거리 공격 로직을 실행합니다.
    /// </summary>
    /// <param name="damage">계산된 최종 데미지</param>
    private void PerformRangedAttack(float damage)
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
                // 계산된 최종 데미지를 발사체에 전달합니다.
                projectileComponent.SetProjectileData(equippedWeapon, monsterLayer, damage);
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