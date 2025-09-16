using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �������� ���� ������ �����ϴ� �������Դϴ�.
/// �� �������� ���� ������ ������ ��ũ���ͺ� ������Ʈ���� ���˴ϴ�.
/// </summary>
public enum WeaponType
{
    None,
    Sword,      // �� (�ٰŸ�)
    Staff,      // ������ (���Ÿ� ����)
    Axe,        // ���� (�ٰŸ�, Ⱦ����)
    Bow,        // Ȱ (���Ÿ� ����)
    Spear,      // â (�߰Ÿ�)
}


/// <summary>
/// ���� �������� �����͸� �����ϴ� ��ũ���ͺ� ������Ʈ�Դϴ�.
/// EquipmentItemSO�� ��ӹ޾� ���⸸�� ���� �Ӽ��� �����ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Item/Weapon Item")]
public class WeaponItemSO : EquipmentItemSO
{
    // === ���� ���� �Ӽ� ===
    [Header("���� ���� �Ӽ�")]
    [Tooltip("�� ���� �������� ���� ������ �����մϴ�. (��: Sword, Axe ��)")]
    public WeaponType weaponType;

    [Tooltip("�� ���Ⱑ ���ϴ� �������� ������ �����մϴ�. (��: Physical, Magic)")]
    public DamageType damageType;

    [Tooltip("������ ��� �ִ� �Ÿ�(�ݰ�)�Դϴ�.")]
    public float attackRange;

    [Tooltip("������ ��� ��ä���� �����Դϴ�. (�� ����)")]
    [Range(0, 360)]
    public float attackAngle;

    [Tooltip("�� ������ ���� �ӵ��Դϴ�. (�ʴ� ���� Ƚ��)")]
    public float attackSpeed;

    [Tooltip("���ݿ� ���� ����� �ڷ� �о�� ���� ũ���Դϴ�. 0�̸� �и��� �ʽ��ϴ�.")]
    public float knockbackForce;

    [Tooltip("�� ���Ⱑ ����ü�� �߻��ϴ� ���, ����� �������� �����մϴ�. ����ü�� ���� ��� ����Ӵϴ�.")]
    public GameObject projectilePrefab;

    [Header("�ð��� �Ӽ�")]
    [Tooltip("�÷��̾ �������� �� �տ� ����� ���� �������Դϴ�. �� �����տ��� 3D �𵨰� �ִϸ��̼��� ���Ե� �� �ֽ��ϴ�.")]
    public GameObject weaponPrefab;
}