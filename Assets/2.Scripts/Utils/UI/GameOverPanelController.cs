using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 게임 오버 패널의 시각적 연출(페이드 인 등) 및 상호작용 상태를 관리하는 컴포넌트입니다.
/// 단일 책임 원칙(SRP)에 따라 UI 연출 로직만 담당합니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))] // 페이드 인/아웃을 위해 CanvasGroup 컴포넌트가 필요함을 명시합니다.
public class GameOverPanelController : MonoBehaviour
{
    // 페이드 연출을 위한 CanvasGroup 컴포넌트 참조
    private CanvasGroup canvasGroup;

    [Header("Fade In Settings")]
    [Tooltip("패널이 완전히 나타나는 데 걸리는 시간(초)입니다.")]
    [SerializeField]
    private float fadeInDuration = 1.5f;

    private void Awake()
    {
        // CanvasGroup 컴포넌트를 가져옵니다.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("GameOverPanelController: CanvasGroup 컴포넌트가 필요합니다. Inspector에서 추가해주세요!");
        }

        // 초기 상태: 게임 오브젝트는 Inspector에서 비활성화되어 있거나,
        // 활성화되어 있더라도 완전히 투명하고 상호작용 불가능하게 설정합니다.
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        // (참고: 패널 GameObject 자체는 Inspector에서 비활성화 상태로 두는 것을 권장합니다.)
    }

    /// <summary>
    /// MainSceneManager로부터 게임 오버 신호를 받아 패널을 활성화하고 연출을 시작합니다.
    /// 이 메서드가 호출될 때 Panel GameObject를 활성화합니다.
    /// </summary>
    public void ShowPanelWithEffect()
    {
        // 1. 패널 게임 오브젝트 자체를 활성화합니다.
        gameObject.SetActive(true);

        // 2. 만약 이미 코루틴이 실행 중이라면 중복 실행을 막기 위해 멈춥니다.
        StopAllCoroutines();

        // 3. 연출 코루틴을 시작합니다.
        StartCoroutine(FadeInCoroutine());
    }

    /// <summary>
    /// 알파 값을 0에서 1로 서서히 증가시켜 패널을 페이드 인 시킵니다.
    /// </summary>
    private IEnumerator FadeInCoroutine()
    {
        float timer = 0f;

        // 시작 투명도를 0으로 설정하여 혹시 모를 잔상을 방지합니다.
        canvasGroup.alpha = 0f;

        while (timer < fadeInDuration)
        {
            // 경과 시간 대비 진행률을 계산합니다. (Time.timeScale의 영향을 받지 않도록 UnscaledDeltaTime 사용)
            float progress = timer / fadeInDuration;

            // 알파 값을 0에서 1로 부드럽게 보간(Lerp)합니다.
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 정확한 마무리를 위해 최종 값으로 설정합니다.
        canvasGroup.alpha = 1f;

        // 패널이 완전히 나타나면 상호작용이 가능하게 설정합니다.
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}