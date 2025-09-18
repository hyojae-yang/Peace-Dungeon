using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// �� ��ũ��Ʈ�� ��ų ����� ���� 1~8�� ������ �����ϴ� UI�� �����մϴ�.
/// </summary>
public class SlotSelectionPanel : MonoBehaviour
{
    [Header("���� ���� ��ư")]
    [Tooltip("1~8�� ���� ��ư���� ������� �Ҵ��ϼ���.")]
    public Button[] slotButtons;

    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    // --- ���� ���� ---
    private SkillData currentSkillData; // ���� ����Ϸ��� ��ų �����͸� �ӽ÷� ����
    private SkillIcon parentSkillIcon;  // �� �г��� Ȱ��ȭ��Ų SkillIcon ����

    private void Awake()
    {
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

        // �� ���� ��ư�� Ŭ�� �̺�Ʈ �����ʸ� �߰��մϴ�.
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i; // Ŭ���� �̽� ������ ���� ���� ���� ���
            slotButtons[i].onClick.AddListener(() => OnSlotButtonClick(slotIndex));
        }

        // �ʱ⿡�� �� �г��� ��Ȱ��ȭ ���·� �Ӵϴ�.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ��� ��û�� ������ �� �� �г��� Ȱ��ȭ�ϰ� ��ų ������ �� �θ� SkillIcon�� �޽��ϴ�.
    /// </summary>
    /// <param name="skillIcon">�� �г��� Ȱ��ȭ��Ų SkillIcon ����</param>
    /// <param name="skillDataToRegister">����� ��ų ������</param>
    public void ShowPanel(SkillIcon skillIcon, SkillData skillDataToRegister)
    {
        this.parentSkillIcon = skillIcon; // SkillIcon ������ ����
        this.currentSkillData = skillDataToRegister;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ��ų ���� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="slotIndex">Ŭ���� ������ �ε���</param>
    private void OnSlotButtonClick(int slotIndex)
    {
        if (currentSkillData != null && parentSkillIcon != null && playerCharacter.playerSkillController != null)
        {
            // PlayerCharacter�� ���� PlayerSkillController�� ��ų ��� �޼��带 ȣ���Ͽ� �����͸� �����մϴ�.
            playerCharacter.playerSkillController.RegisterSkill(slotIndex, currentSkillData);
        }
        else
        {
            Debug.LogWarning("����� ��ų ������, �θ� SkillIcon, �Ǵ� PlayerSkillController�� �����ϴ�. ��ų �������� �ٽ� ������ �ּ���.");
        }

        // ��ų ����� �Ϸ�Ǹ� ��� ���� UI�� �ݰ� isPanelActive �÷��׸� �ʱ�ȭ�մϴ�.
        if (parentSkillIcon != null)
        {
            parentSkillIcon.HideAllRelatedPanels();
        }
        else
        {
            // parentSkillIcon�� null�� �������� ��츦 ����Ͽ� �гθ� �ݽ��ϴ�.
            HidePanel();
        }
    }

    /// <summary>
    /// �г��� ��Ȱ��ȭ�ϰ� �ӽ� �����͸� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false);
        // �г��� ���� �� �ӽ� ������ �ʱ�ȭ�Ͽ� �޸𸮸� �����մϴ�.
        currentSkillData = null;
        parentSkillIcon = null; // ������ �ʱ�ȭ
    }
}