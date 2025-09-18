using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ������ ���� �г��� UI�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ������ ������ �޾ƿ� �г��� ������ ä��ϴ�.
/// �� ��ũ��Ʈ�� ���� �г� �����տ� �����˴ϴ�.
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    // === �ν����Ϳ� �Ҵ��� UI ������Ʈ ===
    [Header("���� ���� UI ���")]
    [Tooltip("������ �������� ǥ���� Image ������Ʈ�Դϴ�.")]
    [SerializeField] private Image itemIconImage;

    [Tooltip("������ �̸��� ǥ���� Text ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("������ ������ ǥ���� Text ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    // �� �Ʒ� �������� ��� ������ ���� �����տ��� �Ҵ�� �� �ֽ��ϴ�.
    // �Ϲ� ������ ���� �����տ����� null�� �˴ϴ�.

    [Header("��� ���� ���� UI ���")]
    [Tooltip("������ ����� ǥ���� Text ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI itemGradeText;

    [Tooltip("�⺻ �ɷ�ġ(���ݷ�, ���� ��)�� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI baseStatsText;

    [Tooltip("�߰� �ɷ�ġ 1�� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI additionalStat1Text;

    [Tooltip("�߰� �ɷ�ġ 2�� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI additionalStat2Text;

    [Tooltip("�߰� �ɷ�ġ 3�� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI additionalStat3Text;

    [Tooltip("�߰� �ɷ�ġ 4�� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI additionalStat4Text;

    [Tooltip("��Ʈ ȿ���� ǥ���� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI setBonusText;

    /// <summary>
    /// ������ ������ �޾� ������ ������ �����մϴ�.
    /// �� �޼���� ��� ������ ���� UI�� �Ҵ�Ǿ� ���� �ʴ���
    /// Null üũ�� ���� �����ϰ� �۵��մϴ�.
    /// </summary>
    /// <param name="item">ǥ���� �������� BaseItemSO ������</param>
    public void SetupTooltip(BaseItemSO item)
    {
        // 1. ���� UI ���� (�� �κ��� ��� ������ �����Ѵٰ� ����)
        if (itemIconImage != null) itemIconImage.sprite = item.itemIcon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescription;

        // 2. ��� �������� ���, �߰� UI ��ҵ��� �����մϴ�.
        if (item is EquipmentItemSO equipmentItem)
        {
            // ��� ���� UI ��ҵ��� null���� üũ �� ����
            if (itemGradeText != null)
            {
                itemGradeText.text = GetGradeName(equipmentItem.itemGrade);
                itemGradeText.color = GetGradeColor(equipmentItem.itemGrade);
            }

            // �⺻ �ɷ�ġ ����
            if (baseStatsText != null)
            {
                if (equipmentItem.baseStats != null && equipmentItem.baseStats.Count > 0)
                {

                    baseStatsText.text = FormatStats(equipmentItem.baseStats);
                }
                else
                {
                    baseStatsText.text = string.Empty;
                }
            }

            // �߰� �ɷ�ġ �ؽ�Ʈ 4���� ���� ����
            if (equipmentItem.additionalStats != null)
            {

                if (additionalStat1Text != null) additionalStat1Text.text = equipmentItem.additionalStats.Count > 0 ? FormatStat(equipmentItem.additionalStats[0]) : string.Empty;
                if (additionalStat2Text != null) additionalStat2Text.text = equipmentItem.additionalStats.Count > 1 ? FormatStat(equipmentItem.additionalStats[1]) : string.Empty;
                if (additionalStat3Text != null) additionalStat3Text.text = equipmentItem.additionalStats.Count > 2 ? FormatStat(equipmentItem.additionalStats[2]) : string.Empty;
                if (additionalStat4Text != null) additionalStat4Text.text = equipmentItem.additionalStats.Count > 3 ? FormatStat(equipmentItem.additionalStats[3]) : string.Empty;
            }
            else
            {
                if (additionalStat1Text != null) additionalStat1Text.text = string.Empty;
                if (additionalStat2Text != null) additionalStat2Text.text = string.Empty;
                if (additionalStat3Text != null) additionalStat3Text.text = string.Empty;
                if (additionalStat4Text != null) additionalStat4Text.text = string.Empty;
            }

            // ��Ʈ ȿ��(������ ��Ʈ ȿ��) ����
            if (setBonusText != null)
            {
                // �����ۿ� ��Ʈ ID�� �ִ��� Ȯ��
                if (!string.IsNullOrEmpty(equipmentItem.setID))
                {
                    // SetBonusDataManager�κ��� ��Ʈ �����͸� �����ɴϴ�.
                    SetBonusDataSO setBonusData = SetBonusDataManager.Instance.GetSetBonusData(equipmentItem.setID);

                    if (setBonusData != null)
                    {
                        // ��Ʈ �̸��� �ܰ躰 ���ʽ��� ���� ���� �������մϴ�.
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"<color=#FFD700>{setBonusData.setName}</color>");

                        // ��Ʈ �ܰ躰 ���ʽ��� ���
                        if (setBonusData.bonusSteps != null)
                        {
                            foreach (var step in setBonusData.bonusSteps.OrderBy(s => s.requiredCount))
                            {
                                sb.AppendLine($"<color=#7CFC00>[{step.requiredCount}�� ȿ��]</color>");
                                sb.AppendLine(FormatStats(step.bonusStats));
                            }
                        }

                        setBonusText.text = sb.ToString();
                    }
                    else
                    {
                        setBonusText.text = "��Ʈ �����͸� ã�� �� �����ϴ�.";
                    }
                }
                else
                {
                    // ��Ʈ �������� �ƴ� ��� �� ���ڿ��� ����
                    setBonusText.text = string.Empty;
                }
            }
        }
    }

    /// <summary>
    /// StatModifier ����Ʈ�� ������ ���� ���� �������Ͽ� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="stats">StatModifier ����Ʈ</param>
    /// <returns>�����õ� ���ڿ�</returns>
    private string FormatStats(List<StatModifier> stats)
    {
        if (stats == null || stats.Count == 0) return string.Empty;

        StringBuilder sb = new StringBuilder();
        foreach (var stat in stats)
        {
            sb.AppendLine(FormatStat(stat));
        }
        return sb.ToString().TrimEnd(); // ������ �ٹٲ� ����
    }

    /// <summary>
    /// ���� StatModifier�� ������ �������Ͽ� ��ȯ�մϴ�.
    /// �� �޼���� ��� ���� �������� ���� å�� ��Ģ�� �����ϴ�.
    /// </summary>
    /// <param name="stat">StatModifier</param>
    /// <returns>�����õ� ���ڿ�</returns>
    private string FormatStat(StatModifier stat)
    {

        // �ۼ�Ʈ�� ǥ���� Ư�� ���ȵ��� ��������� ����
        bool isSpecialPercentageStat = stat.statType == StatType.CriticalChance ||
                                       stat.statType == StatType.CriticalDamage ||
                                       stat.statType == StatType.MoveSpeed;

        if (isSpecialPercentageStat)
        {
            // ���� 100�� ���ϰ� �Ҽ��� ù° �ڸ����� ǥ��
            float displayValue = stat.value * 100f;

            return $"{GetStatName(stat.statType)}: {displayValue.ToString("F1")}%";
        }
        else
        {
            // �� ���� ������ ���� ��Ĵ�� ǥ�� (isPercentage ���� Ȱ��)
            return $"{GetStatName(stat.statType)}: {stat.value}{(stat.isPercentage ? "%" : "")}";
        }
    }

    /// <summary>
    /// ������ ��޿� ���� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <param name="grade">������ ���</param>
    /// <returns>����</returns>
    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return Color.gray;
            case ItemGrade.Rare: return new Color(0.2f, 0.5f, 1f);
            case ItemGrade.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case ItemGrade.Legendary: return new Color(1f, 0.8f, 0.2f);
            default: return Color.white;
        }
    }

    /// <summary>
    /// ������ ��� �������� ���� �ѱ� �̸��� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="grade">������ ���</param>
    /// <returns>��� �̸�</returns>
    private string GetGradeName(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return "�Ϲ�";
            case ItemGrade.Rare: return "���";
            case ItemGrade.Epic: return "����";
            case ItemGrade.Legendary: return "����";
            default: return "�� �� ����";
        }
    }

    /// <summary>
    /// StatType �������� ���� �ѱ� �̸��� ��ȯ�մϴ�.
    /// ���ο� ������ �߰��Ǹ� ���⿡ �߰����־�� �մϴ�.
    /// </summary>
    /// <param name="statType">���� ���� ������</param>
    /// <returns>���� �ѱ� �̸�</returns>
    private string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHealth: return "ü��";
            case StatType.MaxMana: return "����";
            case StatType.AttackPower: return "���ݷ�";
            case StatType.MagicAttackPower: return "���� ���ݷ�";
            case StatType.Defense: return "����";
            case StatType.MagicDefense: return "���� ����";
            case StatType.CriticalChance: return "ġ��Ÿ Ȯ��";
            case StatType.CriticalDamage: return "ġ��Ÿ ���ط�";
            case StatType.MoveSpeed: return "�̵� �ӵ�";
            case StatType.Strength: return "��";
            case StatType.Intelligence: return "����";
            case StatType.Constitution: return "ü��";
            case StatType.Agility: return "��ø";
            case StatType.Focus: return "���߷�";
            case StatType.Endurance: return "�γ���";
            case StatType.Vitality: return "Ȱ��";
            default: return statType.ToString(); // ���ǵ��� ���� ��� ���� �̸� ��ȯ
        }
    }
}