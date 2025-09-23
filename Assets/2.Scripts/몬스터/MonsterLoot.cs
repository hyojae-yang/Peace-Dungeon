using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 몬스터 사망 시 보상(경험치, 골드, 아이템)을 지급하는 클래스입니다.
/// 단일 책임 원칙에 따라 보상 지급의 책임만 가집니다.
/// </summary>
public class MonsterLoot : MonoBehaviour
{
    private MonsterBase monsterBase;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
        if (monsterBase == null)
        {
            Debug.LogError("MonsterLoot: MonsterBase 컴포넌트를 찾을 수 없습니다.", this);
        }

    }

    /// <summary>
    /// 몬스터가 죽었을 때 플레이어에게 보상을 지급하는 메서드입니다.
    /// MonsterBase의 Die() 메서드에서 호출됩니다.
    /// </summary>
    public void GiveReward()
    {
        if (monsterBase.monsterData == null)
        {
            Debug.LogError("MonsterLoot: MonsterData가 할당되지 않았습니다.", this);
            return;
        }


        // 몬스터 사망 시 경험치와 골드를 랜덤하게 계산합니다.
        int expReward = Random.Range(monsterBase.monsterData.minExpReward, monsterBase.monsterData.maxExpReward + 1);
        int goldReward = Random.Range(monsterBase.monsterData.minGoldReward, monsterBase.monsterData.maxGoldReward + 1);

        // 경험치는 PlayerLevelUp의 메서드를 통해, 골드는 PlayerStats의 변수를 통해 추가합니다.
        PlayerCharacter.Instance.playerLevelUp.AddExperience(expReward);
        PlayerCharacter.Instance.playerStats.gold += goldReward;

        // 아이템 드롭 기능 호출
        DropItem();
    }

    /// <summary>
    /// 몬스터 사망 시 아이템을 드롭하고 플레이어 인벤토리에 추가하는 로직입니다.
    /// </summary>
    private void DropItem()
    {

        var lootTable = monsterBase.monsterData.lootTable;
        int dropCount = Random.Range(monsterBase.monsterData.minItemDropCount, monsterBase.monsterData.maxItemDropCount + 1);

        for (int i = 0; i < dropCount; i++)
        {
            // LootTable이 비어 있지 않은 경우에만 아이템을 드롭합니다.
            if (lootTable != null && lootTable.Count > 0)
            {
                foreach (var lootItem in lootTable)
                {
                    if (Random.value <= lootItem.dropChance)
                    {
                        // InventoryManager의 AddItem() 메서드를 호출하여 아이템을 추가합니다.
                        PlayerCharacter.Instance.inventoryManager.AddItem(lootItem.itemData, 1);
                        break;
                    }
                }
            }
        }
    }
}