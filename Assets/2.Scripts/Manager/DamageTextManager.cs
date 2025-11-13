using UnityEngine;
using TMPro; // TextMeshPro를 사용하려면 필요합니다.
/// <summary>
/// 데미지 팝업 텍스트의 생성, 위치 변환 및 배치를 전담하는 싱글톤 관리자입니다.
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    // === 싱글톤 구현 ===
    public static DamageTextManager Instance { get; private set; }

    // === 데미지 유형별 색상 정의 ===
    private static readonly Color PHYSICAL_COLOR = Color.red;       // 물리 데미지: 빨간색
    private static readonly Color MAGIC_COLOR = Color.blue;        // 마법 데미지: 파란색
    // 고정 데미지: 보라색 (RGB: 255, 0, 255)
    private static readonly Color TRUE_COLOR = new Color(1f, 0f, 1f);
    private static readonly Color DEFAULT_COLOR = Color.white;      // 기본/기타 색상

    // === 인스펙터 필드 ===
    [Header("데미지 텍스트 프리팹")]
    [Tooltip("데미지 텍스트 UI가 포함된 프리팹을 연결합니다.")]
    public GameObject damageTextPrefab;

    [Header("캔버스 설정")]
    [Tooltip("데미지 텍스트를 띄울 Screen Space - Overlay 캔버스입니다.")]
    public Canvas targetCanvas;

    private Camera mainCamera; // 성능 최적화를 위해 메인 카메라를 캐싱합니다.

    private void Awake()
    {
        // 1. 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 2. 메인 카메라 캐싱
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[DamageTextManager] 씬에서 Main Camera를 찾을 수 없습니다!");
        }

        // 3. 캔버스 검증
        if (targetCanvas == null)
        {
            Debug.LogError("[DamageTextManager] targetCanvas가 인스펙터에 할당되지 않았습니다. 할당해주세요.");
            enabled = false;
        }
        else if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("[DamageTextManager] 할당된 캔버스 렌더 모드가 Screen Space - Overlay가 아닙니다. 정확한 좌표 변환을 위해 Overlay 모드를 확인하세요.");
        }
    }

    // DamageType 인수를 추가합니다.
    /// <summary>
    /// 외부(MonsterCombat)에서 데미지 텍스트 팝업을 요청할 때 호출됩니다.
    /// </summary>
    /// <param name="damage">표시할 데미지 값입니다.</param>
    /// <param name="worldPosition">팝업이 시작될 몬스터의 3D 월드 위치입니다.</param>
    /// <param name="damageType">데미지 유형에 따라 텍스트 색상을 결정합니다.</param>
    public void ShowDamage(float damage, Vector3 worldPosition, DamageType damageType) // ⭐️ damageType 인수 추가
    {
        if (mainCamera == null || damageTextPrefab == null || targetCanvas == null)
        {
            return;
        }

        // 1.색상 결정
        Color textColor = GetColorByDamageType(damageType);

        // 2. 3D 월드 좌표를 2D 화면 좌표로 변환합니다.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // 3. 생성: 데미지 텍스트 오브젝트를 생성합니다.
        GameObject textObject = Instantiate(damageTextPrefab, targetCanvas.transform);

        // 4. 배치: 텍스트 UI의 위치를 변환된 화면 좌표로 설정합니다.
        screenPosition.y += 50f;
        textObject.GetComponent<RectTransform>().position = screenPosition;

        // 5. 애니메이션 시작
        DamageText damageText = textObject.GetComponent<DamageText>();
        if (damageText != null)
        {
            // ⭐️ [수정] 결정된 색상을 DamageText 스크립트로 전달합니다.
            damageText.SetupAndAnimate(damage, textColor);
        }
    }

    /// <summary>
    /// 데미지 유형에 따라 텍스트 색상을 반환합니다. (SRP Helper)
    /// </summary>
    private Color GetColorByDamageType(DamageType type)
    {
        switch (type)
        {
            case DamageType.Physical: return PHYSICAL_COLOR;
            case DamageType.Magic: return MAGIC_COLOR;
            case DamageType.True: return TRUE_COLOR;
            default: return DEFAULT_COLOR;
        }
    }
}
