using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events; // UnityAction을 명시적으로 사용하기 위해 추가

/// <summary>
/// NPC와 관련된 UI를 관리하는 싱글턴 클래스입니다.
/// 다른 스크립트(NPCInteraction, NPCQuestHandler)의 요청에 따라 UI를 표시/숨깁니다.
/// SOLID 원칙 준수:
/// 1. 단일 책임 원칙 (SRP): 오직 NPC UI의 표시/숨기기 및 데이터 바인딩만 담당합니다.
/// 2. 개방-폐쇄 원칙 (OCP): 새로운 UI 패널이 추가되어도 기존 UI 표시 로직(HideAllUI)을 건드리지 않고,
///    새로운 Show*Panel* 메서드만 추가하면 됩니다.
/// </summary>
public class NPCUIManager : MonoBehaviour
{
    // 싱글턴 인스턴스를 저장하는 정적 프로퍼티입니다.
    // 외부에서 읽기 전용으로 접근 가능합니다.
    public static NPCUIManager Instance { get; private set; }

    // ====================================================================================
    // [Header("UI Panels")]
    // 모든 UI 패널 GameObject 참조
    // ====================================================================================

    [Header("UI Panels")]
    [Tooltip("전체 대화/상호작용의 기본 컨테이너 패널 (가장 최상위)")]
    public GameObject dialoguePanel;
    [Tooltip("메인 상호작용 버튼들을 담는 패널 (예: 대화하기, 퀘스트, 상점)")]
    public GameObject mainButtonsPanel;
    [Tooltip("퀘스트 수락 여부를 묻는 패널")]
    public GameObject questAcceptPanel;
    [Tooltip("진행 중인 퀘스트를 포기할지 묻는 패널")]
    public GameObject questCancelPanel;
    [Tooltip("NPC가 제공하는 퀘스트 목록을 보여주는 패널 (현재 스크립트에서는 숨김/표시만 담당)")]
    public GameObject questListPanel;
    [Tooltip("퀘스트 완료 시 보상을 보여주는 패널")]
    public GameObject questRewardPanel;

    // ====================================================================================
    // [Header("UI Elements")]
    // 텍스트 및 버튼 컴포넌트 참조
    // ====================================================================================

    [Header("UI Elements")]
    [Tooltip("NPC의 이름을 표시하는 텍스트 컴포넌트")]
    public TextMeshProUGUI npcNameText;
    [Tooltip("NPC가 하는 말을 표시하는 텍스트 컴포넌트")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("대화 진행을 위한 '대화하기' 버튼")]
    public Button dialogueButton;
    [Tooltip("퀘스트 목록 또는 퀘스트 수락/완료를 위한 '퀘스트' 버튼")]
    public Button questButton;
    [Tooltip("긴 대화를 다음 페이지로 넘기는 버튼")]
    public Button nextButton;
    [Tooltip("상점, 대장간, 은행 등 NPC의 특수 기능을 실행하는 버튼")]
    public Button specialButton;
    [Tooltip("특수 버튼의 기능을 표시하는 텍스트")]
    public TextMeshProUGUI specialButtonText;

    // ====================================================================================
    // [Header("Quest Panels Buttons")]
    // 퀘스트 관련 패널의 버튼 참조
    // ====================================================================================

    [Header("Quest Panels Buttons")]
    [Tooltip("퀘스트 수락 패널의 '수락' 버튼입니다. (클릭 시 퀘스트 수락 로직 호출)")]
    public Button acceptQuestButton;
    [Tooltip("퀘스트 수락 패널의 '거절' 버튼입니다. (클릭 시 상호작용 종료)")]
    public Button rejectQuestButton;
    [Tooltip("퀘스트 취소 패널의 '확인' 버튼입니다. (클릭 시 퀘스트 취소 로직 호출)")]
    public Button confirmCancelButton;
    [Tooltip("퀘스트 취소 패널의 '취소' 버튼입니다. (클릭 시 상호작용 종료)")]
    public Button cancelQuestButton;
    [Tooltip("퀘스트 보상 패널을 닫고 상호작용을 종료하는 '확인' 버튼입니다.")]
    public Button rewardPanelConfirmButton;

    // ====================================================================================
    // [Header("Quest Reward UI")]
    // 퀘스트 보상 정보 표시용 텍스트 참조
    // ====================================================================================

