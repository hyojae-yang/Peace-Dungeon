using UnityEngine;
using System.Collections.Generic; // Dictionary�� ����ϱ� ���� �߰��մϴ�.

// ���� �����͸� �����ϰ� �����ϴ� ��ũ��Ʈ�Դϴ�.
// �� ��ũ��Ʈ�� MonoBehaviour�� ��ӹ޾� ���� ������Ʈ�� �����Ͽ� ����� �� �ֽ��ϴ�.
public class PlayerStats : MonoBehaviour
{
    // === �⺻ �ɷ�ġ ===
    [Header("�⺻ �ɷ�ġ")]
    [Tooltip("�÷��̾��� ���� ü���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMaxHealth = 100f;
    [Tooltip("�÷��̾��� ���� �����Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMaxMana = 50f;
    [Tooltip("�÷��̾��� ���� ���ݷ��Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseAttackPower = 10f;
    [Tooltip("�÷��̾��� ���� ���� ���ݷ��Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMagicAttackPower = 5f;
    [Tooltip("�÷��̾��� ���� �����Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseDefense = 5f;
    [Tooltip("�÷��̾��� ���� ���� �����Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMagicDefense = 5f;

    [Header("�⺻ Ư�� �ɷ�ġ")]
    [Tooltip("ġ��Ÿ�� �߻��� �⺻ Ȯ���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseCriticalChance = 0.05f;
    [Tooltip("ġ��Ÿ �߻� �� �߰��Ǵ� �⺻ ���ط� �����Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseCriticalDamageMultiplier = 1.5f;
    [Tooltip("ĳ������ �⺻ �̵� �ӵ��Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMoveSpeed = 5f;
    [Tooltip("���� ȸ�� �⺻ Ȯ���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    [Range(0.0f, 1.0f)]
    public float baseEvasionChance = 0.02f;

    // === �ǽð� �ɷ�ġ (���� �÷��� �� ���ϴ� ����) ===
    [Header("�ǽð� �ɷ�ġ")]
    [Tooltip("ĳ���� �̸�")]
    public string characterName = "Hero";
    [Tooltip("���� �ݾ�")]
    public int gold = 0;
    [Tooltip("���� ����")]
    public int level = 1;
    [Tooltip("���� ����ġ")]
    public int experience = 0;
    [Tooltip("���� ������ �ʿ��� �� ����ġ")]
    public float requiredExperience = 10f;

    // PlayerStatSystem ��ũ��Ʈ���� ���Ǿ� ���������� ����Ǵ� �����Դϴ�.
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
    [Tooltip("���� ȸ�� ���� Ȯ���Դϴ�. (0.0 ~ 1.0)")]
    [Range(0.0f, 1.0f)]
    public float evasionChance = 0.02f; // ȸ���� (2%)

    // === ��ų �ý��� ===
    [Header("��ų �ý���")]
    [Tooltip("�÷��̾ ������ ���� ��ų ����Ʈ�Դϴ�.")]
    public int skillPoints;
    [Tooltip("�÷��̾ ������ ��ų�� ���� ���� �������Դϴ�. (��ųID, ��ų����)")]
    public Dictionary<int, int> skillLevels = new Dictionary<int, int>();
}