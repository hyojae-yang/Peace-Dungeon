using UnityEngine;
using System.Collections.Generic;

// 플레이어의 경험치와 레벨업을 관리하는 스크립트입니다.
public class PlayerLevelUp : MonoBehaviour
{
    // PlayerStats와 PlayerStatSystem 스크립트는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.
    // private PlayerStats playerStats;
    // private PlayerStatSystem playerStatSystem;

    // 다음 레벨에 필요한 경험치량을 계산하는 데 사용되는 변수
    [Header("레벨업 공식 설정")]
    [Tooltip("다음 레벨에 필요한 기본 경험치량입니다.")]
    public float baseExp = 10f;
    [Tooltip("레벨이 오를수록 경험치가 증가하는 비율입니다.")]
    public float expGrowthFactor = 1.2f;

    void Start()
    {
        // 게임 시작 시 PlayerStats와 PlayerStatSystem 싱글턴 인스턴스가 존재하는지 확인합니다.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats 인스턴스가 존재하지 않습니다. 게임 시작 시 해당 컴포넌트를 가진 게임 오브젝트가 씬에 있는지 확인해 주세요.");
            return;
        }

        if (PlayerStatSystem.Instance == null)
        {
            Debug.LogError("PlayerStatSystem 인스턴스가 존재하지 않습니다. 게임 시작 시 해당 컴포넌트를 가진 게임 오브젝트가 씬에 있는지 확인해 주세요.");
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
        if (PlayerStats.Instance == null) return;

        PlayerStats.Instance.experience += (int)amount;

        CheckForLevelUp();
    }

    /// <summary>
    /// 다음 레벨에 필요한 경험치량을 계산하여 PlayerStats에 저장합니다.
    /// </summary>
    private void CalculateRequiredExperience()
    {
        // PlayerStats.Instance를 통해 데이터에 접근합니다.
        // 등비수열 공식: 필요한 경험치 = baseExp * (expGrowthFactor ^ (level - 1))
        PlayerStats.Instance.requiredExperience = baseExp * Mathf.Pow(expGrowthFactor, PlayerStats.Instance.level - 1);
    }

    /// <summary>
    /// 경험치를 확인하고 레벨업이 가능한지 체크하는 메서드
    /// </summary>
    private void CheckForLevelUp()
    {
        if (PlayerStats.Instance == null) return;

        while (PlayerStats.Instance.experience >= PlayerStats.Instance.requiredExperience)
        {
            LevelUp();
            CalculateRequiredExperience();
            Debug.Log($"다음 레벨까지 남은 경험치: {PlayerStats.Instance.experience} / {PlayerStats.Instance.requiredExperience}");
        }
    }

    /// <summary>
    /// 플레이어를 레벨업시키는 메서드
    /// </summary>
    private void LevelUp()
    {
        if (PlayerStats.Instance == null || PlayerStatSystem.Instance == null) return;

        float remainingExp = PlayerStats.Instance.experience - PlayerStats.Instance.requiredExperience;

        // 레벨과 경험치를 업데이트합니다.
        PlayerStats.Instance.level++;
        PlayerStats.Instance.experience = (int)remainingExp;

        // 레벨업 시 스탯 포인트를 지급합니다.
        PlayerStatSystem.Instance.statPoints += 5;

        // 레벨업 시 스킬 포인트를 지급합니다.
        PlayerStats.Instance.skillPoints += 2;

        // 레벨업에 따른 스탯 증가 로직을 PlayerStatSystem에 위임합니다.
        PlayerStatSystem.Instance.UpdateFinalStats();
        PlayerStatSystem.Instance.StoreTempStats();

        Debug.Log($"축하합니다! 레벨 {PlayerStats.Instance.level}로 레벨업했습니다!");
    }
}