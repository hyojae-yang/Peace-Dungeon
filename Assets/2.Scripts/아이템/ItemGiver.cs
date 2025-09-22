using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �÷��̾�� �������� �����Ͽ� �κ��丮 �ý����� �׽�Ʈ�ϱ� ���� ��ũ��Ʈ�Դϴ�.
/// �پ��� Ÿ���� ������(���, �Ҹ�ǰ ��)�� �����ϰ� �߰��ϴ� ����� �����մϴ�.
/// </summary>
public class ItemGiver : MonoBehaviour
{
    // === �ν����Ϳ� �Ҵ��� ���� ===
    [Header("�׽�Ʈ ������ ����")]
    [Tooltip("�κ��丮�� �߰��� ������ ���ø��Դϴ�. ��� ������ �������� �Ҵ��� �� �ֽ��ϴ�.")]
    [SerializeField] private BaseItemSO itemTemplate;

    [Tooltip("��� ������ ���� �� ����� ����Դϴ�. (��� ���ø� �Ҵ� �ÿ��� ��ȿ)")]
    [SerializeField] private ItemGrade itemGrade = ItemGrade.Common;

    // === ���� ������ ���� ===
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        // PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("ItemGiver: PlayerCharacter �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // InventoryManager�� PlayerCharacter�� �Ҵ�Ǿ����� Ȯ���մϴ�.
        if (playerCharacter.inventoryManager == null)
        {
            Debug.LogError("ItemGiver: InventoryManager�� PlayerCharacter�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    // === MonoBehaviour �޼��� ===

    /// <summary>
    /// �� �����Ӹ��� Ű �Է��� Ȯ���Ͽ� �������� �����ϰ� �κ��丮�� �����մϴ�.
    /// Z Ű: ��� ������ ���� �� ����
    /// C Ű: �Ҹ�ǰ ������ ���� �� ����
    /// </summary>
    private void Update()
    {
        // Q Ű�� ������ ��� �������� �����մϴ�.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GiveGeneratedItem();
        }

        // E Ű�� ������ �Ҹ�ǰ �������� �����մϴ�.
        if (Input.GetKeyDown(KeyCode.C))
        {
            GiveConsumableItem();
        }
    }

    /// <summary>
    /// ItemGenerator�� ���� ��� �������� �������� �����Ͽ� �κ��丮�� �߰��մϴ�.
    /// �� �޼���� Z Ű �Է� �� ȣ��˴ϴ�.
    /// </summary>
    private void GiveGeneratedItem()
    {
        // ���ø��� ��� ���������� Ȯ���մϴ�.
        EquipmentItemSO equipTemplate = itemTemplate as EquipmentItemSO;

        if (equipTemplate != null && playerCharacter.inventoryManager != null && ItemGenerator.Instance != null)
        {
            // ItemGenerator�� �̱����̹Ƿ� Instance�� ���� �����մϴ�.
            EquipmentItemSO newItem = ItemGenerator.Instance.GenerateItem(equipTemplate, itemGrade);

            // ������ �������� �κ��丮�� �߰��մϴ�.
            playerCharacter.inventoryManager.AddItem(newItem, 1);
        }
    }

    /// <summary>
    /// �Ҵ�� �Ҹ�ǰ �������� �κ��丮�� �߰��մϴ�.
    /// �� �޼���� C Ű �Է� �� ȣ��˴ϴ�.
    /// </summary>
    private void GiveConsumableItem()
    {
        // ���ø��� �Ҹ�ǰ ���������� Ȯ���մϴ�.
        BaseItemSO consumeTemplate = itemTemplate as BaseItemSO;

        if (consumeTemplate != null && playerCharacter.inventoryManager != null)
        {
            // �Ҹ�ǰ �������� ������ ���� ���� ���� �����Ͽ� �κ��丮�� �߰��մϴ�.
            BaseItemSO newItem = Instantiate(consumeTemplate);

            // ������ �������� �κ��丮�� �߰��մϴ�.
            playerCharacter.inventoryManager.AddItem(newItem, 1);
        }
    }
}