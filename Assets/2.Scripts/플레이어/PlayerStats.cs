using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾��� ���� �����͸� �����ϰ� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� ���� �� �̻� �̱����� �ƴϸ�,
/// PlayerCharacter ��ũ��Ʈ�� ����� ���ԵǾ� �����˴ϴ�.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // === ���� ���� �������� �״�� �Ӵϴ�. ===
    // ���� �� �Ʒ��� �ִ� ���� �������� ��� PlayerCharacter.Instance.playerStats.���������� �����ϰ� �˴ϴ�.

    // === �⺻ �ɷ�ġ ===
    [Header("�⺻ �ɷ�ġ")]
    [Tooltip("�÷��̾��� ���� ü���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMaxHealth = 100f;
    public float baseMaxMana = 50f;
    public float baseAttackPower = 10f;
    public float baseMagicAttackPower = 5f;
    public float baseDefense = 5f;
    public float baseMagicDefense = 5f;

    [Header("�⺻ Ư�� �ɷ�ġ")]
    [Tooltip("ġ��Ÿ�� �߻��� �⺻ Ȯ���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseCriticalChance = 0.05f;
    public float baseCriticalDamageMultiplier = 1.5f;
    public float baseMoveSpeed = 5f;

    // === �ǽð� �ɷ�ġ (���� �÷��� �� ���ϴ� ����) ===
    [Header("�ǽð� �ɷ�ġ")]
    public string characterName = "Hero";
    public int gold = 0;
    public int level = 1;
    public int experience = 0;
    public float requiredExperience = 10f;

    public float MaxHealth = 100f; // �ִ� ü��
    public float health = 100f; // ���� ü��
    public float MaxMana = 50f; // �ִ� ����
    public float mana = 50f; // ���� ����
    public float attackPower = 10f; // ���ݷ�
    public float magicAttackPower = 5f; // ���� ���ݷ�
    public float defense = 5f; // ����
    public float magicDefense = 5f; // ���� ����

    // PlayerStatSystem ��ũ��Ʈ���� ���Ǿ� ���������� ����Ǵ� �����Դϴ�.
    [Header("���� Ư�� �ɷ�ġ")]
    [Tooltip("ġ��Ÿ�� �߻��� ���� Ȯ���Դϴ�. (0.0 ~ 1.0)")]
    [Range(0.0f, 1.0f)]
    public float criticalChance = 0.05f; // ġ��Ÿ Ȯ�� (5%)
    [Tooltip("ġ��Ÿ �߻� �� �߰��Ǵ� ���� ���ط� �����Դϴ�.")]
    public float criticalDamageMultiplier = 1.5f; // ġ��Ÿ ������ (150%)
    [Tooltip("ĳ������ ���� �̵� �ӵ��Դϴ�.")]
    public float moveSpeed = 5f; // �̵� �ӵ�

    // === ��ų �ý��� ===
    [Header("��ų �ý���")]
    [Tooltip("�÷��̾ ������ ���� ��ų ����Ʈ�Դϴ�.")]
    public int skillPoints;
    [Tooltip("�÷��̾ ������ ��ų�� ���� ���� �������Դϴ�. (��ųID, ��ų����)")]
    public Dictionary<int, int> skillLevels = new Dictionary<int, int>();
}