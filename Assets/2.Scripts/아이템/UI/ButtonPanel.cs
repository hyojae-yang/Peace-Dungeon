// ButtonPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 아이템 우클릭 시 나타나는 버튼 패널을 관리하는 스크립트입니다.
/// 아이템 타입에 따라 적절한 버튼을 활성화/비활성화합니다.
/// </summary>
public class ButtonPanel : MonoBehaviour
{
    // === 인스펙터에 할당할 참조 변수 ===
    [Tooltip("장비 아이템 장착 버튼입니다.")]
    [SerializeField] private Button equipButton;

    [Tooltip("소모품 아이템 사용 버튼입니다.")]
    [SerializeField] private Button useButton;

    [Tooltip("아이템 버리기 버튼입니다.")]
    [SerializeField] private Button discardButton;

    // === 내부 데이터 변수 ===
    private BaseItemSO currentItem;

    /// <summary>
    /// ItemSlotUI에서 호출되어 현재 아이템 정보를 받고, 버튼을 초기화합니다.
    /// </summary>
    /// <param name="item">현재 슬롯에 있는 아이템 데이터</param>
    public void Initialize(BaseItemSO item)
    {
        currentItem = item;
        ShowButtonsByItemType();
    }

    /// <summary>
    /// 아이템 타입에 따라 버튼들의 활성화 상태를 설정합니다.
    /// </summary>
    private void ShowButtonsByItemType()
    {
        // 모든 버튼을 일단 비활성화합니다.
        equipButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);

        // 아이템 타입에 따라 필요한 버튼만 활성화합니다.
        if (currentItem != null)
        {
            switch (currentItem.itemType)
            {
                case ItemType.Equipment:
                    equipButton.gameObject.SetActive(true);
                    discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Consumable:
                    useButton.gameObject.SetActive(true);
                    discardButton.gameObject.SetActive(true);
                    break;
                case ItemType.Material:
                case ItemType.Quest:
                    discardButton.gameObject.SetActive(true);
                    break;
                default:
                    // 특수 아이템 등은 아무 버튼도 표시하지 않습니다.
                    break;
            }
        }
    }

    // === 버튼 클릭 시 호출될 메서드 (추후 InventoryManager와 연동) ===

    /// <summary>
    /// '장착' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnEquipButtonClicked()
    {
        Debug.Log($"장착 버튼 클릭됨: {currentItem.itemName}");
        // InventoryManager.Instance.EquipItem((EquipmentItemSO)currentItem);
    }

    /// <summary>
    /// '사용' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnUseButtonClicked()
    {
        Debug.Log($"사용 버튼 클릭됨: {currentItem.itemName}");
        // InventoryManager.Instance.UseItem((ConsumableItemSO)currentItem);
    }

    /// <summary>
    /// '버리기' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnDiscardButtonClicked()
    {
        Debug.Log($"버리기 버튼 클릭됨: {currentItem.itemName}");
        // InventoryUIController.Instance.ShowConfirmPanel(currentItem, OnConfirmDiscard);
    }

    // 추후 버리기 확인창에서 호출될 메서드
    // private void OnConfirmDiscard(int amount)
    // {
    //     Debug.Log($"{amount}개 버리기 확정");
    //     // InventoryManager.Instance.RemoveItem(currentItem, amount);
    // }
}