// ConsumableItemSO.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 소모품 아이템의 데이터를 정의하는 스크립터블 오브젝트 클래스입니다.
/// BaseItemSO를 상속받아 소모품만의 고유 속성을 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Item/Consumable Item")]
public class ConsumableItemSO : BaseItemSO
{
    [Header("소모품 속성")]
    [Tooltip("소모품 사용 시 적용될 능력치 보너스입니다. 여러 효과를 가질 수 있습니다.")]
    public List<StatModifier> consumptionEffects = new List<StatModifier>();

    [Tooltip("한 슬롯에 쌓을 수 있는 최대 개수입니다.")]
    public int maxStackCount = 99;

    [Tooltip("소모품 사용 시 플레이어에게 적용되는 버프 또는 디버프의 지속 시간(초)입니다. (0일 경우 즉시 효과)")]
    public float effectDuration = 0f;

    /// <summary>
    /// BaseItemSO의 maxStack을 재정의하여, 이 아이템의 최대 스택 수를 반환합니다.
    /// </summary>
    public override int maxStack => maxStackCount;

    /// <summary>
    /// 소모품을 사용하는 로직을 정의하는 가상(virtual) 메서드입니다.
    /// 추후 이 클래스를 상속받아 더 복잡한 기능을 가진 소모품(예: 부활 아이템)을 만들 수 있습니다.
    /// </summary>
    public virtual void Use(PlayerCharacter player)
    {
        Debug.Log($"{itemName}을 사용했습니다!");

        // consumptionEffects 리스트에 담긴 모든 효과를 플레이어에게 적용하는 로직
        // 예: 체력 회복, 마나 회복 등
        foreach (var effect in consumptionEffects)
        {
            // 여기에서 effect의 statType과 value를 playerStats에 반영하는 코드를 작성해야 합니다.
            // 예를 들어, effect.statType이 MaxHealth라면, playerStats.health를 증가시키는 식입니다.
        }
    }
}