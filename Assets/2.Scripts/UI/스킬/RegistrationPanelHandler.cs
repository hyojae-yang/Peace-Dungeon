using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �� ��ũ��Ʈ�� ��ų ������ ������ ���/���� �г��� �����մϴ�.
// '���' ��ư Ŭ�� �� ��ų ���� ���� UI�� Ȱ��ȭ�ϰ�,
// '����' ��ư Ŭ�� �� ��ų ������ ��û�ϸ�,
// '���' ��ư Ŭ�� �� ��� ���� UI�� �ݽ��ϴ�.
public class RegistrationPanelHandler : MonoBehaviour
{
    [Header("UI ��ư")]
    [Tooltip("��ų ��� ��ư�� �Ҵ��ϼ���.")]
    public Button registerButton;
    [Tooltip("��ų ���� ��ư�� �Ҵ��ϼ���.")]
    public Button unregisterButton;
    [Tooltip("�г��� �ݴ� ��� ��ư�� �Ҵ��ϼ���.")]
    public Button cancelButton; // ��� ��ư ���� �߰�

    [Header("���� ��ũ��Ʈ")]
    [Tooltip("�θ� SkillIcon ��ũ��Ʈ�� �Ҵ��ϼ���.")]
    private SkillIcon parentSkillIcon; // �θ� ��ų ������ ��ũ��Ʈ ����
    // PlayerSkillController�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // public PlayerSkillController playerSkillController;

    void Awake()
    {
        // �θ� ������Ʈ���� SkillIcon ������Ʈ�� ã���ϴ�.
        parentSkillIcon = GetComponentInParent<SkillIcon>();
        if (parentSkillIcon == null)
        {
            Debug.LogError("RegistrationPanelHandler�� SkillIcon�� �ڽ����� ��ġ�Ǿ�� �մϴ�.");
            return;
        }

        // PlayerSkillController �̱��� �ν��Ͻ��� �����ϴ��� Ȯ���մϴ�.
        if (PlayerSkillController.Instance == null)
        {
            Debug.LogError("PlayerSkillController �ν��Ͻ��� �������� �ʽ��ϴ�. ���� PlayerSkillController�� ���� ���� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // '���' ��ư Ŭ�� �̺�Ʈ ����
        registerButton.onClick.AddListener(OnRegisterButtonClick);

        // '����' ��ư Ŭ�� �̺�Ʈ ����
        unregisterButton.onClick.AddListener(OnUnregisterButtonClick);

        // '���' ��ư Ŭ�� �̺�Ʈ ����
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }
        else
        {
            Debug.LogWarning("Cancel Button�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ��� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// ��ų ���� ���� �г� Ȱ��ȭ�� �θ� SkillIcon�� ��û�մϴ�.
    /// </summary>
    private void OnRegisterButtonClick()
    {
        // ���/���� �г��� �״�� �� ä, SlotSelectionPanel Ȱ��ȭ�� SkillIcon�� ��û�մϴ�.
        parentSkillIcon.ShowSlotSelectionPanel();

        // ����: parentSkillIcon.HideRegistrationPanel(); <-- �� �� ����
    }

    /// <summary>
    /// ��ų ���� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// PlayerSkillController���� ��ų ������ ��û�ϰ�, ��� ���� UI�� �ݽ��ϴ�.
    /// </summary>
    private void OnUnregisterButtonClick()
    {
        if (parentSkillIcon.skillData != null)
        {
            // PlayerSkillController.Instance�� ���� UnregisterSkill �޼��带 ȣ���Ͽ� ��ų �����ͷ� ���� ��û
            PlayerSkillController.Instance.UnregisterSkill(parentSkillIcon.skillData);
        }
        else
        {
            Debug.LogWarning("�� �����ܿ� �Ҵ�� ��ų �����Ͱ� ���� ������ �� �����ϴ�.");
        }

        // ��ų ���� �� ��� ���� UI�� �ݰ� isPanelActive �÷��׸� �ʱ�ȭ�մϴ�.
        parentSkillIcon.HideAllRelatedPanels();
    }

    /// <summary>
    /// ��� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// ��� ���� UI�� �ݰ� isPanelActive �÷��׸� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void OnCancelButtonClick()
    {
        parentSkillIcon.HideAllRelatedPanels();
    }
}