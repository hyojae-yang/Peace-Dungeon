using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 몬스터의 모든 기본 스탯과 보상 데이터를 관리하는 스크립터블 오브젝트입니다.
/// 이 에셋 파일을 통해 코드를 수정하지 않고도 다양한 몬스터를 만들 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "New Monster Data", menuName = "Monster/Monster Data", order = 1)]
public class MonsterData : ScriptableObject
{
    [Header("몬스터 기본 정보")]
    [Tooltip("몬스터를 식별하는 고유 ID입니다.")]
    public int monsterID;
    [Tooltip("몬스터의 이름입니다.")]
    public string monsterName;

    [Tooltip("몬스터의 현재 레벨입니다.")]
    public int monsterLevel;

    [Header("몬스터 기본 스탯")]
    public float maxHealth; // 최대 체력
    public int maxMana; // 최대 마나
    public int attackPower; // 공격력
    public float magicAttackPower; // 마법 공격력
    public int defense; // 방어력
    public int magicDefense; // 마법 방어력
    public float moveSpeed; // 이동 속도
    public float criticalChance; // 치명타 확률 (0.0 ~ 1.0)
    public float criticalDamageMultiplier; // 치명타 데미지 배율

    [Header("몬스터 공격 타입")]
    [Tooltip("몬스터의 기본 공격 타입을 설정합니다.")]
    public DamageType attackDamageType = DamageType.Physical;

    [Header("보상 설정")]
    [Tooltip("처치 시 얻는 경험치 보상 범위입니다.")]
    public int minExpReward;
    public int maxExpReward;

    [Tooltip("처치 시 얻는 골드 보상 범위입니다.")]
    public int minGoldReward;
    public int maxGoldReward;

    [Header("아이템 드롭 설정")]
    [Tooltip("드롭 가능한 아이템 목록과 드롭 확률입니다.")]
    public List<LootItem> lootTable = new List<LootItem>();

    [Tooltip("몬스터가 죽었을 때 드롭할 아이템의 최소 개수입니다.")]
    public int minItemDropCount = 0;

    [Tooltip("몬스터가 죽었을 때 드롭할 아이템의 최대 개수입니다.")]
    public int maxItemDropCount = 1;

    /// <summary>
    /// 아이템 드롭 테이블의 각 항목을 정의하는 구조체입니다.
    /// 이 구조체는 MonsterData 스크립터블 오브젝트에서 사용됩니다.
    /// </summary>
    [System.Serializable]
    public struct LootItem
    {
        [Tooltip("드롭할 아이템 스크립터블 오브젝트입니다. (사용자님의 ItemData 클래스에 연결해주세요.)")]
        public BaseItemSO itemData;

        [Tooltip("아이템이 드롭될 확률입니다. (0.0f ~ 1.0f 사이의 값)")]
        [Range(0.0f, 1.0f)]
        public float dropChance;
    }
}
