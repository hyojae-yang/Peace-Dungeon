using UnityEngine;
using System.Collections.Generic; // Dictionary 사용을 위해 추가되었습니다.

/// <summary>
/// UI 패널들을 중앙에서 관리하고, 키 입력 및 외부 호출에 따라 패널의 활성화 상태를 제어하는 컨트롤러입니다.
/// SOLID 원칙 중 OCP(개방-폐쇄 원칙)를 고려하여 키 매핑을 Dictionary로 분리했습니다.
/// </summary>
public class PanelManager : MonoBehaviour
{
    // === 패널 할당 변수 ===
    [Tooltip("관리할 패널들을 순서대로 할당하세요. 0번은 메인/배경 패널입니다.")]
    public GameObject[] panels;

    // === 패널 인덱스 및 단축키 상수 정의 (가독성 및 유지보수성 향상) ===
    // 이 상수는 panels 배열 내에서의 인덱스를 의미합니다.

    private const int MAIN_PANEL_INDEX = 0;      // P, Escape 키 토글 대상
    private const int STATS_PANEL_INDEX = 1;     // C (능력치)
    private const int GEAR_PANEL_INDEX = 2;      // G (장비)
    private const int SKILL_PANEL_INDEX = 3;     // R (Skill) - 변경됨
    private const int INVENTORY_PANEL_INDEX = 4; // Tab (Inventory) - 변경됨
    private const int QUEST_PANEL_INDEX = 5;     // Q (Quest) - 변경됨
    private const int MISC_PANEL_INDEX = 6;      // O (Options)

    // OCP 적용: 키 입력과 패널 인덱스 연결을 관리하는 Dictionary
    private Dictionary<KeyCode, int> _panelKeyMap;

    // === 유니티 생명 주기 메서드 ===

    void Awake()
    {
        // 런타임에 키 매핑을 초기화합니다.
        InitializePanelKeyMap();
    }

    /// <summary>
    /// 새로운 키 배치를 적용하여 패널 키 매핑 딕셔너리를 초기화합니다.
    /// 이 부분만 수정하면 Update() 메서드를 건드리지 않고 키 배치를 변경할 수 있습니다. (OCP 준수)
    /// </summary>
    private void InitializePanelKeyMap()
    {
        // 사용자 요청에 따라 인벤토리(Tab), 퀘스트(Q), 스킬(R)로 변경하고 나머지는 유지합니다.
        _panelKeyMap = new Dictionary<KeyCode, int>
        {
            // WASD 주변의 접근성이 좋은 키로 변경
            { KeyCode.Tab, INVENTORY_PANEL_INDEX }, // 인벤토리
            { KeyCode.Q, QUEST_PANEL_INDEX },       // 퀘스트
            { KeyCode.R, SKILL_PANEL_INDEX },       // 스킬

            // 기존 키 유지
            { KeyCode.C, STATS_PANEL_INDEX },     // 능력치
            { KeyCode.G, GEAR_PANEL_INDEX },      // 장비
            { KeyCode.O, MISC_PANEL_INDEX }       // 기타/옵션
        };

        // Q, R 키는 캐릭터 이동/상호작용 키(WASD, E)와 가깝지만,
        // 키를 눌렀을 때만 패널이 토글되므로 게임 플레이에 지장이 적습니다.
    }

    void Update()
    {
        // 2. Escape 키 입력 감지: 추가 기능 (메인 패널 무조건 비활성화)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Escape 키를 눌렀을 때 메인 패널을 닫아 모든 서브 패널을 숨깁니다.
            DeactivateMainPanel();
        }

