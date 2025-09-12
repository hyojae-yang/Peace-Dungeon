using UnityEngine;
using UnityEngine.UI;

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
}