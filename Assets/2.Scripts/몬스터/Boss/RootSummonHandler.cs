using UnityEngine;
using System.Collections;

/// <summary>
/// ForestBoss에 의해 소환된 개별 뿌리 오브젝트의 생명 주기와 동작을 관리하는 핸들러입니다.
/// (단일 책임 원칙 준수)
/// </summary>
[RequireComponent(typeof(Collider))] // 콜라이더 필수 요구
public class RootSummonHandler : MonoBehaviour
{
    // === 종속성 (인스펙터 할당 필수) ===

    /// <summary>
    /// 경고 단계에서 사용할 시각적 요소 (파티클 또는 데칼 등)입니다.
    /// 이 오브젝트는 이 스크립트가 붙은 오브젝트의 자식이어야 합니다.
    /// </summary>
    [Tooltip("경고 단계에서 활성화/재생할 파티클 또는 이펙트 오브젝트")]
    public GameObject warningVisuals;

    // [추가] BoxCollider 컴포넌트 참조
    private BoxCollider _hitboxCollider;
    private AudioSource _audioSource; // ⭐️ [추가] AudioSource 컴포넌트 참조

    // =========================================================================
    // 인스펙터 설정 변수 (프리팹 자체 책임)
    // =========================================================================
    [Header("뿌리 동작 설정 (프리팹에서 직접 설정)")]
    [Tooltip("경고 파티클이 표시되는 시간(선딜레이)입니다. 이 시간 뒤에 뿌리가 솟아오릅니다.")]
    public float warningDuration = 2.0f;

    [Tooltip("뿌리가 솟아오르는 최종 높이입니다. (소환 위치로부터의 월드 오프셋)")]
    public float rootMaxHeight = 80.0f;

    [Tooltip("뿌리가 솟아오르는 속도입니다. (단위: 월드 거리/초)")]
    public float rootRiseSpeed = 20.0f;

    [Tooltip("뿌리가 솟아오른 후 유지되는 시간입니다. (자동 파괴 기준)")]
    public float rootLifetimeAfterRise = 3.0f;

    [Tooltip("뿌리의 시작 위치를 소환 지점(땅)보다 얼마나 더 깊이 내릴지 (음수 값 권장)입니다.")]
    public float minStartHeightOffset = -9.9f;


    // === 사운드 설정 추가 ===
    [Header("사운드 설정")]
    [Tooltip("뿌리가 땅 속에서 솟아오르는 동안 반복 재생될 소리입니다. ('지잉' 또는 '쉭' 소리)")]
    public AudioClip riseLoopClip; // ⭐ 새로 추가된 클립 변수

    // === 주입받는 변수 (ForestBoss에서 전달) ===
    private float _magicDamage;          // 보스의 마법 공격력 (데미지 주입)

    // === 내부 상태 변수 ===
    private Vector3 _startWorldPosition;    // 뿌리 오브젝트의 시작 월드 위치 (땅 속)
    private Vector3 _targetWorldPosition;   // 뿌리 오브젝트의 목표 월드 위치 (최대 높이)

    /// <summary>
    /// 솟아오르는 동안 이미 피해를 입혔는지 확인하는 플래그입니다. (중복 피해 방지)
    /// </summary>
    private bool _hasDealtDamage = false;

    // [수정] 피해를 입힐 대상의 태그 상수를 유지합니다.
    private const string TargetTag = "Player"; // <--- 플레이어 오브젝트의 태그로 가정
    // === 초기화 메서드 (Dependency Injection) ===

    /// <summary>
    /// ForestBoss에서 필요한 최소한의 설정 값(공격력)을 주입받고 공격 코루틴을 시작합니다.
    /// 이 메서드는 인스턴스 생성 직후 ForestBoss에 의해 호출됩니다. 
    /// </summary>
    /// <param name="magicDamage">이 뿌리가 입힐 피해량입니다. (마법 공격력)</param>
    public void InitializeAndStartAttack(float magicDamage)
    {
        // 1. 값 주입 (공격력만 받음)
        _magicDamage = magicDamage;

        // 2. 공격 시작 코루틴 호출
        StartCoroutine(RootAttackLifecycleRoutine());
    }

    private void Awake()
    {
        // BoxCollider 참조 및 초기 설정
        _hitboxCollider = GetComponent<BoxCollider>();
        if (_hitboxCollider != null)
        {
            _hitboxCollider.isTrigger = true; // 트리거로 설정 (피해 판정 목적)
            _hitboxCollider.enabled = false; // 시작 시 비활성화 (솟아오를 때만 활성화)
        }
        else
        {
            Debug.LogError("RootSummonHandler: BoxCollider 컴포넌트가 필요합니다!");
        }

        // AudioSource 컴포넌트 참조
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogWarning("RootSummonHandler: AudioSource 컴포넌트가 없어 소리 재생이 불가능합니다.", this);
        }


        // 1. Rigidbody 설정을 안전하게 적용
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 2. 위치 계산: 현재 월드 위치(Instantiate된 위치)를 기준으로 시작 위치와 목표 위치를 계산합니다.
        Vector3 initialWorldPosition = transform.position;

        // [로직 1] 시작 위치 설정: 오프셋만큼 Y축을 내립니다.
        _startWorldPosition = initialWorldPosition + new Vector3(0, minStartHeightOffset, 0);

