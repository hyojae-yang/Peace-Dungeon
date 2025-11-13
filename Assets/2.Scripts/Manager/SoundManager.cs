using System.Collections; // 코루틴 사용을 위해 추가
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM 타입(이름)을 정의하는 열거형입니다.
/// 코드의 가독성을 높이고 오타를 방지하며, 클립을 찾는 키 역할을 합니다. (SRP 지원)
/// </summary>
public enum BGMType
{
    None,       // 0: 기본값 또는 정지 상태
    Title,      // 1: 타이틀 씬 BGM
    Loading,    // 2: 로딩 씬 BGM
    Main_A,     // 3: 메인 씬 BGM
    Main_B,     // 4: 던전 씬 BGM
    Main_C,     // 5: 보스룸 씬 BGM
    Main_D,      // 6: 사망 씬 BGM
    Clear,       // 7: 클리어 씬 BGM
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
/// SFX 타입(이름)을 정의하는 열거형입니다. (SRP 지원)
/// </summary>
public enum SFXType
{
    None,
    Button_Click,       // 버튼 클릭음1
    Dungeon_Enter,      // 던전 입장음2
    Dungeon_Exit,       // 던전 퇴장음3
    //Town_Enter,       // 마을 입장음
    Shop_Enter,         // 상점 입장음5
    Restaurant_Enter,   // 식당 입장음6
    Skill_Fireball_Cast,  // 파이어볼 시전 소리 (준비/발동 시작)7
    Skill_Fireball_Impact,
    Skill_Whirlwind_Cast,
    Skill_LifestealBolt_Cast,
    Skill_MagicMissile_Cast,
    General,         // 일반 시스템 메시지 (예: 저장 완료, 장비 교체)12
    Success,         // 긍정적 메시지 (예: 레벨업, 보스 처치, 퀘스트 완료)13
    Warning,    // 경고 메시지 (예: 체력 부족, 실패 알림)14
    Item_Pickup, //아이템 획득 소리15
    Item_Goodpickup, //좋은 아이템 획득16
    Item_Equip, //아이템 장착 소리17
    Item_Heal,          // 음식 먹는 소리18
    Item_Buff,          // 음식 마시는 소리19
    Item_Heal2,          // 요리 먹는 소리20
    Item_Buff2,          // 요리 마시는 소리21
    Item_Scroll,        // 스크롤 사용 (마법진 소리)22
    Levelup_sound,     //레벨업 소리23
    Inventory_openclose_sound, //인벤토리 여닫기 소리24
    QuestAccept, //퀘스트 수락 소리25
    QuestAbandon, //퀘스트 포기 소리26
    QuestComplete, //퀘스트 완료 소리27
    Map_Grab, // 맵 타일을 집어들 때28
    Map_Place, // 맵 타일을 배치할 때 (클릭 해제 시)29
    Map_Rotate, // 맵 타일 회전할 때30
    text_sound, //텍스트 넘기는 소리31
    Dog_Bark, //강아지 짖는 소리32
    Map_Enter, // 맵 타일 배치 모드 진입33
    Map_Exit, // 맵 타일 배치 모드 종료34
}

[System.Serializable]
/// <summary>
/// 인스펙터에서 AudioClip을 SFXType과 묶어 편리하게 할당하기 위한 구조체입니다. (SRP 지원)
/// </summary>
public struct SFXAudio
{
    public SFXType Type;
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

    // --- BGM 피치 제어 필드 추가 (새로운 기능) ---

    [Header("BGM Pitch Control")]
    [Tooltip("BGM 피치가 목표 피치로 변하는 속도입니다. (값이 높을수록 즉각적)")]
    [SerializeField]
    private float _pitchChangeSpeed = 3.0f; // 피치 부드러움 조절

    /// <summary>
    /// DungeonRiskManager에서 설정하는 BGM의 최종 목표 피치입니다. (기본값 1.0f)
    /// </summary>
    private float _targetBGMPitch = 1.0f;

    /// <summary>
        /// 피치 변조 코루틴의 중복 실행을 막기 위한 플래그입니다.
        /// </summary>
    private Coroutine _pitchUpdateCoroutine;

    // --- AudioSource 관리 ---

    [Header("BGM Settings")]
    [SerializeField]
    private AudioSource _bgmAudioSource; // 배경 음악 전용 AudioSource

