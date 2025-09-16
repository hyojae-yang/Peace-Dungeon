using UnityEngine;

/// <summary>
/// 원거리 공격에 사용되는 발사체(투사체)를 관리하는 스크립트입니다.
/// </summary>
public class Projectile : MonoBehaviour
{
    private WeaponItemSO weaponData;
    private LayerMask monsterLayer;
    private float maxDistance;
    private Vector3 startPosition;

    [Header("발사체 속성")]
    [Tooltip("발사체의 이동 속도입니다. 인스펙터에서 직접 조정하세요.")]
    public float projectileSpeed = 10f; // 기본값을 10으로 설정했습니다.

    private Transform target; // 발사체가 추적할 대상 (가장 가까운 몬스터)

    void Awake()
    {
        startPosition = transform.position;
    }

    /// <summary>
    /// PlayerAttack 스크립트에서 호출하여 발사체에 데이터를 설정합니다.
    /// </summary>
    /// <param name="weapon">발사체에 사용할 무기 데이터</param>
    /// <param name="layer">몬스터 레이어 마스크</param>
    public void SetProjectileData(WeaponItemSO weapon, LayerMask layer)
    {
        weaponData = weapon;
        monsterLayer = layer;
        maxDistance = weapon.attackRange;

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
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                float finalDamage = PlayerStats.Instance.attackPower;
                monster.TakeDamage(finalDamage, weaponData.damageType);
                Debug.Log($"<color=red>원거리 공격 히트:</color> {other.name}에게 {finalDamage}의 데미지를 입혔습니다.");

                // 넉백 효과 적용
                if (weaponData.knockbackForce > 0)
                {
                    Rigidbody monsterRb = monster.GetComponent<Rigidbody>();
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