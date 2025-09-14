using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // ���� ǥ���� ����� ���� �߰��մϴ�.

// �� ��ũ��Ʈ�� ��ų ������/�����ٿ��� Ȯ���ϴ� UI �г��� �����մϴ�.
// ��ų �� ������ ǥ���ϰ�, �ӽ� ��ų ������ �����ϴ� ����� ����մϴ�.
public class SkillConfirmationPanel : MonoBehaviour
{
    // === �ܺ� ���� (�ν����Ϳ��� �Ҵ�) ===
    [Header("UI ������Ʈ")]
    [Tooltip("��ų �̸��� ǥ���� Text ������Ʈ")]
    public TextMeshProUGUI skillNameText;
    [Tooltip("��ų ������ ǥ���� Text ������Ʈ")]
    public TextMeshProUGUI skillLevelText;
    [Tooltip("��ų�� �ɷ�ġ�� ǥ���� Text ������Ʈ")]
    public TextMeshProUGUI skillStatText;

    [Header("��ư ������Ʈ")]
    [Tooltip("��ų ������ �ø��� ��ư")]
    public Button levelUpButton;
    [Tooltip("��ų ������ ������ ��ư")]
    public Button levelDownButton;
    [Tooltip("�г��� �ݴ� ��ư")]
    public Button closeButton;

    // === ���� ������ ===
    [Header("������ ����")]
    [Tooltip("���� �г��� �ٷ�� ��ų ������")]
    private SkillData currentSkillData;
    [Tooltip("���� �г��� �����ִ� ��ų�� �ӽ� ����")]
    private int tempLevel;

    // === �ܺ� ��ũ��Ʈ ���� ===
    [Header("�Ŵ��� ����")]
    [Tooltip("��ų ����Ʈ ������ �����ϴ� SkillPointManager ��ũ��Ʈ")]
    public SkillPointManager skillPointManager;

