using UnityEngine;
using TMPro; // <<<< --- TMP 네임스페이스 추가
using UnityEngine.UI; // Slider 사용을 위해 유지
using System;

/// <summary>
/// 보스 정보(체력 게이지, 이름, 체력 텍스트)를 표시하는 UI 패널을 관리합니다.
/// SRP: 오직 UI의 활성화/비활성화 및 데이터 표시에 대한 책임만 가집니다.
/// DIP: BossEvents라는 추상적인 이벤트에 의존하여 보스 객체와 느슨하게 결합됩니다.
/// </summary>
public class BossPanelManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("보스 정보가 포함된 전체 UI 패널 (활성화/비활성화 대상)")]
    [SerializeField] private GameObject bossPanel;

    // 텍스트 컴포넌트 변경: Text -> TextMeshProUGUI
    [Tooltip("보스의 이름을 표시하는 텍스트 컴포넌트")]
    [SerializeField] private TextMeshProUGUI bossNameText; // <<<< --- 변경

    // 텍스트 컴포넌트 변경: Text -> TextMeshProUGUI
    [Tooltip("보스의 현재 체력/최대 체력을 표시하는 텍스트 컴포넌트")]
    [SerializeField] private TextMeshProUGUI healthValueText; // <<<< --- 변경

    [Tooltip("보스의 체력 게이지를 시각적으로 보여주는 Slider 컴포넌트")]
    [SerializeField] private Slider healthBarSlider;

    // 현재 보스의 최대 체력을 저장하는 변수 (체력 텍스트 업데이트 계산용)
    private float currentBossMaxHealth = 1f;

    private void Awake()
    {
        // 초기 상태: UI 패널 비활성화
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("BossPanelManager: bossPanel GameObject가 연결되지 않았습니다!");
        }
    }

    /// <summary>
    /// 오브젝트 활성화 시 BossEvents를 구독하여 통신을 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        // BossEvents 구독: 이벤트 발생 시 해당 핸들러 메서드 호출
        BossEvents.OnBossSpawned += HandleBossSpawned;
        BossEvents.OnBossHealthChanged += HandleBossHealthChanged;
        BossEvents.OnBossDefeated += HandleBossDefeated;
    }

    /// <summary>
    /// 오브젝트 비활성화 시 구독을 해제하여 메모리 누수를 방지합니다.
    /// </summary>
    private void OnDisable()
    {
        // BossEvents 구독 해제
        BossEvents.OnBossSpawned -= HandleBossSpawned;
        BossEvents.OnBossHealthChanged -= HandleBossHealthChanged;
        BossEvents.OnBossDefeated -= HandleBossDefeated;
    }

    // ================== 이벤트 핸들러 ==================

    /// <summary>
    /// Boss Spawned 이벤트를 처리: 패널 활성화 및 초기 정보 설정.
    /// </summary>
    private void HandleBossSpawned(object sender, BossDataEventArgs args)
    {
        // 1. 패널 활성화
        if (bossPanel != null) bossPanel.SetActive(true); // null 체크 추가

        // 2. 최대 체력 및 이름 설정 (TMP 컴포넌트 사용)
        if (bossNameText != null) bossNameText.text = args.BossName; // null 체크 추가
        currentBossMaxHealth = args.MaxHealth;

        // 3. 슬라이더 초기 설정
        if (healthBarSlider != null) // null 체크 추가
        {
            healthBarSlider.maxValue = args.MaxHealth;
            healthBarSlider.value = args.MaxHealth; // 초기 체력은 최대 체력과 동일
        }

        // 4. 텍스트 초기 설정
        UpdateHealthText(args.MaxHealth);
    }

    /// <summary>
    /// Boss Health Changed 이벤트를 처리: 게이지 및 텍스트 업데이트.
    /// </summary>
    private void HandleBossHealthChanged(object sender, float currentHealth)
    {
        // 슬라이더 및 텍스트 업데이트
        if (healthBarSlider != null) healthBarSlider.value = currentHealth; // null 체크 추가
        UpdateHealthText(currentHealth);
    }

    /// <summary>
    /// Boss Defeated 이벤트를 처리: 패널 비활성화.
    /// </summary>
    private void HandleBossDefeated(object sender, EventArgs args)
    {
        if (bossPanel != null) bossPanel.SetActive(false); // null 체크 추가
        // (선택 사항: 사망 시 멋진 애니메이션이나 승리 메시지 출력 로직 추가 가능)
    }

    // ================== 보조 메서드 ==================

    /// <summary>
    /// 체력 값을 포맷하여 텍스트 UI에 표시합니다. (TMP 컴포넌트 사용)
    /// </summary>
    /// <param name="currentHealth">현재 남은 체력 값.</param>
    private void UpdateHealthText(float currentHealth)
    {
        // 텍스트 컴포넌트 사용 (TMP 컴포넌트 사용)
        if (healthValueText != null) // null 체크 추가
        {
            // 체력 값을 소수점 없이 정수로 표시
            healthValueText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(currentBossMaxHealth)}";
        }
    }
}