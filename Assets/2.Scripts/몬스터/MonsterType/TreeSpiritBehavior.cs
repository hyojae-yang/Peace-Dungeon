using UnityEngine;
using System.Collections;

/// <summary>
/// ���� ���� ������ Ưȭ�� �ൿ ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���ڸ����� ����ϴٰ� �÷��̾� ���� �� '�Ѹ� ����' ������ �غ��մϴ�.
/// </summary>
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(MonsterCombat))]
public class TreeSpiritBehavior : MonoBehaviour
{
    // === ���Ӽ� ===
    private Monster monster;
    private MonsterCombat monsterCombat;

    // === �÷��̾� ���� �� ���� ���� ���� ===
    [Header("�ൿ ����")]
    [Tooltip("�÷��̾� ���� �� ������ ������ �ּ� �Ÿ��Դϴ�.")]
    [SerializeField] private float detectionRange = 5f;

    // === Ư�� ���� ���� ���� ===
    [Header("Ư�� ���� ����")]
    [Tooltip("Ư�� ������ ��Ÿ���Դϴ�.")]
    [SerializeField] private float aoeAttackCooldown = 10f;
    [Tooltip("Ư�� ���� �غ� �ð��Դϴ�. (�ִϸ��̼� ���̿� ���߾� ����)")]
    [SerializeField] private float aoeChargeTime = 1.5f;
    [Tooltip("Ư�� ���� ȿ�� �������Դϴ�. RootTrap ��ũ��Ʈ�� ���ԵǾ�� �մϴ�.")]
    [SerializeField] private GameObject rootTrapPrefab;

    // === ���� ���� ���� ���� ===
    private float lastAoeAttackTime;
    private bool isCharging = false;
    private Coroutine chargeRoutine; // �ߺ� ������ ���� ���� �ڷ�ƾ ����

    /// <summary>
    /// ������Ʈ �ʱ�ȭ �� ���Ӽ� Ȯ���� ����մϴ�.
    /// </summary>
    private void Awake()
    {
        monster = GetComponent<Monster>();
        monsterCombat = GetComponent<MonsterCombat>();
        if (monster == null) Debug.LogError("TreeSpiritBehavior ��ũ��Ʈ�� Monster ������Ʈ�� �ʿ�� �մϴ�!", this);
        if (monsterCombat == null) Debug.LogError("TreeSpiritBehavior ��ũ��Ʈ�� MonsterCombat ������Ʈ�� �ʿ�� �մϴ�!", this);

        lastAoeAttackTime = -aoeAttackCooldown;
    }

    /// <summary>
    /// �� ������ ������Ʈ ������ ó���մϴ�.
    /// �÷��̾��� ���� ���ο� �Ÿ��� ���� ������ ���¸� ��ȯ�ϰ� �ൿ�� �����մϴ�.
    /// </summary>
    private void Update()
    {
        // ���Ͱ� ���� �����̰ų� �̹� ������ �غ� ���̸� �ƹ��͵� ���� �ʽ��ϴ�.
        if (monster.currentState == MonsterBase.MonsterState.Dead || isCharging)
        {
            return;
        }

        // �÷��̾ ���� ���� ���� �ְ�, Ư�� ���� ��Ÿ���� �������� Ȯ���մϴ�.
        if (monster.detectableTarget != null && Time.time >= lastAoeAttackTime + aoeAttackCooldown)
        {
            // ���� �غ� ���·� ��ȯ�ϰ� �ڷ�ƾ�� �����մϴ�.
            isCharging = true;
            chargeRoutine = StartCoroutine(ChargeAttackRoutine());
        }
    }

    /// <summary>
    /// Ư�� ������ �غ��ϰ� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// �� ��ƾ�� ����Ǵ� ���� ���ʹ� �ٸ� �ൿ�� ���� �ʽ��ϴ�.
    /// </summary>
    private IEnumerator ChargeAttackRoutine()
    {
        // ������ ���¸� Charge�� �����մϴ�.
        monster.ChangeState(MonsterBase.MonsterState.Charge);

        // aoeChargeTime ��ŭ ��ٸ��ϴ�. �� �ð� ���� ���ʹ� ��¡ ���¸� �����մϴ�.
        yield return new WaitForSeconds(aoeChargeTime);

        // �÷��̾ ���� ���� ���� ���� ���� ��쿡�� ������ �����մϴ�.
        if (monster.detectableTarget != null)
        {
            // RootTrap �������� �÷��̾� ��ġ�� �����Ͽ� ���� ȿ���� �߻���ŵ�ϴ�.
            Vector3 playerPos = monster.detectableTarget.GetTransform().position;
            Instantiate(rootTrapPrefab, playerPos, Quaternion.identity);

            Debug.Log("���� ������ �Ѹ� ���� ������ �����մϴ�!");
        }

        // ������ �Ϸ�Ǿ����Ƿ� ���¸� �ʱ�ȭ�մϴ�.
        monster.ChangeState(MonsterBase.MonsterState.Idle);
        lastAoeAttackTime = Time.time; // ��Ÿ�� ����
        isCharging = false;
    }

    /// <summary>
    /// ����� �� �ð�ȭ�� ���� ����� �׸��ϴ�.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}