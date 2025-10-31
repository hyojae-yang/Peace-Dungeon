using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// 튜토리얼의 각 단계를 정의합니다.
/// 새로운 튜토리얼 내용이 추가될 경우 이 Enum을 확장합니다. (OCP 준수)
/// </summary>
public enum TutorialStep
{
    // 0: 게임 시작 직후, 검은색 패널 페이드 아웃 연출을 담당합니다.
    IntroFade = 0,
    // 1: IntroFade 완료 후 스토리 메시지를 보여주는 단계입니다.
    StorySequence,
    // 2: 인벤토리를 열도록 유도하는 안내 패널을 띄우는 단계입니다.
    GuideOpenInventory,
    // 3: 장비 장착을 유도하는 안내 패널을 띄우는 단계입니다.
    GuideEquipGear,

    // --------------------------------------------------
    // [추가] 기본 조작법 안내
    GuideBasicAttack,            // 4. 좌클릭 기본 공격 안내
    GuideAiming,                 // 5. 우클릭 사거리 표시 및 방향 전환 안내
                                 // --------------------------------------------------

    // 6: 유저가 액자 UI를 열기 전까지 대기하는 초기 상태입니다. (기존 4단계에서 변경)
    Init,
    // 7: 액자 UI가 열렸을 때, 던전 조각을 인벤토리에서 '꺼내는' 것을 안내하는 단계입니다.
    GuideRetrievePiece,
    // 8: 유효한 위치에 던전 조각 '배치'가 완료되기를 기다리는 단계입니다.
    WaitPlacementComplete,
    GuideEnterDungeon,           // 9. 배치 완료 후 문으로 가라는 안내
    WaitDungeonEntry,            // 10. 던전 진입 대기
    GuideWaitForLevelUp,         // 11. 레벨업 대기 및 안내 (이벤트: OnLevelUpDetected)
    GuideAllocateStat,           // 12. 스탯 창 열기 및 분배 안내 (이벤트: OnStatAllocated)
    GuideOpenSkillAllocation,    // 13. 스킬 목록에서 좌클릭하여 스킬 할당 UI를 열도록 안내
    GuideApplySkillPoints,       // 14. 포인트 분배 후 적용 버튼을 누르도록 안내
    GuideRegisterSkillSlot,      // 15. 스킬을 퀵 슬롯에 등록하도록 안내 (우클릭, 슬롯 등록)

    // --------------------------------------------------
    // [최종 추가] 스킬 사용 안내
    GuideUseSkill,               // 16. 퀵 슬롯에 등록된 스킬을 실제로 사용해보도록 안내
    // --------------------------------------------------

    // 17: 튜토리얼이 성공적으로 완료되고, 자유 플레이로 넘어가는 단계입니다. 
    Complete
}

/// <summary>
/// 튜토리얼의 전체 진행 상태와 로직을 관리하는 싱글톤 컴포넌트입니다.
/// - 튜토리얼 단계 진행(AdvanceStep)의 유일한 책임을 가집니다. (SRP 준수)
/// - UI 및 이벤트 감지 책임은 UITutorialHandler에 위임합니다. (DIP 준수)
/// </summary>
public class TutorialManager : MonoBehaviour
{
    // 싱글톤 인스턴스: 어디서든 접근 가능
    public static TutorialManager Instance { get; private set; }

    [Tooltip("스토리 연출을 담당하는 StoryPanelController 컴포넌트입니다.")]
    [SerializeField] private StoryPanelController storyPanelController;

    // 이벤트 버스 및 UI 핸들러 참조
    private UITutorialHandler uiHandler;
    private TutorialStep currentStep = TutorialStep.IntroFade;

    /// <summary>
    /// 현재 튜토리얼 단계입니다. (외부에서 읽기 전용)
    /// </summary>
    public TutorialStep CurrentStep => currentStep;

