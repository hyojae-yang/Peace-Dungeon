using UnityEngine;
using System.Collections.Generic;

// �÷��̾��� ���� �����͸� �����ϰ� �����ϴ� ��ũ��Ʈ�Դϴ�.
// �̱��� �������� �����Ͽ� ��𼭵� ���� �����ϵ��� ����ϴ�.
public class PlayerStats : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    // private static���� �ܺο��� ���� �ν��Ͻ��� �������� ���ϰ� �����ϴ�.
    private static PlayerStats _instance;

    // public static���� �ܺο��� PlayerStats.Instance�� ���� ������ �� �ֵ��� �մϴ�.
    public static PlayerStats Instance
    {
        get
        {
            // �ν��Ͻ��� ���� �������� �ʾ��� ��
            if (_instance == null)
            {
                // ������ PlayerStats ������Ʈ�� ���� ���� ������Ʈ�� ã���ϴ�.
                _instance = FindFirstObjectByType<PlayerStats>();

                // ���� ã�� ���ߴٸ� ���ο� ���� ������Ʈ�� ����� ������Ʈ�� �߰��մϴ�.
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("PlayerStatsSingleton");
                    _instance = singletonObject.AddComponent<PlayerStats>();
                    Debug.Log("���ο� 'PlayerStatsSingleton' ���� ������Ʈ�� �����߽��ϴ�.");
                }
            }
            return _instance;
        }
    }

    // === ��ũ��Ʈ�� Awake�� �� �̱��� �ʱ�ȭ ===
    // �� ��ũ��Ʈ�� Awake�� ������ ȣ��˴ϴ�.
    void Awake()
    {
        // ���� �̹� �ν��Ͻ��� �����ϰ� �� ��ü�� �� �ν��Ͻ��� �ƴ϶��
        if (_instance != null && _instance != this)
        {
            // �ߺ��� ��ü�̹Ƿ� �ı��մϴ�.
            Destroy(gameObject);
        }
        else
        {
            // �� ��ü�� ������ �ν��Ͻ��� �����մϴ�.
            _instance = this;
            // ���� ����Ǿ �� ��ü�� �ı����� �ʵ��� �����մϴ�.
            // DontDestroyOnLoad(gameObject);
            // ���� ���� ���� �� �� ��ü�� �̹� ���� ��ġ�ߴٸ� DontDestroyOnLoad��
            // �ʿ� ���� ���� �ֽ��ϴ�. ������Ʈ�� ������ �°� ������ �ּ���.
        }
    }

    // === ���� ���� �������� �״�� �Ӵϴ�. ===
    // ���� �� �Ʒ��� �ִ� ���� �������� ��� PlayerStats.Instance.���������� �����ϰ� �˴ϴ�.

    // === �⺻ �ɷ�ġ ===
    [Header("�⺻ �ɷ�ġ")]
    [Tooltip("�÷��̾��� ���� ü���Դϴ�. PlayerStatSystem ��ũ��Ʈ���� �����մϴ�.")]
    public float baseMaxHealth = 100f;
    // ... (������ ���� ������) ...
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
    [Range(0.0f, 1.0f)]
    public float baseEvasionChance = 0.02f;

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