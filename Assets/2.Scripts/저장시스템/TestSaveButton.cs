using UnityEngine;

public class TestSaveButton : MonoBehaviour
{
    private void Update()
    {
        // X Ű�� ������ ������ �����մϴ�.
        if (Input.GetKeyDown(KeyCode.X))
        {
            // SaveManager.Instance.SaveGame() �޼��带 ȣ���Ͽ� ���� ��û�� �մϴ�.
            SaveManager.Instance.SaveGame();
            Debug.Log("<color=lime>SaveManager:</color> ���� ���� ��û �Ϸ�.");
        }
    }
}