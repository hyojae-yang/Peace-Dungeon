using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 특수 아이템이 가질 수 있는 효과의 종류입니다.
/// 새로운 효과가 필요하면 여기에 추가하기만 하면 됩니다.
/// </summary>
public enum SpecialEffectType
{
    None,
    Teleport,            // 순간 이동 효과 (예: 특정 위치로 이동)
    GrantPassiveBuff,    // 보유만 해도 적용되는 버프 효과 (예: 공격력 증가)
    InteractWithObject,  // 특정 오브젝트와 상호작용하는 효과 (예: 숨겨진 문 열기)
}

/// <summary>
/// 특수 효과의 종류와 그에 필요한 매개변수를 담는 구조체입니다.
/// 하나의 아이템이 여러 효과를 가질 수 있도록 유연하게 설계되었습니다.
/// </summary>
[System.Serializable]
public struct SpecialEffect
{
    [Tooltip("이 효과가 어떤 종류의 효과인지 정의합니다.")]
    public SpecialEffectType effectType;
    [Tooltip("효과에 필요한 정수형 매개변수입니다. (예: 버프 지속 시간, 특정 ID)")]
    public int intParameter;
    [Tooltip("효과에 필요한 실수형 매개변수입니다. (예: 버프 수치)")]
    public float floatParameter;
    [Tooltip("효과에 필요한 문자열 매개변수입니다. (예: 오브젝트 이름, 지역 이름)")]
    public string stringParameter;
}

/// <summary>
/// 특수 아이템의 데이터를 정의하는 스크립터블 오브젝트입니다.
/// 모든 특수 아이템은 이 스크립트를 기반으로 생성됩니다.
/// BaseItemSO를 상속받아 아이템의 기본 정보를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Special Item", menuName = "Item/Special Item")]
public class SpecialItemSO : BaseItemSO
{
    // === 특수 아이템 고유 속성 ===
    [Header("특수 아이템 고유 속성")]
    [Tooltip("이 아이템이 가질 수 있는 모든 특수 효과의 리스트입니다.")]
    [SerializeField]
    private List<SpecialEffect> specialEffects = new List<SpecialEffect>();

    /// <summary>
    /// 아이템이 가진 모든 특수 효과의 목록을 반환합니다.
    /// 외부 시스템(예: 아이템 사용 로직)에서 이 데이터를 참조하여 효과를 실행합니다.
    /// </summary>
    public List<SpecialEffect> GetSpecialEffects()
    {
        return specialEffects;
    }
    [Tooltip("한 슬롯에 쌓을 수 있는 최대 개수입니다.")]
    public int maxStackCount = 99;
    public override int maxStack => maxStackCount;
}