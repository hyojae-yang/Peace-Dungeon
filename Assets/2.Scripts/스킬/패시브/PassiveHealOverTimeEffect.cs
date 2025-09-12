using UnityEngine;
using System.Collections;

// 플레이어의 체력을 최대 체력의 비율만큼 주기적으로 회복시키는 동적 패시브 스킬입니다.
// IPassiveEffect 인터페이스를 구현합니다.
public class PassiveHealOverTimeEffect : MonoBehaviour, IPassiveEffect
{
    // === 스탯 참조 및 설정 ===
    [Header("참조")]
    // 이 스크립트가 부착될 플레이어 게임오브젝트의 PlayerStats 컴포넌트를 참조합니다.
    private PlayerStats playerStats;

    [Header("설정")]
    [Tooltip("스킬 레벨에 따른 초당 체력 회복 비율(%). 각 레벨의 비율을 배열로 정의합니다.")]
    // 예: 1레벨(1%), 2레벨(2%), 3레벨(3%), 4레벨(4%), 5레벨(5%)
    public float[] healPercentagePerLevel;

    [Tooltip("회복 효과의 틱 주기(초). 예: 1이면 1초마다 체력 회복")]
    [SerializeField] private float tickRate = 1f;

    private Coroutine healCoroutine; // 코루틴을 제어하기 위한 변수
    private int currentSkillLevel; // 현재 스킬 레벨을 저장하는 변수

    private void Awake()
    {
        // 부모 게임오브젝트(플레이어)에서 PlayerStats 컴포넌트를 찾습니다.
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트를 찾을 수 없습니다. 이 스크립트는 PlayerStats가 있는 오브젝트에 부착되어야 합니다.");
        }
    }

    // IPassiveEffect 인터페이스를 구현하는 핵심 메서드입니다.
    // PassiveSkillData.cs에서 이 스크립트를 호출할 때 사용됩니다.
    // 기존의 코루틴 로직을 실행하도록 변경했습니다.
    public void ExecuteEffect(SkillLevelInfo skillLevelInfo, PlayerStats playerStats)
    {
        // 이 시스템에서는 skillLevelInfo를 사용하지 않으므로,
        // 현재 스킬 레벨을 임시로 1로 설정합니다.
        // 이 값은 추후 SkillPointManager에 따라 변경될 수 있습니다.
        currentSkillLevel = 1;

        // ApplyEffect를 호출하여 실제 체력 회복 코루틴을 시작합니다.
        ApplyEffect(currentSkillLevel);
    }

    /// <summary>
    /// 스킬 효과를 활성화하고, 현재 레벨에 맞춰 코루틴을 시작합니다.
    /// </summary>
    /// <param name="skillLevel">현재 스킬 레벨 (1부터 시작)</param>
    public void ApplyEffect(int skillLevel)
    {
        // 첫 적용 시, 레벨을 저장하고 코루틴을 시작합니다.
        currentSkillLevel = skillLevel;
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
        }
        healCoroutine = StartCoroutine(HealOverTime());
        Debug.Log($"체력 회복 패시브 스킬 발동! 레벨: {skillLevel}, 회복 비율: {GetHealPercentage()}%");
    }

    /// <summary>
    /// 스킬 효과를 제거하여 체력 회복 코루틴을 중지합니다.
    /// </summary>
    public void RemoveEffect()
    {
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null; // 코루틴 참조를 null로 설정
        }
    }

    /// <summary>
    /// 스킬의 레벨이 변경될 때 효과를 업데이트합니다.
    /// </summary>
    /// <param name="newSkillLevel">변경된 스킬 레벨</param>
    public void UpdateLevel(int newSkillLevel)
    {
        currentSkillLevel = newSkillLevel;
        Debug.Log($"체력 회복 패시브 스킬 레벨업! 새로운 레벨: {currentSkillLevel}, 회복 비율: {GetHealPercentage()}%");
    }

    /// <summary>
    /// 현재 스킬 레벨에 해당하는 체력 회복 비율을 가져오는 헬퍼 메서드입니다.
    /// </summary>
    /// <returns>현재 스킬 레벨의 체력 회복 비율</returns>
    private float GetHealPercentage()
    {
        if (currentSkillLevel > 0 && currentSkillLevel <= healPercentagePerLevel.Length)
        {
            return healPercentagePerLevel[currentSkillLevel - 1];
        }
        Debug.LogError($"유효하지 않은 스킬 레벨입니다: {currentSkillLevel}");
        return 0f;
    }

    /// <summary>
    /// 일정 시간마다 플레이어의 체력을 회복시키는 코루틴입니다.
    /// </summary>
    private IEnumerator HealOverTime()
    {
        while (true)
        {
            // playerStats가 null이 아닌지 항상 확인하여 안전하게 코드를 실행합니다.
            if (playerStats != null)
            {
                // 현재 레벨에 해당하는 회복 비율을 가져와서 계산합니다.
                float healPercentage = GetHealPercentage();
                // 사용자님의 PlayerStats 스크립트의 변수명을 사용합니다.
                float healAmount = playerStats.MaxHealth * (healPercentage / 100f);

                // 현재 체력에 회복량을 더합니다.
                // 사용자님의 PlayerStats 스크립트의 변수명을 사용합니다.
                playerStats.health += healAmount;

                // 체력이 최대 체력을 초과하지 않도록 보정합니다.
                // 사용자님의 PlayerStats 스크립트의 변수명을 사용합니다.
                playerStats.health = Mathf.Min(playerStats.health, playerStats.MaxHealth);

                // 디버그용 로그
                Debug.Log($"체력 {healAmount:F2} 회복. 현재 체력: {playerStats.health:F2}");
            }
            // 다음 틱까지 대기합니다.
            yield return new WaitForSeconds(tickRate);
        }
    }
}