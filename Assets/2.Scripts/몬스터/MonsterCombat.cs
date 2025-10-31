using UnityEngine;
using System; // 이벤트 사용을 위해 System 네임스페이스 추가

/// <summary>
/// 몬스터의 전투 로직(피해 처리, 공격)을 담당하는 클래스입니다.
/// IDamageable 인터페이스를 구현하여 피해를 입을 수 있도록 합니다.
/// </summary>
public class MonsterCombat : MonoBehaviour, IDamageable
{
    public MonsterBase monsterBase { get; private set; }
    AudioSource audioSource;

    [Header("사운드 설정")] // 추가: 인스펙터 관리를 위해 헤더 추가
    [Tooltip("몬스터가 피격 시 재생되는 효과음입니다.")]
    public AudioClip hitSound;

    [Tooltip("몬스터의 체력이 0이 되어 사망할 때 재생되는 효과음입니다.")]
    public AudioClip deathSound; // 추가: 사망 효과음 클립

    private float currentHealth;
    public ParticleSystem hitVFX;
    // === 이벤트 ===
    // 데미지를 입었을 때 다른 스크립트에 알리는 기존 이벤트
    public event Action<float> OnDamageTaken;

    // 1. 추가된 훅: 현재 체력 값이 변경될 때마다 남은 체력을 외부에 알립니다.
    /// <summary>
    /// 현재 체력 값이 변경될 때 남은 체력 값을 인자로 전달하는 이벤트. (보스 UI 시스템이 사용)
    /// </summary>
    public event Action<float> OnHealthUpdated;

    // 2. 추가된 훅: 몬스터/보스가 사망할 때 외부에 알립니다.
    /// <summary>
    /// 몬스터/보스가 사망 처리 직전에 호출되는 이벤트. (보스 UI 시스템이 사용)
    /// </summary>
    public event Action OnDefeated;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterCombat: MonsterBase 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        audioSource = GetComponent<AudioSource>();
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
        // 피격 효과음 재생
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 기존 이벤트 호출
        OnDamageTaken?.Invoke(finalDamage);

        // 3. 훅 호출: 체력 변경 후, 현재 남은 체력을 외부에 알립니다.
        OnHealthUpdated?.Invoke(currentHealth);
        hitVFX?.Play(true);

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(finalDamage, transform.position, type);
        }

        if (currentHealth <= 0)
        {
            // 4. 훅 호출: 사망 처리(Die) 직전에 사망 이벤트를 외부에 알립니다.
            OnDefeated?.Invoke();

            // 몬스터 사망 효과음 재생 로직 (Die() 호출 직전)
            // monsterBase.Die()에서 오브젝트가 비활성화/파괴될 수 있으므로 먼저 소리를 재생합니다.
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            monsterBase.Die();
        }
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}