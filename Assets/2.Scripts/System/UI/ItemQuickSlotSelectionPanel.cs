using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 이 스크립트는 **소모품 아이템 등록**을 위해 5~8번 퀵슬롯을 선택하는 UI를 관리합니다.
/// SRP (단일 책임 원칙): 사용자에게 선택 UI를 제공하고, 선택된 정보를 퀵슬롯 관리자(PlayerItemController)에게 위임합니다.
/// </summary>
public class ItemQuickSlotSelectionPanel : MonoBehaviour
{
    [Header("슬롯 선택 버튼")]
    [Tooltip("5~8번 퀵슬롯에 대응할 4개의 버튼을 순서대로 할당하세요 (인덱스 0~3).")]
    public Button[] slotButtons; // 인덱스 0, 1, 2, 3

    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    // --- 내부 변수 ---
    // 소모품 아이템 데이터를 임시로 저장합니다.
    private ConsumableItemSO currentItemData;

    private void Awake()
    {
        // PlayerCharacter 인스턴스를 찾아 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("ItemQuickSlotSelectionPanel: PlayerCharacter 인스턴스가 존재하지 않습니다. 씬에 해당 컴포넌트가 있는지 확인해 주세요.");
            return;
        }

        // 퀵슬롯 관리 책임을 가진 PlayerItemController에 대한 참조를 확인합니다.
        if (playerCharacter.playerItemController == null) // InventoryManager -> PlayerItemController로 변경
        {
            Debug.LogError("ItemQuickSlotSelectionPanel: PlayerItemController가 PlayerCharacter에 할당되지 않았습니다. 퀵슬롯 등록 로직을 수행할 수 없습니다.");
            return;
        }

        // 각 슬롯 버튼에 클릭 이벤트 리스너를 추가합니다.
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i; // 클로저 이슈 방지를 위해 로컬 변수 사용 (0, 1, 2, 3)
            // SFX 재생 로직은 UI 버튼 설정에서 처리하거나, 여기서 MainSceneManager를 호출할 수 있습니다. (현재는 생략)
            slotButtons[i].onClick.AddListener(() => OnSlotButtonClick(slotIndex));
        }

        // 초기에는 이 패널을 비활성화 상태로 둡니다.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 등록 요청이 들어왔을 때 이 패널을 활성화하고 소모품 아이템 데이터를 받습니다.
    /// 이 메서드는 InventoryUIController에 의해 호출됩니다.
    /// </summary>
    /// <param name="itemToRegister">등록할 소모품 아이템 데이터</param>
    public void ShowPanel(ConsumableItemSO itemToRegister)
    {
        this.currentItemData = itemToRegister;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 아이템 퀵슬롯 버튼 클릭 시 호출됩니다.
    /// </summary>
    /// <param name="slotIndex">클릭된 슬롯의 인덱스 (0, 1, 2, 3)</param>
    private void OnSlotButtonClick(int slotIndex)
    {
        // PlayerItemController는 퀵슬롯 인덱스를 0부터 시작하는 4개(0, 1, 2, 3)로 관리합니다.
        // ItemQuickSlotSelectionPanel의 버튼 인덱스 0, 1, 2, 3이 바로 PlayerItemController의 퀵슬롯 인덱스 0, 1, 2, 3에 해당합니다.
        // 기존 코드의 slotIndex + 4 로직은 스킬/아이템 퀵슬롯이 1~8번을 공유하는 경우에 전체 인덱스를 맞추기 위함이었으나, 
        // PlayerItemController가 퀵슬롯 5~8번(키 입력 감지)의 인덱스만 0~3으로 관리하고 있으므로, 별도의 변환 없이 slotIndex를 그대로 사용합니다.
        int itemControllerQuickSlotIndex = slotIndex;

        // 퀵슬롯 5~8번에 대한 표시 인덱스 (로그 출력용)
        int displayQuickSlotNumber = itemControllerQuickSlotIndex + 5;

        if (currentItemData != null && playerCharacter.playerItemController != null)
        {
            // PlayerItemController의 등록 메서드를 호출하여 데이터를 전달합니다.
            // PlayerItemController는 RegisterItem(int slotIndex, ConsumableItemSO itemToRegister) 메서드를 가지고 있습니다.
            playerCharacter.playerItemController.RegisterItem(itemControllerQuickSlotIndex, currentItemData);

            // 등록 성공 여부를 직접적으로 반환받지 않으므로, 에러 로직은 PlayerItemController에 위임합니다.
        }
        else
        {
            Debug.LogWarning("등록할 아이템 데이터 또는 PlayerItemController가 없습니다. 다시 시도해 주세요.");
        }

        // 등록 성공/실패와 관계없이, 선택 패널을 닫습니다.
        HidePanel();
    }

    /// <summary>
    /// 패널을 비활성화하고 임시 데이터를 초기화합니다.
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false);
        // 패널이 닫힐 때 임시 변수를 초기화하여 메모리를 정리합니다.
        currentItemData = null;

        // 이 패널은 동적으로 생성되었을 가능성이 높으므로, 파괴 로직을 추가하는 것이 좋습니다.
        //Destroy(gameObject); // 동적 생성 시 파괴
    }
}