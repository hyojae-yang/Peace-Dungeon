using UnityEngine;

// 플레이어의 경험치와 레벨업을 관리하는 스크립트입니다.
public class PlayerLevelUp : MonoBehaviour
{
    // PlayerStats 스크립트 참조 변수
    private PlayerStats playerStats;
    // PlayerStatSystem 스크립트 참조 변수
    private PlayerStatSystem playerStatSystem;

    // 다음 레벨에 필요한 경험치량을 계산하는 데 사용되는 변수
    [Header("레벨업 공식 설정")]
    [Tooltip("다음 레벨에 필요한 기본 경험치량입니다.")]
    public float baseExp = 10f;
    [Tooltip("레벨이 오를수록 경험치가 증가하는 비율입니다.")]
    public float expGrowthFactor = 1.2f;

    void Start()
    {
        // 동일한 게임 오브젝트에 있는 PlayerStats 컴포넌트를 찾습니다.
        playerStats = GetComponent<PlayerStats>();
        // 동일한 게임 오브젝트에 있는 PlayerStatSystem 컴포넌트를 찾습니다.
        playerStatSystem = GetComponent<PlayerStatSystem>();

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트가 PlayerLevelUp 스크립트와 같은 게임 오브젝트에 없습니다.");
            return;
        }

        if (playerStatSystem == null)
        {
            Debug.LogError("PlayerStatSystem 컴포넌트가 PlayerLevelUp 스크립트와 같은 게임 오브젝트에 없습니다.");
            return;
        }

        // 게임 시작 시 초기 requiredExperience를 설정합니다.
        CalculateRequiredExperience();
    }

    // 외부에서 호출하여 플레이어에게 경험치를 추가하는 메서드
    public void AddExperience(float amount)
    {
        if (playerStats == null) return;

        playerStats.experience += (int)amount;

        CheckForLevelUp();
    }

    // 다음 레벨에 필요한 경험치량을 계산하여 PlayerStats에 저장합니다.
    private void CalculateRequiredExperience()
    {
        // 등비수열 공식: 필요한 경험치 = baseExp * (expGrowthFactor ^ (level - 1))
        playerStats.requiredExperience = baseExp * Mathf.Pow(expGrowthFactor, playerStats.level - 1);
    }

    // 경험치를 확인하고 레벨업이 가능한지 체크하는 메서드
    private void CheckForLevelUp()
    {
        while (playerStats.experience >= playerStats.requiredExperience)
        {
            LevelUp();
            CalculateRequiredExperience();
            Debug.Log($"다음 레벨까지 남은 경험치: {playerStats.experience} / {playerStats.requiredExperience}");
        }
    }

    // 플레이어를 레벨업시키는 메서드
    private void LevelUp()
    {
        float remainingExp = playerStats.experience - playerStats.requiredExperience;

        // 레벨과 경험치를 업데이트합니다.
        playerStats.level++;
        playerStats.experience = (int)remainingExp;

        // 레벨업 시 스탯 포인트를 5점 지급합니다.
        if (playerStatSystem != null)
        {
            playerStatSystem.statPoints += 5;
        }
        // 레벨업 시 스킬 포인트를 2점 지급합니다.
        if (playerStats != null)
        {
            playerStats.skillPoints += 2;
        }

        // 레벨업에 따른 스탯 증가 로직을 PlayerStatSystem에 위임합니다.
        playerStatSystem.UpdateFinalStats();
        playerStatSystem.StoreTempStats();

        Debug.Log($"축하합니다! 레벨 {playerStats.level}로 레벨업했습니다!");
    }
}