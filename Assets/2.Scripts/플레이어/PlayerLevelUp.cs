using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 경험치와 레벨업을 관리하는 스크립트입니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
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
    // === 이벤트 선언 ===
    /// <summary>
    /// 플레이어가 레벨업했을 때 외부에 알리는 이벤트입니다.
    /// </summary>
    public static event System.Action OnPlayerLeveledUp;

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
    /// </summary>
    /// <param name="amount">추가할 경험치량</param>
    public void AddExperience(float amount)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. 경험치 추가 실패.");
            return;
        }

        playerCharacter.playerStats.experience += (int)amount;

        CheckForLevelUp();
    }

    /// <summary>
    /// 다음 레벨에 필요한 경험치량을 계산하여 PlayerStats에 저장합니다.
    /// </summary>
    public void CalculateRequiredExperience()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        // PlayerStats를 통해 데이터에 접근합니다.
        // 등비수열 공식: 필요한 경험치 = baseExp * (expGrowthFactor ^ (level - 1))
        playerCharacter.playerStats.requiredExperience = baseExp * Mathf.Pow(expGrowthFactor, playerCharacter.playerStats.level - 1);
    }

    /// <summary>
    /// 경험치를 확인하고 레벨업이 가능한지 체크하는 메서드
    /// </summary>
    private void CheckForLevelUp()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        while (playerCharacter.playerStats.experience >= playerCharacter.playerStats.requiredExperience)
        {
            LevelUp();
            CalculateRequiredExperience();
        }
    }

    /// <summary>
    /// 플레이어를 레벨업시키는 메서드
    /// </summary>
    private void LevelUp()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. 레벨업 실패.");
            return;
        }

        float remainingExp = playerCharacter.playerStats.experience - playerCharacter.playerStats.requiredExperience;

        // 레벨과 경험치를 업데이트합니다.
        playerCharacter.playerStats.level++;
        playerCharacter.playerStats.experience = (int)remainingExp;

        // 레벨업 시 스탯 포인트를 지급합니다. (기존 로직 유지)
        if (playerCharacter.playerStatSystem != null)
        {
            playerCharacter.playerStatSystem.statPoints += 5;

            // 레벨업에 따른 스탯 증가 로직을 PlayerStatSystem에 위임합니다.
            playerCharacter.playerStatSystem.UpdateFinalStats();
            playerCharacter.playerStatSystem.StoreTempStats();
        }

        // 레벨업 시 스킬 포인트를 지급합니다. (기존 로직 유지)
        if (playerCharacter.playerStats != null)
        {
            playerCharacter.playerStats.skillPoints += 2;
        }

        // 체력 및 마나 회복 (기존 로직 유지)
        playerCharacter.playerStats.health = playerCharacter.playerStats.MaxHealth;
        playerCharacter.playerStats.mana = playerCharacter.playerStats.MaxMana;
        // 레벨업이 완료되었음을 외부에 알리는 이벤트를 발생시킵니다.
        OnPlayerLeveledUp?.Invoke();
    }
}