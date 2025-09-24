// ���ϸ�: InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �丮 UI �� �κ��丮 �г��� ���� ������ ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    // === UI ���� ===
    [Header("UI References")]
    [Tooltip("������ �̹����� ǥ���ϴ� UI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private Image itemImage;
    [Tooltip("������ �̸��� ǥ���ϴ� UI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI itemNameText;

    private DraggableItem draggableItem;

    private void Awake()
    {
        draggableItem = GetComponent<DraggableItem>();
    }

    public void SetData(ItemData itemData)
    {
        if (itemData != null && itemData.itemSO != null)
        {
            itemImage.sprite = itemData.itemSO.itemIcon;
            itemNameText.text = itemData.itemSO.itemName;

            // �߰��� ����: DraggableItem ������Ʈ�� ������ ����
            if (draggableItem != null)
            {
                draggableItem.SetItemData(itemData);
            }
        }
    }
    // === ������ �Ҵ� �޼��� ===
    /// <summary>
    /// ������ �����͸� �޾ƿ� UI�� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="itemData">ǥ���� ������ �������Դϴ�.</param>
   
}