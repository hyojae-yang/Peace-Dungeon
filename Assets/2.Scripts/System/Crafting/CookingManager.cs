// 파일명: CookingManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// NPC에게 요리 기능을 부여하는 컴포넌트입니다.
/// INPCFunction 인터페이스를 구현하여 NPC 상호작용 시스템에 통합됩니다.
/// SOLID: 개방-폐쇄 원칙 (기존 NPC 스크립트 수정 없이 기능 추가)
/// </summary>
public class CookingManager : MonoBehaviour, INPCFunction
{
    // === 싱글턴 인스턴스 ===
    // CookingManager의 싱글턴 인스턴스 (UI와의 연결을 위해 추가)
    public static CookingManager Instance { get; private set; }

    // 현재 플레이어 인벤토리 접근을 위한 참조
    private InventoryManager inventoryManager;

    // === INPCFunction 인터페이스 구현 ===

    [Header("Cooking Data")]
    [Tooltip("이 NPC가 제공할 요리 레시피 목록을 담은 ScriptableObject입니다.")]
    [SerializeField]
    private CookingDataSO cookingData;

    [Header("Cooking Result")]
    [Tooltip("레시피를 찾지 못했을 때 지급할 실패작 아이템입니다.")]
    [SerializeField]
    private BaseItemSO failResultItem;

    /// <summary>
    /// INPCFunction 인터페이스의 요구사항: UI 버튼에 표시될 이름을 반환합니다.
    /// </summary>
    public string FunctionButtonName
    {
        get { return "요리하기"; }
    }