    [Header("Quest Reward UI")]
    [Tooltip("퀘스트 완료 보상 아이템 목록을 표시하는 텍스트")]
    public TextMeshProUGUI rewardItemNameText;
    [Tooltip("퀘스트 완료 보상 경험치를 표시하는 텍스트")]
    public TextMeshProUGUI rewardExpText;
    [Tooltip("퀘스트 완료 보상 골드를 표시하는 텍스트")]
    public TextMeshProUGUI rewardGoldText;

    /// <summary>
    /// 싱글턴 인스턴스를 초기화합니다.
    /// 이 객체가 로드될 때 한 번만 호출됩니다.
    /// </summary>
    private void Awake()
    {
        // 인스턴스가 없는 경우, 현재 객체를 인스턴스로 설정합니다.
        if (Instance == null)
        {
            Instance = this;
        }
        // 이미 인스턴스가 존재하는 경우, 중복된 객체를 파괴하여 싱글턴을 유지합니다.
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 시작 시 모든 NPC UI를 초기 상태(숨김)로 설정합니다.
    /// </summary>
    private void Start()
    {
        HideAllUI();
    }

    //----------------------------------------------------------------------------------------------------------------
    // UI 표시/숨기기 (핵심 기능)
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 기본 대화 패널을 표시하고 NPC 이름과 대화 내용을 설정합니다.
    /// 다른 모든 특수 패널(버튼, 퀘스트 등)은 숨깁니다.
    /// </summary>
    /// <param name="npcName">표시할 NPC의 이름</param>
    /// <param name="dialogue">표시할 대화 내용</param>
    public void ShowDialoguePanel(string npcName, string dialogue)
    {
        // 대화 패널 활성화
        dialoguePanel.SetActive(true);

        // 다른 모든 특수 패널 비활성화 (단일 UI 상태 유지를 위함)
        mainButtonsPanel.SetActive(false);
        questAcceptPanel.SetActive(false);
        questCancelPanel.SetActive(false);
        questListPanel.SetActive(false);
        questRewardPanel.SetActive(false);

        // 텍스트 업데이트
        if (npcNameText != null) npcNameText.text = npcName;
        if (dialogueText != null) dialogueText.text = dialogue;
    }

    /// <summary>
    /// NPC와의 대화가 끝난 후, 플레이어가 선택할 수 있는 메인 버튼(대화/퀘스트/특수 기능) 패널을 표시합니다.
    /// </summary>
    /// <param name="npc">현재 상호작용 중인 NPC 객체 (퀘스트 및 특수 기능 여부 확인용)</param>
    public void ShowMainButtons(NPC npc)
    {
        // 대화 패널은 유지한 채, 메인 버튼 패널 활성화
        dialoguePanel.SetActive(true);
        mainButtonsPanel.SetActive(true);

        // 다른 모든 특수 패널 비활성화
        questAcceptPanel.SetActive(false);
        questCancelPanel.SetActive(false);
        questListPanel.SetActive(false);
        questRewardPanel.SetActive(false);

        // 퀘스트 버튼 활성화 여부 결정: NPC가 퀘스트를 제공하는지 확인합니다.
        bool hasQuests = npc != null && npc.QuestGiver != null && npc.QuestGiver.GetQuestDatas().Count > 0;
        if (questButton != null)
        {
            questButton.gameObject.SetActive(hasQuests);
        }

        // 특수 버튼(상점/대장간 등) 활성화 및 텍스트 설정
        if (specialButton != null && specialButtonText != null)
        {
            SetSpecialButton(npc);
        }
    }

    /// <summary>
    /// 퀘스트 수락 여부를 묻는 패널을 표시하고, 수락/거절 버튼에 리스너를 바인딩합니다.
    /// 버튼 클릭 로직은 NPCQuestHandler와 NPCInteraction으로 위임됩니다. (SOLID: 의존성 역전 원칙/LSP)
    /// </summary>
    /// <param name="data">수락할 퀘스트 데이터</param>
    /// <param name="handler">퀘스트 수락 로직을 가진 핸들러</param>
    /// <param name="interaction">상호작용 종료 로직을 가진 인터랙션 컴포넌트</param>
    public void ShowQuestAcceptPanel(QuestData data, NPCQuestHandler handler, NPCInteraction interaction)
    {

        questAcceptPanel.SetActive(true);

        // 다른 패널 비활성화
        mainButtonsPanel.SetActive(false);
        questRewardPanel.SetActive(false);
        questCancelPanel.SetActive(false); // 수락과 취소 패널은 동시에 보일 필요가 없습니다.

        if (acceptQuestButton != null && rejectQuestButton != null)
        {
            // 리스너 중복 추가 방지
            acceptQuestButton.onClick.RemoveAllListeners();
            rejectQuestButton.onClick.RemoveAllListeners();

            // '수락' 버튼: 효과음 재생 -> 핸들러의 퀘스트 수락 로직 호출
            acceptQuestButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            acceptQuestButton.onClick.AddListener(() => handler.OnAcceptQuest(data));

            // '거절' 버튼: 효과음 재생 -> 상호작용 종료 (NPCInteraction이 종료 책임을 가짐)
            rejectQuestButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            rejectQuestButton.onClick.AddListener(interaction.EndInteraction);
        }
    }

    /// <summary>
    /// 퀘스트 포기(취소) 여부를 묻는 패널을 표시하고, 확인/취소 버튼에 리스너를 바인딩합니다.
    /// 버튼 클릭 로직은 NPCQuestHandler와 NPCInteraction으로 위임됩니다.
    /// </summary>
    /// <param name="data">취소할 퀘스트 데이터</param>
    /// <param name="handler">퀘스트 취소 로직을 가진 핸들러</param>
    /// <param name="interaction">상호작용 종료 로직을 가진 인터랙션 컴포넌트</param>
    public void ShowQuestCancelPanel(QuestData data, NPCQuestHandler handler, NPCInteraction interaction)
    {

        questCancelPanel.SetActive(true);

        // 다른 패널 비활성화
        mainButtonsPanel.SetActive(false);
        questRewardPanel.SetActive(false);
        questAcceptPanel.SetActive(false); // 수락과 취소 패널은 동시에 보일 필요가 없습니다.

        if (confirmCancelButton != null && cancelQuestButton != null)
        {
            // 리스너 중복 추가 방지
            confirmCancelButton.onClick.RemoveAllListeners();
            cancelQuestButton.onClick.RemoveAllListeners();

            // '확인' 버튼: 효과음 재생 -> 핸들러의 퀘스트 취소 로직 호출
            confirmCancelButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            confirmCancelButton.onClick.AddListener(() => handler.OnCancelQuest(data));

            // '취소' 버튼: 효과음 재생 -> 상호작용 종료
            cancelQuestButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            cancelQuestButton.onClick.AddListener(interaction.EndInteraction);
        }
    }

    /// <summary>
    /// 퀘스트 완료 보상 패널을 표시합니다. (경험치, 골드, 아이템 정보 표시)
    /// </summary>
    /// <param name="data">퀘스트 데이터(보상 정보 포함)</param>
    /// <param name="interaction">상호작용 종료를 위한 NPCInteraction 컴포넌트</param>
    public void ShowQuestRewardPanel(QuestData data, NPCInteraction interaction)
    {
        // 모든 UI를 숨긴 후, 보상 패널만 활성화
        HideAllUI();
        questRewardPanel.SetActive(true);

        // 보상 텍스트 업데이트
        UpdateRewardTexts(data);

        if (rewardPanelConfirmButton != null)
        {
            // 리스너 중복 추가 방지
            rewardPanelConfirmButton.onClick.RemoveAllListeners();
            // '확인' 버튼: 효과음 재생 -> 상호작용 종료
            rewardPanelConfirmButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            rewardPanelConfirmButton.onClick.AddListener(() => interaction.EndInteraction());
        }
    }

    /// <summary>
    /// 모든 UI 패널을 숨기고 상호작용 상태를 초기화합니다.
    /// NPC와의 상호작용이 끝날 때 주로 호출됩니다.
    /// </summary>
    public void HideAllUI()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (questAcceptPanel != null) questAcceptPanel.SetActive(false);
        if (questCancelPanel != null) questCancelPanel.SetActive(false);
        if (questListPanel != null) questListPanel.SetActive(false);
        if (questRewardPanel != null) questRewardPanel.SetActive(false);

        // 여기에 다른 UI 패널이 추가되더라도 이 메서드만 수정하면 됩니다. (SRP/OCP)
    }

