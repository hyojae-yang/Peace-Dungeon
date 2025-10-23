using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임에 존재하는 모든 SkillData ScriptableObject를 관리하고,
/// 스킬 ID를 통해 빠르게 SkillData 객체를 조회할 수 있도록 돕는 중앙 데이터베이스입니다.
/// 싱글턴 패턴을 사용하여 시스템 전역에서 쉽게 접근할 수 있도록 설계되었습니다.
/// </summary>
public class SkillDatabase : MonoBehaviour
{
    // 단일 책임 원칙 (SRP)을 준수하기 위한 인스턴스
    public static SkillDatabase Instance { get; private set; }

    // 스킬 ID와 실제 SkillData 객체를 매핑하는 딕셔너리
    private Dictionary<int, SkillData> skillDataMap = new Dictionary<int, SkillData>();

    // SkillData ScriptableObject들이 저장된 Resources 폴더 내의 경로
    private const string SkillDataPath = "Skills/Active";

    private void Awake()
    {
        // 싱글턴 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환에도 유지 (선택 사항)
            LoadAllSkills(); // 모든 스킬 데이터를 미리 로드합니다.
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resources 폴더에서 모든 SkillData를 로드하고 딕셔너리에 저장합니다.
    /// 게임 시작 시 한 번만 실행됩니다.
    /// </summary>
    private void LoadAllSkills()
    {
        // Resources.LoadAll을 사용하여 지정된 경로의 모든 ScriptableObject를 로드합니다.
        SkillData[] allSkills = Resources.LoadAll<SkillData>(SkillDataPath);

        foreach (var skill in allSkills)
        {
            if (skillDataMap.ContainsKey(skill.skillId))
            {
                Debug.LogError($"[SkillDatabase] 스킬 ID 중복 발견: {skill.skillId} - {skill.skillName}. 데이터 오류를 수정해야 합니다.");
                continue;
            }
            skillDataMap.Add(skill.skillId, skill);
        }

        // 참고: 사용자는 'Resources/ScriptableObjects/Skills' 폴더를 생성하고 SkillData 파일을 저장해야 합니다.
    }

    /// <summary>
    /// 스킬 ID를 기반으로 해당 SkillData ScriptableObject를 찾아 반환합니다.
    /// OCP 및 DIP 원칙에 따라 PlayerSkillController가 데이터 조회에 의존할 수 있도록 합니다.
    /// </summary>
    /// <param name="skillId">찾고자 하는 스킬의 고유 ID</param>
    /// <returns>해당 ID의 SkillData 객체. 없으면 null을 반환합니다.</returns>
    public SkillData GetSkillData(int skillId)
    {
        if (skillDataMap.TryGetValue(skillId, out SkillData skill))
        {
            return skill;
        }
        //Debug.LogWarning($"[SkillDatabase] ID {skillId} 에 해당하는 스킬 데이터를 찾을 수 없습니다.");
        return null;
    }
}