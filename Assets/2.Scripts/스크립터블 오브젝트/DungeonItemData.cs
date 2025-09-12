using UnityEngine;
using UnityEngine.UI;

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
}