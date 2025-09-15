// BaseItemSO.cs
using UnityEngine;

/// <summary>
/// 모든 아이템의 공통 속성을 정의하는 추상 클래스입니다.
/// 모든 아이템 스크립터블 오브젝트는 이 클래스를 상속받아야 합니다.
/// </summary>
public abstract class BaseItemSO : ScriptableObject
{
    // [Header("기본 아이템 정보")]
    [Tooltip("아이템을 식별하는 고유 ID입니다. 런타임에 이 ID로 아이템을 검색합니다.")]
    public int itemID;

    [Tooltip("게임 내에서 표시될 아이템의 이름입니다.")]
    public string itemName;

    [Tooltip("아이템의 상세 설명입니다. 인벤토리 툴팁에 사용됩니다.")]
    [TextArea(3, 5)]
    public string itemDescription;

    [Tooltip("인벤토리에서 아이템을 나타낼 아이콘입니다.")]
    public Sprite itemIcon;

    [Tooltip("아이템을 상점에 판매할 때의 가격입니다.")]
    public int itemPrice;

    [Tooltip("아이템의 종류를 나타냅니다.")]
    public ItemType itemType;

    [Tooltip("인벤토리 한 칸에 최대로 겹쳐질 수 있는 아이템의 개수입니다. 1로 설정하면 겹치지 않습니다.")]
    public int maxStack = 1;
}