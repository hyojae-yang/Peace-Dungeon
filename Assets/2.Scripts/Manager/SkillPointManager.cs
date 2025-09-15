using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// 이 스크립트는 스킬 포인트 시스템의 핵심 로직을 관리합니다.
// 스킬 포인트를 올리고 내리는 기능, 변경 사항을 확정하거나 취소하는 기능을 담당합니다.
// 싱글턴 패턴을 적용하여 어디서든 접근 가능하도록 변경합니다.
public class SkillPointManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    private static SkillPointManager _instance;
    public static SkillPointManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SkillPointManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SkillPointManagerSingleton");
                    _instance = singletonObject.AddComponent<SkillPointManager>();
                    Debug.Log("새로운 'SkillPointManagerSingleton' 게임 오브젝트를 생성했습니다.");
                }
            }
            return _instance;
        }
    }

    // === 스크립트 초기화 ===
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        // 스크립트가 시작될 때 임시 스킬 레벨을 초기화
        InitializePoints();
    }

    // === UI 및 스크립트 참조 ===
    [Header("UI 및 스크립트 참조")]
    [Tooltip("스킬 포인트를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI skillPointText;

    // 이 스크립트들은 이제 싱글턴으로 접근하므로 인스펙터 할당이 필요 없습니다.
    // [Tooltip("플레이어의 PlayerStats 컴포넌트를 할당하세요.")]
    // public PlayerStats playerStats;
    // [Tooltip("패시브 스킬 효과를 업데이트할 PassiveSkillManager 컴포넌트를 할당하세요.")]
    // public PassiveSkillManager passiveSkillManager;

    // 'currentSkillPoints'는 현재 플레이어가 보유한 스킬 포인트입니다.
    private int currentSkillPoints;

    // 'tempSkillLevels'는 패널에서 임시로 조작하는 스킬 레벨을 저장합니다.
    private Dictionary<int, int> tempSkillLevels;

    // 스킬 포인트 변경을 외부에 알리는 이벤트
    public event System.Action<int> OnSkillPointsChanged;

    // 스킬 레벨이 변경되었음을 외부에 알리는 새로운 이벤트
    public event System.Action<int> OnSkillLeveledUp;

    // === 외부에서 호출되는 메서드 ===

    /// <summary>
    /// 스킬 패널이 열릴 때마다 호출되어, 스킬 포인트를 초기화하고 임시 데이터를 설정합니다.
    /// </summary>
    public void InitializePoints()
    {
        // PlayerStats.Instance를 통해 데이터에 접근
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 최종 스킬 포인트와 스킬 레벨을 임시 데이터로 가져와 초기화합니다.
        currentSkillPoints = PlayerStats.Instance.skillPoints;
        // 깊은 복사(Deep Copy)를 통해 원본 딕셔너리를 보호합니다.
        tempSkillLevels = new Dictionary<int, int>(PlayerStats.Instance.skillLevels);

        // UI 업데이트
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 포인트를 반환합니다.
    /// </summary>
    public int GetTempSkillPoints()
    {
        return currentSkillPoints;
    }

    /// <summary>
    /// 특정 스킬의 현재 임시 레벨을 가져옵니다.
    /// </summary>
    /// <param name="skillId">확인할 스킬의 ID</param>
    /// <returns>임시 레벨, 스킬이 없으면 0을 반환</returns>
    public int GetTempSkillLevel(int skillId)
    {
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

        if (tempSkillLevels.ContainsKey(skillId))
        {
            return tempSkillLevels[skillId];
        }
        return 0; // 스킬을 배우지 않았을 경우
    }

    /// <summary>
    /// 임시 스킬 포인트를 1 감소시킵니다.
    /// </summary>
    public void SpendPoint()
    {
        currentSkillPoints--;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 포인트를 1 증가시킵니다.
    /// </summary>
    public void RefundPoint()
    {
        currentSkillPoints++;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 레벨을 업데이트합니다.
    /// </summary>
    /// <param name="skillId">업데이트할 스킬 ID</param>
    /// <param name="tempLevel">업데이트할 임시 레벨</param>
    public void UpdateTempSkillLevel(int skillId, int tempLevel)
    {
        if (tempSkillLevels == null)
        {
            InitializePoints();
        }

        if (tempLevel > 0)
        {
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels[skillId] = tempLevel;
            }
            else
            {
                tempSkillLevels.Add(skillId, tempLevel);
            }
        }
        else
        {
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels.Remove(skillId);
            }
        }
    }

    /// <summary>
    /// 스킬 레벨다운이 가능한지 확인합니다.
    /// </summary>
    /// <param name="skillId">확인할 스킬의 ID</param>
    /// <returns>레벨 다운 가능 시 true, 아니면 false</returns>
    public bool CanLevelDown(int skillId)
    {
        int tempLevel = GetTempSkillLevel(skillId);
        int permanentLevel = PlayerStats.Instance.skillLevels.ContainsKey(skillId) ? PlayerStats.Instance.skillLevels[skillId] : 0;

        return tempLevel > permanentLevel;
    }

    /// <summary>
    /// 특정 스킬을 배울 수 있는 레벨 조건이 충족되었는지 확인합니다.
    /// </summary>
    /// <param name="skillData">확인할 스킬 데이터</param>
    /// <returns>레벨 조건이 충족되면 true, 아니면 false</returns>
    public bool CanLearnSkill(SkillData skillData)
    {
        if (PlayerStats.Instance == null || skillData == null)
        {
            Debug.LogError("PlayerStats 또는 SkillData가 유효하지 않습니다.");
            return false;
        }

        return skillData.requiredLevel <= PlayerStats.Instance.level;
    }

    /// <summary>
    /// 변경된 스킬 레벨을 최종적으로 적용합니다.
    /// </summary>
    public void ApplyChanges()
    {
        List<int> leveledUpSkillIds = new List<int>();
        foreach (var tempLevelPair in tempSkillLevels)
        {
            int skillId = tempLevelPair.Key;
            int tempLevel = tempLevelPair.Value;
            int permanentLevel = PlayerStats.Instance.skillLevels.ContainsKey(skillId) ? PlayerStats.Instance.skillLevels[skillId] : 0;

            if (tempLevel > permanentLevel)
            {
                leveledUpSkillIds.Add(skillId);
            }
        }

        PlayerStats.Instance.skillLevels = new Dictionary<int, int>(tempSkillLevels);
        PlayerStats.Instance.skillPoints = currentSkillPoints;

        foreach (int skillId in leveledUpSkillIds)
        {
            OnSkillLeveledUp?.Invoke(skillId);
        }

        // PassiveSkillManager가 싱글턴으로 변경되면 아래 주석을 해제하세요.
        // if (PassiveSkillManager.Instance != null)
        // {
        //     PassiveSkillManager.Instance.UpdatePassiveBonuses();
        // }

        Debug.Log("스킬 레벨 변경사항이 적용되었습니다.");
    }

    /// <summary>
    /// 변경 사항을 취소하고 원래 상태로 되돌립니다.
    /// </summary>
    public void DiscardChanges()
    {
        InitializePoints();
        Debug.Log("스킬 변경사항이 취소되었습니다.");
    }

    /// <summary>
    /// 스킬 포인트 UI를 업데이트하고 이벤트를 발생시킵니다.
    /// </summary>
    private void UpdateSkillPointUI()
    {
        if (skillPointText != null)
        {
            skillPointText.text = $"스킬포인트: \n{currentSkillPoints}";
        }
        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }
}