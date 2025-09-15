using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 이 스크립트는 스킬 패널 UI의 상호작용을 관리합니다.
// 버튼 클릭에 따라 패널을 활성화/비활성화하고, 스킬 포인트 시스템과 연동하는 기능을 담당합니다.
public class SkillPanel : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [Tooltip("스킬 패널1 (예: 액티브 스킬) GameObject를 할당하세요.")]
    public GameObject panel1;
    [Tooltip("스킬 패널2 (예: 패시브 스킬) GameObject를 할당하세요.")]
    public GameObject panel2;

    [Tooltip("패널1을 활성화하는 버튼입니다.")]
    public Button button1;
    [Tooltip("패널2를 활성화하는 버튼입니다.")]
    public Button button2;

    [Header("스킬 포인트 시스템")]
    // SkillPointManager는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.
    // [Tooltip("SkillPointManager 스크립트를 할당하세요. 인스펙터에서 직접 연결하세요.")]
    // public SkillPointManager skillPointManager;
    [Tooltip("현재 스킬 포인트를 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI skillPointText;

    [Tooltip("변경 사항을 저장하는 버튼을 할당하세요.")]
    public Button saveButton;
    [Tooltip("변경 사항을 취소하는 버튼을 할당하세요.")]
    public Button cancelButton;

    // 스킬 패널의 활성화 여부를 추적하는 변수
    private bool isPanelActive = false;

    void Awake()
    {
        // SkillPointManager 싱글턴 인스턴스가 존재하는지 확인하고 이벤트 구독을 진행합니다.
        if (SkillPointManager.Instance != null)
        {
            // 스킬 포인트가 변경될 때마다 UI를 업데이트하는 이벤트에 메서드를 등록합니다.
            SkillPointManager.Instance.OnSkillPointsChanged += UpdateSkillPointUI;
        }
        else
        {
            // 인스턴스가 존재하지 않을 경우 경고 메시지를 띄웁니다.
            Debug.LogError("SkillPointManager 인스턴스가 존재하지 않습니다. 씬에 해당 컴포넌트가 있는지 확인해 주세요.");
        }

        // 저장 및 취소 버튼에 클릭 이벤트를 연결합니다.
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClick);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }
    }

    /// <summary>
    /// 이 메서드는 스킬 패널의 전체를 활성화/비활성화할 때 호출됩니다.
    /// 패널이 활성화될 때 SkillPointManager를 초기화합니다.
    /// </summary>
    /// <param name="active">패널 활성화 여부</param>
    public void SetPanelActive(bool active)
    {
        // 패널 활성화 상태를 업데이트합니다.
        isPanelActive = active;
        gameObject.SetActive(active);

        // 패널이 활성화될 때만 SkillPointManager를 초기화합니다.
        // 이렇게 하면 매번 패널을 열 때마다 임시 데이터가 초기 상태로 돌아갑니다.
        if (active && SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.InitializePoints();
        }
    }

    /// <summary>
    /// 첫 번째 스킬 패널만 활성화하고 나머지는 비활성화합니다.
    /// </summary>
    public void ShowPanel1()
    {
        // 첫 번째 패널을 활성화합니다.
        if (panel1 != null)
        {
            panel1.SetActive(true);
        }

        // 나머지 패널들을 비활성화합니다.
        if (panel2 != null)
        {
            panel2.SetActive(false);
        }
    }

    /// <summary>
    /// 두 번째 스킬 패널만 활성화하고 나머지는 비활성화합니다.
    /// </summary>
    public void ShowPanel2()
    {
        // 두 번째 패널을 활성화합니다.
        if (panel2 != null)
        {
            panel2.SetActive(true);
        }

        // 나머지 패널들을 비활성화합니다.
        if (panel1 != null)
        {
            panel1.SetActive(false);
        }
    }

    /// <summary>
    /// SkillPointManager에서 호출하는 이벤트에 등록되어 스킬 포인트를 업데이트합니다.
    /// </summary>
    /// <param name="currentPoints">현재 스킬 포인트</param>
    private void UpdateSkillPointUI(int currentPoints)
    {
        if (skillPointText != null)
        {
            skillPointText.text = $"스킬포인트: \n{currentPoints}";
        }
    }

    /// <summary>
    /// '저장' 버튼 클릭 시 호출됩니다.
    /// SkillPointManager.Instance의 ApplyChanges 메서드를 호출하여 변경 사항을 최종 적용합니다.
    /// </summary>
    private void OnSaveButtonClick()
    {
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.ApplyChanges();
        }
    }

    /// <summary>
    /// '취소' 버튼 클릭 시 호출됩니다.
    /// SkillPointManager.Instance의 DiscardChanges 메서드를 호출하여 변경 사항을 되돌립니다.
    /// </summary>
    private void OnCancelButtonClick()
    {
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.DiscardChanges();
        }
    }

    private void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.OnSkillPointsChanged -= UpdateSkillPointUI;
        }
    }
}