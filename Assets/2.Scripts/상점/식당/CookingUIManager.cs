// ���ϸ�: CookingUIManager.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// �丮 �ý����� UI�� �����ϴ� �̱��� ��ũ��Ʈ�Դϴ�.
/// �丮â�� ���� ������, ������ ����� ǥ���ϴ� ������ ����մϴ�.
/// </summary>
public class CookingUIManager : MonoBehaviour
{
    // === �̱��� �ν��Ͻ� ===
    // CookingUIManager�� ������ �ν��Ͻ��� �����ϴ� ���� �Ӽ��Դϴ�.
    public static CookingUIManager Instance { get; private set; }

    // === UI ���� ===
    [Header("UI References")]
    [Tooltip("�丮 UI ��ü�� ��� �ִ� ���� ������Ʈ�Դϴ�.")]
    [SerializeField]
    private GameObject cookingUIPanel;
    [Header("Sub Panels")]
    [Tooltip("���ʿ� ��ġ�� ������ ��� �г��Դϴ�.")]
    [SerializeField]
    private GameObject recipePanel;
    [Tooltip("����� ��ġ�� �丮�ϴ� �г��Դϴ�.")]
    [SerializeField]
    private GameObject cookingActionPanel;
    [Tooltip("�����ʿ� ��ġ�� �κ��丮 �г��Դϴ�.")]
    [SerializeField]
    private GameObject inventoryPanel;
    // �Ʒ� �ڵ带 �߰��մϴ�.
    [Header("Recipe UI")]
    [Tooltip("������ �������� �������� ������ ScrollView�� Content�Դϴ�.")]
    [SerializeField]
    private Transform recipeContent;
    [Tooltip("������ ��Ͽ� ǥ�õ� ������ �������� UI �������Դϴ�.")]
    [SerializeField]
    private GameObject recipeItemUIPrefab;
    // �Ʒ� �ڵ带 �߰��մϴ�.
    [Header("Cooking Action UI")]
    [Tooltip("�丮 ������ �����ϴ� ��ư�Դϴ�.")]
    [SerializeField]
    private Button craftButton;
    [Tooltip("���� �� ��Ḧ ǥ���ϴ� �ؽ�Ʈ���Դϴ�.")]
    [SerializeField]
    private List<TextMeshProUGUI> cookingIngredientTexts;
    // �Ʒ� �������� �߰��մϴ�.
    [Header("Inventory UI")]
    [Tooltip("�κ��丮 �������� �������� ������ ScrollView�� Content�Դϴ�.")]
    [SerializeField]
    private Transform inventoryContent;
    [Tooltip("�κ��丮 ��Ͽ� ǥ�õ� �κ��丮 ������ UI �������Դϴ�.")]
    [SerializeField]
    private GameObject inventorySlotPrefab;
    // ���� ���Ե� ��Ḧ ������ ����Ʈ�� �߰��մϴ�.
    [Header("Current Cooking Ingredients")]
    [Tooltip("���� ���� ���Ե� ��� ����Դϴ�.")]
    public List<ItemData> currentIngredients = new List<ItemData>();
    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        // �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // �ʿ�� �ּ� ����
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // �丮�ϱ� ��ư�� Ŭ�� ������ �߰�
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
        // �ʱ⿡�� UI�� ��Ȱ��ȭ�մϴ�.
        cookingUIPanel.SetActive(false);
    }

    public void ShowCookingUI(CookingDataSO data)
    {
        // 1. UI �ʱ�ȭ
        // ���� ������ �����۵� ��� ����
        foreach (Transform child in recipeContent)
        {
            Destroy(child.gameObject);
        }
        // ������ �κ�: ���� ��� ����� �ʱ�ȭ�մϴ�.
        currentIngredients.Clear();
        // ��� �丮 �г� ��� �ؽ�Ʈ �ʱ�ȭ
        foreach (var text in cookingIngredientTexts)
        {
            text.text = "-";
        }

        // UI �г��� Ȱ��ȭ�մϴ�.
        cookingUIPanel.SetActive(true);

        // ������ �κ�: �κ��丮 UI�� �����ϴ� �޼��带 ȣ���մϴ�.
        UpdateInventoryUI();

        // TODO: (���� �ܰ�) ������ ����� UI�� ������ ǥ���ϴ� ������ ���⿡ �߰��մϴ�.
        if (data != null && recipeContent != null && recipeItemUIPrefab != null)
        {
            // ������ ����Ʈ�� ��ȸ�ϸ� UI ������ ����
            foreach (var recipe in data.recipes)
            {
                GameObject recipeUIObject = Instantiate(recipeItemUIPrefab, recipeContent);
                RecipeItemUI recipeUI = recipeUIObject.GetComponent<RecipeItemUI>();
                if (recipeUI != null)
                {
                    recipeUI.SetData(recipe);
                }
            }
        }
        else
        {
            Debug.LogError("�ʿ��� UI ������Ʈ(CookingDataSO, recipeContent, recipeItemUIPrefab)�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
    // === UI �̺�Ʈ �ڵ鷯 ===
    /// <summary>
    /// �丮�ϱ� ��ư�� Ŭ���Ǿ��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    private void OnCraftButtonClicked()
    {
        if (CookingManager.Instance != null)
        {
            // ������ �κ�: int ��� ���� ���� �ִ� ��� ����� �����մϴ�.
            CookingManager.Instance.TryCraft(currentIngredients);
        }
    }
    /// <summary>
    /// �κ��丮 �����͸� �޾ƿ� UI�� ǥ���ϴ� �޼����Դϴ�.
    /// CookingManager�κ��� ȣ��˴ϴ�.
    /// </summary>
    private void UpdateInventoryUI()
    {
        // ������ ������ ������ ���Ե� ��� ���� (������ ����)
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        // InventoryManager�κ��� �÷��̾��� �κ��丮 ������ ����� �����ɴϴ�.
        List<ItemData> playerInventory = PlayerCharacter.Instance.inventoryManager.GetInventoryItems();

        // �κ��丮 �������� ��ȸ�ϸ� ���� UI ����
        foreach (var item in playerInventory)
        {
            // ��� �����۸� �丮 �г� �κ��丮�� ǥ���մϴ�.
            // ItemType�� "Ingredient"���� Ȯ���ϴ� ������ �ʿ��մϴ�.
            if (item.itemSO.itemType == ItemType.Material || item.itemSO.itemType == ItemType.Consumable)
            {
                GameObject slotUIObject = Instantiate(inventorySlotPrefab, inventoryContent);
                InventorySlotUI slotUI = slotUIObject.GetComponent<InventorySlotUI>();

                if (slotUI != null)
                {
                    slotUI.SetData(item);
                }
            }
        }
    }
    /// <summary>
    /// �������� ���� ��ӵǾ��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// CookingPotDrop ��ũ��Ʈ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="droppedItemData">���� ��ӵ� ������ �������Դϴ�.</param>
    /// <param name="droppedUIObject">��ӵ� �������� UI ������Ʈ�Դϴ�.</param>
    public void OnItemDroppedInPot(ItemData droppedItemData, GameObject droppedUIObject)
    {
        // ��ӵ� �������� ��� ���������� Ȯ���մϴ�.
        if (droppedItemData.itemSO.itemType == ItemType.Material || droppedItemData.itemSO.itemType == ItemType.Consumable)
        {
            // ���� ��Ḧ �߰��ϰ� UI�� ������Ʈ�մϴ�.
            currentIngredients.Add(droppedItemData);
            UpdateCookingIngredientUI();

            // ����� ���������Ƿ�, ������ �κ��丮 ���� UI�� ��Ȱ��ȭ�մϴ�.
            droppedUIObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("��� �����۸� ���� ���� �� �ֽ��ϴ�.");
            // ��� �������� �ƴ� ���, �巡�׵� UI�� ���� ��ġ�� �ǵ����ϴ�.
            // �� ������ OnEndDrag���� �ڵ����� ó���ǹǷ� �߰� �ڵ�� �ʿ� �����ϴ�.
        }
    }

    /// <summary>
    /// ���� ���� ���Ե� ��� ����� UI�� ǥ���մϴ�.
    /// </summary>
    private void UpdateCookingIngredientUI()
    {
        // ���� �ؽ�Ʈ �ʱ�ȭ
        foreach (var text in cookingIngredientTexts)
        {
            text.text = "-";
        }

        // ���Ե� ��Ḧ ������� �ؽ�Ʈ�� ǥ���մϴ�.
        for (int i = 0; i < currentIngredients.Count && i < cookingIngredientTexts.Count; i++)
        {
            cookingIngredientTexts[i].text = currentIngredients[i].itemSO.itemName;
        }
    }
    /// <summary>
    /// ���� ���Ե� ��� ��ϰ� UI�� ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void ResetCookingIngredientUI()
    {
        // ���� ���Ե� ��� ����Ʈ�� ���ϴ�.
        currentIngredients.Clear();

        // ���� �Ʒ��� ��� �ؽ�Ʈ�� ��� "-"�� �ʱ�ȭ�մϴ�.
        foreach (var text in cookingIngredientTexts)
        {
            text.text = "-";
        }
    }
    /// <summary>
    /// �丮 UI �г��� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void HideCookingUI()
    {
        cookingUIPanel.SetActive(false);
    }
}