    [Tooltip("BGM의 최종 목표 볼륨입니다. (설정값 저장 용도)")]
    [SerializeField]
    // BGM 최대 볼륨 (기본값 설정)
    private float _maxBGMVolume = 0.5f;

    // BGM 페이드 코루틴 중복 실행 방지
    private Coroutine _bgmFadeCoroutine;

    [Header("SFX Settings")]
    [SerializeField]
    /// 씬에 배치된 SFX용 AudioSource들을 직접 등록받아 관리하는 리스트입니다. (SFX Pool)
    private List<AudioSource> _sfxAudioSources = new List<AudioSource>();

    // SFX의 최종 목표 볼륨입니다. (설정값 저장 용도)
    [Tooltip("SFX의 최종 목표 볼륨입니다. (설정값 저장 용도)")]
    [SerializeField]
    private float _maxSFXVolume = 1.0f; // SFX 최대 볼륨 (기본값 설정)

    // --- BGM 클립 데이터 관리 ---

    [Header("BGM Clips")]
    [Tooltip("인스펙터에서 할당할 BGM 클립 목록입니다. BGMType과 일치하도록 설정해야 합니다.")]
    [SerializeField]
    private BGMAudio[] _bgmClipsArray;

    /// <summary>
    /// BGMType과 AudioClip을 매핑하여 런타임에 빠르게 참조하기 위한 딕셔너리입니다.
    /// </summary>
    private readonly Dictionary<BGMType, AudioClip> _bgmClipsMap = new Dictionary<BGMType, AudioClip>();

    // --- SFX 클립 데이터 관리 (새로 추가) ---

    [Header("SFX Clips")]
    [Tooltip("인스펙터에서 할당할 SFX 클립 목록입니다. SFXType과 일치하도록 설정해야 합니다.")]
    [SerializeField]
    private SFXAudio[] _sfxClipsArray; // 인스펙터 할당용 SFX 클립 배열

    /// <summary>
    /// SFXType과 AudioClip을 매핑하여 런타임에 빠르게 참조하기 위한 딕셔너리입니다.
    /// </summary>
    private readonly Dictionary<SFXType, AudioClip> _sfxClipsMap = new Dictionary<SFXType, AudioClip>();

    // --- 초기화 및 생명 주기 ---

