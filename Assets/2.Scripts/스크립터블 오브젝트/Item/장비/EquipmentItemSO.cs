using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 장비 아이템의 데이터를 정의하는 스크립터블 오브젝트 클래스입니다.
/// BaseItemSO를 상속받아 장비만의 고유 속성을 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Item/Equipment Item")]
public class EquipmentItemSO : BaseItemSO
{
    [Header("장비 속성")]
    [Tooltip("장비 아이템의 등급입니다. 등급에 따라 능력치 보너스가 달라집니다.")]
    public ItemGrade itemGrade;

    [Tooltip("이 장비 아이템이 착용될 수 있는 부위입니다. (예: Weapon, Helmet, Accessory1 등)")]
    public EquipSlot equipSlot; // 실제 착용 부위

    [Tooltip("이 장비 아이템의 세부 종류입니다. (예: Weapon, Helmet, Accessory 등)")]
    public EquipType equipType; // 장비의 세부 종류

    // 플레이어의 PlayerStats 스크립트와 연동될 수 있도록 StatModifier를 사용합니다.
    [Header("기본 능력치")]
    [Tooltip("장비가 기본적으로 부여하는 능력치입니다. 등급에 따라 값이 변동됩니다.")]
    public List<StatModifier> baseStats = new List<StatModifier>();

    [Header("추가 능력치 (옵션)")]
    [Tooltip("이 아이템이 가질 수 있는 추가 능력치 목록입니다. ItemGenerator에 의해 무작위로 부여됩니다.")]
    public List<StatModifier> additionalStats = new List<StatModifier>();

    // --- 새로운 기능 추가 ---

    [Header("세트 아이템 속성")]
    [Tooltip("이 아이템이 속한 세트의 고유 ID입니다. 세트 아이템이 아닐 경우 비워둡니다.")]
    public string setID;
}