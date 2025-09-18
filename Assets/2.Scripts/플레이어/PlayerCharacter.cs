using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾�� ���õ� ��� �ֿ� �ý����� �����ϴ� �߾� ��� ��ũ��Ʈ�Դϴ�.
/// �̱��� �������� �����Ǿ� ��𼭵� ���� ������ �� �ֽ��ϴ�.
/// �� ��ũ��Ʈ�� �ڽ��� ������ ���� ������Ʈ�� �����ϴ� �ٸ� �ý��� ��ũ��Ʈ���� ������ �����Ͽ� �����ϴ� ���Ҹ� �����մϴ�.
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    // PlayerCharacter Ŭ������ ������ �ν��Ͻ��� �����ϴ� ���� �Ӽ��Դϴ�.
    // �ܺο��� PlayerCharacter.Instance�� ���� ������ �� �ֽ��ϴ�.
    public static PlayerCharacter Instance;

    // === ���� �ý��� ===
    // �÷��̾� �ý����� �ٽ� ������Ʈ���� ��� ���� ��� �������Դϴ�.
    // �� �������� Inspector���� ���� �Ҵ��ϰų�, Awake �޼��忡�� �ڵ����� �Ҵ�˴ϴ�.
    [Header("�ٽ� �ý��� ����")]
    [Tooltip("�÷��̾��� ���� �����͸� ���� �� �����ϴ� PlayerStats ������Ʈ�Դϴ�.")]
    public PlayerStats playerStats;

    [Tooltip("�÷��̾��� ���� �ý����� �����ϴ� PlayerStatSystem ������Ʈ�Դϴ�.")]
    public PlayerStatSystem playerStatSystem;

    [Tooltip("�÷��̾��� �κ��丮 �ý����� �����մϴ�.")]
    public InventoryManager inventoryManager;

    [Tooltip("�÷��̾��� ��� ���� �ý����� �����ϴ� PlayerEquipmentManager ������Ʈ�Դϴ�.")]
    public PlayerEquipmentManager playerEquipmentManager;

    [Tooltip("�÷��̾��� �̵��� �����ϴ� PlayerController ������Ʈ�Դϴ�.")]
    public PlayerController playerController;

    [Tooltip("�÷��̾��� ������ �����ϴ� PlayerAttack ������Ʈ�Դϴ�.")]
    public PlayerAttack playerAttack;

    [Tooltip("�÷��̾��� ü�� �� ������ ������ ó���ϴ� PlayerHealth ������Ʈ�Դϴ�.")]
    public PlayerHealth playerHealth;

    [Tooltip("�÷��̾��� �������� �����ϴ� PlayerLevelUp ������Ʈ�Դϴ�.")]
    public PlayerLevelUp playerLevelUp;

    [Tooltip("�÷��̾��� ��ų ��� �� ������ ����ϴ� PlayerSkillController ������Ʈ�Դϴ�.")]
    public PlayerSkillController playerSkillController;

    // SkillPointManager�� �̱������� �����˴ϴ�.
    // ���� ���� ���� ������ �ʿ����� �ʽ��ϴ�.

    /// <summary>
    /// �� ��ũ��Ʈ�� Awake�� �� ȣ��Ǹ�, �̱��� �ν��Ͻ��� �ʱ�ȭ�ϰ� ��� �ý��� ������Ʈ�� �Ҵ��մϴ�.
    /// </summary>
    private void Awake()
    {
        // 1. �̱��� �ν��Ͻ� �Ҵ� �� �ߺ� �ν��Ͻ� �ı�
        if (Instance == null)
        {
            Instance = this;
            // ���� ����Ǿ �ı����� �ʰ� �Ϸ��� �Ʒ� �ּ��� �����ϼ���.
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter�� �ν��Ͻ��� �̹� �����մϴ�. �� ������Ʈ�� �ı��մϴ�.");
            Destroy(gameObject);
            return; // �ߺ� �ν��Ͻ��̹Ƿ� �Ʒ� �ڵ�� �������� �ʽ��ϴ�.
        }

        // 2. ��� �ý��� ������Ʈ �ڵ� �Ҵ�
        // ��� ��ũ��Ʈ�� ���� ���� ������Ʈ�� �����Ǿ� �ִٴ� �����Ͽ� GetComponent�� ����մϴ�.
        playerStats = GetComponent<PlayerStats>();
        playerStatSystem = GetComponent<PlayerStatSystem>();
        inventoryManager = GetComponent<InventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealth = GetComponent<PlayerHealth>();
        playerLevelUp = GetComponent<PlayerLevelUp>();
        playerSkillController = GetComponent<PlayerSkillController>();

        // 3. �ʼ� ������Ʈ ���� ���� Ȯ�� (����� ����)
        ValidateSystemReferences();
    }

    /// <summary>
    /// ��� �ý��� ������Ʈ�� ���������� �Ҵ�Ǿ����� Ȯ���մϴ�.
    /// </summary>
    private void ValidateSystemReferences()
    {
        if (playerStats == null) Debug.LogError("PlayerCharacter: 'PlayerStats' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerStatSystem == null) Debug.LogError("PlayerCharacter: 'PlayerStatSystem' ������Ʈ�� �����Ǿ����ϴ�.");
        if (inventoryManager == null) Debug.LogError("PlayerCharacter: 'InventoryManager' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerEquipmentManager == null) Debug.LogError("PlayerCharacter: 'PlayerEquipmentManager' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerController == null) Debug.LogError("PlayerCharacter: 'PlayerController' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerAttack == null) Debug.LogError("PlayerCharacter: 'PlayerAttack' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerHealth == null) Debug.LogError("PlayerCharacter: 'PlayerHealth' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerLevelUp == null) Debug.LogError("PlayerCharacter: 'PlayerLevelUp' ������Ʈ�� �����Ǿ����ϴ�.");
        if (playerSkillController == null) Debug.LogError("PlayerCharacter: 'PlayerSkillController' ������Ʈ�� �����Ǿ����ϴ�.");

        if (playerStats != null && playerStatSystem != null && playerController != null && playerAttack != null && playerHealth != null && playerLevelUp != null && playerSkillController != null)
        {
        }
    }
}