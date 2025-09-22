using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

public class DungeonUIManager : MonoBehaviour
{
    public static DungeonUIManager Instance { get; private set; }

    [Header("���� ����/���� �˸�â")]
    [Tooltip("��ü �˸�â �г��Դϴ�. ��Ȱ��ȭ ���·� �����մϴ�.")]
    [SerializeField] private GameObject alertPanel;
    [Tooltip("�˸�â�� ǥ�õ� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI alertText;
    [Tooltip("�˸�â�� Ȯ�� ��ư�Դϴ�.")]
    [SerializeField] private Button confirmButton;
    [Tooltip("�˸�â�� ��� ��ư�Դϴ�.")]
    [SerializeField] private Button cancelButton;

    [Header("���� Ŭ���� ���â")]
    [Tooltip("���� Ŭ���� �� Ȱ��ȭ�� ���â �г��Դϴ�.")]
    [SerializeField] private GameObject resultsPanel;
    [Tooltip("ȹ���� ���� ������ ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI scoreText; // ���� ������ ǥ���� ���ο� UI ���
    [Tooltip("ȹ���� ��带 ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI goldText;
    [Tooltip("ȹ���� ����ġ�� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI expText;
    [Tooltip("ȹ���� ������ �̸��� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI itemText;
    [Tooltip("���â�� �ݴ� ��ư�Դϴ�.")]
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
            Debug.LogWarning("DungeonUIManager�� UI ��Ұ� ��� �������� �ʾҽ��ϴ�!");
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
    /// ���� Ŭ���� �� ���â�� Ȱ��ȭ�ϰ� ���� ������ ǥ���մϴ�.
    /// </summary>
    /// <param name="finalScore">ȹ���� ���� ����</param>
    /// <param name="gold">ȹ���� ���</param>
    /// <param name="exp">ȹ���� ����ġ</param>
    /// <param name="itemNames">ȹ���� ������ �̸� ����Ʈ</param>
    public void ShowResultsScreen(int finalScore, int gold, int exp, List<string> itemNames)
    {
        if (resultsPanel == null || scoreText == null || goldText == null || expText == null || itemText == null)
        {
            Debug.LogWarning("DungeonUIManager�� ���â UI ��Ұ� ��� �������� �ʾҽ��ϴ�!");
            return;
        }

        scoreText.text = $"���� ����: {finalScore}"; // ���� ���� �ؽ�Ʈ ������Ʈ
        goldText.text = $"ȹ���� ���: {gold}";
        expText.text = $"ȹ���� ����ġ: {exp}";

        if (itemNames.Count > 0)
        {
            itemText.text = "ȹ���� ������:\n" + string.Join("\n", itemNames);
        }
        else
        {
            itemText.text = "ȹ���� ������: ����";
        }
        resultsPanel.SetActive(true);
    }
}