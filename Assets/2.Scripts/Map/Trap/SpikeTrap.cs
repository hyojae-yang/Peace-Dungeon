using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요합니다.

/// <summary>
/// 일정 시간(3~10초) 간격으로 스파이크 함정의 애니메이터 트리거를 순차적으로 발동시키는 스크립트입니다.
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    // === 설정 가능한 변수 ===

    [Header("애니메이션 설정")]
    [Tooltip("스파이크를 돌출시키거나 활성화할 때 사용할 애니메이터 트리거 파라미터 이름입니다.")]
    public string activateTriggerName = "open";

    [Tooltip("스파이크를 집어넣거나 비활성화할 때 사용할 애니메이터 트리거 파라미터 이름입니다.")]
    public string deactivateTriggerName = "close";

    [Header("시간 설정")]
    [Tooltip("함정이 작동하기 전 최소 대기 시간(초)입니다.")]
    [Range(1.0f, 10.0f)]
    public float minDelay = 3.0f;

    [Tooltip("함정이 작동하기 전 최대 대기 시간(초)입니다.")]
    [Range(1.0f, 10.0f)]
    public float maxDelay = 10.0f;

    [Tooltip("활성화 트리거 발동 후 비활성화 트리거가 발동될 때까지의 고정 시간(초)입니다. (요청에 따라 1초로 고정)")]
    private const float ACTIVE_DURATION = 1.0f;


    // === 내부 컴포넌트 참조 ===

    /// <summary>
    /// 함정 오브젝트에 부착된 Animator 컴포넌트입니다.
    /// </summary>
    private Animator spikeAnimator;


    // === 초기화 ===

    void Awake()
    {
        // 1. Animator 컴포넌트 참조 확보
        spikeAnimator = GetComponent<Animator>();

        if (spikeAnimator == null)
        {
            Debug.LogError("SpikeTrap 스크립트는 Animator 컴포넌트가 필요합니다!");
            enabled = false; // 컴포넌트가 없으면 스크립트 비활성화
            return;
        }

        // 2. 함정 사이클 시작
        StartCoroutine(TrapCycle());
    }


    // === 핵심 로직: 함정 동작 사이클 ===

    /// <summary>
    /// 함정의 작동 주기를 무한히 반복하는 코루틴입니다.
    /// </summary>
    private IEnumerator TrapCycle()
    {
        while (true) // 무한 반복
        {
            // 1. 무작위 대기 시간 계산
            // minDelay와 maxDelay 사이의 무작위 시간을 계산합니다.
            float randomWaitTime = Random.Range(minDelay, maxDelay);


            // 2. 작동 전 대기
            yield return new WaitForSeconds(randomWaitTime);

            // 3. 스파이크 활성화 (첫 번째 트리거 발동)
            // 스파이크를 돌출시키는 애니메이션을 시작합니다.
            spikeAnimator.SetTrigger(activateTriggerName);

            // 4. 활성화 상태 유지 (1초 대기)
            // 스파이크가 돌출된 상태를 유지하도록 1초 동안 기다립니다.
            yield return new WaitForSeconds(ACTIVE_DURATION);

            // 5. 스파이크 비활성화 (두 번째 트리거 발동)
            // 스파이크를 집어넣는 애니메이션을 시작합니다.
            spikeAnimator.SetTrigger(deactivateTriggerName);
        }
    }
}