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

    // === INPCFunction 인터페이스 구현 ===

    [Header("Cooking Data")]
    [Tooltip("이 NPC가 제공할 요리 레시피 목록을 담은 ScriptableObject입니다.")]
    [SerializeField]
    private CookingDataSO cookingData;
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
    /// <summary>
    /// 냄비에 투입된 재료 목록을 기반으로 요리를 시도합니다.
    /// </summary>
    /// <param name="ingredients">플레이어가 냄비에 투입한 재료 목록입니다.</param>
    public bool TryCraft(List<ItemData> ingredients)
    {
        // 1. 투입된 재료 목록을 BaseItemSO 리스트로 변환합니다.
        List<BaseItemSO> currentIngredientSOs = ingredients.Select(itemData => itemData.itemSO).ToList();

        // 2. 투입된 재료 목록과 정확히 일치하는 레시피를 찾습니다.
        // 이 메서드는 다음 단계에서 추가할 예정입니다.
        // 현재는 재료 소모 테스트를 위해 이 부분을 잠시 건너뜁니다.

        // 3. 인벤토리에서 재료를 소모합니다.
        // 이 단계에서는 재료가 정상적으로 제거되는지만 확인합니다.
        bool allIngredientsRemoved = true;
        foreach (var itemSO in currentIngredientSOs)
        {
            // PlayerCharacter의 InventoryManager를 통해 아이템을 제거합니다.
            // 현재는 각 아이템이 한 개씩 소모된다고 가정합니다.
            bool removed = PlayerCharacter.Instance.inventoryManager.RemoveItem(itemSO, 1);
            if (!removed)
            {
                // 재료가 부족하여 제거에 실패하면 플래그를 false로 바꾸고 반복문을 중단합니다.
                allIngredientsRemoved = false;
                break;
            }
        }

        if (allIngredientsRemoved)
        {
            Debug.Log("인벤토리에서 재료가 정상적으로 소모되었습니다.");

            // 추가된 부분: 재료 소모가 성공하면 냄비 UI를 초기화합니다.
            if (CookingUIManager.Instance != null)
            {
                CookingUIManager.Instance.ResetCookingIngredientUI();
            }
            

            // 요리 성공 시 UI를 갱신해야 합니다.
            // CookingUIManager.Instance.UpdateInventoryUI();
            return true;
        }
        else
        {
            Debug.LogWarning("재료가 부족하여 요리를 만들 수 없습니다.");
            return false;
        }
    }

}