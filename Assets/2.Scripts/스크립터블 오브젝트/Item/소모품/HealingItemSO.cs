// HealingItemSO.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ü�� �Ǵ� ������ ȸ���ϴ� �Ҹ�ǰ �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// ConsumableItemSO�� ��ӹ޾� ������ ��� ������ �����ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Healing Item", menuName = "Item/Consumable/Healing Item")]
public class HealingItemSO : ConsumableItemSO
{
    /// <summary>
    /// �Ҹ�ǰ�� ����ϴ� ������ �����մϴ�.
    /// �� �޼���� ConsumableItemSO�� ���� �޼��带 �������̵��մϴ�.
    /// </summary>
    /// <param name="player">�������� ����� �÷��̾� ĳ����</param>
    public override void Use(PlayerCharacter player)
    {
        // �÷��̾� �Ǵ� ���� �ý����� ��ȿ���� Ȯ���մϴ�.
        // ���� PlayerCharacter�� ��� ������ playerStats�� �����մϴ�.
        if (player == null || player.playerStats == null)
        {
            Debug.LogError("�÷��̾� �Ǵ� �÷��̾��� ���� �ý����� ã�� �� �����ϴ�. �������� ����� �� �����ϴ�.");
            return;
        }

        // consumptionEffects ����Ʈ�� ��� ��� ȿ���� �÷��̾�� �����մϴ�.
        foreach (var effect in consumptionEffects)
        {
            switch (effect.statType)
            {
                case StatType.MaxHealth:
                    // ü�� ȸ�� ȿ�� ����
                    float currentHealth = player.playerStats.health;
                    float maxHealth = player.playerStats.MaxHealth;
                    float healAmount = 0;

                    if (effect.isPercentage)
                    {
                        // �ۼ�Ʈ ȸ��
                        healAmount = maxHealth * (effect.value / 100f);
                    }
                    else
                    {
                        // ������ ȸ��
                        healAmount = effect.value;
                    }

                    // ���� ü���� ȸ������ŭ ���ϰ�, �ִ� ü���� ���� �ʵ��� �մϴ�.
                    player.playerStats.health = Mathf.Min(currentHealth + healAmount, maxHealth);
                    break;

                case StatType.MaxMana:
                    // ���� ȸ�� ȿ�� ����
                    float currentMana = player.playerStats.mana;
                    float maxMana = player.playerStats.MaxMana;
                    float restoreAmount = 0;

                    if (effect.isPercentage)
                    {
                        // �ۼ�Ʈ ȸ��
                        restoreAmount = maxMana * (effect.value / 100f);
                    }
                    else
                    {
                        // ������ ȸ��
                        restoreAmount = effect.value;
                    }

                    // ���� ������ ȸ������ŭ ���ϰ�, �ִ� ������ ���� �ʵ��� �մϴ�.
                    player.playerStats.mana = Mathf.Min(currentMana + restoreAmount, maxMana);
                    break;

                // ���ο� ȸ�� ������ �߰��Ǹ� ���⿡ case�� �߰��� �� �ֽ��ϴ�.
                default:
                    Debug.LogWarning($"���ǵ��� ���� ȸ�� ȿ���� �߰ߵǾ����ϴ�: {effect.statType}");
                    break;
            }
        }
    }
}