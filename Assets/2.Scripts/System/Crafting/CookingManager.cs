// 파일명: CookingManager.cs (수정된 전문)
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
    public BaseItemSO failResultItem; // **[참고]** UI에서 참조할 수 있도록 public으로 유지

    /// <summary>
    /// 요리 결과를 담는 데이터 구조체입니다.
    /// </summary>
    public class CookingResultData
    {
        // 요리 결과 아이템 (성공작 or 실패작)
        public BaseItemSO resultItem;
        // 재료 소모 성공 여부
        public bool isIngredientsConsumed;
    }

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
    /// **[핵심 수정]** 이 메서드는 재료 소모를 시도하고, 결과를 CookingUIManager에게 전달만 합니다.
    /// 아이템 지급 및 UI 갱신 로직은 CookingUIManager의 코루틴으로 이동했습니다.
    /// </summary>
    /// <param name="ingredients">플레이어가 냄비에 투입한 ItemData 목록입니다.</param>
    public bool TryCraft(List<ItemData> ingredients)
    {
        if (inventoryManager == null)
        {
            Debug.LogError("[CookingManager] InventoryManager를 찾을 수 없어 요리를 진행할 수 없습니다.");
            return false;
        }

        if (ingredients == null || ingredients.Count == 0)
        {
            Debug.LogWarning("투입된 재료가 없습니다. 요리를 시도할 수 없습니다.");
            return false;
        }

        // 1. 재료 소모 및 결과 아이템 결정을 위임하고 결과 데이터를 받습니다. (재료 소모는 여기서 완료됨)
        CookingResultData resultData = GetCookingResult(ingredients);

        // 2. 재료 소모가 실패했다면 요리 중단.
        if (!resultData.isIngredientsConsumed)
        {
            Debug.LogWarning("재료가 부족하여 요리를 만들 수 없습니다. (재료 소모 실패)");
            return false;
        }

        // 3. UI Manager에게 요리 결과를 전달하고 코루틴 실행을 위임합니다.
        if (CookingUIManager.Instance != null)
        {
            // CookingUIManager의 새 메서드를 호출하여 코루틴을 시작합니다.
            CookingUIManager.Instance.StartCookingProcessCoroutine(resultData);
            return true;
        }

        // UI Manager가 없으면 실패
        return false;
    }

    /// <summary>
    /// 냄비에 투입된 재료를 기반으로 요리 결과를 계산하고 재료를 소모하는 핵심 로직입니다.
    /// </summary>
    /// <param name="ingredients">플레이어가 냄비에 투입한 ItemData 목록입니다.</param>
    /// <returns>CookingResultData 객체. isIngredientsConsumed가 false면 재료 소모에 실패한 것임.</returns>
    private CookingResultData GetCookingResult(List<ItemData> ingredients)
    {
        CookingResultData resultData = new CookingResultData();

        // 1. 투입된 재료 목록을 BaseItemSO 리스트로 변환합니다.
        List<BaseItemSO> currentIngredientSOs = ingredients.Select(itemData => itemData.itemSO).ToList();

        // 2. 투입된 재료 목록과 정확히 일치하는 레시피를 찾습니다.
        RecipeSO matchedRecipe = FindMatchingRecipe(currentIngredientSOs);

        // 3. 인벤토리에서 재료를 소모합니다.
        bool allIngredientsRemoved = ConsumeIngredients(currentIngredientSOs);
        resultData.isIngredientsConsumed = allIngredientsRemoved;

        if (!allIngredientsRemoved)
        {
            // 재료 소모에 실패했으면, 결과 아이템은 null로 두고 반환합니다.
            resultData.resultItem = null;
            return resultData;
        }

        // 4. 일치하는 레시피를 찾았는지 확인하고 결과 아이템을 결정합니다.
        if (matchedRecipe != null)
        {
            // 레시피를 찾았다면, 레시피의 결과 아이템을 가져옵니다.
            resultData.resultItem = matchedRecipe.resultItem;

            // 5. [핵심 추가 로직] 레시피 발견 상태 업데이트
            if (RecipeDiscoveryManager.Instance != null)
            {
                RecipeDiscoveryManager.Instance.DiscoverRecipe(matchedRecipe.recipeID);
            }
            //요리성공 사운드
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Good_Cooking);
            }
        }
        else
        {
            // 레시피를 찾지 못했다면, 실패작 아이템을 가져옵니다.
            resultData.resultItem = failResultItem;
            //Debug.LogWarning("[Crafting Failure] 일치하는 레시피를 찾지 못했습니다. 실패작을 만들었습니다.");
            //요리실패 사운드
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.Bad_Cooking);
            }
        }

        // **[기존 로직 삭제]** 아이템 지급 및 UI 갱신 로직 (6, 7번)을 삭제했습니다.

        return resultData;
    }


    /// <summary>
    /// 냄비에 투입된 BaseItemSO 리스트를 기반으로 인벤토리에서 재료를 소모하는 단일 책임을 가집니다.
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
    /// 투입된 재료 목록과 일치하는 레시피를 찾아 반환합니다. (로직 유지)
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