using UnityEngine;

/// <summary>
/// 몬스터의 전투 로직(피해 처리, 공격)을 담당하는 클래스입니다.
/// IDamageable 인터페이스를 구현하여 피해를 입을 수 있도록 합니다.
/// </summary>
public class MonsterCombat : MonoBehaviour, IDamageable
{
    private MonsterBase monsterBase;
    private float currentHealth;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterCombat: MonsterBase 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        currentHealth = monsterBase.monsterData.maxHealth;
    }

    // --- IDamageable 인터페이스 구현 ---
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, DamageType.Physical);
    }

    public void TakeDamage(float damage, DamageType type)
    {
        if (monsterBase.currentState == MonsterBase.MonsterState.Dead) return;

        float finalDamage = damage;
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(damage - monsterBase.monsterData.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(damage - monsterBase.monsterData.magicDefense, 0);
                break;
            case DamageType.True:
                break;
        }

        currentHealth -= finalDamage;

        if (currentHealth <= 0)
        {
            monsterBase.Die();
        }
    }
}