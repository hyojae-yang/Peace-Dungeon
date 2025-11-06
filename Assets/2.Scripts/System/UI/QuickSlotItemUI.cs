using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 아이템 퀵슬롯의 UI 표시를 담당하는 컴포넌트입니다.
/// [SRP]: 아이템 이미지, 수량 텍스트 표시 및 초기화 역할만 수행합니다.
/// </summary>
public class QuickSlotItemUI : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("아이템 이미지를 표시할 Image 컴포넌트를 할당하세요.")]
    public Image itemImage;

    [Tooltip("슬롯이 비어있을 때 표시할 기본 슬롯 스프라이트를 할당하세요.")]
    public Sprite defaultSlotSprite;

    [Tooltip("아이템의 현재 수량을 표시할 TextMeshProUGUI 컴포넌트를 할당하세요.")]
    public TextMeshProUGUI quantityText;

    // 현재 등록된 아이템 데이터를 저장합니다. (참조용)
    private ConsumableItemSO currentItemData; // 이 필드를 private으로 유지하고, public 메서드를 통해 정보를 제공합니다.

    /// <summary>
    /// 외부(QuickSlotItemPanel)에서 호출되어 슬롯의 UI(이미지)를 업데이트합니다.
    /// </summary>
    /// <param name="data">슬롯에 등록할 소모품 아이템 데이터. 해제 시에는 null을 전달합니다.</param>
    public void UpdateUI(ConsumableItemSO data)
    {
        currentItemData = data;

        if (currentItemData != null && itemImage != null)
        {
            // 아이템 등록
            itemImage.enabled = true;
            itemImage.sprite = currentItemData.itemIcon;

            // 수량 텍스트 임시 초기화/숨김 (수량은 QuickSlotItemPanel에서 UpdateStackCountUI를 통해 동기화됩니다)
            SetQuantityText(string.Empty, false);
        }
        else if (itemImage != null)
        {
            // 아이템 해제
            itemImage.enabled = true;
            itemImage.sprite = defaultSlotSprite;

            // 수량 텍스트 완전히 초기화/숨김
            SetQuantityText(string.Empty, false);
        }
    }

    /// <summary>
    /// 외부(QuickSlotItemPanel)에서 호출되어 아이템의 현재 수량을 업데이트합니다.
    /// 이 메서드는 실시간 소모 시 호출됩니다.
    /// </summary>
    /// <param name="count">업데이트된 현재 아이템 수량</param>
    public void UpdateStackCountUI(int count)
    {
        // 아이템이 등록되어 있는 상태에서만 수량을 업데이트해야 합니다.
        if (currentItemData == null)
        {
            // 아이템이 없으므로 수량 텍스트를 숨깁니다.
            SetQuantityText(string.Empty, false);
            return;
        }

        // [수정된 로직] 수량이 1개 이상일 경우 모두 표시합니다.
        if (count >= 1)
        {
            // 수량이 1개 이상일 때 숫자를 표시합니다. (1일 경우 '1'이 표시됩니다)
            SetQuantityText(count.ToString(), true);
        }
        else
        {
            // 수량이 0개일 경우 텍스트를 숨깁니다. 
            // 0개로 인해 슬롯이 비워지는 로직은 PlayerItemController가 담당합니다.
            SetQuantityText(string.Empty, false);
        }
    }

    /// <summary>
    /// 현재 이 퀵슬롯이 지정된 아이템을 표시하고 있는지 확인합니다. (QuickSlotItemPanel에서 사용)
    /// </summary>
    /// <param name="itemSO">비교할 소모품 아이템 데이터</param>
    /// <returns>현재 슬롯에 해당 아이템이 등록되어 있으면 true</returns>
    public bool IsDisplayingItem(ConsumableItemSO itemSO)
    {
        return currentItemData != null && currentItemData == itemSO;
    }

    /// <summary>
    /// 수량 텍스트를 업데이트하는 내부 헬퍼 메서드입니다.
    /// </summary>
    /// <param name="text">표시할 수량 텍스트</param>
    /// <param name="isActive">텍스트 오브젝트를 활성화할지 여부</param>
    private void SetQuantityText(string text, bool isActive)
    {
        if (quantityText != null)
        {
            quantityText.text = text;
            quantityText.gameObject.SetActive(isActive);
        }
    }
}