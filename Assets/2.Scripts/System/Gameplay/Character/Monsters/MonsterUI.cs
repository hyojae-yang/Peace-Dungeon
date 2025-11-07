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
        // 1. 몬스터 전투 스크립트(MonsterCombat) 참조 가져오기
        monsterCombat = GetComponent<MonsterCombat>();

        if (monsterCombat == null)
        {
            if (GetComponentInParent<MonsterCombat>() != null)
            {
                monsterCombat = GetComponentInParent<MonsterCombat>();
            }

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

        // 2. 최대 체력 정보 저장 및 슬라이더 초기 설정
        // Monster 클래스의 MaxHealth 속성을 사용하여 최대 체력 값을 가져옵니다.
        // 주의: MonsterCombat에 Monster 컴포넌트(monster)가 public으로 노출되어 있어야 합니다.
        // (이전 대화에서 제공하신 Monster.cs를 통해 이 접근이 가능하다고 가정합니다.)
        maxHealth = monsterCombat.monster.MaxHealth;

        // [핵심 수정 1] Slider의 최대값을 1.0f로 고정합니다. (비율 기반)
        // 몬스터의 실제 MaxHealth 값과 관계없이 슬라이더는 항상 0%~100% 비율을 나타냅니다.
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;

        // 3. OCP 준수: 체력 변경 이벤트 구독 
        monsterCombat.OnHealthUpdated += UpdateHealthBar;

        // 4. 초기 체력 상태 설정
        // 시작 시점에는 보통 체력이 최대 체력과 같으므로 1.0f (100%)로 설정합니다.
        // 현재 체력 / 최대 체력 = 1.0f
        healthSlider.value = monsterCombat.GetCurrentHealth() / maxHealth;

        // 5. 초기 체력바 시각화 (만피일 때 숨기기)
        SetHealthBarVisibility(healthSlider.value < 1.0f);
    }

    // === 메인 로직 및 메모리 관리 ===

    /// <summary>
    /// 몬스터의 체력이 변경될 때마다 MonsterCombat에 의해 호출됩니다.
    /// 이 메서드가 체력 슬라이더의 값을 실제로 갱신합니다.
    /// </summary>
    /// <param name="currentHealth">현재 몬스터의 남은 체력 값</param>
    private void UpdateHealthBar(float currentHealth)
    {
        // [핵심 수정 2] 슬라이더 값 갱신: 현재 체력이 아닌 '비율'을 대입합니다.
        // 몬스터의 체력이 아무리 커져도 슬라이더는 0.0f~1.0f 사이의 값만 사용합니다.
        // 예: 현재 체력 5250 / 최대 체력 10500 = 0.5f
        float healthRatio = currentHealth / maxHealth;
        healthSlider.value = healthRatio;

        // 2. 체력바 표시/숨김 관리
        // 만피(비율 1.0f)가 아닐 때만 보이게 합니다.
        SetHealthBarVisibility(healthRatio < 1.0f);

        // 3. 몬스터 사망 시 처리
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