    /// <summary>
    /// NPC가 가진 특수 기능(상점, 대장간 등)에 따라 '특수 버튼'을 설정합니다.
    /// NPC의 특수 기능 목록은 INPCFunction 인터페이스를 통해 가져옵니다. (SOLID: 인터페이스 분리 원칙/DIP)
    /// </summary>
    /// <param name="npc">현재 상호작용 중인 NPC 컴포넌트</param>
    public void SetSpecialButton(NPC npc)
    {
        // 1. NPC에게 특수 기능 목록을 요청합니다.
        // NPC는 실제로 기능을 구현하는 컴포넌트(예: ShopKeeper.cs)를 통해 List<INPCFunction>을 반환합니다.
        List<INPCFunction> functions = npc.GetSpecialFunctions();

        // 2. 특수 기능이 하나라도 존재하는지 확인합니다.
        if (functions != null && functions.Count > 0)
        {
            // 3. 기능이 있다면 버튼을 활성화하고, 첫 번째 기능의 이름으로 텍스트를 설정합니다.
            // (현재는 여러 기능 중 첫 번째 기능만 UI에 표시하도록 로직을 단순화했습니다.)
            specialButton.gameObject.SetActive(true);
            specialButtonText.text = functions[0].FunctionButtonName;

            // TODO: 추후 여러 기능이 있을 경우, 팝업 메뉴를 띄우는 로직으로 확장될 수 있습니다.
        }
        else
        {
            // 4. 기능이 없다면 버튼을 비활성화합니다.
            specialButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 퀘스트 보상 패널의 경험치, 골드, 아이템 텍스트를 최신 정보로 업데이트합니다.
    /// </summary>
    /// <param name="data">퀘스트 데이터(보상 정보 포함)</param>
    private void UpdateRewardTexts(QuestData data)
    {
        // 보상 아이템 정보 업데이트
        if (rewardItemNameText != null)
        {
            if (data.rewardItems.Count > 0)
            {
                string itemString = "";
                for (int i = 0; i < data.rewardItems.Count; i++)
                {
                    // ItemSO가 null인지 확인하여 안전성을 확보합니다.
                    if (data.rewardItems[i].itemSO != null)
                    {
                        string itemName = data.rewardItems[i].itemSO.itemName;
                        // 아이템 개수가 0 초과일 경우에만 개수를 표시합니다.
                        itemString += data.rewardItems[i].itemCount > 0 ? $"{itemName} ({data.rewardItems[i].itemCount}개)" : itemName;
                    }
                    else
                    {
                        itemString += "유효하지 않은 아이템"; // 데이터 오류 시 표시
                    }

                    // 마지막 아이템이 아니라면 쉼표를 추가합니다.
                    if (i < data.rewardItems.Count - 1)
                    {
                        itemString += ", ";
                    }
                }
                rewardItemNameText.text = $"보상 아이템: {itemString}";
            }
            else
            {
                rewardItemNameText.text = "보상 아이템: 없음";
            }
        }

        // 경험치 보상 업데이트
        if (rewardExpText != null)
        {
            rewardExpText.text = data.experienceReward > 0 ? $"보상 경험치: +{data.experienceReward}" : "보상 경험치: 없음";
        }

        // 골드 보상 업데이트
        if (rewardGoldText != null)
        {
            rewardGoldText.text = data.goldReward > 0 ? $"보상 골드: +{data.goldReward}" : "보상 골드: 없음";
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 버튼 이벤트 리스너 추가/제거 (UI 관리와 로직 연결)
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// '대화하기' 버튼에 클릭 이벤트를 추가하고 기존 리스너를 모두 제거합니다.
    /// </summary>
    /// <param name="action">버튼 클릭 시 실행할 메서드</param>
    public void AddDialogueButtonListener(UnityAction action)
    {
        if (dialogueButton != null)
        {
            dialogueButton.onClick.RemoveAllListeners(); // 기존 리스너 제거 (중복 실행 방지)
            dialogueButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely); // 효과음 재생 리스너 추가
            dialogueButton.onClick.AddListener(action); // 새 리스너 추가
        }
    }

    /// <summary>
    /// '퀘스트' 버튼에 클릭 이벤트를 추가하고 기존 리스너를 모두 제거합니다.
    /// </summary>
    /// <param name="action">버튼 클릭 시 실행할 메서드</param>
    public void AddQuestButtonListener(UnityAction action)
    {
        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            questButton.onClick.AddListener(action);
        }
    }

    /// <summary>
    /// '다음' 버튼에 클릭 이벤트를 추가하고 기존 리스너를 모두 제거합니다.
    /// (긴 대화 텍스트의 페이지 넘김 등에 사용됩니다.)
    /// </summary>
    /// <param name="action">버튼 클릭 시 실행할 메서드</param>
    public void AddNextButtonListener(UnityAction action)
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(action);
        }
    }

    /// <summary>
    /// '다음' 버튼의 활성화 상태를 토글합니다.
    /// </summary>
    /// <param name="active">활성화 여부</param>
    public void ToggleNextButton(bool active)
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(active);
        }
    }
}