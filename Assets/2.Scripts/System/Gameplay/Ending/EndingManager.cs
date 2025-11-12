using UnityEngine;

/// <summary>
/// EndingManager 클래스는 엔딩 크레딧 패널을 관리하는 싱글톤 인스턴스입니다.
/// 게임의 종료 시퀀스 및 패널 활성화를 담당하는 단일 책임(SRP)을 가집니다.
/// </summary>
public class EndingManager : MonoBehaviour
{
    // [1] 싱글톤 인스턴스 변수 정의
    private static EndingManager _instance;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다. (읽기 전용)
    /// </summary>
    public static EndingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 수정된 부분: FindFirstObjectByType 사용
                _instance = FindFirstObjectByType<EndingManager>();

                if (_instance == null)
                {
                    Debug.LogError("EndingManager 인스턴스가 씬에 없습니다. 게임 오브젝트에 컴포넌트를 추가해야 합니다.");
                }
            }
            return _instance;
        }
    }

    // [2] 엔딩 크레딧 패널을 연결할 변수 (이제 UIManager가 관리하므로 이 변수는 더 이상 필요 없습니다.)
    // [수정] private GameObject endingPanel; 변수는 삭제합니다.

    /// <summary>
    /// 싱글톤 패턴의 무결성을 보장하고, 씬 로드 시 파괴되지 않도록 설정합니다.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    /// <summary>
    /// [3] 엔딩 크레딧 시퀀스를 시작하는 메서드입니다.
    /// 패널 활성화 책임은 EndingUIManager에 위임합니다.
    /// </summary>
    public void ActivateEndingPanel()
    {
        // 1. 엔딩 UI 매니저에게 화면 표시를 명령합니다. (UI 로직 위임)
        if (EndingUIManager.Instance != null)
        {
            EndingUIManager.Instance.ShowEndingScreen();
        }
        else
        {
            Debug.LogError("[EndingManager] EndingUIManager 인스턴스를 찾을 수 없어 엔딩을 표시할 수 없습니다.");
        }

        // 2. [향후 추가될 기능 위치]: 시간 정지, 음악 전환, 카메라 전환 등의 시퀀스 제어 로직
    }
}