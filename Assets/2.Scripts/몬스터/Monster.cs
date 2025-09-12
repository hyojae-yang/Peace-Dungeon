using UnityEngine;

// 모든 몬스터의 부모 클래스입니다.
// 이 클래스는 직접 사용하기보다는 다른 몬스터 스크립트가 상속받아 사용합니다.
public class Monster : MonoBehaviour, IDamageable
{
    // === 몬스터 기본 스탯 변수 ===
    [Header("몬스터 기본 스탯")]
    public float health; // 체력
    public int mana; // 마나
    public int attackPower; // 공격력
    public int magicAttackPower; // 마법 공격력
    public int moveSpeed; // 이동 속도
    public int defense; // 방어력
    public int magicDefense; // 마법 방어력
    public float criticalChance; // 치명타 확률 (0.0 ~ 1.0)
    public float criticalDamageMultiplier; // 치명타 데미지 배율
    public float evasionChance; // 회피 확률 (0.0 ~ 1.0)

    [Header("몬스터 공격 타입")]
    [Tooltip("몬스터의 기본 공격 타입을 설정합니다.")]
    public DamageType attackDamageType = DamageType.Physical;

    [Header("보상 설정")]
    public int maxExpReward; // 처치 시 주는 경험치
    public int minExpReward;
    public int maxGoldReward; // 처치 시 주는 골드
    public int minGoldReward;

    // === 플레이어 감지 관련 변수 ===
    [Header("플레이어 감지 설정")]
    [Tooltip("플레이어를 감지하는 범위(반경)입니다.")]
    public float detectionRange = 10f;
    [Tooltip("플레이어를 감지하는 부채꼴 각도입니다. (총 각도)")]
    [Range(0, 360)]
    public float detectionAngle = 120f; // 몬스터의 시야각 (부채꼴의 총 각도)
    [Tooltip("플레이어 레이어 마스크입니다.")]
    public LayerMask playerLayer;

    // 플레이어를 감지했을 때 실행할 로직
    protected bool isPlayerDetected = false;
    protected IDetectable detectableTarget; // IDetectable 인터페이스 타입으로 변경

    protected virtual void Update()
    {
        // 매 프레임 플레이어를 감지하도록 구현합니다.
        DetectPlayer();

        // 플레이어가 감지되었을 때 추가 로직 (예: 플레이어를 향해 이동)
        if (isPlayerDetected && detectableTarget != null)
        {
            // 플레이어 방향을 바라보도록 회전합니다.
            transform.LookAt(detectableTarget.GetTransform(), Vector3.up);

            // 예시: 플레이어를 향해 이동하는 로직을 여기에 추가
            Vector3 direction = (detectableTarget.GetTransform().position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // 플레이어를 감지하는 메서드
    protected void DetectPlayer()
    {
        // Physics.OverlapSphere를 사용하여 일정 범위 내의 콜라이더를 감지합니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        isPlayerDetected = false; // 기본적으로는 플레이어를 놓쳤다고 가정
        detectableTarget = null; // 인터페이스 변수도 null로 초기화

        // 감지된 콜라이더들을 순회하면서 부채꼴 시야 내에 있는지 확인합니다.
        foreach (Collider hit in hitColliders)
        {
            // 감지된 콜라이더에서 IDetectable 인터페이스를 가진 컴포넌트가 있는지 확인합니다.
            IDetectable target = hit.GetComponent<IDetectable>();

            if (target != null && target.IsDetectable())
            {
                // 몬스터의 앞 방향과 플레이어 방향 벡터 간의 각도를 계산합니다.
                Vector3 directionToTarget = (target.GetTransform().position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                // 계산된 각도가 설정된 감지 각도의 절반보다 작으면 (즉, 부채꼴 범위 내에 있으면)
                if (angle < detectionAngle * 0.5f)
                {
                    // 플레이어를 찾음
                    isPlayerDetected = true;
                    detectableTarget = target;
                    // 한 명만 감지하면 되므로 반복문을 중단합니다.
                    return;
                }
            }
        }
    }

    // 공격 메서드 - virtual 키워드를 사용하여 자식 클래스에서 재정의 가능
    public virtual void Attack()
    {
        if (detectableTarget == null) return;

        // IDetectable 인터페이스를 IDamageable 인터페이스로 캐스팅하여 데미지를 줍니다.
        IDamageable target = detectableTarget as IDamageable;
        if (target != null)
        {
            // 몬스터의 공격력과 공격 타입을 함께 전달합니다.
            target.TakeDamage(attackPower, attackDamageType);
        }
    }

    // IDamageable 인터페이스를 구현하는 TakeDamage 메서드
    public virtual void TakeDamage(float damage)
    {
        // 몬스터 기본 방어력을 적용
        float finalDamage = Mathf.Max(damage - defense, 0);
        health -= finalDamage;

        if (health <= 0)
        {
            Die();
        }
    }

    // 새로운 TakeDamage 메서드: 데미지 타입에 따라 방어력을 다르게 적용
    public virtual void TakeDamage(float damage, DamageType type)
    {
        float finalDamage = damage;

        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(damage - defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(damage - magicDefense, 0);
                break;
            case DamageType.True:
                // True Damage는 방어력을 무시합니다.
                break;
        }

        health -= finalDamage;

        if (health <= 0)
        {
            Die();
        }
    }

    // 몬스터가 죽었을 때 호출될 메서드
    protected virtual void Die()
    {
        // 플레이어에게 보상을 지급하는 로직을 추가합니다.
        GiveRewardToPlayer();

        // 몬스터 오브젝트를 파괴합니다.
        Destroy(gameObject);
    }

    // 플레이어에게 보상을 주는 메서드
    private void GiveRewardToPlayer()
    {
        // "Player" 태그를 가진 게임 오브젝트를 찾아 PlayerStats와 PlayerLevelUp 컴포넌트를 가져옵니다.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다! 'Player' 태그를 확인해 주세요.");
            return;
        }

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        PlayerLevelUp playerLevelUp = playerObject.GetComponent<PlayerLevelUp>();

        if (playerStats == null || playerLevelUp == null)
        {
            Debug.LogError("플레이어에게 PlayerStats 또는 PlayerLevelUp 컴포넌트가 없습니다.");
            return;
        }

        // min과 max 사이의 랜덤 경험치와 골드 보상을 계산합니다.
        float expReward = Random.Range(minExpReward, maxExpReward);
        int goldReward = Random.Range(minGoldReward, maxGoldReward + 1);

        // PlayerLevelUp 스크립트의 AddExperience 메서드를 호출하여 경험치를 지급합니다.
        playerLevelUp.AddExperience(expReward);

        // PlayerStats 스크립트의 골드 변수에 직접 접근하여 골드를 지급합니다.
        playerStats.gold += goldReward;
    }

    // 유니티 에디터에서 감지 범위를 시각적으로 보여주기 위한 함수
    private void OnDrawGizmosSelected()
    {
        // Gizmo 색상 설정
        Gizmos.color = Color.yellow;

        // 감지 범위(반지름)와 감지 각도를 사용하여 부채꼴 모양을 그립니다.
        Vector3 forwardLimit = transform.position + transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, forwardLimit);

        // 부채꼴의 왼쪽 경계선
        Vector3 leftLimit = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);

        // 부채꼴의 오른쪽 경계선
        Vector3 rightLimit = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
    }
}