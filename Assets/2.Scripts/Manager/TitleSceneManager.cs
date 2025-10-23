using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System; // Func<T> 등을 사용하지 않지만, 코드가 더 명확해지도록 using에 남겨둡니다.

/// <summary>
/// 타이틀 씬의 UI 버튼 이벤트를 관리하는 매니저 스크립트입니다.
/// 인스펙터 할당 또는 자동 검색을 통해 버튼에 리스너를 등록하고,
/// 게임 방법 및 설정 패널을 제어하는 기능을 포함합니다.
/// SOLID 규칙 중 SRP(단일 책임 원칙)를 최대한 준수합니다.
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    // === 기존 변수: 게임 시작 관련 ===

    [Tooltip("새로하기 버튼을 인스펙터에서 할당하세요. 할당하지 않으면 'NewGameButton' 이름으로 자동 검색합니다.")]
    public Button newGameButton;

    [Tooltip("이어하기 버튼을 인스펙터에서 할당하세요. 할당하지 않으면 'ContinueButton' 이름으로 자동 검색합니다.")]
    public Button continueButton;

    [Tooltip("게임 종료 버튼을 인스펙터에서 할당하세요. 할당하지 않으면 'QuitButton' 이름으로 자동 검색합니다.")]
    public Button quitButton;

    [Header("새로하기 경고")]
    [Tooltip("저장 파일 삭제 경고 팝업 패널을 할당하세요.")]
    public GameObject confirmationPanel; // 팝업창 전체
    public Button confirmNewGameButton; // 팝업창 내의 '확인' 버튼
    public Button cancelNewGameButton;  // 팝업창 내의 '취소' 버튼

    // === 추가된 변수: 옵션/튜토리얼 패널 관련 ===

    [Header("UI Panel & Button Settings")]
    [Tooltip("게임 방법 패널을 활성화할 버튼입니다.")]
    public Button howToPlayButton;

    [Tooltip("게임 설정 패널을 활성화할 버튼입니다.")]
    public Button settingsButton;

    [Tooltip("게임 방법 내용을 담고 있는 패널입니다. (비활성화/활성화 토글용)")]
    public GameObject howToPlayPanel;

    [Tooltip("게임 설정 내용을 담고 있는 패널입니다. (비활성화/활성화 토글용)")]
    public GameObject settingsPanel;

    [Tooltip("저장 경로를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    public TextMeshProUGUI saveText;

    // === 초기화 ===

    private void Awake()
    {
        // 1. '새로하기' 버튼이 인스펙터에 할당되지 않았다면 이름으로 찾기
        // *SRP 원칙: Awake에서는 버튼 참조 획득에만 집중합니다.
        if (newGameButton == null)
        {
            GameObject newGameObject = GameObject.Find("NewGameButton");
            if (newGameObject != null)
            {
                newGameButton = newGameObject.GetComponent<Button>();
            }
        }

        // 2. '이어하기' 버튼이 인스펙터에 할당되지 않았다면 이름으로 찾기
        if (continueButton == null)
        {
            GameObject continueObject = GameObject.Find("ContinueButton");
            if (continueObject != null)
            {
                continueButton = continueObject.GetComponent<Button>();
            }
        }

        // *참고: '게임 방법' 및 '게임 설정' 버튼/패널은 명시적 할당을 가정합니다.
        // 자동 검색 로직을 추가하면 복잡도가 올라가므로, 인스펙터 할당을 권장합니다.
    }

    private void Start()
    {
        // BGM 재생 로직
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.Title, 2.0f);
        }

        // 1. 기존 '새로하기' 버튼 리스너 등록 및 오류 확인
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가 (가장 먼저 호출되어야 함)
            newGameButton.onClick.AddListener(PlayButtonSFXSafely);
            newGameButton.onClick.AddListener(OnNewGameButtonClick);
        }
        else
        {
            Debug.LogError("오류: '새로하기' 버튼이 할당되지 않았습니다. 인스펙터 또는 이름(NewGameButton)을 확인하세요.");
        }

        // 2. 기존 '이어하기' 버튼 리스너 등록 및 UI 제어
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            continueButton.onClick.AddListener(PlayButtonSFXSafely);
            continueButton.onClick.AddListener(OnContinueButtonClick);

            // SaveManager를 통해 저장 파일 존재 여부를 확인하고 버튼 UI 제어
            // *SRP 원칙: SaveManager 인스턴스를 가정하고 UI 제어 책임을 가집니다.
            if (SaveManager.Instance != null && SaveManager.Instance.DoesSaveFileExist())
            {
                continueButton.interactable = true;
            }
            else
            {
                continueButton.interactable = false;
            }
        }
        else
        {
            Debug.LogError("오류: '이어하기' 버튼이 할당되지 않았습니다. 인스펙터 또는 이름(ContinueButton)을 확인하세요.");
        }

        // [추가] '게임 종료' 버튼 리스너 등록
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            quitButton.onClick.AddListener(PlayButtonSFXSafely);
            quitButton.onClick.AddListener(OnQuitGameButtonClick);
        }
        else
        {
            // 버튼 이름으로 자동 검색하는 로직을 추가해도 되지만, 인스펙터 할당이 가장 빠릅니다.
            Debug.LogError("오류: '게임 종료' 버튼이 할당되지 않았습니다. 인스펙터 할당을 확인하세요.");
        }


        // [추가] 경고 팝업 버튼 리스너 등록
        if (confirmNewGameButton != null)
        {
            confirmNewGameButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            confirmNewGameButton.onClick.AddListener(PlayButtonSFXSafely);
            // '확인' 버튼은 최종 로직을 호출
            confirmNewGameButton.onClick.AddListener(ProceedNewGameLogic);
        }

        if (cancelNewGameButton != null)
        {
            cancelNewGameButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            cancelNewGameButton.onClick.AddListener(PlayButtonSFXSafely);
            // '취소' 버튼은 팝업을 비활성화
            cancelNewGameButton.onClick.AddListener(() => DeactivatePanel(confirmationPanel));
        }

        // 초기 시작 시 경고 패널 비활성화
        if (confirmationPanel != null) confirmationPanel.SetActive(false);


        // 3. 새로 추가된 '게임 방법' 버튼 리스너 등록
        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            howToPlayButton.onClick.AddListener(PlayButtonSFXSafely);
            howToPlayButton.onClick.AddListener(OnHowToPlayButtonClick);
        }
        else
        {
            Debug.LogWarning("경고: '게임 방법' 버튼이 할당되지 않았습니다. 해당 기능은 작동하지 않습니다.");
        }

        // 4. 새로 추가된 '게임 설정' 버튼 리스너 등록
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            // [수정] SFX 재생 리스너 추가
            settingsButton.onClick.AddListener(PlayButtonSFXSafely);
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }
        else
        {
            Debug.LogWarning("경고: '게임 설정' 버튼이 할당되지 않았습니다. 해당 기능은 작동하지 않습니다.");
        }

        // 초기 시작 시 모든 패널 비활성화 (선택 사항)
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // SaveText 업데이트 (SaveManager가 존재할 경우에만)
        if (saveText != null && SaveManager.Instance != null)
        {
            saveText.text = "저장 위치 \n" + SaveManager.Instance.saveFilePath;
        }
    }

    /// <summary>
    /// SoundManager 인스턴스 존재 여부를 확인하고 SFX를 안전하게 재생하는 헬퍼 메서드입니다.
    /// 버튼 OnClick() 이벤트에 연결되어 단일 책임 원칙을 보조합니다.
    /// </summary>
    private void PlayButtonSFXSafely()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonSFX();
        }
    }

    // === 게임 시작 관련 메서드 (기존 기능 유지) ===

    /// <summary>
    /// '새로하기' 버튼 클릭 시 호출되는 메서드입니다.
    /// 게임 데이터를 초기화하고 로딩 씬을 거쳐 메인 씬으로 전환합니다.
    /// </summary>
    public void OnNewGameButtonClick()
    {
        // 1. 저장 파일 존재 여부 확인 (SaveManager에 의존)
        if (SaveManager.Instance != null && SaveManager.Instance.DoesSaveFileExist())
        {
            // 파일이 존재하면 경고 팝업을 활성화합니다.
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
            }
            else
            {
                // 방어적 코딩: 패널이 없으면 경고 없이 바로 진행
                ProceedNewGameLogic();
            }
        }
        else
        {
            // 저장 파일이 없으면 경고 없이 바로 진행
            ProceedNewGameLogic();
        }
    }

    /// <summary>
    /// 새로하기 경고 후 '확인'을 눌렀거나, 저장 파일이 없을 때 호출되는 최종 게임 시작 로직입니다.
    /// </summary>
    public void ProceedNewGameLogic()
    {
        // 경고창이 열려있었다면 닫습니다. (안전하게)
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (SaveManager.Instance != null) SaveManager.Instance.ResetGameData();
        SceneManager.LoadScene("LoadingScene");
    }

    /// <summary>
    /// '이어하기' 버튼 클릭 시 호출되는 메서드입니다.
    /// 저장된 게임 데이터를 불러와 로딩 씬을 거쳐 메인 씬으로 전환합니다.
    /// </summary>
    public void OnContinueButtonClick()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.LoadGame();

        // [수정] 2. 최종 목적지(MainScene)를 정적 변수에 설정
        // MainSceneManager.NextSceneToLoad = "MainScene"; // 사용자의 기존 코드를 따라 주석 처리 없이 남겨둡니다.

        SceneManager.LoadScene("LoadingScene");
    }

    /// <summary>
    /// '게임 종료' 버튼 클릭 시 호출되는 메서드입니다.
    /// 애플리케이션을 종료합니다. (에디터에서는 플레이 모드를 정지합니다.)
    /// </summary>
    public void OnQuitGameButtonClick()
    {
        // 단일 책임 원칙 (SRP): 오직 게임 종료 기능만 수행합니다.

        // 1. 빌드된 게임(exe 등)에서 애플리케이션 종료
        Application.Quit();

        // 2. 유니티 에디터에서 테스트할 때만 사용 (선택 사항)
#if UNITY_EDITOR
        // UnityEditor.EditorApplication.isPlaying = false; // 해당 부분이 없으므로 그대로 주석처리
        // UnityEditor 네임스페이스가 포함되어 있지 않으므로 컴파일 오류 방지를 위해 if문만 남겨둡니다.
#endif
    }

    // === 추가된 UI 제어 메서드 (패널 활성화) ===

    /// <summary>
    /// '게임 방법' 버튼 클릭 시 호출되는 메서드입니다.
    /// HowToPlayPanel을 활성화하여 튜토리얼 텍스트를 표시합니다.
    /// </summary>
    public void OnHowToPlayButtonClick()
    {
        if (howToPlayPanel != null)
        {
            // 다른 패널은 모두 닫고, 이 패널만 엽니다.
            DeactivateAllPanels();
            howToPlayPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("'게임 방법' 패널이 할당되지 않아 기능을 수행할 수 없습니다.");
        }
    }

    /// <summary>
    /// '게임 설정' 버튼 클릭 시 호출되는 메서드입니다.
    /// SettingsPanel을 활성화하여 볼륨/화면 설정을 표시합니다.
    /// </summary>
    public void OnSettingsButtonClick()
    {
        if (settingsPanel != null)
        {
            // 다른 패널은 모두 닫고, 이 패널만 엽니다.
            DeactivateAllPanels();
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("'게임 설정' 패널이 할당되지 않아 기능을 수행할 수 없습니다.");
        }
    }

    // === 유틸리티 메서드 (패널 비활성화 - SRP) ===

    /// <summary>
    /// 인수로 받은 특정 패널을 비활성화합니다.
    /// 패널 내의 '닫기' 버튼 등에 수동으로 연결하기 위해 제공됩니다. (단일 책임 원칙)
    /// </summary>
    /// <param name="targetPanel">비활성화할 GameObject 패널</param>
    public void DeactivatePanel(GameObject targetPanel)
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 타이틀 씬에 존재하는 모든 팝업 패널을 비활성화합니다.
    /// </summary>
    private void DeactivateAllPanels()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // *여기에 추후 추가될 다른 패널(예: 크레딧)도 추가하여 관리할 수 있습니다.
    }
}