// StatModifier.cs
using System; // Serializable을 사용하기 위해 필요합니다.

/// <summary>
/// 아이템이 플레이어에게 부여하는 능력치 보너스 정보를 담는 구조체입니다.
/// </summary>
[Serializable] // 유니티 인스펙터에서 볼 수 있도록 직렬화합니다.
public struct StatModifier
{
    /// <summary>
    /// 보너스를 적용할 능력치의 종류를 나타내는 열거형입니다.
    /// PlayerStatSystem 스크립트의 StatType 열거형과 동일합니다.
    /// </summary>
    public StatType statType;

    /// <summary>
    /// 능력치에 적용할 값입니다.
    /// </summary>
    public float value;

    /// <summary>
    /// 이 보너스가 고정값인지(false) 백분율인지(true) 여부입니다.
    /// </summary>
    public bool isPercentage;

    /// <summary>
    /// StatModifier 구조체의 생성자입니다.
    /// </summary>
    /// <param name="type">스탯 종류</param>
    /// <param name="val">값</param>
    /// <param name="isPercent">백분율 여부</param>
    public StatModifier(StatType type, float val, bool isPercent)
    {
        this.statType = type;
        this.value = val;
        this.isPercentage = isPercent;
    }
}