using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System; // Enum.GetValues를 사용하기 위해 추가

public class Test : MonoBehaviour
{
    public GameObject testPanel;

    [Header("아이템 드롭 기능")]
    [Tooltip("아이템 목록을 표시할 드롭다운 UI 컴포넌트입니다.")]
    public TMP_Dropdown itemDropdown;

    [Tooltip("아이템 등급 목록을 표시할 드롭다운 UI 컴포넌트입니다. 장비 아이템 생성 시 등급을 선택하는 데 사용됩니다.")]
    public TMP_Dropdown gradeDropdown; // ⭐ 추가: 등급 선택 드롭다운

    // 수량은 1로 고정합니다.
    private const int ItemQuantity = 1;

    private void Start()
    {
        InitializeItemDropdown();
        InitializeGradeDropdown(); // ⭐ 추가: 등급 드롭다운 초기화
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // f1 키를 눌렀을 때 메인 패널의 활성화 상태를 토글합니다.
            TestPanelOnOff();
        }
    }

    /// <summary>
    /// 개발자 패널을 켜고 끕니다.
    /// </summary>
    public void TestPanelOnOff()
    {
        testPanel.SetActive(!testPanel.activeSelf);
    }

    /// <summary>
    /// 플레이어에게 10000 골드를 추가합니다.
    /// </summary>
    public void GoldUp()
    {
        // Null 체크: PlayerCharacter 인스턴스를 사용하는 모든 메서드에 필요합니다.
        if (PlayerCharacter.Instance == null || PlayerCharacter.Instance.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats 인스턴스를 찾을 수 없습니다.");
            return;
        }

        PlayerCharacter.Instance.playerStats.gold += 10000;
        Debug.Log($"[Cheat] 골드 10000 추가됨. 현재 골드: {PlayerCharacter.Instance.playerStats.gold}");
    }

    /// <summary>
    /// 플레이어에게 다음 레벨에 필요한 경험치 이상을 부여하여 레벨업을 유도합니다.
    /// 레벨업 로직(경험치 차감, 스탯/스킬 포인트 지급)을 PlayerLevelUp 스크립트에 위임합니다.
    /// SOLID 원칙: Test는 오직 LevelUp 로직을 트리거하는 역할만 수행합니다.
    /// </summary>
    public void LevelUp()
    {
        // 1. 필요한 PlayerCharacter 및 PlayerLevelUp 인스턴스 확인
        if (PlayerCharacter.Instance == null || PlayerCharacter.Instance.playerLevelUp == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerLevelUp 인스턴스를 찾을 수 없습니다. 레벨업 실패.");
            return;
        }

        // 2. 다음 레벨에 필요한 경험치량(requiredExperience)을 가져옵니다.
        float expToGrant = PlayerCharacter.Instance.playerStats.requiredExperience + 1;

        Debug.Log($"[Cheat] 다음 레벨에 필요한 경험치({PlayerCharacter.Instance.playerStats.requiredExperience:F0}) 이상인 {expToGrant:F0}를 부여하여 레벨업을 유도합니다.");

        // 3. AddExperience 메서드를 호출하여 경험치를 부여합니다.
        PlayerCharacter.Instance.playerLevelUp.AddExperience(expToGrant);
    }

    /// <summary>
    /// 플레이어에게 던전 코인 100개를 추가합니다.
    /// </summary>
    public void CoinUP()
    {
        if (DungeonCoinCurrency.Instance != null)
        {
            DungeonCoinCurrency.Instance.AddCoins(100);
            Debug.Log($"[Cheat] 던전 코인 100개 추가됨.");
        }
        else
        {
            Debug.LogError("DungeonCoinCurrency 인스턴스를 찾을 수 없습니다.");
        }
    }

    // --- 드롭다운 로직 ---

    /// <summary>
    /// ItemDatabaseManager에서 모든 아이템의 이름 목록을 가져와 드롭다운을 초기화합니다.
    /// </summary>
    public void InitializeItemDropdown()
    {
        if (itemDropdown == null || ItemDatabaseManager.Instance == null)
        {
            Debug.LogError("Item Dropdown 또는 ItemDatabaseManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        itemDropdown.ClearOptions();

        // 1. ItemDatabaseManager의 모든 아이템 이름만 추출합니다.
        List<string> itemNames = ItemDatabaseManager.Instance
            .GetItemList()
            .Select(item => item.itemName)
            .ToList();

        // 2. 이름 목록을 드롭다운에 옵션으로 추가합니다.
        itemDropdown.AddOptions(itemNames);

        // 3. 첫 번째 아이템이 선택되도록 설정합니다.
        if (itemNames.Count > 0)
        {
            itemDropdown.value = 0;
            itemDropdown.RefreshShownValue();
        }
    }

    /// <summary>
    /// ItemGrade 열거형의 모든 값을 가져와 등급 드롭다운을 초기화합니다.
    /// </summary>
    public void InitializeGradeDropdown()
    {
        if (gradeDropdown == null)
        {
            Debug.LogError("Grade Dropdown 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        gradeDropdown.ClearOptions();

        // 1. ItemGrade 열거형의 이름들을 가져옵니다.
        List<string> gradeNames = Enum.GetNames(typeof(ItemGrade)).ToList();

        // 2. 이름 목록을 드롭다운에 옵션으로 추가합니다.
        gradeDropdown.AddOptions(gradeNames);

        // 3. 기본값 설정 (Common 또는 첫 번째 등급)
        gradeDropdown.value = 0;
        gradeDropdown.RefreshShownValue();
    }

    /// <summary>
    /// 드롭다운에서 선택된 아이템을 플레이어의 인벤토리에 1개 추가합니다. (버튼에 연결될 함수)
    /// 장비 아이템인 경우 ItemGenerator를 통해 무작위 옵션을 생성 후 지급합니다.
    /// SOLID 원칙: OCP (Open/Closed Principle) - 일반/장비 아이템 처리 로직을 분리하여 확장을 용이하게 합니다.
    /// </summary>
    public void AddSelectedItem()
    {
        if (itemDropdown == null || PlayerCharacter.Instance == null || ItemDatabaseManager.Instance == null || ItemGenerator.Instance == null)
        {
            Debug.LogError("필요한 컴포넌트가 준비되지 않았습니다. 아이템 추가 실패.");
            return;
        }

        // 1. 선택된 아이템 이름으로 BaseItemSO 템플릿을 찾습니다.
        string selectedItemName = itemDropdown.options[itemDropdown.value].text;
        BaseItemSO templateItem = ItemDatabaseManager.Instance.GetItemByName(selectedItemName);

        if (templateItem == null)
        {
            Debug.LogError($"'{selectedItemName}'에 해당하는 아이템 템플릿을 찾을 수 없습니다.");
            return;
        }

        BaseItemSO itemToGrant; // 최종적으로 인벤토리에 지급할 아이템 (SO 인스턴스)

        // 2. 장비 아이템인지 확인하고 동적 생성합니다.
        if (templateItem is EquipmentItemSO equipmentTemplate)
        {
            // 2-1. 선택된 등급을 가져옵니다.
            string selectedGradeName = gradeDropdown.options[gradeDropdown.value].text;
            ItemGrade selectedGrade = (ItemGrade)Enum.Parse(typeof(ItemGrade), selectedGradeName);

            // 2-2. ItemGenerator를 통해 무작위 옵션이 적용된 새 인스턴스를 생성합니다.
            itemToGrant = ItemGenerator.Instance.GenerateItem(equipmentTemplate, selectedGrade);

            if (itemToGrant == null)
            {
                Debug.LogError($"장비 아이템 ({selectedItemName}, 등급: {selectedGradeName}) 생성 실패.");
                return;
            }

            Debug.Log($"[Cheat] 장비 생성 성공: **{itemToGrant.itemName}** (등급: {selectedGradeName}, ID: {itemToGrant.itemID})");
        }
        else
        {
            // 3. 일반 아이템인 경우, 템플릿을 그대로 지급합니다. (수량은 1개이므로 템플릿 자체를 사용해도 무방)
            itemToGrant = templateItem;
            Debug.Log($"[Cheat] 일반 아이템 추가: **{itemToGrant.itemName}** (ID: {itemToGrant.itemID}, 개수: {ItemQuantity})");
        }

        // 4. 인벤토리에 아이템 추가
        if (PlayerCharacter.Instance.inventoryManager != null)
        {
            // 장비(새 인스턴스)이든 일반 아이템(템플릿)이든, 준비된 객체를 인벤토리에 추가 요청합니다.
            PlayerCharacter.Instance.inventoryManager.AddItem(itemToGrant, ItemQuantity);
        }
        else
        {
            Debug.LogError("InventoryManager 컴포넌트를 찾을 수 없습니다. 아이템 추가 실패.");
        }
    }
}