using UnityEngine;

public class Tooltip : MonoBehaviour
{
    private void OnDisable()
    {
        // InventoryUIController가 비활성화될 때, 씬에 잔여 툴팁이 있는지 확인합니다.
        // ItemSlotUI 스크립트의 static 변수를 통해 접근합니다.
        if (ItemSlotUI.currentActiveTooltip != null)
        {
            // 남아있는 툴팁 오브젝트를 즉시 파괴합니다.
            Destroy(ItemSlotUI.currentActiveTooltip);
            // static 참조도 null로 초기화하여 다음 사용을 준비합니다.
            ItemSlotUI.currentActiveTooltip = null;

        }

        // 이전에 논의했던 버튼 패널도 static으로 관리되고 있다면 여기서 함께 정리합니다.
        if (ItemSlotUI.instantiatedButtonPanel != null)
        {
            Destroy(ItemSlotUI.instantiatedButtonPanel);
            ItemSlotUI.instantiatedButtonPanel = null;
        }
        // EquipmentSlotUI에 선언된 static 변수를 통해 잔여 툴팁이 있는지 확인합니다.
        if (EquipmentSlotUI.currentActiveEquipTooltip != null)
        {
            // 잔여 툴팁 오브젝트를 즉시 파괴합니다.
            Destroy(EquipmentSlotUI.currentActiveEquipTooltip);
            // static 참조도 null로 초기화합니다.
            EquipmentSlotUI.currentActiveEquipTooltip = null;
        }
    }
}
