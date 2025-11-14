using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 게임의 설정(볼륨, 화면 등)을 관리하고 UI 요소와 연결하는 매니저 스크립트입니다.
/// Scene에 단일 인스턴스로 존재하며, 설정 값의 로드 및 저장을 담당합니다.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // === 인스펙터 할당 변수 ===
    [Header("오디오 설정")]
    [Tooltip("마스터 볼륨을 조절하는 슬라이더입니다.")]
    public Slider masterVolumeSlider;

    // **[추가]** BGM 볼륨을 조절하는 슬라이더입니다.
    [Tooltip("배경음악 볼륨을 조절하는 슬라이더입니다.")]
    public Slider bgmVolumeSlider;

    // **[추가]** SFX 볼륨을 조절하는 슬라이더입니다.
    [Tooltip("효과음 볼륨을 조절하는 슬라이더입니다.")]
    public Slider sfxVolumeSlider;

    [Header("화면 설정")]
    [Tooltip("전체 화면/창 모드를 전환하는 토글입니다.")]
    public Toggle fullscreenToggle;
    [Tooltip("해상도 목록을 표시하는 드롭다운입니다.")]
    public TMP_Dropdown resolutionDropdown;

    [Header("기타 설정")]
    [Tooltip("설정을 기본값으로 초기화하는 버튼입니다.")]
    public Button resetButton;

    // === 내부 변수 및 기본값 ===
    private List<Resolution> availableResolutions;

    // 설정 저장 키
    private const string MasterVolumeKey = "MasterVolume";
    // **[추가]** BGM 볼륨 저장 키
    private const string BGMVolumeKey = "BGMVolume";
    // **[추가]** SFX 볼륨 저장 키
    private const string SFXVolumeKey = "SFXVolume";
    private const string FullscreenKey = "IsFullscreen";
    private const string ResolutionIndexKey = "ResolutionIndex";

    // 기본 설정 값
    private const float DEFAULT_VOLUME = 0.7f;
    // **[추가]** BGM 기본 볼륨 값
    private const float DEFAULT_BGM_VOLUME = 0.5f;
    // **[추가]** SFX 기본 볼륨 값
    private const float DEFAULT_SFX_VOLUME = 1.0f;
    private const bool DEFAULT_FULLSCREEN = true;

    // **[수정] 게임이 목표하는 초기 해상도 인덱스를 저장할 변수**
    private int defaultResolutionIndex = 0;

    // === 초기화 및 설정 로드 ===

    private void Start()
    {
        InitializeAudioSettings();
        InitializeDisplaySettings();
        InitializeOtherSettings();

        // 4. 드롭다운 목록을 채우고 기본 해상도 인덱스를 설정합니다.
        PopulateResolutionDropdown();

        LoadSettings();
    }

    /// <summary>
    /// 오디오 슬라이더에 리스너를 등록합니다.
    /// </summary>
    private void InitializeAudioSettings()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // **[추가]** BGM 볼륨 슬라이더 리스너 등록
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        // **[추가]** SFX 볼륨 슬라이더 리스너 등록
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    /// <summary>
    /// 화면 토글 및 드롭다운에 리스너를 등록합니다.
    /// </summary>
    private void InitializeDisplaySettings()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
    }

    /// <summary>
    /// 기타 설정 UI(초기화 버튼)에 리스너를 등록합니다. (SRP)
    /// </summary>
    private void InitializeOtherSettings()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToDefaults);
        }
    }

    // === 오디오 관리 메서드 ===

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    // **[새 기능]** BGM 볼륨 설정 및 SoundManager에 반영
    /// <summary>
    /// BGM 슬라이더 값에 따라 BGM 볼륨을 설정하고 PlayerPrefs에 저장합니다.
    /// </summary>
    /// <param name="volume">BGM 최대 볼륨 값 (0.0f ~ 1.0f)</param>
    public void SetBGMVolume(float volume)
    {
        // SoundManager의 BGM 최대 볼륨을 설정합니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMaxBGMVolume(volume);
        }

        // 설정 값을 PlayerPrefs에 저장합니다.
        PlayerPrefs.SetFloat(BGMVolumeKey, volume);
    }

    // **[새 기능]** SFX 볼륨 설정 및 SoundManager에 반영
    /// <summary>
    /// SFX 슬라이더 값에 따라 SFX 볼륨을 설정하고 PlayerPrefs에 저장합니다.
    /// </summary>
    /// <param name="volume">SFX 최대 볼륨 값 (0.0f ~ 1.0f)</param>
    public void SetSFXVolume(float volume)
    {
        // SoundManager의 SFX 최대 볼륨을 설정합니다.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMaxSFXVolume(volume);
        }

        // 설정 값을 PlayerPrefs에 저장합니다.
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    // === 화면 관리 메서드 ===

    public void SetFullscreen(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt(FullscreenKey, isFull ? 1 : 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= availableResolutions.Count)
        {
            Debug.LogError("잘못된 해상도 인덱스입니다.");
            return;
        }

        Resolution resolution = availableResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
    }

    /// <summary>
    /// 시스템이 지원하는 해상도 목록을 불러와 드롭다운에 채웁니다. (SRP)
    /// </summary>
    private void PopulateResolutionDropdown()
    {
        availableResolutions = new List<Resolution>();
        Resolution[] resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentSystemResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];
            string resString = res.width + "x" + res.height;

            if (!options.Contains(resString))
            {
                availableResolutions.Add(res);
                options.Add(resString);

                // **[추가] 기준 해상도(1920x1080)의 인덱스 찾기**
                if (res.width == 1920 && res.height == 1080)
                {
                    defaultResolutionIndex = availableResolutions.Count - 1;
                }
            }

            // 현재 시스템 해상도 인덱스 찾기 (LoadSettings의 기본값으로 사용)
            if (res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height)
            {
                currentSystemResolutionIndex = availableResolutions.Count - 1;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        // UI에 표시되는 드롭다운 초기 값은 현재 시스템 해상도로 설정합니다.
        resolutionDropdown.value = currentSystemResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // === 설정 로드 및 초기화 메서드 ===

    /// <summary>
    /// 저장된 설정 값을 불러와 UI와 실제 시스템에 적용합니다. (SRP)
    /// </summary>
    private void LoadSettings()
    {
        float loadedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DEFAULT_VOLUME);
        // **[추가]** BGM/SFX 볼륨 로드
        float loadedBGMVolume = PlayerPrefs.GetFloat(BGMVolumeKey, DEFAULT_BGM_VOLUME);
        float loadedSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, DEFAULT_SFX_VOLUME);

        bool isFull = PlayerPrefs.GetInt(FullscreenKey, DEFAULT_FULLSCREEN ? 1 : 0) == 1;

        // 1. 마스터 볼륨 적용
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(loadedMasterVolume);
            AudioListener.volume = loadedMasterVolume;
        }

        // **[추가]** BGM 볼륨 적용
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.SetValueWithoutNotify(loadedBGMVolume);
            if (SoundManager.Instance != null)
            {
                // UI 값에 따라 SoundManager의 최대 볼륨을 즉시 설정합니다.
                SoundManager.Instance.SetMaxBGMVolume(loadedBGMVolume);
            }
        }

        // **[추가]** SFX 볼륨 적용
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(loadedSFXVolume);
            if (SoundManager.Instance != null)
            {
                // UI 값에 따라 SoundManager의 최대 볼륨을 즉시 설정합니다.
                SoundManager.Instance.SetMaxSFXVolume(loadedSFXVolume);
            }
        }

        // 2. 전체 화면 적용
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
            fullscreenToggle.isOn = isFull;
            Screen.fullScreen = isFull;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        // 3. 해상도 적용
        if (resolutionDropdown != null)
        {
            // [수정] 기본 해상도 인덱스로 defaultResolutionIndex를 사용합니다.
            // 저장된 값이 없으면 defaultResolutionIndex(1920x1080 인덱스)를 사용합니다.
            int loadedIndex = PlayerPrefs.GetInt(ResolutionIndexKey, defaultResolutionIndex);

            if (loadedIndex < resolutionDropdown.options.Count)
            {
                resolutionDropdown.SetValueWithoutNotify(loadedIndex);
                SetResolution(loadedIndex);
            }
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 모든 설정 값을 기본값으로 초기화하고 UI와 시스템에 적용합니다. (SRP)
    /// </summary>
    public void ResetToDefaults()
    {
        // 1. PlayerPrefs의 모든 설정 키를 삭제합니다.
        PlayerPrefs.DeleteKey(MasterVolumeKey);
        PlayerPrefs.DeleteKey(FullscreenKey);
        PlayerPrefs.DeleteKey(ResolutionIndexKey);
        // **[추가]** BGM/SFX 볼륨 키 삭제
        PlayerPrefs.DeleteKey(BGMVolumeKey);
        PlayerPrefs.DeleteKey(SFXVolumeKey);
        PlayerPrefs.Save();

        // 2. 기본값으로 설정 적용 및 UI 업데이트 (LoadSettings 재사용)
        // LoadSettings는 PlayerPrefs 값이 없으면 DEFAULT 상수를 사용하며, 
        // 해상도 초기화는 defaultResolutionIndex를 따르게 됩니다.
        LoadSettings();

    }
}