    /// <summary>
    /// INPCFunction 인터페이스의 요구사항: 버튼이 클릭되었을 때 호출될 함수입니다.
    /// 이 메서드는 요리 UI를 여는 로직을 담당하게 될 것입니다.
    /// </summary>
    public void ExecuteFunction()
    {
        if (CookingUIManager.Instance != null && this.cookingData != null)
        {
            // 이제 null 대신 할당된 cookingData를 전달합니다.
            CookingUIManager.Instance.ShowCookingUI(this.cookingData);
        }
        else
        {
            Debug.LogError("CookingUIManager 인스턴스를 찾을 수 없습니다. 요리 UI를 열 수 없습니다.");
        }
    }

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // 1. CookingManager 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // 한 씬에 여러 CookingManager가 있을 경우를 대비
            Debug.LogWarning("씬에 이미 다른 CookingManager 인스턴스가 존재합니다.");
        }

        // 2. NPCManager에 스스로 등록
        // NPCManager의 RegisterSpecialFunction을 호출하여 요리 기능을 등록합니다.
        NPC npc = GetComponentInParent<NPC>();
        if (npc != null && NPCManager.Instance != null)
        {
            NPCManager.Instance.RegisterSpecialFunction(npc.Data.npcName, this);
        }
        else
        {
            Debug.LogError("NPC 또는 NPCManager 인스턴스를 찾을 수 없습니다. 요리 기능 등록에 실패했습니다.");
        }
    }

    private void Start()
    {
        // 인벤토리 매니저 참조 (다른 시스템이 모두 초기화된 후 접근)
        if (PlayerCharacter.Instance != null)
        {
            inventoryManager = PlayerCharacter.Instance.inventoryManager;
        }
    }

    // === 요리 기능 메서드 ===

    /// <summary>
    /// 냄비에 투입된 재료 목록을 기반으로 요리를 시도합니다.
    /// 요리 성공/실패 여부와 관계없이 재료가 있다면 소모를 시도하고 결과물을 지급합니다.
    /// *주의: 투입된 모든 재료는 수량 1개로 고정되어 처리됩니다.
    /// </summary>
    /// <param name="ingredients">플레이어가 냄비에 투입한 ItemData 목록입니다.</param>
    public bool TryCraft(List<ItemData> ingredients)
    {
        if (inventoryManager == null)
        {
            Debug.LogError("[CookingManager] InventoryManager를 찾을 수 없어 요리를 진행할 수 없습니다.");
            return false;
        }

        // 요리는 최소한 하나의 재료를 필요로 하는 행위이므로, 빈 목록은 유효하지 않습니다.
        if (ingredients == null || ingredients.Count == 0)
        {
            Debug.LogWarning("투입된 재료가 없습니다. 요리를 시도할 수 없습니다.");
            return false;
        }

        // 1. 투입된 재료 목록을 BaseItemSO 리스트로 변환합니다. (수량은 1개로 고정됨을 가정)
        List<BaseItemSO> currentIngredientSOs = ingredients.Select(itemData => itemData.itemSO).ToList();

        // 2. 투입된 재료 목록과 정확히 일치하는 레시피를 찾습니다.
        RecipeSO matchedRecipe = FindMatchingRecipe(currentIngredientSOs);

        // 3. 인벤토리에서 재료를 소모합니다. (모든 재료를 1개씩 소모)
        bool allIngredientsRemoved = ConsumeIngredients(currentIngredientSOs);

        if (allIngredientsRemoved)
        {
            // 최종적으로 플레이어에게 지급할 결과 아이템을 담을 변수입니다.
            BaseItemSO resultItem = null;

            // 4. 일치하는 레시피를 찾았는지 확인하고 결과 아이템을 결정합니다.
            if (matchedRecipe != null)
            {
                // 레시피를 찾았다면, 레시피의 결과 아이템을 가져옵니다.
                resultItem = matchedRecipe.resultItem;

                // 5. [핵심 추가 로직] 레시피 발견 상태 업데이트
                // 요리에 성공했으므로, 이 레시피를 발견 상태로 저장합니다.
                if (RecipeDiscoveryManager.Instance != null)
                {
                    RecipeDiscoveryManager.Instance.DiscoverRecipe(matchedRecipe.recipeID);
                }
                //요리성공 사운드
            }
            else
            {
                // 레시피를 찾지 못했다면, 실패작 아이템을 가져옵니다.
                resultItem = failResultItem;
                Debug.LogWarning("[Crafting Failure] 일치하는 레시피를 찾지 못했습니다. 실패작을 만들었습니다.");
                //요리실패 사운드
            }

            // 6. 결과 아이템을 인벤토리에 추가합니다.
            inventoryManager.AddItem(resultItem, 1);

            // 7. UI 초기화 및 갱신
            if (CookingUIManager.Instance != null)
            {
                CookingUIManager.Instance.ResetCookingIngredientUI();
                CookingUIManager.Instance.UpdateInventoryUI();

                // **[요청 사항 반영]** 요리 성공 직후 레시피 목록 UI를 갱신하여 해금된 레시피를 즉시 표시합니다.
                CookingUIManager.Instance.RefreshRecipeList();
            }

            return true;
        }
        else
        {
            Debug.LogWarning("재료가 부족하여 요리를 만들 수 없습니다.");
            return false;
        }
    }

    /// <summary>
    /// 냄비에 투입된 BaseItemSO 리스트를 기반으로 인벤토리에서 재료를 소모하는 단일 책임을 가집니다.
    /// *주의: 투입된 BaseItemSO 목록의 각 아이템은 1개씩 소모됩니다.
    /// </summary>
    /// <param name="ingredients">냄비에 투입된 BaseItemSO 목록</param>
    /// <returns>모든 재료가 성공적으로 제거되었으면 true, 하나라도 실패하면 false</returns>
    private bool ConsumeIngredients(List<BaseItemSO> ingredients)
    {
        // 1. 재료를 ItemID와 수량별로 그룹화합니다. (중복 재료가 투입된 경우를 대비)
        var groupedIngredients = ingredients
            .GroupBy(item => item.itemID)
            .Select(group => new
            {
                itemSO = group.First(), // BaseItemSO 참조
                totalCount = group.Count() // 투입된 횟수 = 소모할 수량
            })
            .ToList();

        // 2. 모든 재료를 인벤토리에서 제거합니다.
        foreach (var ingredient in groupedIngredients)
        {
            // InventoryManager의 RemoveItem(BaseItemSO, int)를 사용하여 소모 로직을 위임합니다.
            if (!inventoryManager.RemoveItem(ingredient.itemSO, ingredient.totalCount))
            {
                Debug.LogWarning($"[Crafting Error] 재료 소모 실패: {ingredient.itemSO.itemName} x{ingredient.totalCount} (재료 부족)");
                return false;
            }
        }

        return true; // 모든 재료 소모 성공
    }


    /// <summary>
    /// 투입된 재료 목록과 일치하는 레시피를 찾아 반환합니다.
    /// **수량 1개 고정:** 투입된 재료의 종류와 개수만 검사하며, 수량은 항상 1개로 가정합니다.
    /// </summary>
    /// <param name="ingredientList">투입된 BaseItemSO의 리스트입니다.</param>
    /// <returns>일치하는 레시피 SO, 없으면 null을 반환합니다.</returns>
    private RecipeSO FindMatchingRecipe(List<BaseItemSO> ingredientList)
    {
        // 1. 투입된 재료를 아이템ID 기준으로 정렬합니다. (순서 불변성 확보)
        var inputIDs = ingredientList.Select(item => item.itemID).OrderBy(id => id).ToList();

        // 2. 레시피 목록을 하나씩 순회하며 일치하는 레시피를 찾습니다.
        foreach (var recipe in cookingData.recipes)
        {
            // 2-1. 레시피의 재료 목록도 아이템 ID 기준으로 정렬합니다.
            var recipeIngredientIDs = recipe.ingredients.Select(i => i.item.itemID).OrderBy(id => id).ToList();

            // 2-2. 개수가 다르면 바로 통과
            if (recipeIngredientIDs.Count != inputIDs.Count)
            {
                continue;
            }

            // **수량 1개 고정:** 모든 레시피 재료의 수량이 1개인지 확인 (선택 사항이지만 일관성을 위해 추가)
            bool isRecipeValid = recipe.ingredients.All(i => i.quantity == 1);
            if (!isRecipeValid)
            {
                Debug.LogError($"[CookingManager] 레시피 ID {recipe.recipeID}의 재료 수량이 1개가 아닙니다. 시스템 정책 위반!");
                continue;
            }

            // 2-3. 정렬된 ID 목록을 비교하여 일치 여부를 확인합니다.
            bool isMatch = true;
            for (int i = 0; i < inputIDs.Count; i++)
            {
                if (inputIDs[i] != recipeIngredientIDs[i])
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                return recipe;
            }
        }

        // 반복문을 모두 돌았는데도 일치하는 레시피가 없으면 null을 반환합니다.
        return null;
    }
}