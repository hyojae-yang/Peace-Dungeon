using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Ÿ��Ʋ ���� UI ��ư �̺�Ʈ�� �����ϴ� �Ŵ��� ��ũ��Ʈ�Դϴ�.
/// �ν����� �Ҵ� �Ǵ� �ڵ� �˻��� ���� ��ư�� �����ʸ� ����մϴ�.
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    // === ���� ===

    [Tooltip("�����ϱ� ��ư�� �ν����Ϳ��� �Ҵ��ϼ���. �Ҵ����� ������ 'NewGameButton' �̸����� �ڵ� �˻��մϴ�.")]
    public Button newGameButton;

    [Tooltip("�̾��ϱ� ��ư�� �ν����Ϳ��� �Ҵ��ϼ���. �Ҵ����� ������ 'ContinueButton' �̸����� �ڵ� �˻��մϴ�.")]
    public Button continueButton;

    // === �ʱ�ȭ ===

    private void Awake()
    {
        // '�����ϱ�' ��ư�� �ν����Ϳ� �Ҵ���� �ʾҴٸ� �̸����� ã��
        if (newGameButton == null)
        {
            GameObject newGameObject = GameObject.Find("NewGameButton");
            if (newGameObject != null)
            {
                newGameButton = newGameObject.GetComponent<Button>();
            }
        }

        // '�̾��ϱ�' ��ư�� �ν����Ϳ� �Ҵ���� �ʾҴٸ� �̸����� ã��
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
        // 1. '�����ϱ�' ��ư ������ ��� �� ���� Ȯ��
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGameButtonClick);
        }
        else
        {
            Debug.LogError("����: '�����ϱ�' ��ư�� �Ҵ���� �ʾҽ��ϴ�. �ν����� �Ǵ� �̸�(NewGameButton)�� Ȯ���ϼ���.");
        }

        // 2. '�̾��ϱ�' ��ư ������ ��� �� UI ����
        if (continueButton != null)
        {
            // �̾��ϱ� ��ư ������ ���
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClick);

            // ���� ���� ���� ���θ� Ȯ���ϰ� ��ư UI ����
            if (SaveManager.Instance.DoesSaveFileExist())
            {
                // ���� ������ ������ ��ư�� Ȱ��ȭ�ϰ� �α� ���
                continueButton.interactable = true;
            }
            else
            {
                // ���� ������ ������ ��ư�� ��Ȱ��ȭ�ϰ� �α� ���
                continueButton.interactable = false;
            }
        }
        else
        {
            Debug.LogError("����: '�̾��ϱ�' ��ư�� �Ҵ���� �ʾҽ��ϴ�. �ν����� �Ǵ� �̸�(ContinueButton)�� Ȯ���ϼ���.");
        }
    }
    // === �޼��� ===

    /// <summary>
    /// '�����ϱ�' ��ư Ŭ�� �� ȣ��Ǵ� �޼����Դϴ�.
    /// ���� �����͸� �ʱ�ȭ�ϰ� ���� ������ ��ȯ�մϴ�.
    /// </summary>
    public void OnNewGameButtonClick()
    {
        // TODO: ���� �����͸� ������ �ʱ�ȭ�ϴ� ������ ���⿡ �߰�
        SaveManager.Instance.ResetGameData();
        SceneManager.LoadScene("MainScene");
    }

    /// <summary>
    /// '�̾��ϱ�' ��ư Ŭ�� �� ȣ��Ǵ� �޼����Դϴ�.
    /// ����� ���� �����͸� �ҷ��� ���� ������ ��ȯ�մϴ�.
    /// </summary>
    public void OnContinueButtonClick()
    {

        // SaveManager�� LoadGame �޼��带 ȣ���Ͽ� �����͸� �ε��մϴ�.
        SaveManager.Instance.LoadGame();

        // �ε� �۾� �Ϸ� �� ���� ������ �̵�
        SceneManager.LoadScene("MainScene");
    }
}