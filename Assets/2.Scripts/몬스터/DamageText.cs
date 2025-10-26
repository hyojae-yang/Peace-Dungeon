using UnityEngine;
using TMPro; // TextMeshPro 컴포넌트를 사용하기 위해 필요합니다.
using System.Collections; // 코루틴 사용을 위해 필요합니다.

/// <summary>
/// 개별 데미지 팝업 텍스트의 수명과 애니메이션을 관리하는 스크립트입니다.
/// (DamageTextManager에 의해 생성된 프리팹에 부착됩니다.)
/// </summary>
public class DamageText : MonoBehaviour
{
    // === 인스펙터 필드 (Inspector Fields) ===

    [Header("컴포넌트 참조")]
    [Tooltip("데미지 숫자를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("애니메이션 설정")]
    [Tooltip("텍스트가 떠오르는 총 시간(Duration)입니다.")]
    public float lifetime = 1.0f;
    [Tooltip("텍스트가 떠오르는 속도(Y축 이동 거리)입니다.")]
    public float moveSpeed = 100f;
    [Tooltip("텍스트의 투명도가 사라지는 시작 시간 비율 (0.0f ~ 1.0f)입니다.")]
    public float fadeOutStartRatio = 0.5f;

    // === 내부 상태 변수 ===
    private float timer; // 수명 관리를 위한 타이머
    private Color originalColor; // 페이드 아웃을 위한 원래 색상 (결정된 데미지 타입 색상)

    private void Awake()
    {
        // [팩트 체크] 필요한 컴포넌트가 제대로 연결되었는지 확인합니다.
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                Debug.LogError("[DamageText] TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 인스펙터에 할당하거나 오브젝트에 추가해 주세요.");
                enabled = false;
                return;
            }
        }

        // Awake 시점의 기본 색상을 저장하지 않고, SetupAndAnimate에서 받은 색상을 사용합니다.
    }

    // ⭐️ [수정] Color 인수를 추가하여, 받은 색상을 적용합니다.
    /// <summary>
    /// DamageTextManager가 호출하는 초기 설정 및 애니메이션 시작 메서드입니다.
    /// </summary>
    /// <param name="damage">표시할 데미지 값입니다.</param>
    /// <param name="color">DamageTextManager가 결정한, 데미지 유형에 따른 색상입니다.</param>
    public void SetupAndAnimate(float damage, Color color)
    {
        // 1. 데미지 값 표시
        // 정수 데미지는 정수로, 소수점 데미지는 소수점 첫째 자리까지 표시하도록 처리합니다.
        textComponent.text = Mathf.RoundToInt(damage).ToString();

        // 2. ⭐️ 색상 적용: 전달받은 색상을 저장하고 즉시 적용합니다.
        originalColor = color; // 페이드 아웃 코루틴이 사용할 기준 색상을 저장합니다.
        textComponent.color = originalColor;

        // 3. 코루틴 시작: 애니메이션 수명주기를 관리합니다.
        StopAllCoroutines();
        StartCoroutine(AnimateSequence());
    }

    /// <summary>
    /// 텍스트를 위로 이동시키고, 투명하게 사라지게 한 후 파괴하는 코루틴입니다.
    /// </summary>
    private IEnumerator AnimateSequence()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        timer = 0f;

        // 페이드 아웃이 시작될 타이밍을 계산합니다.
        float fadeOutStartTime = lifetime * fadeOutStartRatio;

        while (timer < lifetime)
        {
            // 1. 텍스트 이동 (Translate)
            rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

            // 2. 페이드 아웃 (Fade Out)
            if (timer >= fadeOutStartTime)
            {
                float fadeProgress = (timer - fadeOutStartTime) / (lifetime - fadeOutStartTime);

                // 알파 값을 계산하여 적용합니다. (1.0f -> 0.0f)
                Color currentColor = originalColor;
                currentColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                textComponent.color = currentColor;
            }

            // 3. 타이머 업데이트
            timer += Time.deltaTime;

            yield return null; // 다음 프레임까지 대기
        }

        // 4. 수명 종료 후 오브젝트 파괴 (풀링 대신 생성/파괴 방식 채택)
        Destroy(gameObject);
    }

    // ⭐️ [정리] SetTextColor는 SetupAndAnimate로 통합되었으므로 제거하거나 주석 처리합니다.
    /*
    public void SetTextColor(Color color)
    {
        originalColor = color;
        textComponent.color = color;
    }
    */
}
