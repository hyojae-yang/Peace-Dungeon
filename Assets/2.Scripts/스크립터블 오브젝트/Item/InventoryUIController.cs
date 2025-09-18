using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �κ��丮�� ��� �г��� UI�� �Ѱ������� �����ϴ� ��Ʈ�ѷ� Ŭ�����Դϴ�.
/// InventoryManager�� PlayerEquipmentManager�� �̺�Ʈ�� �����Ͽ� UI�� ������Ʈ�մϴ�.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    public static InventoryUIController Instance { get; private set; }

    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    // === ���� ���� (����Ƽ �ν����Ϳ��� �Ҵ�) ===
    [Header("UI �г� ����")]
    [Tooltip("���� �κ��丮 �г� ������Ʈ�� �Ҵ��մϴ�.")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("��� ���� ����")]
    [Tooltip("��� ���� UI�� �ִ� ���Ե��Դϴ�. (EquipmentSlotUI ��ũ��Ʈ ���)")]
    [SerializeField] private List<EquipmentSlotUI> equippedSlots = new List<EquipmentSlotUI>();

    [Header("�κ��丮 ���� ����Ʈ")]
    [Tooltip("���� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�. (ItemSlotUI ��ũ��Ʈ ���)")]
    [SerializeField] private List<ItemSlotUI> weaponInventorySlots = new List<ItemSlotUI>();
    [Tooltip("�� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> armorInventorySlots = new List<ItemSlotUI>();
    [Tooltip("��ű� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> accessoryInventorySlots = new List<ItemSlotUI>();
    [Tooltip("�Ҹ�ǰ �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> consumableSlots = new List<ItemSlotUI>();
    [Tooltip("��� �гο� �ִ� ������ ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> materialSlots = new List<ItemSlotUI>();
    [Tooltip("����Ʈ ������ �гο� �ִ� ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> questSlots = new List<ItemSlotUI>();
    [Tooltip("Ư�� ������ �гο� �ִ� ���Ե��� ������� �Ҵ��մϴ�.")]
    [SerializeField] private List<ItemSlotUI> specialSlots = new List<ItemSlotUI>();

    [Header("Ȯ��â")]
    [Tooltip("�κ��丮 ������ ������ ���� Ȯ��â UI ������Ʈ�� �Ҵ��մϴ�.")]
    [SerializeField] private GameObject itemDiscardConfirmPanel;
    [Tooltip("��� ������ ������ ���� Ȯ��â UI ������Ʈ�� �Ҵ��մϴ�.")]
    [SerializeField] private GameObject equipDiscardConfirmPanel;

    // === ���� �׷�ȭ�� ���� ��ųʸ� ===
    private Dictionary<ItemType, List<ItemSlotUI>> itemSlotGroups = new Dictionary<ItemType, List<ItemSlotUI>>();
    private Dictionary<EquipType, List<ItemSlotUI>> equipSlotGroups = new Dictionary<EquipType, List<ItemSlotUI>>();

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeSlotGroups();

        // PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;

        // PlayerCharacter�� �Ŵ������� ���� �̺�Ʈ�� �����մϴ�.
        if (playerCharacter != null && playerCharacter.inventoryManager != null)
        {
            playerCharacter.inventoryManager.onInventoryChanged += RefreshInventoryUI;
        }
        if (playerCharacter != null && playerCharacter.playerEquipmentManager != null)
        {
            playerCharacter.playerEquipmentManager.onEquipmentChanged += RefreshEquipmentUI;
        }
    }

    private void Start()
    {
        // ���� ���� �� �ʱ� UI�� �ѹ� �����մϴ�.
        RefreshInventoryUI();
        RefreshEquipmentUI();
    }

    private void OnDestroy()
    {
        // �޸� ������ �����ϱ� ���� ���� �ı��� �� �̺�Ʈ�� ���� �����մϴ�.
        if (playerCharacter != null && playerCharacter.inventoryManager != null)
        {
            playerCharacter.inventoryManager.onInventoryChanged -= RefreshInventoryUI;
        }
        if (playerCharacter != null && playerCharacter.playerEquipmentManager != null)
        {
            playerCharacter.playerEquipmentManager.onEquipmentChanged -= RefreshEquipmentUI;
        }
    }

    /// <summary>
    /// InventoryManager�� onInventoryChanged �̺�Ʈ�� �߻��ϸ� ȣ��˴ϴ�.
    /// �κ��丮 �����͸� ���� �ҷ��� ��� ������ ���� UI�� �����մϴ�.
    /// </summary>
    public void RefreshInventoryUI()
    {
        if (playerCharacter == null || playerCharacter.inventoryManager == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� InventoryManager�� �����Ǿ����ϴ�.");
            return;
        }

        // 1. ��� �κ��丮 ������ ����ݴϴ�.
        ClearAllInventorySlots();

        // 2. InventoryManager���� �ֽ� �κ��丮 �����͸� �����ɴϴ�.
        List<ItemData> inventoryData = playerCharacter.inventoryManager.GetInventoryItems();

        // 3. �� ������ Ÿ�Կ� �´� �гο� �������� ��ġ�մϴ�.
        foreach (var itemData in inventoryData)
        {
            List<ItemSlotUI> slotsToFill = null;
            // ������ Ÿ�Կ� �´� ���� �׷��� ã���ϴ�.
            if (itemData.itemSO is EquipmentItemSO equipmentItem)
            {
                // ��� �������� EquipType�� ���� �׷��� ã���ϴ�.
                equipSlotGroups.TryGetValue(equipmentItem.equipType, out slotsToFill);
            }
            else
            {
                // ��Ÿ �������� ItemType�� ���� �׷��� ã���ϴ�.
                itemSlotGroups.TryGetValue(itemData.itemSO.itemType, out slotsToFill);
            }

            if (slotsToFill != null)
            {
                // ���� �׷� ���� ����ִ� ������ ã�� �������� ä��ϴ�.
                for (int i = 0; i < slotsToFill.Count; i++)
                {
                    if (slotsToFill[i].GetItem() == null)
                    {
                        slotsToFill[i].UpdateSlot(itemData);
                        break; // �������� ä������ ���� ���������� �Ѿ�ϴ�.
                    }
                }
            }
        }
    }

    /// <summary>
    /// PlayerEquipmentManager�� onEquipmentChanged �̺�Ʈ�� �߻��ϸ� ȣ��˴ϴ�.
    /// ��� �г��� UI�� �����մϴ�.
    /// </summary>
    public void RefreshEquipmentUI()
    {
        if (playerCharacter == null || playerCharacter.playerEquipmentManager == null)
        {
            Debug.LogError("PlayerCharacter �Ǵ� PlayerEquipmentManager�� �����Ǿ����ϴ�.");
            return;
        }
        UpdateEquipmentUI(playerCharacter.playerEquipmentManager.GetEquippedItems());
    }

    /// <summary>
    /// �����Ϳ��� �Ҵ��� ���Ե��� ������� ��ųʸ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeSlotGroups()
    {
        // �Ϲ� ������ ���� �׷�ȭ
        itemSlotGroups.Add(ItemType.Consumable, consumableSlots);
        itemSlotGroups.Add(ItemType.Material, materialSlots);
        itemSlotGroups.Add(ItemType.Quest, questSlots);
        itemSlotGroups.Add(ItemType.Special, specialSlots);

        // ��� ������ ���� �׷�ȭ
        equipSlotGroups.Add(EquipType.Weapon, weaponInventorySlots);
        equipSlotGroups.Add(EquipType.Armor, armorInventorySlots);
        equipSlotGroups.Add(EquipType.Accessory, accessoryInventorySlots);
    }

    /// <summary>
    /// ���� ��� ���� �����Ϳ� ����Ͽ� ��� ���� ���� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="equippedItems">������Ʈ�� ��� ����Դϴ�.</param>
    private void UpdateEquipmentUI(Dictionary<EquipSlot, EquipmentItemSO> equippedItems)
    {
        // ��� ��� ������ ���� ���ϴ�.
        foreach (var slot in equippedSlots)
        {
            slot.UpdateSlot(null);
        }

        // ������ ������ �����ͷ� ������ �����մϴ�.
        foreach (var equippedItemPair in equippedItems)
        {
            foreach (var slot in equippedSlots)
            {
                if (slot.equipSlotType == equippedItemPair.Key)
                {
                    slot.UpdateSlot(equippedItemPair.Value);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ��� �κ��丮 ������ �ʱ�ȭ(���)�մϴ�.
    /// ItemSlotUI.UpdateSlot()�� �´� ���·� ������Ʈ�մϴ�.
    /// </summary>
    private void ClearAllInventorySlots()
    {
        // ��� ItemSlotUI ����Ʈ�� ��ȸ�ϸ� ������ ���ϴ�.
        foreach (var slotList in itemSlotGroups.Values)
        {
            foreach (var slot in slotList)
            {
                slot.UpdateSlot(new ItemData(null, 0));
            }
        }
        foreach (var slotList in equipSlotGroups.Values)
        {
            foreach (var slot in slotList)
            {
                slot.UpdateSlot(new ItemData(null, 0));
            }
        }
    }

    /// <summary>
    /// ������ ������ Ȯ��â�� Ȱ��ȭ�ϰ� ������ ������ �����մϴ�.
    /// </summary>
    /// <param name="item">���� �������� ������ (BaseItemSO)</param>
    /// <param name="count">���� �������� ����</param>
    public void ShowDiscardConfirmPanel(BaseItemSO item, int count)
    {
        // 1. ������ Ÿ�Կ� ���� ��ǥ Ȯ��â�� �����մϴ�.
        GameObject targetPanel = null;
        if (item is EquipmentItemSO)
        {
            // ��� �������� ���, ��� ������ Ȯ��â�� �����մϴ�.
            targetPanel = equipDiscardConfirmPanel;
        }
        else
        {
            // �� ���� �Ϲ� �������� ���, �Ϲ� ������ ������ Ȯ��â�� �����մϴ�.
            targetPanel = itemDiscardConfirmPanel;
        }

        // 2. ��ǥ Ȯ��â�� ����� �Ҵ�Ǿ����� Ȯ���մϴ�.
        if (targetPanel != null)
        {
            // 3. �ش� Ȯ��â ������Ʈ�� Ȱ��ȭ�մϴ�.
            targetPanel.SetActive(true);

            // 4. Ȯ��â ��ũ��Ʈ(ConfirmPanel.cs)�� ã�� �ʱ�ȭ �޼��带 ȣ���մϴ�.
            ConfirmPanel confirmPanel = targetPanel.GetComponent<ConfirmPanel>();
            if (confirmPanel != null)
            {
                // Ȯ��â�� ������ ������ ������ �����մϴ�.
                confirmPanel.Initialize(item, count);
            }
        }
    }
}