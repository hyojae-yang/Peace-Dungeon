using UnityEngine;

/// <summary>
/// 저장/로드 기능 테스트를 위한 스크립트입니다.
/// UI 버튼 클릭 이벤트에 연결하여 사용합니다.
/// </summary>
public class SaveTestManager : MonoBehaviour
{
    /// <summary>
    /// 게임 저장 버튼 클릭 시 호출됩니다.
    /// SaveManager의 SaveGame() 메서드를 호출하여 게임 상태를 저장합니다.
    /// </summary>
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogError("SaveManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 게임 로드 버튼 클릭 시 호출됩니다.
    /// SaveManager의 LoadGame() 메서드를 호출하여 게임 상태를 불러옵니다.
    /// </summary>
    public void LoadGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogError("SaveManager 인스턴스를 찾을 수 없습니다.");
        }
    }
}