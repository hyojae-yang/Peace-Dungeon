using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 사용하기 위해 필요합니다.
using System.Collections; // 코루틴(IEnumerator)을 사용하기 위해 필요합니다.

/// <summary>
/// 두 개의 UI Image 컴포넌트의 스프라이트를 일정 시간 간격으로 교체하는 관리자 스크립트입니다.
/// 이미지는 배열 내에서 무작위 순서로 선택되어 표시됩니다.
/// </summary>
public class ImageManager : MonoBehaviour
{
    [Header("== 이미지 UI 할당 ==")]
    [Tooltip("첫 번째 메인 이미지 UI 컴포넌트입니다. 여기에 스프라이트가 교체됩니다.")]
    public Image mainImageA;

    [Tooltip("두 번째 메인 이미지 UI 컴포넌트입니다. 여기에 스프라이트가 교체됩니다.")]
    public Image mainImageB;

    [Header("== 스프라이트 리스트 ==")]
    [Tooltip("mainImageA에 무작위로 적용할 스프라이트 배열입니다.")]
    public Sprite[] spriteArrayA;

    [Tooltip("mainImageB에 무작위로 적용할 스프라이트 배열입니다.")]
    public Sprite[] spriteArrayB;

    [Header("== 설정 ==")]
    [Tooltip("mainImageA의 스프라이트 교체 시간 간격(초)입니다. (기본값: 2.0초)")]
    // OCP: A 이미지의 교체 주기를 인스펙터에서 확장 가능하도록 유지
    public float changeIntervalA = 2.0f;

    [Tooltip("mainImageB의 스프라이트 교체 시간 간격(초)입니다. (기본값: 2.0초)")]
    // OCP: B 이미지의 교체 주기를 인스펙터에서 확장 가능하도록 유지
    public float changeIntervalB = 2.0f;

    /// <summary>
    /// 스크립트가 활성화될 때 한 번 호출됩니다. 이미지 교체 코루틴을 시작합니다.
    /// </summary>
    private void Start()
    {
        // UI Image 컴포넌트 할당 여부 확인
        if (mainImageA == null || mainImageB == null)
        {
            Debug.LogError("ImageManager: 메인 이미지 UI가 할당되지 않았습니다. 인스펙터에서 할당해주세요!");
            return;
        }

        // [SRP 적용]: 각 이미지의 스프라이트 변경 로직을 독립적인 코루틴에 할당합니다.
        // 이제 인덱스 오프셋 없이 무작위로 시작되므로, 별도의 초기화 코드가 필요하지 않습니다.
        StartCoroutine(ChangeSpriteRoutine(mainImageA, spriteArrayA, changeIntervalA));
        StartCoroutine(ChangeSpriteRoutine(mainImageB, spriteArrayB, changeIntervalB));
    }

    /// <summary>
    /// 지정된 이미지 UI의 스프라이트를 일정 시간 간격으로 무작위로 교체하는 코루틴입니다.
    /// </summary>
    /// <param name="targetImage">스프라이트를 변경할 Image 컴포넌트입니다.</param>
    /// <param name="spriteArray">교체에 사용할 Sprite 배열입니다.</param>
    /// <param name="interval">스프라이트 교체 시간 간격(초)입니다.</param>
    private IEnumerator ChangeSpriteRoutine(Image targetImage, Sprite[] spriteArray, float interval)
    {
        // 대상 배열이 유효한지 확인합니다.
        if (spriteArray == null || spriteArray.Length == 0)
        {
            Debug.LogWarning($"ImageManager: {targetImage.name}에 할당된 스프라이트 배열이 비어있습니다. 해당 이미지의 교체는 중단됩니다.");
            yield break;
        }

        // 무한 루프를 통해 스프라이트를 계속 교체합니다.
        while (true)
        {
            // 현재 이미지가 보인 후, 다음 교체 주기만큼 기다립니다.
            yield return new WaitForSeconds(interval);

            // 배열 길이 범위 내에서 무작위로 인덱스를 선택합니다. (min 포함, max 미포함)
            int randomIndex = Random.Range(0, spriteArray.Length);

            // 무작위로 선택된 스프라이트로 이미지 UI를 업데이트합니다.
            targetImage.sprite = spriteArray[randomIndex];
        }
    }
}