        // [로직 2] 목표 위치 설정: 시작 위치에서 rootMaxHeight만큼 위로 올라갑니다.
        _targetWorldPosition = _startWorldPosition + new Vector3(0, rootMaxHeight, 0);

        // [로직 3] 실제 뿌리 오브젝트를 시작 위치(땅 속)로 이동시킵니다.
        transform.position = _startWorldPosition;

        // 3. 경고 시각화 초기 상태 설정 (필요 시 경고)
        if (warningVisuals == null)
        {
            Debug.LogWarning("RootSummonHandler: Warning Visuals 오브젝트가 할당되지 않았습니다. 경고 단계가 시각적으로 표시되지 않습니다.");
        }
    }

    /// <summary>
    /// 뿌리 공격의 전체 생명 주기를 관리하는 코루틴입니다.
    /// (경고 -> 솟아오르기 및 피해 판정 -> 유지 -> 소멸)
    /// </summary>
    private IEnumerator RootAttackLifecycleRoutine()
    {
        // =========================================================================
        // 1. 경고 단계 (Warning)
        // =========================================================================
        if (warningVisuals != null)
        {
            warningVisuals.SetActive(true);
        }

        yield return new WaitForSeconds(warningDuration);

        // 경고 시각화 비활성화
        if (warningVisuals != null)
        {
            warningVisuals.SetActive(false);
        }

        // -------------------------------------------------------------------------
        // 2. 솟아오르기 단계 (Rise and Strike) - [수정] 코루틴 호출
        // -------------------------------------------------------------------------
        yield return StartCoroutine(RiseAndStrikeRoutine());

        // -------------------------------------------------------------------------
        // 3. 유지 단계 (Sustain)
        // -------------------------------------------------------------------------

        // 유지 시간 대기
        yield return new WaitForSeconds(rootLifetimeAfterRise);

        // 4. 소멸 (Destroy)
        Destroy(gameObject);
    }

    /// <summary>
    /// 뿌리가 땅속에서 목표 위치까지 솟아오르는 동작을 처리합니다.
    /// 솟아오르는 동안 피해 판정(BoxCollider)을 활성화합니다.
    /// </summary>
    private IEnumerator RiseAndStrikeRoutine()
    {
        // 1. 피해 판정 활성화 (솟아오르는 순간부터 데미지 활성화)
        if (_hitboxCollider != null)
        {
            _hitboxCollider.enabled = true;
        }

        // 솟아오르기 시작 시 효과음 재생
        if (_audioSource != null && riseLoopClip != null)
        {
            _audioSource.clip = riseLoopClip;
            _audioSource.loop = true; // 솟아오르는 동안 반복 재생 (지잉 효과에 적합)
            _audioSource.Play();
        }

        float totalDistance = rootMaxHeight; // 이동할 총 거리
        // [SOLID: SRP] 속도(World Distance/Sec)를 사용하여 시간에 종속되지 않게 합니다.
        float duration = totalDistance / rootRiseSpeed; // 이동에 걸리는 시간 = 거리 / 속도
        float timeElapsed = 0f;
        // 2. 솟아오르기 동작 (Lerp 대신 Vector3.MoveTowards와 유사한 방식)
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration; // 0에서 1까지의 비율

            // Vector3.Lerp를 사용하여 시작 위치에서 목표 위치로 부드럽게 이동합니다.
            transform.position = Vector3.Lerp(_startWorldPosition, _targetWorldPosition, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 정확한 목표 위치에 도달하도록 보정
        transform.position = _targetWorldPosition;

        // 4. [수정] 솟아오르는 동작 완료 후, 피해 판정을 비활성화합니다.
        if (_hitboxCollider != null)
        {
            _hitboxCollider.enabled = false;
        }

        // 솟아오르기 완료 시 효과음 중단
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.loop = false;
        }
    }

    /// <summary>
    /// 충돌체(BoxCollider)가 다른 충돌체와 겹치기 시작할 때 호출됩니다.
    /// (isTrigger = true일 때만 호출)
    /// </summary>
    /// <param name="other">겹친 상대방의 Collider 컴포넌트</param>
    private void OnTriggerEnter(Collider other)
    {
        // [로직 1] 피해 판정 활성화 여부 및 중복 피해 여부 검사 (Guard Clause)
        // 솟아오르는 중이 아니라면 (Collider가 비활성화된 상태), 또는 이미 피해를 입혔다면 무시합니다.
        if (!_hitboxCollider.enabled || _hasDealtDamage)
        {
            return;
        }

        // [로직 2] 대상이 플레이어인지 확인
        if (other.CompareTag(TargetTag))
        {
            // [로직 3] 플레이어의 피해 처리 인터페이스 (IDamageable)를 찾습니다.
            IDamageable damageableTarget = other.GetComponent<IDamageable>();

            if (damageableTarget != null)
            {
                // 데미지 입히는 핵심 호출 (공격 타입: Magic)
                // [수정] IDamageable 인터페이스와 DamageType.True를 사용합니다.
                damageableTarget.TakeDamage(_magicDamage, DamageType.True);

                // 한 번 피해를 입혔으므로 중복 피해를 방지합니다.
                _hasDealtDamage = true;
            }
        }
    }
}