    private void Awake()
    {
        // 싱글톤 초기화 로직
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // UITutorialHandler (이벤트 버스) 참조 획득
        uiHandler = UITutorialHandler.Instance;
        if (uiHandler == null)
        {
            Debug.LogError("[TutorialManager] UITutorialHandler 인스턴스를 씬에서 찾을 수 없습니다! 튜토리얼이 작동하지 않습니다.");
        }

        // StoryPanelController 의존성 검사 및 스토리 완료 이벤트 연결
        if (storyPanelController != null)
        {
            // 스토리 완료 시 AdvanceStep()을 호출하여 다음 단계로 진행합니다.
            storyPanelController.OnStoryComplete.AddListener(AdvanceStep);
            storyPanelController.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TutorialManager: Story Panel Controller가 할당되지 않았습니다. 스토리 단계를 스킵합니다.");
        }
        StartTutorial();
    }

    /// <summary>
    /// 튜토리얼을 시작합니다. (저장된 게임이 있는지 확인하여 스킵 여부 결정)
    /// </summary>
    public void StartTutorial()
    {
        bool shouldSkip = false;

        // SaveManager 인스턴스가 존재하고, IsNewGame이 false (이어하기)일 경우 스킵
        // SaveManager는 외부 시스템이므로, Null 체크를 통해 안전성을 확보합니다.
        // NOTE: 이 코드가 정상 작동하려면 SaveManager 클래스가 존재해야 합니다.
        // if (SaveManager.Instance != null && !SaveManager.Instance.IsNewGame)
        // {
        //     shouldSkip = true;
        //     // Debug.Log("[TutorialManager] 이어하기 세션이 감지되었습니다. 튜토리얼을 건너뛰고 바로 완료 단계로 진입합니다.");
        // }

        if (shouldSkip)
        {
            currentStep = TutorialStep.Complete;
            FinalizeSystemShutdown();
            return;
        }

        // '새로하기' 세션일 경우, 튜토리얼을 IntroFade 단계부터 시작합니다.
        currentStep = TutorialStep.IntroFade;
        ProcessStep(currentStep);
    }

    /// <summary>
    /// 외부 게임 이벤트(트리거)에 의해 호출되어 다음 단계로 진행합니다.
    /// 이 메서드가 튜토리얼의 유일한 진행 경로입니다.
    /// </summary>
    public void AdvanceStep()
    {
        if (currentStep == TutorialStep.Complete)
        {
            // Debug.LogWarning("[TutorialManager] 이미 완료된 튜토리얼입니다. 추가 진행 무시.");
            return;
        }

        // 다음 단계로 진행하기 전에 이전 단계에서 구독한 이벤트를 모두 해제합니다. (중복 호출 방지)
        UnsubscribeFromAllEvents();

        // 다음 단계로 전환
        currentStep = (TutorialStep)((int)currentStep + 1);

        ProcessStep(currentStep);
    }

    /// <summary>
    /// 튜토리얼을 강제로 스킵하고 종료합니다.
    /// </summary>
    public void SkipTutorial()
    {
        currentStep = TutorialStep.Complete;

        // 모든 안내 UI 제어를 UITutorialHandler에 위임하여 숨깁니다.
        uiHandler?.HideAllUI();
        storyPanelController?.gameObject.SetActive(false);

        // 모든 이벤트 구독을 해제합니다.
        UnsubscribeFromAllEvents();

        FinalizeSystemShutdown(); // 즉시 시스템 종료 호출
    }

    /// <summary>
    /// UI Handler가 완료 메시지를 숨긴 후, 최종적으로 시스템을 종료하기 위해 호출하는 메서드입니다.
    /// </summary>
    public void FinalizeSystemShutdown()
    {
        this.enabled = false;
        // TODO: (추후 추가) 게임의 모든 기능 활성화 로직
    }

