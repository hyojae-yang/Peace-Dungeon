using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

public class DungeonUIManager : MonoBehaviour
{
    public static DungeonUIManager Instance { get; private set; }

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
    [SerializeField] private TextMeshProUGUI scoreText; // 최종 점수를 표시할 새로운 UI 요소
    [Tooltip("획득한 골드를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI goldText;
    [Tooltip("획득한 경험치를 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI expText;
    [Tooltip("획득한 아이템 이름을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI itemText;
    [Tooltip("획득한 던전 코인을 표시할 텍스트입니다.")]
    [SerializeField] private TextMeshProUGUI coinText; // [추가] 던전 코인 텍스트
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

        if (alertPanel != null) alertPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

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
    }
    /// <summary>
    /// 상점 버튼 클릭 시 호출됩니다.
    /// 상점 패널을 토글(활성화/비활성화)하고, 인벤토리 패널은 항상 닫습니다.
    /// </summary>
    private void OnShopButtonClicked()
    {
        MainSceneManager.Instance.PlayButtonSFXSafely(); 

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
         MainSceneManager.Instance.PlayButtonSFXSafely(); 

        // 인벤토리 패널이 존재할 경우 토글합니다.
        if (invenPanel != null)
        {
            bool isActive = invenPanel.activeSelf;
            invenPanel.SetActive(!isActive);
        }
    }
    public void ShowDungeonAlert(string message, Action onConfirmAction)
    {
        if (alertPanel == null || alertText == null || confirmButton == null || cancelButton == null)
        {
            Debug.LogWarning("DungeonUIManager의 UI 요소가 모두 설정되지 않았습니다!");
            return;
        }
        alertText.text = message;
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
        confirmButton.onClick.AddListener(() => {
            onConfirmAction?.Invoke();
            alertPanel.SetActive(false);
        });
        cancelButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
        cancelButton.onClick.AddListener(() => {
            alertPanel.SetActive(false);
        });
        alertPanel.SetActive(true);
    }

    /// 던전 클리어 후 결과창을 활성화하고 보상 정보를 표시합니다.
    /// </summary>
    /// <param name="finalScore">획득한 최종 점수</param>
    /// <param name="gold">획득한 골드</param>
    /// <param name="exp">획득한 경험치</param>
    /// <param name="finalCoins">획득한 던전 코인</param> // [추가]
    /// <param name="itemNames">획득한 아이템 이름 리스트</param>
    public void ShowResultsScreen(int finalScore, int gold, int exp, int finalCoins, List<string> itemNames) // [수정된 시그니처]
    {
        // [수정] null 체크에 coinText를 추가합니다.
        if (resultsPanel == null || scoreText == null || goldText == null || expText == null || itemText == null || coinText == null)
        {
            Debug.LogWarning("DungeonUIManager의 결과창 UI 요소가 모두 설정되지 않았습니다! (CoinText 확인 필요)");
            return;
        }

        scoreText.text = $"최종 점수\n{finalScore}"; // 최종 점수 텍스트 업데이트
        goldText.text = $"골드\n{gold}";
        expText.text = $"경험치\n{exp}";
        // [추가] 던전 코인 텍스트 업데이트
        coinText.text = $"던전코인\n{finalCoins}";
        if (itemNames.Count > 0)
        {
            itemText.text = "획득한 아이템:\n" + string.Join("\n", itemNames);
        }
        else
        {
            itemText.text = "획득한 아이템: 없음";
        }
        resultsPanel.SetActive(true);
    }
}