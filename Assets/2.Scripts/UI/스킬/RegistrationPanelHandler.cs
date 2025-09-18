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

    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    void Awake()
    {
        // �θ� ������Ʈ���� SkillIcon ������Ʈ�� ã���ϴ�.
        parentSkillIcon = GetComponentInParent<SkillIcon>();
        if (parentSkillIcon == null)
        {
            Debug.LogError("RegistrationPanelHandler�� SkillIcon�� �ڽ����� ��ġ�Ǿ�� �մϴ�.");
            return;
        }

        // PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // PlayerCharacter�� ���� PlayerSkillController�� �����մϴ�.
        if (playerCharacter.playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController�� PlayerCharacter�� �Ҵ���� �ʾҽ��ϴ�.");
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
    }

    /// <summary>
    /// ��ų ���� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// PlayerSkillController���� ��ų ������ ��û�ϰ�, ��� ���� UI�� �ݽ��ϴ�.
    /// </summary>
    private void OnUnregisterButtonClick()
    {
        if (parentSkillIcon.skillData != null && playerCharacter.playerSkillController != null)
        {
            // PlayerCharacter�� ���� PlayerSkillController�� UnregisterSkill �޼��带 ȣ���Ͽ� ��ų �����ͷ� ���� ��û
            playerCharacter.playerSkillController.UnregisterSkill(parentSkillIcon.skillData);
        }
        else
        {
            Debug.LogWarning("�� �����ܿ� �Ҵ�� ��ų ������ �Ǵ� PlayerSkillController�� ���� ������ �� �����ϴ�.");
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