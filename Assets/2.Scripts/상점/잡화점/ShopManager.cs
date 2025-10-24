using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

/// <summary>
/// NPC가 상점 기능을 제공할 때 사용되는 컴포넌트입니다.
/// INPCFunction 인터페이스를 구현하여 NPC 상호작용 시스템에 통합됩니다.
/// SOLID: 개방-폐쇄 원칙 (새로운 기능 추가 시 이 스크립트만 수정)
/// </summary>
public class ShopManager : MonoBehaviour, INPCFunction
{
    // ShopManager의 싱글턴 인스턴스 (UI와의 연결을 위해 추가)
    public static ShopManager Instance { get; private set; }

    //----------------------------------------------------------------------------------------------------------------
    // 새로운 기능 추가: ShopManager 싱글턴 및 ShopData 참조
    //----------------------------------------------------------------------------------------------------------------
    [Header("Shop Data")]
    [Tooltip("이 NPC가 판매할 아이템 목록을 담은 ScriptableObject입니다.")]
    [SerializeField]
    private ShopData shopData;

    private void Awake()
    {
        // 1. ShopManager 싱글턴 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // 한 씬에 여러 ShopManager가 있을 경우를 대비해 파괴하지 않습니다.
            // 대신, 다른 ShopManager가 이미 존재함을 경고합니다.
            Debug.LogWarning("씬에 이미 다른 ShopManager 인스턴스가 존재합니다.");
        }