    void Awake()
    {
        // ��ư Ŭ�� �̺�Ʈ�� �����մϴ�.
        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(OnLevelUpButtonClick);
        }
        if (levelDownButton != null)
        {
            levelDownButton.onClick.AddListener(OnLevelDownButtonClick);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }
    }

    /// <summary>
    /// ��ų Ȯ�� �г��� Ȱ��ȭ�ϰ� �����͸� �ʱ�ȭ�մϴ�.
    /// �� �޼���� SkillIcon.cs ��ũ��Ʈ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="data">ǥ���� ��ų ������</param>
    public void ShowPanel(SkillData data)
    {
        // �г� Ȱ��ȭ
        gameObject.SetActive(true);

        // ���� ��ų ������ ����
        currentSkillData = data;

        // SkillPointManager���� ���� ��ų�� ���� ������ ������ �ӽ� ������ �ʱ�ȭ�մϴ�.
        tempLevel = skillPointManager.GetTempSkillLevel(currentSkillData.skillId);

        // UI ������Ʈ
        UpdatePanelUI();
    }

    /// <summary>
    /// UI �ؽ�Ʈ���� ���� �ӽ� ������ ���� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdatePanelUI()
    {
        if (currentSkillData == null) return;

        skillNameText.text = currentSkillData.skillName;

        // ��ų ������ ��ȿ�� ���� ���� �ִ��� Ȯ���մϴ�.
        if (tempLevel >= 0 && tempLevel <= currentSkillData.levelInfo.Length)
        {
            SkillLevelInfo currentLevelInfo = null;

            if (tempLevel == 0)
            {
                skillLevelText.text = "Lv. 0 (�̽���)";
                // ���� ����(1����)�� �ɷ�ġ�� �̸� �����ݴϴ�.
                if (currentSkillData.levelInfo.Length > 0)
                {
                    currentLevelInfo = currentSkillData.levelInfo[0];
                }
            }
            else
            {
                skillLevelText.text = $"Lv. {tempLevel}";
                currentLevelInfo = currentSkillData.levelInfo[tempLevel - 1];
            }

            // ��ų �ɷ�ġ �ؽ�Ʈ�� �������� �����մϴ�.
            if (!string.IsNullOrEmpty(currentSkillData.statFormatString) && currentLevelInfo != null)
            {
                // ���� Ÿ�԰� ���� ������ ��ųʸ� ����
                Dictionary<StatType, float> statValues = new Dictionary<StatType, float>();

                // ���� ������ ��� ������ ��ųʸ��� �����մϴ�.
                foreach (var stat in currentLevelInfo.stats)
                {
                    statValues[stat.statType] = stat.value;
                }

                // ���� ǥ������ ����Ͽ� ���ø��� {�����̸�}�� ã�Ƽ� ������ ��ü�մϴ�.
                string formattedText = Regex.Replace(currentSkillData.statFormatString, @"\{(\w+)\}", match =>
                {
                    string statName = match.Groups[1].Value;
                    StatType statType;

                    // StatType ���������� ��ȯ ���� ���� Ȯ��
                    if (System.Enum.TryParse(statName, out statType) && statValues.ContainsKey(statType))
                    {
                        // �ش� ������ ���� �Ҽ��� 2�ڸ��� �����Ͽ� ��ȯ
                        return statValues[statType].ToString("F2");
                    }
                    else
                    {
                        // �ش��ϴ� ������ ������ N/A�� ��ȯ
                        return "N/A";
                    }
                });

                skillStatText.text = formattedText;
            }
            else
            {
                // statFormatString�� ������ �⺻ ���� ǥ��
                skillStatText.text = currentSkillData.skillDescription;
            }
        }
        else
        {
            Debug.LogWarning("��ų ������ ��ȿ�� ������ ������ϴ�.");
            skillStatText.text = "��ų ���� �ҷ����� ����.";
        }

        // ��ư Ȱ��ȭ/��Ȱ��ȭ ���¸� ������Ʈ�մϴ�.
        UpdateButtonStates();
    }

    /// <summary>
    /// ��ų ������ ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// ��ų ����Ʈ�� ����Ͽ� �ӽ� ������ �ø��ϴ�.
    /// </summary>
    private void OnLevelUpButtonClick()
    {
        // ��ų ����Ʈ�� ����ϰ�, �ִ� ������ �������� �ʾ��� ���� ������ ����
        if (skillPointManager.GetTempSkillPoints() > 0 && tempLevel < currentSkillData.levelInfo.Length)
        {
            // ��ų ����Ʈ ��� (�ӽ� ����)
            skillPointManager.SpendPoint();
            // ��ų �ӽ� ���� ����
            tempLevel++;
            // ��ų ���� ���� ������ SkillPointManager�� ����
            skillPointManager.UpdateTempSkillLevel(currentSkillData.skillId, tempLevel);
            // UI ������Ʈ
            UpdatePanelUI();
        }
    }

    /// <summary>
    /// ��ų �����ٿ� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// ��ų ����Ʈ�� ��ȯ�ϰ� �ӽ� ������ �����ϴ�.
    /// </summary>
    private void OnLevelDownButtonClick()
    {
        // ������ ����: SkillPointManager�� ���� �ٿ��� �������� �����մϴ�.
        if (skillPointManager.CanLevelDown(currentSkillData.skillId))
        {
            // ��ų ����Ʈ ��ȯ (�ӽ� ����)
            skillPointManager.RefundPoint();
            // ��ų �ӽ� ���� ����
            tempLevel--;
            // ��ų ���� ���� ������ SkillPointManager�� ����
            skillPointManager.UpdateTempSkillLevel(currentSkillData.skillId, tempLevel);
            // UI ������Ʈ
            UpdatePanelUI();
        }
    }

    /// <summary>
    /// �ݱ� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnCloseButtonClick()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ��ų ����Ʈ�� ������ ���� ��ư ���¸� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateButtonStates()
    {
        // ������ ��ư ����: �ӽ� ��ų ����Ʈ�� 1 �̻��̰�, �ִ� ������ �������� �ʾ��� �� Ȱ��ȭ
        bool canLevelUp = skillPointManager.GetTempSkillPoints() > 0 && tempLevel < currentSkillData.levelInfo.Length;
        levelUpButton.interactable = canLevelUp;

        // ���� �ٿ� ��ư ����: SkillPointManager�� ���� �ٿ� ���� ���θ� �����մϴ�.
        bool canLevelDown = skillPointManager.CanLevelDown(currentSkillData.skillId);
        levelDownButton.interactable = canLevelDown;
    }
}