using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 플레이어의 스킬 사용 및 관리를 담당하는 컨트롤러 스크립트입니다.
public class PlayerSkillController : MonoBehaviour
{
    [Header("참조 스크립트")]
    [Tooltip("플레이어의 PlayerStats 컴포넌트를 할당하세요.")]
    public PlayerStats playerStats;
    [Tooltip("플레이어의 PlayerStatSystem 컴포넌트를 할당하세요.")]
    public PlayerStatSystem playerStatSystem;
    [Tooltip("스킬 포인트를 관리하는 SkillPointManager 컴포넌트를 할당하세요.")]
    public SkillPointManager skillPointManager; // <-- SkillPointManager 참조 추가

    [Header("스킬 할당")]
    [Tooltip("1~8 키에 할당할 스킬 데이터를 드래그하여 할당하세요.")]
    // 스킬 슬롯을 배열로 관리하여 확장성을 높입니다.
    public SkillData[] skillSlots = new SkillData[8];

    [Header("스킬 발사 지점")]
    [Tooltip("스킬 투사체가 발사될 위치입니다. 플레이어의 자식 오브젝트에 부착하세요.")]
    public Transform skillSpawnPoint;

    // 쿨타임을 관리하기 위한 딕셔너리
    // 키: 스킬 ID, 값: 다음 사용 가능 시간(Time.time)
    private Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();

    // 스킬 슬롯 데이터가 변경되었음을 외부에 알리는 이벤트
    public event System.Action<int, SkillData> OnSkillSlotChanged;

    // 남은 쿨타임 정보를 외부에 알리는 새로운 이벤트 (남은 시간, 최대 시간)
    public event System.Action<int, float, float> OnCooldownUpdated;

