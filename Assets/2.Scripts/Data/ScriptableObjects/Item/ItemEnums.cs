// ItemEnums.cs
/// <summary>
/// 아이템의 주된 종류를 정의하는 열거형입니다.
/// </summary>
public enum ItemType
{
    Equipment,      // 장비 (무기, 방어구 등)
    Consumable,     // 소모품 (포션, 스크롤 등)
    Material,       // 재료 (제작에 사용)
    Quest,          // 퀘스트 아이템 (현재 논의 제외)
    Special         // 특수 아이템 (현재 논의 제외)
}

/// <summary>
/// 장비 아이템의 세부적인 타입을 정의하는 열거형입니다.
/// </summary>
public enum EquipType
{
    None,           // 해당 없음
    Weapon,         // 무기
    Armor,          // 방어구
    Accessory       // 장신구
}

/// <summary>
/// 장비 아이템의 **실제로 착용되는 부위**를 정의하는 열거형입니다.
/// </summary>
public enum EquipSlot
{
    None,           // 장비가 아닌 아이템 또는 사용되지 않는 슬롯
    Weapon,         // 무기 착용 슬롯
    Shield,         // 방패 착용 슬롯
    Helmet,         // 헬멧 착용 슬롯
    Armor,          // 갑옷 착용 슬롯
    Gloves,         // 장갑 착용 슬롯
    Boots,          // 신발 착용 슬롯
    Accessory1,     // 첫 번째 장신구 착용 슬롯
    Accessory2      // 두 번째 장신구 착용 슬롯
}

/// <summary>
/// 아이템의 등급을 정의하는 열거형입니다.
/// </summary>
public enum ItemGrade
{
    Common,         // 일반
    Uncommon,       // 고급
    Rare,           // 희귀
    Epic,           // 영웅
    Legendary       // 전설
}