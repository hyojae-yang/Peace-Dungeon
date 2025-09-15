using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 아이템 버리기 확인창의 UI와 로직을 관리하는 스크립트입니다.
/// 사용자에게 버릴 개수를 입력받고 최종적으로 InventoryManager에 제거를 요청합니다.
/// </summary>
public class ConfirmPanel : MonoBehaviour
{
    // === 인스펙터에 할당할 UI 컴포넌트 ===
    [Tooltip("아이템의 아이콘을 표시할 Image 컴포넌트입니다.")]
    [SerializeField] private Image itemIconImage;

    [Tooltip("아이템의 이름을 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("버릴 개수를 입력받을 InputField 컴포넌트입니다. 장비 아이템의 경우 null일 수 있습니다.")]
    [SerializeField] private TMP_InputField countInputField;

    [Tooltip("'확인' 버튼 컴포넌트입니다.")]
    [SerializeField] private Button confirmButton;

    [Tooltip("'취소' 버튼 컴포넌트입니다.")]
    [SerializeField] private Button cancelButton;

    // === 내부 데이터 변수 ===
    private BaseItemSO currentItem;
    private int currentItemCount;

    private void Awake()
    {
        // 버튼에 클릭 이벤트 리스너를 추가합니다.
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    /// <summary>
    /// InventoryUIController로부터 아이템 정보를 받아 UI를 초기화합니다.
    /// </summary>
    /// <param name="item">버릴 아이템의 데이터</param>
    /// <param name="count">소지하고 있는 아이템의 총 개수</param>
    public void Initialize(BaseItemSO item, int count)
    {
        // 전달받은 데이터를 내부 변수에 저장합니다.
        this.currentItem = item;
        this.currentItemCount = count;

        // UI를 업데이트합니다.
        itemIconImage.sprite = item.itemIcon;
        itemNameText.text = item.itemName;

        // countInputField가 할당되어 있다면(null이 아니라면) 텍스트를 초기화합니다.
        if (countInputField != null)
        {
            countInputField.text = "1"; // 초기 입력값을 1로 설정합니다.
        }
    }

    /// <summary>
    /// '확인' 버튼 클릭 시 호출됩니다.
    /// 입력된 개수만큼 아이템을 버리는 로직을 실행합니다.
    /// </summary>
    private void OnConfirmButtonClicked()
    {
        int countToDiscard = 0;

        // countInputField가 할당되어 있다면 값을 가져오고, 아니면 기본값인 1을 사용합니다.
        if (countInputField != null)
        {
            // 입력 필드의 텍스트를 정수로 변환합니다.
            if (!int.TryParse(countInputField.text, out countToDiscard))
            {
                Debug.LogWarning("숫자만 입력해주세요.");
                return;
            }
        }
        else
        {
            // 장비 아이템과 같이 InputField가 없는 경우, 버릴 개수를 1로 고정합니다.
            countToDiscard = 1;
        }

        // 유효성 검사: 0보다 크고 소지 개수보다 작거나 같은지 확인합니다.
        if (countToDiscard > 0 && countToDiscard <= currentItemCount)
        {
            // InventoryManager에 아이템 제거를 요청합니다.
            InventoryManager.Instance.RemoveItem(currentItem, countToDiscard);

            // 작업 완료 후 확인창을 닫습니다.
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("잘못된 입력값입니다! 버릴 개수를 다시 확인해주세요.");
            // 사용자에게 경고 메시지를 표시하는 UI를 추가할 수 있습니다.
        }
    }

    /// <summary>
    /// '취소' 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnCancelButtonClicked()
    {
        // 확인창을 닫습니다.
        gameObject.SetActive(false);
    }
}