    /// <summary>
    /// 던전 배치 튜토리얼 중, 유효하지 않은 위치에 조각을 놓았을 때 경고 메시지를 표시합니다.
    /// (DungeonMap.cs와 같은 던전 배치 로직에서 호출됩니다.)
    /// </summary>
    public void ShowInvalidPlacementNotification()
    {
        // 현재 단계가 유효성 검사가 필요한 'WaitPlacementComplete' 단계인지 확인합니다.
        if (currentStep != TutorialStep.WaitPlacementComplete)
        {
            return;
        }

        // 경고 메시지 표시를 UITutorialHandler에 위임
        uiHandler?.ShowInvalidPlacementNotification("유효한 던전 타일 영역에 조각을 배치해야 합니다!", 2.0f);
    }

    /// <summary>
    /// UITutorialHandler의 모든 이벤트에서 AdvanceStep 리스너를 해제합니다.
    /// </summary>
    private void UnsubscribeFromAllEvents()
    {
        if (uiHandler == null) return;

        // UI 및 조작 관련 이벤트 해제
        uiHandler.OnInventoryOpened.RemoveListener(AdvanceStep);
        uiHandler.OnGearEquipped.RemoveListener(AdvanceStep);
        uiHandler.OnBasicAttack.RemoveListener(AdvanceStep);
        uiHandler.OnAimingPerformed.RemoveListener(AdvanceStep);

        // 액자 및 던전 관련 이벤트 해제
        uiHandler.OnFrameUIOpened.RemoveListener(AdvanceStep);
        uiHandler.OnPieceRetrieved.RemoveListener(AdvanceStep);
        uiHandler.OnPlacementComplete.RemoveListener(AdvanceStep);
        uiHandler.OnDungeonPlacementUIClose.RemoveListener(AdvanceStep);
        uiHandler.OnDungeonEntryDetected.RemoveListener(AdvanceStep);

        // 레벨업 및 스킬 관련 이벤트 해제
        uiHandler.OnLevelUpDetected.RemoveListener(AdvanceStep);
        uiHandler.OnStatAllocated.RemoveListener(AdvanceStep);
        uiHandler.OnSkillAllocationOpened.RemoveListener(AdvanceStep);
        uiHandler.OnSkillPointsApplied.RemoveListener(AdvanceStep);
        uiHandler.OnSkillRegisteredToSlot.RemoveListener(AdvanceStep);
        uiHandler.OnSkillUsed.RemoveListener(AdvanceStep); // [신규 이벤트 해제]
    }

