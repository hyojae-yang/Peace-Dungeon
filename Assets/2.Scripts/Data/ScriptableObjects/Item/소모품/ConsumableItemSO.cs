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

    [Header("사운드 설정")]
    [Tooltip("아이템 사용 시 재생할 효과음의 종류를 지정합니다.")]
    public SFXType useSFXType = SFXType.Item_Heal;
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

        //Debug.Log($"{itemName}을 사용했습니다!");
        if (SoundManager.Instance != null && useSFXType != SFXType.None)
        {
            // 아이템 사용 효과음은 적당한 0.7f 볼륨으로 설정합니다.
            SoundManager.Instance.PlaySFX(useSFXType);
        }
        // consumptionEffects 리스트에 담긴 모든 효과를 플레이어에게 적용하는 로직
        // 예: 체력 회복, 마나 회복 등
        foreach (var effect in consumptionEffects)
        {
            // 여기에서 effect의 statType과 value를 playerStats에 반영하는 코드를 작성해야 합니다.
            // 예를 들어, effect.statType이 MaxHealth라면, playerStats.health를 증가시키는 식입니다.
        }
    }
    /// <summary>
    /// 소모품이 현재 플레이어 상태 및 게임 환경에서 사용 가능한지 여부를 반환합니다.
    /// 하위 클래스에서 오버라이드하여 구체적인 유효성 검사 로직을 구현합니다.
    /// </summary>
    /// <param name="player">아이템을 사용할 플레이어 캐릭터</param>
    /// <returns>사용 가능하다면 true, 아니라면 false</returns>
    public virtual bool CanUse(PlayerCharacter player)
    {
        // 기본적으로 모든 소모품은 사용 가능하다고 가정합니다.
        return true;
    }
}