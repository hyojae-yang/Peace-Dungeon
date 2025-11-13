using UnityEngine;
using TMPro; // TextMeshPro를 사용하려면 필요합니다.

/// <summary>
/// 개별 골드/경험치 팝업 텍스트를 위한 스크립트입니다.
/// 텍스트 설정, 위로 이동하는 애니메이션, 투명화 및 자기 파괴를 담당합니다.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))] // 이 스크립트를 사용하려면 TextMeshPro 컴포넌트가 필수입니다.
public class RewardText : MonoBehaviour
{
    // === 인스펙터 필드 (애니메이션 설정) ===
    [Header("애니메이션 속도")]
    [Tooltip("텍스트가 위로 이동하는 속도입니다.")]
    public float floatSpeed = 1f;
    [Tooltip("텍스트의 표시 시간입니다 (이후 투명화 시작).")]
    public float lifeTime = 1.0f;
    [Tooltip("투명해지는 속도입니다.")]
    public float fadeSpeed = 2f;

    // === 내부 상태 ===
    private TextMeshProUGUI textMesh; // 캐싱된 텍스트 컴포넌트
    private float currentLifeTime;    // 현재 생존 시간
    private Color startColor;         // 초기 텍스트 색상
    private RectTransform rectTransform; // RectTransform 캐싱

    private void Awake()
    {
        // 필요한 컴포넌트들을 캐싱합니다.
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        if (textMesh == null)
        {
            Debug.LogError("[RewardText] TextMeshProUGUI 컴포넌트가 없습니다.");
            enabled = false;
        }
    }

    /// <summary>
    /// RewardTextManager에 의해 호출되어 텍스트 내용을 설정하고 애니메이션을 시작합니다.
    /// </summary>
    /// <param name="content">표시할 텍스트 내용 (예: "+100 G" 또는 "+50 EXP")</param>
    /// <param name="color">텍스트 색상</param>
    public void SetupAndAnimate(string content, Color color)
    {
        // 1. 초기 상태 설정
        currentLifeTime = lifeTime;
        startColor = color;

        // 2. 텍스트 내용 및 색상 적용
        textMesh.text = content;
        textMesh.color = startColor;

        // 3. 애니메이션 시작을 위해 스크립트를 활성화합니다.
        enabled = true;
    }

    private void Update()
    {
        // 1. 텍스트 위로 이동 (Float Up)
        rectTransform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2. 생존 시간 처리
        if (currentLifeTime > 0)
        {
            // 생존 시간이 남아있을 경우 감소만 시킵니다.
            currentLifeTime -= Time.deltaTime;
        }
        else
        {
            // 3. 투명화 (Fade Out)
            float alphaChange = fadeSpeed * Time.deltaTime;
            startColor.a -= alphaChange;
            textMesh.color = startColor;

            // 4. 파괴 조건 확인
            if (startColor.a <= 0)
            {
                // 완전히 투명해지면 오브젝트를 파괴하여 메모리를 해제합니다.
                Destroy(gameObject);
            }
        }
    }

    // 이 스크립트는 RewardTextManager가 텍스트를 설정한 후
    // 매 프레임 애니메이션을 처리합니다.
}