// ���ϸ�: CookingManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// NPC���� �丮 ����� �ο��ϴ� ������Ʈ�Դϴ�.
/// INPCFunction �������̽��� �����Ͽ� NPC ��ȣ�ۿ� �ý��ۿ� ���յ˴ϴ�.
/// SOLID: ����-��� ��Ģ (���� NPC ��ũ��Ʈ ���� ���� ��� �߰�)
/// </summary>
public class CookingManager : MonoBehaviour, INPCFunction
{
    // === �̱��� �ν��Ͻ� ===
    // CookingManager�� �̱��� �ν��Ͻ� (UI���� ������ ���� �߰�)
    public static CookingManager Instance { get; private set; }

    // === INPCFunction �������̽� ���� ===

    [Header("Cooking Data")]
    [Tooltip("�� NPC�� ������ �丮 ������ ����� ���� ScriptableObject�Դϴ�.")]
    [SerializeField]
    private CookingDataSO cookingData;
    [Header("Cooking Result")]
    [Tooltip("�����Ǹ� ã�� ������ �� ������ ������ �������Դϴ�.")]
    [SerializeField]
    private BaseItemSO failResultItem;
    /// <summary>
    /// INPCFunction �������̽��� �䱸����: UI ��ư�� ǥ�õ� �̸��� ��ȯ�մϴ�.
    /// </summary>
    public string FunctionButtonName
    {
        get { return "�丮�ϱ�"; }
    }

