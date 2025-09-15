// StatModifier.cs
using System; // Serializable�� ����ϱ� ���� �ʿ��մϴ�.

/// <summary>
/// �������� �÷��̾�� �ο��ϴ� �ɷ�ġ ���ʽ� ������ ��� ����ü�Դϴ�.
/// </summary>
[Serializable] // ����Ƽ �ν����Ϳ��� �� �� �ֵ��� ����ȭ�մϴ�.
public struct StatModifier
{
    /// <summary>
    /// ���ʽ��� ������ �ɷ�ġ�� ������ ��Ÿ���� �������Դϴ�.
    /// PlayerStatSystem ��ũ��Ʈ�� StatType �������� �����մϴ�.
    /// </summary>
    public StatType statType;

    /// <summary>
    /// �ɷ�ġ�� ������ ���Դϴ�.
    /// </summary>
    public float value;

    /// <summary>
    /// �� ���ʽ��� ����������(false) ���������(true) �����Դϴ�.
    /// </summary>
    public bool isPercentage;

    /// <summary>
    /// StatModifier ����ü�� �������Դϴ�.
    /// </summary>
    /// <param name="type">���� ����</param>
    /// <param name="val">��</param>
    /// <param name="isPercent">����� ����</param>
    public StatModifier(StatType type, float val, bool isPercent)
    {
        this.statType = type;
        this.value = val;
        this.isPercentage = isPercent;
    }
}