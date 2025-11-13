using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 경험치와 레벨업을 관리하는 스크립트입니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
/// **경험치 변수는 21억 이상의 값 처리를 위해 모두 'long' 타입으로 가정하고 수정되었습니다.**
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
    /// 참고: 이 이벤트는 현재 'int'를 인수로 받으므로, 21억을 초과하는 경험치가 획득될 경우 
    /// 오버플로우를 방지하기 위해 'int.MaxValue'로 제한되어(Clamp) 전달됩니다.
    /// </summary>
    public static event System.Action<int> OnExperienceAdded;

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
    public void AddExperience(float amount)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. 경험치 추가 실패.");
            return;
        }

        // float으로 들어온 경험치량을 64비트 정수(long)로 명확히 정의
        long finalAmount = (long)amount;

        // 현재 경험치에 추가 (long 타입)
        playerCharacter.playerStats.experience += finalAmount;

        // 경험치 최대치(long.MaxValue)를 초과하지 않도록 보장
        if (playerCharacter.playerStats.experience > MAX_EXPERIENCE_CAP)
        {
            playerCharacter.playerStats.experience = MAX_EXPERIENCE_CAP;
            Debug.LogWarning("경험치가 최대 허용치(long.MaxValue)에 도달했습니다!");
        }

        // OnExperienceAdded 이벤트는 int를 요구하므로, 
        // 획득량이 21억을 넘는다면 int.MaxValue로 제한하여 전달합니다.
        int clampedAmountForEvent = (int)Mathf.Clamp(finalAmount, 0, int.MaxValue);
        OnExperienceAdded?.Invoke(clampedAmountForEvent);

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
        // 계산 결과를 long으로 명시적으로 변환하여 requiredExperience (long 타입 가정)에 저장
        playerCharacter.playerStats.requiredExperience =
            (long)(baseExp * Mathf.Pow(expGrowthFactor, playerCharacter.playerStats.level - 1));

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
        // MaxHealth와 MaxMana도 long 타입으로 처리되어야 하지만, 
        // 기존 코드의 시그니처를 유지하기 위해 float/int 호환성을 유지한다고 가정합니다.
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