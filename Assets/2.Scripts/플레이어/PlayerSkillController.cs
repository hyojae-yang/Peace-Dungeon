using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어의 스킬 사용 및 관리를 담당하는 컨트롤러 스크립트입니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
/// </summary>
public class PlayerSkillController : MonoBehaviour
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    [Header("스킬 할당")]
    [Tooltip("1~8 키에 할당할 스킬 데이터를 드래그하여 할당하세요.")]
    public SkillData[] skillSlots = new SkillData[8];

    [Header("스킬 발사 지점")]
    [Tooltip("스킬 투사체가 발사될 위치입니다. 플레이어의 자식 오브젝트에 부착하세요.")]
    public Transform skillSpawnPoint;

    private Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();
    public event System.Action<int, SkillData> OnSkillSlotChanged;
    public event System.Action<int, float, float> OnCooldownUpdated;

    void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter 인스턴스를 찾을 수 없습니다. 스크립트가 제대로 동작하지 않을 수 있습니다.");
            return;
        }

        if (skillSpawnPoint == null)
        {
            Debug.LogError("SkillSpawnPoint가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
        }

        // SkillPointManager의 스킬 레벨업 이벤트를 기존 방식대로 직접 구독합니다.
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.OnSkillLeveledUp += OnSkillLeveledUpHandler;
        }
    }

    void Update()
    {
        // === 쿨타임 텍스트 및 슬라이더 실시간 업데이트 로직 ===
        var skillsOnCooldown = cooldownTimers.ToList();
        foreach (var cooldownInfo in skillsOnCooldown)
        {
            float remainingCooldown = cooldownInfo.Value - Time.time;
            int slotIndex = FindSkillSlotIndex(cooldownInfo.Key);

            if (remainingCooldown <= 0f)
            {
                if (slotIndex != -1)
                {
                    OnCooldownUpdated?.Invoke(slotIndex, 0f, 0f);
                }
                cooldownTimers.Remove(cooldownInfo.Key);
            }
            else
            {
                if (slotIndex != -1 && skillSlots[slotIndex] != null)
                {
                    int currentLevel = 0;
                    // PlayerStats에 playerCharacter를 통해 접근합니다.
                    if (playerCharacter.playerStats != null && playerCharacter.playerStats.skillLevels.ContainsKey(skillSlots[slotIndex].skillId))
                    {
                        currentLevel = playerCharacter.playerStats.skillLevels[skillSlots[slotIndex].skillId];
                    }

                    if (currentLevel > 0 && currentLevel <= skillSlots[slotIndex].levelInfo.Length)
                    {
                        float maxCooldown = skillSlots[slotIndex].levelInfo[currentLevel - 1].cooldown;
                        OnCooldownUpdated?.Invoke(slotIndex, remainingCooldown, maxCooldown);
                    }
                }
            }
        }

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
            OnSkillSlotChanged?.Invoke(slotIndex, skillSlots[slotIndex]);
        }
    }

    /// <summary>
    /// 스킬 ID에 해당하는 스킬이 어느 슬롯에 있는지 찾습니다.
    /// </summary>
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
    /// 지정된 스킬을 사용하는 메서드입니다.
    /// </summary>
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

        // PlayerStats 인스턴스에 playerCharacter를 통해 접근
        PlayerStats playerStatsInstance = playerCharacter.playerStats;
        if (playerStatsInstance == null)
        {
            Debug.LogError("PlayerStats 인스턴스가 존재하지 않습니다.");
            return;
        }

        int currentSkillLevel = 0;
        if (playerStatsInstance.skillLevels.ContainsKey(skill.skillId))
        {
            currentSkillLevel = playerStatsInstance.skillLevels[skill.skillId];
        }

        if (currentSkillLevel == 0)
        {
            Debug.Log(skill.skillName + " 스킬을 배우지 않았습니다.");
            return;
        }

        if (playerStatsInstance.level < skill.requiredLevel)
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

        if (playerStatsInstance.mana < currentLevelInfo.manaCost)
        {
            Debug.Log("마나가 부족합니다!");
            return;
        }

        if (cooldownTimers.ContainsKey(skill.skillId) && Time.time < cooldownTimers[skill.skillId])
        {
            float remainingCooldown = cooldownTimers[skill.skillId] - Time.time;
            return;
        }

        playerStatsInstance.mana -= currentLevelInfo.manaCost;
        cooldownTimers[skill.skillId] = Time.time + currentLevelInfo.cooldown;

        skill.Execute(skillSpawnPoint, playerStatsInstance, currentSkillLevel);
    }

    /// <summary>
    /// 특정 슬롯에 스킬을 등록하고 UI 업데이트 이벤트를 발생시킵니다.
    /// 패시브 스킬은 등록할 수 없습니다.
    /// </summary>
    public void RegisterSkill(int slotIndex, SkillData skillToRegister)
    {
        // === 스킬 레벨 검사 로직 ===
        int realSkillLevel = 0;
        if (playerCharacter != null && playerCharacter.playerStats != null && playerCharacter.playerStats.skillLevels.ContainsKey(skillToRegister.skillId))
        {
            realSkillLevel = playerCharacter.playerStats.skillLevels[skillToRegister.skillId];
        }

        // 실제 스킬 레벨이 1 미만이면 등록을 거부합니다.
        if (realSkillLevel < 1)
        {
            Debug.LogWarning($"[스킬 등록 실패] '{skillToRegister.skillName}' 스킬은 레벨이 1 미만이므로 등록할 수 없습니다.");
            return;
        }

        // 패시브 스킬은 등록할 수 없습니다.
        if (skillToRegister == null || skillToRegister.skillType == SkillType.Passive)
        {
            Debug.LogWarning("패시브 스킬은 액티브 슬롯에 등록할 수 없습니다.");
            return;
        }

        if (slotIndex >= 0 && slotIndex < skillSlots.Length)
        {
            if (skillSlots[slotIndex] != null)
            {
                Debug.LogWarning($"{skillSlots[slotIndex].skillName} 스킬이 {slotIndex + 1}번 슬롯에서 해제됩니다.");
            }

            skillSlots[slotIndex] = skillToRegister;
            Debug.Log($"{skillToRegister.skillName} 스킬이 {slotIndex + 1}번 슬롯에 등록되었습니다.");

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
    public void UnregisterSkill(SkillData skillToUnregister)
    {
        if (skillToUnregister == null)
        {
            Debug.LogWarning("해제할 스킬 데이터가 없습니다.");
            return;
        }

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == skillToUnregister)
            {
                skillSlots[i] = null;
                Debug.Log($"{i + 1}번 슬롯의 스킬이 해제되었습니다.");

                OnSkillSlotChanged?.Invoke(i, null);
                return;
            }
        }
        Debug.LogWarning("해당 스킬이 등록된 슬롯을 찾을 수 없습니다.");
    }
}