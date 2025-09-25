using UnityEngine;

/// <summary>
/// ����/�ε� ��� �׽�Ʈ�� ���� ��ũ��Ʈ�Դϴ�.
/// UI ��ư Ŭ�� �̺�Ʈ�� �����Ͽ� ����մϴ�.
/// </summary>
public class SaveTestManager : MonoBehaviour
{
    /// <summary>
    /// ���� ���� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// SaveManager�� SaveGame() �޼��带 ȣ���Ͽ� ���� ���¸� �����մϴ�.
    /// </summary>
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogError("SaveManager �ν��Ͻ��� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ���� �ε� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// SaveManager�� LoadGame() �޼��带 ȣ���Ͽ� ���� ���¸� �ҷ��ɴϴ�.
    /// </summary>
    public void LoadGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogError("SaveManager �ν��Ͻ��� ã�� �� �����ϴ�.");
        }
    }
}