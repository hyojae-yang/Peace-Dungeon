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
    [Tooltip("현재 장착된 무기의 데이터입니다. PlayerEquipment에서 설정됩니다.")]
    public WeaponItemSO equippedWeapon; // PlayerController에서 접근 가능하도록 public 유지

    /// <summary>
    /// 마지막 공격 시간을 기록하는 변수입니다. 공격 쿨타임을 체크하는 데 사용됩니다.
    /// </summary>
    private float lastAttackTime;

    // === 공격 딜레이 설정 ===
    [Header("공격 설정")]
    [Tooltip("근접 공격의 데미지 판정 딜레이 (초)입니다. 애니메이션과 타이밍을 맞춥니다.")]
    public float meleeDamageDelay = 0.4f;

    [Tooltip("원거리 공격의 발사체 생성 딜레이 (초)입니다. 애니메이션과 타이밍을 맞춥니다.")]
    public float rangedShootDelay = 0.2f;
    [Tooltip("원거리 공격 발사체가 생성될 위치(Transform)입니다. 보통 플레이어 손이나 무기 끝에 위치합니다.")]
    public Transform projectileSpawnPoint;

    [Tooltip("공격 애니메이션 재생 후 이동 제한을 해제할 시간입니다. (애니메이션 길이에 따라 조절)")]
    public float attackMovementUnlockDelay = 0.5f;

    // === 플레이어 스탯 및 레이어 마스크 ===
    [Tooltip("몬스터에게 데미지를 입히는 데 사용할 레이어 마스크입니다.")]
    public LayerMask monsterLayer;

    [Header("시각화 설정")]
    [Tooltip("공격 범위를 표시하는 Line Renderer입니다.")]
    public LineRenderer attackRangeVisualizer;
    [Tooltip("공격 범위 시각화 오브젝트의 부모 컨테이너입니다.")]
    public GameObject visualizerContainer;

    // Line Renderer 정점 해상도
    private const int VisualizerResolution = 30;

    private void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다. (SRP)
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        // 시작 시 시각화는 숨깁니다.
        if (visualizerContainer != null)
        {
            visualizerContainer.SetActive(false);
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
        // equippedWeapon이 null이 아닐 때만 쿨타임을 초기화합니다.
        if (equippedWeapon != null)
        {
            // 무기 장착 시, 마지막 공격 시간을 초기화하여 즉시 공격 가능 상태로 만듭니다.
            lastAttackTime = Time.time - equippedWeapon.attackSpeed;
        }
    }

    void Update()
    {
        // [Null 체크 추가]: equippedWeapon이 없으면 공격 로직을 진행하지 않습니다.
        if (equippedWeapon == null) return;

        // UI 클릭(버튼, 인벤토리 등) 시 공격이 나가지 않게 합니다.
        if (IsPointerOverUI())
        {
            return; // UI 상호작용이 최우선입니다.
        }

        // 공격 가능 조건: 무기가 장착되었고, 마우스 왼쪽 버튼이 눌렸으며, 공격 쿨타임이 지났는지 확인
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + equippedWeapon.attackSpeed)
        {
            playerCharacter.playerController.canMove = false;
            PlayerCharacter.Instance.animator.SetTrigger("Attack"); // 공격 애니메이션 트리거 설정

            // 코루틴을 시작하여 공격 딜레이를 적용
            StartCoroutine(AttackWithDelay());

            lastAttackTime = Time.time; // 공격 시간 업데이트 (쿨타임 시작)
        }
    }

    /// <summary>
    /// 무기 타입에 따라 딜레이를 적용하여 공격 로직을 실행하는 코루틴입니다.
    /// </summary>
    private IEnumerator AttackWithDelay()
    {
        // [Null 체크 추가]: 코루틴 시작 시 무기가 사라질 경우를 대비
        if (equippedWeapon == null) yield break;

        float delay = 0f;

        // 무기 타입 분류
        bool isMelee = IsMeleeWeapon(equippedWeapon.weaponType);
        bool isRanged = IsRangedWeapon(equippedWeapon.weaponType);

        // 딜레이 시간 설정 분기 로직
        if (isMelee && meleeDamageDelay > 0)
        {
            delay = meleeDamageDelay;
        }
        else if (isRanged && rangedShootDelay > 0)
        {
            delay = rangedShootDelay;
        }

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return null;
        }

        // 지연 시간 후, 실제 데미지 계산 및 공격 로직을 실행합니다.
        Attack();

        yield return StartCoroutine(AllowMovementAfterDelay());
    }

    /// <summary>
    /// 공격 애니메이션 재생이 끝난 후 플레이어의 이동 제한을 해제하는 코루틴입니다.
    /// </summary>
    private IEnumerator AllowMovementAfterDelay()
    {
        yield return new WaitForSeconds(attackMovementUnlockDelay);

        // 이동을 다시 허용합니다.
        if (playerCharacter.playerController != null)
        {
            playerCharacter.playerController.canMove = true;
        }
    }

    /// <summary>
    /// 무기 타입이 근접 무기인지 확인합니다.
    /// </summary>
    private bool IsMeleeWeapon(WeaponType type)
    {
        return type == WeaponType.Sword || type == WeaponType.Axe || type == WeaponType.Spear;
    }

    /// <summary>
    /// 무기 타입이 원거리 무기인지 확인합니다.
    /// </summary>
    private bool IsRangedWeapon(WeaponType type)
    {
        return type == WeaponType.Staff || type == WeaponType.Bow;
    }

    /// <summary>
    /// 플레이어의 기본 공격 로직을 실행합니다. (데미지 계산 및 공격 타입 분기)
    /// </summary>
    void Attack()
    {
        // [Null 체크 강화]
        if (equippedWeapon == null || playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("공격 필수 구성 요소(무기/스탯)가 초기화되지 않았습니다. 공격을 중단합니다.");
            // 공격을 중단할 때 이동 제한을 해제하는 것이 안전합니다.
            if (playerCharacter != null && playerCharacter.playerController != null)
            {
                playerCharacter.playerController.canMove = true;
            }
            return;
        }

        // 1. 무기 타입에 따라 기본 데미지 설정
        float baseDamage;
        bool isMagicAttack = (equippedWeapon.weaponType == WeaponType.Staff);

        if (isMagicAttack)
        {
            baseDamage = playerCharacter.playerStats.magicAttackPower;
        }
        else
        {
            baseDamage = playerCharacter.playerStats.attackPower;
        }

        // 2. 치명타 여부 판정
        bool isCritical = UnityEngine.Random.Range(0f, 1f) <= playerCharacter.playerStats.criticalChance;

        // 3. 최종 데미지 계산
        float finalDamage = baseDamage;
        if (isCritical)
        {
            finalDamage *= playerCharacter.playerStats.criticalDamageMultiplier;
        }

        // 4. 무기 타입에 따라 다른 공격 로직 실행 (계산된 데미지 전달)
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
    /// 근접 공격 로직을 실행합니다. (기존 로직 유지)
    /// </summary>
    /// <param name="damage">계산된 최종 데미지</param>
    private void PerformMeleeAttack(float damage)
    {
        // [Null 체크]
        if (equippedWeapon == null) return;
        // ... (기존 PerformMeleeAttack 로직 유지)
        float currentAttackRange = equippedWeapon.attackRange;
        float currentAttackAngle = equippedWeapon.attackAngle;

        // 공격 범위 내의 몬스터를 찾습니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentAttackRange, monsterLayer);

        foreach (Collider monsterCollider in hitColliders)
        {
            Vector3 direction3D = monsterCollider.transform.position - transform.position;

            // 1. 방향 벡터의 Y축 성분을 0으로 설정하여 XZ 평면으로 평탄화합니다.
            Vector3 directionFlat = direction3D;
            directionFlat.y = 0;

            // 2. 플레이어의 전방 벡터도 Y축을 0으로 설정하여 평탄화합니다. (수평 방향)
            Vector3 forwardFlat = transform.forward;
            forwardFlat.y = 0;

            // 3. 평탄화된 전방 벡터와 몬스터 방향 벡터 사이의 각도를 계산합니다.
            float angle = Vector3.Angle(forwardFlat.normalized, directionFlat.normalized);

            // XZ 평면에서의 실제 거리를 다시 체크합니다.
            if (directionFlat.magnitude > currentAttackRange)
            {
                continue; // 수평 거리가 사거리를 벗어났으므로 무시
            }

            // 몬스터가 공격 각도 범위 안에 있는지 확인합니다.
            if (angle < currentAttackAngle * 0.5f)
            {
                IDamageable damageableTarget = monsterCollider.GetComponent<IDamageable>();

                if (damageableTarget != null)
                {
                    damageableTarget.TakeDamage(damage, equippedWeapon.damageType);

                    // 넉백 로직 
                    if (equippedWeapon.knockbackForce > 0)
                    {
                        Rigidbody monsterRb = monsterCollider.GetComponent<Rigidbody>();
                        if (monsterRb != null)
                        {
                            Vector3 knockbackDirection = direction3D.normalized;
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
        // [Null 체크]
        if (equippedWeapon == null) return;
        // ... (기존 PerformRangedAttack 로직 유지)
        if (equippedWeapon.projectilePrefab != null)
        {
            if (projectileSpawnPoint == null)
            {
                Debug.LogError("Projectile Spawn Point가 할당되지 않았습니다! 플레이어 위치에서 생성됩니다.");
                GameObject projectile = Instantiate(equippedWeapon.projectilePrefab, transform.position, transform.rotation);
            }
            else
            {
                GameObject projectile = Instantiate(equippedWeapon.projectilePrefab,
                                                     projectileSpawnPoint.position,
                                                     transform.rotation);

                Projectile projectileComponent = projectile.GetComponent<Projectile>();
                if (projectileComponent != null)
                {
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
        if (EventSystem.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;
    }

    /// <summary>
    /// 무기의 공격 범위와 각도에 맞춰 Line Renderer의 모양을 동적으로 업데이트합니다.
    /// 모든 무기 타입의 시각화를 담당합니다.
    /// </summary>
    public void UpdateVisualizerShape()
    {
        // [Null 체크 강화]: 무기가 없거나 필수 컴포넌트가 없으면 시각화 비활성화 후 리턴
        if (equippedWeapon == null || attackRangeVisualizer == null || visualizerContainer == null)
        {
            if (visualizerContainer != null && visualizerContainer.activeSelf)
            {
                visualizerContainer.SetActive(false);
            }
            return;
        }

        // 1. 필요한 설정 값 가져오기
        float range = equippedWeapon.attackRange;
        WeaponType type = equippedWeapon.weaponType;

        // 2. 무기 타입에 따른 시각화 로직 분기
        if (IsMeleeWeapon(type))
        {
            // 근접 무기: 부채꼴 (Sector) 시각화
            float angle = equippedWeapon.attackAngle;

            // 정점 개수를 설정합니다. (중심점 + 호의 정점들 + 다시 중심점으로 돌아오는 점)
            attackRangeVisualizer.positionCount = VisualizerResolution + 2;

            Vector3[] points = new Vector3[VisualizerResolution + 2];
            points[0] = Vector3.zero; // 첫 번째 점은 플레이어의 중심 (로컬 위치)

            // 시작 각도와 각도 증분 계산 (플레이어의 전방을 기준으로 좌우 대칭)
            float startAngle = -angle * 0.5f;
            float angleStep = angle / VisualizerResolution;

            for (int i = 0; i <= VisualizerResolution; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                float radian = currentAngle * Mathf.Deg2Rad;

                // X, Z 좌표 계산 (삼각함수 사용)
                float x = range * Mathf.Sin(radian);
                float z = range * Mathf.Cos(radian);

                // Y축은 바닥에 파묻히지 않게 살짝 띄우기 위함
                points[i + 1] = new Vector3(x, 0.01f, z);
            }

            // 부채꼴을 닫기 위해 마지막 점을 다시 중앙으로 설정합니다.
            points[VisualizerResolution + 1] = Vector3.zero;

            attackRangeVisualizer.SetPositions(points);
        }
        else if (IsRangedWeapon(type))
        {
            // 원거리 무기: 원 (Circle) 시각화

            // 정점 개수를 설정합니다. (원형이므로 중심점 없이 호의 정점만 필요)
            attackRangeVisualizer.positionCount = VisualizerResolution + 1;

            Vector3[] points = new Vector3[VisualizerResolution + 1];
            float angleStep = 360f / VisualizerResolution;

            for (int i = 0; i <= VisualizerResolution; i++)
            {
                // 0도부터 360도까지
                float currentAngle = angleStep * i;
                float radian = currentAngle * Mathf.Deg2Rad;

                // X, Z 좌표 계산 (원의 형태)
                float x = range * Mathf.Sin(radian);
                float z = range * Mathf.Cos(radian);

                points[i] = new Vector3(x, 0.01f, z);
            }

            attackRangeVisualizer.SetPositions(points);
        }
        else
        {
            // 지원되지 않는 무기 타입일 경우 시각화 비활성화
            visualizerContainer.SetActive(false);
            return;
        }

        // With this updated code:
        if (visualizerContainer.TryGetComponent<LineRenderer>(out var lineRenderer))
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        visualizerContainer.SetActive(true);
    }

    /// <summary>
    /// 공격 범위를 유니티 에디터에서 시각적으로 확인하기 위한 함수입니다. (Gizmos)
    /// 무기 타입에 따른 시각화 로직을 분리했습니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (equippedWeapon != null)
        {
            Gizmos.color = Color.red;
            WeaponType type = equippedWeapon.weaponType;

            if (IsMeleeWeapon(type))
            {
                // 근접 무기: 부채꼴 영역
                // ... (기존 부채꼴 Gizmo 로직 유지)
                Vector3 forwardLimit = transform.position + transform.forward * equippedWeapon.attackRange;
                Gizmos.DrawLine(transform.position, forwardLimit);

                Vector3 leftLimit = Quaternion.Euler(0, -equippedWeapon.attackAngle * 0.5f, 0) * transform.forward * equippedWeapon.attackRange;
                Gizmos.DrawLine(transform.position, transform.position + leftLimit);

                Vector3 rightLimit = Quaternion.Euler(0, equippedWeapon.attackAngle * 0.5f, 0) * transform.forward * equippedWeapon.attackRange;
                Gizmos.DrawLine(transform.position, transform.position + rightLimit);
            }
            else if (IsRangedWeapon(type))
            {
                // 원거리 무기: 원 영역
                Gizmos.DrawWireSphere(transform.position, equippedWeapon.attackRange);
            }
        }
    }
}