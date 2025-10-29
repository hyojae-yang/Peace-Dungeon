using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어의 공격 로직을 제어하는 스크립트입니다.
/// 장착된 무기 데이터에 따라 공격 방식이 동적으로 변경되며, 근접/원거리 공격에 딜레이를 적용합니다.
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
    /// 마지막 공격 시간을 기록하는 변수입니다. 공격 쿨타임을 체크하는 데 사용됩니다.
    /// </summary>
    private float lastAttackTime;

    // === 공격 딜레이 설정 (수정 사항 1: 원거리 딜레이 변수 추가) ===
    [Header("공격 설정")]
    [Tooltip("근접 공격의 데미지 판정 딜레이 (초)입니다. 애니메이션과 타이밍을 맞춥니다.")]
    public float meleeDamageDelay = 0.4f;

    // [추가] 원거리 공격 시 발사체 생성 지연 시간입니다. (애니메이션 손에서 화살이 떠나는 시점 등에 맞춥니다.)
    [Tooltip("원거리 공격의 발사체 생성 딜레이 (초)입니다. 애니메이션과 타이밍을 맞춥니다.")]
    public float rangedShootDelay = 0.2f; // 예시 값 설정
    [Tooltip("원거리 공격 발사체가 생성될 위치(Transform)입니다. 보통 플레이어 손이나 무기 끝에 위치합니다.")]
    public Transform projectileSpawnPoint;
    // === 플레이어 스탯 및 레이어 마스크 ===
    [Tooltip("몬스터에게 데미지를 입히는 데 사용할 레이어 마스크입니다.")]
    public LayerMask monsterLayer;

    private void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다. (SRP)
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        // 초기화 시, 즉시 공격 가능 상태로 만듭니다.
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
        // UI 클릭(버튼, 인벤토리 등) 시 공격이 나가지 않게 합니다.
        if (IsPointerOverUI())
        {
            return; // UI 상호작용이 최우선입니다.
        }

        // 공격 가능 조건: 무기가 장착되었고, 마우스 왼쪽 버튼이 눌렸으며, 공격 쿨타임이 지났는지 확인
        if (equippedWeapon != null && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + equippedWeapon.attackSpeed)
        {
            PlayerCharacter.Instance.animator.SetTrigger("Attack"); // 공격 애니메이션 트리거 설정

            // 코루틴을 시작하여 공격 딜레이를 적용
            StartCoroutine(AttackWithDelay());

            lastAttackTime = Time.time; // 공격 시간 업데이트 (쿨타임 시작)
        }
    }

    /// <summary>
    /// 무기 타입에 따라 딜레이를 적용하여 공격 로직을 실행하는 코루틴입니다.
    /// 코루틴은 Unity에서 시간 지연, 프레임 단위 분할 등의 비동기 작업을 처리할 때 사용합니다.
    /// </summary>
    private IEnumerator AttackWithDelay()
    {
        float delay = 0f;

        // 근접 공격 무기 타입 정의
        bool isMelee = (equippedWeapon.weaponType == WeaponType.Sword ||
                        equippedWeapon.weaponType == WeaponType.Axe ||
                        equippedWeapon.weaponType == WeaponType.Spear);

        // 원거리 공격 무기 타입 정의
        // [수정] 원거리 무기 타입 정의 추가
        bool isRanged = (equippedWeapon.weaponType == WeaponType.Staff ||
                         equippedWeapon.weaponType == WeaponType.Bow);

        // [수정] 딜레이 시간 설정 분기 로직
        if (isMelee && meleeDamageDelay > 0)
        {
            // 근접 공격인 경우 설정된 딜레이를 적용
            delay = meleeDamageDelay;
        }
        else if (isRanged && rangedShootDelay > 0)
        {
            // 원거리 공격인 경우 설정된 딜레이를 적용
            delay = rangedShootDelay;
        }

        // 설정된 딜레이가 0보다 클 경우에만 대기합니다.
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            // 딜레이가 없는 경우 다음 프레임에 바로 실행
            yield return null;
        }

        // 지연 시간 후, 실제 데미지 계산 및 공격 로직을 실행합니다.
        Attack();
    }

    /// <summary>
    /// 플레이어의 기본 공격 로직을 실행합니다. (데미지 계산 및 공격 타입 분기)
    /// 이 메서드는 오직 '데미지 계산'과 '공격 실행 분기'만을 담당합니다. (SRP)
    /// </summary>
    void Attack()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. 공격을 진행할 수 없습니다.");
            return;
        }

        // 1. 무기 타입에 따라 기본 데미지 설정
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

        // 2. 치명타 여부 판정
        // 치명타 확률(criticalChance)을 기반으로 치명타 발생 여부를 결정합니다.
        bool isCritical = Random.Range(0f, 1f) <= playerCharacter.playerStats.criticalChance;

        // 3. 최종 데미지 계산
        float finalDamage = baseDamage;
        if (isCritical)
        {
            // 치명타 발생 시, 치명타 데미지 배율(criticalDamageMultiplier)을 곱합니다.
            finalDamage *= playerCharacter.playerStats.criticalDamageMultiplier;
        }

        // 4. 무기 타입에 따라 다른 공격 로직 실행 (계산된 데미지 전달)
        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Spear:
                // 근접 공격 실행
                PerformMeleeAttack(finalDamage);
                break;
            case WeaponType.Staff:
            case WeaponType.Bow:
                // 원거리 공격 실행
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
        // ... (기존 PerformMeleeAttack 로직 유지) ...
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
                // IDamageable 인터페이스를 가져와 피해를 입힙니다.
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
    /// 원거리 공격 로직을 실행합니다. (발사체 생성)
    /// </summary>
    /// <param name="damage">계산된 최종 데미지</param>
    private void PerformRangedAttack(float damage)
    {
        // 원거리 무기 프리팹(발사체)이 설정되어 있을 경우에만 실행
        if (equippedWeapon.projectilePrefab != null)
        {
            // 발사체(투사체)를 지정된 위치(projectileSpawnPoint)에서 생성합니다.
            // 기존: transform.position
            // 수정: projectileSpawnPoint.position

            if (projectileSpawnPoint == null)
            {
                Debug.LogError("Projectile Spawn Point가 할당되지 않았습니다! 플레이어 위치에서 생성됩니다.");
                // 비상시를 대비해 플레이어 위치에서 생성하도록 폴백합니다.
                GameObject projectile = Instantiate(equippedWeapon.projectilePrefab, transform.position, transform.rotation);
            }
            else
            {
                // 성공적인 생성 로직: FirePoint의 위치와 플레이어의 현재 회전 값을 사용합니다.
                GameObject projectile = Instantiate(equippedWeapon.projectilePrefab,
                                                    projectileSpawnPoint.position,
                                                    transform.rotation);

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
        }
        else
        {
            Debug.LogWarning("원거리 공격을 위한 발사체 프리팹이 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 현재 마우스 포인터가 Unity UI 엘리먼트 위에 있는지 확인합니다. (SRP)
    /// </summary>
    /// <returns>마우스가 UI 위에 있으면 참(true)을 반환합니다.</returns>
    private bool IsPointerOverUI()
    {
        // EventSystem.current.IsPointerOverGameObject()를 사용하여 UI 상호작용을 체크합니다.
        if (EventSystem.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;
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