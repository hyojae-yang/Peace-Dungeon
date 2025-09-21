using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 상점 목록에 표시되는 개별 아이템 UI의 동작을 관리합니다.
/// 구매/판매 버튼의 텍스트와 기능을 동적으로 설정합니다.
/// SOLID: 단일 책임 원칙 (개별 아이템 UI 관리).
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    // === UI 컴포넌트 참조 ===
    [Tooltip("아이템 이미지를 표시할 Image 컴포넌트입니다.")]
    [SerializeField]
    private Image itemIcon;

    [Tooltip("아이템 이름을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI itemName;

    [Tooltip("아이템 가격을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI itemPrice;

    [Tooltip("구매 또는 판매 동작을 실행할 버튼입니다.")]
    [SerializeField]
    private Button actionButton;

    [Tooltip("버튼의 텍스트를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField]
    private TextMeshProUGUI actionButtonText;

    /// <summary>
    /// 아이템 UI를 주어진 데이터로 초기화하는 메서드입니다.
    /// 이 메서드는 ShopUIManager에서 호출됩니다.
    /// </summary>
    /// <param name="itemSO">표시할 아이템의 BaseItemSO 데이터입니다.</param>
    /// <param name="buttonText">버튼에 표시할 텍스트입니다. (예: "구매", "판매")</param>
    /// <param name="onButtonClick">버튼 클릭 시 실행될 동작입니다.</param>
    public void Setup(BaseItemSO itemSO, string buttonText, Action onButtonClick)
    {
        // Null 체크: 매개변수가 유효한지 확인합니다.
        if (itemSO == null)
        {
            Debug.LogError("ShopItemUI: Setup 메서드에 유효하지 않은 BaseItemSO가 전달되었습니다.");
            return;
        }

        // 아이템 정보 표시
        if (itemIcon != null)
        {
            itemIcon.sprite = itemSO.itemIcon;
        }
        if (itemName != null)
        {
            itemName.text = itemSO.itemName;
        }
        if (itemPrice != null)
        {
            // G는 Gold의 약자로 가정합니다.
            itemPrice.text = $"{itemSO.itemPrice} G";
        }

        // 버튼 초기화 및 이벤트 연결
        if (actionButton != null)
        {
            // 기존 리스너를 모두 제거하여 중복 연결을 방지합니다.
            actionButton.onClick.RemoveAllListeners();
            // 새로운 리스너를 추가합니다.
            actionButton.onClick.AddListener(() => onButtonClick?.Invoke());
        }

        // 버튼 텍스트 설정
        if (actionButtonText != null)
        {
            actionButtonText.text = buttonText;
        }
    }
}