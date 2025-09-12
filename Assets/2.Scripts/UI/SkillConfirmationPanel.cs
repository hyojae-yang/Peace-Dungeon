using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // StringBuilder�� ���� �߰��մϴ�.
using System.Collections.Generic;

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
        // �� ������ SkillPointManager�� ��� ��ų ���� �����͸� �����Ѵٴ� �����Ͽ� �۵��մϴ�.
        tempLevel = skillPointManager.GetSkillCurrentLevel(currentSkillData.skillId);

        // UI ������Ʈ
        UpdatePanelUI();
    }

    /// <summary>
    /// UI �ؽ�Ʈ���� ���� �ӽ� ������ ���� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdatePanelUI()
    {
        // StringBuilder�� ����Ͽ� ȿ�������� �ؽ�Ʈ�� ����ϴ�.
        StringBuilder statStringBuilder = new StringBuilder();

        if (currentSkillData != null)
        {
            // ��ų ������ ��ȿ�� ���� ���� �ִ��� Ȯ���մϴ�.
            // tempLevel�� 0�� ��� (��ų �̽���)�� �����Ͽ� ó���մϴ�.
            if (tempLevel >= 0 && tempLevel <= currentSkillData.levelInfo.Length)
            {
                skillNameText.text = currentSkillData.skillName;

                // ������ 0�� ���� �ƴ� ���� �и��Ͽ� ǥ���մϴ�.
                if (tempLevel == 0)
                {
                    skillLevelText.text = "Lv. 0 (�̽���)";
                    // ���� ����(1����)�� �ɷ�ġ�� �̸� �����ݴϴ�.
                    if (currentSkillData.levelInfo.Length > 0)
                    {
                        SkillLevelInfo nextLevelInfo = currentSkillData.levelInfo[0];
                        statStringBuilder.AppendLine("<color=#FFFF00>���� ���� (Lv.1) �ɷ�ġ:</color>");
                        foreach (SkillStat stat in nextLevelInfo.stats)
                        {
                            statStringBuilder.AppendLine(GetStatText(stat.statType, stat.value));
                        }
                    }
                    else
                    {
                        statStringBuilder.AppendLine("�ɷ�ġ ������ �����ϴ�.");
                    }
                }
                else
                {
                    skillLevelText.text = $"Lv. {tempLevel}";
                    // ���� ������ ��ų ���� ��������
                    SkillLevelInfo currentLevelInfo = currentSkillData.levelInfo[tempLevel - 1];
                    // �ɷ�ġ �ؽ�Ʈ�� �������� �����մϴ�.
                    if (currentLevelInfo.stats != null && currentLevelInfo.stats.Length > 0)
                    {
                        foreach (SkillStat stat in currentLevelInfo.stats)
                        {
                            statStringBuilder.AppendLine(GetStatText(stat.statType, stat.value));
                        }
                    }
                    else
                    {
                        statStringBuilder.AppendLine("�ɷ�ġ ������ �����ϴ�.");
                    }
                }
                skillStatText.text = statStringBuilder.ToString();
            }
            else
            {
                Debug.LogWarning("��ų ������ ��ȿ�� ������ ������ϴ�. ���� ������Ʈ ����.");
            }
        }
        // ��ư Ȱ��ȭ/��Ȱ��ȭ ���¸� ������Ʈ�մϴ�.
        UpdateButtonStates();
    }

    /// <summary>
    /// StatType�� ���� ǥ�õ� �ؽ�Ʈ�� �����մϴ�.
    /// </summary>
    /// <param name="type">���� ����</param>
    /// <param name="value">���� ��</param>
    /// <returns>���˵� ���ڿ�</returns>
    private string GetStatText(StatType type, float value)
    {
        switch (type)
        {
            case StatType.BaseDamage:
                return $"�⺻ ������: {value}";
            case StatType.Cooldown:
                return $"��Ÿ��: {value}��";
            case StatType.ManaCost:
                return $"���� �Ҹ�: {value}";
            default:
                return $"{type}: {value}";
        }
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
        // �ּ� ������ 0���� �����մϴ�.
        if (tempLevel > 0)
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

        // ���� �ٿ� ��ư ����: ���� ������ 0���� Ŭ �� Ȱ��ȭ
        bool canLevelDown = tempLevel > 0;
        levelDownButton.interactable = canLevelDown;
    }
}