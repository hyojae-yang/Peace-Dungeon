using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 이 클래스는 플레이어 스탯을 저장하고 로드하기 위한 데이터 전송 객체(DTO)입니다.
/// 저장해야 할 변수만 포함하며, 게임 로직은 전혀 들어있지 않습니다.
/// [System.Serializable] 속성은 이 클래스를 유니티의 JsonUtility (및 Newtonsoft.Json)가
/// JSON 문자열로 변환하고 다시 복원할 수 있도록 해줍니다.
/// </summary>
[System.Serializable]
public class PlayerStatsSaveData
{
    // === 실시간 핵심 능력치 (게임 플레이 중 변하는 값) ===
    /// <summary>
    /// 플레이어의 현재 체력입니다.
    /// </summary>
    public float health;
    /// <summary>
    /// 플레이어의 현재 마나입니다.
    /// </summary>
    public float mana;
    /// <summary>
    /// 플레이어의 현재 공격력입니다. (장비, 버프 등으로 변동 가능)
    /// </summary>
    public float attackPower;
    /// <summary>
    /// 플레이어의 현재 마법 공격력입니다.
    /// </summary>
    public float magicAttackPower;
    /// <summary>
    /// 플레이어의 현재 방어력입니다.
    /// </summary>
    public float defense;
    /// <summary>
    /// 플레이어의 현재 마법 방어력입니다.
    /// </summary>
    public float magicDefense;
    /// <summary>
    /// 플레이어의 최종 치명타 확률입니다.
    /// </summary>
    public float criticalChance;
    /// <summary>
    /// 플레이어의 최종 치명타 피해량 배율입니다.
    /// </summary>
    public float criticalDamageMultiplier;
    /// <summary>
    /// 플레이어의 최종 이동 속도입니다.
    /// </summary>
    public float moveSpeed;

    // === 캐릭터 진행 상황 ===
    /// <summary>
    /// 플레이어의 캐릭터 이름입니다.
    /// </summary>
    public string characterName;
    /// <summary>
    /// 플레이어가 소지한 골드입니다.
    /// </summary>
    public int gold;
    /// <summary>
    /// 플레이어의 현재 레벨입니다.
    /// </summary>
    public int level;
    /// <summary>
    /// 플레이어의 현재 경험치입니다.
    /// </summary>
    public int experience;
    /// <summary>
    /// 플레이어가 현재 보유한 스킬 포인트입니다.
    /// </summary>
    public int skillPoints;

    // === 스킬과 딕셔너리 ===
    /// <summary>
    /// 스킬 ID와 해당 레벨을 저장하는 딕셔너리입니다.
    /// Newtonsoft.Json을 사용하면 딕셔너리를 그대로 직렬화할 수 있습니다.
    /// </summary>
    public Dictionary<int, int> skillLevels;
}

/// <summary>
/// PlayerStatSystem 스크립트의 저장 데이터를 담는 클래스입니다.
/// 플레이어가 투자한 스탯 포인트와 남은 스탯 포인트를 저장합니다.
/// [System.Serializable] 속성을 추가하여 JSON으로 변환할 수 있도록 합니다.
/// </summary>
[System.Serializable]
public class PlayerStatSystemSaveData
{
    // === 투자된 스탯 포인트 ===
    /// <summary>
    /// 플레이어가 투자한 남은 스탯 포인트입니다.
    /// </summary>
    public int statPoints;
    /// <summary>
    /// 투자된 힘(strength) 스탯 포인트입니다.
    /// </summary>
    public int strength;
    /// <summary>
    /// 투자된 지능(intelligence) 스탯 포인트입니다.
    /// </summary>
    public int intelligence;
    /// <summary>
    /// 투자된 체질(constitution) 스탯 포인트입니다.
    /// </summary>
    public int constitution;
    /// <summary>
    /// 투자된 민첩(agility) 스탯 포인트입니다.
    /// </summary>
    public int agility;
    /// <summary>
    /// 투자된 집중력(focus) 스탯 포인트입니다.
    /// </summary>
    public int focus;
    /// <summary>
    /// 투자된 인내력(endurance) 스탯 포인트입니다.
    /// </summary>
    public int endurance;
    /// <summary>
    /// 투자된 활력(vitality) 스탯 포인트입니다.
    /// </summary>
    public int vitality;
}
/// <summary>
/// InventoryManager의 저장 데이터를 담는 컨테이너 클래스입니다.
/// </summary>
[System.Serializable]
public class InventorySaveData
{
    // 일반 아이템 저장용 리스트
    public List<SavableItemData> inventoryItems;

    // 모든 장비 아이템의 상세 정보(능력치 등)를 담는 리스트입니다.
    public List<SavableEquipmentData> allEquipmentItems;

    // 장착된 아이템의 슬롯 위치와 ID만 저장하는 리스트입니다.
    public List<SavableEquippedData> equippedSlots;
}

/// <summary>
/// 인벤토리 아이템의 저장 가능한 데이터를 나타내는 클래스입니다.
/// (SO 참조 없이 ID와 수량만 저장)
/// </summary>
[System.Serializable]
public class SavableItemData
{
    public int itemID;
    public int stackCount;
}

/// <summary>
/// 무작위 능력치가 부여된 장비 아이템의 저장 가능한 데이터를 나타내는 클래스입니다.
/// (SO 참조 없이 ID, 등급, 능력치 목록만 저장)
/// </summary>
[System.Serializable]
public class SavableEquipmentData
{
    // === 변경된 코드 ===
    public string uniqueID; // 각 아이템을 고유하게 식별할 ID
    public int itemID; // 아이템 종류를 식별할 ID
    // ==================
    public ItemGrade itemGrade;
    public List<StatModifier> baseStats;
    public List<StatModifier> additionalStats;
}

/// <summary>
/// 장비 슬롯에 장착된 아이템의 저장 가능한 데이터를 나타내는 클래스입니다.
/// (슬롯 위치와 아이템 ID만 저장)
/// </summary>
[System.Serializable]
public class SavableEquippedData
{
    public EquipSlot equipSlot;
    // === 변경된 코드 ===
    public string uniqueID; // 어떤 아이템이 장착되었는지 고유하게 식별
    // ==================
}