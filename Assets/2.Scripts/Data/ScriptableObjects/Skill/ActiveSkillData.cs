using UnityEngine;

/// <summary>
/// 모든 액티브 스킬이 공통으로 가지는 데이터를 정의합니다.
/// OCP (개방-폐쇄 원칙): 딜레이 시간 변수를 추가하여, 하위 클래스나 PlayerSkillController의 코드를 수정하지 않고도
/// 스킬 발동 타이밍을 확장할 수 있도록 합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewActiveSkillData", menuName = "Skill/New ActiveSkillData")]
public class ActiveSkillData : SkillData
{
    [Header("모션 및 캐스팅 이펙트 정보 (PlayerSkillController 사용)")]

    [Tooltip("스킬 발동 시 실행할 애니메이터 트리거 이름입니다. (예: Skill_Fireball)")]
    // 모든 액티브 스킬은 이 필드를 상속받아 고유한 모션 이름을 설정해야 합니다.
    public string animationTriggerName = "UseSkill";

    [Tooltip("스킬 발동 즉시(UseSkill 시) 재생할 시각 효과 프리팹 (예: 소용돌이 범위 표시).")]
    // 이 필드는 PlayerSkillController에서 즉시 Instantiate됩니다.
    public GameObject castingEffectPrefab;
    [Header("스킬 사운드")]
    [Tooltip("스킬 발동(시전) 시 SoundManager를 통해 재생할 SFX 타입입니다.")]
    public SFXType castSFXType = SFXType.None;
    [Header("스킬 발동 타이밍 설정")]
    [Tooltip("스킬 모션 시작 후, 실제 효과(투사체, 데미지 등)가 발동될 때까지의 지연 시간 (초)")]
    // 이 값이 코루틴에서 WaitForSeconds의 인자로 사용되어 딜레이를 발생시킵니다.
    public float activationDelay = 0.5f; // 기본값 0.5초 설정

    /// <summary>
    /// ActiveSkillData는 Execute의 구체적인 구현을 강제하지 않습니다.
    /// 구체적인 스킬 효과 로직은 FireballSkillData와 같은 하위 클래스가 담당해야 합니다.
    /// OCP 원칙: 스킬의 논리적 실행 성공 여부를 bool로 반환합니다.
    /// </summary>
    public override bool Execute(Transform spawnPoint, PlayerStats playerStats, int skillLevel) // <--- [핵심 수정] bool 반환
    {
        if (SoundManager.Instance != null && castSFXType != SFXType.None)
        {
            // SoundManager를 통해 해당 스킬의 시전 사운드를 재생합니다.
            SoundManager.Instance.PlaySFX(castSFXType);
        }
        // ActiveSkillData 자체는 실행 로직을 가지지 않습니다.
        // 모든 실제 액티브 스킬은 이 메서드를 반드시 오버라이드하여
        // 자신만의 스킬 효과 로직(투사체 발사, 광역 피해 등)을 구현해야 합니다.
        //Debug.LogError($"'{skillName}' 스킬은 Execute() 메서드가 하위 클래스에서 구현되지 않았습니다. 로직을 추가해야 합니다.");

        // 하위 클래스에서 오버라이드하지 않았다면 논리적 실행에 실패한 것으로 간주하여 false 반환
        return false;
    }
}