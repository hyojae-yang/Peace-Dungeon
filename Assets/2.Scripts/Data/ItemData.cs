using UnityEngine;
using System;

/// <summary>
/// 인벤토리 슬롯에 저장되는 아이템의 데이터 클래스입니다.
/// BaseItemSO와 현재 개수를 함께 저장하여, 아이템의 고유성을 보장합니다.
/// </summary>
[Serializable]
public class ItemData
{
    // === 아이템의 기본 정보 ===
    [Tooltip("이 아이템 슬롯이 담고 있는 아이템의 ScriptableObject입니다.")]
    public BaseItemSO itemSO;

    // === 아이템의 현재 개수 ===
    [Tooltip("이 아이템 슬롯에 현재 쌓여 있는 아이템의 개수입니다.")]
    public int stackCount;

    /// <summary>
    /// 새로운 ItemData 인스턴스를 생성합니다.
    /// </summary>
    public ItemData(BaseItemSO item, int count)
    {
        this.itemSO = item;
        this.stackCount = count;
    }
}