    private void Awake()
    {
        InitializeSingleton();

        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            InitializeBGMCilps();
            InitializeSFXClips(); // **[추가]** SFX 클립 초기화
            InitializeBGMAudioSource();
            // **[핵심 추가]** 피치 업데이트 코루틴 시작
            StartPitchUpdateCoroutine();
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

        _bgmAudioSource.loop = true;        // BGM은 반복
        _bgmAudioSource.playOnAwake = false; // 명시적 호출로만 재생
        // _bgmAudioSource.volume은 페이드 코루틴이 관리하므로 초기 볼륨을 0으로 둡니다.
        _bgmAudioSource.volume = 0f;
        _bgmAudioSource.pitch = 1.0f; // 초기 피치 설정
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

    /// <summary>
    /// 인스펙터에서 할당된 SFX 클립 배열을 딕셔너리에 매핑하여 런타임 접근 속도를 최적화합니다.
    /// </summary>
    private void InitializeSFXClips()
    {
        _sfxClipsMap.Clear();

        foreach (var sfxAudio in _sfxClipsArray)
        {
            if (sfxAudio.Clip != null)
            {
                if (!_sfxClipsMap.ContainsKey(sfxAudio.Type))
                {
                    _sfxClipsMap.Add(sfxAudio.Type, sfxAudio.Clip);
                }
                else
                {
                    Debug.LogWarning($"[SoundManager] SFX 클립 중복 타입 발견: {sfxAudio.Type}. 무시됩니다.");
                }
            }
        }
    }

    // --- BGM 피치 제어 메서드 (핵심 추가) ---

    /// <summary>
    /// DungeonRiskManager에서 호출하여 BGM의 목표 피치 값을 설정합니다.
    /// 이 값은 Co_UpdatePitchSmoothly 코루틴에 의해 부드럽게 적용됩니다. (DIP 준수)
    /// </summary>
    /// <param name="newPitch">적용할 목표 피치 값 (0.1f ~ 3.0f)</param>
    public void SetTargetBGMPitch(float newPitch)
    {
        // 유효 범위 클램프
        _targetBGMPitch = Mathf.Clamp(newPitch, 0.1f, 3.0f);
        // Debug.Log($"[SoundManager] 목표 피치 설정: {_targetBGMPitch:F2}");
    }

    /// <summary>
        /// BGM의 현재 피치와 목표 피치를 부드럽게 보간하여 적용하는 코루틴을 시작합니다.
        /// </summary>
    private void StartPitchUpdateCoroutine()
    {
        // 중복 실행 방지
        if (_pitchUpdateCoroutine == null)
        {
            _pitchUpdateCoroutine = StartCoroutine(Co_UpdatePitchSmoothly());
        }
    }

    /// <summary>
    /// BGM의 피치를 매 프레임 목표 피치로 부드럽게 변경하는 코루틴입니다. (핵심 로직)
    /// </summary>
    private IEnumerator Co_UpdatePitchSmoothly()
    {
        while (true)
        {
            if (_bgmAudioSource != null && _bgmAudioSource.isPlaying)
            {
                // 현재 피치를 목표 피치(_targetBGMPitch)로 부드럽게 보간합니다.
                _bgmAudioSource.pitch = Mathf.Lerp(
          _bgmAudioSource.pitch,
          _targetBGMPitch,
          Time.deltaTime * _pitchChangeSpeed
        );
            }
            else
            {
                // BGM이 재생 중이 아닐 때는 피치를 1.0f (정상)으로 유지합니다.
                _bgmAudioSource.pitch = 1.0f;
                _targetBGMPitch = 1.0f;
            }

            yield return null;
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
            //Debug.Log($"[SoundManager] SFX Source 등록 완료. 현재 SFX Pool 개수: {_sfxAudioSources.Count}");
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
            // Debug.Log($"[SoundManager] SFX Source 해제 완료. 현재 SFX Pool 개수: {_sfxAudioSources.Count}");
        }
    }

    // --- BGM 기능 메서드 (기존과 동일) ---

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
            //Debug.LogWarning($"[SoundManager] 요청된 BGM 타입({type})에 해당하는 클립이 딕셔너리에 없습니다.");
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

        //Debug.Log($"[SoundManager] BGM 재생 및 페이드 인 시작: {type}");
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

        //Debug.Log("[SoundManager] BGM 페이드 아웃 시작");
    }

