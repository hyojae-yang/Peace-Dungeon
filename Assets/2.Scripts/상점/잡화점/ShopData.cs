using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC가 판매하는 아이템 목록 데이터를 담는 ScriptableObject입니다.
/// 유니티 에디터에서 Assets -> Create -> NPC -> Shop Data를 통해 에셋으로 생성할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "New Shop Data", menuName = "NPC/Shop Data")]
public class ShopData : ScriptableObject
{
    [Tooltip("이 상점에서 판매할 아이템 목록입니다.")]
    public List<BaseItemSO> itemsToSell;
}