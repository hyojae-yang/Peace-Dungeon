// MagicMissileProjectile.cs
using UnityEngine;

/// <summary>
/// 매직 미사일 투사체의 움직임, 타겟팅 및 충돌 처리를 담당하는 클래스입니다.
/// SRP (단일 책임 원칙): 오직 투사체의 생명 주기와 추적, 데미지 적용 책임만을 가집니다.
/// 목표: 타겟이 죽으면 즉시 재탐색 후 파괴/추적. 타겟에게 데미지를 주고 파괴됩니다.
/// </summary>
public class MagicMissileProjectile : MonoBehaviour
{
    // === 내부 상태 및 설정 필드 ===

    [Header("투사체 이동 설정")]
    [Tooltip("투사체가 타겟을 향해 이동하는 속도입니다.")]
    [SerializeField]
    private float moveSpeed = 10f;

    [Tooltip("투사체가 타겟으로 방향을 틀 때의 회전 속도입니다. (0이면 즉시 회전)")]
    [SerializeField]
    private float rotationSpeed = 5f;

    // === 스킬 데이터로부터 주입받는 필드 (Private) ===
    private float damage;             // 이 투사체가 가할 데미지
    private float maxTargetingRange;  // 몬스터를 찾을 최대 반경
    private LayerMask monsterLayer;   // 몬스터 레이어 마스크
    private DamageType damageType;    // 데미지 타입 필드

    // === 타겟 상태 필드 ===
    private Transform targetTransform; // 현재 추적 중인 몬스터의 Transform

    /// <summary>
    /// 매 프레임 타겟을 추적하고, 타겟의 유효성을 검사합니다.
    /// </summary>
    private void Update()
    {
        // 1. 타겟 유효성 검사 및 재탐색
        bool targetIsInvalid = false;

        // A. Transform 파괴 또는 비활성화 상태 확인
        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
        {
            targetIsInvalid = true;
        }
        else
        {
            // B. [Dead 상태 감지 로직 유지]
            IDetectable targetDetectable = targetTransform.GetComponent<IDetectable>();

            if (targetDetectable != null && !targetDetectable.IsDetectable())
            {
                targetIsInvalid = true;
            }
        }

        if (targetIsInvalid)
        {
            FindTargetAndStartTracking();

            if (targetTransform == null)
            {
                return;
            }
        }

        // 2. 타겟을 향해 회전 (방향 정렬)
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // 3. 타겟을 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 콜라이더 충돌 시 데미지를 적용하고 투사체를 파괴합니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. 핵심 로직: 몬스터 레이어가 아닌 경우 무조건 관통 (파괴하지 않고 리턴)
        if (((1 << other.gameObject.layer) & monsterLayer) == 0)
        {
            return;
        }

        // 2. IDamageable 컴포넌트 찾기 (몬스터 레이어인 경우)
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
        {
            // 자식 콜라이더인 경우, 부모에서 IDamageable을 찾습니다.
            damageable = other.GetComponentInParent<IDamageable>();
        }

        if (damageable != null)
        {

            // 3. 데미지 적용
            damageable.TakeDamage(damage, damageType);

            // 4. 성공적으로 데미지 적용 후 파괴
            Destroy(gameObject);
        }
        else
        {
            // 5. 몬스터 레이어이지만 IDamageable이 없는 경우 (투사체 파괴)
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 투사체를 초기화하고 타겟 탐색을 시작합니다.
    /// </summary>
    public void Initialize(float initialDamage, float range, LayerMask layer, DamageType type)
    {
        this.damage = initialDamage;
        this.maxTargetingRange = range;
        this.monsterLayer = layer;
        this.damageType = type;

        FindTargetAndStartTracking();
    }

    /// <summary>
    /// 투사체의 탐색 반경 내에서 가장 가까운 몬스터를 찾아 targetTransform에 저장합니다.
    /// 몬스터를 찾지 못하면 스스로를 파괴합니다.
    /// </summary>
    private void FindTargetAndStartTracking()
    {
        Vector3 searchOrigin = transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(searchOrigin, maxTargetingRange, monsterLayer);

        targetTransform = null;

        if (hitColliders.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestMonster = null;

            foreach (Collider col in hitColliders)
            {
                if (col.gameObject.activeInHierarchy)
                {
                    IDetectable target = col.GetComponent<IDetectable>();

                    if (target == null)
                    {
                        target = col.GetComponentInParent<IDetectable>();
                    }

                    if (target != null && target.IsDetectable())
                    {
                        // 몬스터의 루트 트랜스폼을 타겟으로 저장합니다.
                        Transform monsterRoot = col.transform.root;

                        float distance = Vector3.Distance(searchOrigin, monsterRoot.position);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestMonster = monsterRoot;
                        }
                    }
                }
            }

            targetTransform = closestMonster;
        }

        if (targetTransform == null)
        {
            Destroy(gameObject);
        }
    }
}