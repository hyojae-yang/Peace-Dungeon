using UnityEngine;
using System; // �̺�Ʈ ����� ���� System ���ӽ����̽� �߰�

/// <summary>
/// ������ ���� ����(���� ó��, ����)�� ����ϴ� Ŭ�����Դϴ�.
/// IDamageable �������̽��� �����Ͽ� ���ظ� ���� �� �ֵ��� �մϴ�.
/// </summary>
public class MonsterCombat : MonoBehaviour, IDamageable
{
    private MonsterBase monsterBase;
    private float currentHealth;

    // === �̺�Ʈ ===
    // �������� �Ծ��� �� �ٸ� ��ũ��Ʈ�� �˸��� �̺�Ʈ
    public event Action<float> OnDamageTaken;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterCombat: MonsterBase ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        currentHealth = monsterBase.monsterData.maxHealth;
    }

    // --- IDamageable �������̽� ���� ---
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

        // �̺�Ʈ ȣ��: ������ ���� ���ڷ� �����մϴ�.
        // �ٸ� ��ũ��Ʈ���� �� �̺�Ʈ�� �����Ͽ� �ʿ��� �ൿ�� ������ �� �ֽ��ϴ�.
        OnDamageTaken?.Invoke(finalDamage);

        if (currentHealth <= 0)
        {
            monsterBase.Die();
        }
    }
}