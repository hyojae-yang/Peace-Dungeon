using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬의 UI 버튼 이벤트를 관리하는 매니저 스크립트입니다.
/// 인스펙터 할당 또는 자동 검색을 통해 버튼에 리스너를 등록합니다.
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    // === 변수 ===

    [Tooltip("새로하기 버튼을 인스펙터에서 할당하세요. 할당하지 않으면 'NewGameButton' 이름으로 자동 검색합니다.")]
    public Button newGameButton;

    [Tooltip("이어하기 버튼을 인스펙터에서 할당하세요. 할당하지 않으면 'ContinueButton' 이름으로 자동 검색합니다.")]
    public Button continueButton;

    // === 초기화 ===

    private void Awake()
    {
        // '새로하기' 버튼이 인스펙터에 할당되지 않았다면 이름으로 찾기
        if (newGameButton == null)
        {
            GameObject newGameObject = GameObject.Find("NewGameButton");
            if (newGameObject != null)
            {
                newGameButton = newGameObject.GetComponent<Button>();
            }
        }

        // '이어하기' 버튼이 인스펙터에 할당되지 않았다면 이름으로 찾기
        if (continueButton == null)
        {
            GameObject continueObject = GameObject.Find("ContinueButton");
            if (continueObject != null)
            {
                continueButton = continueObject.GetComponent<Button>();
            }
        }
    }
    private void Start()
    {
        // 1. '새로하기' 버튼 리스너 등록 및 오류 확인
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGameButtonClick);
        }
        else
        {
            Debug.LogError("오류: '새로하기' 버튼이 할당되지 않았습니다. 인스펙터 또는 이름(NewGameButton)을 확인하세요.");
        }

        // 2. '이어하기' 버튼 리스너 등록 및 UI 제어
        if (continueButton != null)
        {
            // 이어하기 버튼 리스너 등록
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClick);

            // 저장 파일 존재 여부를 확인하고 버튼 UI 제어
            if (SaveManager.Instance.DoesSaveFileExist())
            {
                // 저장 파일이 있으면 버튼을 활성화하고 로그 출력
                continueButton.interactable = true;
            }
            else
            {
                // 저장 파일이 없으면 버튼을 비활성화하고 로그 출력
                continueButton.interactable = false;
            }
        }
        else
        {
            Debug.LogError("오류: '이어하기' 버튼이 할당되지 않았습니다. 인스펙터 또는 이름(ContinueButton)을 확인하세요.");
        }
    }
    // === 메서드 ===

    /// <summary>
    /// '새로하기' 버튼 클릭 시 호출되는 메서드입니다.
    /// 게임 데이터를 초기화하고 메인 씬으로 전환합니다.
    /// </summary>
    public void OnNewGameButtonClick()
    {
        // TODO: 게임 데이터를 완전히 초기화하는 로직을 여기에 추가
        SaveManager.Instance.ResetGameData();
        SceneManager.LoadScene("MainScene");
    }

    /// <summary>
    /// '이어하기' 버튼 클릭 시 호출되는 메서드입니다.
    /// 저장된 게임 데이터를 불러와 메인 씬으로 전환합니다.
    /// </summary>
    public void OnContinueButtonClick()
    {

        // SaveManager의 LoadGame 메서드를 호출하여 데이터를 로드합니다.
        SaveManager.Instance.LoadGame();

        // 로드 작업 완료 후 메인 씬으로 이동
        SceneManager.LoadScene("MainScene");
    }
}