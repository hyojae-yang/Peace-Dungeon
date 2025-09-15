using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// �� ��ũ��Ʈ�� PlayerSkillController�� ��ų ���� ���� �̺�Ʈ�� �����Ͽ� UI�� ������Ʈ�ϴ� �߰��� ������ �մϴ�.
public class SkillSlotPanel : MonoBehaviour
{
    // === UI ������Ʈ ===
    [Header("UI ������Ʈ")]
    [Tooltip("���� ��ų ���� UI(�̹��� ��)�� ����ϴ� SkillSlotUI ������Ʈ �迭�Դϴ�.")]
    public SkillSlotUI[] skillUIs;

    // PlayerSkillController�� SkillPointManager�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // [Header("���� ��ũ��Ʈ")]
    // [Tooltip("��ų �����͸� �����ϴ� PlayerSkillController�� �Ҵ��ϼ���.")]
    // public PlayerSkillController playerSkillController;
    // [Tooltip("��ų ����Ʈ �� ��ų ���� �����͸� �����ϴ� SkillPointManager�� �Ҵ��ϼ���.")]
    // public SkillPointManager skillPointManager;

    private void Awake()
    {
        if (PlayerSkillController.Instance == null)
        {
            Debug.LogError("PlayerSkillController �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            return;
        }
        if (SkillPointManager.Instance == null)
        {
            Debug.LogError("SkillPointManager �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // PlayerSkillController�� ��ų ���� ���� �̺�Ʈ�� ��Ÿ�� ������Ʈ �̺�Ʈ�� �����մϴ�.
        PlayerSkillController.Instance.OnSkillSlotChanged += UpdateSkillSlotUI;
        PlayerSkillController.Instance.OnCooldownUpdated += UpdateCooldownUI;
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
            float manaCost = 0f;
            if (data != null)
            {
                int currentLevel = SkillPointManager.Instance.GetTempSkillLevel(data.skillId);
                if (currentLevel > 0 && currentLevel <= data.levelInfo.Length)
                {
                    SkillLevelInfo levelInfo = data.levelInfo[currentLevel - 1];
                    foreach (var stat in levelInfo.stats)
                    {
                        if (stat.statType == StatType.ManaCost)
                        {
                            manaCost = stat.value;
                            break;
                        }
                    }
                }
            }
            skillUIs[slotIndex].UpdateUI(data, manaCost);
        }
        else
        {
            Debug.LogError("�߸��� ���� �ε����Դϴ�: " + slotIndex);
        }
    }

    /// <summary>
    /// PlayerSkillController�� OnCooldownUpdated �̺�Ʈ�κ��� ȣ��Ǿ� ��Ÿ�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="slotIndex">��Ÿ���� ���ŵ� ������ �ε���</param>
    /// <param name="remainingCooldown">���� ��Ÿ�� �ð� (��)</param>
    /// <param name="maxCooldown">��ų�� �ִ� ��Ÿ�� �ð� (��)</param>
    private void UpdateCooldownUI(int slotIndex, float remainingCooldown, float maxCooldown)
    {
        if (slotIndex >= 0 && slotIndex < skillUIs.Length)
        {
            skillUIs[slotIndex].UpdateCooldownUI(remainingCooldown, maxCooldown);
        }
        else
        {
            Debug.LogError("�߸��� ���� �ε����Դϴ�: " + slotIndex);
        }
    }

    private void OnDestroy()
    {
        if (PlayerSkillController.Instance != null)
        {
            // �� ��ũ��Ʈ�� �ı��� �� �̺�Ʈ ������ �����Ͽ� �޸� ������ �����մϴ�.
            PlayerSkillController.Instance.OnSkillSlotChanged -= UpdateSkillSlotUI;
            PlayerSkillController.Instance.OnCooldownUpdated -= UpdateCooldownUI;
        }
    }
}