using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 무기 아이템의 세부 종류를 정의하는 열거형입니다.
/// 이 열거형은 무기 아이템 데이터 스크립터블 오브젝트에서 사용됩니다.
/// </summary>
public enum WeaponType
{
    None,
    Sword,      // 검 (근거리)
    Staff,      // 지팡이 (원거리 마법)
    Axe,        // 도끼 (근거리, 횡베기)
    Bow,        // 활 (원거리 물리)
    Spear,      // 창 (중거리)
}


/// <summary>
/// 무기 아이템의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// EquipmentItemSO를 상속받아 무기만의 고유 속성을 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Item/Weapon Item")]
public class WeaponItemSO : EquipmentItemSO
{
    // === 무기 고유 속성 ===
    [Header("무기 고유 속성")]
    [Tooltip("이 무기 아이템의 세부 종류를 정의합니다. (예: Sword, Axe 등)")]
    public WeaponType weaponType;

    [Tooltip("이 무기가 가하는 데미지의 종류를 정의합니다. (예: Physical, Magic)")]
    public DamageType damageType;

    [Tooltip("공격이 닿는 최대 거리(반경)입니다.")]
    public float attackRange;

    [Tooltip("공격이 닿는 부채꼴의 각도입니다. (총 각도)")]
    [Range(0, 360)]
    public float attackAngle;

    [Tooltip("이 무기의 공격 속도입니다. (초당 공격 횟수)")]
    public float attackSpeed;

    [Tooltip("공격에 맞은 대상을 뒤로 밀어내는 힘의 크기입니다. 0이면 밀리지 않습니다.")]
    public float knockbackForce;

    [Tooltip("이 무기가 투사체를 발사하는 경우, 사용할 프리팹을 연결합니다. 투사체가 없을 경우 비워둡니다.")]
    public GameObject projectilePrefab;

    [Header("시각적 속성")]
    [Tooltip("플레이어가 장착했을 때 손에 쥐어질 무기 프리팹입니다. 이 프리팹에는 3D 모델과 애니메이션이 포함될 수 있습니다.")]
    public GameObject weaponPrefab;
}