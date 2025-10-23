using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 사용하기 위해 필요합니다.
using System.Collections; // 코루틴(IEnumerator)을 사용하기 위해 필요합니다.

/// <summary>
/// 두 개의 UI Image 컴포넌트의 스프라이트를 일정 시간 간격으로 교체하는 관리자 스크립트입니다.
/// </summary>
public class ImageManager : MonoBehaviour
{
    [Header("== 이미지 UI 할당 ==")]
    [Tooltip("첫 번째 메인 이미지 UI 컴포넌트입니다. 여기에 스프라이트가 교체됩니다.")]
    public Image mainImageA;

    [Tooltip("두 번째 메인 이미지 UI 컴포넌트입니다. 여기에 스프라이트가 교체됩니다.")]
    public Image mainImageB;

    [Header("== 스프라이트 리스트 ==")]
    [Tooltip("mainImageA에 순차적으로 적용할 스프라이트 배열입니다.")]
    public Sprite[] spriteArrayA;

    [Tooltip("mainImageB에 순차적으로 적용할 스프라이트 배열입니다.")]
    public Sprite[] spriteArrayB;

    [Header("== 설정 ==")]
    [Tooltip("스프라이트가 교체될 시간 간격(초)입니다. (기본값: 2.0초)")]
    // [SOLID 원칙 - OCP(개방-폐쇄 원칙) 부분적 준수]: 
    // 교체 주기라는 '확장' 요소는 public으로 인스펙터에 '개방'하여 코드 수정 없이 외부에서 변경 가능하게 합니다.
    public float changeInterval = 2.0f;

    // 현재 mainImageA와 mainImageB에 적용된 스프라이트 배열의 인덱스를 추적합니다.
    // 이 변수들은 이제 코루틴 내부에서 직접 접근하여 갱신됩니다.
    private int _currentIndexA = 0;
    private int _currentIndexB = 0;

    /// <summary>
    /// 스크립트가 활성화될 때 한 번 호출됩니다. 이미지 교체 코루틴을 시작합니다.
    /// </summary>
    private void Start()
    {
        // UI Image 컴포넌트가 할당되었는지 확인하여 Null 참조 예외를 방지합니다.
        if (mainImageA == null || mainImageB == null)
        {
            Debug.LogError("ImageManager: 메인 이미지 UI(mainImageA 또는 mainImageB)가 할당되지 않았습니다. 인스펙터에서 할당해주세요!");
            return;
        }

        // [SOLID 원칙 - SRP(단일 책임 원칙) 적용]: 
        // 각 이미지의 스프라이트 변경 로직을 독립적인 코루틴에 할당하여 책임을 분리합니다.
        // A와 B는 이제 완전히 독립적으로 작동합니다.
        StartCoroutine(ChangeSpriteRoutineA());
        StartCoroutine(ChangeSpriteRoutineB());
    }

    /// <summary>
    /// mainImageA의 스프라이트를 일정 시간 간격으로 순차적으로 교체하는 코루틴입니다.
    /// </summary>
    private IEnumerator ChangeSpriteRoutineA()
    {
        // 대상 배열이 유효한지 확인합니다.
        if (spriteArrayA == null || spriteArrayA.Length == 0)
        {
            Debug.LogWarning($"ImageManager: {mainImageA.name}에 할당된 스프라이트 배열 A가 비어있습니다. 해당 이미지의 교체는 중단됩니다.");
            yield break;
        }

        // 무한 루프를 통해 스프라이트를 계속 교체합니다.
        while (true)
        {
            // 교체 주기만큼 기다립니다. 이 값은 인스펙터에서 설정 가능합니다.
            yield return new WaitForSeconds(changeInterval);

            // 배열의 끝에 도달했으면 처음(0)으로 리셋합니다.
            if (_currentIndexA >= spriteArrayA.Length)
            {
                _currentIndexA = 0;
            }

            // 현재 인덱스의 스프라이트로 이미지 UI를 업데이트합니다.
            mainImageA.sprite = spriteArrayA[_currentIndexA];

            // 다음 스프라이트를 가리키도록 인덱스를 증가시킵니다.
            _currentIndexA++;
        }
    }

    /// <summary>
    /// mainImageB의 스프라이트를 일정 시간 간격으로 순차적으로 교체하는 코루틴입니다.
    /// </summary>
    private IEnumerator ChangeSpriteRoutineB()
    {
        // 대상 배열이 유효한지 확인합니다.
        if (spriteArrayB == null || spriteArrayB.Length == 0)
        {
            Debug.LogWarning($"ImageManager: {mainImageB.name}에 할당된 스프라이트 배열 B가 비어있습니다. 해당 이미지의 교체는 중단됩니다.");
            yield break;
        }

        // 무한 루프를 통해 스프라이트를 계속 교체합니다.
        while (true)
        {
            // 교체 주기만큼 기다립니다.
            yield return new WaitForSeconds(changeInterval);

            // 배열의 끝에 도달했으면 처음(0)으로 리셋합니다.
            if (_currentIndexB >= spriteArrayB.Length)
            {
                _currentIndexB = 0;
            }

            // 현재 인덱스의 스프라이트로 이미지 UI를 업데이트합니다.
            mainImageB.sprite = spriteArrayB[_currentIndexB];

            // 다음 스프라이트를 가리키도록 인덱스를 증가시킵니다.
            _currentIndexB++;
        }
    }
}