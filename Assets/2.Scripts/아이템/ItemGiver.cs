// ItemGiver.cs
using UnityEngine;

/// <summary>
/// �÷��̾�� �������� �����Ͽ� �κ��丮 �ý����� �׽�Ʈ�ϱ� ���� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class ItemGiver : MonoBehaviour
{
    [Header("�׽�Ʈ ������ ����")]
    [Tooltip("Q Ű�� ���� �� �κ��丮�� �߰��� �������� �Ҵ��ϼ���.")]
    [SerializeField] private BaseItemSO itemToGive;

    [Tooltip("�߰��� �������� ������ �����ϼ���.")]
    [SerializeField] private int amountToGive = 1;

    /// <summary>
    /// �� �����Ӹ��� Ű �Է��� Ȯ���մϴ�.
    /// </summary>
    private void Update()
    {
        // P Ű�� ������ �������� �����մϴ�.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GiveItem();
        }
    }

    /// <summary>
    /// ������ �������� InventoryManager�� ���� �κ��丮�� �߰��մϴ�.
    /// </summary>
    private void GiveItem()
    {
        if (itemToGive != null && InventoryManager.Instance != null)
        {
            // InventoryManager�� AddItem �޼��带 ȣ���Ͽ� �������� �߰��մϴ�.
            InventoryManager.Instance.AddItem(itemToGive, amountToGive);
            Debug.Log($"<color=green>[System]</color> {itemToGive.itemName} (x{amountToGive}) �������� �κ��丮�� �߰��߽��ϴ�.");
        }
        else
        {
            Debug.LogError("<color=red>�������� ������ �� �����ϴ�! itemToGive �Ǵ� InventoryManager�� �Ҵ���� �ʾҽ��ϴ�.</color>");
        }
    }
}