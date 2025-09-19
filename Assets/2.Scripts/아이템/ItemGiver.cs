using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어에게 아이템을 지급하여 인벤토리 시스템을 테스트하기 위한 스크립트입니다.
/// 다양한 타입의 아이템(장비, 소모품 등)을 생성하고 추가하는 기능을 포함합니다.
/// </summary>
public class ItemGiver : MonoBehaviour
{
    // === 인스펙터에 할당할 변수 ===
    [Header("테스트 아이템 설정")]
    [Tooltip("인벤토리에 추가할 아이템 템플릿입니다. 모든 종류의 아이템을 할당할 수 있습니다.")]
    [SerializeField] private BaseItemSO itemTemplate;

    [Tooltip("장비 아이템 생성 시 적용될 등급입니다. (장비 템플릿 할당 시에만 유효)")]
    [SerializeField] private ItemGrade itemGrade = ItemGrade.Common;

    // === 내부 데이터 변수 ===
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("ItemGiver: PlayerCharacter 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // InventoryManager가 PlayerCharacter에 할당되었는지 확인합니다.
        if (playerCharacter.inventoryManager == null)
        {
            Debug.LogError("ItemGiver: InventoryManager가 PlayerCharacter에 할당되지 않았습니다.");
        }
    }

    // === MonoBehaviour 메서드 ===

    /// <summary>
    /// 매 프레임마다 키 입력을 확인하여 아이템을 생성하고 인벤토리에 지급합니다.
    /// Q 키: 장비 아이템 생성 및 지급
    /// E 키: 소모품 아이템 생성 및 지급
    /// </summary>
    private void Update()
    {
        // Q 키를 누르면 장비 아이템을 지급합니다.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GiveGeneratedItem();
        }

        // E 키를 누르면 소모품 아이템을 지급합니다.
        if (Input.GetKeyDown(KeyCode.E))
        {
            GiveConsumableItem();
        }
    }

    /// <summary>
    /// ItemGenerator를 통해 장비 아이템을 동적으로 생성하여 인벤토리에 추가합니다.
    /// 이 메서드는 Q 키 입력 시 호출됩니다.
    /// </summary>
    private void GiveGeneratedItem()
    {
        // 템플릿이 장비 아이템인지 확인합니다.
        EquipmentItemSO equipTemplate = itemTemplate as EquipmentItemSO;

        if (equipTemplate != null && playerCharacter.inventoryManager != null && ItemGenerator.Instance != null)
        {
            // ItemGenerator는 싱글톤이므로 Instance를 통해 접근합니다.
            EquipmentItemSO newItem = ItemGenerator.Instance.GenerateItem(equipTemplate, itemGrade);

            // 생성된 아이템을 인벤토리에 추가합니다.
            playerCharacter.inventoryManager.AddItem(newItem, 1);
        }
    }

    /// <summary>
    /// 할당된 소모품 아이템을 인벤토리에 추가합니다.
    /// 이 메서드는 E 키 입력 시 호출됩니다.
    /// </summary>
    private void GiveConsumableItem()
    {
        // 템플릿이 소모품 아이템인지 확인합니다.
        ConsumableItemSO consumeTemplate = itemTemplate as ConsumableItemSO;

        if (consumeTemplate != null && playerCharacter.inventoryManager != null)
        {
            // 소모품 아이템은 별도의 생성 로직 없이 복제하여 인벤토리에 추가합니다.
            ConsumableItemSO newItem = Instantiate(consumeTemplate);

            // 생성된 아이템을 인벤토리에 추가합니다.
            playerCharacter.inventoryManager.AddItem(newItem, 1);
        }
    }
}