    /// <summary>
    /// INPCFunction �������̽��� �䱸����: ��ư�� Ŭ���Ǿ��� �� ȣ��� �Լ��Դϴ�.
    /// �� �޼���� �丮 UI�� ���� ������ ����ϰ� �� ���Դϴ�.
    /// </summary>
    public void ExecuteFunction()
    {
        if (CookingUIManager.Instance != null && this.cookingData != null)
        {
            // ���� null ��� �Ҵ�� cookingData�� �����մϴ�.
            CookingUIManager.Instance.ShowCookingUI(this.cookingData);
        }
        else
        {
            Debug.LogError("CookingUIManager �ν��Ͻ��� ã�� �� �����ϴ�. �丮 UI�� �� �� �����ϴ�.");
        }
    }

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        // 1. CookingManager �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // �� ���� ���� CookingManager�� ���� ��츦 ���
            Debug.LogWarning("���� �̹� �ٸ� CookingManager �ν��Ͻ��� �����մϴ�.");
        }

        // 2. NPCManager�� ������ ���
        // NPCManager�� RegisterSpecialFunction�� ȣ���Ͽ� �丮 ����� ����մϴ�.
        NPC npc = GetComponentInParent<NPC>();
        if (npc != null && NPCManager.Instance != null)
        {
            NPCManager.Instance.RegisterSpecialFunction(npc.Data.npcName, this);
        }
        else
        {
            Debug.LogError("NPC �Ǵ� NPCManager �ν��Ͻ��� ã�� �� �����ϴ�. �丮 ��� ��Ͽ� �����߽��ϴ�.");
        }
    }
    // === �丮 ��� �޼��� ===
    /// <summary>
    /// ���Ե� ��� ��ϰ� ��ġ�ϴ� �����Ǹ� ã�� ��ȯ�մϴ�.
    /// SOLID: ���� å�� ��Ģ (�����Ǹ� ã�� ���Ҹ� ����).
    /// </summary>
    /// <param name="ingredientList">���Ե� ����� BaseItemSO ����Ʈ�Դϴ�.</param>
    /// <returns>��ġ�ϴ� ������ SO, ������ null�� ��ȯ�մϴ�.</returns>
    private RecipeSO FindMatchingRecipe(List<BaseItemSO> ingredientList)
    {
        // 1. ���Ե� ��Ḧ ������ID �������� �����Ͽ� ������ ������� �� �����ϰ� �մϴ�.
        ingredientList.Sort((a, b) => a.itemID.CompareTo(b.itemID));

        // 2. ������ ����� �ϳ��� ��ȸ�ϸ� ��ġ�ϴ� �����Ǹ� ã���ϴ�.
        foreach (var recipe in cookingData.recipes)
        {
            // ���Ե� ����� ������ �������� ��� ������ ������ ���� Ȯ���մϴ�.
            if (recipe.ingredients.Count == ingredientList.Count)
            {
                // ��� ������ ���ٸ�, �� ����� ������ ID�� ������ ���մϴ�.
                bool isMatch = true;
                // **������ �κ�:** �������� ��� ��ϵ� �����Ͽ� �� �غ� �մϴ�.
                var recipeIngredientIDs = recipe.ingredients.Select(i => i.item.itemID).OrderBy(id => id).ToList();

                for (int i = 0; i < ingredientList.Count; i++)
                {
                    if (ingredientList[i].itemID != recipeIngredientIDs[i])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    // **�̰����� ��ġ�ϴ� �����Ǹ� ã�ҽ��ϴ�!**
                    // ���� �ܰ迡�� �� �ڵ带 TryCraft �޼��忡 ������ ���Դϴ�.
                    return recipe;
                }
            }
        }

        // �ݺ����� ��� ���Ҵµ��� ��ġ�ϴ� �����ǰ� ������ null�� ��ȯ�մϴ�.
        return null;
    }
    /// <summary>
    /// ���� ���Ե� ��� ����� ������� �丮�� �õ��մϴ�.
    /// </summary>
    /// <param name="ingredients">�÷��̾ ���� ������ ��� ����Դϴ�.</param>
    public bool TryCraft(List<ItemData> ingredients)
    {
        // 1. ���Ե� ��� ����� BaseItemSO ����Ʈ�� ��ȯ�մϴ�.
        List<BaseItemSO> currentIngredientSOs = ingredients.Select(itemData => itemData.itemSO).ToList();

        // 2. ���Ե� ��� ��ϰ� ��Ȯ�� ��ġ�ϴ� �����Ǹ� ã���ϴ�.
        // �� �� ���� FindMatchingRecipe �޼��带 ȣ���Ͽ� �����Ǹ� ã���ϴ�.
        RecipeSO matchedRecipe = FindMatchingRecipe(currentIngredientSOs);

        // 3. �κ��丮���� ��Ḧ �Ҹ��մϴ�.
        // �� �ܰ迡���� ��ᰡ ���������� ���ŵǴ����� Ȯ���մϴ�.
        bool allIngredientsRemoved = true;
        foreach (var itemSO in currentIngredientSOs)
        {
            // PlayerCharacter�� InventoryManager�� ���� �������� �����մϴ�.
            // ����� �� �������� �� ���� �Ҹ�ȴٰ� �����մϴ�.
            bool removed = PlayerCharacter.Instance.inventoryManager.RemoveItem(itemSO, 1);
            if (!removed)
            {
                // ��ᰡ �����Ͽ� ���ſ� �����ϸ� �÷��׸� false�� �ٲٰ� �ݺ����� �ߴ��մϴ�.
                allIngredientsRemoved = false;
                break;
            }
        }

        if (allIngredientsRemoved)
        {
            // ���������� �÷��̾�� ������ ��� �������� ���� �����Դϴ�.
            BaseItemSO resultItem = null;
            // 2. ��ġ�ϴ� �����Ǹ� ã�Ҵ��� Ȯ���ϰ� ��� �������� �����մϴ�.
            if (matchedRecipe != null)
            {
                // �����Ǹ� ã�Ҵٸ�, �������� ��� �������� �����ɴϴ�.
                resultItem = matchedRecipe.resultItem;
            }
            else
            {
                // �����Ǹ� ã�� ���ߴٸ�, ������ �������� �����ɴϴ�.
                resultItem = failResultItem;
            }
            // **�߰��� �� ��:** resultItem�� �κ��丮�� �߰��մϴ�.
            if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
            {
                PlayerCharacter.Instance.inventoryManager.AddItem(resultItem, 1);
            }
            else
            {
                Debug.LogError("�÷��̾� �Ǵ� �κ��丮 �Ŵ����� ã�� �� �����ϴ�. ������ �߰��� �����߽��ϴ�.");
            }

            // �߰��� �κ�: ��� �Ҹ� �����ϸ� ���� UI�� �ʱ�ȭ�մϴ�.
            if (CookingUIManager.Instance != null)
            {
                CookingUIManager.Instance.ResetCookingIngredientUI();
            }
            

            // �丮 ���� �� UI�� �����ؾ� �մϴ�.
            // CookingUIManager.Instance.UpdateInventoryUI();
            return true;
        }
        else
        {
            Debug.LogWarning("��ᰡ �����Ͽ� �丮�� ���� �� �����ϴ�.");
            return false;
        }
    }

}