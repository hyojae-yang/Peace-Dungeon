using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 아이템 세트 보너스 단계별 정보를 담는 구조체입니다.
/// </summary>
[Serializable]
public struct SetBonusStep
{
    [Tooltip("효과를 받기 위해 필요한 세트 아이템의 개수입니다.")]
    public int requiredCount;
    [Tooltip("이 단계에서 추가로 부여될 능력치입니다.")]
    public List<StatModifier> bonusStats;
}

/// <summary>
/// 세트 아이템의 보너스 효과 데이터를 정의하는 스크립터블 오브젝트입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Set Bonus Data", menuName = "Item/Set Bonus Data")]
public class SetBonusDataSO : ScriptableObject
{
    [Tooltip("해당 세트를 식별하는 고유 ID입니다. EquipmentItemSO의 setID와 동일해야 합니다.")]
    public string setID;

    [Tooltip("UI에 표시될 세트의 이름입니다.")]
    public string setName;

    [Tooltip("세트 아이템 개수에 따른 보너스 효과 목록입니다.")]
    public List<SetBonusStep> bonusSteps;
}