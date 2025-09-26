using UnityEngine;

public class TestSaveButton : MonoBehaviour
{
    private void Update()
    {
        // X 키를 누르면 게임을 저장합니다.
        if (Input.GetKeyDown(KeyCode.X))
        {
            // SaveManager.Instance.SaveGame() 메서드를 호출하여 저장 요청을 합니다.
            SaveManager.Instance.SaveGame();
            Debug.Log("<color=lime>SaveManager:</color> 게임 저장 요청 완료.");
        }
    }
}