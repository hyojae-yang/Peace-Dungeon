using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

public class DungeonUIManager : MonoBehaviour
{
    public static DungeonUIManager Instance { get; private set; }

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
        confirmButton.onClick.AddListener(() => {
            onConfirmAction?.Invoke();
            alertPanel.SetActive(false);
        });
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
    /// <param name="itemNames">획득한 아이템 이름 리스트</param>
    public void ShowResultsScreen(int finalScore, int gold, int exp, List<string> itemNames)
    {
        if (resultsPanel == null || scoreText == null || goldText == null || expText == null || itemText == null)
        {
            Debug.LogWarning("DungeonUIManager의 결과창 UI 요소가 모두 설정되지 않았습니다!");
            return;
        }

        scoreText.text = $"최종 점수: {finalScore}"; // 최종 점수 텍스트 업데이트
        goldText.text = $"획득한 골드: {gold}";
        expText.text = $"획득한 경험치: {exp}";

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