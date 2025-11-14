using UnityEngine;
using TMPro; // TextMeshPro를 사용하려면 필요합니다.

/// <summary>
/// 골드 및 경험치 획득/소비 텍스트 팝업의 생성, 위치 변환 및 배치를 전담하는 싱글톤 관리자입니다.
/// </summary>
public class RewardTextManager : MonoBehaviour
{
    // === 싱글톤 구현 ===
    public static RewardTextManager Instance { get; private set; }

    // === 상수 정의 ===
    private const string GOLD_SUFFIX = "원"; // 골드 접미사
    private const string EXP_SUFFIX = ""; // 경험치 접미사 (통일성을 위해 " " 대신 " EXP"로 변경했습니다.)

    // 팝업 위치 오프셋 (데미지와 구분하고 골드/경험치 간 겹침 방지)
    private const float BASE_OFFSET_Y = 50f;  // 데미지 팝업 (50f)보다 높게 띄울 기본 Y축 오프셋
    private const float GOLD_OFFSET_X = -50f;  // 골드는 중앙에서 왼쪽으로 띄웁니다.
    private const float EXP_OFFSET_X = 50f;   // 경험치는 중앙에서 오른쪽으로 띄웁니다.

    // === 보상 유형별 색상 정의 ===
    // 획득(양수)은 주로 긍정적인 색상, 소비(음수)는 부정적인 색상 또는 회색을 사용합니다.
    private static readonly Color GOLD_GAIN_COLOR = Color.yellow; // 골드 획득: 노란색
    private static readonly Color EXP_COLOR = Color.green; // 경험치 획득: 녹색

    // === 인스펙터 필드 ===
    [Header("보상 텍스트 프리팹")]
    [Tooltip("골드/경험치 텍스트 UI가 포함된 프리팹을 연결합니다.")]
    public GameObject rewardTextPrefab;

    [Header("캔버스 설정")]
    [Tooltip("보상 텍스트를 띄울 Screen Space - Overlay 캔버스입니다.")]
    public Canvas targetCanvas;

    private Camera mainCamera; // 성능 최적화를 위해 메인 카메라를 캐싱합니다.
    private PlayerCharacter playerCharacter; // 팝업 위치를 위해 플레이어 참조 캐싱

    private void Awake()
    {
        // 1. 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // 이전에 있던 DontDestroyOnLoad는 제거했습니다. (선택 사항이므로 제거 가능)

        // 2. 메인 카메라 캐싱
        mainCamera = Camera.main;

        // 3. PlayerCharacter 참조 획득 (Start에서 진행하는 것이 안전할 수 있지만, 여기서는 빠르게 접근합니다.)
        playerCharacter = PlayerCharacter.Instance;

        // 4. 캔버스 및 프리팹 검증
        if (targetCanvas == null || rewardTextPrefab == null)
        {
            Debug.LogError("[RewardTextManager] 캔버스 또는 프리팹이 할당되지 않았습니다. 할당해주세요.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // [핵심] PlayerStats와 PlayerLevelUp의 이벤트를 구독합니다.
        if (PlayerCharacter.Instance != null)
        {
            PlayerCharacter.Instance.playerStats.OnGoldAdded += OnGoldValueChange;
            // PlayerLevelUp이 static 이벤트이므로, null 체크 없이 구독합니다.
            PlayerLevelUp.OnExperienceAdded += OnExperienceGained;
        }
    }

    private void OnDisable()
    {
        // 구독 해지: 메모리 누수 방지
        if (PlayerCharacter.Instance != null)
        {
            PlayerCharacter.Instance.playerStats.OnGoldAdded -= OnGoldValueChange;
        }
        // PlayerLevelUp이 static 이벤트이므로, null 체크 없이 구독 해지합니다.
        PlayerLevelUp.OnExperienceAdded -= OnExperienceGained;
    }

    // === 이벤트 핸들러 ===

    /// <summary>
    /// PlayerStats.OnGoldAdded 이벤트 발생 시 호출됩니다.
    /// 골드 소비(음수)는 필터링되어 획득(양수)만 처리됩니다.
    /// </summary>
    /// <param name="amount">골드 변화량 (획득:+양수, 소비:-음수)</param>
    private void OnGoldValueChange(int amount)
    {
        // 골드 소비(amount < 0)일 경우 텍스트를 띄우지 않고 바로 종료합니다.
        if (amount < 0)
        {
            return;
        }

        // 골드 팝업 생성 로직을 담당합니다.
        ShowReward(amount, true); // isGold: true
    }

    /// <summary>
    /// PlayerLevelUp.OnExperienceAdded 이벤트 발생 시 호출됩니다.
    /// </summary>
    /// <param name="amount">획득한 경험치량 (항상 양수)</param>
    private void OnExperienceGained(long amount)
    {
        // 경험치 팝업 생성 로직을 담당합니다.
        ShowReward(amount, false); // isGold: false
    }

    // === 팝업 생성 및 배치 로직 ===

    /// <summary>
    /// 골드 또는 경험치 팝업을 화면에 띄웁니다.
    /// </summary>
    /// <param name="amount">표시할 수량</param>
    /// <param name="isGold">골드(true)인지 경험치(false)인지 구분</param>
    public void ShowReward(long amount, bool isGold)
    {
        if (mainCamera == null || rewardTextPrefab == null || targetCanvas == null || playerCharacter == null)
        {
            return;
        }

        // 1. 팝업이 시작될 3D 월드 위치 (플레이어 캐릭터 위치)
        Vector3 worldPosition = playerCharacter.transform.position;

        // 2. 3D 월드 좌표를 2D 화면 좌표로 변환합니다.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // 3. 텍스트 설정 (색상, 내용)
        Color textColor;
        string textContent;
        float offsetX; // X축 오프셋 변수

        if (isGold)
        {
            textColor = GOLD_GAIN_COLOR;
            textContent = $"+{amount}{GOLD_SUFFIX}";
            offsetX = GOLD_OFFSET_X; //골드는 왼쪽으로
        }
        else // 경험치 획득
        {
            textColor = EXP_COLOR;
            textContent = $"+{amount}{EXP_SUFFIX}";
            offsetX = EXP_OFFSET_X; //경험치는 오른쪽으로
        }

        // 4. 생성: 보상 텍스트 오브젝트를 생성합니다.
        GameObject textObject = Instantiate(rewardTextPrefab, targetCanvas.transform);

        // 5. 배치: 텍스트 UI의 위치를 변환된 화면 좌표로 설정합니다.
        screenPosition.y += BASE_OFFSET_Y; // 기본 Y 오프셋 적용
        screenPosition.x += offsetX;      // X 오프셋 적용
        textObject.GetComponent<RectTransform>().position = screenPosition;

        // 6. 애니메이션 시작
        RewardText rewardText = textObject.GetComponent<RewardText>();
        if (rewardText != null)
        {
            rewardText.SetupAndAnimate(textContent, textColor);
        }
        else
        {
            // Fallback 로직 (RewardText 스크립트가 없을 경우)
            TextMeshProUGUI tmp = textObject.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = textContent;
                tmp.color = textColor;
                Destroy(textObject, 1.5f);
            }
        }
    }
}