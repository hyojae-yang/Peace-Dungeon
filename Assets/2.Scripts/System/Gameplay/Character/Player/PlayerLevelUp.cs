using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 경험치와 레벨업을 관리하는 스크립트입니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
/// **경험치 변수는 21억 이상의 값 처리를 위해 모두 'long' 타입으로 가정하고 수정되었습니다.**
/// SOLID: 이벤트 호출 시 외부 시스템 오류로부터 핵심 로직(레벨업)을 보호하기 위해 try-catch를 추가했습니다.
/// </summary>
public class PlayerLevelUp : MonoBehaviour
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // 다음 레벨에 필요한 경험치량을 계산하는 데 사용되는 변수
    [Header("레벨업 공식 설정")]
    [Tooltip("다음 레벨에 필요한 기본 경험치량입니다.")]
    public float baseExp = 10f;
    [Tooltip("레벨이 오를수록 경험치가 증가하는 비율입니다.")]
    public float expGrowthFactor = 1.3f;

    // C# 64비트 정수(long)의 최대값: 약 922경
    private const long MAX_EXPERIENCE_CAP = long.MaxValue;

    // === 이벤트 선언 ===
    /// <summary>
    /// 플레이어가 레벨업했을 때 외부에 알리는 이벤트입니다.
    /// </summary>
    public static event System.Action OnPlayerLeveledUp;

    /// <summary>
    /// 플레이어에게 경험치가 추가될 때(획득 시) 외부에 추가된 경험치량을 알립니다.
    /// </summary>
    public static event System.Action<long> OnExperienceAdded;

    void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. PlayerLevelUp 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        // 게임 시작 시 초기 requiredExperience를 설정합니다.
        CalculateRequiredExperience();
    }

    /// <summary>
    /// 외부에서 호출하여 플레이어에게 경험치를 추가하는 메서드
    /// 경험치 변수가 long 타입이므로 21억 이상도 안전하게 처리됩니다.
    /// </summary>
    /// <param name="amount">추가할 경험치량</param>
    public void AddExperience(long amount)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. 경험치 추가 실패.");
            return;
        }

        long finalAmount = amount;

        // 현재 경험치에 추가 (long 타입)
        playerCharacter.playerStats.experience += finalAmount;

        // 경험치 최대치(long.MaxValue)를 초과하지 않도록 보장
        if (playerCharacter.playerStats.experience > MAX_EXPERIENCE_CAP)
        {
            playerCharacter.playerStats.experience = MAX_EXPERIENCE_CAP;
            Debug.LogWarning("경험치가 최대 허용치(long.MaxValue)에 도달했습니다!");
        }

        // --- [수정된 부분: 방어 코드] 이벤트 호출 try-catch 적용 ---
        // UI 시스템(RewardTextManager)의 Null 참조 오류로 인해 LevelUp()이 막히는 것을 방지합니다.
        try
        {
            OnExperienceAdded?.Invoke(finalAmount);
        }
        catch (System.Exception ex)
        {
            // 이벤트 구독자에서 예외가 발생하더라도 핵심 로직(레벨업 체크)은 계속 실행되도록 합니다.
            Debug.LogError($"[BUG_GUARD] OnExperienceAdded 이벤트 구독자(UI)에서 오류 발생! 레벨업 체크는 계속 진행됩니다. 오류: {ex.Message}");
        }
        // ------------------------------------------------------------------

        // 오류가 발생했더라도 이 코드가 반드시 호출되어 레벨업이 진행되도록 보장합니다.
        CheckForLevelUp();
    }

    /// <summary>
    /// 다음 레벨에 필요한 경험치량을 계산하여 PlayerStats에 저장합니다.
    /// 계산 결과는 long 타입으로 형변환하여 저장되어 오버플로우를 방지합니다.
    /// </summary>
    public void CalculateRequiredExperience()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        // 등비수열 공식: 필요한 경험치 = baseExp * (expGrowthFactor ^ (level - 1))
        long calculatedExp = (long)(baseExp * Mathf.Pow(expGrowthFactor, playerCharacter.playerStats.level - 1));

        playerCharacter.playerStats.requiredExperience = calculatedExp;

        // 필요한 경험치 역시 long.MaxValue를 초과하지 않도록 처리
        if (playerCharacter.playerStats.requiredExperience > MAX_EXPERIENCE_CAP)
        {
            playerCharacter.playerStats.requiredExperience = MAX_EXPERIENCE_CAP;
        }

    }

    /// <summary>
    /// 경험치를 확인하고 레벨업이 가능한지 체크하는 메서드
    /// </summary>
    private void CheckForLevelUp()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        // [로그 C: 루프 조건 확인]
        bool isLevelUpConditionMet = playerCharacter.playerStats.experience >= playerCharacter.playerStats.requiredExperience;

        // long 대 long 비교이므로 21억 이상에서도 안전합니다.
        while (playerCharacter.playerStats.experience >= playerCharacter.playerStats.requiredExperience)
        {
            LevelUp();

            // 레벨업을 했으므로 다음 레벨에 필요한 경험치를 즉시 다시 계산합니다.
            CalculateRequiredExperience();
        }
    }

    /// <summary>
    /// 플레이어를 레벨업시키는 메서드
    /// </summary>
    public void LevelUp()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. 레벨업 실패.");
            return;
        }

        // UI 튜토리얼 핸들러에 레벨업 감지 알림
        // (UITutorialHandler가 실제로 존재한다고 가정합니다)
        if (UITutorialHandler.Instance != null)
        { UITutorialHandler.Instance.OnLevelUpDetected.Invoke(); }

        // 남은 경험치 계산 (long - long = long)
        long remainingExp = playerCharacter.playerStats.experience - playerCharacter.playerStats.requiredExperience;

        // 레벨과 경험치를 업데이트합니다.
        playerCharacter.playerStats.level++;
        // 남은 경험치 저장 (long 타입 유지)
        playerCharacter.playerStats.experience = remainingExp;

        // 레벨업 시 스탯 포인트를 지급합니다.
        if (playerCharacter.playerStatSystem != null)
        {
            playerCharacter.playerStatSystem.statPoints += 3;

            // 레벨업에 따른 스탯 증가 로직을 PlayerStatSystem에 위임합니다.
            playerCharacter.playerStatSystem.UpdateFinalStats();
            playerCharacter.playerStatSystem.StoreTempStats();
        }

        // 레벨업 시 스킬 포인트를 지급합니다.
        if (playerCharacter.playerStats != null)
        {
            playerCharacter.playerStats.skillPoints += 1;
        }

        // 체력 및 마나 회복
        playerCharacter.playerStats.health = playerCharacter.playerStats.MaxHealth;
        playerCharacter.playerStats.mana = playerCharacter.playerStats.MaxMana;

        // 레벨업이 완료되었음을 외부에 알리는 이벤트를 발생시킵니다.
        OnPlayerLeveledUp?.Invoke();

        // NotificationManager를 사용하여 레벨 업 성공 알림을 표시합니다.
        if (NotificationManager.Instance != null)
        {
            string currentLevel = playerCharacter.playerStats.level.ToString();
            // 플레이어의 현재 레벨을 포함하여 메시지를 구성합니다.
            NotificationManager.Instance.ShowNotification($"레벨 업! Lv. {currentLevel} 달성!", NotificationType.Success);
        }

        // 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Levelup_sound, 0.5f);
        }
    }
}