    void Start()
    {
        if (playerStats == null || skillSpawnPoint == null)
        {
            Debug.LogError("PlayerStats 또는 SkillSpawnPoint가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
        }
        // SkillPointManager의 스킬 레벨업 이벤트를 구독합니다.
        if (skillPointManager != null)
        {
            skillPointManager.OnSkillLeveledUp += OnSkillLeveledUpHandler;
        }
    }

    void Update()
    {
        // === 쿨타임 텍스트 및 슬라이더 실시간 업데이트 로직 ===
        // 현재 쿨타임 딕셔너리에 있는 모든 스킬의 남은 쿨타임을 갱신합니다.
        var skillsOnCooldown = cooldownTimers.ToList();
        foreach (var cooldownInfo in skillsOnCooldown)
        {
            float remainingCooldown = cooldownInfo.Value - Time.time;
            int slotIndex = FindSkillSlotIndex(cooldownInfo.Key);

            // 쿨타임이 끝난 경우
            if (remainingCooldown <= 0f)
            {
                // 쿨타임이 끝났음을 UI에 알리고 딕셔너리에서 제거합니다.
                if (slotIndex != -1)
                {
                    OnCooldownUpdated?.Invoke(slotIndex, 0f, 0f);
                }
                cooldownTimers.Remove(cooldownInfo.Key);
            }
            else
            {
                // 쿨타임이 남았다면 UI에 남은 시간을 알립니다.
                if (slotIndex != -1 && skillSlots[slotIndex] != null)
                {
                    int currentLevel = 0;
                    if (playerStats.skillLevels.ContainsKey(skillSlots[slotIndex].skillId))
                    {
                        currentLevel = playerStats.skillLevels[skillSlots[slotIndex].skillId];
                    }

                    if (currentLevel > 0 && currentLevel <= skillSlots[slotIndex].levelInfo.Length)
                    {
                        float maxCooldown = skillSlots[slotIndex].levelInfo[currentLevel - 1].cooldown;
                        OnCooldownUpdated?.Invoke(slotIndex, remainingCooldown, maxCooldown);
                    }
                }
            }
        }
        // ==================================

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseSkill(skillSlots[i]);
            }
        }
    }

    /// <summary>
    /// SkillPointManager의 이벤트로부터 호출되어 스킬 레벨업에 따른 UI 업데이트를 처리합니다.
    /// </summary>
    /// <param name="skillId">레벨업된 스킬의 ID</param>
    private void OnSkillLeveledUpHandler(int skillId)
    {
        int slotIndex = FindSkillSlotIndex(skillId);
        if (slotIndex != -1)
        {
            // 해당 스킬의 UI를 업데이트하도록 기존 이벤트를 다시 호출합니다.
            OnSkillSlotChanged?.Invoke(slotIndex, skillSlots[slotIndex]);
        }
    }

    /// <summary>
    /// 스킬 ID에 해당하는 스킬이 어느 슬롯에 있는지 찾습니다.
    /// </summary>
    /// <param name="skillId">찾을 스킬의 ID</param>
    /// <returns>스킬이 등록된 슬롯의 인덱스, 없으면 -1 반환</returns>
    private int FindSkillSlotIndex(int skillId)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null && skillSlots[i].skillId == skillId)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 특정 슬롯에 스킬을 등록하고 UI 업데이트 이벤트를 발생시킵니다.
    /// 패시브 스킬은 등록할 수 없습니다.
    /// </summary>
    /// <param name="slotIndex">스킬을 등록할 슬롯의 인덱스 (0~7)</param>
    /// <param name="skillToRegister">등록할 스킬 데이터</param>
    public void RegisterSkill(int slotIndex, SkillData skillToRegister)
    {
        // 등록하려는 스킬이 null이거나 패시브 스킬일 경우 등록을 거부합니다.
        if (skillToRegister == null || skillToRegister.skillType == SkillType.Passive)
        {
            Debug.LogWarning("패시브 스킬은 액티브 슬롯에 등록할 수 없습니다.");
            return;
        }

        if (slotIndex >= 0 && slotIndex < skillSlots.Length)
        {
            // 이전에 슬롯에 등록된 스킬이 있다면 해제합니다.
            if (skillSlots[slotIndex] != null)
            {
                Debug.LogWarning($"{skillSlots[slotIndex].skillName} 스킬이 {slotIndex + 1}번 슬롯에서 해제됩니다.");
            }

            skillSlots[slotIndex] = skillToRegister;
            Debug.Log($"{skillToRegister.skillName} 스킬이 {slotIndex + 1}번 슬롯에 등록되었습니다.");

            // 스킬 슬롯의 변경 사항을 외부에 알립니다.
            OnSkillSlotChanged?.Invoke(slotIndex, skillToRegister);
        }
        else
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다: " + slotIndex);
        }
    }

    /// <summary>
    /// 특정 스킬 데이터를 찾아 슬롯에서 해제하고 UI 업데이트 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="skillToUnregister">해제할 스킬 데이터</param>
    public void UnregisterSkill(SkillData skillToUnregister)
    {
        if (skillToUnregister == null)
        {
            Debug.LogWarning("해제할 스킬 데이터가 없습니다.");
            return;
        }

        // 스킬 슬롯을 순회하며 해제할 스킬을 찾습니다.
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == skillToUnregister)
            {
                skillSlots[i] = null;
                Debug.Log($"{i + 1}번 슬롯의 스킬이 해제되었습니다.");

                // 스킬 슬롯의 변경 사항을 외부에 알립니다.
                OnSkillSlotChanged?.Invoke(i, null);
                return; // 스킬을 찾아서 해제했으니 반복문을 종료합니다.
            }
        }
        Debug.LogWarning("해당 스킬이 등록된 슬롯을 찾을 수 없습니다.");
    }

    /// <summary>
    /// 지정된 스킬을 사용하는 메서드입니다.
    /// 마나와 쿨타임을 체크하여 스킬 발동 가능 여부를 판단합니다.
    /// </summary>
    /// <param name="skill">사용할 스킬의 ScriptableObject 데이터</param>
    public void UseSkill(SkillData skill)
    {
        if (skill == null)
        {
            Debug.Log("이 슬롯에는 할당된 스킬이 없습니다.");
            return;
        }

        if (skill.skillType != SkillType.Active)
        {
            Debug.Log(skill.skillName + " 스킬은 패시브 스킬입니다.");
            return;
        }

        int currentSkillLevel = 0;
        if (playerStats.skillLevels.ContainsKey(skill.skillId))
        {
            currentSkillLevel = playerStats.skillLevels[skill.skillId];
        }

        if (currentSkillLevel == 0)
        {
            Debug.Log(skill.skillName + " 스킬을 배우지 않았습니다.");
            return;
        }

        if (playerStats.level < skill.requiredLevel)
        {
            Debug.Log("플레이어 레벨이 부족하여 " + skill.skillName + " 스킬을 사용할 수 없습니다.");
            return;
        }

        if (currentSkillLevel > skill.levelInfo.Length)
        {
            Debug.LogError("스킬의 레벨 정보가 부족합니다. 스킬 데이터(" + skill.skillName + ")를 확인하세요.");
            return;
        }

        SkillLevelInfo currentLevelInfo = skill.levelInfo[currentSkillLevel - 1];

        if (playerStats.mana < currentLevelInfo.manaCost)
        {
            Debug.Log("마나가 부족합니다!");
            return;
        }

        // 쿨타임 체크 로직
        if (cooldownTimers.ContainsKey(skill.skillId) && Time.time < cooldownTimers[skill.skillId])
        {
            float remainingCooldown = cooldownTimers[skill.skillId] - Time.time;
            Debug.Log(skill.skillName + " 스킬이 아직 쿨타임입니다. 남은 시간: " + remainingCooldown.ToString("F1") + "초");
            return;
        }

        Debug.Log(skill.skillName + " (Lv." + currentSkillLevel + ") 스킬 사용!");

        playerStats.mana -= currentLevelInfo.manaCost;
        cooldownTimers[skill.skillId] = Time.time + currentLevelInfo.cooldown;

        skill.Execute(skillSpawnPoint, playerStats, currentSkillLevel);
    }
}