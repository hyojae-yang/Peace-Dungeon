using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Monster : MonoBehaviour, IDamageable, IDetectable
{
    // === 몬스터 기본 스탯 변수 ===
    [Header("몬스터 기본 스탯")]
    public float health; // 체력
    public int mana; // 마나
    public int attackPower; // 공격력
    public int magicAttackPower; // 마법 공격력
    public float moveSpeed; // 이동 속도 (int -> float으로 변경)
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

    protected virtual void Awake()
    {
    }

    protected virtual void Update()
    {
        DetectPlayer();

        if (isPlayerDetected && detectableTarget != null)
        {
            transform.LookAt(detectableTarget.GetTransform(), Vector3.up);

            Vector3 direction = (detectableTarget.GetTransform().position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // 플레이어를 감지하는 메서드
    protected void DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        isPlayerDetected = false;
        detectableTarget = null;

        foreach (Collider hit in hitColliders)
        {
            IDetectable target = hit.GetComponent<IDetectable>();
            if (target != null && target.IsDetectable())
            {
                Vector3 directionToTarget = (target.GetTransform().position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);
                if (angle < detectionAngle * 0.5f)
                {
                    isPlayerDetected = true;
                    detectableTarget = target;
                    return;
                }
            }
        }
    }

    // 공격 메서드 - virtual 키워드를 사용하여 자식 클래스에서 재정의 가능
    public virtual void Attack()
    {
        if (detectableTarget == null) return;
        IDamageable target = detectableTarget as IDamageable;
        if (target != null)
        {
            target.TakeDamage(attackPower, attackDamageType);
        }
    }

    // --- IDamageable 인터페이스 구현 (TakeDamage 메서드) ---
    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, DamageType.Physical);
    }

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
                break;
        }

        health -= finalDamage;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        GiveRewardToPlayer();
        Destroy(gameObject);
    }

    private void GiveRewardToPlayer()
    {
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

        float expReward = Random.Range(minExpReward, maxExpReward);
        int goldReward = Random.Range(minGoldReward, maxGoldReward + 1);

        playerLevelUp.AddExperience(expReward);
        playerStats.gold += goldReward;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 forwardLimit = transform.position + transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, forwardLimit);
        Vector3 leftLimit = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
        Vector3 rightLimit = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
    }

    // IDetectable 인터페이스 구현
    public bool IsDetectable()
    {
        return true;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}