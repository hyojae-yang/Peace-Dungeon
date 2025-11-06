using UnityEngine;
using System; // 이벤트 사용을 위해 System 네임스페이스 추가

/// <summary>
/// 몬스터의 전투 로직(피해 처리, 공격)을 담당하는 클래스입니다.
/// IDamageable 인터페이스를 구현하여 피해를 입을 수 있도록 합니다.
/// SOLID 원칙: SRP(단일 책임 원칙)에 따라, 전투 로직과 이벤트 발행 역할만 수행합니다.
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

    // [추가] LoL 스타일의 데미지 감소 계산에 사용되는 상수
    // 플레이어와 동일하게 100을 사용합니다. 방어력이 100일 때 50% 피해 감소.
    private const float DAMAGE_REDUCTION_CONSTANT = 100f;

    // === 이벤트 ===

    // 데미지를 입었을 때 다른 스크립트에 알리는 기존 이벤트
    public event Action<float> OnDamageTaken;

    // 1. 현재 체력 값이 변경될 때마다 남은 체력을 외부에 알립니다.
    /// <summary>
    /// 현재 체력 값이 변경될 때 남은 체력 값을 인자로 전달하는 이벤트. (보스 UI 시스템이 사용)
    /// </summary>
    public event Action<float> OnHealthUpdated;

    // 2. 몬스터/보스가 사망할 때 외부에 알립니다.
    /// <summary>
    /// 몬스터/보스가 사망 처리 직전에 호출되는 이벤트. (보스 UI 시스템이 사용)
    /// </summary>
    public event Action OnDefeated;

    // 3. 몬스터가 피해를 입었을 때 경직 상태를 적용해야 함을 외부에 알리는 이벤트
    /// <summary>
    /// 몬스터가 피해를 입어 경직 효과가 발동해야 할 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action OnStunApplied;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterCombat: MonsterBase 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        audioSource = GetComponent<AudioSource>();
        // Awake 시점에 최대 체력으로 초기화합니다.
        currentHealth = monsterBase.monsterData.maxHealth;
    }

    // --- IDamageable 인터페이스 구현 ---
    public void TakeDamage(float damage)
    {
        // 일반 TakeDamage는 물리 피해로 처리하거나, 필요하다면 True Damage로 처리할 수도 있습니다.
        // 여기서는 기존 TakeDamage와 일관성을 위해 물리 피해로 처리합니다.
        TakeDamage(damage, DamageType.Physical);
    }

    public void TakeDamage(float damage, DamageType type)
    {
        // 몬스터가 이미 사망했다면 데미지 로직을 무시
        if (monsterBase.currentState == MonsterBase.MonsterState.Dead) return;

        float finalDamage = damage;
        float reductionValue = 0f; // 적용할 방어력/마법 방어력 스탯

        // 데미지 타입에 따라 최종 피해량 계산 로직 변경
        switch (type)
        {
            case DamageType.Physical:
                reductionValue = monsterBase.monsterData.defense;

                // [수정된 로직] LoL 방식의 물리 피해 감소율 적용
                // 피해 감소율 = reductionValue / (reductionValue + Constant)
                float physicalDamageReduction = reductionValue / (reductionValue + DAMAGE_REDUCTION_CONSTANT);
                // 최종 피해량 = 기본 피해량 * (1 - 피해 감소율)
                finalDamage = damage * (1f - physicalDamageReduction);
                // 최소 피해는 0 (음수 방지)
                finalDamage = Mathf.Max(finalDamage, 0f);

                break;

            case DamageType.Magic:
                reductionValue = monsterBase.monsterData.magicDefense;

                // [수정된 로직] LoL 방식의 마법 피해 감소율 적용
                // 피해 감소율 = reductionValue / (reductionValue + Constant)
                float magicDamageReduction = reductionValue / (reductionValue + DAMAGE_REDUCTION_CONSTANT);
                // 최종 피해량 = 기본 피해량 * (1 - 피해 감소율)
                finalDamage = damage * (1f - magicDamageReduction);
                // 최소 피해는 0 (음수 방지)
                finalDamage = Mathf.Max(finalDamage, 0f);

                break;

            case DamageType.True:
                // 고정 피해 (방어력 무시): finalDamage는 damage 그대로 유지
                break;
        }

        // 최종 피해량이 0보다 클 경우에만 경직 및 피해 로직 진행
        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;

            // 피격 효과음 재생
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // 기존 이벤트 호출 (최종 피해량 알림)
            OnDamageTaken?.Invoke(finalDamage);

            // 경직 이벤트를 호출하여 Stun 상태로 전환 요청
            OnStunApplied?.Invoke();

            // 체력 업데이트 훅 호출
            OnHealthUpdated?.Invoke(currentHealth);
            hitVFX?.Play(true);

            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowDamage(finalDamage, transform.position, type);
            }

            // 디버그 로그 추가: 변경된 계산 방식 확인
           // Debug.Log($"[Monster LoL식 적용] {type} 기본 피해: {damage:F2}, 방어 스탯: {reductionValue:F2}, 최종 피해: {finalDamage:F2}, 남은 체력: {currentHealth:F2}");
        }

        if (currentHealth <= 0)
        {
            // 사망 이벤트 훅 호출 (사망 처리 직전에 알림)
            OnDefeated?.Invoke();

            // 몬스터 사망 효과음 재생
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            // 실제 사망 로직 호출
            monsterBase.Die();
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}