using UnityEngine;

// UI �гε��� �����ϰ�, Ű �Է� �� ��ư Ŭ���� ���� �г��� �����ϴ� ��ũ��Ʈ�Դϴ�.
public class PanelManager : MonoBehaviour
{
    // === �г� �Ҵ� ���� ===
    [Tooltip("������ �гε��� ������� �Ҵ��ϼ���.")]
    public GameObject[] panels;


    void Update()
    {
        // 'P' Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.P))
        {
            // P Ű�� ������ �� Ư�� �г��� Ȱ��ȭ ���¸� ���(����)�մϴ�.
            // .activeSelf�� ���� ���� ������Ʈ�� Ȱ��ȭ ���¸� ��ȯ�մϴ�.
            TogglePanel();
        }
    }

    /// <summary>
    /// ������ �ε����� �г��� Ȱ��ȭ�ϰų� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void TogglePanel()
    {
            bool isActive = panels[0].activeSelf;
            panels[0].SetActive(!isActive);
    }

    /// <summary>
    /// ������ �ε����� �гθ� Ȱ��ȭ�ϰ� ������ �г��� ��Ȱ��ȭ�մϴ�.
    /// �� �޼���� UI ��ư�� OnClick() �̺�Ʈ�� �����Ͽ� ����մϴ�.
    /// </summary>
    /// <param name="panelIndex">Ȱ��ȭ�� �г��� �迭 �ε���</param>
    public void ActivatePanel(int panelIndex)
    {
        // �迭 ���� ��ȿ�� �˻�
        if (panelIndex < 0 || panelIndex >= panels.Length)
        {
            Debug.LogError("�߸��� �г� �ε����Դϴ�: " + panelIndex);
            return;
        }

        // ��� �г��� ��ȸ�ϸ� Ȱ��ȭ ���¸� �����մϴ�.
        for (int i = 1; i < panels.Length; i++)
        {
            // ���õ� �ε����� �гθ� Ȱ��ȭ�մϴ�.
            if (i == panelIndex)
            {
                panels[i].SetActive(true);
            }
            else
            {
                panels[i].SetActive(false);
            }
        }
    }
}