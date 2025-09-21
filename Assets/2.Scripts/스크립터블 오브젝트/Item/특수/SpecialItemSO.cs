using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ư�� �������� ���� �� �ִ� ȿ���� �����Դϴ�.
/// ���ο� ȿ���� �ʿ��ϸ� ���⿡ �߰��ϱ⸸ �ϸ� �˴ϴ�.
/// </summary>
public enum SpecialEffectType
{
    None,
    Teleport,            // ���� �̵� ȿ�� (��: Ư�� ��ġ�� �̵�)
    GrantPassiveBuff,    // ������ �ص� ����Ǵ� ���� ȿ�� (��: ���ݷ� ����)
    InteractWithObject,  // Ư�� ������Ʈ�� ��ȣ�ۿ��ϴ� ȿ�� (��: ������ �� ����)
}

/// <summary>
/// Ư�� ȿ���� ������ �׿� �ʿ��� �Ű������� ��� ����ü�Դϴ�.
/// �ϳ��� �������� ���� ȿ���� ���� �� �ֵ��� �����ϰ� ����Ǿ����ϴ�.
/// </summary>
[System.Serializable]
public struct SpecialEffect
{
    [Tooltip("�� ȿ���� � ������ ȿ������ �����մϴ�.")]
    public SpecialEffectType effectType;
    [Tooltip("ȿ���� �ʿ��� ������ �Ű������Դϴ�. (��: ���� ���� �ð�, Ư�� ID)")]
    public int intParameter;
    [Tooltip("ȿ���� �ʿ��� �Ǽ��� �Ű������Դϴ�. (��: ���� ��ġ)")]
    public float floatParameter;
    [Tooltip("ȿ���� �ʿ��� ���ڿ� �Ű������Դϴ�. (��: ������Ʈ �̸�, ���� �̸�)")]
    public string stringParameter;
}

/// <summary>
/// Ư�� �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// ��� Ư�� �������� �� ��ũ��Ʈ�� ������� �����˴ϴ�.
/// BaseItemSO�� ��ӹ޾� �������� �⺻ ������ �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Special Item", menuName = "Item/Special Item")]
public class SpecialItemSO : BaseItemSO
{
    // === Ư�� ������ ���� �Ӽ� ===
    [Header("Ư�� ������ ���� �Ӽ�")]
    [Tooltip("�� �������� ���� �� �ִ� ��� Ư�� ȿ���� ����Ʈ�Դϴ�.")]
    [SerializeField]
    private List<SpecialEffect> specialEffects = new List<SpecialEffect>();

    /// <summary>
    /// �������� ���� ��� Ư�� ȿ���� ����� ��ȯ�մϴ�.
    /// �ܺ� �ý���(��: ������ ��� ����)���� �� �����͸� �����Ͽ� ȿ���� �����մϴ�.
    /// </summary>
    public List<SpecialEffect> GetSpecialEffects()
    {
        return specialEffects;
    }
    [Tooltip("�� ���Կ� ���� �� �ִ� �ִ� �����Դϴ�.")]
    public int maxStackCount = 99;
    public override int maxStack => maxStackCount;
}