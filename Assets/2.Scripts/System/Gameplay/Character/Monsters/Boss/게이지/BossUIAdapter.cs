using UnityEngine;
using System;

/// <summary>
/// 보스 오브젝트에만 부착되어, 공유 데미지 스크립트(MonsterCombat)의 정보를
/// 보스 UI 전용 이벤트 시스템(BossEvents)으로 변환하여 전달하는 어댑터 클래스입니다.
/// SRP: 오직 이벤트 중개 및 보스 식별 정보 제공의 책임만 가집니다.
/// LSP: 일반 몬스터의 MonsterCombat 로직을 건드리지 않고 보스만의 기능을 확장합니다.
/// </summary>
public class BossUIAdapter : MonoBehaviour
{
    [Header("Boss Identification")]
    [Tooltip("보스 체력 바에 표시될 보스의 고유 이름입니다.")]
    [SerializeField] private string bossName = "Dungeon Final Boss";

    // 의존성: 같은 오브젝트에 있는 MonsterCombat 및 MonsterBase 컴포넌트
    private MonsterCombat monsterCombat;
    private Monster monster;
    private float maxHealth;

    /// <summary>
    /// 컴포넌트 초기화 및 레퍼런스를 확보합니다.
    /// </summary>
    private void Awake()
    {
        // 필요한 컴포넌트 레퍼런스 확보
        monsterCombat = GetComponent<MonsterCombat>();
        monster = GetComponent<Monster>();

        if (monsterCombat == null || monster == null)
        {
            Debug.LogError($"BossUIAdapter ERROR: '{gameObject.name}' 오브젝트에 MonsterCombat 또는 MonsterBase 컴포넌트가 없어 비활성화됩니다. 이 오브젝트가 보스가 맞는지 확인하세요.");
            // 컴포넌트가 없으면 해당 어댑터의 기능을 중지합니다.
            enabled = false;
            return;
        }

        // MonsterBase에서 최대 체력 정보를 미리 가져옵니다.
        maxHealth = monster.MaxHealth;
    }

    /// <summary>
    /// 오브젝트 활성화 시 MonsterCombat의 훅을 구독하고, 보스 소환 이벤트를 발생시킵니다.
    /// </summary>
    private void OnEnable()
    {
        if (monsterCombat != null)
        {
            // 1. MonsterCombat의 '훅' 구독
            // 이 Adapter만이 구독하므로, 일반 몬스터들은 이벤트 로직에 관여하지 않습니다.
            monsterCombat.OnHealthUpdated += HandleHealthUpdate;
            monsterCombat.OnDefeated += HandleBossDefeated;

            // 2. 보스 소환 이벤트 발생 (UI 활성화 및 초기 체력/이름 설정용)
            // DIP: 구체적인 UI 관리자(BossPanelManager)를 호출하지 않고, 추상적인 이벤트를 발생시킵니다.
            BossEvents.RaiseBossSpawned(this, bossName, maxHealth);
        }
    }

    /// <summary>
    /// 오브젝트 비활성화 시 구독을 해제하여 메모리 누수를 방지합니다.
    /// </summary>
    private void OnDisable()
    {
        if (monsterCombat != null)
        {
            // 구독 해제
            monsterCombat.OnHealthUpdated -= HandleHealthUpdate;
            monsterCombat.OnDefeated -= HandleBossDefeated;
        }
    }

    /// <summary>
    /// MonsterCombat으로부터 체력 변경 정보를 받아 BossEvents로 변환하여 전달합니다.
    /// </summary>
    /// <param name="currentHealth">MonsterCombat이 전달한 현재 남은 체력 값.</param>
    private void HandleHealthUpdate(float currentHealth)
    {
        // 현재 체력 정보를 UI 시스템에 전달합니다.
        BossEvents.RaiseBossHealthChanged(this, currentHealth);
    }

    /// <summary>
    /// MonsterCombat으로부터 사망 정보를 받아 BossEvents로 변환하여 전달합니다.
    /// </summary>
    private void HandleBossDefeated()
    {
        // 보스 사망 이벤트를 UI 시스템에 전달하여 패널 비활성화를 요청합니다.
        BossEvents.RaiseBossDefeated(this);
    }
}