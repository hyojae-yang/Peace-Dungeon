using System.Collections; // 코루틴 사용을 위해 추가
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM 타입(이름)을 정의하는 열거형입니다.
/// 코드의 가독성을 높이고 오타를 방지하며, 클립을 찾는 키 역할을 합니다. (SRP 지원)
/// </summary>
public enum BGMType
{
    None,       // 0: 기본값 또는 정지 상태
    Title,      // 1: 타이틀 씬 BGM
    Loading,    // 2: 로딩 씬 BGM
    Main_A,     // 3: 메인 씬 BGM A
    Main_B,     // 4: 메인 씬 BGM B
    Main_C,     // 5: 메인 씬 BGM C
    Main_D      // 6: 메인 씬 BGM D
}

[System.Serializable]
/// <summary>
/// 인스펙터에서 AudioClip을 BGMType과 묶어 편리하게 할당하기 위한 구조체입니다.
/// </summary>
public struct BGMAudio
{
    public BGMType Type;
    public AudioClip Clip;
}

/// <summary>
/// SoundManager는 게임 내 모든 사운드(BGM, SFX)의 재생 및 관리를 담당하는 싱글톤 클래스입니다.
/// 씬 전환 시에도 파괴되지 않고 유지되며, 역할별 AudioSource를 분리하여 관리합니다.
/// SOLID 규칙 중 SRP(단일 책임 원칙) 및 DIP(의존성 역전 원칙)를 고려하여 설계되었습니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // --- 싱글톤 인스턴스 ---

    /// <summary>
    /// SoundManager의 전역 접근 인스턴스입니다.
    /// </summary>
    public static SoundManager Instance { get; private set; }

    // --- AudioSource 관리 ---

    [Header("BGM Settings")]
    [SerializeField]
    private AudioSource _bgmAudioSource; // 배경 음악 전용 AudioSource

    [Tooltip("BGM의 최종 목표 볼륨입니다. (설정값 저장 용도)")]
    [SerializeField]
    private float _maxBGMVolume = 0.5f; // BGM 최대 볼륨 (기본값 설정)

    // BGM 페이드 코루틴 중복 실행 방지
    private Coroutine _bgmFadeCoroutine;

    [Header("SFX Settings")]
    [SerializeField]
    // 씬에 배치된 SFX용 AudioSource들을 직접 등록받아 관리하는 리스트입니다.
    private List<AudioSource> _sfxAudioSources = new List<AudioSource>();

    // --- BGM 클립 데이터 관리 ---

    [Header("BGM Clips")]
    [Tooltip("인스펙터에서 할당할 BGM 클립 목록입니다. BGMType과 일치하도록 설정해야 합니다.")]
    [SerializeField]
    private BGMAudio[] _bgmClipsArray;

    /// <summary>
    /// BGMType과 AudioClip을 매핑하여 런타임에 빠르게 참조하기 위한 딕셔너리입니다.
    /// </summary>
    private readonly Dictionary<BGMType, AudioClip> _bgmClipsMap = new Dictionary<BGMType, AudioClip>();

    // --- 초기화 및 생명 주기 ---

    private void Awake()
    {
        InitializeSingleton();

        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            InitializeBGMCilps();
            InitializeBGMAudioSource();

            // SFX AudioSource는 씬에 배치된 컴포넌트들이 RegisterSFXSource()를 통해 직접 등록합니다.
            Debug.Log("[SoundManager] 초기화 완료. BGM 클립 맵핑 및 BGM AudioSource 준비 완료.");
        }
    }

    /// <summary>
    /// 싱글톤 패턴을 초기화하고 중복된 인스턴스를 처리합니다.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 파괴
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 배경 음악 AudioSource를 자동으로 할당하고 초기 설정을 진행합니다.
    /// </summary>
    private void InitializeBGMAudioSource()
    {
        _bgmAudioSource = GetComponent<AudioSource>();
        if (_bgmAudioSource == null)
        {
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("[SoundManager] BGM AudioSource가 없어 자동으로 추가되었습니다.");
        }

        _bgmAudioSource.loop = true;          // BGM은 반복
        _bgmAudioSource.playOnAwake = false;  // 명시적 호출로만 재생
        _bgmAudioSource.volume = _maxBGMVolume; // 초기 볼륨 설정
    }

    /// <summary>
    /// 인스펙터에서 할당된 BGM 클립 배열을 딕셔너리에 매핑하여 런타임 접근 속도를 최적화합니다.
    /// </summary>
    private void InitializeBGMCilps()
    {
        _bgmClipsMap.Clear();

        foreach (var bgmAudio in _bgmClipsArray)
        {
            if (bgmAudio.Clip != null)
            {
                if (!_bgmClipsMap.ContainsKey(bgmAudio.Type))
                {
                    _bgmClipsMap.Add(bgmAudio.Type, bgmAudio.Clip);
                }
                else
                {
                    Debug.LogWarning($"[SoundManager] BGM 클립 중복 타입 발견: {bgmAudio.Type}. 무시됩니다.");
                }
            }
        }
    }

    // --- SFX 직접 주입 기능 (기존과 동일) ---

    /// <summary>
    /// 씬에 배치된 SFX AudioSource를 SoundManager에 등록하는 메서드입니다.
    /// </summary>
    public void RegisterSFXSource(AudioSource source)
    {
        if (source != null && !_sfxAudioSources.Contains(source))
        {
            _sfxAudioSources.Add(source);
            source.loop = false;
            source.playOnAwake = false;
        }
    }

    /// <summary>
    /// 씬에서 파괴되는 SFX AudioSource를 SoundManager 리스트에서 해제하는 메서드입니다.
    /// </summary>
    public void UnregisterSFXSource(AudioSource source)
    {
        if (source != null && _sfxAudioSources.Contains(source))
        {
            _sfxAudioSources.Remove(source);
        }
    }

    // --- BGM 기능 메서드 (수정 및 추가) ---

    /// <summary>
    /// 지정된 BGMType의 배경 음악을 페이드 인하며 재생합니다.
    /// </summary>
    /// <param name="type">재생할 배경 음악의 타입</param>
    /// <param name="fadeDuration">페이드 인에 걸리는 시간(초)</param>
    public void PlayBGM(BGMType type, float fadeDuration = 1.5f)
    {
        if (_bgmAudioSource == null)
        {
            Debug.LogError("[SoundManager] BGM AudioSource가 할당되지 않아 재생할 수 없습니다.");
            return;
        }

        // BGM 재생을 멈추고 싶은 경우
        if (type == BGMType.None)
        {
            FadeOutBGM(fadeDuration);
            return;
        }

        // 1. 딕셔너리에서 AudioClip을 찾습니다.
        if (!_bgmClipsMap.TryGetValue(type, out AudioClip clipToPlay) || clipToPlay == null)
        {
            Debug.LogWarning($"[SoundManager] 요청된 BGM 타입({type})에 해당하는 클립이 딕셔너리에 없습니다.");
            return;
        }

        // 2. 현재 재생 중인 클립과 동일한지 확인하여 중복 재생을 방지합니다.
        if (_bgmAudioSource.clip == clipToPlay && _bgmAudioSource.isPlaying)
        {
            // 이미 재생 중이라면 페이드 중인 코루틴만 정지
            StopExistingFadeCoroutine();
            // 볼륨이 이미 목표 볼륨이라면 아무것도 안 함
            if (_bgmAudioSource.volume == _maxBGMVolume) return;
            // 볼륨이 작다면 다시 페이드 인 시도
        }

        // 3. 코루틴을 시작하여 페이드 인을 진행합니다.
        // 기존 코루틴이 있다면 정지하고 새로 시작하여 중복 실행을 방지합니다.
        StopExistingFadeCoroutine();
        _bgmFadeCoroutine = StartCoroutine(FadeBGM(clipToPlay, _maxBGMVolume, fadeDuration));

        Debug.Log($"[SoundManager] BGM 재생 및 페이드 인 시작: {type}");
    }

    /// <summary>
    /// 배경 음악 재생을 페이드 아웃하며 정지합니다.
    /// </summary>
    /// <param name="fadeDuration">페이드 아웃에 걸리는 시간(초)</param>
    public void FadeOutBGM(float fadeDuration = 1.0f)
    {
        if (_bgmAudioSource == null || !_bgmAudioSource.isPlaying) return;

        // 기존 코루틴이 있다면 정지하고 새로 시작
        StopExistingFadeCoroutine();
        _bgmFadeCoroutine = StartCoroutine(FadeBGM(null, 0f, fadeDuration, true));

        Debug.Log("[SoundManager] BGM 페이드 아웃 시작");
    }

    // 이전에 사용하던 StopBGM은 FadeOutBGM으로 대체됩니다.
    // 기존의 StopBGM() 코드는 비페이드 정지가 필요할 경우에만 사용합니다.
    public void StopBGM()
    {
        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = null;
            _bgmAudioSource.volume = _maxBGMVolume; // 다음 재생을 위해 볼륨은 유지
        }
        StopExistingFadeCoroutine();
    }

    // --- 유틸리티 메서드 ---

    /// <summary>
    /// 현재 실행 중인 BGM 페이드 코루틴을 정지합니다.
    /// </summary>
    private void StopExistingFadeCoroutine()
    {
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = null;
        }
    }

    /// <summary>
    /// BGM AudioSource의 볼륨을 부드럽게 조절하는 코루틴입니다.
    /// </summary>
    /// <param name="targetClip">재생할 새 클립 (null이면 클립 교체 없음)</param>
    /// <param name="targetVolume">도달할 최종 볼륨</param>
    /// <param name="duration">페이드에 걸리는 시간</param>
    /// <param name="stopAfterFade">페이드 아웃 후 정지할지 여부</param>
    private IEnumerator FadeBGM(AudioClip targetClip, float targetVolume, float duration, bool stopAfterFade = false)
    {
        // 페이드 인 시 새 클립 설정 및 재생
        if (targetClip != null)
        {
            // 클립이 바뀌는 순간 볼륨을 0으로 설정하여 노이즈 방지
            if (_bgmAudioSource.clip != targetClip)
            {
                _bgmAudioSource.volume = 0f;
                _bgmAudioSource.clip = targetClip;
                _bgmAudioSource.Play();
            }
        }

        float startVolume = _bgmAudioSource.volume;
        float startTime = Time.unscaledTime;

        // 페이드 진행
        while (Time.unscaledTime < startTime + duration)
        {
            float elapsed = Time.unscaledTime - startTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            _bgmAudioSource.volume = newVolume;

            yield return null;
        }

        // 최종 볼륨 설정 및 후처리
        _bgmAudioSource.volume = targetVolume;

        if (stopAfterFade && targetVolume <= 0.01f)
        {
            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = null; // 클립도 초기화
            // 다음 페이드 인을 위해 볼륨은 _maxBGMVolume으로 유지하지 않고 0으로 둠 (StartVolume에 영향)
        }

        _bgmFadeCoroutine = null; // 코루틴이 정상적으로 종료되었음을 표시
    }

    /// <summary>
    /// BGM 볼륨 설정값(최대 볼륨)을 변경합니다. (설정 UI용)
    /// </summary>
    /// <param name="newVolume">새로운 최대 볼륨 (0.0f ~ 1.0f)</param>
    public void SetMaxBGMVolume(float newVolume)
    {
        _maxBGMVolume = Mathf.Clamp01(newVolume);

        // 현재 재생 중이라면, 즉시 현재 볼륨을 새 최대 볼륨으로 업데이트합니다.
        if (_bgmAudioSource != null && _bgmAudioSource.isPlaying)
        {
            _bgmAudioSource.volume = _maxBGMVolume;
        }
    }

    // --- SFX 기능 메서드 (추후 구현 예정) ---

    // public void PlaySFX(AudioClip clip, float volume = 1f) { ... }
}