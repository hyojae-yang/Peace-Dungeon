using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 이 스크립트는 스킬 포인트 시스템의 핵심 로직을 관리합니다.
/// 스킬 포인트를 올리고 내리는 기능, 변경 사항을 확정하거나 취소하는 기능을 담당합니다.
/// 싱글턴 패턴을 적용하여 어디서든 접근 가능하며, 씬 전환에도 유지됩니다.
/// </summary>
public class SkillPointManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 (Singleton Instance) ===
    // S-I-N-G-L-E-T-O-N: 이 클래스의 유일한 인스턴스를 저장하는 정적(static) 변수입니다.
    private static SkillPointManager _instance;

    /// <summary>
    /// SkillPointManager의 공용 인스턴스에 접근하는 속성입니다.
    /// 이 속성을 통해 외부에서 인스턴스에 안전하게 접근할 수 있습니다.
    /// </summary>
    public static SkillPointManager Instance
    {
        get
        {
            // 인스턴스가 아직 초기화되지 않았을 경우에만 찾거나 생성합니다. (지연 초기화 - Lazy Initialization)
            if (_instance == null)
            {
                // 현재 활성화된 씬에서 SkillPointManager 컴포넌트를 가진 오브젝트를 찾습니다.
                _instance = FindFirstObjectByType<SkillPointManager>();
            }
            return _instance;
        }
    }

    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다. (의존성 관리)
    private PlayerCharacter playerCharacter;

    // === UI 및 스크립트 참조 ===
    [Header("UI 및 스크립트 참조")]
    [Tooltip("스킬 포인트를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI skillPointText;

    // 'currentSkillPoints'는 현재 플레이어가 보유한, 또는 임시로 변경 중인 스킬 포인트입니다.
    public int currentSkillPoints { get; private set; }

    // 'tempSkillLevels'는 스킬 패널에서 임시로 조작하는 스킬 레벨을 저장합니다. (원본 보호를 위한 임시 데이터)
    private Dictionary<int, int> tempSkillLevels;

    // 스킬 포인트 변경을 외부에 알리는 이벤트 (관찰자 패턴)
    public event System.Action<int> OnSkillPointsChanged;

    // 스킬 레벨이 최종 적용되어 변경되었음을 외부에 알리는 이벤트
    public event System.Action<int> OnSkillLeveledUp;


    // === 스크립트 초기화 및 생명주기 ===

    /// <summary>
    /// 스크립트가 로드될 때 호출되며, 싱글턴 인스턴스를 설정하고 씬 전환 시 파괴되지 않도록 합니다.
    /// </summary>
    void Awake()
    {
        // 씬에 이미 인스턴스가 존재하고 현재 인스턴스가 그 인스턴스가 아니라면, 중복 인스턴스를 파괴합니다.
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // 이 인스턴스를 유일한 인스턴스로 설정합니다.
            _instance = this;
        }

        // 스크립트가 시작될 때 PlayerCharacter 인스턴스를 찾아 참조를 확보합니다. (초기 의존성 확보)
        // PlayerCharacter가 Awake 단계에서 싱글턴을 설정한다고 가정합니다.
        playerCharacter = PlayerCharacter.Instance;
    }

    /// <summary>
    /// 스크립트가 시작된 후, PlayerCharacter의 데이터로 스킬 포인트를 초기화합니다.
    /// </summary>
    private void Start()
    {
        // PlayerCharacter 인스턴스를 늦게라도 확보합니다.
        if (playerCharacter == null)
        {
            playerCharacter = PlayerCharacter.Instance;
        }

        // 초기화 로직은 단일 메서드로 분리하여 중복을 방지합니다.
        InitializePoints();
    }

    /// <summary>
    /// 씬이 닫히거나 게임이 종료될 때 호출됩니다.
    /// 싱글턴 인스턴스 참조를 명시적으로 해제하여 메모리 누수 경고를 방지합니다.
    /// </summary>
    void OnDestroy()
    {
        // 현재 인스턴스가 싱글턴으로 설정된 인스턴스라면, 참조를 해제합니다.
        if (_instance == this)
        {
            // 이 부분이 직전 에러(SkillPointManagerSingleton not cleaned up)를 해결하는 핵심 로직입니다.
            _instance = null;
        }
    }

    void OnEnable()
    {
        // PlayerLevelUp 스크립트의 레벨업 이벤트를 구독합니다.
        // OCP(개방-폐쇄 원칙): 레벨업 로직은 PlayerLevelUp에 닫혀있고, 이 클래스는 이벤트에 개방되어 반응합니다.
        PlayerLevelUp.OnPlayerLeveledUp += OnLeveledUpHandler;
    }

    void OnDisable()
    {
        // 스크립트 비활성화 시 이벤트 구독을 해제합니다.
        PlayerLevelUp.OnPlayerLeveledUp -= OnLeveledUpHandler;
    }

    // ------------------------------------------------------------------

    // === 핵심 비즈니스 로직 ===

    /// <summary>
    /// 스킬 패널이 열리거나 변경 사항이 취소될 때 호출되어, 스킬 포인트를 초기화하고 임시 데이터를 설정합니다.
    /// PlayerStats의 영구 데이터를 임시 데이터로 '깊은 복사'합니다.
    /// </summary>
    public void InitializePoints()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats 인스턴스를 찾을 수 없습니다. 초기화 실패.");
            return;
        }

        // 1. 최종 스킬 포인트와 스킬 레벨을 임시 데이터로 가져와 초기화합니다.
        currentSkillPoints = playerCharacter.playerStats.skillPoints;

        // 2. 깊은 복사(Deep Copy)를 통해 원본 딕셔너리를 보호합니다. 
        //    (데이터의 독립성을 유지하여 롤백(DiscardChanges)이 가능하게 함)
        tempSkillLevels = new Dictionary<int, int>(playerCharacter.playerStats.skillLevels);

        // 3. UI 업데이트
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 변경된 스킬 레벨과 남은 스킬 포인트를 최종적으로 PlayerStats에 적용합니다.
    /// 임시 데이터(tempSkillLevels, currentSkillPoints)를 영구 데이터로 덮어씁니다.
    /// </summary>
    public void ApplyChanges()
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return;

        // 레벨업된 스킬 목록을 추적하여 이벤트를 발생시키기 위한 리스트입니다.
        List<int> leveledUpSkillIds = new List<int>();
        foreach (var tempLevelPair in tempSkillLevels)
        {
            int skillId = tempLevelPair.Key;
            int tempLevel = tempLevelPair.Value;
            // 영구 레벨을 가져오고, 없으면 0으로 간주합니다.
            int permanentLevel = playerCharacter.playerStats.skillLevels.GetValueOrDefault(skillId, 0);

            // 임시 레벨이 영구 레벨보다 높으면 (즉, 레벨업이 발생했다면)
            if (tempLevel > permanentLevel)
            {
                leveledUpSkillIds.Add(skillId);
            }
        }

        // 1. 임시 데이터를 영구 데이터로 적용합니다. (깊은 복사로 안전하게 덮어쓰기)
        playerCharacter.playerStats.skillLevels = new Dictionary<int, int>(tempSkillLevels);
        playerCharacter.playerStats.skillPoints = currentSkillPoints;

        // 2. 레벨업된 스킬에 대한 이벤트를 발생시킵니다.
        foreach (int skillId in leveledUpSkillIds)
        {
            OnSkillLeveledUp?.Invoke(skillId);
        }

        // 3. 패시브 스킬 효과 갱신 로직을 호출합니다.
        if (playerCharacter.passiveSkillManager != null)
        {
            // OCP: 이 Manager는 PassiveSkillManager의 내부 로직을 모르고, 그저 '업데이트해라'라고 요청만 합니다.
            playerCharacter.passiveSkillManager.UpdatePassiveBonuses();
        }
    }

    /// <summary>
    /// 변경 사항을 취소하고 임시 데이터를 원래의 영구 상태로 되돌립니다.
    /// </summary>
    public void DiscardChanges()
    {
        InitializePoints(); // InitializePoints가 원본 데이터를 다시 가져오므로 롤백 역할을 합니다.
    }

    // ------------------------------------------------------------------

    // === 도우미 및 유틸리티 메서드 ===

    /// <summary>
    /// 임시 스킬 포인트를 1 감소시킵니다. (스킬 레벨업 시 사용)
    /// </summary>
    public void SpendPoint()
    {
        // currentSkillPoints를 직접 수정하므로, 이 매니저의 상태(S)를 변경합니다.
        currentSkillPoints--;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 포인트를 1 증가시킵니다. (스킬 레벨다운 시 사용)
    /// </summary>
    public void RefundPoint()
    {
        currentSkillPoints++;
        UpdateSkillPointUI();
    }

    /// <summary>
    /// 임시 스킬 레벨을 딕셔너리에 업데이트합니다.
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
            tempSkillLevels[skillId] = tempLevel; // 키가 있으면 업데이트, 없으면 추가 (C# Dictionary 특징)
        }
        else
        {
            // 레벨이 0이면 딕셔너리에서 제거하여 메모리를 관리합니다.
            if (tempSkillLevels.ContainsKey(skillId))
            {
                tempSkillLevels.Remove(skillId);
            }
        }
    }

    // ------------------------------------------------------------------

    // === 조회(Query) 메서드 ===

    /// <summary>
    /// 임시 스킬 포인트를 반환합니다.
    /// </summary>
    /// <returns>현재 임시 스킬 포인트</returns>
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
            // 임시 데이터가 초기화되지 않은 경우 안전하게 초기화 시도
            InitializePoints();
        }

        // C# 7.0 이상의 GetValueOrDefault를 사용하면 더 간결하고 안전합니다.
        return tempSkillLevels.GetValueOrDefault(skillId, 0);
    }

    /// <summary>
    /// 스킬 레벨다운이 가능한지 확인합니다.
    /// (임시 레벨이 영구 레벨보다 높을 때만 레벨 다운이 가능)
    /// </summary>
    /// <param name="skillId">확인할 스킬의 ID</param>
    /// <returns>레벨 다운 가능 시 true, 아니면 false</returns>
    public bool CanLevelDown(int skillId)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null) return false;

        int tempLevel = GetTempSkillLevel(skillId);
        // 영구 레벨을 가져오고, 없으면 0으로 간주합니다.
        int permanentLevel = playerCharacter.playerStats.skillLevels.GetValueOrDefault(skillId, 0);

        // 현재 임시 레벨이 영구적으로 적용된 레벨보다 높을 때만 레벨 다운이 가능합니다.
        // 이는 레벨다운을 "이번 패널에서 올린 레벨만큼만 되돌릴 수 있다"는 규칙으로 구현한 것입니다.
        return tempLevel > permanentLevel;
    }

    /// <summary>
    /// 특정 스킬을 배울 수 있는 레벨 조건이 충족되었는지 확인합니다.
    /// (스킬 데이터 구조체가 필요합니다.)
    /// </summary>
    /// <param name="skillData">확인할 스킬 데이터</param>
    /// <returns>레벨 조건이 충족되면 true, 아니면 false</returns>
    public bool CanLearnSkill(SkillData skillData)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null || skillData == null)
        {
            Debug.LogError("PlayerStats 또는 SkillData가 유효하지 않습니다.");
            return false;
        }

        // Liskov Substitution Principle (LSP)를 준수한다고 가정: 
        // PlayerStats의 level 속성이 최소 요구 레벨(requiredLevel)과 올바르게 비교될 수 있습니다.
        return skillData.requiredLevel <= playerCharacter.playerStats.level;
    }

    // ------------------------------------------------------------------

    // === UI 및 이벤트 핸들러 ===

    /// <summary>
    /// 스킬 포인트 UI를 업데이트하고 이벤트를 발생시킵니다.
    /// </summary>
    public void UpdateSkillPointUI()
    {
        if (skillPointText != null)
        {
            skillPointText.text = $"스킬포인트: \n{currentSkillPoints}";
        }
        // 외부 구독자들에게 현재 포인트가 변경되었음을 알립니다.
        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }

    /// <summary>
    /// 레벨업 이벤트가 발생했을 때 호출될 핸들러 메서드입니다.
    /// </summary>
    private void OnLeveledUpHandler()
    {
        // 레벨업 시, PlayerStats의 데이터가 갱신되었을 것이므로, 임시 데이터도 최신 상태로 초기화합니다.
        InitializePoints();
    }
}