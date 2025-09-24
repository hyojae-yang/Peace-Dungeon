using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 버프(Buff) 아이템의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// 이 스크립트는 PlayerStatSystem에 전혀 종속되지 않고, 독립적으로 버프 효과를 적용합니다.
/// 기존 스크립트를 수정하지 않고 새로운 아이템 유형을 추가하는 것을 목표로 합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Buff Item", menuName = "Item/Consumable/Buff Item")]
public class BuffItemSO : ConsumableItemSO
{
    [Header("버프 속성")]
    [Tooltip("버프가 영구적으로 적용되는지 여부입니다. 체크하면 effectDuration은 무시됩니다.")]
    public bool isPermanentBuff;

    /// <summary>
    /// 버프 아이템을 사용하는 로직을 정의합니다.
    /// 이 메서드는 ConsumableItemSO의 가상 메서드를 오버라이드합니다.
    /// 버프에 따라 능력치를 영구적으로 올리거나, 일정 시간 버프를 부여합니다.
    /// </summary>
    /// <param name="player">아이템을 사용할 플레이어 캐릭터</param>
    public override void Use(PlayerCharacter player)
    {
        if (player == null || player.playerStats == null)
        {
            Debug.LogError("플레이어 또는 플레이어의 스탯 시스템을 찾을 수 없습니다. 아이템을 사용할 수 없습니다.");
            return;
        }

        // 일시적 버프인 경우, 기존 버프를 모두 초기화합니다.
        // 영구 버프는 초기화 로직을 건너뜁니다.
        if (!isPermanentBuff)
        {
            ResetAllTimedBuffs(player.playerStats);
        }

        foreach (var effect in consumptionEffects)
        {
            if (isPermanentBuff)
            {
                ApplyPermanentBuff(player.playerStats, effect);
            }
            else
            {
                if (effectDuration > 0)
                {
                    player.StartCoroutine(ApplyTimedBuff(player.playerStats, effect, effectDuration));
                }
                else
                {
                    Debug.LogWarning("일시적 버프이지만 지속 시간이 0으로 설정되어 효과가 즉시 사라집니다.");
                }
            }
        }
    }

