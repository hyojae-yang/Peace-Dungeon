using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ������ ���Ǵ� ��� ��ȭ�� ������ �����ϴ� ������.
/// </summary>
public enum CurrencyType
{
    None,       // ��ȭ�� ���� ��츦 ���� �⺻��
    Gold,       // �Ϲ� ���
    Gem,        // ����
    DungeonCoin // ���� ���� ���� ��ȭ
}
// �ν����� �޴��� ���� ������ ������ ���� ������ �� �ֵ��� �մϴ�.
[CreateAssetMenu(fileName = "New Dungeon Item", menuName = "Dungeon/Item Data")]
public class DungeonItemData : ScriptableObject
{
    // �������� �ĺ��ϱ� ���� ���� ID
    public string itemID;

    // UI�� ǥ�õ� ������ �̸�
    public string itemName;

    // UI�� ǥ�õ� ������ �̹���
    public Sprite itemImage;

    // UI ���� (���� ����)
    public Color backgroundColor = Color.white;

    // �� �����۰� ����� 3D ������ ������
    public GameObject smallMapPrefab;

    // **���Ӱ� �߰��� �κ�:** �� �����ۿ� ����� UI ������
    public GameObject uiItemPrefab;

    // �������� ���� �ؽ�Ʈ
    [TextArea]
    public string description;

    [Header("Shop Settings")]
    public int price;// �������� ����
    public CurrencyType currencyType = CurrencyType.DungeonCoin; // �������� ���ݿ� ���Ǵ� ��ȭ ����
}