        // 2. NPCManager에 스스로 등록
        NPC npc = GetComponentInParent<NPC>();
        if (npc != null && NPCManager.Instance != null)
        {
            NPCManager.Instance.RegisterSpecialFunction(npc.Data.npcName, this);
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 기존 코드 수정 (ExecuteFunction 메서드)
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// INPCFunction 인터페이스의 요구사항: UI 버튼에 표시될 이름을 반환합니다.
    /// </summary>
    public string FunctionButtonName
    {
        get { return "상점"; }
    }

    /// <summary>
    /// INPCFunction 인터페이스의 요구사항: 버튼이 클릭되었을 때 호출될 함수입니다.
    /// 이 메서드는 상점 UI를 활성화하는 로직을 담당합니다.
    /// </summary>
    public void ExecuteFunction()
    {
        // ShopUIManager의 ShowShop 메서드를 호출하여 상점 UI를 엽니다.
        // NPC의 이름을 전달하여 UI에 표시할 수 있게 합니다.
        if (ShopUIManager.Instance != null)
        {
            ShopUIManager.Instance.ShowShop(GetComponentInParent<NPC>().Data.npcName);
        }
        else
        {
            Debug.LogError("ShopUIManager 인스턴스를 찾을 수 없습니다.");
        }

        // 새로 추가된 부분: NPC와의 상호작용을 종료합니다.
        // 이렇게 하면 대화 UI가 사라지고 상점 UI만 남게 됩니다.
        if (GetComponentInParent<NPCInteraction>() != null)
        {
            GetComponentInParent<NPCInteraction>().EndInteraction();
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    // 새로운 기능 추가: 아이템 구매/판매 로직
    //----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 구매 패널을 엽니다.
    /// </summary>
    public void OpenBuyPanel()
    {
        if (ShopUIManager.Instance != null)
        {
            ShopUIManager.Instance.OpenBuyPanel(shopData);
        }
    }

    /// <summary>
    /// 판매 패널을 엽니다.
    /// </summary>
    public void OpenSellPanel()
    {
        if (ShopUIManager.Instance != null)
        {
            ShopUIManager.Instance.OpenSellPanel();
        }
    }

    // 수정된 메서드: 수량(quantity) 인자 추가
    /// <summary>
    /// 상점에서 아이템을 구매하는 로직을 처리합니다.
    /// </summary>
    /// <param name="itemToBuy">구매할 아이템의 데이터입니다.</param>
    /// <param name="quantity">구매할 아이템의 수량입니다.</param>
    public void BuyItem(BaseItemSO itemToBuy, int quantity)
    {
        // 총 구매 가격 계산
        int totalPrice = itemToBuy.itemPrice * quantity;

        // 1. 플레이어 소지금 확인
        if (PlayerCharacter.Instance.playerStats.gold < totalPrice)
        {
            Debug.Log("소지금이 부족합니다.");
            // UI를 닫고 싶다면 여기에 추가
            return;
        }

        // 2. 인벤토리에 아이템 추가 시도
        if (PlayerCharacter.Instance.inventoryManager.AddItem(itemToBuy, quantity))
        {
            // 3. 아이템 추가 성공 시 소지금 차감
            PlayerCharacter.Instance.playerStats.gold -= totalPrice;
            Debug.Log($"{itemToBuy.itemName}을(를) {quantity}개 구매했습니다. 남은 골드: {PlayerCharacter.Instance.playerStats.gold}");
        }
        else
        {
            if (NotificationManager.Instance != null)
            {
                // 인벤토리가 가득 찼음을 알리는 경고 알림을 띄웁니다.
                NotificationManager.Instance.ShowNotification("인벤토리 공간 부족!", NotificationType.Warning);
            }
        }

        // 구매가 완료되면 확인 창을 닫고 UI를 갱신합니다.
        ShopUIManager.Instance.CloseConfirmationPanel();
    }

    /// <summary>
    /// 인벤토리의 아이템을 상점에 판매하는 로직을 처리합니다.
    /// 장비와 일반 아이템을 구분하여 InventoryManager에 올바른 제거 요청을 합니다.
    /// </summary>
    /// <param name="itemToSell">판매할 아이템의 ItemData 인스턴스입니다.</param>
    /// <param name="quantity">판매할 아이템의 수량입니다. (장비의 경우 항상 1)</param>
    public void SellItem(ItemData itemToSell, int quantity)
    {
        if (itemToSell == null || itemToSell.itemSO == null || quantity <= 0)
        {
            ShopUIManager.Instance.CloseConfirmationPanel();
            return;
        }

        // 판매 가격은 구매 가격의 50%로 가정
        // 장비/일반 아이템 모두 이 공식 사용
        int sellPrice = (int)(itemToSell.itemSO.itemPrice * quantity * 0.5f);
        bool removalSuccess = false;

        // 1. 장비 아이템인지 확인합니다.
        if (itemToSell.itemSO is EquipmentItemSO equipmentSO)
        {
            // 1-1. 장비는 항상 1개씩 판매되어야 하며, 고유 ID로 제거합니다.
            if (quantity != 1)
            {
                Debug.LogError("장비 아이템은 1개씩만 판매할 수 있습니다.");
                ShopUIManager.Instance.CloseConfirmationPanel();
                return;
            }

            // InventoryManager에 uniqueID 기반 제거 요청
            // InventoryManager에는 public bool RemoveItem(string uniqueID) 오버로드가 필요합니다.
            removalSuccess = PlayerCharacter.Instance.inventoryManager.RemoveItem(equipmentSO.uniqueID);

            if (removalSuccess)
            {
                Debug.Log($"장비 {itemToSell.itemSO.itemName} (ID: {equipmentSO.uniqueID.Substring(0, 8)})을(를) 판매했습니다.");
            }
        }
        else
        {
            // 1-2. 일반 아이템은 ItemID와 수량으로 제거합니다.
            // InventoryManager에는 public bool RemoveItem(int itemID, int quantity) 오버로드가 필요합니다.
            removalSuccess = PlayerCharacter.Instance.inventoryManager.RemoveItem(itemToSell.itemSO.itemID, quantity);

            if (removalSuccess)
            {
                Debug.Log($"{itemToSell.itemSO.itemName}을(를) {quantity}개 판매했습니다.");
            }
        }

        // 2. 제거 성공 시 골드 추가
        if (removalSuccess)
        {
            PlayerCharacter.Instance.playerStats.gold += sellPrice;
            Debug.Log($"획득 골드: {sellPrice}. 현재 골드: {PlayerCharacter.Instance.playerStats.gold}");
        }
        else
        {
            Debug.LogWarning($"아이템 판매 실패: {itemToSell.itemSO.itemName}을(를) 인벤토리에서 제거할 수 없습니다. (재고 부족, 장착 중 등 오류)");
        }

        // 3. UI 갱신 및 확인 창 닫기
        ShopUIManager.Instance.CloseConfirmationPanel();
        // 판매 패널을 다시 열어 (갱신) 인벤토리 변경 사항을 반영합니다.
        ShopUIManager.Instance.OpenSellPanel();
    }
}
