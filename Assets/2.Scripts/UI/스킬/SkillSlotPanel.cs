using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// �� ��ũ��Ʈ�� PlayerSkillController�� ��ų ���� ���� �̺�Ʈ�� �����Ͽ� UI�� ������Ʈ�ϴ� �߰��� ������ �մϴ�.
public class SkillSlotPanel : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    [Tooltip("���� ��ų ���� UI(�̹��� ��)�� ����ϴ� SkillSlotUI ������Ʈ �迭�Դϴ�.")]
    public SkillSlotUI[] skillUIs;

    [Header("���� ��ũ��Ʈ")]
    [Tooltip("��ų �����͸� �����ϴ� PlayerSkillController�� �Ҵ��ϼ���.")]
    public PlayerSkillController playerSkillController;

    private void Awake()
    {
        if (playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController�� �Ҵ���� �ʾҽ��ϴ�. �ν����Ϳ��� �Ҵ��� �ּ���.");
            return;
        }

        // PlayerSkillController�� ��ų ���� ���� �̺�Ʈ�� �����մϴ�.
        playerSkillController.OnSkillSlotChanged += UpdateSkillSlotUI;
    }

    /// <summary>
    /// PlayerSkillController�� OnSkillSlotChanged �̺�Ʈ�κ��� ȣ��Ǿ� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="slotIndex">������ �߻��� ������ �ε���</param>
    /// <param name="data">���Ӱ� ��ϵ� ��ų ������. ���� �ÿ��� null�Դϴ�.</param>
    private void UpdateSkillSlotUI(int slotIndex, SkillData data)
    {
        if (slotIndex >= 0 && slotIndex < skillUIs.Length)
        {
            skillUIs[slotIndex].UpdateUI(data);
        }
        else
        {
            Debug.LogError("�߸��� ���� �ε����Դϴ�: " + slotIndex);
        }
    }

    private void OnDestroy()
    {
        if (playerSkillController != null)
        {
            // �� ��ũ��Ʈ�� �ı��� �� �̺�Ʈ ������ �����Ͽ� �޸� ������ �����մϴ�.
            playerSkillController.OnSkillSlotChanged -= UpdateSkillSlotUI;
        }
    }
}