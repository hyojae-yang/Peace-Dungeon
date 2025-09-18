using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// �� ��ũ��Ʈ�� PlayerSkillController�� ��ų ���� ���� �̺�Ʈ�� �����Ͽ� UI�� ������Ʈ�ϴ� �߰��� ������ �մϴ�.
/// </summary>
public class SkillSlotPanel : MonoBehaviour
{
    // === UI ������Ʈ ===
    [Header("UI ������Ʈ")]
    [Tooltip("���� ��ų ���� UI(�̹��� ��)�� ����ϴ� SkillSlotUI ������Ʈ �迭�Դϴ�.")]
    public SkillSlotUI[] skillUIs;

    // �߾� ��� ������ �ϴ� PlayerCharacter �ν��Ͻ��� ���� �����Դϴ�.
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        // PlayerCharacter �ν��Ͻ��� ã�� ������ Ȯ���մϴ�.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // SkillPointManager�� �̱����̹Ƿ� ���� �����մϴ�.
        if (SkillPointManager.Instance == null)
        {
            Debug.LogError("SkillPointManager �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
            return;
        }

        // PlayerCharacter�� ���� PlayerSkillController�� �����Ͽ� �̺�Ʈ�� �����մϴ�.
        if (playerCharacter.playerSkillController == null)
        {
            Debug.LogError("PlayerSkillController�� PlayerCharacter�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        playerCharacter.playerSkillController.OnSkillSlotChanged += UpdateSkillSlotUI;
        playerCharacter.playerSkillController.OnCooldownUpdated += UpdateCooldownUI;
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
                // SkillPointManager�� �̱��� �ν��Ͻ��� ���� ��ų ������ �����ɴϴ�.
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
        if (playerCharacter != null && playerCharacter.playerSkillController != null)
        {
            // �� ��ũ��Ʈ�� �ı��� �� �̺�Ʈ ������ �����Ͽ� �޸� ������ �����մϴ�.
            playerCharacter.playerSkillController.OnSkillSlotChanged -= UpdateSkillSlotUI;
            playerCharacter.playerSkillController.OnCooldownUpdated -= UpdateCooldownUI;
        }
    }
}