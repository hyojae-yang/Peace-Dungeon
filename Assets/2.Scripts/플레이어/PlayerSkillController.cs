using UnityEngine;
using System.Collections.Generic;

// 플레이어의 스킬 사용 및 관리를 담당하는 컨트롤러 스크립트입니다.
public class PlayerSkillController : MonoBehaviour
{
    [Header("참조 스크립트")]
    [Tooltip("플레이어의 PlayerStats 컴포넌트를 할당하세요.")]
    public PlayerStats playerStats;
    [Tooltip("플레이어의 PlayerStatSystem 컴포넌트를 할당하세요.")]
    public PlayerStatSystem playerStatSystem;

    [Header("스킬 할당")]
    [Tooltip("1~8 키에 할당할 스킬 데이터를 드래그하여 할당하세요.")]
    // 스킬 슬롯을 배열로 관리하여 확장성을 높입니다.
    public SkillData[] skillSlots = new SkillData[8]; // 최대 8개의 스킬 슬롯

    [Header("스킬 발사 지점")]
    [Tooltip("스킬 투사체가 발사될 위치입니다. 플레이어의 자식 오브젝트에 부착하세요.")]
    public Transform skillSpawnPoint;

    // 쿨타임을 관리하기 위한 딕셔너리
    // 스킬 데이터와 마지막 사용 시간(Time.time)을 매핑합니다.
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    void Start()
    {
        // PlayerStats와 SkillSpawnPoint가 할당되었는지 확인합니다.
        if (playerStats == null || skillSpawnPoint == null)
        {
            Debug.LogError("PlayerStats 또는 SkillSpawnPoint가 할당되지 않았습니다. 인스펙터에서 할당해 주세요.");
        }
    }

    void Update()
    {
        // 1부터 8까지의 숫자 키 입력에 따라 스킬 사용 함수를 호출합니다.
        for (int i = 0; i < skillSlots.Length; i++)
        {
            // KeyCode.Alpha1은 1번 키, KeyCode.Alpha8은 8번 키에 해당합니다.
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseSkill(skillSlots[i]);
            }
        }
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

        // 스킬이 액티브 스킬인지 확인합니다.
        if (skill.skillType != SkillType.Active)
        {
            Debug.Log(skill.skillName + " 스킬은 패시브 스킬입니다.");
            return;
        }

        // 스킬 사용에 필요한 최소 레벨을 확인합니다.
        // 현재 플레이어의 레벨은 PlayerStats에서 가져와야 합니다.
        // 만약 SkillData에 requiredLevel 변수가 있다면 그 값을 사용합니다.
        if (playerStats.level < skill.requiredLevel)
        {
            Debug.Log("플레이어 레벨이 부족하여 " + skill.skillName + " 스킬을 사용할 수 없습니다.");
            return;
        }

        // 플레이어의 현재 스킬 레벨을 가져옵니다.
        // 이 부분은 SkillPointManager와 연동될 예정입니다.
        // 현재는 임시로 1레벨로 고정합니다.
        int currentSkillLevel = 1;

        // 스킬 레벨에 맞는 정보가 있는지 확인
        if (currentSkillLevel > skill.levelInfo.Length)
        {
            Debug.LogError("스킬의 레벨 정보가 부족합니다. 스킬 데이터(" + skill.skillName + ")를 확인하세요.");
            return;
        }

        // 현재 레벨의 스킬 정보 가져오기
        SkillLevelInfo currentLevelInfo = skill.levelInfo[currentSkillLevel - 1];

        // 마나가 충분한지 확인합니다.
        if (playerStats.mana < currentLevelInfo.manaCost)
        {
            Debug.Log("마나가 부족합니다!");
            return;
        }

        // 쿨타임이 지났는지 확인합니다.
        // 스킬 ID를 사용하여 쿨타임을 관리합니다.
        if (cooldownTimers.ContainsKey(skill.skillId.ToString()) && Time.time < cooldownTimers[skill.skillId.ToString()])
        {
            float remainingCooldown = cooldownTimers[skill.skillId.ToString()] - Time.time;
            Debug.Log(skill.skillName + " 스킬이 아직 쿨타임입니다. 남은 시간: " + remainingCooldown.ToString("F1") + "초");
            return;
        }

        // 모든 조건을 통과하면 스킬 발동
        Debug.Log(skill.skillName + " 스킬 사용!");

        // 마나 소모
        playerStats.mana -= currentLevelInfo.manaCost;

        // 쿨타임 업데이트 (스킬 ID를 키로 사용)
        cooldownTimers[skill.skillId.ToString()] = Time.time + currentLevelInfo.cooldown;

        // 스킬의 고유한 효과를 발동시킵니다.
        // SkillData의 Execute 메서드를 호출하며, skillLevel 매개변수를 전달합니다.
        skill.Execute(skillSpawnPoint, playerStats, currentSkillLevel);
    }
}