    /// <summary>
    /// 플레이어에게 영구적인 스탯 버프를 적용하는 로직입니다.
    /// 이 로직은 PlayerStats의 변수를 직접 조작합니다.
    /// </summary>
    /// <param name="playerStats">버프를 적용할 플레이어 스탯</param>
    /// <param name="effect">적용할 스탯 효과</param>
    private void ApplyPermanentBuff(PlayerStats playerStats, StatModifier effect)
    {
        switch (effect.statType)
        {
            case StatType.Defense:
                playerStats.defense += effect.value;
                break;
            case StatType.AttackPower:
                playerStats.attackPower += effect.value;
                break;
            case StatType.MaxHealth:
                playerStats.MaxHealth += effect.value;
                playerStats.health = Mathf.Min(playerStats.health, playerStats.MaxHealth);
                break;
            case StatType.MoveSpeed:
                playerStats.moveSpeed += effect.value;
                if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerController != null)
                {
                    PlayerCharacter.Instance.playerController.walkSpeed = playerStats.moveSpeed;
                }
                break;
            case StatType.MaxMana:
                playerStats.MaxMana += effect.value;
                playerStats.mana = Mathf.Min(playerStats.mana, playerStats.MaxMana);
                break;
            case StatType.MagicAttackPower:
                playerStats.magicAttackPower += effect.value;
                break;
            case StatType.MagicDefense:
                playerStats.magicDefense += effect.value;
                break;
            case StatType.CriticalChance:
                playerStats.criticalChance += effect.value;
                playerStats.criticalChance = Mathf.Clamp(playerStats.criticalChance, 0f, 1f);
                break;
            case StatType.CriticalDamage:
                playerStats.criticalDamageMultiplier += effect.value;
                break;
            default:
                Debug.LogWarning($"정의되지 않은 영구 버프 효과가 발견되었습니다: {effect.statType}");
                break;
        }
        Debug.Log($"영구 버프 적용: {effect.statType}이(가) {effect.value}만큼 상승했습니다.");
    }

    // ⭐ 새롭게 추가된 메서드
    /// <summary>
    /// 모든 일시적 버프를 즉시 해제하고 스탯을 원상복구하는 메서드입니다.
    /// </summary>
    /// <param name="playerStats">버프를 해제할 플레이어 스탯</param>
    private void ResetAllTimedBuffs(PlayerStats playerStats)
    {
        if (PlayerStats.TimedBuffs == null) return;

        foreach (var buff in PlayerStats.TimedBuffs)
        {
            switch (buff.Key)
            {
                case StatType.MoveSpeed:
                    playerStats.moveSpeed -= buff.Value;
                    if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerController != null)
                    {
                        PlayerCharacter.Instance.playerController.walkSpeed = playerStats.moveSpeed;
                    }
                    break;
                case StatType.Defense:
                    playerStats.defense -= buff.Value;
                    break;
                case StatType.AttackPower:
                    playerStats.attackPower -= buff.Value;
                    break;
                case StatType.MaxHealth:
                    playerStats.MaxHealth -= buff.Value;
                    break;
                case StatType.MaxMana:
                    playerStats.MaxMana -= buff.Value;
                    break;
                case StatType.MagicAttackPower:
                    playerStats.magicAttackPower -= buff.Value;
                    break;
                case StatType.MagicDefense:
                    playerStats.magicDefense -= buff.Value;
                    break;
                case StatType.CriticalChance:
                    playerStats.criticalChance -= buff.Value;
                    break;
                case StatType.CriticalDamage:
                    playerStats.criticalDamageMultiplier -= buff.Value;
                    break;
            }
        }
        PlayerStats.TimedBuffs.Clear();
        Debug.Log("모든 기존 버프가 초기화되었습니다.");
    }


    /// <summary>
    /// 플레이어에게 일정 시간 동안 스탯 버프를 적용하고, 시간이 지나면 원상복구하는 코루틴입니다.
    /// 이 로직은 PlayerStats의 변수를 직접 조작합니다.
    /// </summary>
    /// <param name="playerStats">버프를 적용할 플레이어 스탯</param>
    /// <param name="effect">적용할 스탯 효과</param>
    /// <param name="duration">버프 지속 시간</param>
    private IEnumerator ApplyTimedBuff(PlayerStats playerStats, StatModifier effect, float duration)
    {
        // PlayerStats에 임시 버프를 저장할 수 있는 public static 딕셔너리를 사용합니다.
        // 이 방법은 PlayerStatSystem을 건드리지 않으면서, 모든 스크립트가 접근할 수 있는 공간을 만듭니다.
        // 이 딕셔너리는 PlayerStats 스크립트 내부에 선언되어 있어야 합니다.
        if (PlayerStats.TimedBuffs == null)
        {
            PlayerStats.TimedBuffs = new Dictionary<StatType, float>();
        }

        // 버프 적용
        switch (effect.statType)
        {
            case StatType.MoveSpeed:
                playerStats.moveSpeed += effect.value;
                if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerController != null)
                {
                    PlayerCharacter.Instance.playerController.walkSpeed = playerStats.moveSpeed;
                }
                break;
            case StatType.Defense:
                playerStats.defense += effect.value;
                break;
            case StatType.AttackPower:
                playerStats.attackPower += effect.value;
                break;
            case StatType.MaxHealth:
                playerStats.MaxHealth += effect.value;
                playerStats.health = Mathf.Min(playerStats.health, playerStats.MaxHealth);
                break;
            case StatType.MaxMana:
                playerStats.MaxMana += effect.value;
                playerStats.mana = Mathf.Min(playerStats.mana, playerStats.MaxMana);
                break;
            case StatType.MagicAttackPower:
                playerStats.magicAttackPower += effect.value;
                break;
            case StatType.MagicDefense:
                playerStats.magicDefense += effect.value;
                break;
            case StatType.CriticalChance:
                playerStats.criticalChance += effect.value;
                playerStats.criticalChance = Mathf.Clamp(playerStats.criticalChance, 0f, 1f);
                break;
            case StatType.CriticalDamage:
                playerStats.criticalDamageMultiplier += effect.value;
                break;
            default:
                Debug.LogWarning($"정의되지 않은 일시적 버프 효과가 발견되었습니다: {effect.statType}");
                yield break;
        }

        // 딕셔너리에 버프 값 저장
        PlayerStats.TimedBuffs[effect.statType] = effect.value;

        Debug.Log($"{effect.statType}이(가) {duration}초 동안 {effect.value}만큼 증가했습니다.");

        yield return new WaitForSeconds(duration);

        // 버프 원상복구 (딕셔너리에 저장된 값을 사용하여 정확하게 차감)
        if (PlayerStats.TimedBuffs.ContainsKey(effect.statType))
        {
            switch (effect.statType)
            {
                case StatType.MoveSpeed:
                    playerStats.moveSpeed -= PlayerStats.TimedBuffs[effect.statType];
                    if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.playerController != null)
                    {
                        PlayerCharacter.Instance.playerController.walkSpeed = playerStats.moveSpeed;
                    }
                    break;
                case StatType.Defense:
                    playerStats.defense -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.AttackPower:
                    playerStats.attackPower -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.MaxHealth:
                    playerStats.MaxHealth -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.MaxMana:
                    playerStats.MaxMana -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.MagicAttackPower:
                    playerStats.magicAttackPower -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.MagicDefense:
                    playerStats.magicDefense -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.CriticalChance:
                    playerStats.criticalChance -= PlayerStats.TimedBuffs[effect.statType];
                    break;
                case StatType.CriticalDamage:
                    playerStats.criticalDamageMultiplier -= PlayerStats.TimedBuffs[effect.statType];
                    break;
            }
            PlayerStats.TimedBuffs.Remove(effect.statType);
        }
        Debug.Log($"{effect.statType} 버프가 종료되었습니다.");
    }
}