using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// ������ ��޿� ���� ���� ��Ģ�� �����ϴ� ����ü�Դϴ�.
/// </summary>
[Serializable]
public struct ItemGradeRule
{
    public ItemGrade itemGrade;
    [Tooltip("�⺻ �ɷ�ġ�� ������ ������ �ּڰ�(x)�� �ִ�(y)�Դϴ�.")]
    public Vector2 baseStatMultiplierRange;
    [Tooltip("�������� �ο��� �߰� �ɷ�ġ�� �����Դϴ�.")]
    public int additionalStatCount;
    [Tooltip("�߰� �ɷ�ġ�� ���� ������ ������ �ּڰ�(x)�� �ִ�(y)�Դϴ�.")]
    public Vector2 additionalStatMultiplierRange;
}

/// <summary>
/// ��� �������� ��޿� ���� �������� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class ItemGenerator : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static ItemGenerator Instance { get; private set; }

    [Header("��޺� ������ ���� ��Ģ")]
    [Tooltip("�� ��޿� �´� ��Ģ�� ������ �ּ���.")]
    public List<ItemGradeRule> gradeRules = new List<ItemGradeRule>();

    // �� ��ųʸ��� �ν����� ����Ʈ�� ��� ���̺�� ��ȯ�Ͽ� ������ ����ȭ�մϴ�.
    private Dictionary<ItemGrade, ItemGradeRule> gradeRuleMap;

    private void Awake()
    {
        // �̱��� ���� ���� (DontDestroyOnLoad ����)
        if (Instance == null)
        {
            Instance = this;
            // ��Ģ ��ųʸ��� �ʱ�ȭ�Ͽ� O(1) �ð� ���⵵�� ��Ģ�� ã�� �� �ְ� �մϴ�.
            gradeRuleMap = gradeRules.ToDictionary(rule => rule.itemGrade);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ������ ��ް� �⺻ ���ø����� ���ο� ��� �������� �����մϴ�.
    /// </summary>
    /// <param name="templateItem">�⺻ �����͸� ���� EquipmentItemSO ���ø�</param>
    /// <param name="itemGrade">������ �������� ���</param>
    /// <returns>������ �ɼ��� �ο��� ���ο� EquipmentItemSO �ν��Ͻ�</returns>
    public EquipmentItemSO GenerateItem(EquipmentItemSO templateItem, ItemGrade itemGrade)
    {
        if (templateItem == null)
        {
            Debug.LogError("������ ���ø��� null�̹Ƿ� �������� ������ �� �����ϴ�.");
            return null;
        }

        if (!gradeRuleMap.ContainsKey(itemGrade))
        {
            Debug.LogWarning($"��� '{itemGrade}'�� ���� ��Ģ�� ItemGenerator�� �������� �ʾҽ��ϴ�. �⺻ ������ �������� �����մϴ�.");
            return Instantiate(templateItem); // ��Ģ�� ������ ���ø��� �״�� ����
        }

        // 1. ���� ���ø��� �����Ͽ� ���ο� �ν��Ͻ��� ����ϴ�.
        EquipmentItemSO newItem = Instantiate(templateItem);
        newItem.itemGrade = itemGrade; // �������� ����� �����մϴ�.

        // 2. ��޿� �´� ��Ģ�� �����ɴϴ�.
        ItemGradeRule rule = gradeRuleMap[itemGrade];

        // 3. �⺻ �ɷ�ġ�� ������ ������ ������ ����ϰ� ����ŵ�ϴ�.
        float baseStatMultiplier = UnityEngine.Random.Range(rule.baseStatMultiplierRange.x, rule.baseStatMultiplierRange.y);

        List<StatModifier> newBaseStats = new List<StatModifier>();
        foreach (var stat in newItem.baseStats)
        {
            // --- ������ �κ�: �Ҽ��� ù° �ڸ��� �ݿø� ���� �߰� ---
            float roundedValue = Mathf.Round(stat.value * baseStatMultiplier * 10f) / 10f;
            StatModifier upgradedStat = new StatModifier(stat.statType, roundedValue, stat.isPercentage);
            // --------------------------------------------------------
            newBaseStats.Add(upgradedStat);
        }
        newItem.baseStats = newBaseStats;

        // 4. �߰� �ɷ�ġ�� �������� �����ϰ� ��� ��Ģ�� ���� ����ŵ�ϴ�.
        List<StatModifier> newAdditionalStats = new List<StatModifier>();
        // ���ø��� �߰� �ɼ� ����Ʈ�� �����Ͽ� �����ϴ�.
        List<StatModifier> shuffledOptions = newItem.additionalStats.OrderBy(x => Guid.NewGuid()).ToList();

        // �߰� �ɷ�ġ�� ������ ������ ������ ����մϴ�.
        float additionalStatMultiplier = UnityEngine.Random.Range(rule.additionalStatMultiplierRange.x, rule.additionalStatMultiplierRange.y);

        int statsToSelect = Math.Min(rule.additionalStatCount, shuffledOptions.Count);
        for (int i = 0; i < statsToSelect; i++)
        {
            StatModifier selectedStat = shuffledOptions[i];
            // --- ������ �κ�: �Ҽ��� ù° �ڸ��� �ݿø� ���� �߰� ---
            float roundedValue = Mathf.Round(selectedStat.value * additionalStatMultiplier * 10f) / 10f;
            StatModifier upgradedStat = new StatModifier(selectedStat.statType, roundedValue, selectedStat.isPercentage);
            // --------------------------------------------------------
            newAdditionalStats.Add(upgradedStat);
        }
        newItem.additionalStats = newAdditionalStats;

        return newItem;
    }
}