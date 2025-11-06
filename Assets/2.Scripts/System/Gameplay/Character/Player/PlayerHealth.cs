using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// IDetectable과 IDamageable 인터페이스를 구현하며, 플레이어의 체력 및 방어 로직을 관리합니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;
    Animator animator;
    Collider playerCollider;
    Rigidbody playerRigidbody;
    // 데미지 효과를 표시할 UI Image 컴포넌트에 대한 참조입니다.
    [Header("UI Feedback")]
    [Tooltip("화면 가장자리에 붉은색 효과를 표시할 Image 컴포넌트")]
    [SerializeField]
    private UnityEngine.UI.Image damageVignetteImage;

    [Tooltip("데미지 효과가 사라지는 데 걸리는 시간입니다.")]
    [SerializeField]
    private float fadeDuration = 0.5f;

    // 현재 페이드 아웃 코루틴이 실행 중인지 확인하는 변수 (중복 실행 방지)
    private Coroutine fadeOutCoroutine = null;

    private AudioSource playerAudioSource;

    [Header("Audio")] // 추가: 오디오 헤더
    [Tooltip("플레이어가 피격될 때 재생할 효과음 클립입니다.")]
    public AudioClip hitSoundClip; // 인스펙터에서 할당할 피격 효과음
    [Tooltip("플레이어가 사망할 때 재생할 효과음 클립입니다. (단발성)")]
    public AudioClip dieSoundClip; // 사망 효과음

    // LoL 스타일의 데미지 감소 계산에 사용되는 상수
    // (예: 방어력 100일 때 50% 피해 감소)
    private const float DAMAGE_REDUCTION_CONSTANT = 100f;

    void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        // **SOLID 규칙: 의존성 주입(Dependency Injection)은 아니지만, 중앙 허브를 통해 필요한 정보를 가져와 응집도를 높임.**
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. PlayerHealth 스크립트가 제대로 동작하지 않을 수 있습니다.");
        }
        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
        {
            Debug.LogWarning("PlayerHealth 스크립트가 붙은 오브젝트에 AudioSource 컴포넌트가 없습니다.");
        }
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("PlayerHealth 스크립트가 붙은 오브젝트에 Animator 컴포넌트가 없습니다.");
        }
        playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
        {
            Debug.LogWarning("PlayerHealth 스크립트가 붙은 오브젝트에 Collider 컴포넌트가 없습니다.");
        }
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogWarning("PlayerHealth 스크립트가 붙은 오브젝트에 Rigidbody 컴포넌트가 없습니다.");
        }
    }

    // IDetectable 인터페이스의 메서드 구현

    /// <summary>
    /// 플레이어가 감지 가능한 상태인지 확인합니다.
    /// </summary>
    public bool IsDetectable()
    {
        // PlayerCharacter 및 playerStats가 유효한지 먼저 확인합니다.
        if (playerCharacter != null && playerCharacter.playerStats != null)
        {
            // 플레이어가 살아있다면 감지 가능하도록 true를 반환합니다.
            return playerCharacter.playerStats.health > 0;
        }
        return false;
    }

    /// <summary>
    /// 이 오브젝트의 트랜스폼을 반환합니다.
    /// </summary>
    public Transform GetTransform()
    {
        return this.transform;
    }

    // IDamageable 인터페이스의 메서드 구현 (오버로딩)

    /// <summary>
    /// 순수 데미지 값을 받는 메서드입니다. (방어력 미적용)
    /// </summary>
    /// <param name="amount">입을 데미지량</param>
    public void TakeDamage(float amount)
    {
        // 일반 데미지는 True 데미지와 동일하게 방어력 무시로 처리
        ApplyDamage(amount, DamageType.True);
    }

    /// <summary>
    /// 데미지 타입에 따라 플레이어의 방어력을 적용하여 피해를 계산하는 메서드입니다.
    /// **SOLID 규칙: 단일 책임 원칙(SRP)에 따라 데미지 계산 및 적용 로직은 이 클래스에 집중됩니다.**
    /// </summary>
    /// <param name="amount">입을 데미지량</param>
    /// <param name="type">데미지 타입 (물리, 마법, 고정 피해 등)</param>
    public void TakeDamage(float amount, DamageType type)
    {
        ApplyDamage(amount, type);
    }

    /// <summary>
    /// 데미지 타입에 따른 최종 피해량을 계산하고 적용하는 핵심 로직입니다.
    /// </summary>
    /// <param name="amount">기본 데미지량</param>
    /// <param name="type">데미지 타입</param>
    private void ApplyDamage(float amount, DamageType type)
    {
        if (MainSceneManager.Instance.isGameOver)
        {
            return;
        }
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. 데미지 적용 실패.");
            return;
        }

        float finalDamage = amount;
        float reductionValue = 0f; // 적용할 방어력/마법 방어력 스탯

        // 데미지 타입에 따라 방어력/마법 방어력 선택
        switch (type)
        {
            case DamageType.Physical:
                reductionValue = playerCharacter.playerStats.defense;
                break;
            case DamageType.Magic:
                reductionValue = playerCharacter.playerStats.magicDefense;
                break;
            case DamageType.True:
                // 고정 피해는 방어력 및 마법 방어력을 무시합니다.
                break;
        }

        // [수정된 로직] LoL 방식의 피해 감소율 계산 및 적용
        if (type != DamageType.True)
        {
            // 1. 피해 감소율 계산 (0 ~ 1.0f)
            // 공식: DamageReduction = ReductionValue / (ReductionValue + Constant)
            // 이 공식은 감소율이 100%를 초과할 수 없도록 보장하며, 방어력 증가에 따라 감소율 증가폭이 점차 줄어듭니다.
            float damageReduction = reductionValue / (reductionValue + DAMAGE_REDUCTION_CONSTANT);

            // 2. 최종 피해량 계산: FinalDamage = Amount * (1 - DamageReduction)
            finalDamage = amount * (1f - damageReduction);

            // 최종 피해량이 음수가 되는 것을 방지 (힐이 되는 상황 방지)
            finalDamage = Mathf.Max(finalDamage, 0f);
        }

        // 체력 적용
        playerCharacter.playerStats.health -= finalDamage;

        // 귀환 중 피격 시 귀환 취소
        if (PlayerCharacter.Instance.IsReturnProcessActive)
        {
            PlayerCharacter.Instance.CancelReturn();
        }

        // 피격 효과 및 사운드 재생
        ShowDamageEffect();
        PlayHitSound();

        //Debug.Log($"[LoL식 적용] 플레이어가 {amount}(기본) -> {finalDamage:F2}(최종)의 {type} 피해를 입었습니다. 남은 체력: {playerCharacter.playerStats.health:F2}");

        // 사망 체크
        if (playerCharacter.playerStats.health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 피격 효과음을 재생합니다. (AudioSource와 클립이 할당되어 있다면)
    /// </summary>
    private void PlayHitSound() // 추가: 피격음 재생 헬퍼 메서드
    {
        if (playerAudioSource != null && hitSoundClip != null)
        {
            // PlayOneShot을 사용하여 현재 재생 중인 소리(공격음 등)와 충돌 없이 동시 재생합니다.
            playerAudioSource.PlayOneShot(hitSoundClip);
        }
    }

    /// <summary>
    /// 데미지 효과(붉은색 화면 오버레이)를 표시하고 서서히 사라지게 합니다.
    /// </summary>
    private void ShowDamageEffect()
    {
        if (damageVignetteImage == null)
        {
            Debug.LogWarning("Damage Vignette Image가 할당되지 않아 데미지 효과를 재생할 수 없습니다.");
            return;
        }

        // 이미 코루틴이 실행 중이라면 중지하고 재시작하여 효과를 갱신합니다.
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }

        // 새 코루틴 시작
        fadeOutCoroutine = StartCoroutine(FadeDamageVignette());
    }

    /// <summary>
    /// 붉은색 화면 효과의 투명도를 즉시 최대로 설정한 후 서서히 0으로 페이드 아웃 시킵니다.
    /// </summary>
    private IEnumerator FadeDamageVignette()
    {
        // 1. 최대 투명도 설정 (즉시 효과가 나타나도록)
        // 데미지 시 표시할 최대 투명도를 여기서 설정합니다. (0.0f ~ 1.0f)
        float maxAlpha = 0.6f;
        Color startColor = damageVignetteImage.color;
        startColor.a = maxAlpha;
        damageVignetteImage.color = startColor;

        float timer = 0f;

        // 2. 시간 경과에 따라 투명도를 0으로 감소 (페이드 아웃)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(maxAlpha, 0f, timer / fadeDuration);

            Color newColor = damageVignetteImage.color;
            newColor.a = currentAlpha;
            damageVignetteImage.color = newColor;

            yield return null; // 다음 프레임까지 대기
        }

        // 3. 완전히 사라진 후 투명도를 0으로 확정합니다.
        Color finalColor = damageVignetteImage.color;
        finalColor.a = 0f;
        damageVignetteImage.color = finalColor;

        fadeOutCoroutine = null; // 코루틴이 완료되었음을 표시
    }

    /// <summary>
    /// 플레이어가 죽었을 때 호출될 메서드
    /// **SOLID 규칙: 단일 책임 원칙(SRP)에 따라 사망 처리 관련 로직이 이 메서드에 캡슐화됩니다.**
    /// </summary>
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");

        if (MainSceneManager.Instance.isGameOver) return; // 이미 게임 오버 상태라면 중복 호출 방지
        animator.SetTrigger("Die");

        // 오브젝트 비활성화 및 컨트롤 제한
        if (playerCollider != null) playerCollider.enabled = false; // 충돌 비활성화
        if (playerRigidbody != null) playerRigidbody.isKinematic = true; // 물리 비활성화
        if (playerCharacter != null && playerCharacter.playerController != null)
            playerCharacter.playerController.enabled = false; // 플레이어 컨트롤러 비활성화

        // 사망 효과음 재생
        if (playerAudioSource != null && dieSoundClip != null)
        {
            // PlayOneShot을 사용하여 사망 BGM이 시작되기 직전에 단발성 효과음을 재생합니다.
            playerAudioSource.PlayOneShot(dieSoundClip);
        }

        // BGM 변경 및 게임 오버 처리
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.Main_D);
        }
        MainSceneManager.Instance.isGameOver = true;
        DungeonManager.Instance.DeadDungeon(); // 던전 상태 리셋
        MainSceneManager.Instance.SetGameOver();
    }
}