using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� ��� �� ����(����ġ, ���, ������)�� �����ϴ� Ŭ�����Դϴ�.
/// ���� å�� ��Ģ�� ���� ���� ������ å�Ӹ� �����ϴ�.
/// </summary>
public class MonsterLoot : MonoBehaviour
{
    private MonsterBase monsterBase;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterLoot: MonsterBase ������Ʈ�� ã�� �� �����ϴ�.", this);
        }

    }

    /// <summary>
    /// ���Ͱ� �׾��� �� �÷��̾�� ������ �����ϴ� �޼����Դϴ�.
    /// MonsterBase�� Die() �޼��忡�� ȣ��˴ϴ�.
    /// </summary>
    public void GiveReward()
    {
        if (monsterBase.monsterData == null)
        {
            Debug.LogError("MonsterLoot: MonsterData�� �Ҵ���� �ʾҽ��ϴ�.", this);
            return;
        }


        // ���� ��� �� ����ġ�� ��带 �����ϰ� ����մϴ�.
        int expReward = Random.Range(monsterBase.monsterData.minExpReward, monsterBase.monsterData.maxExpReward + 1);
        int goldReward = Random.Range(monsterBase.monsterData.minGoldReward, monsterBase.monsterData.maxGoldReward + 1);

        // ����ġ�� PlayerLevelUp�� �޼��带 ����, ���� PlayerStats�� ������ ���� �߰��մϴ�.
        PlayerCharacter.Instance.playerLevelUp.AddExperience(expReward);
        PlayerCharacter.Instance.playerStats.gold += goldReward;

        // ������ ��� ��� ȣ��
        DropItem();
    }

    /// <summary>
    /// ���� ��� �� �������� ����ϰ� �÷��̾� �κ��丮�� �߰��ϴ� �����Դϴ�.
    /// </summary>
    private void DropItem()
    {

        var lootTable = monsterBase.monsterData.lootTable;
        int dropCount = Random.Range(monsterBase.monsterData.minItemDropCount, monsterBase.monsterData.maxItemDropCount + 1);

        for (int i = 0; i < dropCount; i++)
        {
            // LootTable�� ��� ���� ���� ��쿡�� �������� ����մϴ�.
            if (lootTable != null && lootTable.Count > 0)
            {
                foreach (var lootItem in lootTable)
                {
                    if (Random.value <= lootItem.dropChance)
                    {
                        // InventoryManager�� AddItem() �޼��带 ȣ���Ͽ� �������� �߰��մϴ�.
                        PlayerCharacter.Instance.inventoryManager.AddItem(lootItem.itemData, 1);
                        break;
                    }
                }
            }
        }
    }
}