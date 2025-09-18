// HealingItemSO.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 체력 또는 마나를 회복하는 소모품 아이템의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// ConsumableItemSO를 상속받아 고유한 사용 로직을 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "New Healing Item", menuName = "Item/Consumable/Healing Item")]
public class HealingItemSO : ConsumableItemSO
{
    /// <summary>
    /// 소모품을 사용하는 로직을 정의합니다.
    /// 이 메서드는 ConsumableItemSO의 가상 메서드를 오버라이드합니다.
    /// </summary>
    /// <param name="player">아이템을 사용할 플레이어 캐릭터</param>
    public override void Use(PlayerCharacter player)
    {
        // 플레이어 또는 스탯 시스템이 유효한지 확인합니다.
        // 이제 PlayerCharacter의 멤버 변수인 playerStats에 접근합니다.
        if (player == null || player.playerStats == null)
        {
            Debug.LogError("플레이어 또는 플레이어의 스탯 시스템을 찾을 수 없습니다. 아이템을 사용할 수 없습니다.");
            return;
        }

        // consumptionEffects 리스트에 담긴 모든 효과를 플레이어에게 적용합니다.
        foreach (var effect in consumptionEffects)
        {
            switch (effect.statType)
            {
                case StatType.MaxHealth:
                    // 체력 회복 효과 적용
                    float currentHealth = player.playerStats.health;
                    float maxHealth = player.playerStats.MaxHealth;
                    float healAmount = 0;

                    if (effect.isPercentage)
                    {
                        // 퍼센트 회복
                        healAmount = maxHealth * (effect.value / 100f);
                    }
                    else
                    {
                        // 고정값 회복
                        healAmount = effect.value;
                    }

                    // 현재 체력을 회복량만큼 더하고, 최대 체력을 넘지 않도록 합니다.
                    player.playerStats.health = Mathf.Min(currentHealth + healAmount, maxHealth);
                    break;

                case StatType.MaxMana:
                    // 마나 회복 효과 적용
                    float currentMana = player.playerStats.mana;
                    float maxMana = player.playerStats.MaxMana;
                    float restoreAmount = 0;

                    if (effect.isPercentage)
                    {
                        // 퍼센트 회복
                        restoreAmount = maxMana * (effect.value / 100f);
                    }
                    else
                    {
                        // 고정값 회복
                        restoreAmount = effect.value;
                    }

                    // 현재 마나를 회복량만큼 더하고, 최대 마나를 넘지 않도록 합니다.
                    player.playerStats.mana = Mathf.Min(currentMana + restoreAmount, maxMana);
                    break;

                // 새로운 회복 스탯이 추가되면 여기에 case를 추가할 수 있습니다.
                default:
                    Debug.LogWarning($"정의되지 않은 회복 효과가 발견되었습니다: {effect.statType}");
                    break;
            }
        }
    }
}