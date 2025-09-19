using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 내에서 사용되는 모든 재화의 종류를 정의하는 열거형.
/// </summary>
public enum CurrencyType
{
    None,       // 재화가 없는 경우를 위한 기본값
    Gold,       // 일반 골드
    Gem,        // 보석
    DungeonCoin // 던전 상점 전용 재화
}
// 인스펙터 메뉴를 통해 데이터 에셋을 쉽게 생성할 수 있도록 합니다.
[CreateAssetMenu(fileName = "New Dungeon Item", menuName = "Dungeon/Item Data")]
public class DungeonItemData : ScriptableObject
{
    // 아이템을 식별하기 위한 고유 ID
    public string itemID;

    // UI에 표시될 아이템 이름
    public string itemName;

    // UI에 표시될 아이템 이미지
    public Sprite itemImage;

    // UI 배경색 (선택 사항)
    public Color backgroundColor = Color.white;

    // 이 아이템과 연결된 3D 스몰맵 프리팹
    public GameObject smallMapPrefab;

    // **새롭게 추가된 부분:** 이 아이템에 연결된 UI 프리팹
    public GameObject uiItemPrefab;

    // 아이템의 설명 텍스트
    [TextArea]
    public string description;

    [Header("Shop Settings")]
    public int price;// 아이템의 가격
    public CurrencyType currencyType = CurrencyType.DungeonCoin; // 아이템의 가격에 사용되는 재화 종류
}