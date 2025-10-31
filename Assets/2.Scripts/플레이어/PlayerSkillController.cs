// PlayerSkillController.cs (수정)
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어의 스킬 사용 및 관리를 담당하는 컨트롤러 스크립트입니다.
/// SRP (단일 책임 원칙): 스킬 사용 가능 여부 검사, 쿨타임 관리, 리소스(마나) 소모 및 비동기(딜레이) 흐름 제어를 책임집니다.
/// 실제 스킬 효과 발동은 SkillData의 Execute 메서드에 위임합니다.
/// </summary>
public class PlayerSkillController : MonoBehaviour, ISavable
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    [Header("스킬 할당")]
    [Tooltip("1~8 키에 할당할 스킬 데이터를 드래그하여 할당하세요.")]
    public SkillData[] skillSlots = new SkillData[8];

    [Header("스킬 발사 지점")]
    [Tooltip("스킬 투사체가 발사될 위치입니다. 플레이어의 자식 오브젝트에 부착하세요. (예: 손, 무기 끝)")]
    public Transform skillSpawnPoint;

    [Tooltip("캐스팅 및 지면 효과가 생성될 위치입니다. (주로 플레이어의 발 밑/중앙 지점)")]
    public Transform castingEffectSpawnPoint;

    // 현재 쿨타임이 진행 중인 스킬의 ID와 쿨타임이 끝나는 시점(Time.time)을 저장합니다.
    private Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();

    // 현재 실행 중인 스킬 ID를 저장하여, 중복 발동을 방지하거나 캔슬 로직에 활용할 수 있습니다.
    // 여기서는 간단히 코루틴 참조를 관리하여 중복 실행을 막습니다. (선택 사항이나 유용함)
    private Dictionary<int, Coroutine> activeSkillCoroutines = new Dictionary<int, Coroutine>();

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
            Debug.LogError("SkillSpawnPoint(투사체 발사 지점)가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
        }

        if (castingEffectSpawnPoint == null)
        {
            Debug.LogWarning("CastingEffectSpawnPoint(캐스팅/지면 효과 지점)가 할당되지 않았습니다. 이펙트가 어색하게 출력될 수 있으며, UseSkill에서 임시 위치를 사용합니다.");
        }

        // ISavable 인터페이스를 구현한 이 객체를 SaveManager에 등록합니다.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);
        }

        // SkillPointManager의 스킬 레벨업 이벤트를 구독합니다.
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.OnSkillLeveledUp += OnSkillLeveledUpHandler;
        }
    }

    void Update()
    {
        // === 쿨타임 텍스트 및 슬라이더 실시간 업데이트 로직 (기존 로직 유지) ===
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

        // 키 입력 처리 (Alpha1 ~ Alpha8)
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
    /// <param name="skillId">찾을 스킬의 고유 ID</param>
    /// <returns>스킬이 등록된 슬롯 인덱스 (0부터 시작). 없으면 -1 반환.</returns>
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
    /// 스킬 사용 요청 시, 유효성 검사, **선-소모**, **모션 발동**을 담당하고,
    /// **딜레이 및 효과 발동**은 코루틴으로 위임합니다.
    /// </summary>
    /// <param name="skill">사용할 스킬 데이터</param>
    public void UseSkill(SkillData skill)
    {
        // 1. 유효성 검사 (기존과 동일)
        if (skill == null || skill.skillType != SkillType.Active) return;

        PlayerStats playerStatsInstance = playerCharacter.playerStats;
        if (playerStatsInstance == null) return;

        int currentSkillLevel = playerStatsInstance.skillLevels.ContainsKey(skill.skillId) ? playerStatsInstance.skillLevels[skill.skillId] : 0;

        if (currentSkillLevel == 0) { Debug.Log(skill.skillName + " 스킬을 배우지 않았습니다."); return; }
        if (playerStatsInstance.level < skill.requiredLevel) { Debug.Log("플레이어 레벨이 부족하여 " + skill.skillName + " 스킬을 사용할 수 없습니다."); return; }
        if (currentSkillLevel > skill.levelInfo.Length) { Debug.LogError("스킬의 레벨 정보가 부족합니다. 스킬 데이터(" + skill.skillName + ")를 확인하세요."); return; }

        SkillLevelInfo currentLevelInfo = skill.levelInfo[currentSkillLevel - 1];

        // 2. 마나/쿨타임 확인
        if (playerStatsInstance.mana < currentLevelInfo.manaCost)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.ShowNotification("마나가 부족합니다!", NotificationType.Warning);
            return;
        }

        if (cooldownTimers.ContainsKey(skill.skillId) && Time.time < cooldownTimers[skill.skillId]) return;

        // 3. 리소스 선-소모 및 쿨타임 적용 (모션보다 먼저 비용 소모)
        playerStatsInstance.mana -= currentLevelInfo.manaCost;
        cooldownTimers[skill.skillId] = Time.time + currentLevelInfo.cooldown;

        ActiveSkillData activeSkill = skill as ActiveSkillData;

        if (activeSkill != null)
        {
            // 핵심 추가 로직: 마우스 조준이 필요한 스킬인지 확인 및 정보 주입 (ISP 원칙 준수)
            IHasAiming aimingSkill = activeSkill as IHasAiming;

            if (aimingSkill != null)
            {
                // 1. 마우스 목표 위치 계산 (Raycast 사용)
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPosition;

                // Raycast는 '지면'이나 '몬스터' 등 목표를 맞추는 데 사용합니다. LayerMask를 사용하는 것이 권장됩니다.
                // 여기서는 LayerMask 없이 모든 Collider를 대상으로 기본 Raycast를 시도합니다.
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    targetPosition = hit.point;
                }
                else
                {
                    // 아무것도 맞추지 못하면, 카메라 Z축 방향으로 임의의 먼 지점(50m)을 목표로 설정
                    targetPosition = ray.GetPoint(50f);
                }

                // 2. 파이어볼 스킬에 마우스 목표 위치 주입 (의존성 주입)
                aimingSkill.SetTargetPosition(targetPosition);
            }
            // ⭐ 추가 로직 끝

            // 4. 모션 및 캐스팅 이펙트 발동 (선-소모와 동시에 실행)
            playerCharacter.animator.SetTrigger(activeSkill.animationTriggerName);

            if (activeSkill.castingEffectPrefab != null)
            {
                Transform effectSpawn = castingEffectSpawnPoint != null ? castingEffectSpawnPoint : skillSpawnPoint;
                Destroy(Instantiate(activeSkill.castingEffectPrefab, effectSpawn.position, effectSpawn.rotation), 1.5f);
            }
            if (playerCharacter.playerController != null)
            {
                playerCharacter.playerController.canMove = false; // 움직임 비활성화
            }
            // 5. 딜레이 및 효과 발동을 코루틴에 위임
            // ProcessSkillActivation 시그니처 변경 없음 (기존 로직 유지)
            Coroutine newActivation = StartCoroutine(
                ProcessSkillActivation(activeSkill, playerStatsInstance, currentLevelInfo, currentSkillLevel)
            );

            // 중복 코루틴 실행 방지 (선택 사항이나 안정성을 위해 추가)
            if (activeSkillCoroutines.ContainsKey(activeSkill.skillId))
            {
                StopCoroutine(activeSkillCoroutines[activeSkill.skillId]);
            }
            activeSkillCoroutines[activeSkill.skillId] = newActivation;
            if (UITutorialHandler.Instance != null)
            { UITutorialHandler.Instance.OnSkillUsed.Invoke(); }
        }
    }

    /// <summary>
    /// 스킬 발동의 비동기 흐름(딜레이, 효과 발동, 롤백)을 처리하는 코루틴입니다.
    /// </summary>
    /// <param name="activeSkill">사용할 액티브 스킬 데이터</param>
    /// <param name="playerStats">플레이어의 현재 능력치</param>
    /// <param name="levelInfo">스킬 레벨에 따른 상세 정보 (비용 복구용)</param>
    /// <param name="skillLevel">현재 스킬의 레벨</param>
    private IEnumerator ProcessSkillActivation(
        ActiveSkillData activeSkill,
        PlayerStats playerStats,
        SkillLevelInfo levelInfo,
        int skillLevel)
    {
        // 1. 딜레이 대기 (모션 싱크)
        // ActiveSkillData에 설정된 activationDelay 값만큼 대기합니다.
        yield return new WaitForSeconds(activeSkill.activationDelay);

        // --- 딜레이 종료, 스킬 효과 발동 시점 ---

        // 2. 스킬 발동 처리 및 성공 여부 확인
        // ⭐ Execute 시그니처 변경 없이 그대로 호출합니다.
        bool skillSucceeded = activeSkill.Execute(skillSpawnPoint, playerStats, skillLevel);

        if (skillSucceeded)
        {
            // 스킬 발동이 논리적으로 성공했으므로, 선-소모한 리소스는 유지됩니다.
        }
        else
        {
            // 3. 롤백: 스킬 발동 실패 시, 선-소모한 리소스를 돌려줍니다. (핵심 롤백 로직)

            // 3-1. 마나 롤백
            playerStats.mana += levelInfo.manaCost;

            // 3-2. 쿨타임 롤백
            cooldownTimers.Remove(activeSkill.skillId);

            // 쿨타임 UI 업데이트를 위해 이벤트 강제 호출 (남은 쿨타임 0으로 설정)
            int slotIndex = FindSkillSlotIndex(activeSkill.skillId);
            if (slotIndex != -1)
            {
                OnCooldownUpdated?.Invoke(slotIndex, 0f, 0f);
            }

        }
        if (playerCharacter.playerController != null)
        {
            playerCharacter.playerController.canMove = true; // 움직임 재활성화
        }
        // 코루틴 완료 후 딕셔너리에서 참조 제거
        activeSkillCoroutines.Remove(activeSkill.skillId);
    }

    /// <summary>
    /// 특정 슬롯에 스킬을 등록하고 UI 업데이트 이벤트를 발생시킵니다.
    /// </summary>
    public void RegisterSkill(int slotIndex, SkillData skillToRegister)
    {
        // ... (RegisterSkill은 기존과 동일)
        int realSkillLevel = 0;
        if (playerCharacter != null && playerCharacter.playerStats != null && playerCharacter.playerStats.skillLevels.ContainsKey(skillToRegister.skillId))
        {
            realSkillLevel = playerCharacter.playerStats.skillLevels[skillToRegister.skillId];
        }

        if (realSkillLevel < 1)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.ShowNotification($"[스킬 등록 실패] '{skillToRegister.skillName}' \n스킬은 레벨이 1 미만이므로 등록할 수 없습니다.", NotificationType.Warning);
            return;
        }

        if (skillToRegister == null || skillToRegister.skillType == SkillType.Passive)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.ShowNotification("패시브 스킬은 액티브 슬롯에 등록할 수 없습니다.", NotificationType.Warning);
            return;
        }

        if (slotIndex >= 0 && slotIndex < skillSlots.Length)
        {
            if (skillSlots[slotIndex] != null)
            {
                Debug.LogWarning($"{skillSlots[slotIndex].skillName} 스킬이 {slotIndex + 1}번 슬롯에서 해제됩니다.");
            }

            skillSlots[slotIndex] = skillToRegister;

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
        // ... (UnregisterSkill은 기존과 동일)
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

                OnSkillSlotChanged?.Invoke(i, null);
                return;
            }
        }
        Debug.LogWarning("해당 스킬이 등록된 슬롯을 찾을 수 없습니다.");
    }
    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 스킬 컨트롤러의 데이터를 PlayerSkillControllerSaveData 객체로 변환하여 반환합니다.
    /// 이 메서드는 SaveManager에 의해 호출됩니다.
    /// </summary>
    /// <returns>저장 가능한 데이터 객체 (DTO)</returns>
    public object SaveData()
    {
        // 1. 스킬 슬롯 할당 정보 (SkillData -> Skill ID)
        int[] assignedSkillIds = new int[skillSlots.Length];
        for (int i = 0; i < skillSlots.Length; i++)
        {
            // ScriptableObject 대신 스킬 ID만 저장합니다. 없으면 0을 저장합니다.
            assignedSkillIds[i] = skillSlots[i] != null ? skillSlots[i].skillId : 0;
        }

        // 2. 쿨타임 정보 (Dictionary -> List)
        List<int> cooldownSkillIds = cooldownTimers.Keys.ToList();
        List<float> cooldownEndTimes = cooldownTimers.Values.ToList();

        PlayerSkillControllerSaveData data = new PlayerSkillControllerSaveData
        {
            assignedSkillIds = assignedSkillIds,
            cooldownSkillIds = cooldownSkillIds,
            cooldownEndTimes = cooldownEndTimes
        };
        return data;
    }

    /// <summary>
    /// SaveData 객체의 데이터를 현재 스킬 컨트롤러에 적용합니다.
    /// 이 메서드는 SaveManager에 의해 호출되며, SkillDatabase에 의존합니다.
    /// </summary>
    /// <param name="data">로드할 데이터가 담긴 PlayerSkillControllerSaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is PlayerSkillControllerSaveData loadedData)
        {
            // 1. 스킬 슬롯 정보 로드
            if (loadedData.assignedSkillIds != null && loadedData.assignedSkillIds.Length == skillSlots.Length)
            {
                // SkillDatabase를 사용하여 ID로부터 실제 SkillData 객체를 조회합니다.
                if (SkillDatabase.Instance == null)
                {
                    Debug.LogError("[PlayerSkillController] SkillDatabase가 초기화되지 않아 스킬 슬롯 로드를 건너뜁니다!");
                    return;
                }

                for (int i = 0; i < loadedData.assignedSkillIds.Length; i++)
                {
                    int skillId = loadedData.assignedSkillIds[i];
                    if (skillId > 0)
                    {
                        // DIP (의존성 역전 원칙): 직접 파일 로드를 하는 대신 관리자 클래스에 위임합니다.
                        SkillData skillToAssign = SkillDatabase.Instance.GetSkillData(skillId);

                        if (skillToAssign != null)
                        {
                            // RegisterSkill을 통해 슬롯에 등록하고 UI 업데이트 이벤트를 발생시킵니다.
                            RegisterSkill(i, skillToAssign);
                        }
                        else
                        {
                            skillSlots[i] = null; // 데이터를 찾지 못하면 슬롯을 비웁니다.
                            OnSkillSlotChanged?.Invoke(i, null);
                            Debug.LogWarning($"[PlayerSkillController] 스킬 ID {skillId}의 데이터를 찾을 수 없어 슬롯 {i + 1} 등록 실패.");
                        }
                    }
                    else
                    {
                        skillSlots[i] = null;
                        OnSkillSlotChanged?.Invoke(i, null);
                    }
                }
            }

            // 2. 쿨타임 정보 로드
            cooldownTimers.Clear();
            if (loadedData.cooldownSkillIds != null && loadedData.cooldownEndTimes != null)
            {
                // 현재 시간을 기준으로 과거에 끝난 쿨타임은 제외하고, 남은 쿨타임을 보정합니다.
                float currentTime = Time.time;

                for (int i = 0; i < loadedData.cooldownSkillIds.Count; i++)
                {
                    int skillId = loadedData.cooldownSkillIds[i];
                    float endTime = loadedData.cooldownEndTimes[i];

                    // endTime이 currentTime보다 커야 아직 쿨타임이 남은 것입니다.
                    if (endTime > currentTime)
                    {
                        cooldownTimers.Add(skillId, endTime);
                        // 쿨타임 UI 업데이트를 위해 이벤트 호출
                        int slotIndex = FindSkillSlotIndex(skillId);
                        if (slotIndex != -1 && skillSlots[slotIndex] != null)
                        {
                            // 최대 쿨타임은 SkillData에서 직접 가져와야 함 (로드가 선행되어야 함)
                            int currentLevel = playerCharacter.playerStats.skillLevels.ContainsKey(skillId) ? playerCharacter.playerStats.skillLevels[skillId] : 0;
                            if (currentLevel > 0 && currentLevel <= skillSlots[slotIndex].levelInfo.Length)
                            {
                                float maxCooldown = skillSlots[slotIndex].levelInfo[currentLevel - 1].cooldown;
                                // 남은 시간: endTime - currentTime
                                OnCooldownUpdated?.Invoke(slotIndex, endTime - currentTime, maxCooldown);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // 이 경고는 다른 스크립트 데이터가 로드될 때 발생합니다. 정상적인 동작입니다.
            //Debug.LogWarning("로드된 데이터 타입이 PlayerSkillControllerSaveData와 일치하지 않습니다. (다른 스크립트 데이터입니다.)");
        }
    }
}