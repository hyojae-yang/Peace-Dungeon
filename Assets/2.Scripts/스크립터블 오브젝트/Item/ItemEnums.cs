// ItemEnums.cs
/// <summary>
/// �������� �ֵ� ������ �����ϴ� �������Դϴ�.
/// </summary>
public enum ItemType
{
    Equipment,      // ��� (����, �� ��)
    Consumable,     // �Ҹ�ǰ (����, ��ũ�� ��)
    Material,       // ��� (���ۿ� ���)
    Quest,          // ����Ʈ ������ (���� ���� ����)
    Special         // Ư�� ������ (���� ���� ����)
}

/// <summary>
/// ��� �������� �������� Ÿ���� �����ϴ� �������Դϴ�.
/// </summary>
public enum EquipType
{
    None,           // �ش� ����
    Weapon,         // ����
    Armor,          // ��
    Accessory       // ��ű�
}

/// <summary>
/// ��� �������� **������ ����Ǵ� ����**�� �����ϴ� �������Դϴ�.
/// </summary>
public enum EquipSlot
{
    None,           // ��� �ƴ� ������ �Ǵ� ������ �ʴ� ����
    Weapon,         // ���� ���� ����
    Shield,         // ���� ���� ����
    Helmet,         // ��� ���� ����
    Armor,          // ���� ���� ����
    Gloves,         // �尩 ���� ����
    Boots,          // �Ź� ���� ����
    Accessory1,     // ù ��° ��ű� ���� ����
    Accessory2      // �� ��° ��ű� ���� ����
}

/// <summary>
/// �������� ����� �����ϴ� �������Դϴ�.
/// </summary>
public enum ItemGrade
{
    Common,         // �Ϲ�
    Uncommon,       // ���
    Rare,           // ���
    Epic,           // ����
    Legendary       // ����
}