    /// <summary>
    /// 현재 단계에 따라 필요한 내부 처리를 수행하고, UI 뷰에 표시를 요청합니다.
    /// </summary>
    /// <param name="step">현재 진행된 튜토리얼 단계</param>
    private void ProcessStep(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.IntroFade:
                // 페이드 아웃 로직 처리 (UITutorialHandler가 담당할 수 있으나, 현재는 빈 로직)
                break;

            case TutorialStep.StorySequence:
                storyPanelController?.gameObject.SetActive(true);
                // 스토리 완료 이벤트는 Start()에서 이미 구독되어 있습니다.
                break;

            // ----------------- 1번 패널 활성화 단계 (메인 캔버스) -----------------
            case TutorialStep.GuideOpenInventory:
                storyPanelController?.gameObject.SetActive(false); // 스토리 패널 클린업
                uiHandler?.ShowPrimaryInstruction("G 키를 눌러서 장비창을 열어 보세요.");
                uiHandler.OnInventoryOpened.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideEquipGear:
                uiHandler?.ShowPrimaryInstruction("동검 아이콘을 우클릭하여 장비를 장착해보세요.");
                uiHandler.OnGearEquipped.AddListener(AdvanceStep);
                break;

            // [신규] 기본 공격 안내
            case TutorialStep.GuideBasicAttack:
                uiHandler?.ShowPrimaryInstruction("장비 장착을 완료했습니다. \nESC를 눌러 인벤토리를 닫고, \n마우스를 **좌클릭**하여 기본 공격을 해보세요.");
                uiHandler.OnBasicAttack.AddListener(AdvanceStep);
                break;

            // [신규] 방향 전환 및 조준 안내
            case TutorialStep.GuideAiming:
                uiHandler?.ShowPrimaryInstruction("마우스를 **우클릭**하면 플레이어가 마우스 \n방향으로 돌아서고 무기의 사거리를 보여줍니다. \n우클릭을 해보세요.");
                uiHandler.OnAimingPerformed.AddListener(AdvanceStep);
                break;

            case TutorialStep.Init: // 액자 UI 상호작용 준비 단계
                uiHandler?.ShowPrimaryInstruction("액자 앞으로 이동하여 \n상호작용 E 키를 누르세요.");
                uiHandler.OnFrameUIOpened.AddListener(AdvanceStep);
                break;

            // ----------------- 2번 패널 활성화 단계 (보조 캔버스) -----------------
            case TutorialStep.GuideRetrievePiece:
                uiHandler?.ShowSecondInstruction("좌측에 보이는 토끼조각을 \n드래그 해서 내려놓으세요.");
                uiHandler.OnPieceRetrieved.AddListener(AdvanceStep);
                break;

            case TutorialStep.WaitPlacementComplete:
                uiHandler?.ShowSecondInstruction("조각을 던전 문앞에 배치해보세요.");
                uiHandler.OnPlacementComplete.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideEnterDungeon:
                // 던전 배치 UI가 닫히기를 기다립니다. (액자 UI 닫기)
                uiHandler?.ShowSecondInstruction("배치를 완료했습니다! \n이제 ESC를 눌러 액자 UI를 닫고 문으로 가서 \n첫 던전을 탐험하세요.");
                uiHandler.OnDungeonPlacementUIClose.AddListener(AdvanceStep);
                break;

            case TutorialStep.WaitDungeonEntry:
                uiHandler?.ShowPrimaryInstruction("좌측에 있는 문으로 다가가면 \n던전입장이 가능합니다.");
                uiHandler.OnDungeonEntryDetected.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideWaitForLevelUp:
                uiHandler?.ShowPrimaryInstruction("던전에서 몬스터를 처치하여 레벨업을 해보세요.");
                uiHandler.OnLevelUpDetected.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideAllocateStat:
                uiHandler?.ShowPrimaryInstruction("C 키를 눌러 스탯 창을 열고, 획득한 스탯 포인트를 원하는 능력치에 분배하세요!분배 후 아래에 있는 적용 버튼을 눌러 최종확정시키세요!");
                uiHandler.OnStatAllocated.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideOpenSkillAllocation:
                uiHandler?.ShowPrimaryInstruction("K키를 눌러 스킬 창을 열고, 원하는 스킬을 \n'좌클릭'하여 스킬 포인트를 투자하세요.");
                uiHandler.OnSkillAllocationOpened.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideApplySkillPoints:
                uiHandler?.ShowPrimaryInstruction("'적용' 버튼을 눌러 스킬을 강화하세요.");
                uiHandler.OnSkillPointsApplied.AddListener(AdvanceStep);
                break;

            case TutorialStep.GuideRegisterSkillSlot:
                uiHandler?.ShowPrimaryInstruction("새로 배운 스킬을 우클릭해서 원하는 퀵 슬롯(1~8)에 등록하세요.");
                uiHandler.OnSkillRegisteredToSlot.AddListener(AdvanceStep);
                break;

            // [최종 추가] 스킬 사용 안내
            case TutorialStep.GuideUseSkill:
                uiHandler?.ShowPrimaryInstruction("인벤토리를 닫고 등록한 스킬을 사용해보세요.\n숫자 키 1~8이 스킬 사용 버튼입니다.");
                uiHandler.OnSkillUsed.AddListener(AdvanceStep);
                break;

            case TutorialStep.Complete: // 최종 완료
                // 최종 완료 메시지 표시 및 시스템 종료를 UITutorialHandler에 위임
                uiHandler?.ShowCompletionUI();
                break;
        }
    }
}