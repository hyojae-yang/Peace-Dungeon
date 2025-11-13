using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 레시피의 발견 상태를 저장하기 위한 직렬화 가능한 구조체입니다.
/// Dictionary는 Unity의 기본 JSON 직렬화에서 문제가 발생할 수 있으므로,
/// Dictionary 대신 List<int>로 발견된 레시피 ID 목록만 저장합니다.
/// </summary>
[Serializable]
public class RecipeDiscoverySaveData
{
    // 발견된 레시피의 고유 ID 목록만 저장합니다.
    public List<int> discoveredRecipeIDs = new List<int>();
}

/// <summary>
/// 플레이어의 레시피 발견 상태를 관리하는 싱글톤 스크립트입니다.
/// ISavable 인터페이스를 구현하여 영구적으로 데이터를 저장하고 로드하는 단일 책임을 가집니다.
/// SOLID: 단일 책임 원칙 (레시피 발견 상태 관리), 개방-폐쇄 원칙 (다른 시스템에 영향을 주지 않는 상태 관리).
/// </summary>
public class RecipeDiscoveryManager : MonoBehaviour, ISavable
{
    // === 싱글톤 인스턴스 ===
    public static RecipeDiscoveryManager Instance { get; private set; }

    // === 레시피 상태 데이터 ===
    // <Recipe ID, 발견 여부> 딕셔너리입니다. 런타임에 빠른 조회를 위해 사용됩니다.
    [Tooltip("현재까지 플레이어가 발견한 레시피의 ID와 상태를 저장합니다.")]
    private Dictionary<int, bool> discoveredRecipes = new Dictionary<int, bool>();

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // 싱글톤 초기화 로직
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요하다면 주석 해제
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // ISavable 인터페이스를 구현한 이 객체를 SaveManager에 등록합니다.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavable(this);

            // 새로하기일 때 초기화 로직이 필요하다면 여기에 추가합니다.
            // 레시피는 기본적으로 미발견 상태이므로 초기화 로직은 LoadData에 통합됩니다.
        }
        else
        {
            Debug.LogError("[RecipeDiscoveryManager] SaveManager 인스턴스를 찾을 수 없어 저장 시스템에 등록할 수 없습니다.");
        }
    }

    // === 핵심 비즈니스 로직 ===

    /// <summary>
    /// 새로운 레시피를 발견 상태로 설정합니다.
    /// 요리 성공 시 CookingManager에서 호출됩니다.
    /// </summary>
    /// <param name="recipeID">발견된 레시피의 고유 ID</param>
    public void DiscoverRecipe(int recipeID)
    {
        // 이미 발견된 레시피인지 확인합니다.
        if (discoveredRecipes.ContainsKey(recipeID) && discoveredRecipes[recipeID])
        {
            // Debug.Log($"[RecipeDiscovery] 레시피 ID {recipeID}는 이미 발견되었습니다.");
            return;
        }

        // 새로운 레시피를 발견 상태로 업데이트합니다.
        discoveredRecipes[recipeID] = true;

        // UI 갱신 등 필요한 이벤트 호출 (CookingUIManager에서 이벤트를 구독하게 하는 것이 이상적입니다.)
        // 여기서는 임시로 로그를 출력합니다.

        // 레시피 발견 후 UI 갱신이 필요한 경우, CookingUIManager에 이벤트를 발생시키거나
        // CookingUIManager가 직접 이 Manager의 상태를 주기적으로 조회하게 할 수 있습니다.
        // 여기서는 UI 갱신을 위해 CookingUIManager.Instance.UpdateRecipeUI()를 호출하는 것이 자연스럽습니다.
        // 현재는 CookingUIManager에 UpdateRecipeUI 시그니처가 없으므로 추후 추가 논의가 필요합니다.
    }

    /// <summary>
    /// 특정 레시피가 발견되었는지 여부를 조회합니다.
    /// CookingUIManager에서 레시피 목록 UI를 표시할 때 호출됩니다.
    /// </summary>
    /// <param name="recipeID">조회할 레시피의 고유 ID</param>
    /// <returns>발견 상태 (true: 발견됨, false: 미발견)</returns>
    public bool IsDiscovered(int recipeID)
    {
        // Dictionary에 키가 없거나 값이 false인 경우, 미발견으로 간주합니다.
        return discoveredRecipes.ContainsKey(recipeID) && discoveredRecipes[recipeID];
    }


    // === ISavable 인터페이스 구현 ===

    /// <summary>
    /// 현재 발견된 레시피 ID 목록을 RecipeDiscoverySaveData 객체로 변환하여 반환합니다.
    /// </summary>
    public object SaveData()
    {
        RecipeDiscoverySaveData saveData = new RecipeDiscoverySaveData
        {
            // discoveredRecipes 딕셔너리에서 값(bool)이 true인 키(int)만 추출하여 List<int>로 저장합니다.
            discoveredRecipeIDs = discoveredRecipes.Where(pair => pair.Value).Select(pair => pair.Key).ToList()
        };
        return saveData;
    }

    /// <summary>
    /// 저장된 RecipeDiscoverySaveData 객체를 읽어 discoveredRecipes 딕셔너리에 적용합니다.
    /// </summary>
    /// <param name="data">로드할 데이터가 담긴 RecipeDiscoverySaveData 객체</param>
    public void LoadData(object data)
    {
        if (data is RecipeDiscoverySaveData loadedData)
        {
            // 딕셔너리를 로드 전에 초기화합니다.
            discoveredRecipes.Clear();

            // 로드된 ID 목록을 딕셔너리에 추가합니다. 값은 true입니다.
            foreach (int recipeID in loadedData.discoveredRecipeIDs)
            {
                discoveredRecipes[recipeID] = true;
            }

        }
        else
        {
            // 데이터가 없거나 형식이 맞지 않으면 (예: 새로하기 시) 빈 상태로 시작합니다.
            discoveredRecipes.Clear();
        }

        // 데이터 로드 후, 혹시 UI가 이미 활성화되어 있다면 갱신을 트리거할 수 있습니다.
        // if (CookingUIManager.Instance.IsActive) { CookingUIManager.Instance.UpdateRecipeUI(); }
        // 이 로직은 CookingUIManager 수정 단계에서 구체화합니다.
    }
}