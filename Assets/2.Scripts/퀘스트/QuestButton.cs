using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// ����Ʈ ����� �� ��ư�� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ��ư�� ����Ʈ ID�� �����ϰ�, Ŭ�� �̺�Ʈ�� QuestUIHandler�� �����մϴ�.
/// SOLID: ���� å�� ��Ģ (���� ����Ʈ ��ư ����).
/// </summary>
public class QuestButton : MonoBehaviour
{
    // --- UI ���� ���� ---
    [Tooltip("����Ʈ �̸��� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [Tooltip("��ư ������Ʈ�Դϴ�.")]
    [SerializeField] private Button button;

    // --- ���� ���� ---
    // �� ��ư�� ��Ÿ���� ����Ʈ�� ���� ID
    private int questID;

    /// <summary>
    /// ��ư�� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// QuestUIHandler���� ��ư�� �������� ������ �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="id">��ư�� ��Ÿ�� ����Ʈ�� ID.</param>
    /// <param name="name">��ư�� ǥ�õ� ����Ʈ�� �̸�.</param>
    /// <param name="onClickAction">��ư Ŭ�� �� ����� �׼�.</param>
    public void Initialize(int id, string name, Action<int> onClickAction)
    {
        this.questID = id;

        // ��ư �ؽ�Ʈ ����
        if (questNameText != null)
        {
            questNameText.text = name;
        }

        // ��ư Ŭ�� �̺�Ʈ�� �׼� �߰�
        if (button != null)
        {
            button.onClick.AddListener(() => onClickAction.Invoke(this.questID));
        }
    }
}