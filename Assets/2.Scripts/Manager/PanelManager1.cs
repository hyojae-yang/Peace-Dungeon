using UnityEngine;

// UI �гε��� �����ϰ�, Ű �Է� �� ��ư Ŭ���� ���� �г��� �����ϴ� ��ũ��Ʈ�Դϴ�.
public class PanelManager1 : MonoBehaviour
{
    // === �г� �Ҵ� ���� ===
    [Tooltip("������ �гε��� ������� �Ҵ��ϼ���.")]
    public GameObject[] panels;

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