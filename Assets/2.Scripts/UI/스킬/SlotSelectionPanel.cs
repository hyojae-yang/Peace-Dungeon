using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// �� ��ũ��Ʈ�� ��ų ����� ���� 1~8�� ������ �����ϴ� UI�� �����մϴ�.
public class SlotSelectionPanel : MonoBehaviour
{
    [Header("���� ���� ��ư")]
    [Tooltip("1~8�� ���� ��ư���� ������� �Ҵ��ϼ���.")]
    public Button[] slotButtons;

    // PlayerSkillController�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // [Header("���� ��ũ��Ʈ")]
    // [Tooltip("��ų �����͸� �����ϴ� PlayerSkillController�� �Ҵ��ϼ���.")]
    // public PlayerSkillController playerSkillController;

    // --- ���� ���� ---
    private SkillData currentSkillData; // ���� ����Ϸ��� ��ų �����͸� �ӽ÷� ����
    private SkillIcon parentSkillIcon;  // �� �г��� Ȱ��ȭ��Ų SkillIcon ����

    private void Awake()
    {
        if (PlayerSkillController.Instance == null)
        {
            Debug.LogError("PlayerSkillController �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
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
        if (currentSkillData != null && parentSkillIcon != null)
        {
            // PlayerSkillController.Instance�� ��ų ��� �޼��带 ȣ���Ͽ� �����͸� �����մϴ�.
            PlayerSkillController.Instance.RegisterSkill(slotIndex, currentSkillData);
        }
        else
        {
            Debug.LogWarning("����� ��ų ������ �Ǵ� �θ� SkillIcon�� �����ϴ�. ��ų �������� �ٽ� ������ �ּ���.");
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