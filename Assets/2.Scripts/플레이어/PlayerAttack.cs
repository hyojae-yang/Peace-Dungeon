using UnityEngine;

// 플레이어의 공격을 제어하는 스크립트입니다.
public class PlayerAttack : MonoBehaviour
{
    // 공격 범위를 시각적으로 보여주기 위한 변수 (유니티 에디터에서 보입니다)
    [Header("공격 설정")]
    [Tooltip("공격이 닿는 최대 거리 (반지름)입니다.")]
    public float attackRange = 2f; // 공격 거리 (부채꼴의 반지름)

    [Tooltip("공격이 닿는 부채꼴 각도입니다. (총 각도, 예를 들어 90은 좌우 45도씩)")]
    [Range(0, 360)]
    public float attackAngle = 90f; // 공격 각도 (부채꼴의 총 각도)

    [Tooltip("몬스터에게 데미지를 입히는 데 사용할 레이어 마스크입니다.")]
    public LayerMask monsterLayer;

    // PlayerStats 스크립트는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.
    // private PlayerStats playerStats;

    void Start()
    {
        // 게임 시작 시, PlayerStats 싱글턴 인스턴스가 존재하는지 확인합니다.
        // GetComponent를 통해 가져올 필요가 없습니다.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats 인스턴스가 존재하지 않습니다. 게임 시작 시 PlayerStats를 가진 게임 오브젝트가 씬에 있는지 확인해 주세요.");
        }
    }

    void Update()
    {
        // 마우스 좌클릭(0)이 눌렸는지 확인합니다.
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    /// <summary>
    /// 플레이어의 기본 공격 로직을 실행합니다.
    /// </summary>
    void Attack()
    {
        // 싱글턴 인스턴스가 유효한지 다시 한번 확인합니다.
        if (PlayerStats.Instance == null) return;

        // Physics.OverlapSphere를 사용하여 attackRange 내의 모든 콜라이더를 우선 감지합니다.
        // 이 구는 부채꼴 공격의 최대 반경을 나타냅니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, monsterLayer);

        // 감지된 콜라이더들을 순회하면서 부채꼴 범위 내에 있는지 확인합니다.
        foreach (Collider monsterCollider in hitColliders)
        {
            // 몬스터 오브젝트가 플레이어의 시야(앞 방향) 범위 내에 있는지 확인합니다.
            Vector3 directionToMonster = (monsterCollider.transform.position - transform.position).normalized;

            // 플레이어의 앞 방향과 몬스터 방향 벡터 간의 각도를 계산합니다.
            // Vector3.Angle 함수는 두 벡터 사이의 각도를 0도에서 180도 사이의 값으로 반환합니다.
            float angle = Vector3.Angle(transform.forward, directionToMonster);

            // 계산된 각도가 설정된 공격 각도의 절반보다 작으면 (즉, 부채꼴 범위 내에 있으면) 공격
            if (angle < attackAngle * 0.5f)
            {
                // 몬스터 태그 확인
                if (monsterCollider.CompareTag("Monster"))
                {
                    // 몬스터 부모 클래스인 Monster 컴포넌트를 가져옵니다.
                    Monster monster = monsterCollider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        // PlayerStats.Instance를 통해 공격력을 가져옵니다.
                        float damage = PlayerStats.Instance.attackPower;
                        monster.TakeDamage(damage); // Monster 클래스의 TakeDamage 메서드 호출
                        Debug.Log(monsterCollider.name + "에게 " + damage + "의 데미지를 입혔습니다! 남은 체력: " + monster.health);
                    }
                    else
                    {
                        Debug.LogWarning(monsterCollider.name + "에 Monster 스크립트가 없습니다.");
                    }
                }
            }
        }
    }

    // 공격 범위를 유니티 에디터에서 시각적으로 확인하기 위한 코드
    private void OnDrawGizmosSelected()
    {
        // Gizmo 색상 설정
        Gizmos.color = Color.red;

        // 공격 범위(반지름)와 공격 각도를 사용하여 부채꼴 모양을 그립니다.
        // 정확한 부채꼴을 그리는 것은 조금 복잡하지만, 여기서는 시각적으로 대략적인 범위를 보여줍니다.
        // 중심에서 attackRange까지 선을 긋고, 각도에 따라 옆으로 퍼지는 선을 그립니다.
        Vector3 forwardLimit = transform.position + transform.forward * attackRange;
        Gizmos.DrawLine(transform.position, forwardLimit);

        // 부채꼴의 왼쪽 경계선
        Vector3 leftLimit = Quaternion.Euler(0, -attackAngle * 0.5f, 0) * transform.forward * attackRange;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);

        // 부채꼴의 오른쪽 경계선
        Vector3 rightLimit = Quaternion.Euler(0, attackAngle * 0.5f, 0) * transform.forward * attackRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);

        // 부채꼴의 외곽선 (호)를 그리기 위해 작은 선분들로 연결합니다.
        // 정확한 호를 그리려면 더 많은 계산이 필요하므로, 여기서는 단순화하여 시각적으로 돕습니다.
        // Unity Editor의 Scene 뷰에서 플레이어의 공격 방향과 각도를 직관적으로 확인할 수 있게 됩니다.
    }
}