        // 3. 서브 패널 단축키 감지: Dictionary 순회 로직 적용
        // 키보드 입력이 들어왔는지 효율적으로 검사합니다.
        foreach (var pair in _panelKeyMap)
        {
            // 현재 프레임에서 해당 키(pair.Key)가 눌렸는지 확인합니다.
            if (Input.GetKeyDown(pair.Key))
            {
                // 해당 패널 인덱스(pair.Value)를 사용하여 토글합니다.
                ToggleSubPanel(pair.Value);
                // 하나의 키만 처리하고 Update 루프를 종료하여 불필요한 검사를 줄입니다.
                break;
            }
        }
    }

    // === 핵심 제어 메서드 ===

    /// <summary>
    /// <para>단축키를 눌렀을 때 호출되는 통합 토글 메서드입니다.</para>
    /// <para>1. 닫을 때는 메인 패널(0번)만 끄고, 서브 패널의 상태는 유지합니다.</para>
    /// <para>2. 열 때는 메인 패널과 해당 서브 패널만 켜고, 다른 서브 패널은 모두 끕니다.</para>
    /// </summary>
    /// <param name="panelIndex">활성화/비활성화 상태를 제어할 서브 패널의 배열 인덱스 (1~N)</param>
    public void ToggleSubPanel(int panelIndex)
    {
        // 1. 유효성 검사 (MAIN_PANEL_INDEX 0번은 이 함수에서 직접 다루지 않습니다)
        if (!IsValidIndex(panelIndex)) return;

        // 현재 메인 패널이 켜져 있고, 토글하려는 서브 패널도 켜져 있는지 확인
        bool isPanelOpen = panels[MAIN_PANEL_INDEX].activeSelf && panels[panelIndex].activeSelf;

        if (isPanelOpen)
        {
            // 2. 닫기 로직: 현재 열려있는 상태라면 메인 패널만 비활성화합니다.
            panels[MAIN_PANEL_INDEX].SetActive(false);
            //SetTimeScale(true); // 시간 재개
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Inventory_openclose_sound, 0.5f);
            }
        }
        else
        {
            // 3. 열기 로직: 현재 닫혀있는 상태라면

            // 3-1. 배타적 활성화: 다른 모든 서브 패널을 비활성화합니다.
            DeactivateAllSubPanels(panelIndex);

            // 3-2. 메인 패널과 원하는 서브 패널을 동시에 활성화합니다.
            panels[panelIndex].SetActive(true);
            panels[MAIN_PANEL_INDEX].SetActive(true);
            //SetTimeScale(false); // 시간 멈춤
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Inventory_openclose_sound, 0.5f);
            }
        }
    }

    /// <summary>
    /// <para>메인 패널 (panels[0])을 무조건 비활성화합니다. (Escape 키 기능)</para>
    /// </summary>
    public void DeactivateMainPanel()
    {
        if (panels.Length == 0) return;
        panels[MAIN_PANEL_INDEX].SetActive(false);
        //SetTimeScale(true);
    }

    // === 보조 도우미 메서드 ===

    /// <summary>
    /// 지정된 인덱스를 제외한 모든 서브 패널 (인덱스 1 이상)을 비활성화합니다.
    /// (배타적 활성화 보장)
    /// </summary>
    /// <param name="panelToKeepActive">활성 상태를 유지할 패널의 인덱스</param>
    private void DeactivateAllSubPanels(int panelToKeepActive)
    {
        // 인덱스 1부터 순회하여 서브 패널만 확인
        for (int i = MAIN_PANEL_INDEX + 1; i < panels.Length; i++)
        {
            // 활성 상태를 유지할 패널만 건너뛰고 나머지는 모두 비활성화합니다.
            if (i != panelToKeepActive)
            {
                panels[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 주어진 인덱스가 서브 패널 배열의 유효한 범위 내에 있는지 확인하는 도우미 메서드입니다.
    /// </summary>
    /// <param name="index">확인할 인덱스</param>
    /// <returns>인덱스가 유효하면 true, 그렇지 않으면 false를 반환합니다.</returns>
    private bool IsValidIndex(int index)
    {
        // 0번(메인)보다 커야 하고, 배열 길이보다는 작아야 합니다.
        return index > MAIN_PANEL_INDEX && index < panels.Length;
    }

    /// <summary>
    /// [기존 기능 유지] 지정된 인덱스의 패널만 활성화하고 나머지 패널은 비활성화합니다.
    /// 이 메서드는 UI 버튼의 OnClick() 이벤트에 연결하여 사용합니다. (panels[0]은 제외)
    /// </summary>
    /// <param name="panelIndex">활성화할 패널의 배열 인덱스</param>
    public void ActivatePanel(int panelIndex)
    {
        // 배열 범위 유효성 검사
        if (panelIndex < MAIN_PANEL_INDEX + 1 || panelIndex >= panels.Length)
        {
            Debug.LogError("잘못된 패널 인덱스입니다: " + panelIndex);
            return;
        }

        // 모든 서브 패널을 순회하며 활성화 상태를 조정합니다.
        for (int i = MAIN_PANEL_INDEX + 1; i < panels.Length; i++)
        {
            // 선택된 인덱스의 패널만 활성화합니다.
            panels[i].SetActive(i == panelIndex);
        }

        // 이 함수는 panels[0]의 상태를 변경하지 않으므로, 호출 전 panels[0]이 켜져 있어야 합니다.
    }

    /// <summary>
    /// 게임 시간의 흐름을 제어합니다. (Time.timeScale 변경)
    /// (SOLID: 단일 책임 원칙)
    /// </summary>
    /// <param name="shouldResume">true면 시간 흐름 재개 (Time.timeScale = 1f), false면 시간 멈춤 (Time.timeScale = 0f)</param>
    private void SetTimeScale(bool shouldResume)
    {
        Time.timeScale = shouldResume ? 1f : 0f;
    }
}