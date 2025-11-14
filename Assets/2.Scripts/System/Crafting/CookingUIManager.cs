// 파일명: CookingUIManager.cs (수정된 전문)
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections; // 코루틴 사용을 위해 추가

/// <summary>
/// 요리 시스템의 UI를 관리하는 싱글턴 스크립트입니다.
/// 요리창을 열고 닫으며, 레시피 목록을 표시하는 역할을 담당합니다.
/// SOLID: 단일 책임 원칙 (UI 관리), 개방-폐쇄 원칙 (로직을 함수로 분리)
/// </summary>
public class CookingUIManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    // CookingUIManager의 유일한 인스턴스를 저장하는 정적 속성입니다.
    public static CookingUIManager Instance { get; private set; }

    // === UI 참조 ===
    [Header("UI References")]
    [Tooltip("요리 UI 전체를 담고 있는 게임 오브젝트입니다.")]
    [SerializeField]
    private GameObject cookingUIPanel;

    [Header("Sub Panels")]
    [Tooltip("왼쪽에 위치한 레시피 목록 패널입니다.")]
    [SerializeField]
    private GameObject recipePanel;
    [Tooltip("가운데에 위치한 요리하는 패널입니다.")]
    [SerializeField]
    private GameObject cookingActionPanel;
    [Tooltip("오른쪽에 위치한 인벤토리 패널입니다.")]
    [SerializeField]
    private GameObject inventoryPanel;

    // === [추가된 변수] 요리 진행 UI 참조 ===
    [Header("Cooking Process UI")]
    [Tooltip("요리 과정(진행도 및 결과)을 표시하는 UI 패널입니다.")]
    [SerializeField]
    private GameObject processPanel; // 결과창 패널 (고객님 요청)
    [Tooltip("요리 과정 상태를 표시하는 텍스트입니다. (예: '요리중...', '성공!')")]
    [SerializeField]
    private TextMeshProUGUI processText; // 요리과정 텍스트 (고객님 요청)
    [Tooltip("요리 진행도를 표시하는 슬라이더입니다.")]
    [SerializeField]
    private Slider processSlider; // 슬라이더 (고객님 요청)
    // ======================================

    [Header("Recipe UI")]
    [Tooltip("레시피 아이템을 동적으로 생성할 ScrollView의 Content입니다.")]
    [SerializeField]
    private Transform recipeContent;
    [Tooltip("레시피 목록에 표시될 레시피 아이템의 UI 프리팹입니다.")]
    [SerializeField]
    private GameObject recipeItemUIPrefab;

    [Header("Cooking Action UI")]
    [Tooltip("요리 제작을 시작하는 버튼입니다.")]
    [SerializeField]
    private Button craftButton;
    [Tooltip("냄비에 들어간 재료를 표시하는 텍스트들입니다.")]
    [SerializeField]
    private List<TextMeshProUGUI> cookingIngredientTexts;

    [Header("Inventory UI")]
    [Tooltip("인벤토리 아이템을 동적으로 생성할 ScrollView의 Content입니다.")]
    [SerializeField]
    private Transform inventoryContent;
    [Tooltip("인벤토리 목록에 표시될 인벤토리 슬롯의 UI 프리팹입니다.")]
    [SerializeField]
    private GameObject inventorySlotPrefab;

    // 냄비에 투입된 재료를 저장할 리스트를 추가합니다.
    [Header("Current Cooking Ingredients")]
    [Tooltip("현재 냄비에 투입된 재료 목록입니다.")]
    public List<ItemData> currentIngredients = new List<ItemData>();

    // === 내부 상태 ===
    // 현재 활성화된 NPC의 CookingDataSO를 저장하여 레시피를 재사용할 수 있도록 합니다.
    private CookingDataSO currentCookingData;

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 요리하기 버튼에 클릭 리스너 추가
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(MainSceneManager.Instance.PlayButtonSFXSafely);
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }

        // 초기에는 UI를 비활성화합니다.
        cookingUIPanel.SetActive(false);
    }

    /// <summary>
    /// 요리 UI를 열고 초기화하며, 레시피 목록을 생성합니다.
    /// </summary>
    /// <param name="data">이 NPC가 가진 CookingDataSO입니다.</param>
    public void ShowCookingUI(CookingDataSO data)
    {
        // NPC의 CookingData를 저장합니다.
        currentCookingData = data;

        // 1. UI 초기화
        InitializeUI();

        // 2. 패널 활성화 및 플레이어 컨트롤 비활성화
        cookingUIPanel.SetActive(true);
        if (PlayerCharacter.Instance != null)
        { PlayerCharacter.Instance.playerController.enabled = false; }

        // 3. UI 갱신
        UpdateInventoryUI();
        UpdateRecipeListUI(); // 레시피 목록 생성 로직 분리

        // 4. 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Restaurant_Enter, 0.5f);
        }
    }

    /// <summary>
    /// 외부에서 호출되어 레시피 목록 UI를 즉시 갱신합니다.
    /// CookingManager가 요리 성공 후 해금된 레시피를 바로 표시하기 위해 사용합니다.
    /// </summary>
    public void RefreshRecipeList()
    {
        // 레시피 목록 갱신 로직을 호출합니다.
        UpdateRecipeListUI();
    }

    /// <summary>
    /// 레시피 목록 UI를 동적으로 생성하고, 발견 상태를 반영합니다.
    /// SOLID: 단일 책임 원칙 (레시피 리스트 생성).
    /// </summary>
    private void UpdateRecipeListUI()
    {
        // 기존 레시피 아이템들 모두 제거
        foreach (Transform child in recipeContent)
        {
            Destroy(child.gameObject);
        }

        if (currentCookingData == null || recipeContent == null || recipeItemUIPrefab == null)
        {
            Debug.LogError("레시피 목록을 생성하는 데 필요한 데이터/프리팹이 할당되지 않았습니다.");
            return;
        }

        // 레시피 리스트를 순회하며 UI 아이템 생성
        foreach (var recipe in currentCookingData.recipes)
        {
            GameObject recipeUIObject = Instantiate(recipeItemUIPrefab, recipeContent);
            RecipeItemUI recipeUI = recipeUIObject.GetComponent<RecipeItemUI>();

            if (recipeUI != null)
            {
                // **[핵심 수정 부분]** 레시피 발견 여부를 조회합니다.
                bool isDiscovered = false;
                if (RecipeDiscoveryManager.Instance != null)
                {
                    isDiscovered = RecipeDiscoveryManager.Instance.IsDiscovered(recipe.recipeID);
                }

                // 발견 여부 플래그와 함께 SetData를 호출합니다.
                recipeUI.SetData(recipe, isDiscovered);
            }
        }
    }

    /// <summary>
    /// UI를 열기 전 상태를 초기화합니다.
    /// </summary>
    private void InitializeUI()
    {
        // 수정된 부분: 냄비 재료 목록을 초기화합니다.
        currentIngredients.Clear();

        // 가운데 요리 패널 재료 텍스트 초기화
        foreach (var text in cookingIngredientTexts)
        {
            text.text = "-";
        }
    }

    // === UI 이벤트 핸들러 ===

    /// <summary>
    /// 요리하기 버튼이 클릭되었을 때 호출되는 메서드입니다.
    /// **[핵심 수정]** CookingManager에게 요리 시도를 위임합니다.
    /// </summary>
    private void OnCraftButtonClicked()
    {
        // 1. 재료가 없으면 요리 시도 실패
        if (currentIngredients == null || currentIngredients.Count == 0)
        {
            Debug.LogWarning("재료가 없어 요리를 시작할 수 없습니다.");
            return;
        }

        // 2. CookingManager에게 요리 로직 실행을 요청
        if (CookingManager.Instance != null)
        {
            // TryCraft는 재료 소모를 시도하고, 성공하면 CookingUIManager.StartCookingProcessCoroutine을 호출합니다.
            bool isCraftAttempted = CookingManager.Instance.TryCraft(currentIngredients);

            if (!isCraftAttempted)
            {
                // 재료 부족 등으로 TryCraft가 false를 반환하면 UI 초기화 및 갱신만 진행
                UpdateInventoryUI();
                ResetCookingIngredientUI();
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------
    // [새로 추가된 메서드] - 코루틴 관련
    // --------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// CookingManager에서 호출되어 요리 과정 UI 애니메이션을 시작하고 결과 처리를 담당합니다.
    /// SOLID: 단일 책임 원칙 (요리 과정의 UI 및 시간 흐름 제어).
    /// </summary>
    /// <param name="resultData">요리 결과 데이터 (결과 아이템 및 재료 소모 여부)</param>
    public void StartCookingProcessCoroutine(CookingManager.CookingResultData resultData)
    {
        // 코루틴 시작
        StartCoroutine(CookingCoroutine(resultData));
    }

    /// <summary>
    /// 요리 과정 딜레이, 슬라이더 애니메이션, 결과 표시를 처리하고 최종 아이템을 지급하는 코루틴입니다.
    /// </summary>
    /// <param name="resultData">요리 결과 데이터</param>
    private IEnumerator CookingCoroutine(CookingManager.CookingResultData resultData)
    {
        // 1. 준비: 요리 시작 UI 활성화 및 초기 상태 설정
        if (processPanel != null)
        {
            processPanel.SetActive(true);

            // 초기 슬라이더 및 텍스트 설정
            if (processSlider != null)
            {
                processSlider.value = 0f;
            }
            if (processText != null)
            {
                processText.text = "요리중...";
            }
        }

        float cookTime = 3f; // 요리 과정 딜레이 시간 (3초)
        float elapsedTime = 0f;

        //요리 과정 시작 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Cooking, 0.7f);
        }
        // 2. 요리 과정 (3초 딜레이 및 슬라이더 애니메이션)
        while (elapsedTime < cookTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / cookTime;

            if (processSlider != null)
            {
                processSlider.value = progress;
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 3. 결과 텍스트 결정 및 표시
        BaseItemSO resultItem = resultData.resultItem;
        string resultName = (resultItem != null) ? resultItem.itemName : "실패작";

        // **[수정 시작]**: 결과 아이템이 CookingManager의 실패작 아이템과 동일한지 비교하여 성공/실패를 정확히 구분합니다.
        bool isSuccess = (resultItem != null && CookingManager.Instance != null && resultItem != CookingManager.Instance.failResultItem);

        if (processText != null)
        {
            if (isSuccess) // 성공작을 획득했을 때만 '성공!' 표시
            {
                processText.text = $"성공! \n({resultName} 획득)";
            }
            else if (resultItem != null) // 실패작을 획득했을 때 (failResultItem과 동일)
            {
                processText.text = $"실패... \n({resultName} 획득)";
            }
            else // 아이템을 아예 획득하지 못했을 때 (재료 소모에 실패했거나, 결과가 null일 때)
            {
                processText.text = "실패... 아무것도 획득하지 못했습니다.";
            }
        }
        // **[수정 끝]**

        // 4. 아이템 지급
        if (resultItem != null)
        {
            // *주의*: isSuccess 여부와 관계없이 resultItem이 존재하면 지급합니다.
            // 실패작(failResultItem)도 인벤토리에 지급되어야 합니다.
            if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.inventoryManager != null)
            {
                // **아이템 지급** (CookingManager에서 이리로 이동)
                PlayerCharacter.Instance.inventoryManager.AddItem(resultItem, 1);
            }
            // 레시피 목록 갱신 (성공 시에만)
            if (isSuccess) // 성공한 경우에만 레시피 목록 갱신 (발견된 레시피를 표시해야 하므로)
            {
                RefreshRecipeList();
            }
        }

        // 5. UI 갱신 및 냄비 초기화
        UpdateInventoryUI();
        ResetCookingIngredientUI();

        // 6. 결과 텍스트 표시 유지
        yield return new WaitForSeconds(1.5f);

        // 7. 요리 과정 패널 비활성화
        if (processPanel != null)
        {
            processPanel.SetActive(false);
        }
    }


    /// <summary>
    /// 인벤토리 데이터를 받아와 UI에 표시하는 메서드입니다.
    /// CookingManager로부터 호출되어 인벤토리 상태를 갱신합니다.
    /// </summary>
    public void UpdateInventoryUI()
    {
        // 기존에 생성된 아이템 슬롯들 모두 제거 (갱신을 위해)
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        // InventoryManager로부터 플레이어의 인벤토리 아이템 목록을 가져옵니다.
        // null 체크 추가
        if (PlayerCharacter.Instance == null || PlayerCharacter.Instance.inventoryManager == null)
        {
            Debug.LogError("플레이어 또는 인벤토리 매니저를 찾을 수 없습니다.");
            return;
        }

        List<ItemData> playerInventory = PlayerCharacter.Instance.inventoryManager.GetInventoryItems();

        // 인벤토리 아이템을 순회하며 슬롯 UI 생성
        foreach (var item in playerInventory)
        {
            // 재료 아이템만 요리 패널 인벤토리에 표시합니다.
            // ItemType.Material 또는 ItemType.Consumable만 표시 (재료/소모품만 요리에 사용 가능하다고 가정)
            if (item.itemSO != null && (item.itemSO.itemType == ItemType.Material || item.itemSO.itemType == ItemType.Consumable))
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
    /// 아이템이 냄비에 드롭되었을 때 호출되는 메서드입니다.
    /// CookingPotDrop 스크립트에서 호출됩니다.
    /// </summary>
    /// <param name="droppedItemData">냄비에 드롭된 아이템 데이터입니다.</param>
    /// <param name="droppedUIObject">드롭된 아이템의 UI 오브젝트입니다.</param>
    public void OnItemDroppedInPot(ItemData droppedItemData, GameObject droppedUIObject)
    {
        // 드롭된 아이템이 재료 아이템인지 확인합니다.
        if (droppedItemData.itemSO.itemType == ItemType.Material || droppedItemData.itemSO.itemType == ItemType.Consumable)
        {
            // 냄비에 재료를 추가하고 UI를 업데이트합니다.
            currentIngredients.Add(droppedItemData);
            UpdateCookingIngredientUI();

            // 드롭이 성공했으므로, 원래의 인벤토리 슬롯 UI를 비활성화합니다.
            droppedUIObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("재료 아이템만 냄비에 넣을 수 있습니다.");
        }
    }

    /// <summary>
    /// 현재 냄비에 투입된 재료 목록을 UI에 표시합니다.
    /// </summary>
    private void UpdateCookingIngredientUI()
    {
        // 기존 텍스트 초기화
        foreach (var text in cookingIngredientTexts)
        {
            text.text = "-";
        }

        // 투입된 재료를 순서대로 텍스트에 표시합니다.
        for (int i = 0; i < currentIngredients.Count && i < cookingIngredientTexts.Count; i++)
        {
            // 재료 수량이 1개로 고정되는 의도에 따라 수량 표시는 생략하거나, ItemData의 수량을 사용합니다.
            // 현재는 ItemData를 사용하므로 ItemName만 표시합니다.
            cookingIngredientTexts[i].text = currentIngredients[i].itemSO.itemName;
        }
    }

    /// <summary>
    /// 냄비에 투입된 재료 목록과 UI를 모두 초기화합니다.
    /// </summary>
    public void ResetCookingIngredientUI()
    {
        // 냄비에 투입된 재료 리스트를 비웁니다.
        currentIngredients.Clear();

        // 냄비 아래의 재료 텍스트를 모두 "-"로 초기화합니다.
        foreach (var text in cookingIngredientTexts)
        {
            text.text = "-";
        }
    }

    /// <summary>
    /// 요리 UI 패널을 비활성화합니다.
    /// </summary>
    public void HideCookingUI()
    {
        cookingUIPanel.SetActive(false);
        if (PlayerCharacter.Instance != null)
        { PlayerCharacter.Instance.playerController.enabled = true; }
    }
}