using UnityEngine;
using System.Collections.Generic;

// SkillPointManager는 플레이어의 스킬 포인트를 관리하는 중앙 로직 스크립트입니다.
// 스킬 포인트의 사용, 반환, 저장, 취소 등 모든 로직을 담당합니다.
public class SkillPointManager : MonoBehaviour
{
    // === 외부 참조 ===
    [Tooltip("플레이어의 최종 스탯 데이터를 담고 있는 PlayerStats 스크립트를 할당하세요.")]
    public PlayerStats playerStats;

    // === 내부 데이터 ===
    // 스킬 패널에서 임시로 사용되는 스킬 포인트. 저장/취소 시 초기화됩니다.
    private int tempSkillPoints;
    // 임시로 변경된 스킬 레벨을 저장하는 Dictionary. key: 스킬ID, value: 임시 레벨
    private Dictionary<int, int> tempSkillLevels = new Dictionary<int, int>();
    // 초기화 상태를 추적하여 Start()에서 한 번만 실행되도록 합니다.
    private bool isInitialized = false;

    // === 이벤트 ===
    // 스킬 포인트가 변경될 때 UI에 알리기 위한 이벤트입니다.
    public event System.Action<int> OnSkillPointsChanged;

    void Start()
    {
        // 게임 시작 시 초기화 메서드를 호출하여 기본 스킬 포인트를 설정합니다.
        // 이 메서드는 스킬 패널을 열 때마다 호출되도록 변경하는 것이 더 좋습니다.
        InitializePoints();
    }

    /// <summary>
    /// 스킬 포인트와 스킬 레벨 데이터를 초기화합니다.
    /// 플레이어의 최종 데이터를 임시 변수에 복사합니다.
    /// </summary>
    public void InitializePoints()
    {
        // 패널을 열 때마다 초기화 상태를 확인하고, 이미 초기화되었다면 다시 실행하지 않습니다.
        if (isInitialized)
        {
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats가 할당되지 않았습니다. 인스펙터 창에서 할당해 주세요.");
            return;
        }

        // 최종 스킬 포인트를 임시 변수에 복사
        tempSkillPoints = playerStats.skillPoints;
        // 최종 스킬 레벨 데이터를 임시 Dictionary에 복사
        // 원본 데이터를 수정하지 않기 위해 깊은 복사를 수행합니다.
        tempSkillLevels = new Dictionary<int, int>(playerStats.skillLevels);
        isInitialized = true;

        // 초기 스킬 포인트를 UI에 반영하도록 이벤트를 발생시킵니다.
        OnSkillPointsChanged?.Invoke(tempSkillPoints);

        Debug.Log("스킬 포인트 시스템이 초기화되었습니다. 현재 스킬 포인트: " + tempSkillPoints);
    }

    /// <summary>
    /// 특정 스킬의 임시 레벨을 업데이트합니다.
    /// SkillConfirmationPanel에서 레벨업/다운 시 호출됩니다.
    /// </summary>
    /// <param name="skillID">스킬의 고유 ID</param>
    /// <param name="level">새로운 임시 레벨</param>
    public void UpdateTempSkillLevel(int skillID, int level)
    {
        tempSkillLevels[skillID] = level;
    }

    /// <summary>
    /// 특정 스킬의 현재 임시 레벨을 가져옵니다.
    /// </summary>
    /// <param name="skillID">스킬의 고유 ID</param>
    /// <returns>스킬의 현재 임시 레벨. 데이터가 없으면 0을 반환합니다.</returns>
    public int GetSkillCurrentLevel(int skillID)
    {
        // Dictionary에 해당 스킬이 있는지 확인하고, 없으면 기본값인 0을 반환합니다.
        return tempSkillLevels.ContainsKey(skillID) ? tempSkillLevels[skillID] : 0;
    }

    /// <summary>
    /// 현재 보유한 임시 스킬 포인트를 반환합니다.
    /// </summary>
    /// <returns>임시 스킬 포인트</returns>
    public int GetTempSkillPoints()
    {
        return tempSkillPoints;
    }

    /// <summary>
    /// 스킬 포인트를 1 사용합니다. (임시로 감소)
    /// </summary>
    public void SpendPoint()
    {
        tempSkillPoints--;
        // UI 업데이트를 위해 이벤트를 발생시킵니다.
        OnSkillPointsChanged?.Invoke(tempSkillPoints);
        Debug.Log("스킬 포인트 1 사용! 남은 포인트: " + tempSkillPoints);
    }

    /// <summary>
    /// 스킬 포인트를 1 되돌립니다. (임시로 증가)
    /// </summary>
    public void RefundPoint()
    {
        tempSkillPoints++;
        // UI 업데이트를 위해 이벤트를 발생시킵니다.
        OnSkillPointsChanged?.Invoke(tempSkillPoints);
        Debug.Log("스킬 포인트 1 반환! 남은 포인트: " + tempSkillPoints);
    }

    /// <summary>
    /// 변경된 스킬 포인트와 스킬 레벨을 최종 값에 적용하고 초기화합니다.
    /// 이 메서드는 '저장' 버튼에 연결됩니다.
    /// </summary>
    public void ApplyChanges()
    {
        // 임시 포인트를 최종 스탯에 적용합니다.
        playerStats.skillPoints = tempSkillPoints;
        // 임시 스킬 레벨을 최종 스탯에 적용합니다.
        playerStats.skillLevels = new Dictionary<int, int>(tempSkillLevels);

        Debug.Log("스킬 포인트와 스킬 레벨이 저장되었습니다.");

        // 이 스크립트의 초기화 상태를 재설정하여 다음 패널 활성화 시 다시 초기화되도록 합니다.
        isInitialized = false;
        // UI를 최종 값으로 업데이트
        OnSkillPointsChanged?.Invoke(playerStats.skillPoints);
    }

    /// <summary>
    /// 변경된 스킬 포인트를 모두 취소하고 원래 값으로 되돌립니다.
    /// 이 메서드는 '취소' 버튼에 연결됩니다.
    /// </summary>
    public void DiscardChanges()
    {
        // 임시 포인트를 최종 스탯 값으로 되돌립니다.
        tempSkillPoints = playerStats.skillPoints;
        // 임시 스킬 레벨도 최종 스탯 값으로 되돌립니다.
        tempSkillLevels = new Dictionary<int, int>(playerStats.skillLevels);

        Debug.Log("스킬 포인트 변경이 취소되었습니다. 원래 값으로 복구.");

        // 이 스크립트의 초기화 상태를 재설정하여 다음 패널 활성화 시 다시 초기화되도록 합니다.
        isInitialized = false;
        // UI를 원래 값으로 업데이트
        OnSkillPointsChanged?.Invoke(tempSkillPoints);
    }

    /// <summary>
    /// 레벨업 등의 이유로 플레이어에게 새로운 스킬 포인트를 추가합니다.
    /// 이 메서드는 플레이어 레벨업 시스템에서 호출하는 것이 좋습니다.
    /// </summary>
    /// <param name="amount">추가할 스킬 포인트 양</param>
    public void AddPoints(int amount)
    {
        playerStats.skillPoints += amount;
        // tempSkillPoints가 활성화 상태인 경우 함께 증가시킵니다.
        if (isInitialized)
        {
            tempSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(tempSkillPoints);
        }
        Debug.Log($"스킬 포인트 {amount}개가 추가되었습니다. 현재 총 포인트: {playerStats.skillPoints}");
    }
}