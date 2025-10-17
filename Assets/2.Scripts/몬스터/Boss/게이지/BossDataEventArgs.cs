using System;

/// <summary>
/// 보스 소환 시 UI 관리에 필요한 초기 데이터를 담는 이벤트 인자 클래스입니다.
/// OCP: 새로운 정보가 필요하면 이 클래스를 확장하거나 새로운 이벤트를 추가합니다.
/// </summary>
public class BossDataEventArgs : EventArgs
{
    /// <summary>
    /// 보스의 표시될 이름입니다.
    /// </summary>
    public string BossName { get; }

    /// <summary>
    /// 보스의 최대 체력 값입니다. 체력 게이지의 MaxValue 설정에 사용됩니다.
    /// </summary>
    public float MaxHealth { get; }

    /// <summary>
    /// BossDataEventArgs의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="name">보스의 이름.</param>
    /// <param name="maxHp">보스의 최대 체력.</param>
    public BossDataEventArgs(string name, float maxHp)
    {
        BossName = name;
        MaxHealth = maxHp;
    }
}