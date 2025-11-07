using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

public class DungeonUIManager : MonoBehaviour
{
    public static DungeonUIManager Instance { get; private set; }

    // =======================================================
    // [핵심 추가] 위험도 UI 패널
    // =======================================================
    [Header("던전 위험도 표시")]
    [Tooltip("위험도 레벨 텍스트와 게이지 슬라이더를 포함하는 부모 패널입니다.")]
    [SerializeField] private GameObject riskPanel; // <-- [추가] 이 패널이 켜져야 화면에 보입니다.
    [Tooltip("현재 던전 위험도 레벨을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI riskLevelText;
    [Tooltip("다음 레벨까지의 진행도를 표시할 슬라이더입니다.")]
    [SerializeField] private Slider riskGaugeSlider;
    // =======================================================

    [Header("패널 활성화창")]
    [Tooltip("던전배치 시 활성화/비활성화할 패널")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject invenPanel;
    [Tooltip("상점창의 버튼입니다.")]
    [SerializeField] private Button shopButton;
    [Tooltip("인벤창의 버튼입니다.")]
    [SerializeField] private Button invenButton;

    [Header("던전 진입/퇴장 알림창")]
    [Tooltip("전체 알림창 패널입니다. 비활성화 상태로 시작합니다.")]
    [SerializeField] private GameObject alertPanel;
    [Tooltip("알림창에 표시될 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI alertText;
    [Tooltip("알림창의 확인 버튼입니다.")]
    [SerializeField] private Button confirmButton;
    [Tooltip("알림창의 취소 버튼입니다.")]
    [SerializeField] private Button cancelButton;

    [Header("던전 클리어 결과창")]
    [Tooltip("던전 클리어 시 활성화될 결과창 패널입니다.")]
    [SerializeField] private GameObject resultsPanel;
    [Tooltip("획득한 최종 점수를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Tooltip("획득한 골드를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI goldText;
    [Tooltip("획득한 경험치를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI expText;
    [Tooltip("획득한 아이템 이름을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI itemText;
    [Tooltip("획득한 던전 코인을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI coinText;
    [Tooltip("결과창을 닫는 버튼입니다.")]
    [SerializeField] private Button closeResultsButton;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 초기 패널 비활성화
        if (alertPanel != null) alertPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        // [추가] 위험도 패널 초기 비활성화
        if (riskPanel != null) riskPanel.SetActive(false);

        // 버튼 리스너 연결
        if (closeResultsButton != null)
        {
            closeResultsButton.onClick.AddListener(() => {
                resultsPanel.SetActive(false);
            });
        }
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OnShopButtonClicked);
        }
        if (invenButton != null)
        {
            invenButton.onClick.AddListener(OnInventoryButtonClicked);
        }

        // =======================================================
        // [핵심 추가] DungeonManager 이벤트 구독
        // =======================================================
        DungeonManager.OnDungeonEnter += HandleDungeonEnter;
        DungeonManager.OnDungeonExit += HandleDungeonExit;
        // =======================================================

        // 위험도 UI 초기 상태 설정
        InitializeRiskUI();
    }

    private void OnDestroy()
    {
        // =======================================================
        // [핵심 추가] 이벤트 구독 해제 (메모리 누수 방지)
        // =======================================================
        DungeonManager.OnDungeonEnter -= HandleDungeonEnter;
        DungeonManager.OnDungeonExit -= HandleDungeonExit;
        // =======================================================
    }

    /// <summary>
    /// DungeonManager.OnDungeonEnter 이벤트 발생 시 호출됩니다.
    /// 위험도 UI 패널을 활성화합니다.
    /// </summary>
    private void HandleDungeonEnter()
    {
        if (riskPanel != null)
        {
            riskPanel.SetActive(true);
            // 던전 진입 시 UI를 초기화할 수도 있습니다.
            // UpdateRiskDisplay(0, 0f); 
        }
    }

    /// <summary>
    /// DungeonManager.OnDungeonExit 이벤트 발생 시 호출됩니다.
    /// 위험도 UI 패널을 비활성화하고, 필요하면 UI를 초기화합니다.
    /// </summary>
    private void HandleDungeonExit()
    {
        if (riskPanel != null)
        {
            riskPanel.SetActive(false);
        }
        InitializeRiskUI(); // 퇴장 시 UI를 초기화 상태로 되돌립니다.
    }


    /// <summary>
    /// [추가] 위험도 UI의 초기 상태를 설정합니다. (SRP 유지: 표시 책임)
    /// </summary>
    private void InitializeRiskUI()
    {
        // UI 요소가 할당되어 있다면, 초기값을 설정합니다.
        if (riskLevelText != null)
        {
            // 초기값을 'N/A' 대신 'Lv.0'으로 설정하여 깔끔하게 보일 수도 있습니다.
            riskLevelText.text = "위험도 Lv.0";
        }
        if (riskGaugeSlider != null)
        {
            // 게이지는 0부터 시작하도록 설정합니다.
            riskGaugeSlider.minValue = 0f;
            riskGaugeSlider.maxValue = 1f; // 최대값은 1로 설정하여 비율로 사용합니다.
            riskGaugeSlider.value = 0f;
        }
    }

    /// <summary>
    /// [추가] 외부에서 현재 위험도 레벨과 게이지를 업데이트하는 공용 메서드입니다.
    /// </summary>
    /// <param name="level">현재 위험도 레벨 값</param>
    /// <param name="gaugeRatio">현재 게이지 진행 비율 (0.0f ~ 1.0f)</param>
    public void UpdateRiskDisplay(int level, float gaugeRatio)
    {
        // 레벨 텍스트 업데이트
        if (riskLevelText != null)
        {
            riskLevelText.text = $"위험도 Lv.{level}";
        }

        // 게이지 슬라이더 업데이트 (값 보간 없이 즉시 적용)
        if (riskGaugeSlider != null)
        {
            // 0.0f ~ 1.0f 범위로 클램프하여 안전하게 값을 설정합니다.
            riskGaugeSlider.value = Mathf.Clamp01(gaugeRatio);
        }
    }

    // ... (중략: OnShopButtonClicked, OnInventoryButtonClicked, ShowDungeonAlert, ShowResultsScreen 메서드는 변경 없음) ...
    // ... (편의상 중략하며 원본 코드를 유지합니다.)

    /// <summary>
    /// 상점 버튼 클릭 시 호출됩니다.
    /// 상점 패널을 토글(활성화/비활성화)하고, 인벤토리 패널은 항상 닫습니다.
    /// </summary>
    private void OnShopButtonClicked()
    {
        // MainSceneManager.Instance.PlayButtonSFXSafely(); 

        // 상점 패널이 존재할 경우 토글합니다.
        if (shopPanel != null)
        {
            bool isActive = shopPanel.activeSelf;
            shopPanel.SetActive(!isActive);
        }
    }
    /// <summary>
    /// 인벤토리 버튼 클릭 시 호출됩니다.
    /// 인벤토리 패널을 토글(활성화/비활성화)하고, 상점 패널은 항상 닫습니다.
    /// </summary>
    private void OnInventoryButtonClicked()
    {
        // MainSceneManager.Instance.PlayButtonSFXSafely(); 

        // 인벤토리 패널이 존재할 경우 토글합니다.
        if (invenPanel != null)
        {
            bool isActive = invenPanel.activeSelf;
            invenPanel.SetActive(!isActive);
        }
    }
    public void ShowDungeonAlert(string message, Action onConfirmAction)
    {
        // ... (기존 ShowDungeonAlert 로직 유지)
        if (alertPanel == null || alertText == null || confirmButton == null || cancelButton == null)
        {
            Debug.LogWarning("DungeonUIManager의 UI 요소가 모두 설정되지 않았습니다!");
            return;
        }
        alertText.text = message;
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        // confirmButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely); 
        confirmButton.onClick.AddListener(() => {
            onConfirmAction?.Invoke();
            alertPanel.SetActive(false);
        });
        // cancelButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely); 
        cancelButton.onClick.AddListener(() => {
            alertPanel.SetActive(false);
        });
        alertPanel.SetActive(true);
    }

    /// <summary>
    /// 던전 클리어 후 결과창을 활성화하고 보상 정보를 표시합니다.
    /// </summary>
    /// <param name="finalScore">획득한 최종 점수</param>
    /// <param name="gold">획득한 골드</param>
    /// <param name="exp">획득한 경험치</param>
    /// <param name="finalCoins">획득한 던전 코인</param>
    /// <param name="itemNames">획득한 아이템 이름 리스트</param>
    public void ShowResultsScreen(int finalScore, int gold, int exp, int finalCoins, List<string> itemNames)
    {
        // ... (기존 ShowResultsScreen 로직 유지)
        if (resultsPanel == null || scoreText == null || goldText == null || expText == null || itemText == null || coinText == null)
        {
            Debug.LogWarning("DungeonUIManager의 결과창 UI 요소가 모두 설정되지 않았습니다! (CoinText 확인 필요)");
            return;
        }

        scoreText.text = $"최종 점수\n{finalScore}점";
        goldText.text = $"현금\n{gold}원";
        expText.text = $"경험치\n{exp}";
        coinText.text = $"던전조각\n{finalCoins}개";
        if (itemNames.Count > 0)
        {
            itemText.text = "\n" + string.Join("\n", itemNames);
        }
        else
        {
            itemText.text = "없음";
        }
        resultsPanel.SetActive(true);
    }
}