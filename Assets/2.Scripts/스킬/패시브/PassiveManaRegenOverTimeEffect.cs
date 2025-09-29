// PassiveManaRegenOverTimeEffect.cs

using UnityEngine;
using System.Collections;
using System.Linq; // LINQ를 사용하는 경우 (현재는 사용하지 않음)

// 플레이어의 마나를 최대 마나의 비율만큼 주기적으로 회복시키는 동적 패시브 스킬입니다.
public class PassiveManaRegenOverTimeEffect : MonoBehaviour, IPassiveEffect
{
    // === 스탯 참조 및 설정 ===

    // 이 스크립트가 부착될 플레이어 게임오브젝트의 PlayerStats 컴포넌트를 참조합니다.
    private PlayerStats playerStats;

    [Header("마나 회복 설정")]
    [Tooltip("스킬 레벨에 따른 초당 마나 회복 비율(%). 각 레벨의 비율을 배열로 정의합니다.")]
    // 예: 1레벨(1%), 2레벨(2%), 3레벨(3%), 4레벨(4%), 5레벨(5%)
    public float[] manaRegenPercentagePerLevel; // [수정됨: 이름 변경]

    [Tooltip("회복 효과의 틱 주기(초). 예: 1이면 1초마다 마나 회복")]
    [SerializeField] private float tickRate = 1f;

    private Coroutine regenCoroutine;
    private int currentSkillLevel;

    private void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// PassiveSkillData에서 이 스크립트를 호출할 때 사용되며, 효과를 활성화합니다.
    /// 기존 PassiveHealOverTimeEffect의 ExecuteEffect 역할을 대신합니다.
    /// </summary>
    public void ExecuteEffect(SkillLevelInfo skillLevelInfo, PlayerStats playerStats)
    {
        // 실제 레벨을 SO가 아닌 PlayerStats나 SkillPointManager에서 가져와야 하지만,
        // 여기서는 ApplyEffect가 호출될 때 외부로부터 레벨을 받아온다고 가정하고 임시로 처리합니다.
        // 현재 스킬 레벨을 임시로 1로 설정합니다.
        currentSkillLevel = 1;
        ApplyEffect(currentSkillLevel);
    }

    // IPassiveEffect 인터페이스 구현

    /// <summary>
    /// 스킬 효과를 활성화하고, 현재 레벨에 맞춰 코루틴을 시작합니다.
    /// </summary>
    /// <param name="skillLevel">현재 스킬 레벨 (1부터 시작)</param>
    public void ApplyEffect(int skillLevel)
    {
        currentSkillLevel = skillLevel;
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        // [수정됨: 코루틴 이름 변경]
        regenCoroutine = StartCoroutine(RegenerateManaOverTime());
    }

    /// <summary>
    /// 스킬 효과를 제거하여 마나 회복 코루틴을 중지합니다.
    /// </summary>
    public void RemoveEffect()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    /// <summary>
    /// 스킬의 레벨이 변경될 때 효과를 업데이트합니다.
    /// (코루틴은 이미 'currentSkillLevel'을 참조하므로, 값을 업데이트하는 것으로 충분합니다.)
    /// </summary>
    /// <param name="newSkillLevel">변경된 스킬 레벨</param>
    public void UpdateLevel(int newSkillLevel)
    {
        currentSkillLevel = newSkillLevel;
        Debug.Log($"마나 회복 패시브 스킬 레벨업! 새로운 레벨: {currentSkillLevel}, 회복 비율: {GetRegenPercentage()}%");
    }

    /// <summary>
    /// 현재 스킬 레벨에 해당하는 마나 회복 비율을 가져오는 헬퍼 메서드입니다.
    /// </summary>
    private float GetRegenPercentage()
    {
        // [수정됨: 배열 이름 변경]
        if (currentSkillLevel > 0 && currentSkillLevel <= manaRegenPercentagePerLevel.Length)
        {
            // [수정됨: 배열 이름 변경]
            return manaRegenPercentagePerLevel[currentSkillLevel - 1];
        }
        Debug.LogError($"[ManaRegen] 유효하지 않은 스킬 레벨입니다: {currentSkillLevel}");
        return 0f;
    }

    /// <summary>
    /// 일정 시간마다 플레이어의 마나를 회복시키는 코루틴입니다.
    /// </summary>
    private IEnumerator RegenerateManaOverTime() // [수정됨: 이름 변경]
    {
        while (true)
        {
            if (playerStats != null)
            {
                float regenPercentage = GetRegenPercentage();

                // [수정됨: 마나 기준 계산]
                // MaxHealth 대신 MaxMana를 사용해야 합니다. (playerStats에 MaxMana가 있다고 가정)
                float manaAmount = playerStats.MaxMana * (regenPercentage / 100f);

                // [수정됨: 마나에 적용]
                playerStats.mana += manaAmount;

                // [수정됨: 최대 마나 보정]
                playerStats.mana = Mathf.Min(playerStats.mana, playerStats.MaxMana);
            }
            yield return new WaitForSeconds(tickRate);
        }
    }
}