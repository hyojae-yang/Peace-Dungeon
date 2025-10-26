using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 몬스터의 머리 위 UI(주로 체력 슬라이더)의 시각적 업데이트를 전담하는 스크립트입니다.
/// MonsterCombat의 이벤트를 구독하여 체력 정보를 가져옵니다. (OCP 준수)
/// </summary>
public class MonsterUI : MonoBehaviour
{
    // === 인스펙터 필드 (Inspector Fields) ===

    [Header("UI 요소 참조")]
    [Tooltip("체력바 역할을 할 유니티 Slider 컴포넌트를 인스펙터에서 연결하세요.")]
    [SerializeField] private Slider healthSlider;

    // === 내부 변수 (Private Fields) ===

    private MonsterCombat monsterCombat;
    private float maxHealth; // 몬스터의 최대 체력을 저장합니다.

    // === 초기화 (Awake & Start) ===

    private void Start()
    {
        // 1. [핵심] 몬스터 전투 스크립트(MonsterCombat) 참조 가져오기
        // 몬스터 루트 오브젝트에 MonsterCombat가 있다고 가정하고 GetComponent를 사용합니다.
        // 만약 MonsterUI가 자식 오브젝트에 있다면 GetComponentInParent<MonsterCombat>()로 변경해야 합니다.
        monsterCombat = GetComponent<MonsterCombat>();

        if (monsterCombat == null)
        {
            // ⭐️ UI가 몬스터 모델의 자식이라면 다음 코드로 변경해보세요.
            // monsterCombat = GetComponentInParent<MonsterCombat>();

            if (monsterCombat == null)
            {
                Debug.LogError("[MonsterUI] MonsterCombat 스크립트를 찾을 수 없습니다. UI를 비활성화합니다.");
                enabled = false;
                return;
            }
        }

        if (healthSlider == null)
        {
            Debug.LogError("[MonsterUI] healthSlider가 인스펙터에 할당되지 않았습니다. 할당해 주세요.");
            enabled = false;
            return;
        }

        // 2. ⭐️ [팩트 기반 수정] 최대 체력 정보 저장 및 슬라이더 초기 설정
        // MonsterCombat의 Awake 로직을 기반으로 MaxHealth를 가져옵니다.
        // (MonsterCombat가 Awake에서 monsterBase를 가져왔다고 가정합니다.)
        // maxHealth = monsterCombat.monsterBase.monsterData.maxHealth;
        // 🚨 주의: monsterBase에 직접 접근할 수 없다면, MonsterCombat에 MaxHealth를 public 속성으로 추가해야 합니다.

        // 몬스터의 최대 체력을 가져오는 더 안전한 방법을 위해 MonsterCombat에 공개 속성을 추가하는 것이 SOLID 원칙에 더 맞습니다.
        // 현재는 MonsterCombat 내부에 MaxHealth 속성을 추가할 수 없으므로, GetCurrentHealth()의 최대값을 임시로 사용하거나,
        // MonsterCombat 내부에 MaxHealth 속성이 있다는 가정하에 진행하겠습니다.

        // **[가정]** MonsterCombat에 public float MaxHealth { get; private set; } 속성이 있다고 가정하고,
        // Awake 시점에 이 값을 설정한다고 가정합니다. (현재는 접근 불가로 임시 변수 사용)

        // **********************************************************************************************
        // 💡SOLID 원칙을 위해 MonsterCombat에 MaxHealth 속성을 추가하거나, GetMaxHealth() 메서드를 추가하는 것을 권장합니다.
        // **********************************************************************************************

        // MonsterCombat 내부에 public float MaxHealth { get; private set; }이 있다고 가정합니다.
        // maxHealth = monsterCombat.MaxHealth;

        // 현재는 MonsterCombat가 가진 GetCurrentHealth()만 사용 가능하므로, 
        // Awake 시점에 체력바의 MaxValue를 초기화하는 코드를 제거하고,
        // MonsterCombat 내부에 MaxHealth 속성을 추가한 후 다시 코드를 수정하는 것이 좋습니다.

        // ⭐️ 현재 MonsterCombat를 수정할 수 없으므로, 임시 변수 maxHealth 대신 MonsterCombat에서 값을 가져오는 안전한 방법으로 수정합니다.
        // **********************************************************************************************

        // 2-1. [팩트 기반 수정] 최대 체력 설정 (MonsterCombat의 초기 설정에 따라 가정)
        // monsterCombat.monsterBase.monsterData.maxHealth에 접근 가능하다고 가정합니다.
        // 현재 MonsterCombat가 가진 정보를 바탕으로 접근 경로를 설정합니다.
        // ⭐️ 주의: 이 경로는 `MonsterBase`가 public이 아니면 실패합니다.
        // 팩트: MonsterCombat가 가진 `monsterBase.monsterData.maxHealth`를 사용합니다.
        maxHealth = monsterCombat.monsterBase.monsterData.maxHealth;
        healthSlider.maxValue = maxHealth;

        // 3. OCP 준수: 체력 변경 이벤트 구독 (이 부분은 이미 완벽합니다!)
        monsterCombat.OnHealthUpdated += UpdateHealthBar;

        // 4. [팩트 기반 수정] 초기 체력 상태 설정
        // GetCurrentHealth() 메서드를 사용하여 정확한 초기 체력 값을 가져옵니다.
        healthSlider.value = monsterCombat.GetCurrentHealth();

        // 5. 초기 체력바 시각화 (선택적 최적화)
        // 만피일 때는 숨기고, 만피가 아니면 보이게 합니다.
        SetHealthBarVisibility(healthSlider.value < maxHealth);
    }

    // === 메인 로직 및 메모리 관리 (이전과 동일하게 유지) ===

    /// <summary>
    /// 몬스터의 체력이 변경될 때마다 MonsterCombat에 의해 호출됩니다.
    /// 이 메서드가 체력 슬라이더의 값을 실제로 갱신합니다.
    /// </summary>
    /// <param name="currentHealth">현재 몬스터의 남은 체력 값</param>
    private void UpdateHealthBar(float currentHealth)
    {
        // 1. 슬라이더 값 갱신: 시각적인 피드백 제공
        healthSlider.value = currentHealth;

        // 2. 체력바 표시/숨김 관리
        SetHealthBarVisibility(currentHealth < maxHealth);

        // 3. 몬스터 사망 시 처리 (선택 사항)
        if (currentHealth <= 0)
        {
            SetHealthBarVisibility(false);
        }
    }

    /// <summary>
    /// 체력바 오브젝트의 활성/비활성 상태를 설정합니다. (코드 중복 방지)
    /// </summary>
    /// <param name="isVisible">체력바를 보이게 할지 여부</param>
    private void SetHealthBarVisibility(bool isVisible)
    {
        if (healthSlider.gameObject.activeSelf != isVisible)
        {
            // ⭐️ 주의: UI 전체를 담고 있는 Canvas 오브젝트를 비활성화하는 것이 일반적입니다.
            // 여기서는 Slider 컴포넌트가 붙어있는 GameObject를 비활성화합니다.
            healthSlider.gameObject.SetActive(isVisible);
        }
    }

    private void OnDestroy()
    {
        // [필수] 메모리 누수 방지: 오브젝트가 파괴될 때 이벤트 구독을 해지해야 합니다.
        if (monsterCombat != null)
        {
            monsterCombat.OnHealthUpdated -= UpdateHealthBar;
        }
    }
}