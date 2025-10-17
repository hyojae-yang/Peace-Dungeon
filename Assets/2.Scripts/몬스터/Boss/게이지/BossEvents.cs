using System;
using UnityEngine;

/// <summary>
/// 보스 관련 시스템 이벤트를 모아둔 정적 이벤트 게이트웨이(허브)입니다.
/// DIP: UI 관리자(BossPanelManager)와 보스 객체(BossUIAdapter) 간의 의존성을 분리합니다.
/// </summary>
public static class BossEvents
{
    // 보스 소환 이벤트: 보스 이름, 최대 체력 정보를 담아 전달 (EventArgs 사용)
    /// <summary>
    /// 보스 소환 시 발생하며, UI에 초기 정보를 전달합니다.
    /// </summary>
    public static event EventHandler<BossDataEventArgs> OnBossSpawned;

    // 보스 체력 변경 이벤트: 현재 체력 값만 전달
    /// <summary>
    /// 보스의 현재 체력이 변경될 때 발생하며, 현재 체력 값을 전달합니다.
    /// </summary>
    public static event EventHandler<float> OnBossHealthChanged;

    // 보스 사망 이벤트: 인자 없이 단순 시점만 전달
    /// <summary>
    /// 보스가 사망하여 패널을 비활성화해야 할 때 발생합니다.
    /// </summary>
    public static event EventHandler OnBossDefeated;

    /// <summary>
    /// 보스 소환 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="sender">이 이벤트를 발생시킨 객체 (대부분 BossUIAdapter).</param>
    /// <param name="bossName">보스의 이름.</param>
    /// <param name="maxHealth">보스의 최대 체력.</param>
    public static void RaiseBossSpawned(object sender, string bossName, float maxHealth)
    {
        // Null Check를 통해 구독자가 없을 때의 에러를 방지합니다.
        OnBossSpawned?.Invoke(sender, new BossDataEventArgs(bossName, maxHealth));
        Debug.Log($"[BossEvents] Boss Spawned Event Raised: {bossName}");
    }

    /// <summary>
    /// 보스 체력 변경 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="sender">이 이벤트를 발생시킨 객체 (대부분 BossUIAdapter).</param>
    /// <param name="currentHealth">보스의 현재 체력.</param>
    public static void RaiseBossHealthChanged(object sender, float currentHealth)
    {
        OnBossHealthChanged?.Invoke(sender, currentHealth);
    }

    /// <summary>
    /// 보스 사망 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="sender">이 이벤트를 발생시킨 객체 (대부분 BossUIAdapter).</param>
    public static void RaiseBossDefeated(object sender)
    {
        OnBossDefeated?.Invoke(sender, EventArgs.Empty);
        Debug.Log("[BossEvents] Boss Defeated Event Raised.");
    }
}