    /// <summary>
    /// BGM을 즉시 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = null;
            // 다음 재생을 위해 볼륨은 0으로 유지 (FadeBGM에서 다시 올림)
            _bgmAudioSource.volume = 0f;
            // BGM 정지 시 피치도 정상값으로 즉시 리셋합니다.
            _bgmAudioSource.pitch = 1.0f;
            _targetBGMPitch = 1.0f;
        }
        StopExistingFadeCoroutine();
    }

    /// <summary>
    /// BGM 볼륨 설정값(최대 볼륨)을 변경합니다. (설정 UI용)
    /// SettingsManager에서 호출하여 BGM의 최대 볼륨을 설정합니다.
    /// </summary>
    /// <param name="newVolume">새로운 최대 볼륨 (0.0f ~ 1.0f)</param>
    public void SetMaxBGMVolume(float newVolume)
    {
        _maxBGMVolume = Mathf.Clamp01(newVolume);

        // 현재 재생 중이라면, 즉시 현재 볼륨을 새 최대 볼륨으로 업데이트합니다.
        // PlayBGM의 페이드 코루틴이 돌고 있다면, 이 코루틴의 목표 볼륨이 자동으로 업데이트됩니다.
        if (_bgmAudioSource != null && _bgmAudioSource.isPlaying)
        {
            // 현재 볼륨을 새 최대 볼륨으로 직접 적용합니다.
            _bgmAudioSource.volume = _maxBGMVolume;
        }
    }

    /// <summary>
    /// SFX 볼륨 설정값(최대 볼륨)을 변경합니다. (설정 UI용)
    /// SettingsManager에서 호출하여 SFX의 최대 볼륨을 설정합니다.
    /// </summary>
    /// <param name="newVolume">새로운 최대 볼륨 (0.0f ~ 1.0f)</param>
    public void SetMaxSFXVolume(float newVolume)
    {
        _maxSFXVolume = Mathf.Clamp01(newVolume);
    }

    // --- SFX 기능 메서드 (새로 추가) ---

    /// <summary>
    /// 지정된 SFXType의 효과음을 재생합니다. AudioSource Pool을 사용하여 유휴 소스에 할당합니다.
    /// </summary>
    /// <param name="type">재생할 효과음의 타입</param>
    /// <param name="volume">재생 볼륨 (0.0f ~ 1.0f). AudioListener.volume의 영향을 받습니다.</param>
    public void PlaySFX(SFXType type, float volume = 1f)
    {
        if (type == SFXType.None) return;

        // 1. 딕셔너리에서 AudioClip을 찾습니다.
        if (!_sfxClipsMap.TryGetValue(type, out AudioClip clipToPlay) || clipToPlay == null)
        {
            Debug.LogWarning($"[SoundManager] 요청된 SFX 타입({type})에 해당하는 클립이 딕셔너리에 없습니다.");
            return;
        }

        // 2. 유휴 AudioSource를 찾습니다. (Pooling)
        AudioSource availableSource = FindAvailableSFXSource();

        // 3. 소스 할당 및 재생
        if (availableSource != null)
        {
            availableSource.clip = clipToPlay;
            // 최종 볼륨에 _maxSFXVolume을 곱하여 적용합니다.
            availableSource.volume = Mathf.Clamp01(volume) * _maxSFXVolume;
            availableSource.Play();

            // 요청하신 디버그 로그를 출력합니다.
            //Debug.Log($"[SoundManager] SFX 재생: {type}");
        }
        else
        {
            // 요청하신 정책: 모든 소스가 재생 중일 때 경고 후 재생 건너뛰기
            Debug.LogWarning("[SoundManager] SFX AudioSource Pool이 가득 찼습니다. 새 소리(" + type + ")를 재생할 수 없습니다.");
        }
    }
    /// <summary>
    /// 모든 UI 버튼 클릭 이벤트를 처리하기 위한 편의 메서드입니다. (SRP 위반 최소화를 위해 명확하게 주석 처리)
    /// 버튼 OnClick()에 직접 연결할 수 있도록 매개변수 없이 구현되었습니다.
    /// 모든 버튼의 사운드는 SFXType.Button_Click으로 통일됩니다.
    /// </summary>
    public void PlayButtonSFX()
    {
        // 모든 버튼은 Button_Click SFX를 재생합니다.
        const SFXType buttonSfx = SFXType.Button_Click;
        const float defaultVolume = 1f;

        PlaySFX(buttonSfx, defaultVolume);
    }
    // --- 유틸리티 메서드 ---

    /// <summary>
    /// 현재 재생 중인 BGM 페이드 코루틴을 정지합니다.
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
    /// BGM AudioSource의 볼륨을 부드럽게 조절하는 코루틴입니다. (기존과 동일)
    /// </summary>
    /// <param name="targetClip">재생할 새 클립 (null이면 클립 교체 없음)</param>
    /// <param name="targetVolume">도달할 최종 볼륨</param>
    /// <param name="duration">페이드에 걸리는 시간</param>
    /// <param name="stopAfterFade">페이드 아웃 후 정지할지 여부</param>
    private IEnumerator FadeBGM(AudioClip targetClip, float targetVolume, float duration, bool stopAfterFade = false)
    {
        // (기존 BGM Fade 로직 유지)
        if (targetClip != null)
        {
            if (_bgmAudioSource.clip != targetClip)
            {
                _bgmAudioSource.volume = 0f;
                _bgmAudioSource.clip = targetClip;
                _bgmAudioSource.Play();
            }
        }

        float startVolume = _bgmAudioSource.volume;
        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + duration)
        {
            float elapsed = Time.unscaledTime - startTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            _bgmAudioSource.volume = newVolume;

            yield return null;
        }

        _bgmAudioSource.volume = targetVolume;

        if (stopAfterFade && targetVolume <= 0.01f)
        {
            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = null;
        }

        _bgmFadeCoroutine = null;
    }

    /// <summary>
    /// 현재 재생 중이 아닌, 유휴 상태의 SFX AudioSource를 찾아 반환합니다. (Pooling)
    /// </summary>
    /// <returns>사용 가능한 AudioSource, 없으면 null</returns>
    private AudioSource FindAvailableSFXSource()
    {
        // O(N) 순회로 사용 가능한 소스(isPlaying == false)를 찾습니다.
        foreach (var source in _sfxAudioSources)
        {
            if (source != null && !source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }
}