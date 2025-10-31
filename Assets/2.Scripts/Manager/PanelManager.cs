using UnityEngine;

/// <summary>
/// UI 패널들을 중앙에서 관리하고, 키 입력 및 외부 호출에 따라 패널의 활성화 상태를 제어하는 컨트롤러입니다.
/// </summary>
public class PanelManager : MonoBehaviour
{
    // === 패널 할당 변수 ===
    [Tooltip("관리할 패널들을 순서대로 할당하세요. 0번은 메인/배경 패널입니다.")]
    public GameObject[] panels;

    // === 패널 인덱스 및 단축키 상수 정의 (가독성 및 유지보수성 향상) ===

    // 메인/배경 패널 (P 키 토글 대상)
    private const int MAIN_PANEL_INDEX = 0;

    // 서브 패널 단축키 매핑 (논의 확정)
    private const int STATS_PANEL_INDEX = 1;    // C (능력치)
    private const int GEAR_PANEL_INDEX = 2;     // G (장비)
    private const int SKILL_PANEL_INDEX = 3;    // K (Skill)
    private const int INVENTORY_PANEL_INDEX = 4; // I (Inventory)
    private const int QUEST_PANEL_INDEX = 5;    // J (Quest)
    private const int MISC_PANEL_INDEX = 6;     // O (Options)

    // === 유니티 생명 주기 메서드 ===

    void Update()
    {
        // 1. P 키 입력 감지: 기존 기능 유지 (메인 패널 토글, 서브 패널 상태는 유지)
        if (Input.GetKeyDown(KeyCode.P))
        {
            // P 키를 눌렀을 때 메인 패널의 활성화 상태를 토글합니다.
            ToggleMainPanelOnly();
        }

        // 2. Escape 키 입력 감지: 추가 기능 (메인 패널 무조건 비활성화)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Escape 키를 눌렀을 때 메인 패널을 닫아 모든 서브 패널을 숨깁니다.
            DeactivateMainPanel();
        }

        // 3. 서브 패널 단축키 감지: 새로운 통합 토글 로직 적용
        // 단축키는 WASDE와 겹치지 않는 C, G, K, I, J, O를 사용합니다.

        if (Input.GetKeyDown(KeyCode.C))
        {
            // C 키: 능력치 패널 토글 (1번 인덱스)
            ToggleSubPanel(STATS_PANEL_INDEX);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // G 키: 장비 패널 토글 (2번 인덱스)
            ToggleSubPanel(GEAR_PANEL_INDEX);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            // K 키: 스킬 패널 토글 (3번 인덱스)
            ToggleSubPanel(SKILL_PANEL_INDEX);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            // I 키: 인벤토리 패널 토글 (4번 인덱스)
            ToggleSubPanel(INVENTORY_PANEL_INDEX);
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            // J 키: 퀘스트 패널 토글 (5번 인덱스)
            ToggleSubPanel(QUEST_PANEL_INDEX);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            // O 키: 기타 패널 토글 (6번 인덱스)
            ToggleSubPanel(MISC_PANEL_INDEX);
        }
    }

    // === 핵심 제어 메서드 ===

    /// <summary>
    /// <para>단축키를 눌렀을 때 호출되는 통합 토글 메서드입니다.</para>
    /// <para>사용자님의 의도에 따라:</para>
    /// <para>1. 닫을 때는 메인 패널(0번)만 끄고, 서브 패널의 활성화 상태는 유지합니다.</para>
    /// <para>2. 열 때는 메인 패널과 해당 서브 패널만 켜고, 다른 서브 패널은 모두 끕니다.</para>
    /// </summary>
    /// <param name="panelIndex">활성화/비활성화 상태를 제어할 서브 패널의 배열 인덱스 (1~N)</param>
    public void ToggleSubPanel(int panelIndex)
    {
        // 1. 유효성 검사
        if (!IsValidIndex(panelIndex)) return;

        bool isPanelOpen = panels[MAIN_PANEL_INDEX].activeSelf && panels[panelIndex].activeSelf;

        if (isPanelOpen)
        {
            // 2. 닫기 로직: 현재 열려있는 상태라면
            // 메인 패널만 비활성화하여 모든 UI를 숨깁니다.
            // 서브 패널 (panelIndex)의 상태는 'true'로 유지되어, P 키를 눌렀을 때 다시 나타날 수 있게 합니다.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Inventory_openclose_sound, 0.5f);
            }
            panels[MAIN_PANEL_INDEX].SetActive(false);

            // 이스터 에그: 숨기지 않았다고 삐지지 마세요. 다시 P를 누르면 나타날 거예요!
        }
        else
        {
            // 3. 열기 로직: 현재 닫혀있는 상태라면

            // 3-1. 배타적 활성화: 다른 모든 서브 패널을 비활성화합니다.
            DeactivateAllSubPanels(panelIndex);

            // 3-2. 메인 패널과 원하는 서브 패널을 동시에 활성화합니다.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Inventory_openclose_sound, 0.5f);
            }
            panels[panelIndex].SetActive(true);
            panels[MAIN_PANEL_INDEX].SetActive(true);
        }
    }

    /// <summary>
    /// <para>P 키나 특정 버튼으로 메인 패널(0번)의 활성화 상태를 토글합니다.</para>
    /// <para>이때, 서브 패널 중 활성화 상태인 것이 있으면 함께 나타나 텅 비지 않게 합니다.</para>
    /// </summary>
    public void ToggleMainPanelOnly()
    {
        // 배열 유효성 검사 (0번 인덱스만 사용하지만, 혹시 모를 상황 대비)
        if (panels.Length == 0) return;

        bool isActive = panels[MAIN_PANEL_INDEX].activeSelf;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Inventory_openclose_sound, 0.5f);
        }
        panels[MAIN_PANEL_INDEX].SetActive(!isActive);
    }

    /// <summary>
    /// <para>메인 패널 (panels[0])을 무조건 비활성화합니다. (Escape 키 기능)</para>
    /// <para>자식 오브젝트인 서브 패널들은 활성화 상태가 유지되지만, 메인 패널이 꺼져있어 화면에 보이지 않습니다.</para>
    /// </summary>
    public void DeactivateMainPanel()
    {
        if (panels.Length == 0) return;
        panels[MAIN_PANEL_INDEX].SetActive(false);
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
    /// 주어진 인덱스가 패널 배열의 유효한 범위 내에 있는지 확인하는 도우미 메서드입니다.
    /// </summary>
    /// <param name="index">확인할 인덱스</param>
    /// <returns>인덱스가 유효하면 true, 그렇지 않으면 false를 반환합니다.</returns>
    private bool IsValidIndex(int index)
    {
        return index > MAIN_PANEL_INDEX && index < panels.Length;
    }

    // === 기존 ActivatePanel은 새 로직과 충돌 가능성이 있어 비활성화하거나 용도를 재정의하는 것이 좋습니다.
    // 여기서는 기존 기능을 그대로 두어, 외부 버튼이 여전히 서브 패널만 전환할 수 있게 유지합니다.

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

        // 주의: 이 함수는 panels[0]의 상태를 건드리지 않으므로, 이 함수를 호출하기 전에 panels[0]이 켜져 있어야 합니다.
        // 이것이 토글 로직과 기존 로직의 주요 차이점입니다.
    }
}