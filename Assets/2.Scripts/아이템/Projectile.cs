using UnityEngine;
using System.Collections;

/// <summary>
/// 원거리 공격에 사용되는 발사체(투사체)를 관리하는 스크립트입니다.
/// 단일 책임 원칙(SRP)을 준수하기 위해 데미지 계산 로직을 PlayerAttack 스크립트에서 받아오도록 수정했습니다.
/// </summary>
public class Projectile : MonoBehaviour
{
    // === 발사체 데이터 ===
    /// <summary>
    /// 발사체에 사용할 무기 데이터입니다.
    /// </summary>
    private WeaponItemSO weaponData;

    /// <summary>
    /// 몬스터 레이어를 식별하는 데 사용되는 레이어 마스크입니다.
    /// </summary>
    private LayerMask monsterLayer;

    /// <summary>
    /// 발사체가 날아갈 수 있는 최대 거리입니다.
    /// </summary>
    private float maxDistance;

    /// <summary>
    /// 발사체가 생성된 시작 위치입니다. 최대 사거리를 계산하는 데 사용됩니다.
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// PlayerAttack 스크립트에서 계산되어 전달받은 최종 데미지 값입니다.
    /// </summary>
    private float finalDamage;

    // === 발사체 속성 ===
    [Header("발사체 속성")]
    [Tooltip("발사체의 이동 속도입니다. 인스펙터에서 직접 조정하세요.")]
    public float projectileSpeed = 10f;

    /// <summary>
    /// 발사체가 추적할 대상 (가장 가까운 몬스터)의 Transform입니다.
    /// </summary>
    private Transform target;

    void Awake()
    {
        // 발사체 생성 시점의 위치를 기록합니다.
        startPosition = transform.position;
    }

    /// <summary>
    /// PlayerAttack 스크립트에서 호출하여 발사체에 데이터를 설정합니다.
    /// </summary>
    /// <param name="weapon">발사체에 사용할 무기 데이터</param>
    /// <param name="layer">몬스터 레이어 마스크</param>
    /// <param name="damage">계산된 최종 데미지 값</param>
    public void SetProjectileData(WeaponItemSO weapon, LayerMask layer, float damage)
    {
        weaponData = weapon;
        monsterLayer = layer;
        maxDistance = weapon.attackRange;
        finalDamage = damage; // 전달받은 최종 데미지 값을 저장합니다.

        // 발사체 생성 시점에 가장 가까운 몬스터를 찾아 타겟으로 설정합니다.
        FindClosestMonster();
    }

    /// <summary>
    /// 공격 범위 내에서 가장 가까운 몬스터를 찾아 타겟으로 설정합니다.
    /// </summary>
    private void FindClosestMonster()
    {
        // OverlapSphere를 사용해 주변 몬스터를 모두 찾습니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDistance, monsterLayer);

        if (hitColliders.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestMonster = null;

            foreach (Collider col in hitColliders)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMonster = col.transform;
                }
            }
            target = closestMonster;
        }
    }

    void Update()
    {
        // 1. 발사체 이동
        // projectileSpeed는 인스펙터에서 설정된 값을 사용합니다.
        if (target != null)
        {
            // 타겟이 있으면 타겟을 향해 회전하고 이동합니다.
            Vector3 direction = (target.position - transform.position).normalized;
            transform.forward = direction;
            transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            // 타겟이 없으면 직진합니다.
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
        }

        // 2. 최대 사거리 확인 후 파괴
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 다른 콜라이더와 충돌했을 때 호출되는 메서드입니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 몬스터 레이어에 속하는지 확인
        if (((1 << other.gameObject.layer) & monsterLayer) != 0)
        {
            // 수정된 부분: Monster 컴포넌트 대신 IDamageable 인터페이스를 가져옵니다.
            IDamageable damageableTarget = other.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                // PlayerAttack 스크립트에서 전달받은 최종 데미지 값을 사용합니다.
                damageableTarget.TakeDamage(finalDamage, weaponData.damageType);

                // 넉백 효과 적용 (기존 로직 유지)
                if (weaponData.knockbackForce > 0)
                {
                    Rigidbody monsterRb = other.GetComponent<Rigidbody>();
                    if (monsterRb != null)
                    {
                        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                        monsterRb.AddForce(knockbackDirection * weaponData.knockbackForce, ForceMode.Impulse);
                    }
                }
            }
            // 몬스터에게 데미지를 입혔으므로 발사체를 파괴합니다.
            Destroy(gameObject);
        }
    }
}